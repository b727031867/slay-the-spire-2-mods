using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2Mod.Relics;

[HarmonyPatch(typeof(LastingCandy), nameof(LastingCandy.TryModifyCardRewardOptions))]
internal static class LastingCandyRewardPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var constructor = AccessTools.Constructor(typeof(CardCreationOptions),
            [typeof(IEnumerable<CardModel>), typeof(CardCreationSource), typeof(CardRarityOddsType)]);
        var replaced = 0;
        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Newobj && Equals(instruction.operand, constructor))
            {
                instruction.opcode = OpCodes.Call;
                instruction.operand = AccessTools.Method(typeof(LastingCandyRewardPatch), nameof(CreateOptions));
                replaced++;
            }
            yield return instruction;
        }
        if (replaced != 1) throw new InvalidOperationException("LastingCandy reward constructor changed.");
    }

    internal static CardCreationOptions CreateOptions(IEnumerable<CardModel> cards, CardCreationSource source, CardRarityOddsType odds)
    {
        var pool = cards.ToArray();
        // The native constructor rejects rarity rolls for a pool with just one rarity.
        if (pool.Length > 0 && pool.All(card => card.Rarity == pool[0].Rarity)) odds = CardRarityOddsType.Uniform;
        return new CardCreationOptions(pool, source, odds);
    }
}
