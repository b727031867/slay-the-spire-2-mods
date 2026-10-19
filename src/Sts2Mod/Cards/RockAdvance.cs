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

[CustomID("TC_C05"), Pool(typeof(IroncladCardPool))]
public sealed class RockAdvance : CustomCardModel
{
    public RockAdvance() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }
    public override string CustomPortraitPath => "res://Sts2Mod/art/rock_advance.svg";
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(5m, ValueProp.Move), new BlockVar(3m, ValueProp.Move)];
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars.Block.UpgradeValueBy(2m);
    }

    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = ComboPower.Reserve(this, context, play, Element.Rock);
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash").Execute(context);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, play);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}
