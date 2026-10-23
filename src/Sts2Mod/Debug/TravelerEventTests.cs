using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Sts2Mod.Cards;
using Sts2Mod.Combo;
using Sts2Mod.Events;
using Sts2Mod.Relics;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task<T> EventFixture<T>() where T : EventModel
    {
        var model = (T)ModelDb.Event<T>().ToMutable();
        await model.BeginEvent(_player, false);
        Check(model.CurrentOptions.Count > 0 && model.Description is not null, "EVENT_begin_" + model.Id.Entry);
        return model;
    }

    private async Task TestTravelerEvents()
    {
        var start = _assertions;
        using var selector = CardSelectCmd.PushSelector(new ExpansionCardSelector());
        foreach (var relic in _player.Relics.Where(r => r is not JourneyCompass).ToArray()) await RelicCmd.Remove(relic);
        foreach (var model in ModelDb.AllEvents.OfType<TravelerEvent>())
        {
            Check(!model.Title.GetFormattedText().Contains(".title"), "EVENT_title_" + model.Id.Entry);
            Check(ResourceLoader.Exists(model.CustomInitialPortraitPath), "EVENT_portrait_" + model.Id.Entry);
            Check(model.IsAllowed(_player.RunState) == (model is UnmappedPath or MisalignedSurvey), "EVENT_act1_filter_" + model.Id.Entry);
        }
        foreach (var type in new[] { typeof(WallPiercingNail), typeof(EchoSpool), typeof(ReverseWatch), typeof(OutsideLetter), typeof(AddressCorrection), typeof(UnclaimedMail) })
        {
            var relic = ModelDb.AllRelics.Single(r => r.GetType() == type);
            Check(RelicModel.FromSerializable(relic.ToMutable().ToSerializable()).GetType() == type, "EVENT_relic_save_" + relic.Id.Entry);
            Check(!new MegaCrit.Sts2.Core.Localization.LocString("relics", relic.Id.Entry + ".description").GetFormattedText().Contains(".description"), "EVENT_relic_text_" + relic.Id.Entry);
        }
        var actField = AccessTools.Field(typeof(RunState), "_currentActIndex");
        var oldAct = _player.RunState.CurrentActIndex;
        try
        {
            for (var act = 0; act < 3; act++)
            {
                actField.SetValue(_player.RunState, act);
                Check(ModelDb.Event<UnmappedPath>().IsAllowed(_player.RunState) == (act < 2)
                    && ModelDb.Event<MisalignedSurvey>().IsAllowed(_player.RunState) == (act < 2)
                    && ModelDb.Event<LetterFromOutside>().IsAllowed(_player.RunState) == (act > 0)
                    && ModelDb.Event<LastGuide>().IsAllowed(_player.RunState) == (act > 0), "EVENT_act_range_" + act);
            }
        }
        finally { actField.SetValue(_player.RunState, oldAct); }
        var curse = _player.RunState.CreateCard<Homeward>(_player);
        Check(curse.Type == CardType.Curse && curse.Keywords.SetEquals([CardKeyword.Unplayable]) && !MisalignedSurvey.Eligible(curse), "EVENT_curse_only_unplayable_excluded_from_survey");
        Check(!TravelerContent.RewardCardTypes.Contains(typeof(Homeward)), "EVENT_curse_not_normal_reward");

        _player.Creature.SetCurrentHpInternal(5);
        var low = await EventFixture<UnmappedPath>();
        Check(low.CurrentOptions[0].IsLocked, "EVENT_low_hp_entry_locked");
        await low.CurrentOptions[1].Chosen();
        _player.Creature.SetCurrentHpInternal(100);
        await PlayerCmd.SetGold(100, _player);
        var path = await EventFixture<UnmappedPath>();
        var entry = path.CurrentOptions[0];
        await entry.Chosen(); await entry.Chosen();
        Check(_player.Creature.CurrentHp == 95 && _player.Gold == 135, "EVENT_depth1_exact_once");
        await path.CurrentOptions[1].Chosen();
        Check(path.IsFinished && _player.Gold == 135, "EVENT_leave_keeps_rewards");
        _player.Creature.SetCurrentHpInternal(100);
        await PlayerCmd.SetGold(100, _player);
        var count = _player.Deck.Cards.Count;
        path = await EventFixture<UnmappedPath>();
        await path.CurrentOptions[0].Chosen(); await path.CurrentOptions[0].Chosen();
        Check(_player.Creature.CurrentHp == 88 && _player.Deck.Cards.Count == count + 1, "EVENT_depth2_cost_card");
        await path.CurrentOptions[0].Chosen();
        Check(_player.Deck.Cards.Count == count + 2 && _player.Deck.Cards.Last() is Homeward, "EVENT_depth3_curse_added");
        await path.CurrentOptions[0].Chosen();
        Check(path.IsFinished && _player.Creature.CurrentHp == 100 && _player.Gold == 195, "EVENT_supplies_cumulative");
        path = await EventFixture<UnmappedPath>();
        await path.CurrentOptions[0].Chosen(); await path.CurrentOptions[0].Chosen(); await path.CurrentOptions[0].Chosen();
        count = _player.Deck.Cards.Count;
        await path.CurrentOptions[1].Chosen();
        Check(path.IsFinished && _player.Deck.Cards.Count == count + 1 && _player.Deck.Cards.Last().Rarity == CardRarity.Rare, "EVENT_rare_branch");

        var original = _player.Deck.Cards.First(MisalignedSurvey.Eligible);
        var originalCost = original.EnergyCost.Canonical;
        var upgraded = original.CurrentUpgradeLevel;
        await PlayerCmd.SetGold(60, _player);
        var survey = await EventFixture<MisalignedSurvey>();
        await survey.CurrentOptions[0].Chosen(); await survey.CurrentOptions[0].Chosen();
        Check(CombatCardElements.Get(original) == Element.Wind && _player.Gold == 60 && !MisalignedSurvey.Eligible(original), "EVENT_survey_first_free");
        Check(original.CurrentUpgradeLevel == upgraded && original.EnergyCost.Canonical == originalCost, "EVENT_survey_preserves_card");
        var roundtrip = CardModel.FromSerializable(original.ToSerializable());
        Check(CombatCardElements.Get(roundtrip) == Element.Wind && CombatCardElements.Get((CardModel)original.ClonePreservingMutability()) == Element.Wind, "EVENT_permanent_element_serialization_clone");
        await survey.CurrentOptions[0].Chosen(); await survey.CurrentOptions[1].Chosen();
        Check(survey.IsFinished && _player.Gold == 0 && _player.Deck.Cards.Count(c => CombatCardElements.Permanent.Get(c) != 0) == 2, "EVENT_survey_second_cost60");
        survey = await EventFixture<MisalignedSurvey>();
        await survey.CurrentOptions[0].Chosen(); await survey.CurrentOptions[2].Chosen();
        Check(survey.CurrentOptions[0].IsLocked, "EVENT_survey_second_insufficient_gold");
        await survey.CurrentOptions[1].Chosen();

        for (var i = 0; i < 3; i++)
        {
            var letter = await EventFixture<LetterFromOutside>();
            await letter.CurrentOptions[i].Chosen();
            Check(letter.IsFinished && _player.Relics.Any(r => r.Id.Entry == "TC_R" + (11 + i)), "EVENT_contract_choice_" + i);
        }
        var reject = await EventFixture<LetterFromOutside>();
        count = _player.Relics.Count;
        await reject.CurrentOptions[3].Chosen();
        Check(reject.IsFinished && _player.Relics.Count == count, "EVENT_contract_decline");
        for (var i = 0; i < 3; i++)
        {
            var guide = await EventFixture<LastGuide>();
            var gold = _player.Gold;
            var hp = _player.Creature.CurrentHp;
            await guide.CurrentOptions[i].Chosen();
            Check(guide.IsFinished && _player.Relics.Any(r => r.Id.Entry == "TC_R" + (8 + i).ToString("00"))
                && _player.Gold == gold && _player.Creature.CurrentHp == hp, "EVENT_guide_free_choice_" + i);
        }

        if (!_player.Relics.Any(r => r is JourneyCompass or UnboundJourneyCompass)) await RelicCmd.Obtain<JourneyCompass>(_player);
        var hpBeforeStart = _player.Creature.CurrentHp;
        await StartFreshCombat();
        Check(_player.Creature.CurrentHp == hpBeforeStart - 2, "EVENT_letter_native_start_cost2");
        Check(Power.State.TurnLimit == 5, "EVENT_watch_compass_limit5");
        var permanent = new[] { PileType.Hand, PileType.Draw, PileType.Discard, PileType.Exhaust }.SelectMany(p => p.GetPile(_player).Cards).First(c => c.DeckVersion == original);
        Check(CombatCardElements.Get(permanent) == Element.Wind, "EVENT_deck_element_enters_combat");
        var context = new BlockingPlayerChoiceContext();
        var address = _player.Relics.OfType<AddressCorrection>().Single();
        Check(address.ModifyHandDraw(_player, 5) == 4, "EVENT_address_first_turn_draw_minus1");
        await address.AfterPlayerTurnStartEarly(context, _player);
        var mailed = PileType.Hand.GetPile(_player).Cards.Single();
        Check(TravelerContent.RewardCardTypes.Contains(mailed.GetType()) && mailed.CurrentUpgradeLevel == 0
            && CombatCardElements.Get(mailed) != Element.None && mailed.Keywords.Contains(CardKeyword.Ethereal)
            && mailed.EnergyCost.GetWithModifiers(CostModifiers.All) == 0 && mailed.DeckVersion is null, "EVENT_address_elemental_temporary_ethereal_free");
        await Queue(new MegaCrit.Sts2.Core.GameActions.PlayCardAction(mailed, mailed.TargetType == TargetType.AnyEnemy ? _target : null));
        Check(mailed.Keywords.Contains(CardKeyword.Ethereal), "EVENT_address_play_keeps_ethereal");
        await NextTurn();
        Check(mailed.EnergyCost.GetWithModifiers(CostModifiers.All) == mailed.EnergyCost.Canonical && address.ModifyHandDraw(_player, 5) == 5,
            "EVENT_address_cost_resets_later_turn_draw_normal");
        await _player.Relics.OfType<OutsideLetter>().Single().BeforeCombatStart();
        await _player.Relics.OfType<EchoSpool>().Single().BeforeCombatStart();
        await _player.Relics.OfType<WallPiercingNail>().Single().BeforeCombatStart();
        _player.Creature.SetCurrentHpInternal(100);
        var outside = _player.Relics.OfType<OutsideLetter>().Single();
        var before = _player.Creature.CurrentHp;
        await outside.OnReward(context, ComboKind.WindRock); await outside.OnReward(context, ComboKind.WindRock);
        await outside.OnReward(context, ComboKind.RockLightning);
        Check(_player.Creature.CurrentHp == before, "EVENT_letter_requires_three_distinct");
        await outside.OnReward(context, ComboKind.LightningWind);
        Check(_player.Creature.CurrentHp == before + 3, "EVENT_letter_heal3");
        await outside.OnReward(context, ComboKind.LightningWind);
        Check(_player.Creature.CurrentHp == before + 3, "EVENT_letter_once_per_combat");
        var spool = _player.Relics.OfType<EchoSpool>().Single();
        var weakBefore = _player.Creature.CombatState!.Enemies.Sum(e => e.GetPowerAmount<WeakPower>());
        await spool.OnReward(context, ComboKind.WindRock);
        var weak = _player.Creature.CombatState!.Enemies.Sum(e => e.GetPowerAmount<WeakPower>());
        await spool.OnReward(context, ComboKind.WindRock);
        Check(weak == weakBefore + 1 && _player.Creature.CombatState.Enemies.Sum(e => e.GetPowerAmount<WeakPower>()) == weak, "EVENT_spool_once_per_turn");
        var nail = _player.Relics.OfType<WallPiercingNail>().Single();
        var vulnerable = _target.GetPowerAmount<VulnerablePower>();
        await nail.OnHit(context, _target); await nail.OnHit(context, _target);
        Check(_target.GetPowerAmount<VulnerablePower>() == vulnerable + 1, "EVENT_nail_once_per_turn");
        await RelicCmd.Remove(_player.Relics.Single(r => r is JourneyCompass or UnboundJourneyCompass));
        Power.RefreshLimit();
        Check(Power.State.TurnLimit == 4, "EVENT_watch_without_compass_limit4");
        await NextTurn();
        Check(Power.State.TurnLimit == 4, "EVENT_watch_not_accumulating");

        var reward = new CardReward(CardCreationOptions.ForRoom(_player, RoomType.Monster), 3, _player);
        reward.Populate(); reward.Populate();
        Check(reward.Cards.Count() == 4, "EVENT_mail_four_candidates_idempotent");
        var saved = Reward.FromSerializable(reward.ToSerializable(), _player) as CardReward;
        saved!.Populate();
        Check(saved.Cards.Count() == 4, "EVENT_mail_reward_restore_no_growth");
        var elite = new CardReward(CardCreationOptions.ForRoom(_player, RoomType.Elite), 3, _player); elite.Populate();
        Check(elite.Cards.Count() == 3 && !UnclaimedMailRewards.Applies(elite), "EVENT_mail_excludes_elite");
        await PlayerCmd.SetGold(14, _player);
        count = _player.Deck.Cards.Count;
        Check(!await reward.SelectUnsynchronized() && _player.Deck.Cards.Count == count && _player.Gold == 14, "EVENT_mail_insufficient_gold_no_card");
        await PlayerCmd.SetGold(15, _player);
        Check(await reward.SelectUnsynchronized() && _player.Gold == 0 && _player.Deck.Cards.Count == count + 1, "EVENT_mail_pay15_take_one");
        reward.OnSkipped();
        Check(_player.Gold == 0, "EVENT_mail_no_skip_after_claim");
        await UnclaimedMailRewards.Skip(saved); await UnclaimedMailRewards.Skip(saved);
        Check(_player.Gold == 10, "EVENT_mail_skip10_once");
        foreach (var relic in _player.Relics.OfType<TravelerEventRelic>().ToArray()) await RelicCmd.Remove(relic);
        await CreatureCmd.Kill(_player.Creature.CombatState!.Enemies.ToArray(), force: true);
        await MegaCrit.Sts2.Core.Combat.CombatManager.Instance.CheckWinCondition();
        await Until(() => !MegaCrit.Sts2.Core.Combat.CombatManager.Instance.IsInProgress, "event room fixture combat cleanup");
        foreach (var model in ModelDb.AllEvents.OfType<TravelerEvent>())
        {
            await RunManager.Instance.EnterRoomDebug(RoomType.Event, model: model);
            var room = _player.RunState.CurrentRoom as EventRoom;
            Check(room is not null && room.LocalMutableEvent.Node is not null && room.LocalMutableEvent.CurrentOptions.Count > 0,
                "EVENT_native_room_" + model.Id.Entry);
            var current = room!.LocalMutableEvent;
            await current.CurrentOptions[model is LastGuide ? 0 : current.CurrentOptions.Count - 1].Chosen();
            Check(current.IsFinished, "EVENT_native_room_finish_" + model.Id.Entry);
        }
        foreach (var relic in _player.Relics.OfType<TravelerEventRelic>().ToArray()) await RelicCmd.Remove(relic);
        await StartFreshCombat();
        GD.Print($"STS2MOD_EVENTS_COMPLETE assertions={_assertions - start} logic=PASS rendering=NOT_RUN multiplayer=NOT_RUN");
    }
}
