using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Powers;
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
            var hp = enemies.Select(e => e.CurrentHp).ToArray();
            var rng = _player.RunState.Rng.CombatTargets.Counter;
            await Play<ThunderChain>(upgraded);
            Check(enemies.Select((enemy, i) => hp[i] - enemy.CurrentHp).All(damage => damage == (upgraded ? 15 : 10))
                && _player.RunState.Rng.CombatTargets.Counter == rng
                && Power.State.Last == Element.Lightning && Power.State.Used == 0,
                "BAL_chain_native_all_enemy_hits_" + upgraded);
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
        Check(_player.PlayerCombatState.Energy == energy + 1, "BAL_upgraded_resonance_first_refunds_energy");
        await Play<WindBlade>();
        Check(_player.PlayerCombatState.Energy == energy, "BAL_upgraded_resonance_once_per_turn");

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

    private async Task TestBalance0315()
    {
        var start = _assertions;
        foreach (var up in new[] { false, true })
        {
            await FreshExpansion(); await SeedDraw();
            var finale = await Add<TriphaseFinale>(up);
            finale.GiveSingleTurnRetain();
            await Play<WindBlade>(); await Play<RockGuard>(); await Play<WindBlade>();
            await ExpansionNativeTurn();
            Check(Power.State.CombosThisCombat == 2 && Power.State.Used == 0 && finale.Pile?.Type == PileType.Hand,
                "BAL15_finale_combat_count_survives_turn_" + up);
            await PlayerCmd.SetEnergy(10, _player);
            await Play<RockGuard>(); await Play<WindBlade>();
            var expected = (up ? 12 : 10) + 8;
            var hp = _target.CurrentHp;
            finale.DynamicVars.CalculatedDamage.UpdateCardPreview(finale, CardPreviewMode.Normal, _target, true);
            var preview = finale.DynamicVars.CalculatedDamage.PreviewValue;
            await Queue(new MegaCrit.Sts2.Core.GameActions.PlayCardAction(finale, _target));
            GD.Print($"STS2MOD_BAL15_FINALE upgraded={up} damage={hp - _target.CurrentHp} preview={preview} expected={expected} combos={Power.State.CombosThisCombat} cost={finale.EnergyCost.GetWithModifiers(CostModifiers.Local)}");
            Check(hp - _target.CurrentHp == expected && preview == expected && Power.State.CombosThisCombat == 4
                && finale.EnergyCost.GetWithModifiers(CostModifiers.Local) == (up ? 1 : 2), "BAL15_finale_uncapped_combat_scaling_" + up);

            foreach (var strength in new[] { -5, 0, 3 })
            {
                await FreshExpansion();
                if (strength != 0) await PowerCmd.Apply<StrengthPower>(ExpansionContext(), _player.Creature, strength, _player.Creature, null);
                var rock = await Add<ShatteringRock>(up);
                rock.DynamicVars.Damage.UpdateCardPreview(rock, CardPreviewMode.Normal, _target, true);
                preview = rock.DynamicVars.Damage.PreviewValue;
                hp = _target.CurrentHp;
                await Queue(new MegaCrit.Sts2.Core.GameActions.PlayCardAction(rock, _target));
                expected = Math.Max(0, (up ? 18 : 12) + strength * 3);
                Check(hp - _target.CurrentHp == expected && preview == expected && rock.EnergyCost.Canonical == 2,
                    "BAL15_rock_triple_strength_preview_" + strength + "_" + up);
            }
        }
        await FreshExpansion();
        Check(Power.State.CombosThisCombat == 0, "BAL15_finale_new_combat_resets");
        _target.SetCurrentHpInternal(7);
        await Play<RockGuard>(); await Play<LightningThrust>();
        var countedFinale = await Add<TriphaseFinale>();
        Check(Power.State.CombosThisCombat == 1 && Power.State.RewardsThisTurn == 0
            && countedFinale.DynamicVars.CalculatedDamage.Calculate(_target) == 12, "BAL15_finale_counts_trigger_without_reward");

        await FreshExpansion();
        await Play<LightningThrust>();
        await PowerCmd.Apply<StrengthPower>(ExpansionContext(), _player.Creature, 2, _player.Creature, null);
        var before = _target.CurrentHp;
        await Play<ShatteringRock>();
        Check(before - _target.CurrentHp == 24, "BAL15_rock_combo_followup_keeps_normal_strength");
        await FreshExpansion();
        await PowerCmd.Apply<StrengthPower>(ExpansionContext(), _player.Creature, 2, _player.Creature, null);
        await PowerCmd.Apply<WeakPower>(ExpansionContext(), _player.Creature, 1, _target, null);
        await PowerCmd.Apply<VulnerablePower>(ExpansionContext(), _target, 1, _player.Creature, null);
        var modified = await Add<ShatteringRock>();
        modified.DynamicVars.Damage.UpdateCardPreview(modified, CardPreviewMode.Normal, _target, true);
        var modifiedPreview = Math.Floor(modified.DynamicVars.Damage.PreviewValue);
        before = _target.CurrentHp;
        await Queue(new MegaCrit.Sts2.Core.GameActions.PlayCardAction(modified, _target));
        Check(before - _target.CurrentHp == 20 && modifiedPreview == 20,
            "BAL15_rock_native_weak_vulnerable_once");

        foreach (var copies in new[] { new[] { false, false }, new[] { true, true }, new[] { false, true },
            new[] { true, false }, new[] { false, true, false, true } })
        {
            await FreshExpansion(); await SeedDraw();
            if (copies.Length == 4) await RelicCmd.Obtain<JourneyCompass>(_player);
            foreach (var up in copies) await Play<ElementResonance>(up);
            var order = string.Join("", copies.Select(up => up ? "U" : "B"));
            var power = _player.Creature.GetPower<ElementResonancePower>()!;
            Check(power.Amount == copies.Length && power.Description.GetFormattedText().Contains(
                string.Join(" / ", copies.Select(up => up ? 2 : 1))), "BAL15_resonance_order_description_" + order);
            await Play<WindBlade>();
            for (var i = 0; i <= copies.Length; i++)
            {
                var energy = _player.PlayerCombatState!.Energy;
                if (Power.State.Last == Element.Wind) await Play<RockGuard>(); else await Play<WindBlade>();
                var gain = i < copies.Length ? (copies[i] ? 2 : 1) : 0;
                Check(_player.PlayerCombatState.Energy == energy - 1 + gain, "BAL15_resonance_order_" + order + "_" + i);
            }
            await ExpansionNativeTurn();
            var nextEnergy = _player.PlayerCombatState!.Energy;
            if (Power.State.Last == Element.Wind) await Play<RockGuard>(); else await Play<WindBlade>();
            Check(_player.PlayerCombatState.Energy == nextEnergy - 1 + (copies[0] ? 2 : 1),
                "BAL15_resonance_order_resets_" + order);
        }
        GD.Print($"STS2MOD_BALANCE15_COMPLETE assertions={_assertions - start} multiplayer=NOT_RUN rendering=NOT_RUN");
    }
}
