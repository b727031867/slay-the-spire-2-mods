using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Sts2Mod.Cards;
using Sts2Mod.Combo;
using Sts2Mod.Relics;

namespace Sts2Mod.Debug;

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
    private static HoverTip CardComboTip(ElementCardModel card) => card.HoverTips.OfType<HoverTip>().Single(tip =>
        tip.Title == new LocString("powers", "TC_PREVIEW.title").GetFormattedText());
    private void CheckNoComboPanel(string suffix) => Check(VisualDescendants(NCombatRoom.Instance!.Ui)
        .All(node => node.Name != "TravelerComboHud" && node.GetType().Name != "ComboHud"), "VIS_no_custom_panel_" + suffix);
    private void CheckStatusIcon(Element element)
    {
        var node = VisualDescendants(_player.Creature.GetCreatureNode()!).OfType<NPower>().Single(p => p.Model == Power);
        Check(node.GetNode<TextureRect>("%Icon").Texture.ResourcePath == ComboIcons.ElementPath(element),
            "VIS_native_status_icon_" + element);
    }
    private async Task TestComboHud()
    {
        var start = _assertions;
        await FreshExpansion();
        await TestEnergyIcons();
        // Give native deferred UI setup a frame: absence must hold after mounting would have occurred.
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        CheckNoComboPanel("fresh_combat");
        foreach (var element in Enum.GetValues<Element>())
            Check(ComboIcons.ElementTexture(element)?.GetWidth() == 128, "VIS_icon_resource_" + element);
        CheckStatusIcon(Element.None);
        await Play<WindBlade>();
        CheckStatusIcon(Element.Wind);
        var rock = (RockGuard)await Add<RockGuard>();
        var lightning = (LightningThrust)await Add<LightningThrust>();
        var thunder = (ThunderPrep)await Add<ThunderPrep>();
        var same = (WindBlade)await Add<WindBlade>();
        Check(CardComboTip(rock).Description == ComboPresentation.Preview(rock).GetFormattedText()
            && ComboPresentation.VisualPreview(rock) is { Key: "block", Bonus: 4 }, "VIS_native_card_tip_block");
        Check(CardComboTip(lightning).Description == ComboPresentation.Preview(lightning).GetFormattedText()
            && ComboPresentation.VisualPreview(lightning) is { Key: "draw", Bonus: 1 }, "VIS_native_card_tip_draw");
        Check(ComboPresentation.Preview(same).LocEntryKey == "TC_PREVIEW.record", "VIS_native_card_tip_record");
        var encoded = Power.State.Encoded;
        var rng = _player.RunState.Rng.CombatTargets.Counter;
        var energy = _player.PlayerCombatState!.Energy;
        var hand = PileType.Hand.GetPile(_player).Cards.Count;
        for (var i = 0; i < 100; i++) { _ = CardComboTip(rock); _ = CardComboTip(lightning); }
        Check(Power.State.Encoded == encoded && _player.RunState.Rng.CombatTargets.Counter == rng
            && _player.PlayerCombatState.Energy == energy && PileType.Hand.GetPile(_player).Cards.Count == hand,
            "VIS_100_tooltip_reads_no_state_rng_energy_or_hand_mutation");
        await PlayerCmd.SetEnergy(0, _player);
        Check(ComboPresentation.Preview(rock).LocEntryKey == "TC_PREVIEW.unplayable", "VIS_native_card_tip_unplayable");
        await PlayerCmd.SetEnergy(30, _player);
        await RelicCmd.Obtain<PaimonCheer>(_player);
        Check(CardComboTip(rock).Description.Contains('8') && CardComboTip(lightning).Description.Contains('2'), "VIS_native_card_tip_paimon");
        await Queue(new MegaCrit.Sts2.Core.GameActions.PlayCardAction(rock, null));
        CheckStatusIcon(Element.Rock);
        Check(CardComboTip(lightning).Description == ComboPresentation.Preview(lightning).GetFormattedText()
            && ComboPresentation.VisualPreview(lightning) is { Key: "attack", Bonus: 8 }, "VIS_native_card_tip_targeted_attack");
        Check(CardComboTip(thunder).Description == ComboPresentation.Preview(thunder).GetFormattedText()
            && ComboPresentation.VisualPreview(thunder) is { Key: "randomAttack", Bonus: 8 }, "VIS_native_card_tip_random_attack");
        await Queue(new MegaCrit.Sts2.Core.GameActions.PlayCardAction(thunder, null));
        CheckStatusIcon(Element.Lightning);
        await Play<WindBlade>();
        Check(ComboPresentation.Preview(lightning).LocEntryKey == "TC_PREVIEW.exhausted", "VIS_native_card_tip_exhausted");
        CheckNoComboPanel("after_plays");
        await NextTurn(preserveElement: true);
        Check(Power.State.Last == Element.Wind && Power.State.Remaining == 3, "VIS_turn_reset_preserves_status");
        await StartFreshCombat();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        CheckNoComboPanel("next_combat");
        CheckStatusIcon(Element.None);
        foreach (var relic in _player.Relics.OfType<PaimonCheer>().ToArray()) await RelicCmd.Remove(relic);
        GD.Print($"STS2MOD_VISUAL_COMPLETE assertions={_assertions - start} panel=ABSENT status=PASS tooltip=PASS readonly=PASS rendering=NOT_RUN multiplayer=NOT_RUN");
    }
}
