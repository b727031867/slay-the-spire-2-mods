using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Sts2Mod.Combo;

/// <summary>Pure presentation: finite, nonblocking, shader-free and silent.</summary>
public partial class ElementComboVfx : Node2D
{
    public ComboKind Kind { get; private set; }
    public bool Doubled { get; private set; }
    public bool Blocked { get; private set; }
    private double _elapsed;
    public const double Duration = 0.22;
    public const int MaxEffectsPerAnchor = 1;
    public const float PeakOpacity = 0.55f;
    public const float MaxRadius = 28f;

    public static ElementComboVfx Spawn(Node parent, ComboKind kind, bool doubled, bool blocked = false)
    {
        var effect = parent.GetChildren().OfType<ElementComboVfx>().FirstOrDefault(v => !v.IsQueuedForDeletion());
        if (effect is null)
        {
            effect = new ElementComboVfx { ZIndex = 5 };
            parent.AddChild(effect);
        }
        effect.Kind = kind;
        effect.Doubled = doubled;
        effect.Blocked = blocked;
        effect._elapsed = 0;
        effect.Scale = Vector2.One;
        effect.Modulate = new Color(1, 1, 1, PeakOpacity);
        effect.QueueRedraw();
        return effect;
    }

    public static void Show(Creature target, ComboKind kind, bool doubled, bool blocked = false)
    {
        var anchor = target.GetCreatureNode();
        var container = target.GetVfxContainer();
        if (anchor is null || container is null || !GodotObject.IsInstanceValid(anchor) || !anchor.IsInsideTree()) return;
        var effect = Spawn(container, kind, doubled, blocked);
        effect.GlobalPosition = anchor.VfxSpawnPosition;
    }

    public override void _Process(double delta)
    {
        _elapsed += delta;
        if (_elapsed >= Duration) { QueueFree(); return; }
        var progress = (float)(_elapsed / Duration);
        Modulate = new Color(1, 1, 1, (1 - progress) * PeakOpacity);
        QueueRedraw();
    }

    public override void _Draw()
    {
        var color = Kind switch
        {
            ComboKind.WindRock => new Color("eac27c"),
            ComboKind.RockLightning => new Color("bea4ea"),
            _ => new Color("7ddbc8")
        };
        if (Blocked)
        {
            DrawArc(new(-6,0),20,-1.1f,1.1f,16,color,2,true);
        }
        else if (Kind == ComboKind.WindRock)
        {
            DrawPolyline([new(-18,-16),new(0,-23),new(18,-16),new(14,10),new(0,22),new(-14,10)], color, Doubled ? 2.2f : 1.8f, true);
        }
        else if (Kind == ComboKind.RockLightning)
        {
            DrawPolyline([new(9,-23),new(-10,-2),new(5,-2),new(-8,22)], color, Doubled ? 2.2f : 1.8f, true);
        }
        else
        {
            DrawArc(new(0,-5),18,-2.8f,0.5f,18,color,1.8f,true);
            DrawArc(new(0,5),20,-0.2f,2.9f,18,color,1.8f,true);
        }
    }
}
