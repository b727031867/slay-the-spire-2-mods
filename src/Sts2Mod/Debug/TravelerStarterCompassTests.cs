using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using Sts2Mod.Cards;
using Sts2Mod.Relics;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestStarterCompassBalance()
    {
        await StartFreshCombat(); await SeedDraw();
        var compass = _player.GetRelic<JourneyCompass>()!;
        Check(Power.State.TurnLimit == 4, "CHAR_compass_four_combo_limit");
        await Play<WindBlade>(); await Play<RockGuard>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 0 && !compass.SpentThisTurn,
            "CHAR_compass_no_draw_after_first_reward");
        await Play<WindBlade>(); await Play<RockGuard>();
        Check(_player.Creature.GetPower<StrengthPower>()?.Amount == 1 && compass.SpentThisTurn,
            "CHAR_compass_third_reward_one_strength");
        await Play<WindBlade>(); await Play<RockGuard>();
        Check(Power.State.Used == 4 && _player.Creature.GetPower<StrengthPower>()?.Amount == 1,
            "CHAR_compass_fourth_allowed_fifth_capped");
        await NextTurn(preserveElement: true);
        await Play<WindBlade>(); await Play<RockGuard>(); await Play<WindBlade>();
        Check(_player.Creature.GetPower<StrengthPower>()?.Amount == 2,
            "CHAR_compass_strength_each_turn_no_cooldown");
        await StartFreshCombat(); await SeedDraw();
        Check(_player.Creature.GetPower<StrengthPower>() is null && !compass.SpentThisTurn,
            "CHAR_compass_new_combat_clears_strength");
        var source = await Play<WindBlade>();
        await PowerCmd.Apply<NoDrawPower>(source.PlayerChoiceContext!, _player.Creature, 1m, _player.Creature, null);
        await Play<LightningThrust>(); await Play<RockGuard>(); await Play<WindBlade>();
        Check(Power.State.Used == 3 && Power.State.RewardsThisTurn == 2 && !compass.SpentThisTurn,
            "CHAR_compass_skipped_reward_not_strength_progress");
        await Play<RockGuard>();
        Check(compass.SpentThisTurn && _player.Creature.GetPower<StrengthPower>()?.Amount == 1
            && PileType.Hand.GetPile(_player).Cards.Count == 0, "CHAR_compass_third_actual_after_skip_strength_no_draw");
        await RelicCmd.Remove(compass);
    }
}
