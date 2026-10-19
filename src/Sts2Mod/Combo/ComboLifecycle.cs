using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;

namespace Sts2Mod.Combo;

/// <summary>Stateless subscription. Mutable state belongs only to each creature's power.</summary>
public sealed class ComboLifecycle : SingletonModel
{
    public override bool ShouldReceiveCombatHooks => true;
    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext context, Player player)
    {
        if (player.Character is not Ironclad || !player.Creature.IsAlive) return;
        var power = player.Creature.GetPower<ComboPower>()
            ?? await PowerCmd.Apply<ComboPower>(context, player.Creature, 1m, player.Creature, null, silent: true);
        power?.BeginTurn();
    }
}
