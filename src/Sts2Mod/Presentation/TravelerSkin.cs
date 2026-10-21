using Godot;

namespace Sts2Mod.Presentation;

/// <summary>One registered mother image, shared mesh seams, explicit anatomical bind points.</summary>
internal sealed class TravelerSkin
{
    internal const string TexturePath="res://Sts2Mod/character/body.png";
    internal const int Width=1237, Height=1271;
    internal static readonly Vector2 Origin=new(760,1255);
    internal readonly Node2D Canvas;
    internal readonly Dictionary<string,Node2D> Bones=[];
    internal readonly Dictionary<string,Vector2> Bind=[];
    internal readonly Vector2[] Uv;
    internal Vector2[] Vertices { get; private set; }
    internal readonly List<Polygon2D> Layers=[];
    internal readonly List<int[]>[] Triangles=[[],[],[],[],[],[],[]];
    private readonly (string name,float weight)[][] _weights;

    internal TravelerSkin(Node2D owner)
    {
        Canvas=new Node2D {Name="Mother",Position=-Origin}; owner.AddChild(Canvas);
        void Bone(string name,string? parent,Vector2 bind)
        {
            var node=new Node2D {Name=name,Position=bind-(parent is null?Vector2.Zero:Bind[parent])};
            (parent is null?Canvas:Bones[parent]).AddChild(node); Bones.Add(name,node); Bind.Add(name,bind);
        }
        Bone("Pelvis",null,new(760,620)); Bone("Torso","Pelvis",new(760,550)); Bone("Head","Torso",new(790,190));
        Bone("SwordArm","Torso",new(650,300)); Bone("SwordForearm","SwordArm",new(545,445));
        Bone("Hand","SwordForearm",new(493,564)); Bone("Sword","Hand",new(507,593));
        Bone("FreeArm","Torso",new(875,307)); Bone("FreeForearm","FreeArm",new(925,460)); Bone("FreeHand","FreeForearm",new(997,586));
        Bone("LeftThigh","Pelvis",new(720,620)); Bone("LeftShin","LeftThigh",new(650,920)); Bone("LeftFoot","LeftShin",new(525,1175));
        Bone("RightThigh","Pelvis",new(825,620)); Bone("RightShin","RightThigh",new(930,905)); Bone("RightFoot","RightShin",new(940,1100));
        Bone("Cape","Torso",new(680,205)); Bone("Coat","Pelvis",new(760,550));
        // Every source cell occurs once. Layers share skinned vertices at seams.
        var xs=Enumerable.Range(0,Width/28+1).Select(i=>(float)(i*28)).Append(Width).Distinct().ToArray();
        var ys=Enumerable.Range(0,Height/28+1).Select(i=>(float)(i*28)).Append(Height).Distinct().ToArray();
        var grid=ys.SelectMany(y=>xs.Select(x=>new Vector2(x,y))).ToArray();
        Uv=grid.Concat(Weapon).ToArray(); Vertices=Uv.ToArray();
        _weights=grid.Select(Weights).Concat(Weapon.Select(_=>new[]{("Hand",1f)})).ToArray();
        for(var y=0;y<ys.Length-1;y++) for(var x=0;x<xs.Length-1;x++)
        {
            var a=y*xs.Length+x; var b=a+1; var c=a+xs.Length; var d=c+1;
            var layer=Layer((Uv[a]+Uv[d])/2); Triangles[layer].Add([a,b,d]); Triangles[layer].Add([a,d,c]);
        }
        var weaponTriangles=Geometry2D.TriangulatePolygon(Weapon);
        for(var i=0;i<weaponTriangles.Length;i+=3)
            Triangles[6].Add([grid.Length+weaponTriangles[i],grid.Length+weaponTriangles[i+1],grid.Length+weaponTriangles[i+2]]);
        var texture=ResourceLoader.Load<Texture2D>(TexturePath);
        var shader=new Shader { Code=WeaponMaskShader };
        var material=new ShaderMaterial { Shader=shader };
        material.SetShaderParameter("weapon_polygon",Weapon);
        string[] names=["CapeArt","BodyArt","FreeArmArt","SwordArmArt","HandArt","CoatArt","WeaponArt"];
        int[] z=[-2,0,1,2,3,-1,4];
        for(var i=0;i<names.Length;i++)
        {
            var polygons=new Godot.Collections.Array();
            foreach(var triangle in Triangles[i]) polygons.Add(triangle);
            var layer=new Polygon2D {Name=names[i],Texture=texture,Polygon=Vertices,UV=Uv,Polygons=polygons,ZIndex=z[i],
                Material=i==6?null:material};
            Canvas.AddChild(layer); Layers.Add(layer);
        }
    }
    internal static float Smooth(float a,float b,float value)
    {
        var t=Math.Clamp((value-a)/(b-a),0,1); return t*t*(3-2*t);
    }
    private static bool Inside(Vector2 p,Vector2[] polygon)=>Geometry2D.IsPointInPolygon(p,polygon);
    private static bool Near(Vector2 p,Vector2[] polygon)
    {
        if(Inside(p,polygon))return true;
        for(var i=0;i<polygon.Length;i++)
            if(Geometry2D.GetClosestPointToSegment(p,polygon[i],polygon[(i+1)%polygon.Length]).DistanceTo(p)<28)return true;
        return false;
    }
    internal static readonly Vector2[] Weapon=[new(10,1240),new(376,708),new(411,663),new(384,638),new(385,610),
        new(433,604),new(450,584),new(457,554),new(499,548),new(529,537),new(546,508),new(579,509),
        new(589,539),new(550,575),new(541,619),new(519,653),new(516,680),new(469,688),new(444,714),new(80,1240)];
    private const string WeaponMaskShader="""
        shader_type canvas_item;
        uniform vec2 weapon_polygon[20];
        varying vec4 tint;
        void vertex(){tint=COLOR;}
        void fragment(){
            vec2 p=UV*vec2(1237.0,1271.0);bool inside=false;int j=19;
            for(int i=0;i<20;i++){
                vec2 a=weapon_polygon[i];vec2 b=weapon_polygon[j];
                if((a.y>p.y)!=(b.y>p.y)){
                    if(p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x)inside=!inside;
                }
                j=i;
            }
            if(inside)discard;
            COLOR=texture(TEXTURE,UV)*tint;
        }
        """;
    private static readonly Vector2[] Sleeve=[new(558,305),new(677,302),new(646,384),new(614,439),new(560,477),new(473,446),new(478,392),new(526,354)];
    private static readonly Vector2[] Forearm=[new(512,420),new(589,455),new(544,550),new(529,582),new(463,610),new(448,553),new(475,475)];
    private static readonly Vector2[] FreeSleeve=[new(882,280),new(910,305),new(935,380),new(948,440),new(1003,545),
        new(1050,600),new(1037,669),new(999,669),new(974,611),new(953,553),new(925,485),new(901,416)];
    private static bool IsWeapon(Vector2 p)=>Inside(p,Weapon)||(p.Y>=565&&p.DistanceTo(new(498,601))<49);
    private static bool IsCape(Vector2 p)=>p.Y<530&&p.X<590&&!Near(p,Sleeve)&&!Near(p,Forearm)&&!IsWeapon(p);
    private static int Layer(Vector2 p)=>IsWeapon(p)?4:Near(p,Sleeve)||Near(p,Forearm)?3:Near(p,FreeSleeve)?2
        :IsCape(p)?0:p.Y>620&&(p.X<460||p.X>1020)?5:1;
    private static (string,float)[] Mix(string a,string b,float t)=>[(a,1-t),(b,t)];
    private static (string,float)[] Weights(Vector2 p)
    {
        if(Near(p,Forearm)||Near(p,Sleeve)) return p.Y<365?Mix("Torso","SwordArm",Smooth(300,365,p.Y))
            :p.Y<500?Mix("SwordArm","SwordForearm",Smooth(405,475,p.Y)):Mix("SwordForearm","Hand",Smooth(535,585,p.Y));
        if(Near(p,FreeSleeve))
        {
            var arm=p.Y<365?Mix("Torso","FreeArm",Smooth(295,365,p.Y))
                :p.Y<530?Mix("FreeArm","FreeForearm",Smooth(425,490,p.Y)):Mix("FreeForearm","FreeHand",Smooth(560,610,p.Y));
            // The coat is immediately behind this forearm in the mother image.
            // A feathered inner boundary keeps its white panel attached to the waist.
            var edge=925+(p.Y-455)*.25f;
            var strength=p.Y<455?1:Smooth(edge-16,edge+16,p.X);
            return arm.Select(w=>(w.Item1,w.Item2*strength)).Append(("Coat",1-strength)).ToArray();
        }
        if(IsCape(p)) return Mix("Torso","Cape",1-Smooth(470,650,p.X));
        if(p.Y<230&&p.X>680) return Mix("Torso","Head",1-Smooth(170,220,p.Y));
        if(p.Y<600) return Mix("Torso","Pelvis",Smooth(485,600,p.Y));
        var left=p.X<770; var foot=left?1175f:1100f;
        var leg=Smooth(630,900,p.Y)*Smooth(left?395:1140,left?510:1000,p.X);
        var knee=Smooth(860,965,p.Y); var ankle=Smooth(foot-95,foot-15,p.Y);
        return [("Coat",1-leg),(left?"LeftThigh":"RightThigh",leg*(1-knee)*(1-ankle)),
            (left?"LeftShin":"RightShin",leg*knee*(1-ankle)),(left?"LeftFoot":"RightFoot",leg*ankle)];
    }
    internal void ResetBones()
    {
        foreach(var (name,bone) in Bones)
        {
            bone.Rotation=0;
            bone.Scale=Vector2.One;
            bone.Position=Bind[name]-(bone.GetParent()==Canvas?Vector2.Zero:Bind[bone.GetParent().Name.ToString()]);
        }
    }
    internal Vector2 BonePosition(string name)=>Canvas.ToLocal(Bones[name].GlobalPosition);
    // Solve in torso space so the small side-on body turn does not stretch limb lengths.
    internal void AimArm(bool sword,Vector2 target,float angle,float weight)
    {
        if(weight<=0)return;
        var upper=Bones[sword?"SwordArm":"FreeArm"];
        var lower=Bones[sword?"SwordForearm":"FreeForearm"];
        var hand=Bones[sword?"Hand":"FreeHand"];
        var parent=upper.GetParent<Node2D>();
        var to=parent.ToLocal(Canvas.ToGlobal(target))-upper.Position;
        var a=lower.Position; var b=hand.Position;
        var l1=a.Length(); var l2=b.Length();
        var distance=Math.Clamp(to.Length(),Mathf.Abs(l1-l2)+.01f,l1+l2-.01f);
        var bend=sword?-1:1;
        var first=to.Angle()-bend*Mathf.Acos(Math.Clamp((l1*l1+distance*distance-l2*l2)/(2*l1*distance),-1,1));
        upper.Rotation=Mathf.LerpAngle(upper.Rotation,first-a.Angle(),weight);
        var joint=parent.ToLocal(lower.GlobalPosition);
        var second=(parent.ToLocal(Canvas.ToGlobal(target))-joint).Angle();
        lower.Rotation=Mathf.LerpAngle(lower.Rotation,second-b.Angle()-upper.Rotation,weight);
        // Keep the authored, unwrapped wrist angle: shortest-arc TRS interpolation
        // flips the supporting palm when a raised blade crosses -PI during grip-in.
        // Rigid canvas-space rotation also cancels torso foreshortening for the sword.
        var current=Canvas.GlobalTransform.AffineInverse()*hand.GlobalTransform;
        hand.GlobalTransform=Canvas.GlobalTransform*new Transform2D(angle*weight,current.Origin);
    }
    internal void PlantLeg(string side)
    {
        var hip=Bones[side+"Thigh"]; var knee=Bones[side+"Shin"]; var ankle=Bones[side+"Foot"];
        var target=Bind[side+"Foot"]; var start=BonePosition(side+"Thigh");
        var upper=Bind[side+"Shin"]-Bind[side+"Thigh"]; var lower=Bind[side+"Foot"]-Bind[side+"Shin"];
        var l1=upper.Length(); var l2=lower.Length(); var to=target-start;
        var distance=Math.Clamp(to.Length(),Mathf.Abs(l1-l2)+.001f,l1+l2-.001f);
        var first=to.Angle()-Math.Sign(upper.Cross(lower))*Mathf.Acos(Math.Clamp((l1*l1+distance*distance-l2*l2)/(2*l1*distance),-1,1));
        var parentRotation=(Canvas.GlobalTransform.AffineInverse()*hip.GetParent<Node2D>().GlobalTransform).Rotation;
        hip.Rotation=first-upper.Angle()-parentRotation;
        var joint=BonePosition(side+"Shin"); var second=(target-joint).Angle();
        var hipRotation=(Canvas.GlobalTransform.AffineInverse()*hip.GlobalTransform).Rotation;
        knee.Rotation=second-lower.Angle()-hipRotation;
        ankle.Rotation=-(Canvas.GlobalTransform.AffineInverse()*knee.GlobalTransform).Rotation;
    }
    internal void Update()
    {
        var inverse=Canvas.GlobalTransform.AffineInverse();
        var matrices=Bones.ToDictionary(e=>e.Key,e=>inverse*e.Value.GlobalTransform*new Transform2D(0,-Bind[e.Key]));
        var vertices=new Vector2[Uv.Length];
        for(var i=0;i<vertices.Length;i++) foreach(var (name,weight) in _weights[i]) vertices[i]+=(matrices[name]*Uv[i])*weight;
        Vertices=vertices; foreach(var layer in Layers) layer.Polygon=vertices;
    }
}

