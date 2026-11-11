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
    public int TurnLimit { get; private set; } = Limit;
    public void SetTurnLimit(int limit) => TurnLimit = Math.Max(Limit, limit);
    private readonly HashSet<object> _seen = new(ReferenceEqualityComparer.Instance);
    // Diagnostic only: lets the rules test observe that combat end releases the dedupe set.
    internal int SeenCount => _seen.Count;
    public Element Last { get; private set; }
    public int Used { get; private set; }
    public int RewardsThisTurn { get; private set; }
    public int CombosThisCombat { get; private set; }
    public int Remaining => Math.Max(0, TurnLimit - Used);
    // Always positive: the engine removes powers whose amount reaches zero.
    public int Encoded => 1 + (int)Last + 4 * Used;

    public void BeginTurn() { Used = 0; RewardsThisTurn = 0; }
    // The dedupe set only spans one combat: keeping it would retain every action object
    // and grow without bound in long fights. Natural cross-turn duplicates still work
    // because turns do not clear it; only a combat end does.
    public void ClearCombat()
    {
        Last = Element.None; Used = 0; RewardsThisTurn = 0; CombosThisCombat = 0;
        if (_seen.Count > 0) _seen.Clear();
    }
    public int MarkRewardGranted() => ++RewardsThisTurn;

    // Read-only: hovering must never reserve an action or spend a combo use.
    public ComboKind Preview(Element element) => Used >= TurnLimit ? ComboKind.None : Pair(Last, element);

    private static ComboKind Pair(Element first, Element second) => (first, second) switch
    {
        (Element.Wind, Element.Rock) or (Element.Rock, Element.Wind) => ComboKind.WindRock,
        (Element.Rock, Element.Lightning) or (Element.Lightning, Element.Rock) => ComboKind.RockLightning,
        (Element.Lightning, Element.Wind) or (Element.Wind, Element.Lightning) => ComboKind.LightningWind,
        _ => ComboKind.None
    };

    public ComboStep Reserve(object action, Element element, bool automatic = false, bool firstInSeries = true)
    {
        var before = Last;
        if (element == Element.None || automatic || !firstInSeries || !_seen.Add(action))
            return new(false, ComboKind.None, before, Last, Used);

        if (Used >= TurnLimit)
        {
            Last = element;
            return new(true, ComboKind.None, before, Last, Used);
        }
        var kind = Preview(element);
        if (kind != ComboKind.None) { Used++; CombosThisCombat++; }
        Last = element;
        return new(true, kind, before, Last, Used);
    }
}
