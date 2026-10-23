using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Combo;
using Sts2Mod.Pools;

namespace Sts2Mod.Cards;

[CustomID("TC_C19"), Pool(typeof(TravelerCardPool))]
public sealed class ThunderChain : ElementCardModel
{
    public ThunderChain() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies) { }
    public override Element CardElement => Element.Lightning;
    public override string CustomPortraitPath => "res://Sts2Mod/art/drafts/TC_C19.svg";
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move), new DynamicVar("Hits", 2m)];
    protected override void OnUpgrade() => DynamicVars["Hits"].UpgradeValueBy(1m);
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        ArgumentNullException.ThrowIfNull(CombatState);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(DynamicVars["Hits"].IntValue)
            .FromCard(this).TargetingAllOpponents(CombatState).WithHitFx("vfx/vfx_attack_slash").Execute(context);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}
