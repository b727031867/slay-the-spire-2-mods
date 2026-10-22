using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using Sts2Mod.Cards;
using Sts2Mod.Combo;
using Sts2Mod.Pools;
using Sts2Mod.Powers;
using Sts2Mod.Relics;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestCompassUpgrade()
    {
        await StartFreshCombat();
        var original=await RelicCmd.Obtain<JourneyCompass>(_player);
        var touch=(TouchOfOrobas)ModelDb.Relic<TouchOfOrobas>().ToMutable();
        Check(touch.SetupForPlayer(_player) && touch.StarterRelic==original.Id
            && touch.UpgradedRelic==ModelDb.Relic<UnboundJourneyCompass>().Id,
            "CHAR_upgrade_orobas_maps_compass_instead_of_circlet");
        Check(touch.DynamicEventDescription.GetFormattedText().Contains(ModelDb.Relic<UnboundJourneyCompass>().Title.GetFormattedText())
            && touch.HoverTips.Count()>1, "CHAR_upgrade_native_preview_names_real_upgrade");
        var savedTouch=(TouchOfOrobas)RelicModel.FromSerializable(touch.ToSerializable());
        Check(savedTouch.StarterRelic==original.Id && savedTouch.UpgradedRelic==ModelDb.Relic<UnboundJourneyCompass>().Id,
            "CHAR_upgrade_pending_reward_native_serialization");
        Check(touch.GetUpgradedStarterRelic(ModelDb.Relic<BurningBlood>()) is BlackBlood,
            "CHAR_upgrade_vanilla_starter_mapping_unchanged");
        await RelicCmd.Obtain(savedTouch,_player);
        var upgraded=_player.GetRelic<UnboundJourneyCompass>()!;
        Check(upgraded is not null && _player.GetRelic<JourneyCompass>() is null
            && !_player.Relics.OfType<Circlet>().Any(), "CHAR_upgrade_native_reward_replaces_original_once");
        Check(RelicModel.FromSerializable(upgraded!.ToSerializable()) is UnboundJourneyCompass loaded
            && loaded.Id==upgraded.Id && !loaded.ShowCounter,
            "CHAR_upgrade_saved_relic_roundtrip_no_fake_zero_counter");
        Check(ModelDb.RelicPool<TravelerRelicPool>().AllRelics.Count(r=>r is UnboundJourneyCompass)==1
            && !ModelDb.RelicPool<SharedRelicPool>().AllRelics.Any(r=>r is UnboundJourneyCompass)
            && !ModelDb.RelicPool<IroncladRelicPool>().AllRelics.Any(r=>r is UnboundJourneyCompass),
            "CHAR_upgrade_traveler_exclusive");

        var flashes=0;
        upgraded.Flashed+=(_,_)=>flashes++;
        await SeedDraw();
        await Play<WindBlade>();
        Check(flashes==0, "CHAR_upgrade_first_element_has_no_reward_draw");
        await Play<RockGuard>(); await Play<WindBlade>(); await Play<RockGuard>();
        Check(Power.State.Used==3 && PileType.Hand.GetPile(_player).Cards.Count==3 && flashes==4,
            "CHAR_upgrade_three_rewards_draw_three_same_turn");
        Check(_player.Creature.GetPower<StrengthPower>()?.Amount==1,
            "CHAR_upgrade_third_trigger_one_strength");
        await Play<WindBlade>();
        Check(Power.State.Used==4 && PileType.Hand.GetPile(_player).Cards.Count==4 && flashes==5,
            "CHAR_upgrade_fourth_combo_allowed");
        await Play<RockGuard>();
        Check(Power.State.Used==4 && PileType.Hand.GetPile(_player).Cards.Count==4 && flashes==5,
            "CHAR_upgrade_does_not_remove_shared_combo_cap");
        await NextTurn(preserveElement:true); await SeedDraw();
        await Play<WindBlade>();
        Check(PileType.Hand.GetPile(_player).Cards.Count==1 && flashes==6,
            "CHAR_upgrade_next_turn_has_no_cooldown");
        await RelicCmd.Obtain<PaimonCheer>(_player);
        await Play<RockGuard>();
        Check(PileType.Hand.GetPile(_player).Cards.Count==2 && flashes==7,
            "CHAR_upgrade_paimon_does_not_double_compass");
        Check(_player.Creature.GetPower<StrengthPower>()?.Amount==1,
            "CHAR_upgrade_two_triggers_no_extra_strength");
        await Play<WindBlade>(); await Play<RockGuard>();
        Check(_player.Creature.GetPower<StrengthPower>()?.Amount==2,
            "CHAR_upgrade_strength_each_turn_once_including_paimon");
        await RelicCmd.Remove(_player.GetRelic<PaimonCheer>()!);

        await StartFreshCombat(); await SeedDraw();
        Check(_player.Creature.GetPower<StrengthPower>() is null,
            "CHAR_upgrade_new_combat_clears_strength");
        var source=await Play<WindBlade>();
        await PowerCmd.Apply<NoDrawPower>(source.PlayerChoiceContext!,_player.Creature,1m,_player.Creature,null);
        var before=flashes;
        await Play<LightningThrust>();
        Check(flashes==before && PileType.Hand.GetPile(_player).Cards.Count==0,
            "CHAR_upgrade_zero_combo_reward_does_not_trigger");
        await Play<RockGuard>();
        Check(flashes==before+1 && PileType.Hand.GetPile(_player).Cards.Count==0,
            "CHAR_upgrade_blocked_own_draw_is_not_a_new_combo");
        await PowerCmd.Remove<NoDrawPower>(_player.Creature);
        await Play<WindBlade>();
        Check(flashes==before+3 && PileType.Hand.GetPile(_player).Cards.Count==1,
            "CHAR_upgrade_draws_again_after_draw_prevention");
        Check(Power.State.Used==3 && Power.State.RewardsThisTurn==2
            && _player.Creature.GetPower<StrengthPower>()?.Amount==1,
            "CHAR_upgrade_skipped_reward_still_counts_trigger");

        await StartFreshCombat(); await SeedDraw();
        var infusion=await PowerCmd.Apply<TriphaseInfusionPower>(ExpansionContext(),_player.Creature,1,_player.Creature,null);
        var expected=NextExpansionElement();
        var first=expected==Element.Wind ? Element.Rock : Element.Wind;
        Power.State.Reserve(new object(),first);
        Power.State.Reserve(new object(),first==Element.Wind ? Element.Rock : Element.Wind);
        Power.State.Reserve(new object(),first);
        await infusion!.Trigger(ExpansionContext(),await Add<TriphaseInfusion>(),null);
        Check(Power.State.Used==3 && _player.Creature.GetPower<StrengthPower>()?.Amount==1,
            "CHAR_upgrade_infusion_third_trigger_strength");
        await upgraded.OnComboTriggered(ExpansionContext());
        Check(_player.Creature.GetPower<StrengthPower>()?.Amount==1,
            "CHAR_upgrade_repeated_callback_no_extra_strength");
        await PowerCmd.Remove<TriphaseInfusionPower>(_player.Creature);
        await RelicCmd.Remove(upgraded);
        await RelicCmd.Remove(savedTouch);
    }
}
