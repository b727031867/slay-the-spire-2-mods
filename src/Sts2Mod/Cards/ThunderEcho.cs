using BaseLib.Utils.Attributes;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Sts2Mod.Pools;
using Sts2Mod.Powers;

namespace Sts2Mod.Cards;

[CustomID("TC_C09"), Pool(typeof(TravelerCardPool))]
public sealed class ThunderEcho : TravelerPowerCard<ThunderEchoPower>
{
    public ThunderEcho() : base(1, CardRarity.Rare, 2m) { }
    public override string CustomPortraitPath => "res://Sts2Mod/art/thunder_echo.png";
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        DynamicVars["PowerAmount"].UpgradeValueBy(2);
    }
    protected override async Task OnPlay(PlayerChoiceContext context, CardPlay play)
    {
        var current = Owner.Creature.GetPower<ThunderEchoPower>()?.Amount ?? 0;
        var increase = DynamicVars["PowerAmount"].BaseValue - current;
        if (increase > 0)
            await PowerCmd.Apply<ThunderEchoPower>(context, Owner.Creature, increase, Owner.Creature, this);
    }
}
