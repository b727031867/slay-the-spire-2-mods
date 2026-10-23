using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
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
    // Events are selected for the whole party. Do not expose Traveler-only content to other characters.
    public override bool IsAllowed(IRunState run) => run.Players.All(p => p.Character is Traveler)
        && run.CurrentActIndex >= (EarlyActs ? 0 : 1) && run.CurrentActIndex <= (EarlyActs ? 1 : 2);
    public override string CustomInitialPortraitPath => $"res://Sts2Mod/art/events/{Id.Entry}.svg";
    protected EventOption Pick(string key, Func<Task>? chosen) => new(this, chosen, $"{Id.Entry}.pages.INITIAL.options.{key}");
    protected void Page(string key, params EventOption[] options) => SetEventState(PageDescription(key), options);
    protected Task Leave() { SetEventFinished(PageDescription("LEAVE")); return Task.CompletedTask; }
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
        Page("CAMP", Pick("DEEPER", Owner.Creature.CurrentHp > 7 ? Second : null), Pick("LEAVE", Leave));
    }
    private async Task OfferCards(bool rare)
    {
        var pool = ModelDb.AllCards.Where(c => TravelerContent.RewardCardTypes.Contains(c.GetType()) && (!rare || c.Rarity == CardRarity.Rare)).ToList();
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
        Page("DOOR", Pick("CURSE", Third), Pick("LEAVE", Leave));
    }
    private async Task Third()
    {
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(Owner!.RunState.CreateCard<Homeward>(Owner), PileType.Deck));
        Page("TREASURE", Pick("SUPPLIES", Supplies), Pick("RARE", Rare));
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
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() => Options();
    private EventOption[] Options() => [Pick(_modified == 0 ? "FREE" : "PAID",
        _modified < 2 && Owner!.Deck.Cards.Any(Eligible) && (_modified == 0 || Owner.Gold >= 60) ? Select : null), Pick("LEAVE", Leave)];
    private async Task Select()
    {
        _selected = (await CardSelectCmd.FromDeckGeneric(Owner!, new CardSelectorPrefs(new LocString("events", "TC_E02.select"), 1), Eligible)).FirstOrDefault();
        if (_selected is null) { Page("INITIAL", Options()); return; }
        Page("ELEMENT", Pick("WIND", () => Assign(Element.Wind)), Pick("ROCK", () => Assign(Element.Rock)), Pick("THUNDER", () => Assign(Element.Lightning)));
    }
    private async Task Assign(Element element)
    {
        if (_selected is null || !Eligible(_selected) || (_modified > 0 && Owner!.Gold < 60))
            throw new InvalidOperationException("Survey choice no longer eligible.");
        if (_modified > 0) await PlayerCmd.LoseGold(60, Owner!);
        CombatCardElements.Permanent.Set(_selected, (int)element);
        CardCmd.Preview(_selected);
        _selected = null;
        _modified++;
        if (_modified == 2) SetEventFinished(PageDescription("DONE"));
        else Page("CALIBRATED", Options());
    }
}

[CustomID("TC_E03")]
public sealed class LetterFromOutside : TravelerEvent
{
    protected override bool EarlyActs => false;
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() => [
        Pick("SIGN", () => Give<OutsideLetter>()), Pick("VERIFY", () => Give<AddressCorrection>()),
        Pick("BUY", () => Give<UnclaimedMail>()), Pick("LEAVE", Leave)];
}

[CustomID("TC_E04")]
public sealed class LastGuide : TravelerEvent
{
    protected override bool EarlyActs => false;
    protected override IReadOnlyList<EventOption> GenerateInitialOptions() => [
        Pick("NAIL", () => Give<WallPiercingNail>()), Pick("SPOOL", () => Give<EchoSpool>()),
        Pick("WATCH", () => Give<ReverseWatch>())];
}
