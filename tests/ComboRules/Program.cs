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
Check(Play(a, Element.Rock).Kind == ComboKind.WindRock && a.Remaining == 2, "Wind Rock consumes one of three combos");
Play(a, Element.Rock);
Check(Play(a, Element.Wind).Kind == ComboKind.WindRock && a.Used == 2, "Rock Wind reverse also grants block combo");
Play(a, Element.Wind);
Check(Play(a, Element.Rock).Triggered && a.Used == 3 && a.Last == Element.None, "third combo is allowed");
Play(a, Element.Wind);
Check(!Play(a, Element.Rock).Triggered && a.Last == Element.None, "fourth combo is capped and clears sequence");

a.BeginTurn();
Play(a, Element.Rock);
Check(Play(a, Element.Lightning).Kind == ComboKind.RockLightning, "Rock Lightning grants attack combo");
a.BeginTurn();
Play(a, Element.Lightning);
Check(Play(a, Element.Rock).Kind == ComboKind.RockLightning, "Lightning Rock reverse grants attack combo");
a.BeginTurn();
Play(a, Element.Lightning);
Check(Play(a, Element.Wind).Kind == ComboKind.LightningWind, "Lightning Wind grants draw combo");
a.BeginTurn();
Play(a, Element.Wind);
Check(Play(a, Element.Lightning).Kind == ComboKind.LightningWind, "Wind Lightning reverse grants draw combo");

a.BeginTurn();
Play(a, Element.Wind);
Check(!Play(a, Element.None).Advanced && a.Last == Element.Wind, "normal cards preserve element");
Check(!a.Reserve(new object(), Element.Rock, automatic: true).Advanced && a.Last == Element.Wind, "automatic play preserves element");
Check(!a.Reserve(new object(), Element.Rock, firstInSeries: false).Advanced && a.Last == Element.Wind, "extra resolution preserves element");
var action = new object();
Check(a.Reserve(action, Element.Rock).Triggered && !a.Reserve(action, Element.Rock).Advanced && a.Used == 1, "same logical action awards once");
a.EndTurn();
Check(a.Last == Element.None && !Play(a, Element.Rock).Triggered, "sequence cannot cross turns");

var playerOne = new ComboState();
var playerTwo = new ComboState();
Play(playerOne, Element.Wind); Play(playerTwo, Element.Rock);
Check(playerOne.Last == Element.Wind && playerTwo.Last == Element.Rock && playerOne.Used == 0 && playerTwo.Used == 0,
    "player state isolation");
Check(Play(playerOne, Element.Rock).Triggered && !Play(playerTwo, Element.Rock).Triggered && playerTwo.Used == 0,
    "one player cannot consume another sequence");

var encodings = new HashSet<int>();
for (var used = 0; used <= ComboState.Limit; used++)
{
    var state = new ComboState();
    for (var i = 0; i < used; i++) { Play(state, Element.Wind); Play(state, Element.Rock); }
    Check(state.Encoded > 0 && encodings.Add(state.Encoded), $"positive unique native counter {used}");
    if (used < ComboState.Limit)
    {
        Play(state, Element.Wind);
        Check(encodings.Add(state.Encoded), $"encoded Wind {used}");
        Play(state, Element.None);
        Check(state.Last == Element.Wind, $"ignored event preserves counter {used}");
    }
}

var limit = new ComboState();
for (var i = 0; i < ComboState.Limit; i++) { Play(limit, Element.Lightning); Play(limit, Element.Wind); }
Check(limit.Used == 3 && limit.Remaining == 0, "all element pairs share three-combo limit");
limit.BeginTurn();
Check(limit.Used == 0 && limit.Remaining == 3 && limit.Last == Element.None, "native turn reset restores three combos");

Console.WriteLine($"COMBO_RULES_PASS assertions={passed}");
