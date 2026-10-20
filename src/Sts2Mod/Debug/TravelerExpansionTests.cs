using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Powers.Mocks;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Cards;
using Sts2Mod.Combo;
using Sts2Mod.Pools;
using Sts2Mod.Powers;
using Sts2Mod.Relics;

namespace Sts2Mod.Debug;

internal sealed class ExpansionCardSelector(int limit = int.MaxValue) : ICardSelector
{
    public Task<IEnumerable<CardModel>> GetSelectedCards(IEnumerable<CardModel> options, int minSelect, int maxSelect)
        => Task.FromResult<IEnumerable<CardModel>>(options.Take(Math.Min(maxSelect, limit)).ToArray());
    public CardRewardSelection GetSelectedCardReward(IReadOnlyList<CardCreationResult> options,
        IReadOnlyList<CardRewardAlternative> alternatives) => throw new NotSupportedException("Use the native reward fixture.");
}

public partial class TravelerTestRunner
{
    private async Task FreshExpansion()
    {
        foreach (var relic in _player.Relics.Where(r => r is PaimonCheer or JourneyCompass).ToArray())
            await RelicCmd.Remove(relic);
        await StartFreshCombat();
        foreach (var enemy in _player.Creature.CombatState!.Enemies)
        {
            enemy.SetMaxHpInternal(3000m); enemy.SetCurrentHpInternal(3000m);
        }
    }
    private async Task ExpansionNativeTurn()
    {
        _player.Creature.HealInternal(9999m);
        var number = _player.PlayerCombatState!.TurnNumber;
        await Queue(new EndPlayerTurnAction(_player, number));
        await Until(() => _player.PlayerCombatState!.TurnNumber > number && ReadyToPlay, "expansion native next turn");
    }

