using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;

namespace Sts2Mod.Pools;

// No character potions yet. The native shared potion pool remains available.
[CustomID("TC_POTION_POOL")]
public sealed class TravelerPotionPool : CustomPotionPoolModel
{
    public override string BigEnergyIconPath => "res://Sts2Mod/art/compass.svg";
    public override string TextEnergyIconPath => BigEnergyIconPath;
}
