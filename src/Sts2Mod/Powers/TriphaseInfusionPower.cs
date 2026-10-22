using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Sts2Mod.Combo;
using Sts2Mod.Relics;

namespace Sts2Mod.Powers;

[CustomID("TC_TRIPHASE_INFUSION")]
public sealed class TriphaseInfusionPower : TravelerAbilityPower
{
    private CardModel? _source;
    private int _temporaryDexterity;
    public override string CustomPackedIconPath => "res://Sts2Mod/art/thunder_echo.png";
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext context, Player player)
    {
        if (player == Owner.Player && _source is not null) await Trigger(context, _source, null);
    }
    public async Task Trigger(PlayerChoiceContext context, CardModel source, CardPlay? play)
    {
        _source = source;
        if (!CanReward) return;
        var combo = Owner.GetPower<ComboPower>();
        if (combo is null) return;
        var next = Owner.Player!.RunState.Rng.CombatCardGeneration.NextItem(new[] { Element.Wind, Element.Rock, Element.Lightning });
        var before = combo.State.Last;
        combo.RefreshLimit();
        var step = combo.State.Reserve(new object(), next);
        combo.Publish("infusion");
        if (before == next)
        {
            var dexterityBefore = Owner.GetPower<DexterityPower>()?.Amount ?? 0;
            await PowerCmd.Apply<DexterityPower>(context, Owner, Amount, Owner, source);
            _temporaryDexterity += Math.Max(0, (Owner.GetPower<DexterityPower>()?.Amount ?? 0) - dexterityBefore);
        }
        if (!step.Triggered) return;
        foreach (var blessing in Owner.Player.Relics.OfType<StatueBlessing>()) await blessing.OnComboTriggered(context);
        var reservation = new Reservation(step.Kind, null, ComboPresentation.HasPaimon(Owner.Player), Effect: true);
        var effectPlay = play ?? new CardPlay { Card = source, Target = null, ResultPile = PileType.None,
            Resources = new ResourceInfo { EnergySpent = 0, EnergyValue = 0, StarsSpent = 0, StarValue = 0 }, IsAutoPlay = true, PlayIndex = 0, PlayCount = 1 };
        await ComboPower.Resolve(source, context, effectPlay, reservation);
    }
    public override async Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || _temporaryDexterity == 0) return;
        var amount = _temporaryDexterity;
        _temporaryDexterity = 0;
        await PowerCmd.Apply<DexterityPower>(context, Owner, -amount, Owner, null);
    }
}
