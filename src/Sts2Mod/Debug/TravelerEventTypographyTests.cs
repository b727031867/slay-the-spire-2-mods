using System.Text.Json;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Events;
using Sts2Mod.Events;

namespace Sts2Mod.Debug;

public partial class TravelerTestRunner
{
    private async Task TestEventTypography()
    {
        var model = await EventFixture<LastGuide>();
        var links = new[] { ("TC_E03", "SIGN", "TC_R11"), ("TC_E03", "VERIFY", "TC_R12"),
            ("TC_E03", "BUY", "TC_R13"), ("TC_E03", "SIGN_GUEST", "TC_R14"), ("TC_E03", "VERIFY_GUEST", "TC_R15"),
            ("TC_E04", "NAIL", "TC_R08"), ("TC_E04", "SPOOL", "TC_R09"), ("TC_E04", "WATCH", "TC_R10") };
        foreach (var language in new[] { "zhs", "eng" })
        {
            using var events = JsonDocument.Parse(Godot.FileAccess.GetFileAsString($"res://Sts2Mod/localization/{language}/events.json"));
            using var relics = JsonDocument.Parse(Godot.FileAccess.GetFileAsString($"res://Sts2Mod/localization/{language}/relics.json"));
            Check(links.All(link => events.RootElement.GetProperty($"{link.Item1}.pages.INITIAL.options.{link.Item2}.description").GetString()
                == relics.RootElement.GetProperty(link.Item3 + ".description").GetString()), "EVENT_copy_event_relic_consistent_" + language);
            Check(relics.RootElement.GetProperty("TC_R10.description").GetString() == (language == "zhs"
                ? "持有任意罗盘时，每回合连携可用次数变为5次。" : "With either compass, you can use up to 5 combos per turn."),
                "EVENT_copy_watch_concise_" + language);
            var options = events.RootElement.EnumerateObject().Where(p => p.Name.Contains(".options.") && p.Name.EndsWith(".title")).ToArray();
            foreach (var group in options.Chunk(3))
            {
                var container = new VBoxContainer { Size = new Vector2(900, 600) };
                AddChild(container);
                try
                {
                    foreach (var entry in group)
                    {
                        var key = entry.Name[..^6];
                        var option = new EventOption(model, () => Task.CompletedTask, new LocString("events", key + ".title"),
                            new LocString("events", key + ".description"), key, []);
                        var button = NEventOptionButton.Create(model, option, container.GetChildCount());
                        container.AddChild(button);
                        button.GetNode<MegaRichTextLabel>("%Text").Text = $"[gold][b]{entry.Value.GetString()}[/b][/gold]\n"
                            + events.RootElement.GetProperty(key + ".description").GetString();
                    }
                    foreach (var width in new[] { 900, 650, 1000 })
                    {
                        container.Size = new Vector2(width, 600);
                        for (var frame = 0; frame < 4; frame++) await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                        var labels = container.GetChildren().OfType<NEventOptionButton>().Select(b => b.GetNode<MegaRichTextLabel>("%Text")).ToArray();
                        Check(labels.Select(label => label.GetThemeFontSize("normal_font_size")).Distinct().Count() == 1
                            && labels.All(label => label.GetThemeFontSize("normal_font_size") == label.GetThemeFontSize("bold_font_size")),
                            $"EVENT_option_font_uniform_{language}_{group[0].Name}_{width}");
                        Check(labels.All(label => label.GetContentHeight() <= label.Size.Y + 1),
                            $"EVENT_option_text_fits_{language}_{group[0].Name}_{width}");
                    }
                }
                finally { container.Free(); }
            }
        }
    }
}
