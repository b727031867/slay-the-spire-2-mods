using BaseLib.Utils.Attributes;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using Sts2Mod.Pools;
using Sts2Mod.Powers;

namespace Sts2Mod.Cards;

[CustomID("TC_C09"), Pool(typeof(TravelerCardPool))]
public sealed class ThunderEcho : TravelerPowerCard<ThunderEchoPower>
{
    public ThunderEcho() : base(2, CardRarity.Rare, 1m) { }
    public override string CustomPortraitPath => "res://Sts2Mod/art/thunder_echo.png";
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
