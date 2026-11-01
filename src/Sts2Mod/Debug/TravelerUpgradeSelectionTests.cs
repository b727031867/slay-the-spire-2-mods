using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using Sts2Mod.Cards;
using Sts2Mod.Events;
using Sts2Mod.Presentation;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestJourneyPlansUpgradeUi()
    {
        using var nativeSelector = CardSelectCmd.SuspendSelectorForTest();
        foreach (var upgraded in new[] { false, true })
        {
            await FreshExpansion();
            foreach (var card in PileType.Hand.GetPile(_player).Cards.ToArray())
                await CardPileCmd.Add(card, PileType.Discard);
            var damage = await Add<WindBlade>();
            var cost = await Add<WindGuidance>();
            var keyword = await Add<WindPathfinding>();
            var plans = await Add<JourneyPlans>(upgraded);
            var hand = NCombatRoom.Instance!.Ui.Hand;
            var container = hand.GetNode<NSelectedHandCardContainer>("%SelectedHandCardContainer");
            await Until(() => new[] { damage, cost, keyword, plans }.All(c => NCard.FindOnTable(c) != null), "upgrade UI hand mounted");
            var originals = new[] { damage, cost, keyword }.ToDictionary(card => card, card => NCard.FindOnTable(card)!);
            var deckLevels = _player.Deck.Cards.Select(card => card.CurrentUpgradeLevel).ToArray();
            var play = Queue(new PlayCardAction(plans, null));
            await Until(() => hand.CurrentMode == NPlayerHand.Mode.SimpleSelect, "JourneyPlans native multi-select");
            var confirm = hand.GetNode<NButton>("%SelectModeConfirmButton");
            Check(confirm.IsEnabled == upgraded, "UPGRADE_UI_native_selection_" + upgraded);

            async Task Select(CardModel card)
            {
                var holder = originals[card].GetParent<NHandCardHolder>();
                hand.Call("SelectCardInSimpleMode", holder);
                await Until(() => originals[card].GetNodeOrNull<NCard>(HandUpgradePreviews.PreviewName) != null, "selected upgraded visual");
                var preview = originals[card].GetNode<NCard>(HandUpgradePreviews.PreviewName);
                Check(container.Holders.Any(h => h.CardNode == originals[card] && h.CardNode.Model == card)
                    && originals[card].GetChildren().OfType<NCard>().Count() == 1
                    && preview.Model != card && preview.Model!.CurrentUpgradeLevel == 1
                    && preview.Model.UpgradePreviewType == CardUpgradePreviewType.Combat
                    && preview.GetNode<Control>("%DescriptionLabel").Get("text").AsString()
                        == "[center]" + preview.Model.GetDescriptionForUpgradePreview() + "[/center]",
                    "UPGRADE_UI_visual_" + card.Id.Entry + "_" + upgraded);
                Check(preview.FindChildren("*", recursive: true, owned: false).OfType<Control>().Prepend(preview)
                    .All(control => control.MouseFilter == Control.MouseFilterEnum.Ignore && control.FocusMode == Control.FocusModeEnum.None),
                    "UPGRADE_UI_input_passthrough_" + card.Id.Entry + "_" + upgraded);
            }

            await Select(damage);
            Check(damage.DynamicVars.Damage.BaseValue < originals[damage].GetNode<NCard>(HandUpgradePreviews.PreviewName).Model!.DynamicVars.Damage.BaseValue,
                "UPGRADE_UI_damage_increases_" + upgraded);
            container.DeselectCard(damage);
            await Until(() => originals[damage].GetNodeOrNull<NCard>(HandUpgradePreviews.PreviewName) == null, "deselected preview removed");
            Check(originals[damage].GetNodeOrNull<NCard>(HandUpgradePreviews.PreviewName) == null
                && !damage.IsUpgraded, "UPGRADE_UI_deselect_cleans_" + upgraded);
            await Select(cost);
            var costPreview = originals[cost].GetNode<NCard>(HandUpgradePreviews.PreviewName);
            Check(cost.EnergyCost.GetWithModifiers(CostModifiers.Local) == 1
                && costPreview.Model!.EnergyCost.GetWithModifiers(CostModifiers.Local) == 0
                && costPreview.GetNode<Control>("%EnergyLabel").Get("text").AsString().Contains('0'),
                "UPGRADE_UI_cost_decreases_" + upgraded);
            await Select(keyword);
            Check(keyword.Keywords.Contains(CardKeyword.Exhaust)
                && !originals[keyword].GetNode<NCard>(HandUpgradePreviews.PreviewName).Model!.Keywords.Contains(CardKeyword.Exhaust)
                && container.Holders.Count == (upgraded ? 2 : 1)
                && originals.Values.All(node => !node.Model!.IsUpgraded), "UPGRADE_UI_keywords_and_limit_" + upgraded);
            hand.Call("OnSelectModeConfirmButtonPressed", confirm);
            await play;
            await Until(() => container.Holders.Count == 0, "native selected holders released");
            Check(keyword.IsUpgraded && cost.IsUpgraded == upgraded && !damage.IsUpgraded
                && originals.All(pair => pair.Value.Model == pair.Key && pair.Value.GetNodeOrNull<NCard>(HandUpgradePreviews.PreviewName) == null)
                && _player.Deck.Cards.Select(card => card.CurrentUpgradeLevel).SequenceEqual(deckLevels),
                "UPGRADE_UI_confirm_identity_and_temporary_upgrade_" + upgraded);

            // Re-enter after disposal: the old scope must not decorate a different selection.
            var repeat = Queue(new PlayCardAction(await Add<JourneyPlans>(true), null));
            await Until(() => hand.CurrentMode == NPlayerHand.Mode.SimpleSelect, "repeat upgrade selection");
            await Select(damage);
            container.DeselectCard(damage);
            await Until(() => originals[damage].GetNodeOrNull<NCard>(HandUpgradePreviews.PreviewName) == null, "repeat deselected preview removed");
            hand.Call("OnSelectModeConfirmButtonPressed", confirm);
            await repeat;
            for (var frame = 0; frame < 3; frame++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(!damage.IsUpgraded && originals.Values.All(node => node.GetNodeOrNull<NCard>(HandUpgradePreviews.PreviewName) == null),
                "UPGRADE_UI_repeat_skip_cleanup_" + upgraded);
            var generic = hand.SelectCards(new MegaCrit.Sts2.Core.CardSelection.CardSelectorPrefs(
                new MegaCrit.Sts2.Core.Localization.LocString("gameplay_ui", "CHOOSE_CARD_UPGRADE_HEADER"), 0, 1), c => c == damage, null);
            hand.Call("SelectCardInSimpleMode", originals[damage].GetParent<NHandCardHolder>());
            for (var frame = 0; frame < 3; frame++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(originals[damage].GetNodeOrNull<NCard>(HandUpgradePreviews.PreviewName) == null,
                "UPGRADE_UI_disposed_scope_inactive_" + upgraded);
            var canceled = false;
            try
            {
                using var previews = new HandUpgradePreviews(_player);
                await Until(() => originals[damage].GetNodeOrNull<NCard>(HandUpgradePreviews.PreviewName) != null, "canceled scope has a live preview");
                hand.Call("CancelHandSelectionIfNecessary");
                await generic;
            }
            catch (OperationCanceledException) { canceled = true; }
            for (var frame = 0; frame < 3; frame++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(canceled && hand.CurrentMode == NPlayerHand.Mode.Play && container.Holders.Count == 0
                && originals[damage].GetNodeOrNull<NCard>(HandUpgradePreviews.PreviewName) == null && !damage.IsUpgraded
                && NCard.FindOnTable(damage) == originals[damage], "UPGRADE_UI_canceled_scope_cleanup_" + upgraded);
        }
        GD.Print("STS2MOD_UPGRADE_UI_COMPLETE selection=native clone=isolated rendering=NOT_RUN multiplayer=NOT_RUN");
    }

    private async Task TestGuestSurveyUpgradeUi()
    {
        using var nativeSelector = CardSelectCmd.SuspendSelectorForTest();
        Check(CardSelectCmd.Selector == null, "EVENT_guest_upgrade_native_selector");
        var survey = await EventFixture<MisalignedSurvey>();
        var card = _player.Deck.Cards.First(c => c.IsUpgradable);
        var gold = _player.Gold;
        var chosen = survey.CurrentOptions[0].Chosen();
        NDeckUpgradeSelectScreen? screen = null;
        await Until(() => (screen = GetTree().Root.FindChildren("*", recursive: true, owned: false)
            .OfType<NDeckUpgradeSelectScreen>().FirstOrDefault(s => s.IsInsideTree())) != null, "guest native deck upgrade screen");
        AccessTools.Method(typeof(NDeckUpgradeSelectScreen), "OnCardClicked").Invoke(screen, [card]);
        var preview = (NUpgradePreview)AccessTools.Field(typeof(NDeckUpgradeSelectScreen), "_singlePreview").GetValue(screen)!;
        var after = preview.GetNode<Control>("%After").FindChildren("*", recursive: true, owned: false).OfType<NCard>().Single();
        Check(preview.Card == card && after.Model != card && after.Model!.CurrentUpgradeLevel == 1
            && after.Model.UpgradePreviewType == CardUpgradePreviewType.Deck && !card.IsUpgraded && _player.Gold == gold,
            "EVENT_guest_upgrade_native_preview_before_confirm");
        AccessTools.Method(typeof(NDeckUpgradeSelectScreen), "CancelSelection").Invoke(screen, [null]);
        AccessTools.Method(typeof(NDeckUpgradeSelectScreen), "CloseSelection").Invoke(screen, [null]);
        await chosen;
        Check(!card.IsUpgraded && _player.Gold == gold && !survey.IsFinished,
            "EVENT_guest_upgrade_native_cancel_preserves_card_gold");
    }
}
