// Build an offline review page from actual game-exported mesh frames.
import fs from 'node:fs';
import path from 'node:path';
const root=path.resolve(import.meta.dirname,'..');
const input=path.resolve(process.argv[2]??path.join(root,'.local/motion039/motion.json'));
const output=path.resolve(process.argv[3]??path.join(root,'.local/motion039/preview.html'));
const data=JSON.parse(fs.readFileSync(input,'utf8'));
if(!data.weapon)throw Error('Export the current native rigid-weapon frames before building the review page.');
const image=fs.readFileSync(path.join(root,'src/Sts2Mod/Sts2Mod/character/body.png')).toString('base64');
const html=String.raw`<!doctype html><html lang="zh-CN"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>旅行者 · 同母稿动作评审</title>
<style>
*{box-sizing:border-box}body{margin:0;background:#101b21;color:#e3eee9;font:15px/1.6 system-ui,"Microsoft YaHei",sans-serif}
main{max-width:1600px;margin:auto;padding:28px}h1{font-size:26px;margin:0}p{margin:5px 0 20px;color:#a8b9ba}
.tag{color:#8ed6bf;font-size:12px;letter-spacing:2px}nav{display:flex;gap:10px;flex-wrap:wrap;align-items:center;margin:20px 0}
button,select{background:#223a44;color:#e2f0ea;border:1px solid #42636b;border-radius:7px;padding:9px 15px;cursor:pointer}
button:hover{background:#34535a}label{color:#b9c9c8}.cards{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:14px}
.card{border:1px solid #354c53;border-radius:12px;overflow:hidden;background:#19292e}.bar{padding:12px 16px;border-bottom:1px solid #354c53}
.stage{position:relative;aspect-ratio:480/440;background:radial-gradient(ellipse at 45% 85%,#344035 0,transparent 56%),linear-gradient(#16272d,#263630)}
canvas{width:100%;height:100%;display:block}.overlay{position:absolute;inset:0;pointer-events:none}.timeline{display:flex;gap:12px;align-items:center;margin:20px 0}
input[type=range]{flex:1;accent-color:#89d2b7}.keys{display:grid;grid-template-columns:repeat(5,minmax(0,1fr));gap:10px}.keys .stage{aspect-ratio:480/440}.caption{padding:6px 12px;font-size:13px;color:#b9c9c8}
.note{border-left:3px solid #72bca5;padding:10px 16px;background:#192b31;margin-top:22px;color:#b8ccca}
@media(max-width:900px){.cards{grid-template-columns:1fr}.keys{grid-template-columns:repeat(2,1fr)}main{padding:16px}}
</style>
<main><div class="tag">TRAVELER / MOTION STUDY 0.3.9</div><h1>同一母稿，连续动作评审</h1>
<p>原生节点计算的网格帧 · 原速 / 四分之一慢放 / 逐帧 · 离线预览，无游戏窗口</p>
<nav><select id="clip" aria-label="选择动作"><option value="Attack">侧身前突刺</option><option value="Defend">双手举剑防御</option><option value="Hit">侧身后仰受击</option><option value="Continuous">突刺与防御连续切换</option><option value="Interrupted">受击中断与恢复</option></select>
<button id="play">暂停</button><button id="restart">从头播放</button><label><input id="bones" type="checkbox">显示关节与脚底约束</label>
<button id="previous">上一帧</button><button id="next">下一帧</button></nav>
<div class="cards"><div class="card"><div class="bar">母稿 / 原始绑定姿势</div><div class="stage"><canvas id="mother" width="480" height="440"></canvas></div></div>
<div class="card"><div class="bar">正常速度 · 1×</div><div class="stage"><canvas id="normal" width="480" height="440"></canvas><canvas class="overlay" id="normalBones" width="480" height="440"></canvas></div></div>
<div class="card"><div class="bar">慢放 · 0.25×</div><div class="stage"><canvas id="slow" width="480" height="440"></canvas><canvas class="overlay" id="slowBones" width="480" height="440"></canvas></div></div></div>
<div class="timeline"><span id="frameLabel">帧</span><input id="timeline" type="range" min="0" value="0" aria-label="逐帧位置"></div>
<div class="bar">关键姿势对照</div><div class="keys" id="keys"></div>
<div class="note">检查肩肘连接、握柄稳定、剑路、脚底滑动及动作切换。预览采用游戏导出的实际网格与原母稿纹理，不包含战场光照、原生特效或 UI；它不能代替实机画面验收。</div>
</main><script>
const DATA=__DATA__;
const TEXTURE='data:image/png;base64,__IMAGE__';
const clips=Object.fromEntries(DATA.clips.map(c=>[c.name,c]));
const image=new Image();image.src=TEXTURE;
const order=DATA.layers.toSorted((a,b)=>a.z-b.z);
const indices=new Uint16Array(order.flatMap(l=>l.triangles.flat()));
const uv=new Float32Array(DATA.uv.map((v,i)=>v/(i%2?1271:1237)));
const camera={x:200,y:402,scale:.88};
function renderer(canvas){
 const gl=canvas.getContext('webgl',{alpha:true,antialias:true,premultipliedAlpha:true,preserveDrawingBuffer:true});
 if(!gl)throw Error('此浏览器未启用 WebGL');
 function shader(type,source){let s=gl.createShader(type);gl.shaderSource(s,source);gl.compileShader(s);if(!gl.getShaderParameter(s,gl.COMPILE_STATUS))throw Error(gl.getShaderInfoLog(s));return s}
 const program=gl.createProgram();gl.attachShader(program,shader(gl.VERTEX_SHADER,'attribute vec2 p;attribute vec2 uv;varying vec2 t;void main(){t=uv;vec2 q=(p*.88+vec2(200.,402.))/vec2(480.,440.);gl_Position=vec4(q.x*2.-1.,1.-q.y*2.,0.,1.);}'));
 const point=i=>'vec2('+DATA.weapon[i*2].toFixed(3)+','+DATA.weapon[i*2+1].toFixed(3)+')';
 const edges=Array.from({length:20},(_,i)=>'if(edge('+point(i)+','+point((i+1)%20)+',p))inside=!inside;').join('');
 const fragment='precision highp float;varying vec2 t;uniform sampler2D tex;uniform float mask;bool edge(vec2 a,vec2 b,vec2 p){return ((a.y>p.y)!=(b.y>p.y))&&(p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x);}void main(){vec2 p=t*vec2(1237.,1271.);bool inside=false;'+edges+'if(mask>.5&&inside)discard;gl_FragColor=texture2D(tex,t);}';
 gl.attachShader(program,shader(gl.FRAGMENT_SHADER,fragment));gl.linkProgram(program);if(!gl.getProgramParameter(program,gl.LINK_STATUS))throw Error(gl.getProgramInfoLog(program));gl.useProgram(program);
 const mask=gl.getUniformLocation(program,'mask');
 const position=gl.createBuffer();gl.bindBuffer(gl.ARRAY_BUFFER,position);let at=gl.getAttribLocation(program,'p');gl.enableVertexAttribArray(at);gl.vertexAttribPointer(at,2,gl.FLOAT,false,0,0);
 const texcoord=gl.createBuffer();gl.bindBuffer(gl.ARRAY_BUFFER,texcoord);gl.bufferData(gl.ARRAY_BUFFER,uv,gl.STATIC_DRAW);at=gl.getAttribLocation(program,'uv');gl.enableVertexAttribArray(at);gl.vertexAttribPointer(at,2,gl.FLOAT,false,0,0);
 gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER,gl.createBuffer());gl.bufferData(gl.ELEMENT_ARRAY_BUFFER,indices,gl.STATIC_DRAW);
 gl.bindTexture(gl.TEXTURE_2D,gl.createTexture());gl.pixelStorei(gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL,true);gl.texImage2D(gl.TEXTURE_2D,0,gl.RGBA,gl.RGBA,gl.UNSIGNED_BYTE,image);
 for(const flag of [gl.TEXTURE_WRAP_S,gl.TEXTURE_WRAP_T])gl.texParameteri(gl.TEXTURE_2D,flag,gl.CLAMP_TO_EDGE);
 gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_MIN_FILTER,gl.LINEAR);gl.texParameteri(gl.TEXTURE_2D,gl.TEXTURE_MAG_FILTER,gl.LINEAR);gl.enable(gl.BLEND);gl.blendFunc(gl.ONE,gl.ONE_MINUS_SRC_ALPHA);gl.clearColor(0,0,0,0);
 return vertices=>{gl.clear(gl.COLOR_BUFFER_BIT);gl.bindBuffer(gl.ARRAY_BUFFER,position);gl.bufferData(gl.ARRAY_BUFFER,new Float32Array(vertices),gl.DYNAMIC_DRAW);let offset=0;for(const layer of order){const count=layer.triangles.length*3;gl.uniform1f(mask,layer.mask?1:0);gl.drawElements(gl.TRIANGLES,count,gl.UNSIGNED_SHORT,offset*2);offset+=count}}
}
let selected='Attack',playing=true,time=0,last=0,manual=0,pausedSlow=0,slowOffset=0,lastNormal=0,lastSlow=0,drawNormal,drawSlow;
const keyViews=[];
const select=document.querySelector('#clip'),slider=document.querySelector('#timeline'),label=document.querySelector('#frameLabel');
function boneOverlay(id,clip,index){
 const ctx=document.getElementById(id).getContext('2d');ctx.clearRect(0,0,480,440);
 if(!document.querySelector('#bones').checked)return;const frame=clip.boneFrames[index];ctx.lineWidth=1.2;ctx.strokeStyle='#f6d781';ctx.fillStyle='#c9fff1';
 DATA.bones.forEach((name,i)=>{const x=frame[i*2]*camera.scale+camera.x,y=frame[i*2+1]*camera.scale+camera.y,p=DATA.parents[i];
 if(p>=0){ctx.beginPath();ctx.moveTo(x,y);ctx.lineTo(frame[p*2]*camera.scale+camera.x,frame[p*2+1]*camera.scale+camera.y);ctx.stroke()}
 ctx.beginPath();ctx.arc(x,y,name.endsWith('Foot')?4:2.2,0,Math.PI*2);ctx.fill()});
}
function keys(){
 const clip=clips[selected],box=document.querySelector('#keys');
 const frames=selected==='Attack'?[0,5,9,10,19]:selected==='Defend'?[0,5,9,12,20]:selected==='Hit'?[0,3,5,8,12]:selected==='Continuous'?[9,14,24,36,80]:[0,6,10,18,30];
 const names=selected==='Attack'?['原始姿态','侧身转剑','直线突刺','保持','收剑回位']:selected==='Defend'?['原始姿态','副手接柄','双手防守','保持','回位']:selected==='Hit'?['原始姿态','侧身卸力','轻微后仰','恢复','回原姿']:selected==='Continuous'?['前突刺','转入防御','再次突刺','再次防御','回位']:['起手','受击中断','后仰','接入防御','恢复'];
 if(!keyViews.length)frames.forEach(()=>{const card=document.createElement('div');card.className='card';const stage=document.createElement('div');stage.className='stage';const canvas=document.createElement('canvas');canvas.width=480;canvas.height=440;stage.append(canvas);const caption=document.createElement('div');caption.className='caption';card.append(stage,caption);box.append(card);keyViews.push({caption,draw:renderer(canvas)})});
 frames.forEach((frame,i)=>{keyViews[i].caption.textContent=names[i]+' · '+frame+'帧';keyViews[i].draw(clip.frames[Math.min(frame,clip.frames.length-1)])})
}
function tick(now){
 const dt=last?Math.min((now-last)/1000,.1):0;last=now;if(playing)time+=dt;
 const clip=clips[selected],duration=clip.frames.length/clip.fps+.3;
 const normalTime=playing?Math.min((time%duration)*clip.fps,clip.frames.length-1):manual;
 const slowTime=playing?Math.min((((time*.25+slowOffset)%duration+duration)%duration)*clip.fps,clip.frames.length-1):pausedSlow;
 lastNormal=normalTime;lastSlow=slowTime;
 const normal=Math.floor(normalTime),slow=Math.floor(slowTime);
 const between=f=>{const a=Math.floor(f),b=Math.min(a+1,clip.frames.length-1),t=f-a;return clip.frames[a].map((value,i)=>value+(clip.frames[b][i]-value)*t)};
 drawNormal(between(normalTime));drawSlow(between(slowTime));boneOverlay('normalBones',clip,normal);boneOverlay('slowBones',clip,slow);
 slider.value=normal;label.textContent='帧 '+normal+' / '+(clip.frames.length-1);requestAnimationFrame(tick)
}
select.onchange=()=>{selected=select.value;time=0;manual=0;pausedSlow=0;slowOffset=0;slider.max=clips[selected].frames.length-1;keys()};
document.querySelector('#play').onclick=()=>{if(playing){manual=lastNormal;pausedSlow=lastSlow}else{time=manual/clips[selected].fps;slowOffset=pausedSlow/clips[selected].fps-time*.25}playing=!playing;document.querySelector('#play').textContent=playing?'暂停':'播放'};
document.querySelector('#restart').onclick=()=>{time=0;manual=0;pausedSlow=0;slowOffset=0;playing=true;document.querySelector('#play').textContent='暂停'};
slider.oninput=()=>{manual=+slider.value;pausedSlow=manual;playing=false;document.querySelector('#play').textContent='播放'};
for(const [id,delta]of [['previous',-1],['next',1]])document.getElementById(id).onclick=()=>{manual=Math.max(0,Math.min(clips[selected].frames.length-1,Math.floor(playing?lastNormal:manual)+delta));pausedSlow=manual;slider.value=manual;playing=false;document.querySelector('#play').textContent='播放'};
image.onload=()=>{renderer(document.querySelector('#mother'))(clips.Bind.frames[0]);drawNormal=renderer(document.querySelector('#normal'));drawSlow=renderer(document.querySelector('#slow'));slider.max=clips[selected].frames.length-1;keys();window.previewReady=true;requestAnimationFrame(tick)};
window.addEventListener('error',e=>{document.querySelector('.note').textContent='预览错误：'+e.message});
</script></html>`;
fs.writeFileSync(output,html.replace('__DATA__',JSON.stringify(data)).replace('__IMAGE__',image));
console.log(output);

