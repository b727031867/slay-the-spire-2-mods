using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using Sts2Mod.Characters;
using Sts2Mod.Cards;
using Sts2Mod.Relics;

namespace Sts2Mod.Debug;

public sealed class TravelerHandCommand : AbstractConsoleCmd
{
    public override string CmdName => "tc_hand";
    public override string Args => "[1|2|3] [lightning|full] [upgraded] [paimon]";
    public override string Description => "TEST ONLY: prepare Traveler cards and optional Paimon relic; requires --sts2mod-debug, singleplayer Traveler.";
    public override bool DebugOnly => false;
    public override bool IsNetworked => false;
    public override CmdResult Process(Player? player, string[] args)
    {
        if (!CommandLineHelper.HasArg("sts2mod-debug")) return new(false, "Restart with --sts2mod-debug to enable test commands.");
        if (player?.Character is not Traveler || !CombatManager.Instance.IsInProgress
            || player.Creature.CombatState?.Players.Count != 1 || CombatManager.Instance.PlayerActionsDisabled
            || MegaCrit.Sts2.Core.Runs.RunManager.Instance.ActionExecutor.CurrentlyRunningAction is not null)
            return new(false, "Requires a singleplayer Traveler combat during the player phase.");
        if (args.Length > 4 || (args.Length > 0 && (!int.TryParse(args[0], out var n) || n < 1 || n > 3))
            || args.Skip(1).Any(a => a is not ("upgraded" or "lightning" or "full" or "paimon"))
            || args.Skip(1).Distinct().Count() != args.Skip(1).Count()
            || (args.Contains("full") && args.Contains("lightning")))
            return new(false, "Usage: tc_hand [1|2|3] [lightning|full] [upgraded] [paimon]");
        var pairs = args.Length > 0 ? int.Parse(args[0]) : 1;
        if (args.Contains("full") && pairs != 1) return new(false, "The full six-card hand supports one set only.");
        return new(Prepare(player, pairs, args.Contains("upgraded"), args.Contains("lightning"), args.Contains("full"), args.Contains("paimon")), true,
            "Test hand prepared; the permanent deck is unchanged. Optional Paimon is obtained only in this unsaved test run.");
    }

    internal static async Task Prepare(Player player, int pairs, bool upgraded, bool lightning = false, bool full = false, bool paimon = false)
    {
        await CardPileCmd.RemoveFromCombat(PileType.Hand.GetPile(player).Cards.ToArray());
        for (var i = 0; i < pairs; i++)
        {
            var cards = full
                ? new CardModel[] { ModelDb.Card<WindBlade>(), ModelDb.Card<RockGuard>(), ModelDb.Card<LightningThrust>(), ModelDb.Card<TailwindPrep>(), ModelDb.Card<RockAdvance>(), ModelDb.Card<ThunderPrep>() }
                : lightning
                    ? new CardModel[] { ModelDb.Card<RockGuard>(), ModelDb.Card<LightningThrust>() }
                    : new CardModel[] { ModelDb.Card<WindBlade>(), ModelDb.Card<RockGuard>() };
            foreach (var canonical in cards)
            {
                var card = CombatManager.Instance.DebugOnlyGetState()!.CreateCard(canonical, player);
                if (upgraded) CardCmd.Upgrade(card);
                await CardPileCmd.Add(card, PileType.Hand);
            }
        }
        if (paimon && player.GetRelic<PaimonCheer>() is null) await RelicCmd.Obtain<PaimonCheer>(player);
        await PlayerCmd.SetEnergy(10 * pairs, player);
    }
}
