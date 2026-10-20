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
Check(Play(a, Element.Rock).Triggered && a.Used == 3 && a.Last == Element.Rock, "third combo is allowed");
Play(a, Element.Wind);
Check(!Play(a, Element.Rock).Triggered && a.Last == Element.Rock, "fourth combo is capped and retains latest element");

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
    if (used > 0) Play(state, Element.Lightning);
    for (var i = 0; i < used; i++) Play(state, i % 2 == 0 ? Element.Rock : Element.Lightning);
    Check(state.Encoded > 0 && encodings.Add(state.Encoded), $"positive unique native counter {used}");
    if (used < ComboState.Limit)
    {
        Play(state, state.Last == Element.None ? Element.Wind : state.Last);
        if (used == 0) Check(encodings.Add(state.Encoded), $"encoded Wind {used}");
        var last = state.Last;
        Play(state, Element.None);
        Check(state.Last == last, $"ignored event preserves counter {used}");
    }
}

var limit = new ComboState();
for (var i = 0; i < ComboState.Limit; i++) { Play(limit, Element.Lightning); Play(limit, Element.Wind); }
Check(limit.Used == 3 && limit.Remaining == 0, "all element pairs share three-combo limit");
limit.BeginTurn();
Check(limit.Used == 0 && limit.Remaining == 3 && limit.Last == Element.None, "native turn reset restores three combos");

foreach (var first in new[] { Element.Wind, Element.Rock, Element.Lightning })
foreach (var second in new[] { Element.Wind, Element.Rock, Element.Lightning })
{
    var preview = new ComboState();
    Play(preview, first);
    var before = preview.Encoded;
    var predicted = preview.Preview(second);
    for (var i = 0; i < 100; i++) preview.Preview(second);
    Check(preview.Encoded == before && preview.Last == first && preview.Used == 0, $"100 previews do not consume {first}/{second}");
    var played = Play(preview, second);
    Check(played.Kind == predicted, $"preview matches next manual action {first}/{second}");
}
var cappedPreview = new ComboState();
for (var i = 0; i < 3; i++) { Play(cappedPreview, Element.Wind); Play(cappedPreview, Element.Rock); }
Check(cappedPreview.Preview(Element.Lightning) == ComboKind.None && cappedPreview.Used == 3, "exhausted preview cannot promise a combo");
// Exhaust all six-card elemental sequences against an independent adjacency oracle.
// This catches reuse of the earlier element, accidental non-overlap and cap errors.
var elements = new[] { Element.Wind, Element.Rock, Element.Lightning };
for (var code = 0; code < 729; code++)
{
    var state = new ComboState();
    var remainingCode = code;
    var previous = Element.None;
    var uses = 0;
    for (var index = 0; index < 6; index++)
    {
        var next = elements[remainingCode % 3];
        remainingCode /= 3;
        var expected = ComboKind.None;
        if (previous != Element.None && previous != next && uses < 3)
        {
            expected = previous != Element.Lightning && next != Element.Lightning ? ComboKind.WindRock
                : previous != Element.Wind && next != Element.Wind ? ComboKind.RockLightning : ComboKind.LightningWind;
        }
        var encoded = state.Encoded;
        if (state.Preview(next) != expected || state.Encoded != encoded) throw new InvalidOperationException($"oracle preview {code}/{index}");
        var step = Play(state, next);
        if (expected != ComboKind.None) uses++;
        if (step.Kind != expected || state.Used != uses || state.Last != next) throw new InvalidOperationException($"oracle reserve {code}/{index}");
        previous = next;
    }
}
Check(true, "729 six-card sequences match independent overlapping-pair oracle");
var chain = new ComboState();
Play(chain, Element.Wind); Play(chain, Element.Rock);
Check(chain.Last == Element.Rock && chain.Preview(Element.Wind) == ComboKind.WindRock && chain.Used == 1, "Wind Rock retains Rock and previews second block");
Play(chain, Element.None);
var duplicate = new object();
Check(chain.Reserve(duplicate, Element.Wind).Triggered && !chain.Reserve(duplicate, Element.Wind).Advanced
    && chain.Last == Element.Wind && chain.Used == 2, "ordinary card and duplicate action do not break retained chain");
Check(!chain.Reserve(new object(), Element.Lightning, automatic: true).Advanced && chain.Last == Element.Wind
    && !chain.Reserve(new object(), Element.Lightning, firstInSeries: false).Advanced && chain.Used == 2, "automatic and extra resolution preserve retained chain");
Console.WriteLine($"COMBO_RULES_PASS assertions={passed}");
