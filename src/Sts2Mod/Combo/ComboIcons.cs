using Godot;

namespace Sts2Mod.Combo;

/// <summary>Purpose-designed UI symbols, not card illustration placeholders.</summary>
public static class ComboIcons
{
    public static string ElementPath(Element element) => "res://Sts2Mod/ui/element_" + element.ToString().ToLowerInvariant() + ".svg";
    public static string RewardPath(ComboKind kind) => "res://Sts2Mod/ui/reward_" + (kind switch
    {
        ComboKind.WindRock => "block.svg",
        ComboKind.RockLightning => "attack.svg",
        _ => "draw.svg"
    });
    public static Texture2D ElementTexture(Element element) => ResourceLoader.Load<Texture2D>(ElementPath(element));
    public static Texture2D RewardTexture(ComboKind kind) => ResourceLoader.Load<Texture2D>(RewardPath(kind));
    public static Color Color(Element element) => element switch
    {
        Element.Wind => new("7ddbc8"), Element.Rock => new("edc578"), Element.Lightning => new("bea4ea"), _ => new("9daeb9")
    };
    public static Color Color(ComboKind kind) => Color(kind switch
    {
        ComboKind.WindRock => Element.Rock, ComboKind.RockLightning => Element.Lightning, _ => Element.Wind
    });
}
