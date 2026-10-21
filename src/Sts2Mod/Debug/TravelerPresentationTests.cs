using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;
using Sts2Mod.Cards;
using Sts2Mod.Combo;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestContinuousCombos()
    {
        // A previous fixture can have ended combat; never target its old creature.
        _target = _player.Creature.CombatState!.Enemies.First(enemy => enemy.IsAlive);
        _target.SetMaxHpInternal(3000m);
        _target.SetCurrentHpInternal(3000m);
        var doubled = ComboPresentation.HasPaimon(_player);
        var suffix = doubled ? "paimon" : "normal";
        var blockBonus = doubled ? 8 : 4;
        var draws = doubled ? 2 : 1;
        await NextTurn();
        await PlayerCmd.SetEnergy(3m, _player);
        var hp = _target.CurrentHp;
        await Play<WindBlade>(); await Play<RockGuard>();
        var thirdWind = (WindBlade)await Add<WindBlade>();
        Check(Power.State.Last == Element.Rock && Power.State.Used == 1
            && ComboPresentation.Preview(thirdWind).LocEntryKey == "TC_PREVIEW.block",
            "CHAIN_after_pair_retains_rock_and_previews_next_block_" + suffix);
        await Queue(new PlayCardAction(thirdWind, _target));
        GD.Print($"STS2MOD_CHAIN_DETAIL case=three_energy_{suffix} damage={hp - _target.CurrentHp} block={_player.Creature.Block} used={Power.State.Used} last={Power.State.Last} energy={_player.PlayerCombatState!.Energy}");
        Check(hp - _target.CurrentHp == 12 && _player.Creature.Block == 7 + 2 * blockBonus
            && Power.State.Used == 2 && Power.State.Last == Element.Wind && _player.PlayerCombatState!.Energy == 0,
            "CHAIN_three_energy_wind_rock_wind_two_blocks_" + suffix);

        await NextTurn(); await PlayerCmd.SetEnergy(3m, _player); hp = _target.CurrentHp;
        await Play<WindBlade>(); await Play<RockGuard>(); await Play<LightningThrust>();
        Check(hp - _target.CurrentHp == 13 + blockBonus && _player.Creature.Block == 7 + blockBonus
            && Power.State.Used == 2 && Power.State.Last == Element.Lightning,
            "CHAIN_three_energy_wind_rock_lightning_block_and_attack_" + suffix);

        await NextTurn(); await SeedDraw();
        await Play<WindBlade>(); await Play<LightningThrust>(); await Play<WindBlade>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 2 * draws && Power.State.Used == 2
            && Power.State.Last == Element.Wind, "CHAIN_wind_lightning_wind_two_draw_rewards_" + suffix);

        await NextTurn();
        await Play<WindBlade>(); await Play<RockGuard>(); await Play<WindBlade>(); await Play<RockGuard>();
        Check(_player.Creature.Block == 14 + 3 * blockBonus && Power.State.Used == 3
            && Power.State.Last == Element.Rock && Power.DisplayAmount == 0,
            "CHAIN_four_cards_reach_shared_three_cap_" + suffix);
        await Play<WindBlade>();
        Check(_player.Creature.Block == 14 + 3 * blockBonus && Power.State.Used == 3
            && Power.State.Last == Element.Wind && Power.State.Preview(Element.Rock) == ComboKind.None,
            "CHAIN_fifth_card_no_fourth_bonus_" + suffix);

        await NextTurn(); await SeedDraw(); hp = _player.Creature.CombatState!.Enemies.Sum(e => e.CurrentHp);
        await Play<RockGuard>(); await Play<ThunderPrep>();
        Check(hp - _player.Creature.CombatState!.Enemies.Sum(e => e.CurrentHp) == blockBonus && Power.State.Last == Element.Lightning && Power.State.Used == 1,
            "CHAIN_random_defensive_reward_still_retains_lightning_" + suffix);
        await Play<WindBlade>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 1 + draws && Power.State.Used == 2
            && Power.State.Last == Element.Wind, "CHAIN_can_continue_after_targetless_pair_" + suffix);
        await NextTurn(preserveElement: true);
        Check(Power.State.Last == Element.Wind && Power.State.Used == 0, "CHAIN_retains_element_at_turn_boundary_" + suffix);
        Power.State.ClearCombat();
    }

    private async Task TestPresentation()
    {
        var startCount = _assertions;
        foreach (var art in new[] { "wind", "rock", "lightning", "tailwind", "rock_advance", "thunder_prep", "paimon" })
        {
            var texture = ResourceLoader.Load<Texture2D>($"res://Sts2Mod/art/{art}.png");
            Check(texture is not null && texture.GetWidth() > 0 && texture.GetHeight() > 0, "UX_art_import_" + art);
        }
        foreach (var type in new[] { typeof(WindBlade), typeof(RockGuard), typeof(LightningThrust), typeof(TailwindPrep), typeof(RockAdvance), typeof(ThunderPrep) })
        {
            var model = (ElementCardModel)ModelDb.GetById<CardModel>(ModelDb.GetId(type));
            Check(!model.HoverTips.OfType<MegaCrit.Sts2.Core.HoverTips.HoverTip>().Any(tip =>
                    tip.Title == new MegaCrit.Sts2.Core.Localization.LocString("powers", "TC_PREVIEW.title").GetFormattedText()
                    || tip.Title == new MegaCrit.Sts2.Core.Localization.LocString("powers", "TC_RULES.title").GetFormattedText())
                && !model.GetDescriptionForPile(PileType.None).Contains("{ComboPreview}"), "UX_library_safe_" + model.Id.Entry);
        }
        Check(ComboPresentation.Trigger(ComboKind.WindRock, false).GetFormattedText().Contains('4')
            && ComboPresentation.Trigger(ComboKind.WindRock, true).GetFormattedText().Contains('8'), "UX_block_feedback_4_and_8");
        Check(ComboPresentation.Trigger(ComboKind.LightningWind, false).GetFormattedText().Contains('1')
            && ComboPresentation.Trigger(ComboKind.LightningWind, true).GetFormattedText().Contains('2'), "UX_draw_feedback_1_and_2");
        Check(ComboPresentation.Trigger(ComboKind.RockLightning, true).GetFormattedText().Contains('8'), "UX_attack_feedback_base8");
        Check(ComboPresentation.Trigger(ComboKind.LightningWind, true, 0).GetFormattedText().Contains("0/2"), "UX_limited_draw_feedback_actual_count");

        await NextTurn();
        await Play<WindBlade>();
        var rock = (RockGuard)await Add<RockGuard>();
        var lightning = (LightningThrust)await Add<LightningThrust>();
        var before = Power.State.Encoded;
        for (var i = 0; i < 100; i++) { ComboPresentation.Preview(rock).GetFormattedText(); _ = rock.HoverTips.ToArray(); }
        Check(Power.State.Encoded == before && Power.State.Used == 0, "UX_hover_100_times_no_gameplay_mutation");
        Check(ComboPresentation.Preview(rock).GetFormattedText().Contains('8') && rock.GetDescriptionForPile(PileType.Hand).Contains('8'), "UX_rock_preview_paimon8_in_card_and_tip");
        Check(ComboPresentation.Preview(lightning).GetFormattedText().Contains('2'), "UX_lightning_preview_paimon_draw2");
        await Queue(new PlayCardAction(rock, null));
        Check(_player.Creature.Block == 15 && Power.State.Used == 1, "UX_preview_matches_native_block_reward");
        var wind = (WindBlade)await Add<WindBlade>();
        await PlayerCmd.SetEnergy(0m, _player);
        var blocked = ComboPresentation.Preview(wind).LocEntryKey;
        Check(blocked == "TC_PREVIEW.unplayable", "UX_no_reward_promise_when_unplayable");
        await PlayerCmd.SetEnergy(30m, _player);
        for (var i = 0; i < 2; i++) { await Play<WindBlade>(); await Play<RockGuard>(); }
        Check(ComboPresentation.Preview(wind).LocEntryKey == "TC_PREVIEW.exhausted", "UX_limit_exhausted_preview");
        await NextTurn();
        await Play<RockGuard>();
        var noTarget = (ThunderPrep)await Add<ThunderPrep>();
        var hp = _player.Creature.CombatState!.Enemies.Sum(e => e.CurrentHp);
        Check(ComboPresentation.Preview(noTarget).LocEntryKey == "TC_PREVIEW.randomAttack", "UX_defensive_preview_promises_random_attack");
        await Queue(new PlayCardAction(noTarget, null));
        Check(hp - _player.Creature.CombatState!.Enemies.Sum(e => e.CurrentHp) == 8 && Power.State.Used == 1, "UX_random_preview_matches_native_attack");

        var effects = new[] { ComboKind.WindRock, ComboKind.RockLightning, ComboKind.LightningWind }
            .Select(kind => ElementComboVfx.Spawn(GetTree().Root, kind, true)).ToArray();
        await Until(() => effects.All(effect => !GodotObject.IsInstanceValid(effect)), "finite presentation effect cleanup", 5);
        Check(effects.All(effect => !GodotObject.IsInstanceValid(effect)), "UX_three_effects_free_without_gameplay_wait");
        GD.Print($"STS2MOD_UX_COMPLETE assertions={_assertions - startCount} rendering=NOT_RUN multiplayer=NOT_RUN");
    }
}
