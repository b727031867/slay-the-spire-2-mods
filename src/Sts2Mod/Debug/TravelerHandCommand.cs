using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using Sts2Mod.Cards;

namespace Sts2Mod.Debug;

public sealed class TravelerHandCommand : AbstractConsoleCmd
{
    public override string CmdName => "tc_hand";
    public override string Args => "[1|2|3] [upgraded|lightning] [upgraded]";
    public override string Description => "TEST ONLY: replace combat hand with Wind/Rock or Rock/Lightning pairs; requires --sts2mod-debug, singleplayer Ironclad.";
    public override bool DebugOnly => false;
    public override bool IsNetworked => false;
    public override CmdResult Process(Player? player, string[] args)
    {
        if (!CommandLineHelper.HasArg("sts2mod-debug")) return new(false, "Restart with --sts2mod-debug to enable test commands.");
        if (player?.Character is not Ironclad || !CombatManager.Instance.IsInProgress
            || player.Creature.CombatState?.Players.Count != 1 || CombatManager.Instance.PlayerActionsDisabled
            || MegaCrit.Sts2.Core.Runs.RunManager.Instance.ActionExecutor.CurrentlyRunningAction is not null)
            return new(false, "Requires a singleplayer Ironclad combat during the player phase.");
        if (args.Length > 3 || (args.Length > 0 && (!int.TryParse(args[0], out var n) || n < 1 || n > 3))
            || (args.Length >= 2 && args[1] is not ("upgraded" or "lightning"))
            || (args.Length == 3 && (args[1] != "lightning" || args[2] != "upgraded")))
            return new(false, "Usage: tc_hand [1|2|3] [upgraded|lightning] [upgraded]");
        var pairs = args.Length > 0 ? int.Parse(args[0]) : 1;
        return new(Prepare(player, pairs, args.Contains("upgraded"), args.Contains("lightning")), true, "Test hand prepared; the permanent deck is unchanged.");
    }

    internal static async Task Prepare(Player player, int pairs, bool upgraded, bool lightning = false)
    {
        await CardPileCmd.RemoveFromCombat(PileType.Hand.GetPile(player).Cards.ToArray());
        for (var i = 0; i < pairs; i++)
        {
            foreach (var canonical in lightning
                ? new CardModel[] { ModelDb.Card<RockGuard>(), ModelDb.Card<LightningThrust>() }
                : new CardModel[] { ModelDb.Card<WindBlade>(), ModelDb.Card<RockGuard>() })
            {
                var card = CombatManager.Instance.DebugOnlyGetState()!.CreateCard(canonical, player);
                if (upgraded) CardCmd.Upgrade(card);
                await CardPileCmd.Add(card, PileType.Hand);
            }
        }
        await PlayerCmd.SetEnergy(2 * pairs, player);
    }
}
