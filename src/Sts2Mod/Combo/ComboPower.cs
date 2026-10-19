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
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2Mod.Combo;

public readonly record struct Reservation(ComboKind Kind, uint? ActionId);

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
        GD.Print($"STS2MOD_COMBO player={Owner.Player?.NetId} event={reason} last={State.Last} used={State.Used} remaining={State.Remaining}");
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
        power.Publish(step.Advanced ? "reserve" : "ignored");
        GD.Print($"STS2MOD_STEP player={card.Owner.NetId} action={action.Id} card={card.Id.Entry} auto={play.IsAutoPlay} index={play.PlayIndex} advanced={step.Advanced} before={step.Before} after={step.After} used={step.Used} kind={step.Kind} reward={(step.Triggered ? 4 : 0)}");
        return new(step.Kind, action.Id);
    }

    public static async Task Resolve(CardModel card, PlayerChoiceContext context, CardPlay play, Reservation reservation)
    {
        if (reservation.Kind == ComboKind.None) return;
        var creature = card.Owner.Creature;
        if (!CombatManager.Instance.IsInProgress || CombatManager.Instance.IsEnding || !creature.IsAlive)
        {
            GD.Print($"STS2MOD_REWARD player={card.Owner.NetId} action={reservation.ActionId} skipped=combat_ended_or_dead");
            return;
        }
        if (reservation.Kind == ComboKind.RockLightning)
        {
            var target = play.Target;
            if (target is null || !target.IsAlive || !creature.CombatState!.Creatures.Contains(target))
            {
                GD.Print($"STS2MOD_REWARD player={card.Owner.NetId} action={reservation.ActionId} skipped=target_dead_or_removed kind=RockLightning");
                return;
            }
            var hp = target.CurrentHp;
            await DamageCmd.Attack(4m).FromCard(card).Targeting(target)
                .WithHitFx("vfx/vfx_attack_slash").Execute(context);
            ShowTrigger(creature, "TC_COMBO.rockLightningTrigger");
            GD.Print($"STS2MOD_REWARD player={card.Owner.NetId} action={reservation.ActionId} kind=RockLightning hp_before={hp} hp_after={target.CurrentHp} base_bonus=4");
            return;
        }
        var before = creature.Block;
        creature.GetPower<ComboPower>()?.Flash();
        await CreatureCmd.GainBlock(creature, 4m, ValueProp.Unpowered, play);
        ShowTrigger(creature, "TC_COMBO.trigger");
        GD.Print($"STS2MOD_REWARD player={card.Owner.NetId} action={reservation.ActionId} block_before={before} block_after={creature.Block} bonus=4");
    }

    private static void ShowTrigger(Creature creature, string key)
    {
        if (creature.GetCreatureNode() is not null && creature.GetVfxContainer() is { } container
            && NThoughtBubbleVfx.Create(new LocString("powers", key).GetFormattedText(), creature, 1.2) is { } bubble)
            container.AddChild(bubble);
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
