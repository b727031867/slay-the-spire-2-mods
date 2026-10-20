using BaseLib.Utils.Attributes;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using Sts2Mod.Pools;
using Sts2Mod.Powers;

namespace Sts2Mod.Cards;

[CustomID("TC_C07"), Pool(typeof(TravelerCardPool))]
public sealed class WindGuidance : TravelerPowerCard<WindGuidancePower>
{
    public WindGuidance() : base(1, CardRarity.Uncommon, 1m) { }
    public override string CustomPortraitPath => "res://Sts2Mod/art/wind_guidance.svg";
    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
