using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Cards;
using Sts2Mod.Combo;
using Sts2Mod.Pools;
using Sts2Mod.Powers;
using Sts2Mod.Relics;

namespace Sts2Mod.Debug;

internal sealed class TuningCardSelector(Element element) : ExpansionCardSelector
{
    public CardModel[] Candidates { get; private set; } = [];
    public CardModel? Selected { get; private set; }
    public bool HandInputBlocked { get; private set; }
    public override async Task<IEnumerable<CardModel>> GetSelectedCards(IEnumerable<CardModel> options, int minSelect, int maxSelect)
    {
        var hand = NPlayerHand.Instance!;
        await hand.ToSignal(hand.GetTree(), SceneTree.SignalName.ProcessFrame);
        var holder = hand.ActiveHolders.First();
        hand.Call(NPlayerHand.MethodName.OnHolderPressed, holder);
        HandInputBlocked = hand == ElementTuningHandInputPatch.ChoosingHand && !hand.InCardPlay && !hand.IsAwaitingPlay(holder);
        var shortcuts = (StringName[])AccessTools.Field(typeof(NPlayerHand), "_selectCardShortcuts").GetValue(hand)!;
        using var shortcut = new InputEventAction { Action = shortcuts[0], Pressed = true };
        hand._UnhandledInput(shortcut);
        HandInputBlocked &= !hand.InCardPlay && !hand.IsAwaitingPlay(holder);
        hand.CancelAllCardPlay();
        Candidates = options.ToArray();
        Selected = Candidates.OfType<ElementCardModel>().First(c => c.CardElement == element);
        return [Selected];
    }
}

internal sealed class InterruptedTuningSelector : ExpansionCardSelector
{
    public override Task<IEnumerable<CardModel>> GetSelectedCards(IEnumerable<CardModel> options, int minSelect, int maxSelect)
        => Task.FromException<IEnumerable<CardModel>>(new OperationCanceledException("tuning-choice-test"));
}

public partial class TravelerTestRunner
{
    private static BlockingPlayerChoiceContext ExpansionContext() => new();
    private Element NextExpansionElement()
    {
        var rng = _player.RunState.Rng.CombatCardGeneration;
        return new Rng(rng.Seed, rng.Counter).NextItem(new[] { Element.Wind, Element.Rock, Element.Lightning });
    }
    private void SetExpansionElement(Element element)
    {
        Power.State.ClearCombat();
        if (element != Element.None) Power.State.Reserve(new object(), element);
        Power.Publish("expansion_fixture");
    }

