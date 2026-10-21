using BaseLib.Utils.Attributes;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Combo;
using Sts2Mod.Pools;

namespace Sts2Mod.Cards;

[CustomID("TC_C13"), Pool(typeof(TravelerCardPool))]
public sealed class ThunderMark : ElementCardModel
{
    public ThunderMark() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy) { }
    public override Element CardElement => Element.Lightning;
    public override string CustomPortraitPath => "res://Sts2Mod/art/thunder_mark.png";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move), new DynamicVar("Vulnerable", 1m)];
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["Vulnerable"].UpgradeValueBy(1m);
    }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = ComboPower.Reserve(this, context, play, CardElement);
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash").Execute(context);
        if (play.Target.IsAlive)
            await PowerCmd.Apply<VulnerablePower>(context, play.Target, DynamicVars["Vulnerable"].BaseValue, Owner.Creature, this);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}
