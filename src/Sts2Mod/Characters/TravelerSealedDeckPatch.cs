using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models.Modifiers;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2Mod.Characters;

[HarmonyPatch]
internal static class TravelerSealedDeckPatch
{
    private static MethodBase TargetMethod() => AccessTools.Method(AccessTools.Method(typeof(SealedDeck), "ChooseCards")
        .GetCustomAttribute<System.Runtime.CompilerServices.AsyncStateMachineAttribute>()!.StateMachineType, "MoveNext");

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var create = AccessTools.Method(typeof(CardFactory), nameof(CardFactory.CreateForReward),
            [typeof(Player), typeof(int), typeof(CardCreationOptions)]);
        var replaced = 0;
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(create))
            {
                instruction.operand = AccessTools.Method(typeof(TravelerSealedDeckPatch), nameof(CreateRewards));
                replaced++;
            }
            yield return instruction;
        }
        if (replaced != 1) throw new InvalidOperationException("Sealed Deck reward generation changed.");
    }

    private static IEnumerable<CardCreationResult> CreateRewards(Player player, int count, CardCreationOptions options)
    {
        if (player.Character is Traveler)
        {
            options = Hook.ModifyCardRewardCreationOptions(player.RunState, player, options);
            var available = options.GetPossibleCards(player)
                .Where(card => card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare)
                .Select(card => card.Id).Distinct().Count();
            // Smaller unlocked or modified pools must still satisfy native distinct-card generation.
            count = Math.Min(count, available);
        }
        return CardFactory.CreateForReward(player, count, options);
    }
}
