using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Sts2Mod.Characters;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestArchitectVictory()
    {
        Check(_player.Character is Traveler, "VICTORY_traveler_fixture");
        var dialogues = ModelDb.Event<TheArchitect>().DialogueSet;
        foreach (var wins in new[] { 0, 1, 2, 10, 100 })
        {
            var valid = dialogues.GetValidDialogues(ModelDb.Character<Traveler>().Id, wins, wins, false).ToArray();
            Check(valid.Length == 1 && valid[0].IsRepeating && valid[0].Lines.Count == 1
                && valid[0].Lines[0].LineText!.Exists() && valid[0].EndAttackers == ArchitectAttackers.Both,
                "VICTORY_dialogue_available_" + wins);
        }
        if (CombatManager.Instance.IsInProgress)
        {
            await CreatureCmd.Kill(_player.Creature.CombatState!.Enemies.ToArray(), force: true);
            await CombatManager.Instance.CheckWinCondition();
            await Until(() => !CombatManager.Instance.IsInProgress, "victory fixture combat ended");
        }
        var manager = RunManager.Instance;
        Check(!manager.ShouldSave, "VICTORY_isolated_unsaved_fixture");
        await manager.EnterAct(2, doTransition: false);
        await manager.EnterNextAct();
        var room = (EventRoom)_player.RunState.CurrentRoom!;
        var architect = (TheArchitect)room.LocalMutableEvent;
        await Until(() => architect.CurrentOptions.Count == 1, "Architect dialogue proceed option");
        Check(room.IsVictoryRoom && architect.Owner == _player, "VICTORY_native_architect_room");
        await architect.CurrentOptions.Single().Chosen();
        await Until(() => GetTree().Root.FindChildren("*", recursive: true, owned: false)
            .OfType<NGameOverScreen>().Any(screen => screen.IsVisibleInTree()), "native victory summary", 60);
        Check((bool)AccessTools.Field(typeof(RunManager), "_runHistoryWasUploaded").GetValue(manager)!
            && _player.RunState.CurrentRoom!.IsVictoryRoom, "VICTORY_native_win_and_summary");
        GD.Print("STS2MOD_VICTORY_COMPLETE dialogue=PASS animations=PASS summary=PASS shouldSave=False rendering=NOT_RUN multiplayer=NOT_RUN");
    }
}
