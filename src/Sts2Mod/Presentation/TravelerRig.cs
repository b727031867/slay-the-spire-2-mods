using Godot;
using Sts2Mod.Combo;

namespace Sts2Mod.Presentation;

/// <summary>Mother-image skinning and authored poses; never owns gameplay or RNG.</summary>
public partial class TravelerRig : Node2D
{
    public const float RigScale=312f/TravelerSkin.Height;
    public string ActionName { get; private set; }="Idle";
    public Element Style { get; set; }
    public bool CardActive { get; set; }
    public bool CardAttacked { get; set; }
    public int ActionSerial { get; private set; }
    public int PartCount=>_skin?.Layers.Count??0;
    internal TravelerSkin Skin=>_skin!;
    internal Element LastDefenseElement { get; set; }
    internal Element LastImpactElement { get; set; }
    internal string LastReaction { get; set; }="None";
    internal int DefenseEvents { get; set; }
    internal int ImpactEvents { get; set; }
    internal int ReactionEvents { get; set; }
    public double Elapsed { get; private set; }
    private TravelerSkin? _skin;
    private double _clock,_blendElapsed,_catchUp;
    private bool _guardOnRecovery,_dead;
    private float _cape,_coat;
    private Pose _pose,_from;
    private const double BlendDuration=.075;
    private record struct Pose(float X=0,float Y=0,float Torso=0,float Head=0,float Arm=0,float Forearm=0,
        float Wrist=0,float FreeArm=0,float FreeForearm=0,float Root=0,
        float Aim=0,float ReachX=493,float ReachY=564,float SwordAngle=0,float Grip=0,float Yaw=1)
    {
        public Pose():this(0,0,0,0,0,0,0,0,0,0,0,493,564,0,0,1) { }
    }
    private readonly record struct Key(float Time,Pose Pose);
    private static readonly Key[] Attack=[new(0,new()),
        new(.25f,new(-8,8,-.015f,.01f,FreeArm:.12f,Aim:1,ReachX:760,ReachY:490,SwordAngle:-2.18f,Yaw:.90f)),
        new(.48f,new(48,22,.17f,-.06f,FreeArm:.20f,Aim:1,ReachX:1050,ReachY:420,SwordAngle:-2.18f,Yaw:.86f)),
        new(.60f,new(48,22,.17f,-.06f,FreeArm:.20f,Aim:1,ReachX:1050,ReachY:420,SwordAngle:-2.18f,Yaw:.86f)),
        new(.82f,new(3,6,.02f,0,FreeArm:.05f,Aim:1,ReachX:775,ReachY:490,SwordAngle:-2.18f,Yaw:.94f)),new(1,new())];
    private static readonly Key[] Guard=[new(0,new()),
        new(.24f,new(-5,14,-.015f,Aim:1,ReachX:850,ReachY:475,SwordAngle:-2.70f,Grip:.8f,Yaw:.94f)),
        new(.40f,new(-8,25,-.035f,.015f,Aim:1,ReachX:900,ReachY:445,SwordAngle:-3.75f,Grip:1,Yaw:.92f)),
        new(.66f,new(-8,25,-.035f,.015f,Aim:1,ReachX:900,ReachY:445,SwordAngle:-3.75f,Grip:1,Yaw:.92f)),
        new(.84f,new(-2,9,-.01f,Aim:1,ReachX:795,ReachY:505,SwordAngle:-2.30f,Grip:.35f,Yaw:.97f)),new(1,new())];

