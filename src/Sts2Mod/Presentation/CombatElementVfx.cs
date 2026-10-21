using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using Sts2Mod.Combo;

namespace Sts2Mod.Presentation;

public enum CombatEffect { Attack, Defend, Contact, Hurt, Trail }

/// <summary>Bounded local feedback with distinct wind curves, rock facets and lightning segments.</summary>
public partial class CombatElementVfx : Node2D
{
    public Element Element { get; init; }
    public CombatEffect Effect { get; init; }
    public const double Lifetime = 0.38;
    public const int MaxEffectsPerAnchor = 6;
    private double _elapsed;

    public static void Show(Creature creature, Element element, CombatEffect effect)
    {
        var anchor = creature.GetCreatureNode();
        // Headless exercises nodes/lifetimes too; drawing is left to Godot and is not asserted visually.
        if (anchor is null || !GodotObject.IsInstanceValid(anchor) || !anchor.IsInsideTree()) return;
        var parent = anchor.Visuals;
        var node = Spawn(parent, element, effect);
        node.GlobalPosition = anchor.VfxSpawnPosition;
    }

    public static CombatElementVfx Spawn(Node parent, Element element, CombatEffect effect)
    {
        var existing = parent.GetChildren().OfType<CombatElementVfx>().Where(v => !v.IsQueuedForDeletion()).ToArray();
        if (existing.Length >= MaxEffectsPerAnchor) existing[0].QueueFree();
        var node = new CombatElementVfx { Element = element, Effect = effect, ZIndex = 4 };
        parent.AddChild(node);
        return node;
    }

    public override void _Process(double delta)
    {
        _elapsed += delta * CombatPresentation.Speed;
        if (_elapsed >= Lifetime) { QueueFree(); return; }
        var t = (float)(_elapsed / Lifetime);
        Modulate = new Color(1, 1, 1, Mathf.Sin(Mathf.Pi * t) * 0.85f);
        Scale = Vector2.One * (0.85f + t * 0.3f);
        QueueRedraw();
    }

    public override void _Draw()
    {
        var t = (float)Math.Clamp(_elapsed / Lifetime, 0, 1);
        var color = Element switch
        {
            Element.Wind => new Color("79d9c8"), Element.Rock => new Color("eac27c"),
            Element.Lightning => new Color("bea4ea"), _ => new Color("e8e8df")
        };
        if (Effect == CombatEffect.Hurt)
        {
            DrawArc(Vector2.Zero, 22 + t * 8, -2.3f, -0.8f, 12, new Color("dfb29b"), 3, true);
            return;
        }
        if (Effect == CombatEffect.Contact)
        {
            DrawArc(new(-12, 0), 42 + t * 5, -1.1f, 1.1f, 20, color, 3, true);
            for (var i = 0; i < 3; i++)
                DrawLine(new(28, -12 + i * 12), new(36 + t * 12, -18 + i * 18), color, 2, true);
            return;
        }
        if (Effect == CombatEffect.Trail)
        {
            // Local to the sword: follows the hand, rather than a detached full-screen slash.
            DrawArc(new(0, 35), 40, -1.8f, 0.3f + t, 24, color, 3, true);
            return;
        }
        var defend = Effect == CombatEffect.Defend;
        switch (Element)
        {
            case Element.Wind:
                DrawArc(new(defend ? 5 : -10, -12), defend ? 52 : 44, -2.8f + t * 0.5f,
                    defend ? 0.3f : -0.2f, 28, color, 4, true);
                DrawArc(new(0, 12), defend ? 44 : 30, -0.1f, 2.8f - t * 0.4f, 24, color, 2, true);
                if (!defend) DrawLine(new(-40 + t * 25, 24), new(42 + t * 12, -24), color, 3, true);
                break;
            case Element.Rock:
                if (defend)
                {
                    for (var i = -1; i <= 1; i++)
                    {
                        var x = i * (24 - t * 8); var y = Math.Abs(i) * 10 + (1 - t) * 18;
                        Vector2[] plate = [new(x - 15, y - 40), new(x + 15, y - 30), new(x + 13, y + 22), new(x, y + 40), new(x - 13, y + 22)];
                        DrawColoredPolygon(plate, new Color(color, 0.16f));
                        DrawPolyline([.. plate, plate[0]], color, 3, true);
                    }
                }
                else
                {
                    DrawPolyline([new(-32, -32), new(0, -12), new(28, 24), new(4, 12), new(-32, -32)], color, 4, true);
                    for (var i = 0; i < 5; i++)
                    {
                        var a = i * 1.24f;
                        var p = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (18 + t * 26);
                        DrawColoredPolygon([p, p + new Vector2(7, -3), p + new Vector2(3, 6)], color);
                    }
                }
                break;
            case Element.Lightning:
                if (defend)
                {
                    Vector2[] nodes = [new(-25, -40), new(15, -48), new(40, -12), new(26, 32), new(-14, 40)];
                    DrawPolyline(nodes, color, 3, true);
                    DrawPolyline([nodes[0], new(12, -10), nodes[3], nodes[1], new(-5, 8), nodes[4]], color, 2, true);
                    foreach (var p in nodes) DrawCircle(p, 3, color);
                }
                else
                {
                    DrawPolyline([new(-32, 24), new(-8, -6), new(4, 1), new(30, -32)], color, 4, true);
                    DrawPolyline([new(-8, -6), new(-17, -24), new(-10, -37)], color, 2, true);
                    DrawLine(new(4, 1), new(26, 12), color, 2, true);
                }
                break;
            default:
                if (defend) DrawArc(new(-8, 0), 48, -1.25f, 1.25f, 24, color, 3, true);
                else
                {
                    DrawLine(new(-28, 25), new(28, -25), color, 3, true);
                    DrawLine(new(-12, -18), new(12, 18), color, 2, true);
                }
                break;
        }
    }
}
