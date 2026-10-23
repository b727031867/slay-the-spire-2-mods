using BaseLib.Abstracts;
using BaseLib.Utils.Attributes;
using Godot;

namespace Sts2Mod.Pools;

// Kept separate from normal reward pools; content is granted only by the four events.
[CustomID("TC_EVENT_RELIC_POOL")]
public sealed class TravelerEventRelicPool : CustomRelicPoolModel
{
    public override bool IsShared => true;
    public override bool SeenByDefault => true;
    public override string EnergyColorName => "ironclad";
}

[CustomID("TC_EVENT_CARD_POOL")]
public sealed class TravelerEventCardPool : CustomCardPoolModel
{
    public override bool IsShared => true;
    public override string Title => "traveler";
    public override Color DeckEntryCardColor => new("79D9C8");
    public override Color ShaderColor => new("79D9C8");
    public override bool IsColorless => false;
    public override bool SeenByDefault => true;
    public override string EnergyColorName => "ironclad";
}
