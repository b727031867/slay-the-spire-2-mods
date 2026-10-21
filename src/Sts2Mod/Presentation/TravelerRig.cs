using Godot;
using Sts2Mod.Combo;

namespace Sts2Mod.Presentation;

/// <summary>A small deterministic cutout rig. Owns visual state only, never gameplay or RNG.</summary>
public partial class TravelerRig : Node2D
{
    public const string AtlasPath = "res://Sts2Mod/character/layers/atlas.png";
    public const float RigScale = 312f / 344f;
    public string ActionName { get; private set; } = "Idle";
    public Element Style { get; set; }
    public bool CardActive { get; set; }
    public bool CardAttacked { get; set; }
    public int ActionSerial { get; private set; }
    public int PartCount => _parts.Count;
    internal Element LastDefenseElement { get; set; }
    internal Element LastImpactElement { get; set; }
    internal string LastReaction { get; set; } = "None";
    internal int DefenseEvents { get; set; }
    internal int ImpactEvents { get; set; }
    internal int ReactionEvents { get; set; }
    public double Elapsed { get; private set; }
    private readonly List<Sprite2D> _parts = [];
    private Node2D _torso = null!, _head = null!, _cape = null!, _coat = null!;
    private Node2D _arm = null!, _forearm = null!, _sword = null!, _freeArm = null!, _freeForearm = null!;
    private Node2D _leftLeg = null!, _rightLeg = null!;
    private double _clock;
    private bool _guardOnRecovery;
    private bool _dead;

    public override void _Ready()
    {
        var atlas = ResourceLoader.Load<Texture2D>(AtlasPath);
        // Explicit regions account for the generated atlas's margins and the sword hilt
        // extending above its nominal cell. No GPU readback (also works with headless textures).
        Rect2[] regions = [new(56,43,280,291), new(384,0,311,363), new(724,33,350,306),
            new(1070,35,375,330), new(70,377,215,321), new(477,374,142,347),
            new(850,340,109,377), new(1197,376,195,311), new(77,722,218,342),
            new(443,723,180,355), new(838,723,217,340)];
        Node2D Joint(string name, Node parent, Vector2 position)
        {
            var joint = new Node2D { Name = name, Position = position };
            parent.AddChild(joint);
            return joint;
        }
        void Part(Node2D joint, int slot, float height, Vector2 offset)
        {
            var region = regions[slot];
            var texture = new AtlasTexture { Atlas = atlas, Region = region, FilterClip = true };
            var scale = height / region.Size.Y;
            var sprite = new Sprite2D { Name = "Art", Texture = texture, Centered = false,
                Scale = Vector2.One * scale, Position = offset + new Vector2(-region.Size.X * scale / 2, 0) };
            joint.AddChild(sprite);
            _parts.Add(sprite);
        }
        _leftLeg = Joint("LeftLeg", this, new(-25, -116)); Part(_leftLeg, 9, 116, Vector2.Zero);
        _rightLeg = Joint("RightLeg", this, new(28, -116)); Part(_rightLeg, 10, 116, Vector2.Zero);
        _torso = Joint("Torso", this, new(0, -160));
        _cape = Joint("Cape", _torso, new(-28, -108)); Part(_cape, 2, 105, new(-60, 0));
        _freeArm = Joint("FreeArm", _torso, new(38, -100)); Part(_freeArm, 7, 52, new(0, -6));
        _freeForearm = Joint("FreeForearm", _freeArm, new(0, 41)); Part(_freeForearm, 8, 53, new(0, -6));
        _coat = Joint("Coat", _torso, new(0, -3)); Part(_coat, 3, 145, new(0, -8));
        var chest = Joint("Chest", _torso, Vector2.Zero); Part(chest, 1, 125, new(0, -125));
        _head = Joint("Head", _torso, new(9, -119)); Part(_head, 0, 76, new(0, -65));
        _arm = Joint("SwordArm", _torso, new(-37, -100)); Part(_arm, 4, 55, new(0, -9));
        _forearm = Joint("SwordForearm", _arm, new(0, 41)); Part(_forearm, 5, 55, new(0, -8));
        _sword = Joint("Sword", _forearm, new(0, 39)); Part(_sword, 6, 128, new(0, -13));
        SamplePose();
    }

    public void PlayAction(string action)
    {
        action = action switch { "revive" => "Revive", "Relaxed" => "Idle", _ => action };
        if (_dead && action is not ("Revive" or "Idle")) return;
        if (_dead && action == "Idle") return;
        if (action == "Revive") { _dead = false; action = "Idle"; }
        if (action == "Dead") _dead = true;
        if (action == "Attack" && CardActive && CardAttacked) return;
        if (action == "Attack") CardAttacked = true;
        if (action == "Defend" && ActionName == "Attack") { _guardOnRecovery = true; return; }
        if (action == "Idle" && ActionName != "Idle") return;
        _guardOnRecovery = false;
        ActionName = action;
        Elapsed = 0;
        ActionSerial++;
        if (action == "Attack" && _sword is not null) CombatElementVfx.Spawn(_sword, Style, CombatEffect.Trail);
        if (IsNodeReady()) SamplePose();
    }

