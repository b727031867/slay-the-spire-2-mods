using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using Sts2Mod.Cards;
using Sts2Mod.Combo;
using Sts2Mod.Relics;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestBalanceChanges()
    {
        foreach (var upgraded in new[] { false, true })
        {
            await FreshExpansion();
            var enemies = _player.Creature.CombatState!.Enemies.ToArray();
            var hp = enemies.Sum(e => e.CurrentHp);
            var rng = _player.RunState.Rng.CombatTargets.Counter;
            await Play<ThunderChain>(upgraded);
            Check(hp - enemies.Sum(e => e.CurrentHp) == (upgraded ? 12 : 9)
                && _player.RunState.Rng.CombatTargets.Counter == rng + (upgraded ? 4 : 3)
                && Power.State.Last == Element.Lightning && Power.State.Used == 0,
                "BAL_chain_native_random_hits_" + upgraded);
            await FreshExpansion(); await SeedDraw();
            await Play<TailwindPrep>(upgraded);
            Check(PileType.Hand.GetPile(_player).Cards.Count == 1
                && PileType.Exhaust.GetPile(_player).Cards.OfType<TailwindPrep>().Any() == !upgraded,
                "BAL_tailwind_one_draw_exhaust_boundary_" + upgraded);
        }
        await FreshExpansion(); await SeedDraw();
        await Play<WindGuidance>(); await Play<WindBlade>(); await Play<RockGuard>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 1, "BAL_guidance_wind_rock_draws");
        await Play<LightningThrust>(); await Play<WindBlade>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 2, "BAL_guidance_pairs_share_once");

        await FreshExpansion(); await SeedDraw();
        await Play<ThunderEcho>(); await Play<WindBlade>(); await Play<RockGuard>();
        Check(_player.Creature.Block == 13, "BAL_echo_wind_rock_six_block");
        await RelicCmd.Obtain<PaimonCheer>(_player);
        await Play<WindBlade>();
        Check(_player.Creature.Block == 21, "BAL_echo_paimon_block_eight_no_stack");
        await FreshExpansion(); await SeedDraw();
        await Play<ThunderEcho>(); await Play<WindBlade>(); await Play<LightningThrust>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 2, "BAL_echo_doubles_lightning_wind");
        await RelicCmd.Obtain<PaimonCheer>(_player);
        await Play<WindBlade>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 4, "BAL_echo_paimon_draw_two_no_stack");
        var card = await Add<LightningThrust>();
        Check(ComboPresentation.VisualPreview((ElementCardModel)card).Bonus == 2,
            "BAL_echo_preview_matches_draw");

        await FreshExpansion(); await SeedDraw();
        await Play<ElementResonance>(true);
        await Play<WindBlade>();
        var energy = _player.PlayerCombatState!.Energy;
        await Play<RockGuard>();
        Check(_player.PlayerCombatState.Energy == energy, "BAL_upgraded_resonance_first_refunds_energy");
        await Play<WindBlade>();
        Check(_player.PlayerCombatState.Energy == energy - 1, "BAL_upgraded_resonance_once_per_turn");

        await FreshExpansion(); await SeedDraw();
        await RelicCmd.Obtain<JourneyCompass>(_player);
        await Play<WindBlade>(); await Play<RockGuard>(); await Play<WindBlade>(); await Play<RockGuard>();
        Check(Power.State.Used == 3 && _player.Creature.GetPower<StrengthPower>()?.Amount == 1,
            "BAL_compass_third_actual_reward_strength");
        await Play<WindBlade>();
        Check(Power.State.Used == 4 && Power.State.Remaining == 0
            && _player.Creature.GetPower<StrengthPower>()?.Amount == 1, "BAL_compass_fourth_no_second_strength");
        await Play<RockGuard>();
        Check(Power.State.Used == 4, "BAL_compass_fifth_combo_blocked");
        await NextTurn(preserveElement: true);
        await Play<WindBlade>(); await Play<RockGuard>(); await Play<WindBlade>();
        Check(_player.Creature.GetPower<StrengthPower>()?.Amount == 2, "BAL_compass_strength_accumulates_next_turn");
        await FreshExpansion();
        Check(_player.Creature.GetPower<StrengthPower>() is null && Power.State.TurnLimit == 3,
            "BAL_new_combat_strength_reset_without_compass");
    }
}
