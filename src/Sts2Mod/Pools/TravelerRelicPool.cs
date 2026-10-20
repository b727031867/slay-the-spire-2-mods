using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;

namespace Sts2Mod.Pools;

[CustomID("TC_RELIC_POOL")]
public sealed class TravelerRelicPool : CustomRelicPoolModel
{
    public override bool SeenByDefault => true;
    public override string EnergyColorName => "ironclad";
}
