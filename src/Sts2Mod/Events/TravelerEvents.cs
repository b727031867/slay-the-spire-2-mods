using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Cards;
using Sts2Mod.Characters;
using Sts2Mod.Combo;
using Sts2Mod.Relics;

namespace Sts2Mod.Events;

public abstract class TravelerEvent : CustomEventModel
{
    protected abstract bool EarlyActs { get; }
    public override bool IsAllowed(IRunState run) => run.Players.Any(p => p.Character is Traveler)
        && run.CurrentActIndex >= (EarlyActs ? 0 : 1) && run.CurrentActIndex <= (EarlyActs ? 1 : 2);
    public override string CustomInitialPortraitPath => $"res://Sts2Mod/art/events/{Id.Entry}.svg";
    public override LocString InitialDescription => Owner is not null && !IsTraveler
        ? new LocString("events", $"{Id.Entry}.pages.INITIAL_GUEST.description") : base.InitialDescription;
    protected EventOption Pick(string key, Func<Task>? chosen) => new(this, chosen, $"{Id.Entry}.pages.INITIAL.options.{key}");
    protected void Page(string key, params EventOption[] options) => SetEventState(key == "INITIAL" ? InitialDescription : PageDescription(key), options);
    protected Task Leave() { SetEventFinished(PageDescription("LEAVE")); return Task.CompletedTask; }
    protected bool IsTraveler => Owner!.Character is Traveler;
    protected async Task Give<T>() where T : RelicModel
    {
        await RelicCmd.Obtain<T>(Owner!);
        SetEventFinished(PageDescription("DONE"));
    }
}

[CustomID("TC_E01")]
public sealed class UnmappedPath : TravelerEvent
{
    protected override bool EarlyActs => true;
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
        [Pick("ENTER", Owner!.Creature.CurrentHp > 5 ? First : null), Pick("LEAVE", Leave)];
    private async Task Hurt(int amount) => await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner!.Creature,
        amount, ValueProp.Unblockable | ValueProp.Unpowered, null, null);
    private async Task First()
    {
        await Hurt(5);
        if (!Owner!.Creature.IsAlive) { await Leave(); return; }
        await PlayerCmd.GainGold(35, Owner);
        Page("CAMP", Pick(IsTraveler ? "DEEPER" : "DEEPER_GUEST", Owner.Creature.CurrentHp > 7 ? Second : null), Pick("LEAVE", Leave));
    }
    private async Task OfferCards(bool rare)
    {
        var pool = Owner!.Character.CardPool.GetUnlockedCards(Owner.UnlockState, 0)
            .Where(c => c.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare && (!rare || c.Rarity == CardRarity.Rare)).ToList();
        var choices = new List<CardModel>();
        for (var i = 0; i < 3 && pool.Count > 0; i++)
        {
            var canonical = Rng.NextItem(pool)!;
            pool.Remove(canonical);
            choices.Add(Owner!.RunState.CreateCard(canonical, Owner));
        }
        var selected = await CardSelectCmd.FromChooseACardScreen(new BlockingPlayerChoiceContext(), choices, Owner!, canSkip: true);
        if (selected is not null) CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(selected, PileType.Deck));
    }
    private async Task Second()
    {
        await Hurt(7);
        if (!Owner!.Creature.IsAlive) { await Leave(); return; }
        await OfferCards(false);
        Page("DOOR", Pick(IsTraveler ? "CURSE" : "CURSE_GUEST", Third), Pick("LEAVE", Leave));
    }
    private async Task Third()
    {
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(Owner!.RunState.CreateCard<Homeward>(Owner), PileType.Deck));
        Page("TREASURE", Pick("SUPPLIES", Supplies), Pick(IsTraveler ? "RARE" : "RARE_GUEST", Rare));
    }
    private async Task Supplies()
    {
        await PlayerCmd.GainGold(60, Owner!);
        await CreatureCmd.Heal(Owner!.Creature, 12);
        SetEventFinished(PageDescription("DONE"));
    }
    private async Task Rare() { await OfferCards(true); SetEventFinished(PageDescription("DONE")); }
}

