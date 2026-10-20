using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Runs;
using Sts2Mod.Cards;
using Sts2Mod.Combo;
using Sts2Mod.Relics;

namespace Sts2Mod.Debug;

internal partial class ComboHudFixtureModal : Control, IScreenContext
{
    public Control? DefaultFocusedControl => null;
}

public partial class TravelerTestRunner
{
    private static IEnumerable<Node> VisualDescendants(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            yield return child;
            foreach (var descendant in VisualDescendants(child)) yield return descendant;
        }
    }
    private ComboHud CurrentHud => NCombatRoom.Instance!.Ui.GetNode<ComboHud>(ComboHud.NodeName);
    private async Task TestComboHud()
    {
        var start = _assertions;
        await FreshExpansion();
        await Until(() => NCombatRoom.Instance!.Ui.GetNodeOrNull<ComboHud>(ComboHud.NodeName) is not null, "native combo HUD mounted");
        var hud = CurrentHud;
        hud.RefreshNow();
        Check(hud.Visible && hud.LastView is { Current: Element.None, Remaining: 3 }, "VIS_always_visible_without_hover");
        Check(NCombatRoom.Instance!.Ui.GetChildren().OfType<ComboHud>().Count() == 1, "VIS_one_hud_per_combat_ui");
        Check(VisualDescendants(hud).OfType<Control>().Prepend(hud).All(c => c.MouseFilter == Control.MouseFilterEnum.Ignore
            && c.FocusMode == Control.FocusModeEnum.None), "VIS_all_controls_click_through_no_focus");
        Check(hud.Position.X >= 0 && hud.Position.Y >= 80 && hud.Position.X + hud.Size.X < 1280
            && hud.Position.Y + hud.Size.Y < 720, "VIS_layout_bounds_1280_720");
        var modals = NModalContainer.Instance!;
        Check(modals.OpenModal is null, "VIS_no_unexpected_fixture_modal");
        var modal = new ComboHudFixtureModal();
        modals.Add(modal, showBackstop: false); hud.RefreshNow();
        Check(!hud.Visible && hud.HighlightedKind == ComboKind.None, "VIS_modal_hides_hud_and_highlight");
        if (!ReferenceEquals(modals.OpenModal, modal)) throw new InvalidOperationException("Preserving an unexpected fixture modal");
        modals.Clear(); hud.RefreshNow();
        Check(hud.Visible, "VIS_hud_returns_after_modal_closes");
        foreach (var element in Enum.GetValues<Element>())
        {
            var texture = ComboIcons.ElementTexture(element);
            Check(texture is not null && texture.GetWidth() == 128, "VIS_icon_resource_" + element);
        }
        foreach (var kind in new[] { ComboKind.WindRock, ComboKind.RockLightning, ComboKind.LightningWind })
            Check(ComboIcons.RewardTexture(kind) is not null, "VIS_reward_resource_" + kind);
        var tracker = RunManager.Instance.HoveredModelTracker;
        await Play<WindBlade>(); hud.RefreshNow();
        Check(hud.LastView is { Current: Element.Wind } && Power.Icon.ResourcePath == ComboIcons.ElementPath(Element.Wind), "VIS_actual_play_updates_wind_icon");
        var nativePower = VisualDescendants(_player.Creature.GetCreatureNode()!).OfType<NPower>().Single(p => p.Model == Power);
        Check(nativePower.GetNode<TextureRect>("%Icon").Texture.ResourcePath == ComboIcons.ElementPath(Element.Wind), "VIS_native_status_icon_refreshes_after_play");
        var rock = (RockGuard)await Add<RockGuard>();
        var lightning = (LightningThrust)await Add<LightningThrust>();
        var thunder = (ThunderPrep)await Add<ThunderPrep>();
        var same = (WindBlade)await Add<WindBlade>();
        var encoded = Power.State.Encoded;
        var rng = _player.RunState.Rng.CombatTargets.Counter;
        var energy = _player.PlayerCombatState!.Energy;
        var hand = PileType.Hand.GetPile(_player).Cards.Count;
        tracker.OnLocalCardHovered(rock); hud.RefreshNow();
        Check(hud.HighlightedKind == ComboKind.WindRock && hud.LastView is { Current: Element.Wind, Preview.Element: Element.Rock }, "VIS_hover_rock_highlights_block_not_current_element");
        for (var i = 0; i < 100; i++) { hud.RefreshNow(); _ = ComboPresentation.VisualPreview(rock); }
        Check(Power.State.Encoded == encoded && _player.RunState.Rng.CombatTargets.Counter == rng
            && _player.PlayerCombatState.Energy == energy && PileType.Hand.GetPile(_player).Cards.Count == hand,
            "VIS_100_previews_no_state_rng_energy_or_hand_mutation");
        tracker.OnLocalCardSelected(lightning); hud.RefreshNow();
        Check(hud.HighlightedKind == ComboKind.LightningWind, "VIS_selected_card_overrides_hover_and_highlights_draw");
        tracker.OnLocalCardDeselected(); hud.RefreshNow();
        Check(hud.HighlightedKind == ComboKind.WindRock, "VIS_cancel_selection_returns_to_hover");
        tracker.OnLocalCardUnhovered(); hud.RefreshNow();
        Check(hud.HighlightedKind == ComboKind.None && hud.LastView?.Preview is null, "VIS_unhover_clears_highlight");
        tracker.OnLocalCardHovered(same); hud.RefreshNow();
        Check(hud.HighlightedKind == ComboKind.None && hud.LastView?.Preview?.Key == "record", "VIS_same_element_only_records");
        var ability = await Add<ElementResonance>();
        tracker.OnLocalCardSelected(ability); hud.RefreshNow();
        Check(hud.LastView?.Preview is null && hud.HighlightedKind == ComboKind.None, "VIS_ability_has_no_element_highlight");
        tracker.OnLocalCardDeselected();
        tracker.OnLocalCardHovered(rock);
        await PlayerCmd.SetEnergy(0, _player); hud.RefreshNow();
        Check(hud.HighlightedKind == ComboKind.None && hud.LastView?.Preview?.Key == "unplayable", "VIS_unplayable_never_highlights_reward");
        await PlayerCmd.SetEnergy(30, _player);
        await RelicCmd.Obtain<PaimonCheer>(_player); hud.RefreshNow();
        Check(hud.LastView is { Doubled: true, Preview.Bonus: 8 }, "VIS_paimon_updates_preview_and_chips");
        tracker.OnLocalCardUnhovered();
        await Queue(new MegaCrit.Sts2.Core.GameActions.PlayCardAction(rock, null)); hud.RefreshNow();
        Check(hud.LastView is { Current: Element.Rock, Remaining: 2 } && Power.Icon.ResourcePath == ComboIcons.ElementPath(Element.Rock), "VIS_actual_play_updates_rock_and_remaining");
        tracker.OnLocalCardSelected(lightning); hud.RefreshNow();
        Check(hud.HighlightedKind == ComboKind.RockLightning && hud.LastView?.Preview?.Key == "attack", "VIS_targeted_attack_highlight_is_base_and_conditional");
        tracker.OnLocalCardDeselected(); tracker.OnLocalCardSelected(thunder); hud.RefreshNow();
        Check(hud.HighlightedKind == ComboKind.RockLightning && hud.LastView?.Preview?.Key == "randomAttack", "VIS_skill_attack_highlight_is_random_no_rng_use");
        tracker.OnLocalCardDeselected();
        await Queue(new MegaCrit.Sts2.Core.GameActions.PlayCardAction(thunder, null)); hud.RefreshNow();
        Check(hud.LastView is { Current: Element.Lightning, Remaining: 1 } && Power.Icon.ResourcePath == ComboIcons.ElementPath(Element.Lightning), "VIS_actual_play_updates_lightning_icon");
        await Play<WindBlade>(); hud.RefreshNow();
        tracker.OnLocalCardHovered(lightning); hud.RefreshNow();
        Check(hud.LastView is { Remaining: 0 } && hud.HighlightedKind == ComboKind.None
            && hud.LastView?.Preview?.Key == "exhausted", "VIS_cap_zero_dims_all_rewards");
        tracker.OnLocalCardUnhovered();
        await NextTurn(preserveElement: true); hud.RefreshNow();
        Check(hud.LastView is { Current: Element.Wind, Remaining: 3 }, "VIS_turn_reset_keeps_element_and_restores_pips");
        var oldHud = hud;
        await StartFreshCombat();
        await Until(() => !GodotObject.IsInstanceValid(oldHud), "old HUD freed with native room");
        CurrentHud.RefreshNow();
        Check(CurrentHud.LastView is { Current: Element.None, Remaining: 3 }
            && CurrentHud.HighlightedKind == ComboKind.None, "VIS_new_combat_clears_element_selection_and_old_panel");
        foreach (var relic in _player.Relics.OfType<PaimonCheer>().ToArray()) await RelicCmd.Remove(relic);
        GD.Print($"STS2MOD_VISUAL_COMPLETE assertions={_assertions - start} nodes=PASS readonly=PASS rendering=NOT_RUN multiplayer=NOT_RUN");
    }
}
