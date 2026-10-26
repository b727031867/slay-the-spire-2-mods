using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using Sts2Mod.Characters;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestSandworm()
    {
        foreach (var (character, count) in new (CharacterModel, int)[] {
            (ModelDb.Character<Ironclad>(), 1), (ModelDb.Character<Traveler>(), 1), (ModelDb.Character<Traveler>(), 2) })
        {
            var label = $"{character.Id.Entry}_{count}";
            GD.Print($"SANDWORM_STAGE case={label} stage=start network=singleplayer_fixture");
            RunState run;
            if (count == 1)
                run = await NGame.Instance!.StartNewSingleplayerRun(character, true,
                    [ModelDb.Act<Overgrowth>(), ModelDb.Act<Hive>(), ModelDb.Act<Glory>()], [], "SANDWORM26", GameMode.Standard, 10);
            else
            {
                // shortcut: local transport omits Steam synchronization; use both real clients when logs implicate networking.
                var lobby = new StartRunLobby(GameMode.Standard, new NetSingleplayerGameService(), null!, 2);
                for (var i = 0; i < count; i++)
                    lobby.Players.Add(new LobbyPlayer { id = (ulong)(i + 1), slotId = i, character = character,
                        unlockState = SaveManager.Instance.GenerateUnlockStateFromProgress().ToSerializable(), isReady = true });
                run = await NGame.Instance!.StartNewMultiplayerRun(lobby, true,
                    [ModelDb.Act<Overgrowth>(), ModelDb.Act<Hive>(), ModelDb.Act<Glory>()], [], "SANDWORM26", 10);
            }
            _player = run.Players[0];
            foreach (var player in run.Players)
            { player.Creature.SetMaxHpInternal(5000); player.Creature.SetCurrentHpInternal(5000); }
            await RunManager.Instance.EnterAct(1, doTransition: false);
            run.Act.SetBossEncounter(ModelDb.Encounter<TheInsatiableBoss>());
            GD.Print($"SANDWORM_STAGE case={label} stage=enter");
            await RunManager.Instance.EnterMapCoord(run.Map.BossMapPoint.coord);
            await Until(() => CombatManager.Instance.IsInProgress && ReadyToPlay, label + " boss entry");
            _target = _player.Creature.CombatState!.Enemies.Single();
            Check(_target.Monster is TheInsatiable, "SANDWORM_" + label + "_entry");
            var turn = _player.PlayerCombatState!.TurnNumber;
            GD.Print($"SANDWORM_STAGE case={label} stage=first_enemy_turn");
            foreach (var player in run.Players)
                await Queue(new EndPlayerTurnAction(player, player.PlayerCombatState!.TurnNumber));
            await Until(() => _player.PlayerCombatState!.TurnNumber > turn && ReadyToPlay, label + " first enemy turn");
            Check(_target.Powers.OfType<SandpitPower>().Count() == count, "SANDWORM_" + label + "_sandpit_per_player");
            GD.Print($"SANDWORM_STAGE case={label} stage=fire_potion_lethal");
            _target.SetCurrentHpInternal(1);
            var potion = await PotionCmd.TryToProcure<FirePotion>(_player);
            Check(potion.success, "SANDWORM_" + label + "_fire_potion_procured");
            await Queue(new UsePotionAction(potion.potion, _target, true));
            await Until(() => !CombatManager.Instance.IsInProgress && GetTree().Root.FindChildren("*", recursive: true, owned: false)
                .OfType<NRewardsScreen>().Any(s => s.IsVisibleInTree()), label + " potion lethal reward screen");
            Check(_target.IsDead && ((CombatRoom)run.CurrentRoom).IsPreFinished, "SANDWORM_" + label + "_potion_victory");
            await SaveManager.Instance.SaveRun(run.CurrentRoom);
            await NGame.Instance.ReturnToMainMenu();
            GD.Print($"SANDWORM_STAGE case={label} stage=load_saved_run");
            var read = SaveManager.Instance.LoadRunSave();
            Check(read.Success && read.SaveData is not null, "SANDWORM_" + label + "_disk_read");
            var save = read.SaveData!;
            var restored = RunState.FromSerializable(save);
            await RunManager.Instance.SetUpSavedSingleplayer(restored, save);
            await NGame.Instance.LoadRun(restored, save.PreFinishedRoom);
            _player = restored.Players[0];
            await Until(() => GetTree().Root.FindChildren("*", recursive: true, owned: false)
                .OfType<NRewardsScreen>().Any(s => s.IsVisibleInTree()), label + " saved victory rewards");
            Check(!CombatManager.Instance.IsInProgress && restored.CurrentRoom is CombatRoom { IsPreFinished: true }
                && restored.Players.Count == count, "SANDWORM_" + label + "_reload_victory");
            await NGame.Instance.ReturnToMainMenu();
        }
        GD.Print($"SANDWORM_COMPLETE assertions={_assertions} real_network=NOT_RUN rendering=NOT_RUN");
    }
}
