using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Sts2Mod.Combo;

/// <summary>Stateless subscription. Mutable state belongs only to each creature's power.</summary>
public sealed class ComboLifecycle : SingletonModel
{
    public override bool ShouldReceiveCombatHooks => true;
    public override Task AfterCardExhausted(PlayerChoiceContext context, CardModel card, bool causedByEthereal)
    {
        CombatCardElements.Exhaustions.Set(card, CombatCardElements.Exhaustions.Get(card) + 1);
        return Task.CompletedTask;
    }
    public override async Task AfterCardPlayed(PlayerChoiceContext context, MegaCrit.Sts2.Core.Entities.Cards.CardPlay play)
    {
        if (play.Card is ElementCardModel || CombatCardElements.Get(play.Card) == Element.None) return;
        var reservation = await ComboPower.Reserve(play.Card, context, play, CombatCardElements.Get(play.Card));
        await ComboPower.Resolve(play.Card, context, play, reservation);
    }

    public override bool ShouldPlay(CardModel card, MegaCrit.Sts2.Core.Entities.Cards.AutoPlayType autoPlayType)
        => card is not Sts2Mod.Cards.HeavenEarthConvergence
            || card.Owner.Creature.GetPower<ComboPower>()?.State.Last is Element.Wind or Element.Rock or Element.Lightning;

    public override async Task AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom room)
    {
        foreach (var player in room.CombatState.Players)
            foreach (var card in player.Deck.Cards) CombatCardElements.Assigned.Set(card, 0);
        await Task.CompletedTask;
    }
    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    {
        if (!player.Creature.IsAlive) return;
        // Preserve existing Ironclad saves containing these cards, without adding a status to other new characters.
        if (player.Character is not Sts2Mod.Characters.Traveler && !player.Deck.Cards.OfType<ElementCardModel>().Any()) return;
        var power = player.Creature.GetPower<ComboPower>()
            ?? await PowerCmd.Apply<ComboPower>(context, player.Creature, 1m, player.Creature, null, silent: true);
        power?.BeginTurn();
    }
}
