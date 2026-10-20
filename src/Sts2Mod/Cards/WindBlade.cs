using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Sts2Mod.Pools;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Combo;

namespace Sts2Mod.Cards;

[CustomID("TC_C01"), Pool(typeof(TravelerCardPool))]
public sealed class WindBlade : ElementCardModel
{
    public WindBlade() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }
    public override Element CardElement => Element.Wind;
    public override string CustomPortraitPath => "res://Sts2Mod/art/wind.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var reservation = ComboPower.Reserve(this, choiceContext, cardPlay, Element.Wind);
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
        await ComboPower.Resolve(this, choiceContext, cardPlay, reservation);
    }
}