    public override void _Ready() { _skin=new TravelerSkin(this); ResetForPreview(); }
    public double Duration=>ActionName switch { "Attack"=>.60,"Defend"=>.62,"BlockHit"=>.22,
        "Hit" or "PartialHit"=>.36,"LifeLoss"=>.22,"Dead"=>.60,_=>.44 };
    public void PlayAction(string action)
    {
        action=action switch { "revive"=>"Revive","Relaxed"=>"Idle",_=>action };
        if(_dead&&action!="Revive") return;
        var revive=action=="Revive";
        if(revive) { _dead=false; action="Idle"; }
        if(action=="Dead") _dead=true;
        if(action=="Attack"&&CardActive&&CardAttacked) return;
        if(action=="Attack") CardAttacked=true;
        if(action=="Defend"&&ActionName=="Attack"&&CardActive&&CardAttacked) { _guardOnRecovery=true; return; }
        if(action=="Idle"&&!revive&&ActionName!="Idle"&&Elapsed<Duration) return;
        _from=_pose; _blendElapsed=0; _catchUp=0; _guardOnRecovery=false;
        var switchWeapon=ActionName is "Attack" or "Defend" && action is "Attack" or "Defend";
        ActionName=action; Elapsed=switchWeapon?Duration*.24:0; ActionSerial++;
        if(action=="Attack"&&_skin is not null)
        {
            var trail=CombatElementVfx.Spawn(_skin.Bones["Sword"],Style,CombatEffect.Trail);
            trail.Scale=Vector2.One/RigScale;
        }
        if(IsNodeReady()) ApplyPose(0);
    }
    public void Contact() { if(ActionName=="Attack") _catchUp=Math.Max(0,Duration*.48-Elapsed); }
    public override void _Process(double delta)=>Advance(delta);
    public void Advance(double delta)
    {
        _clock+=delta;
        var step=delta*CombatPresentation.Speed;
        var extra=Math.Min(_catchUp,step*1.5); _catchUp-=extra;
        Elapsed+=step+extra; _blendElapsed+=step;
        if(_guardOnRecovery&&ActionName=="Attack"&&Elapsed>=Duration*.60)
        {
            _from=_pose; _blendElapsed=0; _guardOnRecovery=false; ActionName="Defend"; Elapsed=Duration*.24;
        }
        else if(!_dead&&ActionName!="Idle"&&Elapsed>=Duration)
        {
            var next=_guardOnRecovery?"Defend":"Idle";
            _guardOnRecovery=false; _from=_pose; _blendElapsed=0; ActionName=next; Elapsed=0;
            if(delta>=BlendDuration) _blendElapsed=BlendDuration;
        }
        ApplyPose(delta);
    }
    private static Pose Lerp(Pose a,Pose b,float t)=>new(
        Mathf.Lerp(a.X,b.X,t),Mathf.Lerp(a.Y,b.Y,t),Mathf.Lerp(a.Torso,b.Torso,t),Mathf.Lerp(a.Head,b.Head,t),
        Mathf.Lerp(a.Arm,b.Arm,t),Mathf.Lerp(a.Forearm,b.Forearm,t),Mathf.Lerp(a.Wrist,b.Wrist,t),
        Mathf.Lerp(a.FreeArm,b.FreeArm,t),Mathf.Lerp(a.FreeForearm,b.FreeForearm,t),Mathf.Lerp(a.Root,b.Root,t),
        Mathf.Lerp(a.Aim,b.Aim,t),Mathf.Lerp(a.ReachX,b.ReachX,t),Mathf.Lerp(a.ReachY,b.ReachY,t),
        Mathf.Lerp(a.SwordAngle,b.SwordAngle,t),Mathf.Lerp(a.Grip,b.Grip,t),Mathf.Lerp(a.Yaw,b.Yaw,t));
    private static Pose Sample(Key[] keys,float t)
    {
        for(var i=1;i<keys.Length;i++)
            if(t<=keys[i].Time) return Lerp(keys[i-1].Pose,keys[i].Pose,TravelerSkin.Smooth(keys[i-1].Time,keys[i].Time,t));
        return keys[^1].Pose;
    }
    private void ApplyPose(double delta)
    {
        if(_skin is null) return;
        var t=(float)Math.Clamp(Elapsed/Duration,0,1); var pulse=Mathf.Sin(t*Mathf.Pi);
        var target=ActionName switch {
            "Attack"=>Sample(Attack,t),"Defend"=>Sample(Guard,t),
            "BlockHit"=>new Pose(-4*pulse,9*pulse,-.02f*pulse,0,-.45f*pulse,-.55f*pulse,-.20f*pulse),
            "Hit" or "PartialHit"=>new Pose(-18*pulse,8*pulse,-.21f*pulse,.025f*pulse,.06f*pulse,Wrist:.20f*pulse,Yaw:1-.10f*pulse),
            "Cast" or "PowerUp"=>new Pose(0,4*pulse,.01f*pulse,-.01f*pulse,0,0,0,-.30f*pulse,-.55f*pulse),
            "Dead"=>new Pose(-15*t,15*t,-.04f*t,0,0,0,0,0,0,-1.35f*t),_=>new Pose() };
        _pose=Lerp(_from,target,TravelerSkin.Smooth(0,(float)BlendDuration,(float)_blendElapsed));
        var breath=(float)Math.Sin(_clock*2.4)*1.5f;
        _skin.ResetBones(); var bones=_skin.Bones;
        bones["Pelvis"].Position+=new Vector2(_pose.X,_pose.Y);
        bones["Torso"].Rotation=_pose.Torso; bones["Torso"].Position+=new Vector2(0,breath);
        bones["Torso"].Scale=new Vector2(_pose.Yaw,1);
        bones["Head"].Rotation=_pose.Head; bones["SwordArm"].Rotation=_pose.Arm;
        bones["SwordForearm"].Rotation=_pose.Forearm; bones["Hand"].Rotation=_pose.Wrist;
        bones["FreeArm"].Rotation=_pose.FreeArm; bones["FreeForearm"].Rotation=_pose.FreeForearm;
        _skin.AimArm(true,new Vector2(_pose.ReachX,_pose.ReachY),_pose.SwordAngle,_pose.Aim);
        // Supporting palm meets the same handle, below the dominant palm in guard.
        var hand=bones["Hand"];
        var gripTarget=_skin.Canvas.ToLocal(hand.ToGlobal(new Vector2(0,-45)));
        _skin.AimArm(false,gripTarget,_pose.SwordAngle,_pose.Grip);
        _cape=Mathf.Lerp(_cape,-_pose.Torso*.32f-_pose.Arm*.025f,1-(float)Math.Exp(-delta*9));
        _coat=Mathf.Lerp(_coat,-_pose.Torso*.18f,1-(float)Math.Exp(-delta*14));
        bones["Cape"].Rotation=_cape+(float)Math.Sin(_clock*2.4-.7)*.004f; bones["Coat"].Rotation=_coat;
        _skin.PlantLeg("Left"); _skin.PlantLeg("Right");
        Position=Vector2.Zero; Rotation=_pose.Root; Scale=Vector2.One*RigScale;
        Modulate=ActionName is "Hit" or "PartialHit" or "LifeLoss"?new Color(1,1-.12f*pulse,1-.14f*pulse):Colors.White;
        _skin.Update();
    }
    internal void ResetForPreview()
    {
        ActionName="Idle"; Elapsed=0; _clock=0; _catchUp=0; _dead=false; _guardOnRecovery=false;
        _blendElapsed=BlendDuration; _pose=new(); _from=new(); _cape=0; _coat=0;
        CardActive=false; CardAttacked=false; Position=Vector2.Zero; Rotation=0; Scale=Vector2.One*RigScale;
        if(_skin is null) return; _skin.ResetBones(); _skin.Update();
    }
    internal float[] FrameVertices()=>_skin!.Vertices.SelectMany(p=>new[]{(p.X-TravelerSkin.Origin.X)*RigScale,(p.Y-TravelerSkin.Origin.Y)*RigScale}).ToArray();
}
