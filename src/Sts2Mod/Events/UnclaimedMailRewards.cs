using BaseLib.Utils;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Entities.Cards;
using Sts2Mod.Relics;

namespace Sts2Mod.Events;

internal static class UnclaimedMailRewards
{
    private static readonly SpireField<CardReward, bool> Settled = new(() => false);
    private static readonly System.Reflection.PropertyInfo Options = AccessTools.Property(typeof(CardReward), "Options");
    internal static bool Applies(CardReward reward) => Options.GetValue(reward) is CardCreationOptions { Source: CardCreationSource.Encounter, RarityOdds: CardRarityOddsType.RegularEncounter }
        && reward.Player.Relics.Any(r => r is UnclaimedMail { IsMelted: false });

    internal static async Task<bool> Select(CardReward reward)
    {
        if (Settled.Get(reward)) return true;
        var player = reward.Player;
        // A synchronized native grid keeps charging and card addition in the same selection flow.
        var cards = player.Gold >= 15 ? reward.Cards.Select(c => new CardCreationResult(c)).ToList() : [];
        if (cards.Count == 0) return false;
        var chosen = (await CardSelectCmd.FromSimpleGridForRewards(new BlockingPlayerChoiceContext(), cards, player,
            new CardSelectorPrefs(new LocString("events", "TC_MAIL.select"), 0, 1) { Cancelable = true })).FirstOrDefault();
        if (chosen is null) { await Skip(reward); return true; }
        if (player.Gold < 15) return false;
        var added = await CardPileCmd.Add(chosen, PileType.Deck);
        if (!added.success) return false;
        Settled.Set(reward, true);
        await PlayerCmd.LoseGold(15, player);
        foreach (var offered in reward.Cards)
            player.RunState.CurrentMapPointHistoryEntry?.GetEntry(player.NetId).CardChoices.Add(new CardChoiceHistoryEntry(offered, offered == chosen));
        CardCmd.PreviewCardPileAdd(added);
        return true;
    }

    internal static async Task Skip(CardReward reward)
    {
        if (Settled.Get(reward) || reward.SuccessfullySelected) return;
        Settled.Set(reward, true);
        await PlayerCmd.GainGold(10, reward.Player);
    }
}

[HarmonyPatch(typeof(CardReward), "OnSelect")]
internal static class UnclaimedMailSelectionPatch
{
    private static bool Prefix(CardReward __instance, ref Task<bool> __result)
    {
        if (!UnclaimedMailRewards.Applies(__instance)) return true;
        __result = UnclaimedMailRewards.Select(__instance);
        return false;
    }
}

[HarmonyPatch(typeof(CardReward), nameof(CardReward.Populate))]
internal static class UnclaimedMailOptionsPatch
{
    private static readonly AccessTools.FieldRef<CardReward, int> Count = AccessTools.FieldRefAccess<CardReward, int>("<OptionCount>k__BackingField");
    private static void Prefix(CardReward __instance, out bool __state)
    {
        __state = UnclaimedMailRewards.Applies(__instance) && !__instance.IsPopulated;
        if (__state) Count(__instance)++;
    }
    // Serialize the original count so restoring or rerolling never accumulates extra candidates.
    private static void Finalizer(CardReward __instance, bool __state) { if (__state) Count(__instance)--; }
}

[HarmonyPatch(typeof(CardReward), nameof(CardReward.OnSkipped))]
internal static class UnclaimedMailSkipPatch
{
    private static void Postfix(CardReward __instance)
    {
        if (UnclaimedMailRewards.Applies(__instance)) TaskHelper.RunSafely(UnclaimedMailRewards.Skip(__instance));
    }
}

[HarmonyPatch(typeof(CardReward), nameof(CardReward.Description), MethodType.Getter)]
internal static class UnclaimedMailDescriptionPatch
{
    private static void Postfix(CardReward __instance, ref LocString __result)
    {
        if (UnclaimedMailRewards.Applies(__instance)) __result = new LocString("events", "TC_MAIL.reward");
    }
}