    public void Contact()
    {
        // Actual damage is authoritative; bring the sword to contact if native fast mode lands early.
        if (ActionName == "Attack") Elapsed = Math.Max(Elapsed, Duration * 0.43);
    }

    public double Duration => ActionName switch
    {
        "Attack" => 0.4, "Defend" => 0.34, "BlockHit" => 0.2,
        "PartialHit" or "Hit" => 0.3, "LifeLoss" => 0.22, "Dead" => 0.6, _ => 0.4
    };

    public override void _Process(double delta) => Advance(delta);

    // Also used by headless tests to sample animation without depending on frame timing.
    public void Advance(double delta)
    {
        _clock += delta;
        Elapsed += delta * CombatPresentation.Speed;
        if (!_dead && ActionName != "Idle" && Elapsed >= Duration)
        {
            ActionName = _guardOnRecovery ? "Defend" : "Idle";
            _guardOnRecovery = false;
            Elapsed = 0;
        }
        SamplePose();
    }

    private void SamplePose()
    {
        if (_torso is null) return;
        var t = (float)Math.Clamp(Elapsed / Duration, 0, 1);
        var pulse = Mathf.Sin(t * Mathf.Pi);
        var breath = (float)Math.Sin(_clock * 2.4);
        Position = Vector2.Zero; Rotation = 0; Scale = Vector2.One * RigScale; Modulate = Colors.White;
        _torso.Position = new(0, -160 + breath * 0.7f); _torso.Rotation = 0;
        _head.Rotation = breath * 0.006f;
        _cape.Rotation = breath * 0.025f; _coat.Rotation = -breath * 0.009f;
        _arm.Rotation = 0.28f; _forearm.Rotation = 0.1f; _sword.Rotation = 0.2f;
        _freeArm.Rotation = -0.06f; _freeForearm.Rotation = -0.08f;
        _leftLeg.Rotation = 0.03f; _rightLeg.Rotation = -0.03f;
        switch (ActionName)
        {
            case "Attack":
                var swing = t < 0.23f ? Mathf.Lerp(0.28f, 0.75f, t / 0.23f)
                    : t < 0.48f ? Mathf.Lerp(0.75f, -1.8f, (t - 0.23f) / 0.25f)
                    : Mathf.Lerp(-1.8f, 0.28f, (t - 0.48f) / 0.52f);
                _arm.Rotation = swing;
                _forearm.Rotation = -0.5f * pulse;
                _sword.Rotation = 0.2f - 0.8f * pulse;
                _torso.Rotation = 0.1f * pulse; _torso.Position += new Vector2(9 * pulse, 2 * pulse);
                _head.Rotation = -0.08f * pulse; _cape.Rotation += 0.14f * pulse;
                _coat.Rotation -= 0.08f * pulse; _freeArm.Rotation += 0.25f * pulse;
                if (Style == Element.Rock)
                {
                    _arm.Rotation += 0.3f * pulse;
                    _torso.Position += new Vector2(0, 3 * pulse);
                    _sword.Rotation += 0.3f * pulse;
                }
                else if (Style == Element.Lightning)
                {
                    _arm.Rotation = 0.28f - 1.6f * pulse;
                    _forearm.Rotation = 0.1f - 0.3f * pulse;
                    _sword.Rotation = 0.2f - 0.4f * pulse;
                    _torso.Position += new Vector2(4 * pulse, -pulse);
                }
                break;
            case "Defend": case "BlockHit":
                _arm.Rotation -= 1.05f * pulse; _forearm.Rotation -= 0.9f * pulse;
                _sword.Rotation -= 0.5f * pulse; _freeArm.Rotation -= 0.65f * pulse;
                _torso.Position += new Vector2(-3 * pulse, 4 * pulse); _head.Rotation = -0.05f * pulse;
                if (ActionName == "Defend" && Style == Element.Wind) _freeForearm.Rotation -= 0.7f * pulse;
                if (ActionName == "Defend" && Style == Element.Rock) _torso.Position += new Vector2(0, 2 * pulse);
                if (ActionName == "Defend" && Style == Element.Lightning) _sword.Rotation -= 0.3f * pulse;
                break;
            case "PartialHit": case "Hit":
                _torso.Rotation = -0.13f * pulse; _torso.Position += new Vector2(-7 * pulse, 2 * pulse);
                _head.Rotation = -0.1f * pulse; _arm.Rotation += 0.18f * pulse;
                _cape.Rotation -= 0.13f * pulse; _freeForearm.Rotation += 0.18f * pulse;
                if (ActionName == "PartialHit") _forearm.Rotation -= 0.5f * pulse;
                Modulate = new Color(1, 1 - 0.13f * pulse, 1 - 0.16f * pulse);
                break;
            case "LifeLoss": Modulate = new Color(1, 1 - 0.12f * pulse, 1 - 0.12f * pulse); break;
            case "Dead": Rotation = -1.35f * t; Position = new(-12 * t, 12 * t); _head.Rotation = -0.15f * t; break;
            case "Cast": case "PowerUp":
                _freeArm.Rotation -= 0.9f * pulse; _freeForearm.Rotation -= 0.5f * pulse;
                _head.Rotation = -0.05f * pulse; _cape.Rotation += 0.08f * pulse;
                break;
        }
    }
}
