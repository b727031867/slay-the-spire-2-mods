using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Sts2Mod.Characters;
using Sts2Mod.Cards;
using Sts2Mod.Combo;
using Sts2Mod.Events;
using Sts2Mod.Relics;

namespace Sts2Mod.Debug;

internal sealed class EventCardSelector : ExpansionCardSelector
{
    public CardModel[] Candidates { get; private set; } = [];
    public override Task<IEnumerable<CardModel>> GetSelectedCards(IEnumerable<CardModel> options, int minSelect, int maxSelect)
    {
        Candidates = options.ToArray();
        return base.GetSelectedCards(Candidates, minSelect, maxSelect);
    }
}

public partial class TravelerTestRunner
{
    private async Task CompanionCombat()
    {
        if (CombatManager.Instance.IsInProgress)
        {
            await CreatureCmd.Kill(_player.Creature.CombatState!.Enemies.ToArray(), force: true);
            await CombatManager.Instance.CheckWinCondition();
            await Until(() => !CombatManager.Instance.IsInProgress, "companion previous combat cleanup");
        }
        var encounter = ModelDb.Encounter<CultistsNormal>().ToMutable();
        encounter.DebugRandomizeRng();
        await RunManager.Instance.EnterRoomDebug(RoomType.Monster, model: encounter);
        await Until(() => CombatManager.Instance.IsInProgress && ReadyToPlay, "companion combat ready");
        _target = _player.Creature.CombatState!.Enemies.First();
        _target.SetMaxHpInternal(5000); _target.SetCurrentHpInternal(5000);
    }