    private async Task TestPoolExpansion20261022()
    {
        var start = _assertions;
        await FreshExpansion();
        foreach (var type in TravelerContent.RewardCardTypes.Skip(19))
        foreach (var upgraded in new[] { false, true })
        {
            var card = CombatManager.Instance.DebugOnlyGetState()!.CreateCard(ModelDb.AllCards.Single(c => c.GetType() == type), _player);
            if (upgraded) CardCmd.Upgrade(card);
            var restored = CardModel.FromSerializable(card.ToSerializable());
            Check(restored.Id == card.Id && restored.CurrentUpgradeLevel == card.CurrentUpgradeLevel
                && !restored.GetDescriptionForPile(PileType.None).Contains('{'), "POOL22_model_locale_roundtrip_" + card.Id.Entry + "_" + upgraded);
        }
        var relicTypes = new[] { typeof(TravelerMedicineChest), typeof(TricolorPrism), typeof(ExpeditionDrum), typeof(StatueBlessing) };
        foreach (var type in relicTypes)
        {
            var relic = ModelDb.RelicPool<TravelerRelicPool>().AllRelics.Single(r => r.GetType() == type);
            Check(RelicModel.FromSerializable(relic.ToMutable().ToSerializable()).GetType() == type,
                "POOL22_relic_roundtrip_" + relic.Id.Entry);
        }

        foreach (var up in new[] { false, true })
        {
            await FreshExpansion(); await SeedDraw();
            var status = await Add<Dazed>(); var normal = await Add<WindBlade>();
            await Play<CleansingWind>(up);
            Check(status.Pile?.Type == PileType.Exhaust && normal.Pile?.Type == PileType.Hand
                && PileType.Hand.GetPile(_player).Cards.Count == 2 && _player.Creature.Block == (up ? 8 : 5)
                && Power.State.Last == Element.Wind, "POOL22_cleanse_exhaust_draw_" + up);
            await FreshExpansion();
            await Play<CleansingWind>(up);
            Check(_player.Creature.Block == (up ? 8 : 5) && Power.State.Last == Element.Wind,
                "POOL22_cleanse_empty_" + up);
            await Add<Dazed>();
            using (CardSelectCmd.PushSelector(new ExpansionCardSelector(0))) await Play<CleansingWind>(up);
            Check(PileType.Hand.GetPile(_player).Cards.Single() is Dazed, "POOL22_cleanse_optional_" + up);

            await FreshExpansion();
            await PowerCmd.Apply<WeakPower>(ExpansionContext(), _player.Creature, 1, _target, null);
            await PowerCmd.Apply<VulnerablePower>(ExpansionContext(), _player.Creature, 3, _target, null);
            await PowerCmd.Apply<FrailPower>(ExpansionContext(), _player.Creature, up ? 2 : 1, _target, null);
            await Play<ClearRockMind>(up);
            Check(_player.Creature.GetPower<WeakPower>() is null && _player.Creature.GetPower<FrailPower>() is null
                && _player.Creature.GetPower<VulnerablePower>()?.Amount == (up ? 1 : 2)
                && _player.Creature.Block == (up ? 8 : 6) && !PileType.Exhaust.GetPile(_player).Cards.OfType<ClearRockMind>().Any(),
                "POOL22_rock_cleanse_before_block_reusable_" + up);

            await FreshExpansion();
            var hp = _target.CurrentHp;
            await Play<HeadwindRestraint>(up);
            Check(hp - _target.CurrentHp == (up ? 7 : 4) && _target.GetPower<WeakPower>()?.Amount == 2,
                "POOL22_headwind_damage_then_weak_" + up);
            await PowerCmd.Apply<VulnerablePower>(ExpansionContext(), _target, 3, _player.Creature, null);
            var attack = await Add<OpportunistLightning>(up);
            attack.DynamicVars.CalculatedDamage.UpdateCardPreview(attack, CardPreviewMode.Normal, _target, true);
            var preview = Math.Floor(attack.DynamicVars.CalculatedDamage.PreviewValue);
            hp = _target.CurrentHp;
            await Queue(new PlayCardAction(attack, _target));
            Check(hp - _target.CurrentHp == (up ? 36 : 24) && preview == (up ? 36 : 24)
                && _target.GetPower<WeakPower>()?.Amount == 2 && _target.GetPower<VulnerablePower>()?.Amount == 3,
                "POOL22_opportunist_layers_native_preview_" + up);

            foreach (var rewards in new[] { 0, 1, 2, 3, 4, 7 })
            {
                await FreshExpansion();
                for (var i = 0; i < rewards; i++) Power.State.MarkRewardGranted();
                hp = _target.CurrentHp;
                await Play<ThunderBurst>(up);
                Check(hp - _target.CurrentHp == (up ? 5 : 4) * (2 + Math.Min(rewards, 4)),
                    "POOL22_burst_hits_" + rewards + "_" + up);
            }

            foreach (var element in new[] { Element.Wind, Element.Rock, Element.Lightning })
            foreach (var x in new[] { 0, 1, 3 })
            {
                await FreshExpansion(); await SeedDraw(); SetExpansionElement(element);
                var card = await Add<HeavenEarthConvergence>(up);
                await PlayerCmd.SetEnergy(x, _player);
                var hpBefore = _player.Creature.CombatState!.Enemies.Select(e => e.CurrentHp).ToArray();
                await Queue(new PlayCardAction(card, null));
                var wind = x + (up ? 1 : 0);
                Check(_player.PlayerCombatState!.Energy == (element == Element.Wind ? wind : 0)
                    && PileType.Hand.GetPile(_player).Cards.Count == (element == Element.Wind ? wind : 0)
                    && _player.Creature.Block == 0
                    && (_player.Creature.GetPower<PlatingPower>()?.Amount ?? 0) == (element == Element.Rock ? (up ? 4 : 3) * x : 0)
                    && _player.Creature.CombatState!.Enemies.All(e => (e.GetPower<VulnerablePower>()?.Amount ?? 0)
                        == (element == Element.Lightning ? x + (up ? 1 : 0) : 0))
                    && _player.Creature.CombatState.Enemies.Select((e, i) => hpBefore[i] - e.CurrentHp)
                        .All(d => d == (element == Element.Lightning ? (up ? 8 : 6) * x : 0))
                    && card.Keywords.Contains(CardKeyword.Exhaust) && card.Pile?.Type == PileType.Exhaust
                    && !card.GainsBlock && card.Type == CardType.Skill && Power.State.Last == element && Power.State.Used == 0,
                    "POOL22_x_branch_" + element + "_" + x + "_" + up);
            }
        }
        await FreshExpansion();
        var blocked = await Add<HeavenEarthConvergence>();
        var energy = _player.PlayerCombatState!.Energy;
        Check(!blocked.CanPlay(out var reason, out var preventer) && reason.HasFlag(UnplayableReason.BlockedByHook)
            && preventer is ComboLifecycle && _player.PlayerCombatState.Energy == energy
            && new LocString("cards", "TC_C27.requiresElement").GetFormattedText().Length > 0, "POOL22_x_requires_element_no_payment");
        await FreshExpansion(); await SeedDraw(); SetExpansionElement(Element.Wind);
        await RelicCmd.Obtain<MegaCrit.Sts2.Core.Models.Relics.ChemicalX>(_player);
        await PlayerCmd.SetEnergy(3, _player);
        await Play<HeavenEarthConvergence>(true);
        Check(_player.PlayerCombatState!.Energy == 6 && PileType.Hand.GetPile(_player).Cards.Count == 6,
            "POOL22_x_native_chemical_x_bonus");
        await RelicCmd.Remove(_player.GetRelic<MegaCrit.Sts2.Core.Models.Relics.ChemicalX>()!);
        await FreshExpansion(); SetExpansionElement(Element.Rock);
        var freeX = await Add<HeavenEarthConvergence>();
        await PlayerCmd.SetEnergy(3, _player);
        await CardCmd.AutoPlay(ExpansionContext(), freeX, null);
        Check(_player.PlayerCombatState!.Energy == 3 && _player.Creature.Block == 0
            && _player.Creature.GetPower<PlatingPower>()?.Amount == 9 && freeX.Pile?.Type == PileType.Exhaust,
            "POOL22_x_native_free_autoplay_captures_without_payment");
        await FreshExpansion(); SetExpansionElement(Element.Rock);
        await PowerCmd.Apply<DuplicationPower>(ExpansionContext(), _player.Creature, 1, _player.Creature, null);
        var repeatedHp = _target.CurrentHp;
        await Play<ThunderBurst>();
        Check(repeatedHp - _target.CurrentHp == 20 && Power.State.Used == 1 && Power.State.RewardsThisTurn == 1,
            "POOL22_burst_replay_preserves_preplay_reward_snapshot");
        await FreshExpansion(); SetExpansionElement(Element.Rock);
        await PowerCmd.Apply<DuplicationPower>(ExpansionContext(), _player.Creature, 1, _player.Creature, null);
        var repeatedX = await Add<HeavenEarthConvergence>();
        CombatCardElements.Assigned.Set(repeatedX, (int)Element.Lightning);
        await PlayerCmd.SetEnergy(3, _player);
        repeatedHp = _player.Creature.CombatState!.Enemies.Sum(e => e.CurrentHp);
        await Queue(new PlayCardAction(repeatedX, null));
        Check(_player.Creature.Block == 0 && _player.Creature.GetPower<PlatingPower>()?.Amount == 18
            && repeatedX.Pile?.Type == PileType.Exhaust && _player.PlayerCombatState!.Energy == 0
            && repeatedHp - _player.Creature.CombatState.Enemies.Sum(e => e.CurrentHp) == 4 && Power.State.Used == 1,
            "POOL22_x_replay_preserves_element_and_payment_snapshot");

        foreach (var up in new[] { false, true })
        {
            await FreshExpansion(); SetExpansionElement(Element.Lightning);
            var enemies = _player.Creature.CombatState!.Enemies.ToArray();
            var protectedEnemy = enemies.Last();
            await PowerCmd.Apply<ArtifactPower>(ExpansionContext(), protectedEnemy, 1, protectedEnemy, null);
            _target.SetCurrentHpInternal(1);
            await PlayerCmd.SetEnergy(1, _player);
            var card = await Add<HeavenEarthConvergence>(up);
            await Queue(new PlayCardAction(card, null));
            Check(!_target.IsAlive && _target.GetPower<VulnerablePower>() is null
                && protectedEnemy.IsAlive && protectedEnemy.GetPower<VulnerablePower>() is null
                && protectedEnemy.GetPower<ArtifactPower>() is null && card.Pile?.Type == PileType.Exhaust,
                "POOL22_x_lightning_dead_targets_and_artifact_" + up);
        }

        await TestEchoingBlight();
        await TestExpansion22Relics();
        await TestExpansion22Tuning();
        await TestExpansion22Infusion();
        GD.Print($"STS2MOD_POOL22_COMPLETE assertions={_assertions - start} cards=9 relics=4 rendering=NOT_RUN multiplayer=NOT_RUN");
    }

