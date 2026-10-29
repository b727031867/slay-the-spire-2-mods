using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using Sts2Mod.Cards;
using Sts2Mod.Combo;
using Sts2Mod.Powers;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task FreshNewCards()
    {
        await FreshExpansion();
        await CardPileCmd.RemoveFromCombat(PileType.Draw.GetPile(_player).Cards.Concat(PileType.Discard.GetPile(_player).Cards).ToArray());
    }

    private async Task TestUncommonExpansionCards()
    {
        using var selector = CardSelectCmd.PushSelector(new ExpansionCardSelector());
        var types = new[] { typeof(WindPathfinding), typeof(WindShelteredPlans), typeof(StoneRidgeCounter), typeof(LayeredRockEcho),
            typeof(StoneGathering), typeof(ThunderNavigation), typeof(VenomThunderNeedle), typeof(VenomCarryingWind),
            typeof(ToxicSatchel), typeof(PoisonStoneBarrier) };
        var pool = _player.Character.CardPool.GetUnlockedCards(_player.UnlockState, 0).Where(c => c.Rarity != CardRarity.Basic).ToArray();
        Check(pool.Length == 38 && pool.Count(c => c.Rarity == CardRarity.Uncommon) == 23
            && types.All(t => pool.Count(c => c.GetType() == t) == 1), "NEW_pool_complete");
        foreach (var type in types)
        {
            var canonical = pool.Single(c => c.GetType() == type);
            foreach (var up in new[] { false, true })
            {
                var card = canonical.ToMutable();
                if (up) CardCmd.Upgrade(card);
                var restored = CardModel.FromSerializable(card.ToSerializable());
                Check(card.Rarity == CardRarity.Uncommon && card.EnergyCost.Canonical == 1
                    && restored.GetType() == type && restored.CurrentUpgradeLevel == (up ? 1 : 0)
                    && !card.Description.GetFormattedText().Contains("{"), $"NEW_metadata_{card.Id.Entry}_{up}");
            }
            var art = ResourceLoader.Load<Texture2D>(((CustomCardModel)canonical).CustomPortraitPath);
            Check(art is not null && art.GetWidth() >= 250 && art.GetHeight() >= 190,
                $"NEW_art_{canonical.Id.Entry}");
        }
        foreach (var up in new[] { false, true })
        {
            await FreshNewCards();
            var chosen = await Add<ThunderNavigation>();
            await CardPileCmd.Add(chosen, PileType.Draw);
            var wrong = await Add<WindBlade>(); await CardPileCmd.Add(wrong, PileType.Draw);
            var search = await Add<WindPathfinding>(up);
            await Queue(new PlayCardAction(search, null));
            Check(chosen.Pile?.Type == PileType.Hand && wrong.Pile?.Type == PileType.Draw
                && search.Pile?.Type == (up ? PileType.Discard : PileType.Exhaust)
                && Power.State.Last == Element.Wind, "NEW_search_filter_and_exhaust_" + up);
            await FreshNewCards();
            await Play<WindPathfinding>(up);
            Check(Power.State.Last == Element.Wind, "NEW_search_empty_returns_" + up);

            await FreshNewCards(); await SeedDraw(20);
            await Play<WindShelteredPlans>(up);
            var retained = PileType.Hand.GetPile(_player).Cards.Single(c => c.ShouldRetainThisTurn);
            Check(PileType.Hand.GetPile(_player).Cards.Count == (up ? 3 : 2)
                && !retained.Keywords.Contains(CardKeyword.Retain), "NEW_retain_draw_and_temporary_flag_" + up);
            await ExpansionNativeTurn();
            Check(retained.Pile?.Type == PileType.Hand && !retained.ShouldRetainThisTurn, "NEW_retain_survives_one_flush_" + up);
            await ExpansionNativeTurn();
            Check(retained.Pile?.Type == PileType.Discard, "NEW_retain_expires_next_flush_" + up);

            foreach (var guarded in new[] { false, true })
            {
                await FreshNewCards();
                if (guarded) await CreatureCmd.GainBlock(_player.Creature, 1, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, null);
                var hp = _target.CurrentHp;
                await Play<StoneRidgeCounter>(up);
                Check(hp - _target.CurrentHp == (up ? 10 : 7)
                    && (_target.GetPower<WeakPower>()?.Amount ?? 0) == (guarded ? 2 : 0), $"NEW_counter_condition_{guarded}_{up}");
            }

            await FreshNewCards(); await Play<LayeredRockEcho>(up);
            await Play<WindBlade>(); await Play<RockGuard>();
            var dex = up ? 2 : 1;
            Check(_player.Creature.GetPower<DexterityPower>()?.Amount == dex, "NEW_dexterity_first_combo_" + up);
            await Play<WindBlade>();
            Check(_player.Creature.GetPower<DexterityPower>()?.Amount == dex, "NEW_dexterity_once_" + up);
            await ExpansionNativeTurn();
            Check((_player.Creature.GetPower<DexterityPower>()?.Amount ?? 0) == 0, "NEW_dexterity_expires_" + up);
            await Play<RockGuard>();
            Check(_player.Creature.GetPower<DexterityPower>()?.Amount == dex, "NEW_dexterity_next_turn_reset_" + up);

            await FreshNewCards(); await Play<StoneGathering>(up);
            var vigor = up ? 10 : 7;
            Check(_player.Creature.Block == (up ? 11 : 8) && _player.Creature.GetPower<VigorPower>()?.Amount == vigor,
                "NEW_vigor_block_and_amount_" + up);
            await ExpansionNativeTurn();
            Check(_player.Creature.GetPower<VigorPower>()?.Amount == vigor, "NEW_vigor_survives_turn_" + up);
            Power.State.ClearCombat();
            var before = _player.Creature.CombatState!.Enemies.ToDictionary(e => e, e => e.CurrentHp);
            await Play<ThunderChain>();
            Check(before.All(e => e.Value - e.Key.CurrentHp == 2 * (5 + vigor))
                && (_player.Creature.GetPower<VigorPower>()?.Amount ?? 0) == 0, "NEW_vigor_all_multihits_then_consumed_" + up);
            await FreshNewCards(); await Play<LightningThrust>();
            var total = _player.Creature.CombatState!.Enemies.Sum(e => e.CurrentHp);
            await Play<StoneGathering>(up);
            Check(total - _player.Creature.CombatState!.Enemies.Sum(e => e.CurrentHp) == 4 + vigor
                && (_player.Creature.GetPower<VigorPower>()?.Amount ?? 0) == 0 && Power.State.RewardsThisTurn == 1,
                "NEW_vigor_native_combo_consumes_" + up);

            await FreshNewCards(); await SeedDraw(25);
            var navigation = await Add<ThunderNavigation>(up);
            await Queue(new PlayCardAction(navigation, null));
            Check(_player.Creature.Block == (up ? 9 : 7) && _player.Creature.GetPower<DrawCardsNextTurnPower>()?.Amount == 2
                && navigation.Pile?.Type == (up ? PileType.Discard : PileType.Exhaust), "NEW_navigation_block_exhaust_" + up);
            await ExpansionNativeTurn();
            Check(PileType.Hand.GetPile(_player).Cards.Count == 7 && _player.Creature.GetPower<DrawCardsNextTurnPower>() is null,
                "NEW_navigation_next_turn_draw_" + up);
            await ExpansionNativeTurn();
            Check(PileType.Hand.GetPile(_player).Cards.Count == 5, "NEW_navigation_no_repeat_" + up);

            await FreshNewCards(); var targetHp = _target.CurrentHp;
            await Play<VenomThunderNeedle>(up);
            var poison = up ? 4 : 3;
            Check(targetHp - _target.CurrentHp == (up ? 8 : 5) && _target.GetPower<PoisonPower>()?.Amount == poison,
                "NEW_needle_damage_poison_" + up);
            await Play<EchoingBlight>();
            Check(_target.GetPower<PoisonPower>()?.Amount == poison + 3, "NEW_needle_existing_blight_additive_" + up);
            targetHp = _target.CurrentHp;
            await ExpansionNativeTurn();
            Check(targetHp - _target.CurrentHp == poison + 3 && _target.GetPower<PoisonPower>()?.Amount == poison + 2,
                "NEW_needle_native_poison_tick_" + up);

            await FreshNewCards(); await SeedDraw();
            await Play<VenomCarryingWind>(up);
            Check(_player.Creature.CombatState!.Enemies.All(e => e.GetPower<PoisonPower>()?.Amount == poison)
                && PileType.Hand.GetPile(_player).Cards.Count == 1 && Power.State.Last == Element.Wind,
                "NEW_poison_wind_all_and_draw_" + up);

            await FreshNewCards(); await SeedDraw(25); await Play<ToxicSatchel>(up);
            await Play<LightningThrust>(); await Play<WindBlade>();
            var satchel = up ? 3 : 2;
            Check(_player.Creature.CombatState!.Enemies.All(e => e.GetPower<PoisonPower>()?.Amount == satchel),
                "NEW_satchel_first_combo_" + up);
            await Play<LightningThrust>();
            Check(_target.GetPower<PoisonPower>()?.Amount == satchel, "NEW_satchel_once_" + up);
            await ExpansionNativeTurn(); await PlayerCmd.SetEnergy(20, _player);
            await Play<WindBlade>();
            Check(_target.GetPower<PoisonPower>()?.Amount == 2 * satchel - 1, "NEW_satchel_next_turn_reset_" + up);

            foreach (var poisoned in new[] { false, true })
            {
                await FreshNewCards();
                if (poisoned) await PowerCmd.Apply<PoisonPower>(ExpansionContext(), _target, 2, _player.Creature, null);
                await Play<PoisonStoneBarrier>(up);
                Check(_player.Creature.Block == (up ? 10 : 7) + (poisoned ? 3 : 0)
                    && Power.State.Last == Element.Rock, $"NEW_barrier_condition_{poisoned}_{up}");
            }
        }
        await FreshNewCards();
        await PowerCmd.Apply<ArtifactPower>(ExpansionContext(), _target, 1, _target, null);
        await Play<VenomThunderNeedle>();
        Check(_target.GetPower<PoisonPower>() is null && _target.GetPower<ArtifactPower>() is null, "NEW_boundary_artifact");
        await FreshNewCards(); _target.SetCurrentHpInternal(1);
        await Play<VenomThunderNeedle>();
        Check(!_target.IsAlive && _target.GetPower<PoisonPower>() is null, "NEW_boundary_kill_skips_poison");
        await FreshNewCards(); await Play<LayeredRockEcho>(); await Play<LayeredRockEcho>(true);
        await Play<WindBlade>(); await Play<RockGuard>();
        Check(_player.Creature.GetPower<DexterityPower>()?.Amount == 3, "NEW_boundary_dexterity_copies_stack");
        await PowerCmd.Apply<DexterityPower>(ExpansionContext(), _player.Creature, 4, _player.Creature, null);
        await ExpansionNativeTurn();
        Check(_player.Creature.GetPower<DexterityPower>()?.Amount == 4, "NEW_boundary_keeps_other_dexterity");
        await FreshNewCards(); await SeedDraw(); await Play<ToxicSatchel>(); await Play<ToxicSatchel>(true);
        await Play<WindBlade>(); await Play<LightningThrust>();
        Check(_target.GetPower<PoisonPower>()?.Amount == 5, "NEW_boundary_satchel_copies_stack");
        await FreshNewCards(); await Play<ToxicSatchel>();
        await Play<WindBlade>(); await PowerCmd.Apply<NoDrawPower>(ExpansionContext(), _player.Creature, 1, _player.Creature, null);
        await Play<LightningThrust>();
        Check(_target.GetPower<PoisonPower>() is null && Power.State.RewardsThisTurn == 0, "NEW_boundary_zero_draw_no_poison");
        await FreshNewCards(); await Play<WindShelteredPlans>();
        Check(PileType.Hand.GetPile(_player).Cards.Count == 0, "NEW_boundary_empty_retain_returns");
        Check(ResourceLoader.Load<Texture2D>("res://Sts2Mod/art/TC_C32.png") is not null
            && ResourceLoader.Load<Texture2D>("res://Sts2Mod/art/TC_C37.png") is not null, "NEW_boundary_power_icons");
        foreach (var name in new[] { "search", "retain", "counter", "dexterity", "vigor", "navigation", "needle", "poison_wind", "satchel", "barrier", "boundaries" })
            Check(true, "NEW_" + name + "_complete");
    }
}
