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
using Sts2Mod.Powers;
using Sts2Mod.Cards;

namespace Sts2Mod.Combo;

public readonly record struct Reservation(ComboKind Kind, uint? ActionId, bool PaimonDoubles);

[CustomID("TC_COMBO")]
public sealed class ComboPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override string CustomPackedIconPath => ComboIcons.ElementPath(IsMutable ? State.Last : Element.None);
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
    private void Clear(string reason) { State.ClearCombat(); Publish(reason); }
    internal void FlashReward() => Flash();

    internal void Publish(string reason)
    {
        SetAmount(State.Encoded, silent: true);
        InvokeDisplayAmountChanged();
        ComboFeedback.RefreshHand(Owner);
        GD.Print($"STS2MOD_COMBO player={Owner.Player?.NetId} event={reason} last={State.Last} used={State.Used} remaining={State.Remaining}");
    }

    // Stable facade for existing card models; engine orchestration lives in ComboResolver.
    public static Reservation Reserve(CardModel card, PlayerChoiceContext context, CardPlay play, Element element)
        => ComboResolver.Reserve(card, context, play, element);

    public static Task Resolve(CardModel card, PlayerChoiceContext context, CardPlay play, Reservation reservation)
        => ComboResolver.Resolve(card, context, play, reservation);

    public override Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        return Task.CompletedTask;
    }
    public override Task AfterDeath(PlayerChoiceContext context, Creature creature, bool prevented, float animationLength)
    {
        return Task.CompletedTask;
    }
    public override Task AfterCombatEnd(CombatRoom room) { Clear("combat_end"); return Task.CompletedTask; }
}
