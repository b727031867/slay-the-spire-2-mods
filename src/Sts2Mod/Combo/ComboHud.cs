using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2Mod.Combo;

public readonly record struct ComboHudView(Element Current, int Remaining, bool Doubled, bool PlayPhase, ComboVisualPreview? Preview);

/// <summary>Local, read-only combat UI. Owned/freed by the native combat UI, no static player/card references.</summary>
public partial class ComboHud : Control
{
    public const string NodeName = "TravelerComboHud";
    private TextureRect _current = null!;
    private TextureRect _candidate = null!;
    private Label _element = null!;
    private Label _remaining = null!;
    private Label _hint = null!;
    private Label _arrow = null!;
    private ComboPips _pips = null!;
    private readonly Dictionary<ComboKind, (Panel Panel, Label Label)> _rewards = [];
    private string? _language;
    public ComboHudView? LastView { get; private set; }
    public ComboKind HighlightedKind { get; private set; }

    public override void _Ready()
    {
        Name = NodeName;
        MouseFilter = MouseFilterEnum.Ignore;
        FocusMode = FocusModeEnum.None;
        Position = new Vector2(24, 168);
        Size = new Vector2(470, 190);
        ZIndex = 5;
        Visible = false;
        var background = new Panel { Name = "Background", Size = Size, MouseFilter = MouseFilterEnum.Ignore };
        background.AddThemeStyleboxOverride("panel", Style(new Color("111f2bed"), new Color("435d70"), 1));
        AddChild(background);
        _current = Icon("CurrentElement", new(14, 12), new(48, 48));
        AddLabel("CurrentCaption", new(74, 9), new(190, 20), 15).Text = Text("current");
        _element = AddLabel("ElementName", new(74, 28), new(190, 34), 27);
        _pips = new ComboPips { Name = "RemainingPips", Position = new(340, 17), Size = new(110, 16), MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_pips);
        _remaining = AddLabel("Remaining", new(260, 36), new(194, 26), 17);
        _remaining.HorizontalAlignment = HorizontalAlignment.Right;
        _arrow = AddLabel("PreviewArrow", new(20, 74), new(28, 32), 24);
        _arrow.Text = "→";
        _candidate = Icon("CandidateElement", new(56, 73), new(30, 30));
        _hint = AddLabel("PreviewHint", new(100, 69), new(350, 40), 16);
        _hint.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        var index = 0;
        foreach (var kind in new[] { ComboKind.WindRock, ComboKind.RockLightning, ComboKind.LightningWind })
        {
            var chip = new Panel { Name = kind.ToString(), Position = new(14 + index++ * 149, 119), Size = new(143, 55), MouseFilter = MouseFilterEnum.Ignore };
            var icon = new TextureRect { Texture = ComboIcons.RewardTexture(kind), Position = new(8, 12), Size = new(30, 30),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, MouseFilter = MouseFilterEnum.Ignore };
            var label = MakeLabel("Reward", new(45, 5), new(94, 45), 16);
            label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            chip.AddChild(icon);
            chip.AddChild(label);
            AddChild(chip);
            _rewards.Add(kind, (chip, label));
        }
        RefreshNow();
    }

    public override void _Process(double delta) => RefreshNow();

    public void RefreshNow()
    {
        var room = NCombatRoom.Instance;
        if (!RunManager.Instance.IsInProgress || !CombatManager.Instance.IsInProgress || CombatManager.Instance.IsEnding
            || room is null || GetParent() != room.Ui
            || !ReferenceEquals(ActiveScreenContext.Instance.GetCurrentScreen(), room))
        {
            HideAndClear();
            return;
        }
        var player = LocalContext.GetMe(room.CreatureNodes.Select(node => node.Entity))?.Player;
        var power = player?.Creature.GetPower<ComboPower>();
        if (player is null || power is null || !player.Creature.IsAlive)
        {
            HideAndClear();
            return;
        }
        // The native tracker prioritizes selected/dragged cards over hovered ones,
        // including mouse, controller and shortcut selection. Never synthesize input.
        var hovered = RunManager.Instance.HoveredModelTracker.GetHoveredModel(player.NetId);
        ComboVisualPreview? preview = room.Ui.Hand.CurrentMode == NPlayerHand.Mode.Play
            && hovered is ElementCardModel card && card.Owner == player && ComboPresentation.InHand(card)
            ? ComboPresentation.VisualPreview(card) : null;
        var playPhase = RunManager.Instance.ActionQueueSynchronizer.CombatState == ActionSynchronizerCombatState.PlayPhase
            && !CombatManager.Instance.PlayerActionsDisabled && CombatManager.Instance.IsPartOfPlayerTurn(player)
            && room.Ui.Hand.CurrentMode == NPlayerHand.Mode.Play;
        var view = new ComboHudView(power.State.Last, power.State.Remaining, ComboPresentation.HasPaimon(player), playPhase, preview);
        Visible = true;
        if (LastView == view && _language == LocManager.Instance.Language) return;
        LastView = view;
        _language = LocManager.Instance.Language;
        Display(view);
    }

    private void HideAndClear()
    {
        Visible = false;
        LastView = null;
        HighlightedKind = ComboKind.None;
    }

