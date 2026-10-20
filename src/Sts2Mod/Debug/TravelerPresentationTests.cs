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
            Check(model.HoverTips.Count() >= 2 && !model.GetDescriptionForPile(PileType.None).Contains("{ComboPreview}"), "UX_library_safe_" + model.Id.Entry);
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
        var hp = _target.CurrentHp;
        Check(ComboPresentation.Preview(noTarget).LocEntryKey == "TC_PREVIEW.noTarget", "UX_no_target_warns_consumed_use");
        await Queue(new PlayCardAction(noTarget, null));
        Check(_target.CurrentHp == hp && Power.State.Used == 1, "UX_no_target_preview_matches_native_skip");

        var effects = new[] { ComboKind.WindRock, ComboKind.RockLightning, ComboKind.LightningWind }
            .Select(kind => new ElementComboVfx { Kind = kind, Doubled = true }).ToArray();
        foreach (var effect in effects) GetTree().Root.AddChild(effect);
        await Until(() => effects.All(effect => !GodotObject.IsInstanceValid(effect)), "finite presentation effect cleanup", 5);
        Check(effects.All(effect => !GodotObject.IsInstanceValid(effect)), "UX_three_effects_free_without_gameplay_wait");
        GD.Print($"STS2MOD_UX_COMPLETE assertions={_assertions - startCount} rendering=NOT_RUN multiplayer=NOT_RUN");
    }
}
