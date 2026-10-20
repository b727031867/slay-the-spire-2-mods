using Sts2Mod.Characters;
using Sts2Mod.Pools;
using System.IO;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using Sts2Mod.Cards;
using Sts2Mod.Combo;
using Sts2Mod.Relics;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private sealed record SaveExpectation(string[] Cards, string[] Relics, int Gold);
    private static readonly Type[] ThemeTypes = TravelerContent.RewardCardTypes.ToArray();
    private static bool IsTheme(CardModel card) => ThemeTypes.Contains(card.GetType());
    private string ExpectationPath => Path.Combine(TestSaveSandbox.Root, "expectation.json");
    private SaveExpectation Snapshot() => new(
        _player.Deck.Cards.Select(c => $"{c.Id.Entry}:{c.CurrentUpgradeLevel}").Order().ToArray(),
        _player.Relics.Select(r => r.Id.Entry).Order().ToArray(), _player.Gold);

    private async Task RunEndToEnd()
    {
        if (CommandLineHelper.HasArg("sts2mod-resume")) { await ResumeAndSoak(); return; }
        GD.Print("STS2MOD_E2E_START phase=prepare seed=TASK003A storage=native_disk");
        var run = await NGame.Instance!.StartNewSingleplayerRun(ModelDb.Character<Traveler>(), true,
            [ModelDb.Act<Overgrowth>(), ModelDb.Act<Hive>(), ModelDb.Act<Glory>()], [], "TASK003A", GameMode.Standard);
        _player = run.Players[0];
        Check(_player.Character is Traveler && _player.GetRelic<JourneyCompass>() is not null, "CHAR_E2E_traveler_and_compass_native_start");
        // Controlled durability and budget are fixture setup; acquisition itself uses unchanged native offers/prices.
        _player.Creature.SetMaxHpInternal(5000m); _player.Creature.SetCurrentHpInternal(5000m);
        _player.Gold = 10000;
        await EnterOrdinaryMapCombat();
        await AcquireRoomCard();
        await AcquireShopItems();

        // All base/upgraded models must survive a real disk save, including cards not rolled in the acquisition sample.
        foreach (var type in ThemeTypes)
        {
            foreach (var upgraded in new[] { false, true })
            {
                var card = run.CreateCard(ModelDb.AllCards.First(c => c.GetType() == type), _player);
                if (upgraded) CardCmd.Upgrade(card);
                await CardPileCmd.Add(card, PileType.Deck);
            }
        }
        Check(ThemeTypes.All(t => _player.Deck.Cards.Any(c => c.GetType() == t && c.CurrentUpgradeLevel == 0)
            && _player.Deck.Cards.Any(c => c.GetType() == t && c.CurrentUpgradeLevel == 1)), "TASK008_save_fixture_eighteen_base_and_upgrades");
        await EnterOrdinaryMapCombat();
        await SaveManager.Instance.SaveRun(null);
        Check(SaveManager.Instance.HasRunSave, "TASK003_native_disk_save_exists");
        System.IO.File.WriteAllText(ExpectationPath, JsonSerializer.Serialize(Snapshot()));
        await CardPileCmd.RemoveFromCombat(PileType.Hand.GetPile(_player).Cards.ToArray());
        await PlayerCmd.SetEnergy(3m, _player);
        await Play<WindBlade>();
        Check(Power.State.Last == Element.Wind, "TASK003_leave_unsaved_wind_before_exit");
        // Native combat-entry rollback is the supported boundary, not arbitrary mid-action continuation.
        GD.Print($"STS2MOD_E2E_PREPARED assertions={_assertions} cards=18 upgrades=18 paimon=1 shouldSave={RunManager.Instance.ShouldSave} boundary=combat_entry");
        await ExitThroughNativeMenu();
    }

    private async Task EnterOrdinaryMapCombat()
    {
        var state = RunManager.Instance.DebugOnlyGetState()!;
        var point = state.Map.GetAllMapPoints().Where(p => p.PointType == MapPointType.Monster && !state.VisitedMapCoords.Contains(p.coord))
            .OrderBy(p => p.coord.row).First();
        await RunManager.Instance.EnterMapCoord(point.coord);
        await Until(() => CombatManager.Instance.IsInProgress && ReadyToPlay && _player.Creature.GetPower<ComboPower>() is not null, "ordinary map combat");
        _target = _player.Creature.CombatState!.Enemies.First(e => e.IsAlive);
    }

    private async Task AcquireRoomCard()
    {
        for (var battle = 1; battle <= 60; battle++)
        {
            RewardProbe.Latest = null;
            await CreatureCmd.Kill(_player.Creature.CombatState!.Enemies.ToArray(), force: true);
            await CombatManager.Instance.CheckWinCondition();
            await Until(() => RewardProbe.Latest is not null && !CombatManager.Instance.IsInProgress, "unmodified native room rewards");
            var reward = RewardProbe.Latest!;
            var card = reward.Cards.FirstOrDefault(IsTheme);
            if (card is not null)
            {
                var before = _player.Deck.Cards.Count;
                var selection = RunManager.Instance.RewardsSetSynchronizer.SelectLocalReward(reward);
                NCardRewardSelectionScreen? screen = null;
                await Until(() => (screen = GetTree().Root.FindChildren("*", recursive: true, owned: false)
                    .OfType<NCardRewardSelectionScreen>().LastOrDefault(s => s.IsVisibleInTree())) is not null, "native card selection callback");
                // Native callback only, with --headless: no mouse/keyboard input or visible window.
                screen!.Call("SelectCard", screen.GetCardHolder(card));
                await Until(() => selection.IsCompleted, "native reward acquisition completion");
                Check(await selection && reward.SuccessfullySelected && _player.Deck.Cards.Count == before + 1
                    && _player.Deck.Cards.Any(c => ReferenceEquals(c, card)), "TASK003_native_room_reward_enters_deck");
                GD.Print($"STS2MOD_E2E_ACQUIRED source=room card={card.Id.Entry} battle={battle} offers=runtime_rules");
                RunManager.Instance.RewardsSetSynchronizer.SkipLocalRewardsSet();
                return;
            }
            RunManager.Instance.RewardsSetSynchronizer.SkipLocalRewardsSet();
            var encounter = ModelDb.Encounter<CultistsNormal>().ToMutable(); encounter.DebugRandomizeRng();
            await RunManager.Instance.EnterRoomDebug(RoomType.Monster, model: encounter, showTransition: false);
            await Until(() => CombatManager.Instance.IsInProgress && ReadyToPlay, "next reward sample combat");
        }
        throw new InvalidOperationException("No theme offer in 60 unmodified native room rewards.");
    }

    private async Task AcquireShopItems()
    {
        var cardBought = false;
        var relicBought = false;
        for (var shop = 1; shop <= 100; shop++)
        {
            var room = (MerchantRoom)await RunManager.Instance.EnterRoomDebug(RoomType.Shop, showTransition: false);
            var inventory = room.GetLocalInventory();
            var cardEntry = inventory.CharacterCardEntries.FirstOrDefault(e => e.CreationResult is { } c && IsTheme(c.Card));
            if (!cardBought && cardEntry is not null)
            {
                var card = cardEntry.CreationResult!.Card;
                var before = _player.Deck.Cards.Count;
                var gold = _player.Gold; var cost = cardEntry.Cost;
                Check(await cardEntry.OnTryPurchaseWrapper(inventory) && _player.Deck.Cards.Count == before + 1
                    && _player.Deck.Cards.Contains(card) && _player.Gold == gold - cost,
                    "TASK003_native_shop_card_purchase_cost_and_deck");
                // The native UI disables sold slots; invoking its purchase-failure
                // event on an empty slot is not a supported UI action.
                Check(!cardEntry.IsStocked,
                    "TASK003_sold_card_slot_not_stocked");
                cardBought = true;
                GD.Print($"STS2MOD_E2E_ACQUIRED source=shop card={card.Id.Entry} shop={shop} cost={cost} offers=runtime_rules");
            }
            var relicEntry = inventory.RelicEntries.FirstOrDefault(e => e.Model is PaimonCheer);
            if (!relicBought && relicEntry is not null)
            {
                var gold = _player.Gold; var cost = relicEntry.Cost;
                Check(await relicEntry.OnTryPurchaseWrapper(inventory) && _player.Relics.Count(r => r is PaimonCheer) == 1
                    && _player.Gold == gold - cost, "TASK003_native_shop_paimon_purchase");
                Check(!relicEntry.IsStocked,
                    "TASK003_sold_paimon_slot_not_stocked");
                relicBought = true;
                GD.Print($"STS2MOD_E2E_ACQUIRED source=shop relic=TC_R01 shop={shop} cost={cost} offers=runtime_rules");
            }
            if (cardBought && relicBought)
            {
                Check(!RunManager.Instance.DebugOnlyGetState()!.SharedRelicGrabBag.Contains(ModelDb.Relic<PaimonCheer>())
                    && !_player.RelicGrabBag.Contains(ModelDb.Relic<PaimonCheer>()), "TASK003_obtained_paimon_absent_from_both_bags");
                // Sample more native inventories after obtaining; no hand-edited pool or forced relic IDs.
                for (var sample = 0; sample < 3; sample++)
                {
                    var next = (MerchantRoom)await RunManager.Instance.EnterRoomDebug(RoomType.Shop, showTransition: false);
                    Check(next.GetLocalInventory().RelicEntries.All(e => e.Model is not PaimonCheer), "TASK003_paimon_not_offered_again_" + sample);
                }
                return;
            }
        }
        throw new InvalidOperationException("Could not acquire theme card and Paimon from 100 ordinary native shops.");
    }

    private async Task LoadSavedRun()
    {
        var read = SaveManager.Instance.LoadRunSave();
        Check(read.Success && read.SaveData is not null, "TASK003_native_saved_run_read");
        var save = read.SaveData!;
        var state = RunState.FromSerializable(save);
        await RunManager.Instance.SetUpSavedSingleplayer(state, save);
        await NGame.Instance!.LoadRun(state, save.PreFinishedRoom);
        _player = state.Players[0];
        await Until(() => CombatManager.Instance.IsInProgress && ReadyToPlay && _player.Creature.GetPower<ComboPower>() is not null, "reloaded native combat entry");
        _target = _player.Creature.CombatState!.Enemies.First(e => e.IsAlive);
        var expected = JsonSerializer.Deserialize<SaveExpectation>(System.IO.File.ReadAllText(ExpectationPath))!;
        var actual = Snapshot();
        Check(expected.Cards.SequenceEqual(actual.Cards), "TASK003_disk_reload_exact_deck_and_upgrades");
        Check(expected.Relics.SequenceEqual(actual.Relics) && actual.Gold == expected.Gold,
            "TASK003_disk_reload_exact_relics_and_gold");
        Check(_player.Relics.Count(r => r is PaimonCheer) == 1 && Power.State.Last == Element.None && Power.State.Used == 0,
            "TASK003_reload_no_duplicate_relic_or_stale_combo");
        Check(ThemeTypes.All(t => _player.Character.CardPool.GetUnlockedCards(_player.UnlockState, 0).Count(c => c.GetType() == t) == 1)
            && ModelDb.RelicPool<TravelerRelicPool>().GetUnlockedRelics(_player.UnlockState).Count(r => r is PaimonCheer) == 1,
            "TASK003_reload_no_duplicate_registration");
        Check(!state.SharedRelicGrabBag.Contains(ModelDb.Relic<PaimonCheer>()) && !_player.RelicGrabBag.Contains(ModelDb.Relic<PaimonCheer>()),
            "TASK003_reload_paimon_stays_removed_from_bags");
    }

    private async Task ResumeAndSoak()
    {
        GD.Print("STS2MOD_E2E_START phase=resume seed=TASK003A storage=native_disk");
        await LoadSavedRun();
        // Reload a second time through the native menu boundary; neither load can award anything.
        await SaveManager.Instance.SaveRun(null);
        await NGame.Instance!.ReturnToMainMenu();
        await LoadSavedRun();
        await CardPileCmd.RemoveFromCombat(PileType.Hand.GetPile(_player).Cards.ToArray());
        await PlayerCmd.SetEnergy(3m, _player);
        await Play<RockGuard>();
        Check(_player.Creature.Block == 7 && Power.State.Used == 0, "TASK003_first_rock_after_reload_has_no_old_wind_reward");
        Check(_player.Character is Traveler && _player.GetRelic<JourneyCompass>() is { SpentThisTurn: false }, "CHAR_E2E_character_and_unspent_compass_after_reload");
        await Play<WindBlade>();
        Check(_player.Creature.Block == 15 && Power.State.Used == 1, "TASK003_paimon_still_doubles_after_reload");
        Check(_player.GetRelic<JourneyCompass>() is { SpentThisTurn: true }, "CHAR_E2E_compass_rewards_after_disk_reload");
        await RunContinuousBattles();
        GD.Print($"STS2MOD_E2E_COMPLETE assertions={_assertions} acquisition=PASS disk_restart=PASS soak=PASS multiplayer=NOT_RUN ui=NOT_RUN");
        await ExitThroughNativeMenu();
    }

    private async Task ExitThroughNativeMenu()
    {
        await NGame.Instance!.ReturnToMainMenu();
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        NGame.Instance.Quit();
    }

    private static Element CardElement(CardModel c) => c switch
    {
        ElementCardModel elemental => elemental.CardElement,
        _ => Element.None
    };

    private async Task RunContinuousBattles()
    {
        using var selector = CardSelectCmd.PushSelector(new ExpansionCardSelector());
        var totalActions = 0; var totalCombos = 0; var totalTurns = 0; var naturalWins = 0;
        for (var battle = 1; battle <= 6; battle++)
        {
            if (CombatManager.Instance.IsInProgress)
            {
                await CreatureCmd.Kill(_player.Creature.CombatState!.Enemies.ToArray(), force: true);
                await CombatManager.Instance.CheckWinCondition();
                await Until(() => !CombatManager.Instance.IsInProgress, "soak previous combat complete");
            }
            var encounter = ModelDb.Encounter<CultistsNormal>().ToMutable(); encounter.DebugRandomizeRng();
            await RunManager.Instance.EnterRoomDebug(RoomType.Monster, model: encounter, showTransition: false);
            await Until(() => CombatManager.Instance.IsInProgress && ReadyToPlay, "soak combat start");
            Check(Power.State.Used == 0 && Power.State.Last == Element.None, "TASK003_soak_fresh_state_battle_" + battle);
            var oracle = new ComboState();
            var expectedEnergy = _player.MaxEnergy;
            for (var turn = 1; turn <= 4 && CombatManager.Instance.IsInProgress; turn++)
            {
                totalTurns++;
                Check(_player.PlayerCombatState!.Energy == expectedEnergy, "TASK003_soak_native_energy_b" + battle + "_t" + turn);
                var turnActions = 0;
                while (CombatManager.Instance.IsInProgress && turnActions < 30)
                {
                    var card = PileType.Hand.GetPile(_player).Cards.Where(c => c.CanPlay())
                        .OrderByDescending(c => CardElement(c) != Element.None && CardElement(c) != oracle.Last && oracle.Last != Element.None)
                        .ThenByDescending(IsTheme).FirstOrDefault();
                    if (card is null) break;
                    var target = card.TargetType == TargetType.AnyEnemy ? _player.Creature.CombatState!.Enemies.FirstOrDefault(e => e.IsAlive) : null;
                    var action = new PlayCardAction(card, target);
                    var expected = oracle.Reserve(action, CardElement(card));
                    var state = Power.State;
                    await Queue(action);
                    if (expected.Triggered) totalCombos++;
                    var stillFighting = CombatManager.Instance.IsInProgress && !CombatManager.Instance.IsEnding
                        && _player.Creature.CombatState?.Enemies.Any(e => e.IsAlive) == true;
                    if (!stillFighting) await Until(() => !CombatManager.Instance.IsInProgress, "soak natural combat cleanup");
                    Check(stillFighting ? state.Used == oracle.Used && state.Last == oracle.Last && state.Used <= 3
                        : state.Used == 0 && state.Last == Element.None,
                        "TASK003_soak_sequence_b" + battle + "_t" + turn + "_a" + turnActions);
                    Check(!stillFighting || (_player.PlayerCombatState!.Energy >= 0 && PileType.Hand.GetPile(_player).Cards.Count <= 10),
                        "TASK003_soak_energy_hand_bounds_b" + battle + "_t" + turn + "_a" + turnActions);
                    totalActions++; turnActions++;
                }
                Check(turnActions < 30, "TASK003_soak_no_unbounded_draw_loop_b" + battle + "_t" + turn);
                if (!CombatManager.Instance.IsInProgress) { naturalWins++; break; }
                if (turn == 4) break;
                var number = _player.PlayerCombatState!.TurnNumber;
                expectedEnergy = _player.MaxEnergy + (_player.Creature.GetPower<MegaCrit.Sts2.Core.Models.Powers.EnergyNextTurnPower>()?.Amount ?? 0);
                await Queue(new EndPlayerTurnAction(_player, number));
                await Until(() => !CombatManager.Instance.IsInProgress || (_player.PlayerCombatState!.TurnNumber > number && ReadyToPlay), "soak next native turn");
                oracle.BeginTurn();
                if (CombatManager.Instance.IsInProgress)
                    Check(Power.State.Used == 0 && Power.State.Last == oracle.Last, "TASK003_soak_turn_reset_b" + battle + "_t" + turn);
            }
        }
        Check(totalActions >= 30 && totalCombos >= 6 && totalTurns >= 12, "TASK003_soak_meaningful_sample");
        GD.Print($"STS2MOD_SOAK_COMPLETE seed=TASK003A battles=6 turns={totalTurns} actions={totalActions} combos={totalCombos} natural_wins={naturalWins} energy_injection=false hand_injection=false bounded_turns=4");
    }
}
