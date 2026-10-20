using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;

namespace Sts2Mod.Debug;

/// <summary>Read-only observer registered only by the explicit reward acquisition test.</summary>
public sealed class RewardProbe : SingletonModel
{
    public override bool ShouldReceiveCombatHooks => false;
    internal static CardReward? Latest { get; set; }
    public override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
    {
        if (room is CombatRoom)
        {
            Latest = rewards.OfType<CardReward>().FirstOrDefault();
            GD.Print($"STS2MOD_NATIVE_REWARD player={player.NetId} cards={string.Join(',', Latest?.Cards.Select(c => c.Id.Entry) ?? [])} modified=false");
        }
        return false;
    }
}
