namespace Sts2Mod.Combo;

public enum Element { None, Wind, Rock, Lightning }
public enum ComboKind { None, WindRock, RockLightning, LightningWind }

public readonly record struct ComboStep(bool Advanced, ComboKind Kind, Element Before, Element After, int Used)
{
    public bool Triggered => Kind != ComboKind.None;
}

/// <summary>One player's combat state. Action identity is stable across engine choice/resume IDs.</summary>
public sealed class ComboState
{
    public const int Limit = 3;
    private readonly HashSet<object> _seen = new(ReferenceEqualityComparer.Instance);
    public Element Last { get; private set; }
    public int Used { get; private set; }
    public int Remaining => Limit - Used;
    // Always positive: the engine removes powers whose amount reaches zero.
    public int Encoded => 1 + (int)Last + 4 * Used;

    public void BeginTurn() { Last = Element.None; Used = 0; }
    public void EndTurn() => Last = Element.None;

    public ComboStep Reserve(object action, Element element, bool automatic = false, bool firstInSeries = true)
    {
        var before = Last;
        if (element == Element.None || automatic || !firstInSeries || !_seen.Add(action))
            return new(false, ComboKind.None, before, Last, Used);

        if (Used >= Limit)
        {
            Last = Element.None;
            return new(true, ComboKind.None, before, Last, Used);
        }
        var kind = (Last, element) switch
        {
            (Element.Wind, Element.Rock) or (Element.Rock, Element.Wind) => ComboKind.WindRock,
            (Element.Rock, Element.Lightning) or (Element.Lightning, Element.Rock) => ComboKind.RockLightning,
            (Element.Lightning, Element.Wind) or (Element.Wind, Element.Lightning) => ComboKind.LightningWind,
            _ => ComboKind.None
        };
        if (kind != ComboKind.None) { Used++; Last = Element.None; }
        else Last = element;
        return new(true, kind, before, Last, Used);
    }
}
