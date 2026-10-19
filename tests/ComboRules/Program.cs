using Sts2Mod.Combo;

var passed = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new InvalidOperationException(name);
    Console.WriteLine("PASS " + name);
    passed++;
}
ComboStep Play(ComboState state, Element element) => state.Reserve(new object(), element);
var a = new ComboState();
Play(a, Element.Wind);
Check(Play(a, Element.Rock).Triggered && a.Last == Element.None && a.Remaining == 1, "TC-01 Wind Rock consumes sequence");
Play(a, Element.Rock); Play(a, Element.Wind);
Check(a.Used == 1 && a.Last == Element.Wind, "TC-04 reverse order gives no reward");
Check(!Play(a, Element.None).Advanced && a.Last == Element.Wind, "TC-05 normal cards preserve element");
Play(a, Element.Wind);
Check(Play(a, Element.Rock).Triggered && a.Used == 2, "TC-06 same element updates rather than triggers");
Play(a, Element.Wind);
Check(!Play(a, Element.Rock).Triggered && a.Last == Element.None, "TC-08 cap remains empty");
a.BeginTurn(); Play(a, Element.Wind); a.EndTurn();
Check(a.Last == Element.None && !Play(a, Element.Rock).Triggered, "TC-09 sequence cannot cross turns");
a.BeginTurn(); Play(a, Element.Wind);
Check(!a.Reserve(new object(), Element.Rock, automatic: true).Advanced && a.Last == Element.Wind, "TC-10 automatic preserves sequence");
Check(!a.Reserve(new object(), Element.Rock, firstInSeries: false).Advanced && a.Last == Element.Wind, "TC-10 extra replay preserves sequence");
var action = new object();
Check(a.Reserve(action, Element.Rock).Triggered && !a.Reserve(action, Element.Rock).Advanced && a.Used == 1, "TC-11 same logical action awards once");
a.BeginTurn();
Check(!a.Reserve(action, Element.Wind).Advanced && a.Last == Element.None, "resumed old action remains deduplicated after turn reset");
var b = new ComboState(); Play(a, Element.Wind); Play(b, Element.Rock);
Check(a.Last == Element.Wind && b.Last == Element.Rock && a.Used == 0 && b.Used == 0, "TC-16 player state isolation (logic only)");
Check(Play(a, Element.Rock).Triggered && !Play(b, Element.Rock).Triggered && b.Used == 0, "one player cannot consume another sequence");
var encodings = new HashSet<int>();
for (var used = 0; used <= 2; used++)
{
    var state = new ComboState();
    for (var i = 0; i < used; i++) { Play(state, Element.Wind); Play(state, Element.Rock); }
    Check(state.Encoded > 0 && encodings.Add(state.Encoded), $"positive unique native state counter {used}");
    if (used < 2)
    {
        Play(state, Element.Wind); Check(encodings.Add(state.Encoded), $"encoded Wind {used}");
        Play(state, Element.None); Check(state.Last == Element.Wind, $"ignored event preserves counter {used}");
        // Rock would consume Wind; construct Rock state from a fresh turn with the same used count.
        var rock = new ComboState();
        for (var i = 0; i < used; i++) { Play(rock, Element.Wind); Play(rock, Element.Rock); }
        Play(rock, Element.Rock); Check(encodings.Add(rock.Encoded), $"encoded Rock {used}");
    }
}
Console.WriteLine($"COMBO_RULES_PASS assertions={passed}");
