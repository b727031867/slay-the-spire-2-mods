using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace Sts2Mod.Combo;

/// <summary>Pure presentation: finite, nonblocking, shader-free and silent.</summary>
public partial class ElementComboVfx : Node2D
{
    public ComboKind Kind { get; init; }
    public bool Doubled { get; init; }
    private double _elapsed;
    public const double Duration = 0.55;

    public static void Show(Creature target, ComboKind kind, bool doubled)
    {
        if (DisplayServer.GetName() == "headless") return;
        var anchor = target.GetCreatureNode();
        var container = target.GetVfxContainer();
        if (anchor is null || container is null || !GodotObject.IsInstanceValid(anchor) || !anchor.IsInsideTree()) return;
        var effect = new ElementComboVfx { Kind = kind, Doubled = doubled, ZIndex = 5 };
        container.AddChild(effect);
        effect.GlobalPosition = anchor.VfxSpawnPosition;
    }

    public override void _Process(double delta)
    {
        _elapsed += delta;
        if (_elapsed >= Duration) { QueueFree(); return; }
        var progress = (float)(_elapsed / Duration);
        Modulate = new Color(1, 1, 1, (1 - progress) * 0.85f);
        Scale = Vector2.One * (0.8f + progress * 0.45f);
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
        if (Kind == ComboKind.WindRock)
        {
            Vector2[] shield = [new(-38,-35), new(0,-50), new(38,-35), new(30,20), new(0,45), new(-30,20), new(-38,-35)];
            DrawPolyline(shield, color, 5, true);
            DrawLine(new(-25,-12),new(25,-12),color,3,true);
        }
        else if (Kind == ComboKind.RockLightning)
        {
            Vector2[] bolt = [new(15,-55), new(-18,-5), new(8,-5), new(-15,45)];
            DrawPolyline(bolt, color, 6, true);
            DrawArc(Vector2.Zero,44,0,Mathf.Tau,28,new Color(color,0.5f),2,true);
        }
        else
        {
            DrawArc(new(0,-12),35,-2.8f,0.5f,24,color,4,true);
            DrawArc(new(0,12),45,-0.2f,2.9f,24,color,4,true);
        }
        if (Doubled)
        {
            var gold = new Color("f4d78d");
            DrawArc(Vector2.Zero,60,-Mathf.Pi,Mathf.Pi,36,gold,2,true);
            DrawLine(new(48,-54),new(48,-34),gold,3,true);
            DrawLine(new(38,-44),new(58,-44),gold,3,true);
        }
    }
}
