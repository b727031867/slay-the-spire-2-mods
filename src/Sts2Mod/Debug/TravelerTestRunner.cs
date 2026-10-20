using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Powers.Mocks;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Sts2Mod.Cards;
using Sts2Mod.Combo;
using Sts2Mod.Relics;

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
        _player.Creature.HealInternal(9999m);
        var turn = _player.PlayerCombatState!.TurnNumber;
        await Queue(new EndPlayerTurnAction(_player, turn));
        await Until(() => _player.PlayerCombatState.TurnNumber > turn && ReadyToPlay, "next native turn");
        _player.Creature.HealInternal(999m);
        await CardPileCmd.RemoveFromCombat(PileType.Hand.GetPile(_player).Cards.ToArray());
        await PlayerCmd.SetEnergy(20m, _player);
        Check(Power.State.Last == Element.None && Power.State.Used == 0, "native_turn_reset");
    }
    private static bool ReadyToPlay => RunManager.Instance.ActionQueueSynchronizer.CombatState == ActionSynchronizerCombatState.PlayPhase;

    private async Task StartFreshCombat()
    {
        await CreatureCmd.Kill(_player.Creature.CombatState!.Enemies.ToArray(), force: true);
        await CombatManager.Instance.CheckWinCondition();
        await Until(() => !CombatManager.Instance.IsInProgress, "combat end before fresh fixture");
        var encounter = ModelDb.Encounter<CultistsNormal>().ToMutable();
        encounter.DebugRandomizeRng();
        await RunManager.Instance.EnterRoomDebug(RoomType.Monster, model: encounter);
        await Until(() => CombatManager.Instance.IsInProgress && ReadyToPlay && _player.Creature.GetPower<ComboPower>() is not null, "fresh combat");
        _target = _player.Creature.CombatState!.Enemies.First();
        _target.SetMaxHpInternal(3000m);
        _target.SetCurrentHpInternal(3000m);
        await CardPileCmd.RemoveFromCombat(PileType.Hand.GetPile(_player).Cards.ToArray());
        await PlayerCmd.SetEnergy(30m, _player);
    }

    private async Task SeedDraw(int count = 12)
    {
        for (var i = 0; i < count; i++)
        {
            var filler = CombatManager.Instance.DebugOnlyGetState()!.CreateCard(ModelDb.Card<DefendIronclad>(), _player);
            await CardPileCmd.Add(filler, PileType.Draw);
        }
    }

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
        // Fixed high health only in the unsaved fixture, so long-running turn-boundary cases remain deterministic.
        _player.Creature.SetMaxHpInternal(5000m); _player.Creature.SetCurrentHpInternal(5000m);
        _target.SetMaxHpInternal(2000m); _target.SetCurrentHpInternal(2000m);
        await CardPileCmd.RemoveFromCombat(PileType.Hand.GetPile(_player).Cards.ToArray());
        await PlayerCmd.SetEnergy(20m, _player);
        if (CommandLineHelper.HasArg("sts2mod-acceptance"))
        {
            await RelicCmd.Obtain<PaimonCheer>(_player);
            await TestTask003Acceptance();
            if (!CombatManager.Instance.IsInProgress) await StartFreshCombat();
            await TravelerHandCommand.Prepare(_player, 1, false, full: true, paimon: true);
            GD.Print($"STS2MOD_TEST_COMPLETE assertions={_assertions} multiplayer=NOT_RUN ui=READY_FOR_MANUAL_CHECK shouldSave={RunManager.Instance.ShouldSave}");
            return;
        }
        var pool = ModelDb.CardPool<MegaCrit.Sts2.Core.Models.CardPools.IroncladCardPool>().GetUnlockedCards(_player.UnlockState, 0).ToArray();
        Check(pool.Count(c => c is WindBlade) == 1 && pool.Count(c => c is RockGuard) == 1, "two_cards_once_in_native_unlocked_pool");
        Check(!ModelDb.CardPool<MegaCrit.Sts2.Core.Models.CardPools.SilentCardPool>().GetUnlockedCards(_player.UnlockState, 0).Any(c => c is WindBlade or RockGuard), "theme_cards_absent_from_silent_pool");
        Check(!_player.Deck.Cards.Any(c => c is WindBlade or RockGuard), "normal_starting_deck_unchanged");

        var hp = _target.CurrentHp;
        await Play<WindBlade>(); await Play<RockGuard>();
        Check(hp - _target.CurrentHp == 6 && _player.Creature.Block == 11 && Power.State.Used == 1 && Power.State.Last == Element.None, "TC01_damage6_block11_consumed");
        await NextTurn();
        await Play<RockGuard>(); await Play<WindBlade>();
        Check(_player.Creature.Block == 11 && Power.State.Used == 1 && Power.State.Last == Element.None, "TC04_reverse_wind_rock_combo");
        await NextTurn();
        await Play<WindBlade>(); await Play<DefendIronclad>(); await Play<RockGuard>();
        Check(_player.Creature.Block == 16 && Power.State.Used == 1, "TC05_normal_defend_preserves_sequence");
        await NextTurn();
        await Play<WindBlade>(); await Play<WindBlade>(); await Play<RockGuard>();
        Check(_player.Creature.Block == 11 && Power.State.Used == 1, "TC06_same_element_then_one_combo");
        await NextTurn();
        for (var i = 0; i < 4; i++) { await Play<WindBlade>(); await Play<RockGuard>(); }
        Check(_player.Creature.Block == 40 && Power.State.Used == 3 && Power.State.Last == Element.None && Power.DisplayAmount == 0, "TC08_four_pairs_only_three_bonuses");
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
        await StartFreshCombat();
        await TestFullContent();
        await TestTask003Acceptance();
        if (!CombatManager.Instance.IsInProgress) await StartFreshCombat();
        await TravelerHandCommand.Prepare(_player, 1, false, full: true, paimon: true);
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
        Check(hp - _target.CurrentHp == 28 && _player.Creature.Block == 25 && Power.State.Used == 3 && Power.State.Last == Element.None, "TC08_mixed_combos_share_three_cap");
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

    private async Task TestFullContent()
    {
        var themeTypes = new HashSet<Type>
        {
            typeof(WindBlade), typeof(RockGuard), typeof(LightningThrust),
            typeof(TailwindPrep), typeof(RockAdvance), typeof(ThunderPrep)
        };
        var pool = ModelDb.CardPool<MegaCrit.Sts2.Core.Models.CardPools.IroncladCardPool>().GetUnlockedCards(_player.UnlockState, 0).ToArray();
        Check(themeTypes.All(t => pool.Count(c => c.GetType() == t) == 1), "TASK002_six_cards_once_in_ironclad_pool");
        Check(!ModelDb.CardPool<MegaCrit.Sts2.Core.Models.CardPools.SilentCardPool>().GetUnlockedCards(_player.UnlockState, 0)
            .Any(c => themeTypes.Contains(c.GetType())), "TASK002_theme_cards_absent_from_silent_pool");
        Check(!_player.Deck.Cards.Any(c => themeTypes.Contains(c.GetType())), "TASK002_normal_starting_deck_unchanged");

        var relicPool = ModelDb.RelicPool<SharedRelicPool>().GetUnlockedRelics(_player.UnlockState).ToArray();
        Check(relicPool.Count(r => r is PaimonCheer) == 1 && ModelDb.Relic<PaimonCheer>().Rarity == RelicRarity.Uncommon,
            "TASK002_paimon_once_in_shared_uncommon_relic_pool");
        Check(!ModelDb.Relic<PaimonCheer>().IsStackable && !ModelDb.RelicPool<IroncladRelicPool>().GetUnlockedRelics(_player.UnlockState).Any(r => r is PaimonCheer),
            "TASK002_paimon_nonstackable_and_absent_from_ironclad_pool");

        await SeedDraw();
        await Play<TailwindPrep>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 1
            && PileType.Exhaust.GetPile(_player).Cards.Any(c => c is TailwindPrep)
            && Power.State.Last == Element.Wind && Power.State.Used == 0,
            "TC23_tailwind_draw1_exhaust_wind");
        await NextTurn(); await SeedDraw();
        await Play<TailwindPrep>(true);
        Check(PileType.Hand.GetPile(_player).Cards.Count == 2, "TC13_tailwind_upgrade_draw2");

        await NextTurn(); await SeedDraw();
        await Play<LightningThrust>();
        await Play<TailwindPrep>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 2 && Power.State.Used == 1 && Power.State.Last == Element.None,
            "TC03_lightning_wind_base_draw1_bonus_draw1");

        await NextTurn(); await SeedDraw();
        var lightning = await Play<LightningThrust>();
        await PowerCmd.Apply<BurstPower>(lightning.PlayerChoiceContext!, _player.Creature, 1m, _player.Creature, null);
        await Play<TailwindPrep>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 3 && Power.State.Used == 1 && Power.State.Last == Element.None,
            "TC11_burst_tailwind_two_base_resolutions_one_combo_draw");

        await NextTurn();
        var hp = _target.CurrentHp;
        await Play<RockAdvance>();
        Check(hp - _target.CurrentHp == 5 && _player.Creature.Block == 3 && Power.State.Last == Element.Rock,
            "TC13_rock_advance_damage5_then_block3");
        await NextTurn(); hp = _target.CurrentHp;
        await Play<RockAdvance>(true);
        Check(hp - _target.CurrentHp == 7 && _player.Creature.Block == 5, "TC13_rock_advance_upgrade_damage7_block5");

        await NextTurn(); await SeedDraw();
        await Play<ThunderPrep>();
        Check(_player.Creature.Block == 5 && PileType.Hand.GetPile(_player).Cards.Count == 1 && Power.State.Last == Element.Lightning,
            "TC13_thunder_prep_block5_then_draw1");
        await NextTurn(); await SeedDraw();
        await Play<ThunderPrep>(true);
        Check(_player.Creature.Block == 8 && PileType.Hand.GetPile(_player).Cards.Count == 1,
            "TC13_thunder_prep_upgrade_block8_draw1");

        await NextTurn(); await SeedDraw(); hp = _target.CurrentHp;
        await Play<RockAdvance>(); await Play<ThunderPrep>();
        Check(hp - _target.CurrentHp == 5 && _player.Creature.Block == 8 && Power.State.Used == 1 && Power.State.Last == Element.None,
            "rock_to_targetless_thunder_consumes_combo_without_retarget");
        await NextTurn(); await SeedDraw();
        await Play<ThunderPrep>(); await Play<TailwindPrep>();
        Check(_player.Creature.Block == 5 && PileType.Hand.GetPile(_player).Cards.Count == 3 && Power.State.Used == 1,
            "new_cards_lightning_wind_draw_order");

        await NextTurn(); await SeedDraw(); hp = _target.CurrentHp;
        await Play<WindBlade>(); await Play<LightningThrust>();
        Check(hp - _target.CurrentHp == 13 && PileType.Hand.GetPile(_player).Cards.Count == 1 && Power.State.Used == 1,
            "reverse_wind_lightning_draws_bonus");
        await NextTurn(); hp = _target.CurrentHp;
        await Play<LightningThrust>(); await Play<RockAdvance>();
        Check(hp - _target.CurrentHp == 16 && _player.Creature.Block == 3 && Power.State.Used == 1,
            "reverse_lightning_rock_attacks_same_target");

        await RelicCmd.Obtain<PaimonCheer>(_player);
        await NextTurn();
        await Play<WindBlade>(); await Play<RockGuard>();
        Check(_player.Creature.Block == 15 && Power.State.Used == 1,
            "TC12_paimon_doubles_wind_rock_block");
        hp = _target.CurrentHp;
        await Play<RockGuard>(); await Play<LightningThrust>();
        Check(_player.Creature.Block == 22 && hp - _target.CurrentHp == 15 && Power.State.Used == 2,
            "TC12_paimon_doubles_rock_lightning_attack");
        await SeedDraw();
        await Play<LightningThrust>(); await Play<TailwindPrep>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 3 && Power.State.Used == 3,
            "TC12_paimon_doubles_lightning_wind_draw");
        await NextTurn();
        await Play<WindBlade>(); await Play<RockGuard>();
        Check(_player.Creature.Block == 15 && Power.State.Used == 1 && _player.Relics.Count(r => r is PaimonCheer) == 1,
            "paimon_doubles_every_successful_combo_without_duplicate_copy");

        var originalRelic = ModelDb.Relic<PaimonCheer>().ToMutable();
        var restoredRelic = RelicModel.FromSerializable(originalRelic.ToSerializable());
        Check(restoredRelic is PaimonCheer && restoredRelic.Id == originalRelic.Id,
            "native_relic_serialization_roundtrip_TC_R01");
        foreach (var canonical in new CardModel[] { ModelDb.Card<TailwindPrep>(), ModelDb.Card<RockAdvance>(), ModelDb.Card<ThunderPrep>() })
        {
            var original = canonical.ToMutable();
            original.UpgradeInternal(); original.FinalizeUpgradeInternal();
            var restored = CardModel.FromSerializable(original.ToSerializable());
            Check(restored.GetType() == original.GetType() && restored.Id == original.Id && restored.CurrentUpgradeLevel == 1,
                "native_card_serialization_roundtrip_" + original.Id.Entry);
        }
        GD.Print($"STS2MOD_TASK002_COMPLETE assertions={_assertions} cards=6 relics=1 multiplayer=NOT_RUN acquisition=POOL_VERIFIED");
    }

    private async Task TestTask003Acceptance()
    {
        await NextTurn(); await SeedDraw();
        var lightning = await Play<LightningThrust>();
        var automaticWind = await Add<TailwindPrep>();
        await CardCmd.AutoPlay(lightning.PlayerChoiceContext!, automaticWind, null);
        Check(PileType.Hand.GetPile(_player).Cards.Count == 1 && Power.State.Last == Element.Lightning && Power.State.Used == 0,
            "TASK003_auto_tailwind_draws_base_only_and_preserves_lightning");
        await Play<TailwindPrep>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 4 && Power.State.Used == 1 && Power.State.Last == Element.None,
            "TASK003_manual_tailwind_after_auto_triggers_paimon_double_draw");

        await NextTurn();
        await Play<WindBlade>(); await Play<RockGuard>();
        Check(_player.Creature.Block == 15, "TASK003_paimon_doubles_before_extra_turn");
        var source = await Play<WindBlade>();
        await PowerCmd.Apply<MockExtraTurnPower>(source.PlayerChoiceContext!, _player.Creature, 1m, _player.Creature, null);
        await NextTurn();
        Check(Power.State.Used == 0 && Power.State.Last == Element.None
            && _player.Creature.GetPower<MockExtraTurnPower>() is null, "TASK003_native_extra_turn_resets_combo_and_paimon");
        await Play<WindBlade>(); await Play<RockGuard>();
        Check(_player.Creature.Block == 15, "TASK003_paimon_doubles_on_extra_turn");

        await NextTurn();
        var enemies = _player.Creature.CombatState!.Enemies.Where(e => e.IsAlive).ToArray();
        _target = enemies[0];
        _target.SetCurrentHpInternal(7m);
        var survivor = enemies.First(e => e != _target);
        survivor.SetMaxHpInternal(3000m); survivor.SetCurrentHpInternal(3000m);
        await Play<RockGuard>(); await Play<LightningThrust>();
        Check(_target.IsDead && survivor.IsAlive && _player.Creature.Block == 7 && Power.State.Used == 1,
            "TASK003_dead_rock_lightning_target_does_not_retarget_or_create_bonus");

        await NextTurn();
        _target = survivor;
        _target.SetCurrentHpInternal(7m);
        var finalTurnState = Power.State;
        await Play<RockGuard>();
        var blockBeforeKill = _player.Creature.Block;
        var peakBlock = blockBeforeKill;
        void TrackBlock(int before, int after) => peakBlock = Math.Max(peakBlock, after);
        _player.Creature.BlockChanged += TrackBlock;
        PlayCardAction finalAction;
        try { finalAction = await Play<LightningThrust>(); }
        finally { _player.Creature.BlockChanged -= TrackBlock; }
        // Native Player.AfterCombatEnd clears block. Observe every change during
        // the killing action so cleanup timing cannot hide an unwanted award.
        GD.Print($"STS2MOD_TEST_DETAIL case=final_base_kill player={_player.NetId} action={finalAction.Id} block_before={blockBeforeKill} block_peak={peakBlock} block_after={_player.Creature.Block}");
        Check(_target.IsDead && finalTurnState.Used == 1 && blockBeforeKill == 7 && peakBlock == 7,
            "TASK003_base_kill_ending_combat_skips_combo_reward");
        await Until(() => !CombatManager.Instance.IsInProgress, "TASK003 combat ending after final base kill");
        GD.Print($"STS2MOD_TASK003_AUTOMATION_COMPLETE assertions={_assertions} singleplayer=PASS multiplayer=NOT_RUN ui=READY_FOR_USER_PLAYTEST");
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
            var theme = reward.Cards.FirstOrDefault(c => c is WindBlade or RockGuard or LightningThrust or TailwindPrep or RockAdvance or ThunderPrep);
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
