using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Sts2Mod.Cards;
using Sts2Mod.Relics;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestLastingCandyRewards()
    {
        var power = ModelDb.Card<TriphaseInfusion>();
        var rejected = false;
        try { _ = new CardCreationOptions([power], CardCreationSource.Other, CardRarityOddsType.BossEncounter); }
        catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "CHAR_candy_native_single_rarity_reproduces_bug");
        var single = LastingCandyRewardPatch.CreateOptions([power], CardCreationSource.Other, CardRarityOddsType.BossEncounter);
        Check(single.RarityOdds == CardRarityOddsType.Uniform && single.GetPossibleCards(_player).Single() == power,
            "CHAR_candy_single_rarity_uses_uniform");
        var mixed = LastingCandyRewardPatch.CreateOptions([power, ModelDb.Card<WindBlade>()],
            CardCreationSource.Other, CardRarityOddsType.BossEncounter);
        Check(mixed.RarityOdds == CardRarityOddsType.BossEncounter && mixed.GetPossibleCards(_player).Count() == 2,
            "CHAR_candy_mixed_rarity_preserves_odds");
        Check(!LastingCandyRewardPatch.CreateOptions([], CardCreationSource.Other, CardRarityOddsType.BossEncounter)
            .GetPossibleCards(_player).Any(), "CHAR_candy_empty_pool_preserved");

        var candy = await RelicCmd.Obtain<LastingCandy>(_player);
        try
        {
            candy.CombatsSeen = 2;
            var bossOptions = CardCreationOptions.ForRoom(_player, RoomType.Boss);
            var offered = bossOptions.GetPossibleCards(_player)
                .Where(card => card.Type == CardType.Power && card.Rarity == CardRarity.Rare)
                .Select(card => new CardCreationResult(card.ToMutable())).ToList();
            var before = offered.Count;
            Check(candy.TryModifyCardRewardOptions(_player, offered, bossOptions)
                && offered.Count == before + 1 && offered.Last().Card.Type == CardType.Power
                && offered.Last().Card.Rarity == CardRarity.Uncommon,
                "CHAR_candy_native_hook_single_rarity_candidates");
            foreach (var combats in new[] { 0, 1, 2, 4 })
            foreach (var room in new[] { RoomType.Monster, RoomType.Elite, RoomType.Boss })
            {
                candy.CombatsSeen = combats;
                var reward = new CardReward(CardCreationOptions.ForRoom(_player, room), 3, _player);
                reward.Populate();
                var active = combats > 0 && combats % 2 == 0;
                Check(reward.Cards.Count() == (active ? 4 : 3)
                    && reward.Cards.All(IsTheme)
                    && (!active || reward.Cards.Last().Type == CardType.Power)
                    && (room != RoomType.Boss || reward.Cards.Take(3).All(card => card.Rarity == CardRarity.Rare)),
                    $"CHAR_candy_reward_{room}_{combats}");
                var restored = (CardReward)Reward.FromSerializable(reward.ToSerializable(), _player);
                restored.Populate();
                restored.Populate();
                Check(restored.Cards.Count() == reward.Cards.Count()
                    && restored.Cards.All(IsTheme)
                    && (!active || restored.Cards.Last().Type == CardType.Power)
                    && restored.ToSerializable().RarityOdds == CardCreationOptions.ForRoom(_player, room).RarityOdds,
                    $"CHAR_candy_reward_roundtrip_{room}_{combats}");
            }
        }
        finally { await RelicCmd.Remove(candy); }
    }
}
