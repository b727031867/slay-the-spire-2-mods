using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using Godot;
using HarmonyLib;
using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Sts2Mod.Combo;
using Sts2Mod.Pools;

namespace Sts2Mod.Cards;

[CustomID("TC_C24"), Pool(typeof(TravelerCardPool))]
public sealed class ElementTuning() : ElementCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override Element CardElement => Element.None;
    public override string CustomPortraitPath => "res://Sts2Mod/art/TC_C24.png";
    protected override void OnUpgrade() { }
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        description.Add("Candidates", CurrentUpgradeLevel > 0
            ? new LocString("cards", "TC_C24.upgradedCandidates").GetFormattedText() : "");
    }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        await ComboPower.Resolve(this, context, play, reservation);
        if (CombatManager.Instance.IsOverOrEnding) return;
        var pool = ModelDb.AllCards.OfType<ElementCardModel>().Where(c => TravelerContent.RewardCardTypes.Contains(c.GetType())).ToArray();
        var candidates = new[] { Element.Wind, Element.Rock, Element.Lightning }.Select(element =>
        {
            var canonical = Owner.RunState.Rng.CombatCardGeneration.NextItem(pool.Where(c => c.CardElement == element).ToArray())!;
            var candidate = CombatState!.CreateCard(canonical, Owner);
            if (CurrentUpgradeLevel > 0) CardCmd.Upgrade(candidate);
            return candidate;
        }).ToArray();
        CardModel? selected;
        Creature? target;
        var hand = LocalContext.IsMe(Owner) ? NPlayerHand.Instance : null;
        var previousHand = ElementTuningHandInputPatch.ChoosingHand;
        if (hand is not null) ElementTuningHandInputPatch.ChoosingHand = hand;
        try
        {
            hand?.CancelAllCardPlay();
            selected = await CardSelectCmd.FromChooseACardScreen(context, candidates, Owner, canSkip: false);
            foreach (var candidate in candidates.Where(c => c != selected)) candidate.RemoveFromState();
            if (selected is null)
            {
                if (CombatManager.Instance.IsOverOrEnding) return;
                throw new InvalidOperationException("Element Tuning requires one card.");
            }
            target = selected.TargetType == TargetType.AnyEnemy ? await SelectEnemy(context, selected) : null;
        }
        finally
        {
            if (hand is not null) ElementTuningHandInputPatch.ChoosingHand = previousHand;
        }
        if (CombatManager.Instance.IsOverOrEnding) { selected.RemoveFromState(); return; }
        selected.ExhaustOnNextPlay = true;
        await CardPileCmd.AddGeneratedCardToCombat(selected, PileType.Play, Owner);
        CombatCardElements.Chosen.Set(selected, true);
        try { await CardCmd.AutoPlay(context, selected, target); }
        finally { CombatCardElements.Chosen.Set(selected, false); }
    }

    private async Task<Creature?> SelectEnemy(PlayerChoiceContext context, CardModel selected)
    {
        var targets = CombatState!.HittableEnemies.Where(c => c.IsAlive).ToArray();
        if (targets.Length == 0) return null;
        if (CardSelectCmd.Selector is not null) return targets[0];
        var synchronizer = RunManager.Instance.PlayerChoiceSynchronizer;
        var choiceId = synchronizer.ReserveChoiceId(Owner);
        context.PushModel(selected);
        await context.SignalPlayerChoiceBegun(PlayerChoiceOptions.None);
        try
        {
            int index;
            if (LocalContext.IsMe(Owner) && RunManager.Instance.NetService.Type != NetGameType.Replay)
            {
                Node? node;
                do
                {
                    var controller = NControllerManager.Instance?.IsUsingController == true;
                    NTargetManager.Instance.StartTargeting(TargetType.AnyEnemy,
                        NCard.FindOnTable(this)?.GlobalPosition ?? Vector2.Zero,
                        controller ? TargetMode.Controller : TargetMode.ClickMouseToTarget,
                        () => CombatManager.Instance.IsOverOrEnding,
                        n => n is NCreature creature && targets.Contains(creature.Entity));
                    if (controller)
                    {
                        var nodes = NCombatRoom.Instance!.CreatureNodes.Where(n => targets.Contains(n.Entity)).ToArray();
                        NCombatRoom.Instance.RestrictControllerNavigation(nodes.Select(n => n.Hitbox));
                        nodes.First().Hitbox.TryGrabFocus();
                    }
                    node = await NTargetManager.Instance.SelectionFinished();
                } while (node is null && !CombatManager.Instance.IsOverOrEnding);
                index = node is NCreature creature ? Array.IndexOf(targets, creature.Entity) : -1;
                synchronizer.SyncLocalChoice(Owner, choiceId, PlayerChoiceResult.FromIndex(index));
            }
            else index = (await synchronizer.WaitForRemoteChoice(Owner, choiceId)).AsIndex();
            return index >= 0 && index < targets.Length ? targets[index] : null;
        }
        finally
        {
            if (LocalContext.IsMe(Owner) && RunManager.Instance.NetService.Type != NetGameType.Replay)
                NCombatRoom.Instance?.EnableControllerNavigation();
            await context.SignalPlayerChoiceEnded();
            context.PopModel(selected);
        }
    }
}

[HarmonyPatch]
internal static class ElementTuningHandInputPatch
{
    internal static NPlayerHand? ChoosingHand;
    private static IEnumerable<MethodBase> TargetMethods()
        => new[] { AccessTools.Method(typeof(NPlayerHand), "OnHolderPressed"),
            AccessTools.Method(typeof(NPlayerHand), nameof(NPlayerHand._UnhandledInput)) };

    // Shortcuts cancel native targeting before checking whether another card can be played.
    private static bool Prefix(NPlayerHand __instance) => __instance != ChoosingHand;
}