    private async Task TestExpandedCards()
    {
        await TestComboHud();
        using var selector = CardSelectCmd.PushSelector(new ExpansionCardSelector());
        var start = _assertions;
        foreach (var asset in new[] { "wind_guidance", "rock_heart", "thunder_echo", "element_resonance", "gale_sweep",
            "shattering_rock", "thunder_mark", "triphase_finale", "wind_shelter", "patient_rampart", "charge_thunder",
            "journey_plans", "strike", "defend", "compass", "combo", "body", "icon", "select_background", "head_icon" })
        {
            var folder = asset is "body" or "icon" or "select_background" or "head_icon" ? "character" : "art";
            var image = ResourceLoader.Load<Texture2D>($"res://Sts2Mod/{folder}/{asset}.png");
            Check(image is not null && image.GetWidth() >= 512 && image.GetHeight() >= 512, "ART_final_png_" + asset);
        }
        var visual = ResourceLoader.Load<PackedScene>("res://Sts2Mod/character/traveler.tscn")!.Instantiate<Node2D>();
        var body = visual.GetNode<Sprite2D>("Visuals/Body");
        Check(body.Texture.ResourcePath.EndsWith("body.png") && body.Texture.GetHeight() * body.Scale.Y is >= 310 and <= 314,
            "ART_character_native_battle_scale_312px");
        visual.Free();
        var topBarIcon = _player.Character.Icon;
        Check(topBarIcon is TextureRect topTexture && topTexture.Texture.ResourcePath.EndsWith("head_icon.png"),
            "ART_topbar_uses_face_closeup_scene");
        topBarIcon.Free();
        await TestDefensiveRandomCombos();
        await FreshExpansion();
        var pool = _player.Character.CardPool.GetUnlockedCards(_player.UnlockState, 0).Where(c => c.Rarity != CardRarity.Basic).ToArray();
        Check(pool.Length == 18 && pool.Count(c => c.Type == CardType.Attack) == 7
            && pool.Count(c => c.Type == CardType.Skill) == 7 && pool.Count(c => c.Type == CardType.Power) == 4,
            "EXP_pool_18_types_7_7_4");
        Check(pool.Count(c => c.Rarity == CardRarity.Common) == 8 && pool.Count(c => c.Rarity == CardRarity.Uncommon) == 7
            && pool.Count(c => c.Rarity == CardRarity.Rare) == 3, "EXP_rarity_8_7_3");
        Check(TravelerContent.RewardCardTypes.All(t => pool.Count(c => c.GetType() == t) == 1), "EXP_all_ids_once");
        var boss = CardFactory.CreateForReward(_player, 3, CardCreationOptions.ForRoom(_player, RoomType.Boss)).ToArray();
        Check(boss.Length == 3 && boss.All(c => c.Card.Rarity == CardRarity.Rare), "EXP_boss_native_rare_rewards");
        for (var i = 0; i < 5; i++)
        {
            var shop = MegaCrit.Sts2.Core.Entities.Merchant.MerchantInventory.CreateForNormalMerchant(_player);
            var cards = shop.CharacterCardEntries.Select(e => e.CreationResult!.Card).ToArray();
            Check(cards.Count(c => c.Type == CardType.Power) == 1 && cards.Count(c => c.Type == CardType.Skill) == 2
                && cards.Count(c => c.Type == CardType.Attack) == 2, "EXP_shop_native_type_slots_" + i);
        }

        var upgradedCosts = new[] { 0, 1, 2, 1, 1, 2, 1, 2, 1, 1, 1, 0 };
        for (var i = 0; i < 12; i++)
        {
            var canonical = pool.Single(c => c.Id.Entry == "TC_C" + (i + 7).ToString("00"));
            foreach (var upgraded in new[] { false, true })
            {
                var original = canonical.ToMutable();
                if (upgraded) CardCmd.Upgrade(original);
                var restored = CardModel.FromSerializable(original.ToSerializable());
                Check(restored.Id == original.Id && restored.GetType() == original.GetType()
                    && restored.CurrentUpgradeLevel == (upgraded ? 1 : 0)
                    && restored.EnergyCost.GetWithModifiers(CostModifiers.Local) == (upgraded ? upgradedCosts[i] : canonical.EnergyCost.Canonical),
                    "EXP_native_roundtrip_" + canonical.Id.Entry + "_" + upgraded);
                Check(!restored.GetDescriptionForPile(PileType.None).Contains("{ComboPreview}"),
                    "EXP_library_description_" + canonical.Id.Entry + "_" + upgraded);
            }
            Check(ResourceLoader.Load<Texture2D>(((CustomCardModel)canonical).CustomPortraitPath) is not null,
                "EXP_portrait_" + canonical.Id.Entry);
        }

        foreach (var up in new[] { false, true })
        {
            await FreshExpansion();
            var hp = _player.Creature.CombatState!.Enemies.Select(e => e.CurrentHp).ToArray();
            await Play<GaleSweep>(up);
            Check(_player.Creature.CombatState.Enemies.Select((e, i) => hp[i] - e.CurrentHp).All(d => d == (up ? 8 : 5))
                && Power.State.Last == Element.Wind, "EXP_gale_all_enemies_" + up);
            await FreshExpansion();
            var before = _target.CurrentHp;
            await Play<ShatteringRock>(up);
            Check(before - _target.CurrentHp == (up ? 17 : 13) && Power.State.Last == Element.Rock,
                "EXP_shattering_rock_" + up);
            await FreshExpansion();
            await Play<RockGuard>();
            before = _target.CurrentHp;
            await Play<ThunderMark>(up);
            Check(before - _target.CurrentHp == (up ? 14 : 11) && _target.GetPower<VulnerablePower>()?.Amount == 1
                && Power.State.Used == 1, "EXP_thunder_mark_vulnerable_before_combo_" + up);
            await FreshExpansion();
            await Play<WindShelter>(up);
            Check(_player.Creature.Block == (up ? 9 : 6) && Power.State.Last == Element.Wind, "EXP_wind_shelter_" + up);
            await FreshExpansion();
            var wall = await Add<PatientRampart>(up);
            await Queue(new PlayCardAction(wall, null));
            Check(_player.Creature.Block == (up ? 8 : 5) && wall.Keywords.Contains(CardKeyword.Retain),
                "EXP_patient_rampart_block_" + up);
            await FreshExpansion();
            await Play<ChargeThunder>(up);
            Check(_player.Creature.Block == (up ? 7 : 4) && _player.Creature.GetPower<EnergyNextTurnPower>()?.Amount == 1
                && PileType.Exhaust.GetPile(_player).Cards.Any(c => c is ChargeThunder), "EXP_charge_thunder_base_" + up);
            await SeedDraw();
            await ExpansionNativeTurn();
            Check(_player.PlayerCombatState!.Energy == _player.MaxEnergy + 1 && _player.Creature.GetPower<EnergyNextTurnPower>() is null,
                "EXP_charge_thunder_next_turn_energy_" + up);
            await ExpansionNativeTurn();
            Check(_player.PlayerCombatState!.Energy == _player.MaxEnergy, "EXP_charge_thunder_not_repeated_" + up);
            await FreshExpansion();
            before = _target.CurrentHp;
            await Play<TriphaseFinale>(up);
            Check(before - _target.CurrentHp == (up ? 14 : 10) && Power.State.Used == 0 && Power.State.Last == Element.None,
                "EXP_finale_zero_combos_" + up);
            await Play<WindBlade>(); await Play<RockGuard>(); await Play<WindBlade>(); await Play<RockGuard>();
            before = _target.CurrentHp;
            await Play<TriphaseFinale>(up);
            Check(before - _target.CurrentHp == (up ? 26 : 22) && Power.State.Used == 3 && Power.State.Last == Element.Rock,
                "EXP_finale_three_combos_without_element_advance_" + up);
            var finale = await Add<TriphaseFinale>(up);
            await PowerCmd.Apply<StrengthPower>(new BlockingPlayerChoiceContext(), _player.Creature, 2m, _player.Creature, null);
            await PowerCmd.Apply<WeakPower>(new BlockingPlayerChoiceContext(), _player.Creature, 2m, _player.Creature, null);
            await PowerCmd.Apply<VulnerablePower>(new BlockingPlayerChoiceContext(), _target, 2m, _player.Creature, null);
            finale.DynamicVars.CalculatedDamage.UpdateCardPreview(finale, CardPreviewMode.Normal, _target, true);
            var displayedFinale = Math.Floor(finale.DynamicVars.CalculatedDamage.PreviewValue);
            var expectedFinale = up ? 31 : 27;
            before = _target.CurrentHp;
            await Queue(new PlayCardAction(finale, _target));
            Check(before - _target.CurrentHp == expectedFinale && displayedFinale == expectedFinale,
                "EXP_finale_native_preview_matches_modified_damage_" + up);
        }

        await TestExpansionRetain();
        await TestExpansionAbilities();
        await FreshExpansion();
        Check(Power.State.Last == Element.None && Power.State.Used == 0 && Power.State.RewardsThisTurn == 0
            && !_player.Creature.Powers.OfType<TravelerAbilityPower>().Any(), "EXP_new_combat_clears_ability_and_combo_state");
        GD.Print($"STS2MOD_EXPANSION_COMPLETE assertions={_assertions - start} cards=18 powers=4 multiplayer=NOT_RUN rendering=NOT_RUN");
    }

