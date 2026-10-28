using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using Sts2Mod.Characters;

namespace Sts2Mod.Events;

[HarmonyPatch(typeof(TheFutureOfPotions), "PotionToCardType", MethodType.Getter)]
internal static class TravelerPotionTradePatch
{
    private static readonly System.Reflection.MethodInfo GetCardRarity = AccessTools.Method(typeof(TheFutureOfPotions), "GetCardRarity");

    private static void Prefix(TheFutureOfPotions __instance, ref Dictionary<PotionModel, CardType>? ____cardTypes)
    {
        if (__instance.Owner?.Character is not Traveler || ____cardTypes is not null) return;
        var player = __instance.Owner;
        var pool = player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint).ToArray();
        var choices = new Dictionary<PotionModel, CardType>();
        foreach (var potion in player.Potions)
        {
            var rarity = (CardRarity)GetCardRarity.Invoke(__instance, [potion])!;
            // The native trade requires three distinct cards of the advertised rarity and type.
            var types = new[] { CardType.Attack, CardType.Skill, CardType.Power }
                .Where(type => pool.Count(card => card.Rarity == rarity && card.Type == type) >= 3).ToArray();
            if (types.Length == 0) throw new InvalidOperationException($"No Traveler potion trade can offer three {rarity} cards.");
            choices.Add(potion, __instance.Rng.NextItem(types));
        }
        ____cardTypes = choices;
    }
}
