using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Utils.Attributes;
using MegaCrit.Sts2.Core.Entities.Relics;
using Sts2Mod.Pools;

namespace Sts2Mod.Relics;

[CustomID("TC_R01"), Pool(typeof(TravelerRelicPool))]
public sealed class PaimonCheer : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public override bool IsStackable => false;
    public override string PackedIconPath => "res://Sts2Mod/art/paimon.png";
    protected override string PackedIconOutlinePath => PackedIconPath;
    protected override string BigIconPath => PackedIconPath;
}
