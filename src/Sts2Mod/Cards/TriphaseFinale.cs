using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Combo;
using Sts2Mod.Pools;

namespace Sts2Mod.Cards;

[CustomID("TC_C14"), Pool(typeof(TravelerCardPool))]
public sealed class TriphaseFinale : CustomCardModel
{
    public TriphaseFinale() : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy) { }
    public override string CustomPortraitPath => "res://Sts2Mod/art/triphase_finale.png";
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new CalculationBaseVar(10m), new ExtraDamageVar(4m),
            new CalculatedDamageVar(ValueProp.Move).WithMultiplier(static (card, _) =>
                Math.Clamp(card.Owner.Creature.GetPower<ComboPower>()?.State.Used ?? 0, 0, ComboState.Limit))];
    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(2m);
        EnergyCost.UpgradeBy(-1);
    }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this).Targeting(play.Target).WithHitFx("vfx/vfx_attack_slash").Execute(context);
    }
}
