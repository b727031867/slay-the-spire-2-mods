using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Sts2Mod.Pools;

namespace Sts2Mod.Relics;

/// <summary>Orobas starter upgrade: one draw per awarded combo, without a turn budget.</summary>
[CustomID("TC_R03"), Pool(typeof(TravelerRelicPool))]
public sealed class UnboundJourneyCompass : CustomRelicModel, IComboRewardRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override bool IsStackable => false;
    public override string PackedIconPath => "res://Sts2Mod/art/compass.png";
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;

    public async Task OnComboReward(PlayerChoiceContext context)
    {
        if (IsMelted || !Owner.Creature.IsAlive || !CombatManager.Instance.IsInProgress || CombatManager.Instance.IsEnding
            || Owner.Creature.CombatState?.Enemies.Any(enemy => enemy.IsAlive) != true) return;
        Flash();
        var actual = (await CardPileCmd.Draw(context, 1m, Owner)).Count();
        GD.Print($"STS2MOD_UNBOUND_COMPASS draw={actual} requested=1 cooldown=none");
    }
}
