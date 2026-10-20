using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using Godot;

namespace Sts2Mod.Pools;

[CustomID("TC_CARD_POOL")]
public sealed class TravelerCardPool : CustomCardPoolModel
{
    public override string Title => "traveler";
    public override Color DeckEntryCardColor => new("79D9C8");
    public override Color ShaderColor => new("79D9C8");
    public override bool IsColorless => false;
    public override bool SeenByDefault => true;
    public override string BigEnergyIconPath => "res://Sts2Mod/art/compass.png";
    public override string TextEnergyIconPath => BigEnergyIconPath;
}
