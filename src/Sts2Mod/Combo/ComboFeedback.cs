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
        // Native numbers/cards carry amounts. This layer adds no bubbles or enlarged relic artwork.
        if (actualDraw is <= 0) return;
        if (reservation.PaimonDoubles)
            creature.Player?.Relics.OfType<PaimonCheer>().FirstOrDefault(r => !r.IsMelted)?.Flash();
        GD.Print($"STS2MOD_FEEDBACK kind={reservation.Kind} bonus={ComboPresentation.Reward(reservation.Kind, reservation.PaimonDoubles, ComboPresentation.EchoCopies(creature.Player))} doubled={reservation.PaimonDoubles}");
        if (DisplayServer.GetName() == "headless") return;
        Presentation.ElementAudio.Play(Presentation.ElementAudio.ComboPath);
        // Attack/block fusion is already placed at the authoritative native result, once per target.
        if ((reservation.Kind == ComboKind.LightningWind && actualDraw > 0)
            || Presentation.CombatPresentation.Rig(creature) is null)
            ElementComboVfx.Show(effectTarget, reservation.Kind, reservation.PaimonDoubles);
    }
}
