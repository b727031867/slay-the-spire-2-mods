using BaseLib.Utils.Attributes;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using Sts2Mod.Pools;
using Sts2Mod.Powers;

namespace Sts2Mod.Cards;

[CustomID("TC_C08"), Pool(typeof(TravelerCardPool))]
public sealed class RockHeart : TravelerPowerCard<RockHeartPower>
{
    public RockHeart() : base(1, CardRarity.Uncommon, 4m) { }
    public override string CustomPortraitPath => "res://Sts2Mod/art/rock_heart.png";
    protected override void OnUpgrade() => DynamicVars["PowerAmount"].UpgradeValueBy(2m);
}
