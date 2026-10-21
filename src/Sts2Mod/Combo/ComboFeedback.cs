using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Relics;
using Sts2Mod.Powers;
using Sts2Mod.Cards;

namespace Sts2Mod.Combo;

/// <summary>Native visual updates and feedback; never advances the combo state.</summary>
public static class ComboFeedback
{
    public static void RefreshHand(Creature creature)
    {
        if (DisplayServer.GetName() == "headless" || creature.Player is not { } player || !CombatManager.Instance.IsInProgress) return;
        foreach (var card in PileType.Hand.GetPile(player).Cards.Where(card => card is ElementCardModel or TriphaseFinale))
        {
            var node = NCard.FindOnTable(card);
            if (node is not null && GodotObject.IsInstanceValid(node) && node.IsInsideTree())
                node.CallDeferred(NCard.MethodName.UpdateVisuals, (int)PileType.Hand, (int)CardPreviewMode.Normal);
        }
    }

    public static void ShowReward(Creature creature, Creature effectTarget, Reservation reservation, int? actualDraw = null)
    {
        // One bubble per actual reward. Skipped target/combat rewards never claim a success.
        if (reservation.PaimonDoubles)
            creature.Player?.Relics.OfType<PaimonCheer>().FirstOrDefault(r => !r.IsMelted)?.Flash();
        var text = ComboPresentation.Trigger(reservation.Kind, reservation.PaimonDoubles, actualDraw).GetFormattedText();
        GD.Print($"STS2MOD_FEEDBACK kind={reservation.Kind} bonus={ComboPresentation.Reward(reservation.Kind, reservation.PaimonDoubles)} doubled={reservation.PaimonDoubles}");
        if (DisplayServer.GetName() == "headless") return;
        if (creature.GetCreatureNode() is not null && creature.GetVfxContainer() is { } container
            && NThoughtBubbleVfx.Create(text, creature, 1.0) is { } bubble)
            container.AddChild(bubble);
        ElementComboVfx.Show(effectTarget, reservation.Kind, reservation.PaimonDoubles);
    }
}
