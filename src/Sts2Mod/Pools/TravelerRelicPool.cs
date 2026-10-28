using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;

namespace Sts2Mod.Pools;

[CustomID("TC_RELIC_POOL")]
public sealed class TravelerRelicPool : CustomRelicPoolModel
{
    public override bool SeenByDefault => true;
    public override string? BigEnergyIconPath => "res://Sts2Mod/ui/energy/hover.svg";
    public override string? TextEnergyIconPath => "res://Sts2Mod/ui/energy/glyph.svg";
}