    private async Task TestExpansion22Relics()
    {
        await FreshExpansion(); await SeedDraw();
        await RelicCmd.Obtain<TravelerMedicineChest>(_player);
        await Play<CleansingWind>();
        Check(_player.Creature.Block == 5, "POOL22_medicine_no_exhaust_no_proc");
        await Add<Dazed>(); await Play<CleansingWind>();
        Check(_player.Creature.Block == 15, "POOL22_medicine_first_status_five_block");
        var status = await Add<Dazed>(); await CardCmd.Exhaust(ExpansionContext(), status, causedByEthereal: true);
        Check(_player.Creature.Block == 15, "POOL22_medicine_once_per_turn");
        await ExpansionNativeTurn();
        var before = _player.Creature.Block;
        status = await Add<Dazed>(); await CardCmd.Exhaust(ExpansionContext(), status, causedByEthereal: true);
        Check(_player.Creature.Block == before + 5, "POOL22_medicine_native_ethereal_next_turn");

        await FreshExpansion();
        await RelicCmd.Obtain<StatueBlessing>(_player);
        await Play<RockGuard>();
        Check(_player.Creature.GetPower<PlatingPower>() is null, "POOL22_statue_initial_record_no_proc");
        _target.SetCurrentHpInternal(7);
        await Play<LightningThrust>();
        Check(_player.Creature.GetPower<PlatingPower>()?.Amount == 6 && Power.State.Used == 1
            && Power.State.RewardsThisTurn == 0, "POOL22_statue_trigger_not_actual_reward");
        await Play<RockGuard>();
        Check(_player.Creature.GetPower<PlatingPower>()?.Amount == 6, "POOL22_statue_once_per_combat");

        await FreshExpansion();
        await RelicCmd.Obtain<ExpeditionDrum>(_player);
        await Play<WindBlade>(); await Play<RockGuard>(); await Play<WindBlade>(); await Play<RockGuard>();
        Check(_player.GetRelic<ExpeditionDrum>()!.Ready, "POOL22_drum_third_actual_arms");
        var source = await Play<GaleSweep>();
        var automatic = await Add<LightningThrust>();
        await CardCmd.AutoPlay(source.PlayerChoiceContext!, automatic, _target);
        await Play<ThunderChain>();
        SetExpansionElement(Element.Lightning);
        await Play<HeavenEarthConvergence>();
        Check(_player.GetRelic<ExpeditionDrum>()!.Ready, "POOL22_drum_ignores_aoe_random_auto_and_x_skill");
        await PowerCmd.Remove<VulnerablePower>(_target);
        await PlayerCmd.SetEnergy(10, _player);
        var hp = _target.CurrentHp;
        // Resetting the input state also resets the burst's count; two hits should gain +8 only once.
        await Play<ThunderBurst>();
        Check(hp - _target.CurrentHp == 16 && !_player.GetRelic<ExpeditionDrum>()!.Ready,
            "POOL22_drum_only_first_hit_not_combo");
        SetExpansionElement(Element.Wind);
        await Play<RockGuard>(); await Play<WindBlade>(); await Play<RockGuard>();
        await ExpansionNativeTurn();
        Check(!_player.GetRelic<ExpeditionDrum>()!.Ready, "POOL22_drum_expires_at_turn_end");

        await FreshExpansion();
        await RelicCmd.Obtain<TricolorPrism>(_player);
        var expected = NextExpansionElement();
        var card = await Add<TravelerDefend>();
        await Queue(new PlayCardAction(card, null));
        Check(CombatCardElements.Get(card) == expected && Power.State.Last == expected,
            "POOL22_prism_plain_skill_manual_combo_entry");
        Check(card.GetDescriptionForPile(PileType.Discard).Contains(new LocString("powers", "TC_ELEMENT_" + expected.ToString().ToUpperInvariant()).GetFormattedText()),
            "POOL22_prism_visible_on_plain_card");
        var marked = CombatCardElements.Assigned.Get(card);
        await CardPileCmd.Add(card, PileType.Hand);
        await Queue(new PlayCardAction(card, null));
        Check(CombatCardElements.Assigned.Get(card) == marked, "POOL22_prism_instance_survives_discard_redraw");
        var restored = CardModel.FromSerializable(card.ToSerializable());
        var clone = card.CreateClone();
        Check(CombatCardElements.Get(restored) == expected && CombatCardElements.Assigned.Get(clone) == 0,
            "POOL22_prism_serialized_mark_copy_does_not_inherit");
        await ExpansionNativeTurn();
        expected = NextExpansionElement();
        await CardPileCmd.Add(card, PileType.Hand);
        await Queue(new PlayCardAction(card, null));
        Check(CombatCardElements.Get(card) == expected, "POOL22_prism_next_turn_rerolls_same_instance");
        await FreshExpansion();
        await RelicCmd.Obtain<TricolorPrism>(_player);
        var automaticFirst = await Add<WindBlade>();
        expected = NextExpansionElement();
        await CardCmd.AutoPlay(ExpansionContext(), automaticFirst, _target);
        Check(CombatCardElements.Get(automaticFirst) == expected && Power.State.Last == Element.None,
            "POOL22_prism_auto_first_assigns_without_combo");
        var unmarked = await Add<TravelerDefend>();
        await Queue(new PlayCardAction(unmarked, null));
        Check(CombatCardElements.Assigned.Get(unmarked) == 0 && Power.State.Last == Element.None,
            "POOL22_prism_auto_consumes_first_card_slot");
        await FreshExpansion(); await SeedDraw();
        await RelicCmd.Obtain<TricolorPrism>(_player);
        var rng = _player.RunState.Rng.CombatCardGeneration;
        var oracle = new Rng(rng.Seed, rng.Counter);
        var assigned = oracle.NextItem(new[] { Element.Wind, Element.Rock, Element.Lightning });
        var infusion = oracle.NextItem(new[] { Element.Wind, Element.Rock, Element.Lightning });
        await Play<TriphaseInfusion>();
        Check(rng.Counter == oracle.Counter && Power.State.Last == assigned
            && Power.State.Used == (infusion == assigned ? 0 : 1), "POOL22_prism_infusion_effect_before_card_combo");
    }

