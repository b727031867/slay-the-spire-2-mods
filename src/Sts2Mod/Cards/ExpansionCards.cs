using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Combo;
using Sts2Mod.Pools;

namespace Sts2Mod.Cards;

[CustomID("TC_C20"), Pool(typeof(TravelerCardPool))]
public sealed class CleansingWind() : ElementCardModel(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override bool GainsBlock => true;
    public override Element CardElement => Element.Wind;
    public override string CustomPortraitPath => "res://Sts2Mod/art/wind_shelter.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, play);
        var selected = await CardSelectCmd.FromHand(context, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 0, 1),
            c => c.Type is CardType.Status or CardType.Curse, this);
        foreach (var card in selected)
        {
            var exhaustedBefore = CombatCardElements.Exhaustions.Get(card);
            await CardCmd.Exhaust(context, card);
            if (CombatCardElements.Exhaustions.Get(card) > exhaustedBefore) await CardPileCmd.Draw(context, 1, Owner);
        }
        await ComboPower.Resolve(this, context, play, reservation);
    }
}

[CustomID("TC_C21"), Pool(typeof(TravelerCardPool))]
public sealed class ClearRockMind() : ElementCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;
    public override Element CardElement => Element.Rock;
    public override string CustomPortraitPath => "res://Sts2Mod/art/rock_heart.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6, ValueProp.Move), new DynamicVar("Cleanse", 1)];
    protected override void OnUpgrade() { DynamicVars.Block.UpgradeValueBy(2); DynamicVars["Cleanse"].UpgradeValueBy(1); }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        foreach (var power in Owner.Creature.Powers.Where(p => p is WeakPower or VulnerablePower or FrailPower).ToArray())
            await PowerCmd.ModifyAmount(context, power, -Math.Min(Math.Max(0, power.Amount), DynamicVars["Cleanse"].IntValue), Owner.Creature, this);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, play);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}

[CustomID("TC_C22"), Pool(typeof(TravelerCardPool))]
public sealed class HeadwindRestraint() : ElementCardModel(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    public override Element CardElement => Element.Wind;
    public override string CustomPortraitPath => "res://Sts2Mod/art/wind.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4, ValueProp.Move), new DynamicVar("Weak", 2)];
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target).WithHitFx("vfx/vfx_attack_slash").Execute(context);
        if (play.Target.IsAlive) await PowerCmd.Apply<WeakPower>(context, play.Target, 2, Owner.Creature, this);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}

[CustomID("TC_C23"), Pool(typeof(TravelerCardPool))]
public sealed class OpportunistLightning() : ElementCardModel(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override Element CardElement => Element.Lightning;
    public override string CustomPortraitPath => "res://Sts2Mod/art/thunder_mark.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CalculationBaseVar(6), new ExtraDamageVar(2),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (_, target) =>
            Math.Max(0, target?.GetPower<WeakPower>()?.Amount ?? 0) + Math.Max(0, target?.GetPower<VulnerablePower>()?.Amount ?? 0))];
    protected override void OnUpgrade() { DynamicVars.CalculationBase.UpgradeValueBy(3); DynamicVars.ExtraDamage.UpgradeValueBy(1); }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var damage = DynamicVars.CalculatedDamage.Calculate(play.Target);
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        await DamageCmd.Attack(damage).FromCard(this).Targeting(play.Target).WithHitFx("vfx/vfx_attack_slash").Execute(context);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}

[CustomID("TC_C26"), Pool(typeof(TravelerCardPool))]
public sealed class ThunderBurst() : ElementCardModel(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    private int _hits;
    public override Element CardElement => Element.Lightning;
    public override string CustomPortraitPath => "res://Sts2Mod/art/lightning.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4, ValueProp.Move)];
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1);
    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        description.Add("Hits", 2 + Math.Clamp(IsMutable ? Owner?.Creature.GetPower<ComboPower>()?.State.RewardsThisTurn ?? 0 : 0, 0, 4));
    }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (play.IsFirstInSeries)
            _hits = 2 + Math.Clamp(Owner.Creature.GetPower<ComboPower>()?.State.RewardsThisTurn ?? 0, 0, 4);
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(_hits).FromCard(this).Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash").Execute(context);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}

[CustomID("TC_C27"), Pool(typeof(TravelerCardPool))]
public sealed class HeavenEarthConvergence() : ElementCardModel(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    private Element _element;
    private int _x;
    public override bool GainsBlock => true;
    public override Element CardElement => Element.None;
    protected override bool HasEnergyCostX => true;
    public override string CustomPortraitPath => "res://Sts2Mod/art/triphase_finale.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("RockBlock", 6), new DynamicVar("LightningDamage", 5), new DynamicVar("WindPenalty", 1)];
    protected override void OnUpgrade() { DynamicVars["RockBlock"].UpgradeValueBy(3); DynamicVars["LightningDamage"].UpgradeValueBy(2); DynamicVars["WindPenalty"].UpgradeValueBy(-1); }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        if (play.IsFirstInSeries)
        {
            _element = Owner.Creature.GetPower<ComboPower>()?.State.Last ?? Element.None;
            _x = Math.Max(0, ResolveEnergyXValue());
        }
        switch (_element)
        {
            case Element.Wind:
                var amount = Math.Max(0, _x - DynamicVars["WindPenalty"].IntValue);
                if (amount > 0) { await CardPileCmd.Draw(context, amount, Owner); await PlayerCmd.GainEnergy(amount, Owner); }
                break;
            case Element.Rock:
                if (_x > 0) await CreatureCmd.GainBlock(Owner.Creature, DynamicVars["RockBlock"].BaseValue * _x, ValueProp.Move, play);
                break;
            case Element.Lightning:
                if (_x > 0) await DamageCmd.Attack(DynamicVars["LightningDamage"].BaseValue * _x).FromCard(this).TargetingAllOpponents(CombatState!).Execute(context);
                break;
        }
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}