[CustomID("TC_E02")]
public sealed class MisalignedSurvey : TravelerEvent
{
    private CardModel? _selected;
    private int _modified;
    protected override bool EarlyActs => true;
    public static bool Eligible(CardModel card) => card.Type is CardType.Attack or CardType.Skill or CardType.Power
        && CombatCardElements.Get(card) == Element.None;
    private bool CanModify(CardModel card) => IsTraveler ? Eligible(card) : card.IsUpgradable;
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() => Options();
    private EventOption[] Options() => [Pick((_modified == 0 ? "FREE" : "PAID") + (IsTraveler ? "" : "_GUEST"),
        _modified < 2 && Owner!.Deck.Cards.Any(CanModify) && (_modified == 0 || Owner.Gold >= 60) ? Select : null), Pick("LEAVE", Leave)];
    private async Task Select()
    {
        _selected = (await CardSelectCmd.FromDeckGeneric(Owner!, new CardSelectorPrefs(new LocString("events", IsTraveler ? "TC_E02.select" : "TC_E02.selectGuest"), 0, 1) { Cancelable = true }, CanModify)).FirstOrDefault();
        if (_selected is null) { Page("INITIAL", Options()); return; }
        if (!IsTraveler) { await Assign(Element.None); return; }
        Page("ELEMENT", Pick("WIND", () => Assign(Element.Wind)), Pick("ROCK", () => Assign(Element.Rock)), Pick("THUNDER", () => Assign(Element.Lightning)));
    }
    private async Task Assign(Element element)
    {
        if (_selected is null || !Owner!.Deck.Cards.Contains(_selected) || !CanModify(_selected) || (_modified > 0 && Owner.Gold < 60))
            throw new InvalidOperationException("Survey choice no longer eligible.");
        if (_modified > 0) await PlayerCmd.LoseGold(60, Owner!);
        if (IsTraveler) CombatCardElements.Permanent.Set(_selected, (int)element);
        else CardCmd.Upgrade(_selected);
        CardCmd.Preview(_selected);
        _selected = null;
        _modified++;
        if (_modified == 2) SetEventFinished(PageDescription("DONE"));
        else Page(IsTraveler ? "CALIBRATED" : "CALIBRATED_GUEST", Options());
    }
}

[CustomID("TC_E03")]
public sealed class LetterFromOutside : TravelerEvent
{
    protected override bool EarlyActs => false;
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() => [
        Pick(IsTraveler ? "SIGN" : "SIGN_GUEST", () => IsTraveler ? Give<OutsideLetter>() : Give<CompanionLetter>()),
        Pick(IsTraveler ? "VERIFY" : "VERIFY_GUEST", () => IsTraveler ? Give<AddressCorrection>() : Give<CompanionAddress>()),
        Pick("BUY", () => Give<UnclaimedMail>()), Pick("LEAVE", Leave)];
}

[CustomID("TC_E04")]
public sealed class LastGuide : TravelerEvent
{
    protected override bool EarlyActs => false;
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        if (IsTraveler) return [Pick("NAIL", () => Give<WallPiercingNail>()), Pick("SPOOL", () => Give<EchoSpool>()), Pick("WATCH", () => Give<ReverseWatch>())];
        var pool = ModelDb.RelicPool<SharedRelicPool>().GetUnlockedRelics(Owner!.UnlockState)
            .Concat(Owner.Character.RelicPool.GetUnlockedRelics(Owner.UnlockState))
            .Where(r => r.GetType().Assembly == typeof(RelicModel).Assembly && r.Rarity == RelicRarity.Common
                && r.IsAllowed(Owner.RunState) && Owner.Relics.All(owned => owned.Id != r.Id)).Distinct().ToList();
        var options = new List<EventOption>();
        while (options.Count < 3 && pool.Count > 0)
        {
            var relic = Rng.NextItem(pool)!.ToMutable();
            pool.RemoveAll(r => r.Id == relic.Id);
            options.Add(RelicOption(relic, async () => {
                await RelicCmd.Obtain(relic, Owner);
                SetEventFinished(PageDescription("DONE"));
            }));
        }
        return options.Count > 0 ? options : [Pick("LEAVE", Leave)];
    }
}
