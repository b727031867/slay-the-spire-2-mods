using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Combo;

namespace Sts2Mod.Cards;

[CustomID("TC_C03"), Pool(typeof(IroncladCardPool))]
public sealed class LightningThrust : CustomCardModel
{
    public LightningThrust() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }
    public override string CustomPortraitPath => "res://Sts2Mod/art/lightning.svg";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = ComboPower.Reserve(this, context, play, Element.Lightning);
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash").Execute(context);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}
