using BaseLib.Utils.Attributes;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2Mod.Combo;
using Sts2Mod.Pools;

namespace Sts2Mod.Cards;

[CustomID("TC_C12"), Pool(typeof(TravelerCardPool))]
public sealed class ShatteringRock : ElementCardModel
{
    public ShatteringRock() : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy) { }
    public override Element CardElement => Element.Rock;
    public override string CustomPortraitPath => "res://Sts2Mod/art/shattering_rock.png";
    private bool _resolvingCombo;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12m, ValueProp.Move)];
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(6m);
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        => cardSource == this && !_resolvingCombo && dealer == Owner.Creature && props.IsPoweredAttack()
            ? 2 * (Owner.Creature.GetPower<StrengthPower>()?.Amount ?? 0) : 0;
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash").Execute(context);
        // Combo follow-ups keep the normal Strength scaling.
        _resolvingCombo = true;
        try { await ComboPower.Resolve(this, context, play, reservation); }
        finally { _resolvingCombo = false; }
    }
}
