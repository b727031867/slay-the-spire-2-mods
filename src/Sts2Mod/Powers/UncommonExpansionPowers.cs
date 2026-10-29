using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using Sts2Mod.Combo;

namespace Sts2Mod.Powers;

[CustomID("TC_LAYERED_ROCK_ECHO")]
public sealed class LayeredRockEchoPower : TravelerAbilityPower
{
    private int _spentAmount;
    private int _temporaryDexterity;
    public override string CustomPackedIconPath => "res://Sts2Mod/art/TC_C32.png";
    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    {
        if (player == Owner.Player) _spentAmount = 0;
        return Task.CompletedTask;
    }
    public override async Task OnComboReward(PlayerChoiceContext context, ComboKind kind, int actualRewardNumber)
    {
        if (!CanReward || kind != ComboKind.WindRock || Amount <= _spentAmount) return;
        var amount = Amount - _spentAmount;
        _spentAmount = Amount;
        Flash();
        var before = Owner.GetPower<DexterityPower>()?.Amount ?? 0;
        await PowerCmd.Apply<DexterityPower>(context, Owner, amount, Owner, null);
        _temporaryDexterity += Math.Max(0, (Owner.GetPower<DexterityPower>()?.Amount ?? 0) - before);
    }
    public override async Task AfterSideTurnEnd(PlayerChoiceContext context, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || _temporaryDexterity == 0) return;
        var amount = _temporaryDexterity;
        _temporaryDexterity = 0;
        await PowerCmd.Apply<DexterityPower>(context, Owner, -amount, Owner, null);
    }
}

[CustomID("TC_TOXIC_SATCHEL")]
public sealed class ToxicSatchelPower : TravelerAbilityPower
{
    private int _spentAmount;
    public override string CustomPackedIconPath => "res://Sts2Mod/art/TC_C37.png";
    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    {
        if (player == Owner.Player) _spentAmount = 0;
        return Task.CompletedTask;
    }
    public override async Task OnComboReward(PlayerChoiceContext context, ComboKind kind, int actualRewardNumber)
    {
        if (!CanReward || kind != ComboKind.LightningWind || Amount <= _spentAmount) return;
        var amount = Amount - _spentAmount;
        _spentAmount = Amount;
        Flash();
        foreach (var enemy in Owner.CombatState!.Enemies.Where(e => e.IsAlive).ToArray())
            await PowerCmd.Apply<PoisonPower>(context, enemy, amount, Owner, null);
    }
}