    private async Task TestDefensiveRandomCombos()
    {
        foreach (var upgraded in new[] { false, true })
        foreach (var type in new[] { typeof(RockGuard), typeof(PatientRampart), typeof(ThunderPrep), typeof(ChargeThunder) })
        {
            await FreshExpansion();
            if (upgraded) await RelicCmd.Obtain<PaimonCheer>(_player);
            // The priming attack must not use the target RNG.
            if (type == typeof(RockGuard) || type == typeof(PatientRampart)) await Play<LightningThrust>();
            else await Play<RockGuard>();
            var enemies = _player.Creature.CombatState!.GetOpponentsOf(_player.Creature).Where(e => e.IsAlive).ToArray();
            var health = enemies.Select(e => e.CurrentHp).ToArray();
            var rng = _player.RunState.Rng.CombatTargets;
            var oracle = new MegaCrit.Sts2.Core.Random.Rng(rng.Seed, rng.Counter);
            var expected = oracle.NextItem(enemies)!;
            var before = rng.Counter;
            var card = CombatManager.Instance.DebugOnlyGetState()!.CreateCard(ModelDb.AllCards.Single(c => c.GetType() == type), _player);
            if (upgraded) CardCmd.Upgrade(card);
            await CardPileCmd.Add(card, PileType.Hand);
            // Native self-target hand actions use a null explicit target, not the player creature.
            await Queue(new PlayCardAction(card, null));
            var bonus = upgraded ? 8 : 4;
            GD.Print($"STS2MOD_RANDOM_DETAIL card={card.Id.Entry} upgraded={upgraded} expected={expected.CombatId} rng_before={before} rng_after={rng.Counter} oracle={oracle.Counter} damage={string.Join(',', enemies.Select((e, i) => health[i] - e.CurrentHp))}");
            Check(enemies.Select((enemy, i) => health[i] - enemy.CurrentHp == (enemy == expected ? bonus : 0)).All(ok => ok)
                && rng.Counter == oracle.Counter && rng.Counter > before && Power.State.Used == 1
                && Power.State.RewardsThisTurn == 1, "RANDOM_seeded_single_living_target_" + card.Id.Entry + "_" + upgraded);
        }
        await FreshExpansion();
        var other = _player.Creature.CombatState!.Enemies.First(e => e != _target);
        await CreatureCmd.Kill(other, force: true);
        var hp = _target.CurrentHp;
        await Play<LightningThrust>(); await Play<RockGuard>();
        Check(hp - _target.CurrentHp == 11 && other.IsDead, "RANDOM_only_living_enemy_selected");
        var counter = _player.RunState.Rng.CombatTargets.Counter;
        await Play<ThunderPrep>(); await Play<RockGuard>();
        hp = _target.CurrentHp;
        await Play<ThunderPrep>();
        Check(_target.CurrentHp == hp && Power.State.Used == 3
            && _player.RunState.Rng.CombatTargets.Counter == counter + 2, "RANDOM_shared_cap_no_fourth_hit_or_rng");

        await FreshExpansion();
        await Play<ThunderEcho>();
        foreach (var enemy in _player.Creature.CombatState!.Enemies)
            await PowerCmd.Apply<BufferPower>(new BlockingPlayerChoiceContext(), enemy, 1m, enemy, null);
        hp = _player.Creature.CombatState!.Enemies.Sum(e => e.CurrentHp);
        await Play<RockGuard>(); await Play<ThunderPrep>();
        Check(_player.Creature.CombatState!.Enemies.Sum(e => e.CurrentHp) == hp && Power.State.RewardsThisTurn == 0,
            "RANDOM_zero_damage_no_echo_or_actual_reward");
        GD.Print("STS2MOD_RANDOM_COMBO_COMPLETE cases=11 seeded_rng=native cap=3 rendering=NOT_RUN multiplayer=NOT_RUN");
    }

