using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Sts2Mod.Combo;
using Sts2Mod.Pools;
using Sts2Mod.Powers;

namespace Sts2Mod.Cards;

[CustomID("TC_C25"), Pool(typeof(TravelerCardPool))]
public sealed class TriphaseInfusion() : ElementCardModel(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    public override Element CardElement => Element.None;
    public override string CustomPortraitPath => "res://Sts2Mod/art/TC_C25.tres";
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var power = await PowerCmd.Apply<TriphaseInfusionPower>(context, Owner.Creature, 1, Owner.Creature, this);
        if (power is not null) await power.Trigger(context, this, play);
        var reservation = await ComboPower.Reserve(this, context, play, CardElement);
        await ComboPower.Resolve(this, context, play, reservation);
    }
}