    private async Task TestCompanionEvents()
    {
        foreach (var character in new CharacterModel[] { ModelDb.Character<Ironclad>(), ModelDb.Character<Silent>(), ModelDb.Character<Regent>(), ModelDb.Character<Necrobinder>(), ModelDb.Character<Defect>() })
        {
            await NGame.Instance!.ReturnToMainMenu();
            var run = await NGame.Instance.StartNewSingleplayerRun(character, false,
                [ModelDb.Act<Overgrowth>(), ModelDb.Act<Hive>(), ModelDb.Act<Glory>()], [], "COMPANION17", GameMode.Standard);
            _player = run.Players[0];
            var tag = character.Id.Entry;
            var players = (List<Player>)AccessTools.Field(typeof(RunState), "_players").GetValue(run)!;
            var traveler = Player.CreateForNewRun(ModelDb.Character<Traveler>(), _player.UnlockState, 999);
            var actField = AccessTools.Field(typeof(RunState), "_currentActIndex");
            for (var act = 0; act < 3; act++)
            {
                actField.SetValue(run, act);
                Check(ModelDb.AllEvents.OfType<TravelerEvent>().All(e => !e.IsAllowed(run)), "EVENT_guest_no_traveler_" + tag + "_" + act);
                players.Add(traveler);
                try
                {
                    Check(ModelDb.Event<UnmappedPath>().IsAllowed(run) == (act < 2)
                        && ModelDb.Event<MisalignedSurvey>().IsAllowed(run) == (act < 2)
                        && ModelDb.Event<LetterFromOutside>().IsAllowed(run) == (act > 0)
                        && ModelDb.Event<LastGuide>().IsAllowed(run) == (act > 0), "EVENT_mixed_party_acts_" + tag + "_" + act);
                }
                finally { players.Remove(traveler); }
            }
            actField.SetValue(run, 0);
            _player.Creature.SetMaxHpInternal(5000); _player.Creature.SetCurrentHpInternal(4000);
            using var selector = CardSelectCmd.PushSelector(new EventCardSelector());
            // Exercise native rooms with the companion as the real local owner, without a network session.
            foreach (var model in ModelDb.AllEvents.OfType<TravelerEvent>())
            {
                await RunManager.Instance.EnterRoomDebug(RoomType.Event, model: model);
                var current = ((EventRoom)run.CurrentRoom!).LocalMutableEvent;
                Check(current.Owner == _player && !current.IsShared && current.Node is not null
                    && !current.Description!.GetFormattedText().Contains("INITIAL_GUEST.description"), "EVENT_guest_native_room_" + tag + "_" + model.Id.Entry);
                await current.CurrentOptions[model is LastGuide ? 0 : current.CurrentOptions.Count - 1].Chosen();
                Check(current.IsFinished, "EVENT_guest_native_finish_" + tag + "_" + model.Id.Entry);
            }
            var recorder = new EventCardSelector();
            using (CardSelectCmd.PushSelector(recorder))
            {
                var path = await EventFixture<UnmappedPath>();
                await path.CurrentOptions[0].Chosen(); await path.CurrentOptions[0].Chosen();
                var ownCards = character.CardPool.GetUnlockedCards(_player.UnlockState, 0).Select(c => c.Id).ToHashSet();
                Check(recorder.Candidates.Length == 3 && recorder.Candidates.All(c => ownCards.Contains(c.Id) && c.Owner == _player)
                    && traveler.Gold == traveler.Character.StartingGold, "EVENT_guest_path_own_pool_" + tag);
                await path.CurrentOptions[0].Chosen(); await path.CurrentOptions[1].Chosen();
                Check(path.IsFinished && recorder.Candidates.Length == 3 && recorder.Candidates.All(c => ownCards.Contains(c.Id) && c.Rarity == CardRarity.Rare)
                    && _player.Deck.Cards.Any(c => c is Homeward), "EVENT_guest_path_rare_curse_" + tag);
                await PlayerCmd.SetGold(60, _player);
                var survey = await EventFixture<MisalignedSurvey>();
                await survey.CurrentOptions[0].Chosen();
                var first = recorder.Candidates.First();
                Check(first.CurrentUpgradeLevel == 1 && CombatCardElements.Permanent.Get(first) == 0 && _player.Gold == 60,
                    "EVENT_guest_upgrade_free_" + tag);
                using (CardSelectCmd.PushSelector(new ExpansionCardSelector(0))) await survey.CurrentOptions[0].Chosen();
                Check(_player.Gold == 60 && !survey.IsFinished && survey.Description!.GetFormattedText() == survey.InitialDescription.GetFormattedText(),
                    "EVENT_guest_upgrade_cancel_no_charge_" + tag);
                var paid = survey.CurrentOptions[0];
                await paid.Chosen(); await paid.Chosen();
                Check(survey.IsFinished && _player.Gold == 0 && recorder.Candidates.First().CurrentUpgradeLevel == 1,
                    "EVENT_guest_upgrade_second_once_" + tag);
                survey = await EventFixture<MisalignedSurvey>();
                await survey.CurrentOptions[0].Chosen();
                Check(survey.CurrentOptions[0].IsLocked, "EVENT_guest_upgrade_insufficient_gold_" + tag);
                await survey.CurrentOptions[1].Chosen();
                foreach (var card in _player.Deck.Cards.Where(c => c.IsUpgradable).ToArray()) CardCmd.Upgrade(card);
                survey = await EventFixture<MisalignedSurvey>();
                Check(survey.CurrentOptions[0].IsLocked && !survey.CurrentOptions[1].IsLocked, "EVENT_guest_upgrade_empty_" + tag);
                await survey.CurrentOptions[1].Chosen();
            }
            foreach (var relic in _player.Relics.ToArray()) await RelicCmd.Remove(relic);
            var guide = await EventFixture<LastGuide>();
            var offered = guide.CurrentOptions.Select(o => o.Relic!).ToArray();
            var allowed = ModelDb.RelicPool<MegaCrit.Sts2.Core.Models.RelicPools.SharedRelicPool>().GetUnlockedRelics(_player.UnlockState)
                .Concat(character.RelicPool.GetUnlockedRelics(_player.UnlockState)).Select(r => r.Id).ToHashSet();
            Check(offered.Length == 3 && offered.Select(r => r.Id).Distinct().Count() == 3
                && offered.All(r => r.Rarity == RelicRarity.Common && allowed.Contains(r.Id) && r is not TravelerEventRelic), "EVENT_guest_guide_native_candidates_" + tag);
            var gold = _player.Gold; var hp = _player.Creature.CurrentHp;
            var choice = guide.CurrentOptions[0];
            await choice.Chosen(); await choice.Chosen();
            Check(guide.IsFinished && _player.Relics.Count == 1 && _player.Relics[0].Id == offered[0].Id
                && _player.Gold == gold && _player.Creature.CurrentHp == hp, "EVENT_guest_guide_free_once_" + tag);
            guide = await EventFixture<LastGuide>();
            Check(guide.CurrentOptions.All(o => o.Relic?.Id != offered[0].Id), "EVENT_guest_guide_no_owned_" + tag);
            foreach (var relic in _player.Relics.ToArray()) await RelicCmd.Remove(relic);
            for (var index = 0; index < 3; index++)
            {
                var letter = await EventFixture<LetterFromOutside>();
                await letter.CurrentOptions[index].Chosen();
                Check(letter.IsFinished && _player.Relics.Count == index + 1 && _player.Relics.All(r => r is CompanionLetter or CompanionAddress or UnclaimedMail),
                    "EVENT_guest_contract_choice_" + tag + "_" + index);
            }
            Check(_player.Relics.All(r => RelicModel.FromSerializable(r.ToSerializable()).Id == r.Id), "EVENT_guest_contract_serialization_" + tag);
            var beforeStart = _player.Creature.CurrentHp;
            await CompanionCombat();
            var address = _player.Relics.OfType<CompanionAddress>().Single();
            var letterRelic = _player.Relics.OfType<CompanionLetter>().Single();
            var mailed = PileType.Hand.GetPile(_player).Cards.Single(c => c.DeckVersion is null);
            Check(_player.Creature.CurrentHp == beforeStart - 2 && address.ModifyHandDraw(_player, 5) == 4,
                "EVENT_guest_contract_native_start_" + tag);
            Check(character.CardPool.GetUnlockedCards(_player.UnlockState, 0).Any(c => c.Id == mailed.Id)
                && mailed.Rarity is CardRarity.Common or CardRarity.Uncommon && mailed.CurrentUpgradeLevel == 0
                && !mailed.EnergyCost.CostsX && mailed.Keywords.Contains(CardKeyword.Ethereal)
                && mailed.EnergyCost.GetWithModifiers(CostModifiers.All) == 0 && mailed.CurrentStarCost <= 0 && CombatCardElements.Get(mailed) == Element.None,
                "EVENT_guest_address_own_temporary_free_" + tag);
            var context = new BlockingPlayerChoiceContext();
            var hpBefore = _player.Creature.CurrentHp;
            var handBefore = PileType.Hand.GetPile(_player).Cards.Count;
            await letterRelic.AfterPlayerTurnStartEarly(context, traveler);
            await address.AfterPlayerTurnStartEarly(context, traveler);
            Check(_player.Creature.CurrentHp == hpBefore && PileType.Hand.GetPile(_player).Cards.Count == handBefore
                && address.ModifyHandDraw(traveler, 5) == 5, "EVENT_guest_contract_ignores_other_owner_" + tag);
            await letterRelic.AfterSideTurnEnd(context, _player.Creature.Side, [_player.Creature]);
            Check(_player.Creature.CurrentHp == hpBefore, "EVENT_guest_letter_no_early_heal_" + tag);
            // Native end-turn actions prove the contract is wired into the real turn lifecycle.
            for (var turn = 1; turn <= 3; turn++)
            {
                var number = _player.PlayerCombatState!.TurnNumber;
                hpBefore = _player.Creature.CurrentHp;
                await CreatureCmd.GainBlock(_player.Creature, 999, MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered, null);
                await letterRelic.AfterSideTurnEnd(context, _player.Creature.Side, []);
                Check(_player.Creature.CurrentHp == hpBefore, "EVENT_guest_letter_no_other_participant_" + tag + "_" + turn);
                await Queue(new EndPlayerTurnAction(_player, number));
                await Until(() => _player.PlayerCombatState!.TurnNumber > number && ReadyToPlay, "companion next turn");
                Check(_player.Creature.CurrentHp == hpBefore + (turn == 3 ? 3 : 0), "EVENT_guest_letter_native_turn_" + tag + "_" + turn);
                if (turn == 1) Check(!PileType.Hand.GetPile(_player).Cards.Contains(mailed)
                    && mailed.EnergyCost.GetWithModifiers(CostModifiers.All) == mailed.EnergyCost.Canonical
                    && mailed.CurrentStarCost == mailed.CanonicalStarCost
                    && address.ModifyHandDraw(_player, 5) == 5, "EVENT_guest_address_exhaust_cost_reset_" + tag);
            }
            hpBefore = _player.Creature.CurrentHp;
            await letterRelic.AfterSideTurnEnd(context, _player.Creature.Side, [_player.Creature]);
            Check(_player.Creature.CurrentHp == hpBefore, "EVENT_guest_letter_no_repeat_" + tag);
            var reward = new CardReward(CardCreationOptions.ForRoom(_player, RoomType.Monster), 3, _player); reward.Populate();
            Check(reward.Cards.Count() == 4 && reward.Cards.All(c => character.CardPool.GetUnlockedCards(_player.UnlockState, 0).Any(own => own.Id == c.Id)),
                "EVENT_guest_mail_own_pool_four_" + tag);
            await PlayerCmd.SetGold(15, _player);
            var deckCount = _player.Deck.Cards.Count;
            Check(await reward.SelectUnsynchronized() && _player.Gold == 0 && _player.Deck.Cards.Count == deckCount + 1,
                "EVENT_guest_mail_pay15_" + tag);
            await CreatureCmd.Kill(_player.Creature.CombatState!.Enemies.ToArray(), force: true);
            await CombatManager.Instance.CheckWinCondition();
            await Until(() => !CombatManager.Instance.IsInProgress, "companion final cleanup");
        }
        GD.Print("STS2MOD_COMPANION_EVENTS_COMPLETE characters=5 rewards=owner_scoped multiplayer=NOT_RUN");
    }
}
