using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;

namespace Sts2Mod.Characters;

// Guard future restricted pools that omit a shop type. The current 18-card pool
// has attacks, skills and powers, so normal native slots need no substitution.
[HarmonyPatch(typeof(CardFactory), nameof(CardFactory.CreateForMerchant),
    new Type[] { typeof(Player), typeof(IEnumerable<CardModel>), typeof(CardType) })]
internal static class TravelerMerchantPatch
{
    private static void Prefix(Player player, ref IEnumerable<CardModel> options, ref CardType type)
    {
        if (player.Character is not Traveler) return;
        var available = options.ToArray();
        options = available;
        var requested = type;
        if (available.Any(card => card.Rarity != CardRarity.Basic && card.Type == requested)) return;
        var fallback = available.FirstOrDefault(card => card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare);
        if (fallback is not null) type = fallback.Type;
    }
}
