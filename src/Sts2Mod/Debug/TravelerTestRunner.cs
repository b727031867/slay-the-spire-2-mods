using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Sts2Mod.Cards;
using Sts2Mod.Combo;

namespace Sts2Mod.Debug;

/// <summary>Explicit launch-only, unsaved singleplayer fixture. Never instantiated in normal play.</summary>
public partial class TravelerTestRunner : Node
{
    private int _assertions;
    private Player _player = null!;
    private Creature _target = null!;
    private ComboPower Power => _player.Creature.GetPower<ComboPower>()!;

    public override async void _Ready()
    {
        try { await Run(); }
        catch (Exception ex) { GD.PrintErr($"STS2MOD_TEST_FAIL {ex}"); }
    }
    private async Task Until(Func<bool> predicate, string condition, int timeoutSeconds = 45)
    {
        var deadline = System.Diagnostics.Stopwatch.StartNew();
        while (!predicate())
        {
            if (deadline.Elapsed > TimeSpan.FromSeconds(timeoutSeconds)) throw new TimeoutException(condition);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }
    private void Check(bool condition, string name)
    {
        if (!condition) throw new InvalidOperationException(name);
        _assertions++;
        GD.Print($"STS2MOD_TEST_PASS {name}");
    }
    private async Task Queue(GameAction action)
    {
        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(action);
        await Until(() => action.CompletionTask.IsCompleted, "native action completion");
        if (action.Exception is { } ex) throw ex;
    }
    private async Task<CardModel> Add<T>(bool upgraded = false) where T : CardModel
    {
        var card = CombatManager.Instance.DebugOnlyGetState()!.CreateCard(ModelDb.Card<T>(), _player);
        if (upgraded) CardCmd.Upgrade(card);
        await CardPileCmd.Add(card, PileType.Hand);
        return card;
    }
    private async Task<PlayCardAction> Play<T>(bool upgraded = false) where T : CardModel
    {
        var card = await Add<T>(upgraded);
        var action = new PlayCardAction(card, card.TargetType == TargetType.AnyEnemy ? _target : null);
        await Queue(action);
        return action;
    }
    private async Task NextTurn()
    {
        var turn = _player.PlayerCombatState!.TurnNumber;
        await Queue(new EndPlayerTurnAction(_player, turn));
        await Until(() => _player.PlayerCombatState.TurnNumber > turn && ReadyToPlay, "next native turn");
        _player.Creature.HealInternal(999m);
        await CardPileCmd.RemoveFromCombat(PileType.Hand.GetPile(_player).Cards.ToArray());
        await PlayerCmd.SetEnergy(20m, _player);
        Check(Power.State.Last == Element.None && Power.State.Used == 0, "native_turn_reset");
    }
    private static bool ReadyToPlay => RunManager.Instance.ActionQueueSynchronizer.CombatState == ActionSynchronizerCombatState.PlayPhase;

    private async Task Run()
    {
        await Until(() => NGame.Instance?.MainMenu is not null, "main menu");
        GD.Print("STS2MOD_TEST_START fixture=singleplayer shouldSave=false seed=TC001A");
        var run = await NGame.Instance!.StartNewSingleplayerRun(ModelDb.Character<Ironclad>(), false,
            [ModelDb.Act<Overgrowth>(), ModelDb.Act<Hive>(), ModelDb.Act<Glory>()], [], "TC001A", GameMode.Standard);
        _player = run.Players[0];
        var encounter = ModelDb.Encounter<CultistsNormal>().ToMutable();
        encounter.DebugRandomizeRng();
        await RunManager.Instance.EnterRoomDebug(RoomType.Monster, model: encounter);
        await Until(() => CombatManager.Instance.IsInProgress && ReadyToPlay && _player.Creature.GetPower<ComboPower>() is not null, "first player turn");
        _target = _player.Creature.CombatState!.Enemies.First();
        if (CommandLineHelper.HasArg("sts2mod-rewards")) { await ScanNormalRewards(); return; }
        // Fixed high health only in the unsaved fixture, so all cases share a live target.
        _target.SetMaxHpInternal(2000m); _target.SetCurrentHpInternal(2000m);
        await CardPileCmd.RemoveFromCombat(PileType.Hand.GetPile(_player).Cards.ToArray());
        await PlayerCmd.SetEnergy(20m, _player);
        var pool = ModelDb.CardPool<MegaCrit.Sts2.Core.Models.CardPools.IroncladCardPool>().GetUnlockedCards(_player.UnlockState, 0).ToArray();
        Check(pool.Count(c => c is WindBlade) == 1 && pool.Count(c => c is RockGuard) == 1, "two_cards_once_in_native_unlocked_pool");
        Check(!ModelDb.CardPool<MegaCrit.Sts2.Core.Models.CardPools.SilentCardPool>().GetUnlockedCards(_player.UnlockState, 0).Any(c => c is WindBlade or RockGuard), "theme_cards_absent_from_silent_pool");
        Check(!_player.Deck.Cards.Any(c => c is WindBlade or RockGuard), "normal_starting_deck_unchanged");

        var hp = _target.CurrentHp;
        await Play<WindBlade>(); await Play<RockGuard>();
        Check(hp - _target.CurrentHp == 6 && _player.Creature.Block == 11 && Power.State.Used == 1 && Power.State.Last == Element.None, "TC01_damage6_block11_consumed");
        await NextTurn();
        await Play<RockGuard>(); await Play<WindBlade>();
        Check(_player.Creature.Block == 7 && Power.State.Used == 0 && Power.State.Last == Element.Wind, "TC04_reverse_no_combo");
        await NextTurn();
        await Play<WindBlade>(); await Play<DefendIronclad>(); await Play<RockGuard>();
        Check(_player.Creature.Block == 16 && Power.State.Used == 1, "TC05_normal_defend_preserves_sequence");
        await NextTurn();
        await Play<WindBlade>(); await Play<WindBlade>(); await Play<RockGuard>();
        Check(_player.Creature.Block == 11 && Power.State.Used == 1, "TC06_same_element_then_one_combo");
        await NextTurn();
        for (var i = 0; i < 3; i++) { await Play<WindBlade>(); await Play<RockGuard>(); }
        Check(_player.Creature.Block == 29 && Power.State.Used == 2 && Power.State.Last == Element.None && Power.DisplayAmount == 0, "TC08_three_pairs_only_two_bonuses");
        await NextTurn();
        await Play<WindBlade>(); await NextTurn(); await Play<RockGuard>();
        Check(_player.Creature.Block == 7 && Power.State.Used == 0, "TC09_wind_does_not_cross_turn");
        await NextTurn();
        var source = await Play<WindBlade>();
        var automatic = await Add<RockGuard>();
        await CardCmd.AutoPlay(source.PlayerChoiceContext!, automatic, null);
        Check(Power.State.Last == Element.Wind && Power.State.Used == 0 && _player.Creature.Block == 7, "TC10_native_autoplay_preserves_wind");
        await Play<RockGuard>();
        Check(Power.State.Used == 1 && _player.Creature.Block == 18, "TC10_manual_rock_after_autoplay_triggers");
        await NextTurn();
        source = await Play<WindBlade>();
        await PowerCmd.Apply<BurstPower>(source.PlayerChoiceContext!, _player.Creature, 1m, _player.Creature, null);
        await Play<RockGuard>();
        Check(_player.Creature.Block == 18 && Power.State.Used == 1 && Power.State.Last == Element.None, "TC11_native_burst_extra_resolution_no_second_advance");
        await NextTurn();
        hp = _target.CurrentHp;
        await Play<WindBlade>(true); await Play<RockGuard>(true);
        Check(hp - _target.CurrentHp == 9 && _player.Creature.Block == 14 && Power.State.Used == 1, "TC13_upgraded_damage9_block14");
        foreach (var canonical in new CardModel[] { ModelDb.Card<WindBlade>(), ModelDb.Card<RockGuard>() })
        {
            var original = canonical.ToMutable();
            original.UpgradeInternal();
            original.FinalizeUpgradeInternal();
            var restored = CardModel.FromSerializable(original.ToSerializable());
            Check(restored.GetType() == original.GetType() && restored.Id == original.Id && restored.CurrentUpgradeLevel == 1
                && (restored is WindBlade ? restored.DynamicVars.Damage.BaseValue == 9m : restored.DynamicVars.Block.BaseValue == 10m), "native_card_serialization_roundtrip_" + original.Id.Entry);
        }
        await NextTurn();
        await Play<WindBlade>(); await PlayerCmd.SetEnergy(0m, _player);
        var unplayable = await Add<RockGuard>();
        await Queue(new PlayCardAction(unplayable, null));
        Check(Power.State.Last == Element.Wind && Power.State.Used == 0 && _player.Creature.Block == 0 && unplayable.Pile?.Type == PileType.Hand, "TC15_insufficient_energy_does_not_advance");

        await NextTurn();
        await Play<WindBlade>();
        var oldPower = Power;
        await CreatureCmd.Kill(_player.Creature.CombatState!.Enemies.ToArray(), force: true);
        await CombatManager.Instance.CheckWinCondition();
        await Until(() => !CombatManager.Instance.IsInProgress, "combat end");
        encounter = ModelDb.Encounter<CultistsNormal>().ToMutable();
        encounter.DebugRandomizeRng();
        await RunManager.Instance.EnterRoomDebug(RoomType.Monster, model: encounter);
        await Until(() => CombatManager.Instance.IsInProgress && ReadyToPlay && _player.Creature.GetPower<ComboPower>() is not null, "new combat");
        Check(Power != oldPower && Power.State.Last == Element.None && Power.State.Used == 0, "TC18_new_combat_has_fresh_state");
        await TestLightning();
        await TravelerHandCommand.Prepare(_player, 1, false, lightning: true);
        GD.Print($"STS2MOD_TEST_COMPLETE assertions={_assertions} multiplayer=NOT_RUN ui=READY_FOR_MANUAL_CHECK shouldSave={RunManager.Instance.ShouldSave}");
    }

    private async Task TestLightning()
    {
        _target = _player.Creature.CombatState!.Enemies.First();
        _target.SetMaxHpInternal(2000m); _target.SetCurrentHpInternal(2000m);
        await CardPileCmd.RemoveFromCombat(PileType.Hand.GetPile(_player).Cards.ToArray());
        await PlayerCmd.SetEnergy(20m, _player);
        var pool = ModelDb.CardPool<MegaCrit.Sts2.Core.Models.CardPools.IroncladCardPool>().GetUnlockedCards(_player.UnlockState, 0);
        Check(pool.Count(c => c is LightningThrust) == 1, "TC002_lightning_once_in_native_pool");
        var hp = _target.CurrentHp;
        await Play<RockGuard>(); await Play<LightningThrust>();
        Check(hp - _target.CurrentHp == 11 && _player.Creature.Block == 7 && Power.State.Used == 1 && Power.State.Last == Element.None, "TC02_rock_lightning_damage7_plus4");
        await NextTurn(); hp = _target.CurrentHp;
        await Play<RockGuard>(); await Play<LightningThrust>(true);
        Check(hp - _target.CurrentHp == 14 && Power.State.Used == 1, "TC13_lightning_upgrade10_plus4");
        await NextTurn(); hp = _target.CurrentHp;
        await Play<WindBlade>(); await Play<RockGuard>();
        await Play<RockGuard>(); await Play<LightningThrust>();
        await Play<RockGuard>(); await Play<LightningThrust>();
        Check(hp - _target.CurrentHp == 24 && _player.Creature.Block == 25 && Power.State.Used == 2 && Power.State.Last == Element.None, "TC08_mixed_combos_share_cap");
        await NextTurn(); hp = _target.CurrentHp;
        var rock = await Play<RockGuard>();
        var auto = await Add<LightningThrust>();
        await CardCmd.AutoPlay(rock.PlayerChoiceContext!, auto, _target);
        Check(hp - _target.CurrentHp == 7 && Power.State.Last == Element.Rock && Power.State.Used == 0, "TC10_auto_lightning_preserves_rock");
        await Play<LightningThrust>();
        Check(hp - _target.CurrentHp == 18 && Power.State.Used == 1, "TC10_manual_lightning_after_auto_triggers");
        await NextTurn(); hp = _target.CurrentHp;
        rock = await Play<RockGuard>();
        await PowerCmd.Apply<DuplicationPower>(rock.PlayerChoiceContext!, _player.Creature, 1m, _player.Creature, null);
        await Play<LightningThrust>();
        Check(hp - _target.CurrentHp == 18 && Power.State.Used == 1 && Power.State.Last == Element.None, "TC11_native_duplicated_lightning_one_bonus");
        await NextTurn(); hp = _target.CurrentHp;
        rock = await Play<RockGuard>();
        await PowerCmd.Apply<StrengthPower>(rock.PlayerChoiceContext!, _player.Creature, 2m, _player.Creature, null);
        await PowerCmd.Apply<WeakPower>(rock.PlayerChoiceContext!, _player.Creature, 2m, _target, null);
        await PowerCmd.Apply<VulnerablePower>(rock.PlayerChoiceContext!, _target, 2m, _player.Creature, null);
        await CreatureCmd.GainBlock(_target, 3m, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, null);
        await Play<LightningThrust>();
        Check(hp - _target.CurrentHp == 13 && _target.Block == 0, "TC24_strength_weak_vulnerable_block_native_formula");
        await PowerCmd.Remove<StrengthPower>(_player.Creature);
        await PowerCmd.Remove<WeakPower>(_player.Creature);
        await PowerCmd.Remove<VulnerablePower>(_target);
        await NextTurn();
        var other = _player.Creature.CombatState!.Enemies.First(e => e != _target);
        var otherHp = other.CurrentHp;
        _target.SetCurrentHpInternal(7m);
        await Play<RockGuard>(); await Play<LightningThrust>();
        Check(_target.IsDead && other.CurrentHp == otherHp && Power.State.Used == 1 && Power.State.Last == Element.None, "TC14_dead_target_no_retarget_counter_consumed");
        var original = ModelDb.Card<LightningThrust>().ToMutable();
        original.UpgradeInternal(); original.FinalizeUpgradeInternal();
        var restored = CardModel.FromSerializable(original.ToSerializable());
        Check(restored is LightningThrust && restored.CurrentUpgradeLevel == 1 && restored.DynamicVars.Damage.BaseValue == 10m, "native_card_serialization_roundtrip_TC_C03");
        GD.Print("STS2MOD_LIGHTNING_COMPLETE multiplayer=NOT_RUN normal_acquisition=NOT_RUN");
    }

    private async Task ScanNormalRewards()
    {
        for (var battle = 1; battle <= 60; battle++)
        {
            RewardProbe.Latest = null;
            await CreatureCmd.Kill(_player.Creature.CombatState!.Enemies.ToArray(), force: true);
            await CombatManager.Instance.CheckWinCondition();
            await Until(() => RewardProbe.Latest is not null
                && GetTree().Root.FindChildren("*", recursive: true, owned: false).OfType<NRewardsScreen>().Any(s => s.IsVisibleInTree()), "native room reward screen");
            var reward = RewardProbe.Latest!;
            var theme = reward.Cards.FirstOrDefault(c => c is WindBlade or RockGuard);
            if (theme is not null)
            {
                GD.Print($"STS2MOD_ACQUISITION_READY battle={battle} card={theme.Id.Entry} source=unmodified_native_room_reward shouldSave={RunManager.Instance.ShouldSave}");
                await Until(() => _player.Deck.Cards.Any(c => c.Id == theme.Id), "manually choose the theme card from the reward screen", 600);
                Check(true, "TC21_native_room_reward_selected_into_deck");
                GD.Print("STS2MOD_ACQUISITION_COMPLETE shouldSave=false");
                return;
            }
            RunManager.Instance.RewardsSetSynchronizer.SkipLocalRewardsSet();
            var encounter = ModelDb.Encounter<CultistsNormal>().ToMutable();
            encounter.DebugRandomizeRng();
            await RunManager.Instance.EnterRoomDebug(RoomType.Monster, model: encounter);
            await Until(() => CombatManager.Instance.IsInProgress && ReadyToPlay, "next reward test combat");
        }
        throw new InvalidOperationException("No theme card offered in 60 unmodified native rewards.");
    }
}
