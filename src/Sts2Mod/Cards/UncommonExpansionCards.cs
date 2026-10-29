using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Combo;
using Sts2Mod.Pools;
using Sts2Mod.Powers;

namespace Sts2Mod.Cards;

[CustomID("TC_C29"), Pool(typeof(TravelerCardPool))]
public sealed class WindPathfinding() : ElementCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override Element CardElement => Element.Wind;
    public override string CustomPortraitPath => "res://Sts2Mod/art/TC_C29.tres";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        var selected = await CardSelectCmd.FromCombatPile(context, PileType.Draw.GetPile(Owner), Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1),
            card => CombatCardElements.Get(card, (card as ElementCardModel)?.CardElement ?? Element.None) is Element.Rock or Element.Lightning);
        foreach (var card in selected) await CardPileCmd.Add(card, PileType.Hand);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}

[CustomID("TC_C30"), Pool(typeof(TravelerCardPool))]
public sealed class WindShelteredPlans() : ElementCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override Element CardElement => Element.Wind;
    public override string CustomPortraitPath => "res://Sts2Mod/art/TC_C30.tres";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];
    protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1);
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        await CardPileCmd.Draw(context, DynamicVars.Cards.IntValue, Owner);
        var selected = await CardSelectCmd.FromHand(context, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1),
            card => card != this && !card.ShouldRetainThisTurn, this);
        foreach (var card in selected) card.GiveSingleTurnRetain();
        await ComboPower.Resolve(this, context, play, reservation);
    }
}

[CustomID("TC_C31"), Pool(typeof(TravelerCardPool))]
public sealed class StoneRidgeCounter() : ElementCardModel(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override Element CardElement => Element.Rock;
    public override string CustomPortraitPath => "res://Sts2Mod/art/TC_C31.tres";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7, ValueProp.Move), new DynamicVar("Weak", 2)];
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var hasBlock = Owner.Creature.Block > 0;
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(context);
        if (hasBlock && play.Target.IsAlive) await PowerCmd.Apply<WeakPower>(context, play.Target, 2, Owner.Creature, this);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}

[CustomID("TC_C32"), Pool(typeof(TravelerCardPool))]
public sealed class LayeredRockEcho() : TravelerPowerCard<LayeredRockEchoPower>(1, CardRarity.Uncommon, 1)
{
    public override string CustomPortraitPath => "res://Sts2Mod/art/TC_C32.tres";
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DexterityPower>()];
    protected override void OnUpgrade() => DynamicVars["PowerAmount"].UpgradeValueBy(1);
}

[CustomID("TC_C33"), Pool(typeof(TravelerCardPool))]
public sealed class StoneGathering() : ElementCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;
    public override Element CardElement => Element.Rock;
    public override string CustomPortraitPath => "res://Sts2Mod/art/TC_C33.tres";
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Append(HoverTipFactory.FromPower<VigorPower>());
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8, ValueProp.Move), new DynamicVar("Vigor", 7)];
    protected override void OnUpgrade() { DynamicVars.Block.UpgradeValueBy(3); DynamicVars["Vigor"].UpgradeValueBy(3); }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await PowerCmd.Apply<VigorPower>(context, Owner.Creature, DynamicVars["Vigor"].BaseValue, Owner.Creature, this);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}

[CustomID("TC_C34"), Pool(typeof(TravelerCardPool))]
public sealed class ThunderNavigation() : ElementCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;
    public override Element CardElement => Element.Lightning;
    public override string CustomPortraitPath => "res://Sts2Mod/art/TC_C34.tres";
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move), new CardsVar(2)];
    protected override void OnUpgrade() { DynamicVars.Block.UpgradeValueBy(2); RemoveKeyword(CardKeyword.Exhaust); }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await PowerCmd.Apply<DrawCardsNextTurnPower>(context, Owner.Creature, DynamicVars.Cards.BaseValue, Owner.Creature, this);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}

[CustomID("TC_C35"), Pool(typeof(TravelerCardPool))]
public sealed class VenomThunderNeedle() : ElementCardModel(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override Element CardElement => Element.Lightning;
    public override string CustomPortraitPath => "res://Sts2Mod/art/TC_C35.tres";
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Append(HoverTipFactory.FromPower<PoisonPower>());
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5, ValueProp.Move), new DynamicVar("Poison", 3)];
    protected override void OnUpgrade() { DynamicVars.Damage.UpgradeValueBy(3); DynamicVars["Poison"].UpgradeValueBy(1); }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).Execute(context);
        if (play.Target.IsAlive) await PowerCmd.Apply<PoisonPower>(context, play.Target, DynamicVars["Poison"].BaseValue, Owner.Creature, this);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}

[CustomID("TC_C36"), Pool(typeof(TravelerCardPool))]
public sealed class VenomCarryingWind() : ElementCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override Element CardElement => Element.Wind;
    public override string CustomPortraitPath => "res://Sts2Mod/art/TC_C36.tres";
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Append(HoverTipFactory.FromPower<PoisonPower>());
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poison", 3), new CardsVar(1)];
    protected override void OnUpgrade() => DynamicVars["Poison"].UpgradeValueBy(1);
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        foreach (var enemy in CombatState!.Enemies.Where(e => e.IsAlive).ToArray())
            await PowerCmd.Apply<PoisonPower>(context, enemy, DynamicVars["Poison"].BaseValue, Owner.Creature, this);
        await CardPileCmd.Draw(context, 1, Owner);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}

[CustomID("TC_C37"), Pool(typeof(TravelerCardPool))]
public sealed class ToxicSatchel() : TravelerPowerCard<ToxicSatchelPower>(1, CardRarity.Uncommon, 2)
{
    public override string CustomPortraitPath => "res://Sts2Mod/art/TC_C37.tres";
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<PoisonPower>()];
    protected override void OnUpgrade() => DynamicVars["PowerAmount"].UpgradeValueBy(1);
}

[CustomID("TC_C38"), Pool(typeof(TravelerCardPool))]
public sealed class PoisonStoneBarrier() : ElementCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;
    public override Element CardElement => Element.Rock;
    public override string CustomPortraitPath => "res://Sts2Mod/art/TC_C38.tres";
    protected override IEnumerable<IHoverTip> ExtraHoverTips => base.ExtraHoverTips.Append(HoverTipFactory.FromPower<PoisonPower>());
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move), new DynamicVar("ExtraBlock", 3)];
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        var poisoned = CombatState!.Enemies.Any(e => e.IsAlive && (e.GetPower<PoisonPower>()?.Amount ?? 0) > 0);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        if (poisoned) await CreatureCmd.GainBlock(Owner.Creature, DynamicVars["ExtraBlock"].BaseValue, ValueProp.Move, play);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}