    private async Task TestExpansion22Tuning()
    {
        foreach (var up in new[] { false, true })
        foreach (var element in new[] { Element.Wind, Element.Rock, Element.Lightning })
        {
            await FreshExpansion(); await SeedDraw();
            SetExpansionElement(element == Element.Rock ? Element.Lightning : Element.Rock);
            await RelicCmd.Obtain<StatueBlessing>(_player);
            var selector = new TuningCardSelector(element);
            await Add<WindBlade>();
            var energy = _player.PlayerCombatState!.Energy;
            using (CardSelectCmd.PushSelector(selector)) await Play<ElementTuning>(up);
            Check(selector.HandInputBlocked && ElementTuningHandInputPatch.ChoosingHand is null,
                "POOL22_tuning_blocks_hand_input_and_restores_" + element + "_" + up);
            Check(selector.Candidates.Length == 3 && selector.Candidates.OfType<ElementCardModel>().Select(c => c.CardElement).Distinct().Count() == 3
                && selector.Candidates.All(c => TravelerContent.RewardCardTypes.Contains(c.GetType()) && c.CurrentUpgradeLevel == (up ? 1 : 0))
                && selector.Candidates.Where(c => c != selector.Selected).All(c => c.Pile is null)
                && selector.Selected!.Pile?.Type == PileType.Exhaust && _player.PlayerCombatState.Energy == energy - 1
                && Power.State.Last == element && Power.State.Used == 1 && _player.Creature.GetPower<PlatingPower>()?.Amount == 6,
                "POOL22_tuning_choice_free_exhaust_combo_" + element + "_" + up);
            Check(selector.Selected!.EnergyCost.GetWithModifiers(CostModifiers.Local) == selector.Selected.EnergyCost.Canonical,
                "POOL22_tuning_does_not_change_permanent_cost_" + element + "_" + up);
        }
        await FreshExpansion(); await SeedDraw();
        await RelicCmd.Obtain<TricolorPrism>(_player);
        var assigned = NextExpansionElement();
        var chosenElement = assigned == Element.Wind ? Element.Rock : Element.Wind;
        SetExpansionElement(chosenElement);
        var selected = new TuningCardSelector(chosenElement);
        await Add<WindBlade>();
        using (CardSelectCmd.PushSelector(selected)) await Play<ElementTuning>();
        Check(Power.State.Used == 2 && Power.State.Last == chosenElement,
            "POOL22_prism_tuning_two_distinct_combo_events");

        await FreshExpansion();
        var interrupted = await Add<ElementTuning>();
        var play = new CardPlay { Card = interrupted, Target = null, ResultPile = PileType.Discard,
            Resources = new ResourceInfo { EnergySpent = 0, EnergyValue = 0, StarsSpent = 0, StarValue = 0 },
            IsAutoPlay = true, PlayIndex = 0, PlayCount = 1 };
        var cancelled = false;
        using (CardSelectCmd.PushSelector(new InterruptedTuningSelector()))
        {
            try { await (Task)AccessTools.Method(typeof(ElementTuning), "OnPlay").Invoke(interrupted, [ExpansionContext(), play])!; }
            catch (OperationCanceledException ex) when (ex.Message == "tuning-choice-test") { cancelled = true; }
        }
        Check(cancelled && ElementTuningHandInputPatch.ChoosingHand is null,
            "POOL22_tuning_interrupted_choice_restores_hand_input");
    }