    private void Display(ComboHudView view)
    {
        var font = FontManager.GetSubstituteFont(LocManager.Instance.Language, FontType.Regular) ?? ThemeDB.FallbackFont;
        foreach (var label in FindChildren("*", "Label", recursive: true, owned: false).OfType<Label>())
            label.AddThemeFontOverride("font", font);
        _current.Texture = ComboIcons.ElementTexture(view.Current);
        _element.Text = new LocString("powers", "TC_ELEMENT_" + view.Current.ToString().ToUpperInvariant()).GetFormattedText();
        _element.Modulate = ComboIcons.Color(view.Current);
        GetNode<Label>("CurrentCaption").Text = Text("current");
        var remaining = new LocString("powers", "TC_HUD.remaining");
        remaining.Add("Remaining", view.Remaining);
        _remaining.Text = remaining.GetFormattedText();
        _pips.Remaining = view.Remaining;
        _pips.QueueRedraw();
        HighlightedKind = view.PlayPhase && view.Preview is { HighlightsReward: true } preview ? preview.Kind : ComboKind.None;
        _candidate.Visible = _arrow.Visible = view.Preview.HasValue;
        if (view.Preview is { } candidate) _candidate.Texture = ComboIcons.ElementTexture(candidate.Element);
        _hint.Text = Text(view.Preview?.Key ?? (!view.PlayPhase ? "waiting" : view.Remaining == 0 ? "exhausted" : "choose"));
        _hint.Modulate = HighlightedKind == ComboKind.None ? new Color("bccad6") : ComboIcons.Color(HighlightedKind);
        foreach (var (kind, (panel, label)) in _rewards)
        {
            var active = kind == HighlightedKind;
            var color = ComboIcons.Color(kind);
            panel.AddThemeStyleboxOverride("panel", Style(active ? new Color(color, .24f) : new Color("1c2c3bd9"),
                active ? color : new Color("435566"), active ? 3 : 1));
            panel.Modulate = view.Remaining == 0 ? new Color(.55f, .55f, .55f) : Colors.White;
            var loc = new LocString("powers", "TC_HUD." + kind);
            loc.Add("Bonus", ComboPresentation.Reward(kind, view.Doubled));
            label.Text = loc.GetFormattedText();
            label.Modulate = active ? color : new Color("d7e1e8");
        }
    }

    private TextureRect Icon(string name, Vector2 position, Vector2 size)
    {
        var texture = new TextureRect { Name = name, Position = position, Size = size,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(texture);
        return texture;
    }
    private Label AddLabel(string name, Vector2 position, Vector2 size, int fontSize)
    {
        var label = MakeLabel(name, position, size, fontSize);
        AddChild(label);
        return label;
    }
    private static Label MakeLabel(string name, Vector2 position, Vector2 size, int fontSize)
    {
        var label = new Label { Name = name, Position = position, Size = size, MouseFilter = MouseFilterEnum.Ignore,
            FocusMode = FocusModeEnum.None, VerticalAlignment = VerticalAlignment.Center };
        // Reuse the game's locale font; no proprietary font is copied into this mod.
        label.AddThemeFontOverride("font", FontManager.GetSubstituteFont(LocManager.Instance.Language, FontType.Regular) ?? ThemeDB.FallbackFont);
        label.AddThemeFontSizeOverride("font_size", fontSize);
        label.AddThemeColorOverride("font_color", new Color("edf2f6"));
        return label;
    }
    private static StyleBoxFlat Style(Color background, Color border, int width) => new()
    {
        BgColor = background, BorderColor = border, BorderWidthLeft = width, BorderWidthRight = width,
        BorderWidthTop = width, BorderWidthBottom = width,
        CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10, CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10
    };
    private static string Text(string key) => new LocString("powers", "TC_HUD." + key).GetFormattedText();
}

public partial class ComboPips : Control
{
    public int Remaining { get; set; }
    public override void _Draw()
    {
        for (var i = 0; i < ComboState.Limit; i++)
        {
            var center = new Vector2(14 + 32 * i, 8);
            DrawCircle(center, 7, i < Remaining ? new Color("7ddbc8") : new Color("344454"));
            DrawArc(center, 7, 0, Mathf.Tau, 24, new Color("9cbdc5"), 1, true);
        }
    }
}

[HarmonyPatch(typeof(NCombatUi), nameof(NCombatUi._Ready))]
internal static class ComboHudMountPatch
{
    private static void Postfix(NCombatUi __instance)
    {
        if (__instance.GetNodeOrNull<ComboHud>(ComboHud.NodeName) is null)
            __instance.CallDeferred(Node.MethodName.AddChild, new ComboHud { Name = ComboHud.NodeName });
    }
}

[HarmonyPatch(typeof(NPower), "OnDisplayAmountChanged")]
internal static class ComboPowerIconPatch
{
    private static void Postfix(NPower __instance)
    {
        // Native amount refresh only updates the label, not the icon texture.
        if (__instance.IsNodeReady() && __instance.Model is ComboPower power
            && __instance.GetNodeOrNull<TextureRect>("%Icon") is { } icon)
            icon.Texture = ComboIcons.ElementTexture(power.State.Last);
    }
}
