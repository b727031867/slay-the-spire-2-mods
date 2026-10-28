using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestPotionTradeCompatibility()
    {
        foreach (var (rarity, type) in new[] { (CardRarity.Rare, CardType.Attack),
            (CardRarity.Rare, CardType.Skill), (CardRarity.Uncommon, CardType.Power) })
        {
            var options = CardCreationOptions.ForNonCombatWithUniformOdds([_player.Character.CardPool],
                card => card.Rarity == rarity && card.Type == type)
                .WithFlags(CardCreationFlags.NoRarityModification | CardCreationFlags.NoCardPoolModifications);
            var rejected = false;
            try { _ = CardFactory.CreateForReward(_player, 3, options).ToArray(); }
            catch (InvalidOperationException ex) { rejected = ex.Message.Contains("couldn't generate a valid card"); }
            Check(rejected, $"EVENT_potion_trade_native_shortage_{rarity}_{type}");
        }
        if (CombatManager.Instance.IsInProgress)
        {
            await CreatureCmd.Kill(_player.Creature.CombatState!.Enemies.ToArray(), force: true);
            await CombatManager.Instance.CheckWinCondition();
            await Until(() => !CombatManager.Instance.IsInProgress, "potion trade fixture combat end");
        }
        for (var index = 0; index < 3; index++)
        {
            foreach (var potion in _player.Potions.ToArray()) await PotionCmd.Discard(potion);
            Check((await PotionCmd.TryToProcure<FirePotion>(_player)).success
                && (await PotionCmd.TryToProcure<Ashwater>(_player)).success
                && (await PotionCmd.TryToProcure<FruitJuice>(_player)).success, $"EVENT_potion_trade_inventory_{index}");
            await RunManager.Instance.EnterRoomDebug(RoomType.Event, model: ModelDb.Event<TheFutureOfPotions>());
            var model = (TheFutureOfPotions)((EventRoom)_player.RunState.CurrentRoom!).LocalMutableEvent;
            var property = AccessTools.Property(typeof(TheFutureOfPotions), "PotionToCardType");
            var types = (Dictionary<PotionModel, CardType>)property.GetValue(model)!;
            Check(ReferenceEquals(types, property.GetValue(model)) && types.Count == 3
                && types.Where(pair => pair.Key is FruitJuice).All(pair => pair.Value == CardType.Power)
                && types.Where(pair => pair.Key is Ashwater).All(pair => pair.Value != CardType.Power),
                $"EVENT_potion_trade_valid_stable_types_{index}");
            var traded = _player.Potions.ElementAt(index);
            var rarity = index switch { 0 => CardRarity.Common, 1 => CardRarity.Uncommon, _ => CardRarity.Rare };
            var option = model.CurrentOptions[index];
            var trade = option.Chosen();
            NRewardsScreen? screen = null;
            await Until(() => trade.IsCompleted || (screen = GetTree().Root.FindChildren("*", recursive: true, owned: false)
                .OfType<NRewardsScreen>().LastOrDefault(node => node.IsVisibleInTree())) is not null, "native potion trade reward screen");
            if (trade.IsCompleted) await trade;
            var set = (RewardsSet)AccessTools.Field(typeof(NRewardsScreen), "_rewardsSet").GetValue(screen!)!;
            var reward = set.Rewards.OfType<CardReward>().Single();
            Check(reward.Cards.Count() == 3 && reward.Cards.Select(card => card.Id).Distinct().Count() == 3
                && reward.Cards.All(card => card.Rarity == rarity && card.Type == types[traded] && card.CurrentUpgradeLevel == 1)
                && !_player.Potions.Contains(traded), $"EVENT_potion_trade_native_rewards_{index}");
            RunManager.Instance.RewardsSetSynchronizer.SkipLocalRewardsSet();
            NOverlayStack.Instance!.Remove(screen!);
            await trade;
            await option.Chosen();
            Check(model.IsFinished && _player.CanRemovePotions && _player.Potions.Count() == 2,
                $"EVENT_potion_trade_finishes_{index}");
        }
        foreach (var potion in _player.Potions.ToArray()) await PotionCmd.Discard(potion);
        await StartFreshCombat();
    }
}
