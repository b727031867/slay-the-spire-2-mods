using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Characters;
using Sts2Mod.Combo;

namespace Sts2Mod.Presentation;

/// <summary>Read-only native event adapter; no waits, targets, damage or random choices are introduced.</summary>
public static class CombatPresentation
{
    private static readonly AsyncLocal<ComboKind> RewardKind = new();
    private static readonly AsyncLocal<bool> RewardDoubled = new();
    public static IDisposable Reward(ComboKind kind, bool doubled) => new RewardContext(kind, doubled);
    private sealed class RewardContext : IDisposable
    {
        private readonly ComboKind _previous = RewardKind.Value;
        private readonly bool _previousDoubled = RewardDoubled.Value;
        public RewardContext(ComboKind kind, bool doubled) { RewardKind.Value = kind; RewardDoubled.Value = doubled; }
        public void Dispose() { RewardKind.Value = _previous; RewardDoubled.Value = _previousDoubled; }
    }
    public static bool ShowFusion(Creature receiver, ComboKind kind, bool blocked = false)
    {
        if (RewardKind.Value != kind) return false;
        ElementComboVfx.Show(receiver, kind, RewardDoubled.Value, blocked);
        return true;
    }
    public static float Speed => SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast ? 1.6f : 1;
    public static Element Style(CardModel? card) => card is null ? Element.None : CombatCardElements.Get(card);
    public static TravelerRig? Rig(Creature? creature)
    {
        if (creature?.Player?.Character is not Traveler) return null;
        return creature.GetCreatureNode()?.Visuals.GetNodeOrNull<TravelerRig>("Visuals");
    }

    public static string Reaction(int blocked, int lostHp, bool attack) => lostHp > 0
        ? attack ? blocked > 0 ? "PartialHit" : "Hit" : "LifeLoss"
        : attack && blocked > 0 ? "BlockHit" : "None";

    public static void Damage(Creature receiver, Creature? dealer, DamageResult result, CardModel? card)
    {
        var attack = dealer is not null && dealer != receiver && result.Props.HasFlag(ValueProp.Move);
        var rig = Rig(receiver);
        var reaction = Reaction(result.BlockedDamage, result.UnblockedDamage, attack);
        if (rig is not null && reaction != "None" && !result.Props.HasFlag(ValueProp.SkipHurtAnim))
        {
            rig.LastReaction = reaction;
            rig.ReactionEvents++;
            rig.PlayAction(receiver.IsDead ? "Dead" : reaction);
            CombatElementVfx.Show(receiver, Element.None, result.UnblockedDamage > 0 ? CombatEffect.Hurt : CombatEffect.Contact);
            if (result.BlockedDamage > 0 && result.UnblockedDamage > 0)
                CombatElementVfx.Show(receiver, Element.None, CombatEffect.Contact);
        }
        if (attack && Rig(dealer) is { } attacker)
        {
            attacker.LastImpactElement = Style(card);
            attacker.ImpactEvents++;
            attacker.Contact();
            if (result.TotalDamage > 0 && !ShowFusion(receiver, ComboKind.RockLightning, result.UnblockedDamage <= 0))
                CombatElementVfx.Show(receiver, Style(card), result.UnblockedDamage > 0 ? CombatEffect.Attack : CombatEffect.Contact);
        }
    }
}

public sealed class CombatPresentationLifecycle : SingletonModel
{
    public override bool ShouldReceiveCombatHooks => true;
    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (CombatPresentation.Rig(cardPlay.Card.Owner.Creature) is { } rig)
        {
            rig.Style = CombatPresentation.Style(cardPlay.Card);
            rig.CardActive = true;
            rig.CardAttacked = false;
            if (cardPlay.Card.Type != CardType.Attack && !cardPlay.Card.GainsBlock) rig.PlayAction("Cast");
        }
        return Task.CompletedTask;
    }
    public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (CombatPresentation.Rig(cardPlay.Card.Owner.Creature) is { } rig) rig.CardActive = false;
        return Task.CompletedTask;
    }
}

// Native history receives each authoritative result before hurt/death animation dispatch.
// This also includes fully blocked attacks, which never receive the native Hit trigger.
[HarmonyPatch(typeof(CombatHistory), nameof(CombatHistory.DamageReceived))]
public static class CombatDamagePresentationPatch
{
    [HarmonyPostfix]
    public static void Postfix(Creature receiver, Creature? dealer, DamageResult result, CardModel? cardSource)
        => CombatPresentation.Damage(receiver, dealer, result, cardSource);
}

[HarmonyPatch(typeof(CombatHistory), nameof(CombatHistory.BlockGained))]
public static class CombatBlockPresentationPatch
{
    [HarmonyPostfix]
    public static void Postfix(Creature receiver, int amount, CardPlay? cardPlay)
    {
        if (amount <= 0 || CombatPresentation.Rig(receiver) is not { } rig) return;
        rig.LastDefenseElement = CombatPresentation.Style(cardPlay?.Card);
        rig.DefenseEvents++;
        rig.Style = rig.LastDefenseElement;
        rig.PlayAction("Defend");
        if (!CombatPresentation.ShowFusion(receiver, ComboKind.WindRock))
            CombatElementVfx.Show(receiver, CombatPresentation.Style(cardPlay?.Card), CombatEffect.Defend);
    }
}

// The result adapter has already selected Hit / PartialHit / BlockHit / LifeLoss.
// Keep BaseLib's AnimationPlayer forwarding for all other triggers, including death and revive.
[HarmonyPatch(typeof(NCreature), nameof(NCreature.SetAnimationTrigger))]
public static class CombatHurtPresentationPatch
{
    [HarmonyPrefix, HarmonyPriority(Priority.First)]
    public static bool Prefix(NCreature __instance, string trigger)
        => trigger != "Hit" || CombatPresentation.Rig(__instance.Entity) is null;
}
