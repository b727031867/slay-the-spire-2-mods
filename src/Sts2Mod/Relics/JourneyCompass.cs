using BaseLib.Utils;
using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using Sts2Mod.Pools;

namespace Sts2Mod.Relics;

[CustomID("TC_R02"), Pool(typeof(TravelerRelicPool))]
public sealed class JourneyCompass : CustomRelicModel
{
    private bool _spent;
    public bool SpentThisTurn => _spent;
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool IsStackable => false;
    public override string PackedIconPath => "res://Sts2Mod/art/compass.png";
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;
    public override Task BeforeCombatStart()
    {
        _spent = false;
        return Task.CompletedTask;
    }
    public override Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    {
        if (player == Owner) _spent = false;
        return Task.CompletedTask;
    }
    public async Task OnComboReward(PlayerChoiceContext context)
    {
        if (IsMelted || SpentThisTurn || !Owner.Creature.IsAlive || !CombatManager.Instance.IsInProgress || CombatManager.Instance.IsEnding
            || Owner.Creature.CombatState?.Enemies.Any(enemy => enemy.IsAlive) != true) return;
        // Reserve before awaiting Draw: nested native callbacks cannot award twice.
        _spent = true;
        Flash();
        var actual = (await CardPileCmd.Draw(context, 1m, Owner)).Count();
        GD.Print($"STS2MOD_COMPASS draw={actual} requested=1 spent=True");
    }
}
