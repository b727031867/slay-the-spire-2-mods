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
    // Match the native energy counter. Full-resolution relic art is not an energy glyph.
    public override string EnergyColorName => "ironclad";
}