    private async Task TestExpansionRetain()
    {
        await FreshExpansion();
        await SeedDraw();
        var wall = await Add<PatientRampart>();
        var filler = await Add<WindBlade>();
        await ExpansionNativeTurn();
        Check(wall.Pile?.Type == PileType.Hand && filler.Pile?.Type == PileType.Discard, "EXP_retain_keyword_survives_native_flush");

        foreach (var up in new[] { false, true })
        {
            await FreshExpansion(); await SeedDraw();
            var first = await Add<WindBlade>(); var second = await Add<LightningThrust>();
            var third = await Add<RockGuard>();
            await Play<JourneyPlans>(up);
            Check(first.ShouldRetainThisTurn && second.ShouldRetainThisTurn == up && !third.ShouldRetainThisTurn
                && Power.State.Last == Element.None && PileType.Exhaust.GetPile(_player).Cards.Any(c => c is JourneyPlans),
                "EXP_plans_native_selection_" + up);
            await ExpansionNativeTurn();
            Check(first.Pile?.Type == PileType.Hand && (up ? second.Pile?.Type == PileType.Hand : second.Pile?.Type == PileType.Discard),
                "EXP_plans_native_retention_" + up);
        }
        await FreshExpansion();
        await Play<JourneyPlans>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 0, "EXP_plans_empty_hand_returns_without_choice");
        await Add<WindBlade>();
        using (CardSelectCmd.PushSelector(new ExpansionCardSelector(0))) await Play<JourneyPlans>(true);
        Check(PileType.Hand.GetPile(_player).Cards.All(c => !c.ShouldRetainThisTurn), "EXP_plans_upgraded_can_skip");

