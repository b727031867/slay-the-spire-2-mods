using BaseLib.Utils;
using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Rooms;
using Sts2Mod.Pools;

namespace Sts2Mod.Relics;

[CustomID("TC_R02"), Pool(typeof(TravelerRelicPool))]
public sealed class JourneyCompass : CustomRelicModel
{
    private bool _spent;
    private bool _inCombat;
    private int _turn;
    private int _readyOnTurn = 1;
    public bool SpentThisTurn => _spent;
    public bool CanTriggerThisTurn => IsMutable && _inCombat && !_spent && _turn >= _readyOnTurn && !IsMelted;
    public int TurnsUntilReady => _inCombat ? Math.Max(0, _readyOnTurn - _turn) : 0;
    public override bool ShowCounter => true;
    public override int DisplayAmount => CanTriggerThisTurn ? 1 : 0;
    public override LocString Title
    {
        get
        {
            if (!IsMutable || !_inCombat) return base.Title;
            var title = new LocString("relics", "TC_R02.stateTitle");
            title.Add("State", new LocString("relics", "TC_R02." + (CanTriggerThisTurn ? "ready" : _spent ? "spent" : "cooldown")));
            return title;
        }
    }
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool IsStackable => false;
    public override string PackedIconPath => "res://Sts2Mod/art/compass.png";
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;
    public override Task AfterObtained()
    {
        if (CombatManager.Instance.IsInProgress && Owner.PlayerCombatState is { } state)
        {
            _inCombat = true;
            _turn = Math.Max(1, state.TurnNumber);
            _readyOnTurn = _turn;
            _spent = false;
            RefreshIndicator();
        }
        return Task.CompletedTask;
    }
    public override Task BeforeCombatStart()
    {
        _inCombat = true;
        _turn = 0;
        _readyOnTurn = 1;
        _spent = false;
        RefreshIndicator();
        return Task.CompletedTask;
    }
    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    {
        if (player == Owner)
        {
            _turn++;
            _spent = false;
            RefreshIndicator();
        }
        return Task.CompletedTask;
    }
    public override Task AfterCombatEnd(CombatRoom room)
    {
        _inCombat = false;
        _turn = 0;
        _readyOnTurn = 1;
        _spent = false;
        RefreshIndicator();
        return Task.CompletedTask;
    }
    private void RefreshIndicator()
    {
        Status = !_inCombat ? RelicStatus.Normal : CanTriggerThisTurn ? RelicStatus.Active : RelicStatus.Disabled;
        InvokeDisplayAmountChanged();
    }
    public async Task OnComboReward(PlayerChoiceContext context)
    {
        if (!CanTriggerThisTurn || !Owner.Creature.IsAlive || !CombatManager.Instance.IsInProgress || CombatManager.Instance.IsEnding
            || Owner.Creature.CombatState?.Enemies.Any(enemy => enemy.IsAlive) != true) return;
        // Reserve before awaiting Draw: nested native callbacks cannot award twice.
        _spent = true;
        _readyOnTurn = _turn + 2;
        RefreshIndicator();
        Flash();
        var actual = (await CardPileCmd.Draw(context, 1m, Owner)).Count();
        GD.Print($"STS2MOD_COMPASS draw={actual} requested=1 spent=True turn={_turn} ready_on={_readyOnTurn}");
    }
}
