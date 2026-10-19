namespace Sts2Mod.Combo;

public enum Element { None, Wind, Rock }

public readonly record struct ComboStep(bool Advanced, bool Triggered, Element Before, Element After, int Used);

/// <summary>One player's combat state. Action identity is stable across engine choice/resume IDs.</summary>
public sealed class ComboState
{
    public const int Limit = 2;
    private readonly HashSet<object> _seen = new(ReferenceEqualityComparer.Instance);
    public Element Last { get; private set; }
    public int Used { get; private set; }
    public int Remaining => Limit - Used;
    // Always positive: the engine removes powers whose amount reaches zero.
    public int Encoded => 1 + (int)Last + 3 * Used;

    public void BeginTurn() { Last = Element.None; Used = 0; }
    public void EndTurn() => Last = Element.None;

    public ComboStep Reserve(object action, Element element, bool automatic = false, bool firstInSeries = true)
    {
        var before = Last;
        if (element == Element.None || automatic || !firstInSeries || !_seen.Add(action))
            return new(false, false, before, Last, Used);

        if (Used >= Limit)
        {
            Last = Element.None;
            return new(true, false, before, Last, Used);
        }
        var triggered = Last == Element.Wind && element == Element.Rock;
        if (triggered) { Used++; Last = Element.None; }
        else Last = element;
        return new(true, triggered, before, Last, Used);
    }
}
