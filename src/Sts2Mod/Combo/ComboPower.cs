using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Relics;

namespace Sts2Mod.Combo;

public readonly record struct Reservation(ComboKind Kind, uint? ActionId, bool PaimonDoubles);

[CustomID("TC_COMBO")]
public sealed class ComboPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override string CustomPackedIconPath => "res://Sts2Mod/art/combo.svg";
    public override string CustomBigIconPath => CustomPackedIconPath;
    protected override object InitInternalData() => new ComboState();
    public ComboState State => GetInternalData<ComboState>();
    public override int DisplayAmount => IsMutable ? State.Remaining : ComboState.Limit;

    private LocString WithState(string key)
    {
        var loc = new LocString("powers", $"{Id.Entry}.{key}");
        loc.Add("Element", new LocString("powers", "TC_ELEMENT_" + (IsMutable ? State.Last : Element.None).ToString().ToUpperInvariant()));
        loc.Add("Remaining", DisplayAmount);
        var doubled = IsMutable && ComboPresentation.HasPaimon(Owner?.Player);
        loc.Add("BlockBonus", doubled ? 8 : 4);
        loc.Add("AttackBonus", doubled ? 8 : 4);
        loc.Add("DrawBonus", doubled ? 2 : 1);
        return loc;
    }
    public override LocString Title => WithState(DisplayAmount == 0 ? "exhaustedTitle" : "title");
    public override LocString Description => WithState(DisplayAmount == 0 ? "exhaustedDescription" : "description");

    public void BeginTurn() { State.BeginTurn(); Publish("turn_start"); }
    private void Clear(string reason) { State.EndTurn(); Publish(reason); }
    private void Publish(string reason)
    {
        SetAmount(State.Encoded, silent: true);
        InvokeDisplayAmountChanged();
        RefreshHand();
        GD.Print($"STS2MOD_COMBO player={Owner.Player?.NetId} event={reason} last={State.Last} used={State.Used} remaining={State.Remaining}");
    }

    private void RefreshHand()
    {
        if (DisplayServer.GetName() == "headless" || Owner?.Player is not { } player || !CombatManager.Instance.IsInProgress) return;
        foreach (var card in PileType.Hand.GetPile(player).Cards.OfType<ElementCardModel>())
        {
            var node = NCard.FindOnTable(card);
            if (node is not null && GodotObject.IsInstanceValid(node) && node.IsInsideTree())
                node.CallDeferred(NCard.MethodName.UpdateVisuals, (int)PileType.Hand, (int)CardPreviewMode.Normal);
        }
    }

    public static Reservation Reserve(CardModel card, PlayerChoiceContext context, CardPlay play, Element element)
    {
        var power = card.Owner.Creature.GetPower<ComboPower>();
        if (play.IsAutoPlay || !play.IsFirstInSeries)
        {
            GD.Print($"STS2MOD_STEP player={card.Owner.NetId} card={card.Id.Entry} auto={play.IsAutoPlay} index={play.PlayIndex} advanced=False reason=automatic_or_extra_resolution");
            return default;
        }
        // Only a validated, active native hand play may advance the sequence.
        if (power is null || context is not GameActionPlayerChoiceContext { Action: PlayCardAction action }
            || action.Player != card.Owner || action.NetCombatCard.ToCardModel() != card)
            return default;
        var step = power.State.Reserve(action, element, play.IsAutoPlay, play.IsFirstInSeries);
        var paimonDoubles = step.Triggered
            && card.Owner.Relics.Any(r => r is PaimonCheer { IsMelted: false });
        var reward = step.Kind switch
        {
            ComboKind.WindRock => paimonDoubles ? "block8" : "block4",
            ComboKind.RockLightning => paimonDoubles ? "attack8" : "attack4",
            ComboKind.LightningWind => paimonDoubles ? "draw2" : "draw1",
            _ => "none"
        };
        power.Publish(step.Advanced ? "reserve" : "ignored");
        GD.Print($"STS2MOD_STEP player={card.Owner.NetId} action={action.Id} card={card.Id.Entry} auto={play.IsAutoPlay} index={play.PlayIndex} advanced={step.Advanced} before={step.Before} after={step.After} used={step.Used} kind={step.Kind} reward={reward} paimonDouble={paimonDoubles}");
        return new(step.Kind, action.Id, paimonDoubles);
    }

    public static async Task Resolve(CardModel card, PlayerChoiceContext context, CardPlay play, Reservation reservation)
    {
        if (reservation.Kind == ComboKind.None) return;
        var creature = card.Owner.Creature;
        if (!CombatManager.Instance.IsInProgress || CombatManager.Instance.IsEnding || !creature.IsAlive
            || creature.CombatState?.Enemies.Any(enemy => enemy.IsAlive) != true)
        {
            GD.Print($"STS2MOD_REWARD player={card.Owner.NetId} action={reservation.ActionId} skipped=combat_ended_or_dead");
            return;
        }
        var bonus = reservation.PaimonDoubles ? 8m : 4m;
        var draws = reservation.PaimonDoubles ? 2m : 1m;
        var awarded = false;
        if (reservation.Kind == ComboKind.RockLightning)
        {
            var target = play.Target;
            if (target is null || !target.IsAlive || !creature.CombatState!.Creatures.Contains(target))
            {
                GD.Print($"STS2MOD_REWARD player={card.Owner.NetId} action={reservation.ActionId} skipped=target_dead_or_removed kind=RockLightning");
            }
            else
            {
                var hp = target.CurrentHp;
                await DamageCmd.Attack(bonus).FromCard(card).Targeting(target)
                    .WithHitFx("vfx/vfx_attack_slash").Execute(context);
                ShowReward(creature, target, reservation);
                awarded = true;
                GD.Print($"STS2MOD_REWARD player={card.Owner.NetId} action={reservation.ActionId} kind=RockLightning hp_before={hp} hp_after={target.CurrentHp} bonus={bonus}");
            }
        }
        else if (reservation.Kind == ComboKind.LightningWind)
        {
            var handBefore = PileType.Hand.GetPile(card.Owner).Cards.Count;
            var drawn = (await CardPileCmd.Draw(context, draws, card.Owner)).Count();
            ShowReward(creature, creature, reservation, drawn);
            awarded = drawn > 0;
            GD.Print($"STS2MOD_REWARD player={card.Owner.NetId} action={reservation.ActionId} kind=LightningWind hand_before={handBefore} hand_after={PileType.Hand.GetPile(card.Owner).Cards.Count} bonus_draw={draws}");
        }
        else
        {
            var before = creature.Block;
            creature.GetPower<ComboPower>()?.Flash();
            await CreatureCmd.GainBlock(creature, bonus, ValueProp.Unpowered, play);
            ShowReward(creature, creature, reservation);
            awarded = true;
            GD.Print($"STS2MOD_REWARD player={card.Owner.NetId} action={reservation.ActionId} block_before={before} block_after={creature.Block} bonus={bonus}");
        }
        if (awarded)
            foreach (var compass in card.Owner.Relics.OfType<JourneyCompass>().ToArray())
                await compass.OnComboReward(context);
    }

    private static void ShowReward(Creature creature, Creature effectTarget, Reservation reservation, int? actualDraw = null)
    {
        // One bubble per actual reward. Skipped target/combat rewards never claim a success.
        if (reservation.PaimonDoubles)
            creature.Player?.Relics.OfType<PaimonCheer>().FirstOrDefault(r => !r.IsMelted)?.Flash();
        var text = ComboPresentation.Trigger(reservation.Kind, reservation.PaimonDoubles, actualDraw).GetFormattedText();
        GD.Print($"STS2MOD_FEEDBACK kind={reservation.Kind} bonus={ComboPresentation.Reward(reservation.Kind, reservation.PaimonDoubles)} doubled={reservation.PaimonDoubles}");
        if (DisplayServer.GetName() == "headless") return;
        if (creature.GetCreatureNode() is not null && creature.GetVfxContainer() is { } container
            && NThoughtBubbleVfx.Create(text, creature, 1.0) is { } bubble)
            container.AddChild(bubble);
        ElementComboVfx.Show(effectTarget, reservation.Kind, reservation.PaimonDoubles);
    }

    public override Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner)) Clear("turn_end");
        return Task.CompletedTask;
    }
    public override Task AfterDeath(PlayerChoiceContext context, Creature creature, bool prevented, float animationLength)
    {
        if (creature == Owner && !prevented) Clear("death");
        return Task.CompletedTask;
    }
    public override Task AfterCombatEnd(CombatRoom room) { Clear("combat_end"); return Task.CompletedTask; }
}