        foreach (var up in new[] { false, true })
        {
            await FreshExpansion(); await SeedDraw();
            await Play<RockHeart>(up);
            await CreatureCmd.GainBlock(_player.Creature, 50m, ValueProp.Unpowered, null);
            // Zero enemy attacks for this native-boundary test; no block is added on the new turn.
            foreach (var enemy in _player.Creature.CombatState!.Enemies)
                await PowerCmd.Apply<StrengthPower>(new BlockingPlayerChoiceContext(), enemy, -999m, enemy, null);
            var gains = 0;
            void Observe(int before, int after) { if (after > before) gains++; }
            _player.Creature.BlockChanged += Observe;
            try { await ExpansionNativeTurn(); }
            finally { _player.Creature.BlockChanged -= Observe; }
            Check(_player.Creature.Block == (up ? 6 : 4) && gains == 0, "EXP_rock_heart_retains_without_regaining_" + up);
        }
        await FreshExpansion(); await SeedDraw();
        await Play<RockHeart>(); await Play<RockHeart>(true);
        await CreatureCmd.GainBlock(_player.Creature, 30m, ValueProp.Unpowered, null);
        foreach (var enemy in _player.Creature.CombatState!.Enemies)
            await PowerCmd.Apply<StrengthPower>(new BlockingPlayerChoiceContext(), enemy, -999m, enemy, null);
        await ExpansionNativeTurn();
        Check(_player.Creature.Block == 10, "EXP_rock_heart_stacks_caps_4_plus_6");
        await FreshExpansion(); await SeedDraw();
        await Play<RockHeart>();
        await CreatureCmd.GainBlock(_player.Creature, 2m, ValueProp.Unpowered, null);
        foreach (var enemy in _player.Creature.CombatState!.Enemies)
            await PowerCmd.Apply<StrengthPower>(new BlockingPlayerChoiceContext(), enemy, -999m, enemy, null);
        await ExpansionNativeTurn();
        Check(_player.Creature.Block == 2, "EXP_rock_heart_does_not_create_missing_block");
        foreach (var blur in new[] { false, true })
        {
            await FreshExpansion(); await SeedDraw();
            await Play<RockHeart>();
            if (blur) await PowerCmd.Apply<BlurPower>(new BlockingPlayerChoiceContext(), _player.Creature, 1m, _player.Creature, null);
            else await PowerCmd.Apply<BarricadePower>(new BlockingPlayerChoiceContext(), _player.Creature, 1m, _player.Creature, null);
            await CreatureCmd.GainBlock(_player.Creature, 30m, ValueProp.Unpowered, null);
            foreach (var enemy in _player.Creature.CombatState!.Enemies)
                await PowerCmd.Apply<StrengthPower>(new BlockingPlayerChoiceContext(), enemy, -999m, enemy, null);
            await ExpansionNativeTurn();
            Check(_player.Creature.Block == 30, "EXP_rock_heart_native_retention_wins_" + blur);
        }
        await FreshExpansion(); await SeedDraw();
        await Play<RockHeart>();
        await CreatureCmd.GainBlock(_player.Creature, 2m, ValueProp.Unpowered, null);
        await ExpansionNativeTurn();
        await ExpansionNativeTurn();
        Check(_player.Creature.Block == 0, "EXP_rock_heart_does_not_restore_enemy_consumed_block");
    }

    private async Task TestExpansionAbilities()
    {
        await FreshExpansion(); await SeedDraw();
        await Play<WindGuidance>(); await Play<WindGuidance>(true);
        Check(Power.State.Last == Element.None && _player.Creature.GetPower<WindGuidancePower>()?.Amount == 2,
            "EXP_power_cards_do_not_advance_and_copies_stack");
        await Play<WindBlade>(); await Play<LightningThrust>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 3, "EXP_guidance_two_copies_once_each");
        await Play<WindBlade>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 4, "EXP_guidance_second_combo_no_second_draw");
        await NextTurn(preserveElement: true); await SeedDraw();
        await Play<LightningThrust>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 3, "EXP_guidance_resets_on_next_turn");

        await FreshExpansion(); await SeedDraw();
        await Play<WindGuidance>(); await Play<WindBlade>(); await Play<LightningThrust>();
        await Play<WindGuidance>();
        await Play<WindBlade>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 4, "EXP_guidance_new_copy_can_trigger_after_old_spent");
        await FreshExpansion(); await SeedDraw();
        await Play<WindGuidance>();
        var wind = await Play<WindBlade>();
        await PowerCmd.Apply<NoDrawPower>(wind.PlayerChoiceContext!, _player.Creature, 1m, _player.Creature, null);
        await Play<LightningThrust>();
        Check(Power.State.RewardsThisTurn == 0 && PileType.Hand.GetPile(_player).Cards.Count == 0, "EXP_zero_draw_does_not_consume_actual_reward");
        await PowerCmd.Remove<NoDrawPower>(_player.Creature);
        await Play<WindBlade>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 2 && Power.State.RewardsThisTurn == 1,
            "EXP_guidance_after_blocked_draw_still_triggers");

        await FreshExpansion(); await SeedDraw();
        await RelicCmd.Obtain<PaimonCheer>(_player); await RelicCmd.Obtain<JourneyCompass>(_player);
        await Play<WindGuidance>(); await Play<WindBlade>(); await Play<LightningThrust>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 4, "EXP_paimon_combo2_guidance1_compass1");

        await FreshExpansion();
        await Play<ThunderEcho>(); await Play<ThunderEcho>(true);
        var other = _player.Creature.CombatState!.Enemies.First(e => e != _target);
        var hp = _target.CurrentHp; var otherHp = other.CurrentHp;
        await Play<RockGuard>(); await Play<LightningThrust>();
        Check(hp - _target.CurrentHp == 19 && otherHp - other.CurrentHp == 8
            && _player.Creature.GetPower<ThunderEchoPower>()?.Amount == 8, "EXP_echo_stacks_3_plus_5_and_hits_all");
        hp = _target.CurrentHp; otherHp = other.CurrentHp;
        await Play<ShatteringRock>();
        Check(hp - _target.CurrentHp == 25 && otherHp - other.CurrentHp == 8 && Power.State.Used == 2,
            "EXP_echo_repeats_on_second_rock_lightning");

        await FreshExpansion();
        await RelicCmd.Obtain<PaimonCheer>(_player);
        await Play<ThunderEcho>();
        var source = await Play<RockGuard>();
        await PowerCmd.Apply<StrengthPower>(source.PlayerChoiceContext!, _player.Creature, 2m, _player.Creature, null);
        hp = _target.CurrentHp; other = _player.Creature.CombatState!.Enemies.First(e => e != _target); otherHp = other.CurrentHp;
        await Play<LightningThrust>();
        Check(hp - _target.CurrentHp == 22 && otherHp - other.CurrentHp == 3,
            "EXP_echo_unaffected_by_paimon_or_strength");

        await FreshExpansion();
        await Play<ThunderEcho>(); await Play<ElementResonance>();
        hp = _target.CurrentHp; other = _player.Creature.CombatState!.Enemies.First(e => e != _target); otherHp = other.CurrentHp;
        await Play<RockGuard>(); await Play<ThunderPrep>();
        Check(hp + otherHp - _target.CurrentHp - other.CurrentHp == 10
            && hp > _target.CurrentHp && otherHp > other.CurrentHp && Power.State.RewardsThisTurn == 1,
            "EXP_random_defensive_attack_triggers_echo_and_actual_reward");
        await FreshExpansion();
        await Play<ThunderEcho>();
        other = _player.Creature.CombatState!.Enemies.First(e => e != _target); otherHp = other.CurrentHp;
        _target.SetCurrentHpInternal(7m);
        await Play<RockGuard>(); await Play<LightningThrust>();
        Check(_target.IsDead && other.CurrentHp == otherHp, "EXP_base_kill_does_not_trigger_echo");

        await FreshExpansion();
        await Play<ThunderEcho>();
        await PowerCmd.Apply<BufferPower>(new BlockingPlayerChoiceContext(), _target, 2m, _target, null);
        other = _player.Creature.CombatState!.Enemies.First(e => e != _target); otherHp = other.CurrentHp;
        hp = _target.CurrentHp;
        await Play<RockGuard>(); await Play<LightningThrust>();
        Check(_target.CurrentHp == hp && other.CurrentHp == otherHp && Power.State.RewardsThisTurn == 0,
            "EXP_zero_attack_damage_does_not_trigger_echo");

        await FreshExpansion(); await SeedDraw();
        await Play<ElementResonance>(); await Play<ElementResonance>(true);
        await Play<WindBlade>(); await Play<RockGuard>();
        var energy = _player.PlayerCombatState!.Energy;
        await Play<WindBlade>();
        Check(_player.PlayerCombatState.Energy == energy + 1 && Power.State.RewardsThisTurn == 2 && Power.State.Used == 2,
            "EXP_resonance_second_reward_grants_one_per_copy");
        energy = _player.PlayerCombatState.Energy;
        await Play<RockGuard>();
        Check(_player.PlayerCombatState.Energy == energy - 1 && Power.State.RewardsThisTurn == 3,
            "EXP_resonance_third_reward_does_not_repeat");
        energy = _player.PlayerCombatState.Energy;
        await Play<WindBlade>();
        Check(_player.PlayerCombatState.Energy == energy - 1, "EXP_resonance_cap_does_not_repeat");
        source = await Play<WindBlade>();
        await PowerCmd.Apply<MockExtraTurnPower>(source.PlayerChoiceContext!, _player.Creature, 1m, _player.Creature, null);
        await NextTurn(preserveElement: true);
        Check(Power.State.RewardsThisTurn == 0 && Power.State.Last == Element.Wind, "EXP_extra_turn_resets_actual_reward_counter");
        await Play<RockGuard>();
        energy = _player.PlayerCombatState.Energy;
        await Play<WindBlade>();
        Check(_player.PlayerCombatState.Energy == energy + 1, "EXP_resonance_can_trigger_on_extra_turn");

        await FreshExpansion();
        await Play<ElementResonance>();
        await Play<RockGuard>();
        // A deliberately blocked draw still consumes a combo but grants no actual reward.
        source = await Play<WindBlade>();
        await PowerCmd.Apply<NoDrawPower>(source.PlayerChoiceContext!, _player.Creature, 1m, _player.Creature, null);
        energy = _player.PlayerCombatState!.Energy;
        await Play<LightningThrust>();
        Check(Power.State.RewardsThisTurn == 1 && _player.PlayerCombatState.Energy == energy - 1,
            "EXP_resonance_skipped_reward_is_not_second");
        await PowerCmd.Remove<NoDrawPower>(_player.Creature);
        energy = _player.PlayerCombatState.Energy;
        await Play<RockGuard>();
        Check(Power.State.Used == 3 && Power.State.RewardsThisTurn == 2 && _player.PlayerCombatState.Energy == energy,
            "EXP_resonance_second_actual_after_skip_gains_energy");
        energy = _player.PlayerCombatState.Energy;
        await Play<WindBlade>();
        Check(_player.PlayerCombatState.Energy == energy - 1, "EXP_resonance_no_energy_after_skipped_reward");

        await FreshExpansion();
        await Play<ThunderEcho>(); await Play<ElementResonance>(); await RelicCmd.Obtain<JourneyCompass>(_player);
        foreach (var enemy in _player.Creature.CombatState!.Enemies.Where(e => e != _target).ToArray())
            await CreatureCmd.Kill(enemy, force: true);
        _target.SetCurrentHpInternal(10m);
        await Play<RockGuard>(); await Play<LightningThrust>();
        await Until(() => !CombatManager.Instance.IsInProgress, "expansion combo kills last enemy");
        Check(_target.IsDead && _player.GetRelic<JourneyCompass>() is { SpentThisTurn: false },
            "EXP_final_combo_kill_skips_followup_abilities_and_compass");
    }
}