    private async Task TestExpansion22Infusion()
    {
        await FreshExpansion(); await SeedDraw();
        var expected = NextExpansionElement();
        await Play<TriphaseInfusion>();
        Check(Power.State.Last == expected && Power.State.Used == 0 && _player.Creature.GetPower<DexterityPower>() is null,
            "POOL22_infusion_empty_records_only");
        var power = _player.Creature.GetPower<TriphaseInfusionPower>()!;
        expected = NextExpansionElement(); SetExpansionElement(expected);
        var rngBefore = _player.RunState.Rng.CombatCardGeneration.Counter;
        await Play<TriphaseInfusion>(true);
        Check(power.Amount == 2 && _player.Creature.GetPower<DexterityPower>()?.Amount == 2
            && _player.RunState.Rng.CombatCardGeneration.Counter == rngBefore + 1 && Power.State.Used == 0,
            "POOL22_infusion_two_copies_one_roll_two_dexterity");
        var source = await Add<TriphaseInfusion>();
        expected = NextExpansionElement(); SetExpansionElement(expected);
        await power.Trigger(ExpansionContext(), source, null);
        await PowerCmd.Apply<DexterityPower>(ExpansionContext(), _player.Creature, 5, _player.Creature, null);
        await power.AfterSideTurnEnd(ExpansionContext(), _player.Creature.Side, [_player.Creature]);
        Check(_player.Creature.GetPower<DexterityPower>()?.Amount == 5,
            "POOL22_infusion_accumulates_and_removes_only_own_dexterity");
        expected = NextExpansionElement();
        SetExpansionElement(expected == Element.Rock ? Element.Lightning : expected == Element.Lightning ? Element.Rock : Element.Lightning);
        rngBefore = _player.RunState.Rng.CombatCardGeneration.Counter;
        await RelicCmd.Obtain<StatueBlessing>(_player);
        await power.Trigger(ExpansionContext(), source, null);
        Check(Power.State.Used == 1 && Power.State.RewardsThisTurn == 1 && Power.State.Last == expected
            && _player.RunState.Rng.CombatCardGeneration.Counter == rngBefore + 1
            && _player.Creature.GetPower<PlatingPower>()?.Amount == 6,
            "POOL22_infusion_effect_combo_rewards_and_statue");
        for (var i = Power.State.Used; i < Power.State.TurnLimit; i++)
            Power.State.Reserve(new object(), Power.State.Last == Element.Wind ? Element.Rock : Element.Wind);
        var rewards = Power.State.RewardsThisTurn;
        expected = NextExpansionElement();
        await power.Trigger(ExpansionContext(), source, null);
        Check(Power.State.Last == expected && Power.State.Used == Power.State.TurnLimit && Power.State.RewardsThisTurn == rewards,
            "POOL22_infusion_cap_updates_record_without_reward");
        rngBefore = _player.RunState.Rng.CombatCardGeneration.Counter;
        await ExpansionNativeTurn();
        Check(_player.RunState.Rng.CombatCardGeneration.Counter == rngBefore + 1 && Power.State.Used <= 1
            && Power.State.RewardsThisTurn <= 1, "POOL22_infusion_turn_start_after_reset_single_roll");
    }
}
