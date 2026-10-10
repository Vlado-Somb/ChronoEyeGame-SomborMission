'use strict';
const E=id=>document.getElementById(id),C=x=>JSON.parse(JSON.stringify(x));
const H=x=>String(x??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
const M={map:['Геолокације','waypoints.json'],dialogue:['Дијалози','dialogues/act-0.json'],mission:['Мисије','missions/act-0.json'],flow:['Ток сцене','production/act-0/scene-flow.json'],source:['Извори','sources.json'],json:['JSON','manifest.json']};
let mode='map',file='waypoints.json',data=null,original=null,revision='',files=[],selected=0,history=[],dirty=false,map=null,layer=null;
async function req(url,options){const r=await fetch(url,options),x=await r.json();if(!r.ok)throw Error(x.error||JSON.stringify(x));return x;}
function post(url,x){return req(url,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(x)});}
function say(t){E('message').textContent=t;E('message').classList.add('visible');}
function changed(){dirty=true;E('dirty').textContent='● Несачувано';E('save').disabled=false;}
function saved(){dirty=false;E('dirty').textContent='✓ Сачувано';E('save').disabled=true;}
function snap(){if(data){history.push(C(data));if(history.length>25)history.shift();}}
function input(label,path,value,extra=''){return '<label>'+H(label)+'</label><input data-path="'+H(path)+'" value="'+H(value??'')+'" '+extra+'>';}
function area(label,path,value,rows=3){return '<label>'+H(label)+'</label><textarea data-path="'+H(path)+'" rows="'+rows+'">'+H(value??'')+'</textarea>';}
function title(t){return '<div class="section-label">'+H(t)+'</div>';}
function pick(name,desc,i,tag){return '<button class="item '+(i===selected?'active':'')+'" data-pick="'+i+'"><span><b>'+H(name)+'</b><small>'+H(desc)+'</small></span><span class="tag">'+H(tag||'')+'</span></button>';}
async function load(path){
 if(dirty&&!confirm('Одбацити несачуване измене?'))return false;
 const x=await req('/api/file?path='+encodeURIComponent(path));file=path;data=x.data;original=C(data);revision=x.revision;selected=0;history=[];saved();render();say('Учитавање: '+file);return true;
}
async function switchMode(next){
 const previous=mode;mode=next;
 try{if(!(await load(next==='json'?file:M[next][1]))){mode=previous;render();}}catch(e){mode=previous;render();throw e;}
}
function render(){
 if(!data)return;
 E('pageTitle').textContent=M[mode][0];E('breadcrumb').textContent='BUDAPEST V2 / '+file.toUpperCase();
 E('pageDesc').textContent='Локално уређивање · Без јавног deploy-а · Теренска провера је обавезна.';
 document.querySelectorAll('[data-mode]').forEach(b=>b.classList.toggle('active',b.dataset.mode===mode));
 E('mapPane').classList.toggle('hidden',mode!=='map');E('contentPane').classList.toggle('hidden',mode==='map');
 const options=files.filter(p=>mode==='map'?p==='waypoints.json':mode==='dialogue'?p.startsWith('dialogues/'):mode==='mission'?p.startsWith('missions/'):mode==='flow'?p.endsWith('/scene-flow.json'):mode==='source'?p==='sources.json':true);
 E('fileSelect').innerHTML=options.map(p=>'<option>'+H(p)+'</option>').join('');E('fileSelect').value=file;
 if(mode==='map')geo();else content();
 preview();
}
function geo(){
 const ws=data.waypoints||[],w=ws[selected];
 E('waypointList').innerHTML=ws.map((x,i)=>pick(x.name,x.id,i,x.fieldVerified?'VERIFIED':'PENDING')).join('');
 if(!w)return;E('mapStatus').textContent=w.fieldVerified?'VERIFIED':'SURVEY PENDING';
 const a=w.safeStandingPoint||{},b=w.arAnchor||{};
 E('mapForm').innerHTML=input('ID','id',w.id,'readonly')+input('Назив','name',w.name)+title('ГЛАВНИ PIN · WGS84')+
 '<div class="row">'+input('Latitude','latitude',w.latitude,'type="number" step="any"')+input('Longitude','longitude',w.longitude,'type="number" step="any"')+'</div>'+
 title('БЕЗБЕДНО СТАЈАЛИШТЕ')+'<div class="row">'+input('Latitude','safeLatitude',a.latitude,'type="number" step="any"')+input('Longitude','safeLongitude',a.longitude,'type="number" step="any"')+'</div>'+
 title('AR GEOSPATIAL СИДРО · НАЦРТ')+'<div class="row">'+input('Latitude','arLatitude',b.latitude,'type="number" step="any"')+input('Longitude','arLongitude',b.longitude,'type="number" step="any"')+'</div>'+
 title('GPS ЗОНЕ')+area('zones[] JSON','zones',JSON.stringify(w.zones||[],null,2),5)+
 '<p class="help">Кликни на мапу или превуци pin. Нова координата увек враћа fieldVerified и navigationEnabled на false.</p>';
 draw();
}
function draw(){
 if(!window.L)return;
 if(!map){map=L.map('map').setView([47.4979,19.0402],12);L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',{maxZoom:19,attribution:'© OpenStreetMap contributors'}).addTo(map);layer=L.layerGroup().addTo(map);
 map.on('click',e=>{if(mode!=='map')return;snap();place(data.waypoints[selected],e.latlng);changed();render();});}
 layer.clearLayers();
 (data.waypoints||[]).forEach((w,i)=>{if(typeof w.latitude!=='number'||typeof w.longitude!=='number')return;const marker=L.marker([w.latitude,w.longitude],{draggable:true}).addTo(layer);
 marker.bindTooltip(w.id+' · '+(w.fieldVerified?'VERIFIED':'PENDING'));
 marker.on('click',()=>{selected=i;render();});marker.on('dragstart',snap);marker.on('dragend',e=>{selected=i;place(w,e.target.getLatLng());changed();render();});});
 setTimeout(()=>map.invalidateSize(),50);
}
function place(w,p){w.latitude=Number(p.lat.toFixed(7));w.longitude=Number(p.lng.toFixed(7));w.fieldVerified=false;w.navigationEnabled=false;w.coordinateStatus='survey_required_v2';}
function content(){
 let items=[],html='',name='';
 if(mode==='dialogue'){
  items=data.scenes||[];name='Сцене';const x=items[selected];if(x){html=input('ID','id',x.id,'readonly')+input('Наслов','title',x.title)+input('Локација','location',x.location)+title('РЕПЛИКЕ');
   html+=(x.lines||[]).map((l,i)=>'<div class="editor-card"><b>'+H(l.id)+'</b>'+input('Говорник','lines.'+i+'.speakerId',l.speakerId)+area('Текст','lines.'+i+'.text',l.text,2)+area('Титл','lines.'+i+'.subtitle',l.subtitle,2)+input('Voice ID','lines.'+i+'.voiceAssetId',l.voiceAssetId)+'</div>').join('');
   html+=title('ЗВУЧНИ СИГНАЛИ')+(x.mediaCues||[]).map((c,i)=>area(c.id,'mediaCues.'+i+'.direction',c.direction,2)).join('');}}
 else if(mode==='mission'){
  items=data.tasks||[];name='Задаци';const x=items[selected];if(x){html=input('ID','id',x.id,'readonly')+input('Наслов','title',x.title)+area('Упутство','instruction',x.instruction);
   if(x.question)html+=title('КВИЗ')+area('Питање','question.text',x.question.text)+area('Одговори · по реду','question.choices',x.question.choices.join('\n'),5)+input('Тачан индекс','question.correctIndex',x.question.correctIndex,'type="number"')+area('Повратна информација','question.wrongAnswerFeedback',x.question.wrongAnswerFeedback);
   html+=title('ИСТОРИЈСКА КАРТИЦА')+area('Пасуси','historyParagraphs',(data.narrative?.historyParagraphs||[]).join('\n\n'),5);}}
 else if(mode==='flow'){
  items=data.nodes||[];name='Чворови';const x=items[selected];if(x){html=input('ID','id',x.id,'readonly')+input('Тип','kind',x.kind,'readonly')+input('Следећи','next',x.next);
   if(x.kind==='dialogue')html+=input('ID сцене','dialogueId',x.dialogueId)+area('Резиме прескочене сцене','skipSummary',x.skipSummary,5);
   if(x.kind==='choice')html+=area('Избори JSON','choices',JSON.stringify(x.choices||[],null,2),6);}}
 else if(mode==='source'){
  items=data.sources||[];name='Извори';const x=items[selected];if(x)html=input('ID','id',x.id,'readonly')+input('Наслов','title',x.title)+input('URL','url',x.url)+area('Опсег доказа','scope',x.scope,5)+input('Статус','reviewStatus',x.reviewStatus)+input('Датум провере','checkedAt',x.checkedAt);
 }else{items=[{id:file,title:file}];name='JSON';html=area('Комплетан JSON','raw',JSON.stringify(data,null,2),30);}
 E('listTitle').textContent=name;E('itemCount').textContent=items.length+' ставки';
 E('itemList').innerHTML=items.map((x,i)=>pick(x.title||x.id,x.id||x.kind,i,x.type||x.kind||x.reviewStatus)).join('');
 E('formTitle').textContent=items[selected]?.title||items[selected]?.id||file;
 E('formStatus').textContent='DRAFT';E('contentForm').innerHTML=html;
}
function preview(){
 let html='';
 if(mode==='map'){const w=data.waypoints?.[selected];if(w)html='<b>'+H(w.name)+'</b><p>WGS84: '+H(w.latitude)+' / '+H(w.longitude)+'</p><p>Safe: '+H(JSON.stringify(w.safeStandingPoint))+' · AR: '+H(JSON.stringify(w.arAnchor))+'</p>';}
 else if(mode==='dialogue'){const x=data.scenes?.[selected];if(x){const dict={};for(const [t,k] of [['line','lines'],['cue','mediaCues'],['stage','stageDirections'],['beat','gameplayBeats']])for(const a of x[k]||[])dict[t+':'+a.id]=a;
 html='<b>'+H(x.title)+'</b>';for(const step of x.sequence||[]){const a=dict[step.type+':'+step.id];if(a)html+=step.type==='line'?'<div class="preview-line"><b>'+H(a.speakerId)+'</b> '+H(a.text)+'</div>':'<p class="note">'+H(a.text||a.direction||a.kind)+'</p>';}}}
 else if(mode==='mission'){const t=data.tasks?.[selected];if(t){html='<b>'+H(t.title)+'</b><p>'+H(t.instruction)+'</p>';if(t.question)html+='<p>'+H(t.question.text)+'</p><ol>'+t.question.choices.map((c,i)=>'<li>'+H(c)+(i===t.question.correctIndex?' ✓':'')+'</li>').join('')+'</ol>';}}
 else if(mode==='flow')html='<p>'+H(data.nodes?.[selected]?.skipSummary||'Нема резимеа')+'</p>';
 else if(mode==='source')html='<p>'+H(data.sources?.[selected]?.scope)+'</p>';
 else html='<p>'+H(file)+'</p>';
 E('previewBody').innerHTML=html;
}
function update(key,value){
 if(mode==='json'){data=JSON.parse(value);return;}
 const obj=mode==='map'?data.waypoints[selected]:mode==='dialogue'?data.scenes[selected]:mode==='mission'?data.tasks[selected]:mode==='flow'?data.nodes[selected]:data.sources[selected];
 if(['id','kind'].includes(key))return;
 if(mode==='map'){
  if(['latitude','longitude'].includes(key)){obj[key]=value.trim()===''?null:Number(value);obj.fieldVerified=false;obj.navigationEnabled=false;obj.coordinateStatus='survey_required_v2';}
  else if(key.startsWith('safe')||key.startsWith('ar')){const p=key.startsWith('safe')?'safeStandingPoint':'arAnchor',axis=key.endsWith('Latitude')?'latitude':'longitude',v=obj[p]||{latitude:null,longitude:null,status:'draft_unverified'};v[axis]=value.trim()===''?null:Number(value);obj[p]=v.latitude===null&&v.longitude===null?null:v;obj.fieldVerified=false;obj.navigationEnabled=false;}
  else if(key==='zones')obj.zones=JSON.parse(value);else obj[key]=value;return;
 }
 if(key==='historyParagraphs'){data.narrative.historyParagraphs=value.split(/\n\s*\n/).filter(Boolean);return;}
 const parts=key.split('.');let target=obj;for(let i=0;i<parts.length-1;i++)target=target[parts[i]];
 const last=parts[parts.length-1];
 target[last]=key==='question.choices'?value.split('\n').filter(Boolean):key==='question.correctIndex'?Number(value):key==='choices'?JSON.parse(value):key.endsWith('voiceAssetId')?(value||null):value;
}
function diff(a,b,p='',out=[]){
 if(out.length>100||JSON.stringify(a)===JSON.stringify(b))return out;
 if(a&&b&&typeof a==='object'&&typeof b==='object'&&!Array.isArray(a)&&!Array.isArray(b))for(const k of new Set([...Object.keys(a),...Object.keys(b)]))diff(a[k],b[k],p+'.'+k,out);
 else if(Array.isArray(a)&&Array.isArray(b))for(let i=0;i<Math.max(a.length,b.length);i++)diff(a[i],b[i],p+'['+i+']',out);
 else out.push(p+': '+JSON.stringify(a)?.slice(0,100)+' → '+JSON.stringify(b)?.slice(0,100));
 return out;
}
function modal(title,body){E('modalTitle').textContent=title;E('modalContent').textContent=body;E('modal').showModal();}
E('closeModal').onclick=()=>E('modal').close();
E('modules').onclick=async e=>{const b=e.target.closest('[data-mode]');if(b)try{await switchMode(b.dataset.mode);}catch(x){say(x.message);}};
E('fileSelect').onchange=async e=>{try{if(!(await load(e.target.value)))render();}catch(x){say(x.message);}};
document.addEventListener('click',e=>{const b=e.target.closest('[data-pick]');if(b){selected=Number(b.dataset.pick);render();}});
document.addEventListener('focusin',e=>{if(e.target.matches('[data-path]'))snap();});
document.addEventListener('change',e=>{if(!e.target.matches('[data-path]'))return;try{update(e.target.dataset.path,e.target.value);changed();preview();if(mode==='map'&&['latitude','longitude'].includes(e.target.dataset.path))draw();}catch(x){say('Грешка уноса: '+x.message);}});
E('save').onclick=async()=>{try{const v=await post('/api/validate',{path:file,data});if(!v.ok)return modal('Грешке',JSON.stringify(v,null,2));const x=await post('/api/save',{path:file,data,revision});revision=x.revision;original=C(data);history=[];saved();say('Сачувано · backup '+x.backup);}catch(x){say('Чување није успело: '+x.message);}};
E('undo').onclick=()=>{if(!history.length)return say('Нема измена');data=history.pop();changed();render();};
E('diff').onclick=()=>modal('Промене',diff(original,data).join('\n')||'Нема промена');
E('qa').onclick=async()=>{try{const x=await req('/api/qa');modal(x.ok?'QA PASS':'QA FAIL',JSON.stringify(x,null,2));}catch(x){say(x.message);}};
E('export').onclick=()=>{const url=URL.createObjectURL(new Blob([JSON.stringify(data,null,2)+'\n'],{type:'application/json'})),a=document.createElement('a');a.href=url;a.download=file.replaceAll('/','__');a.click();setTimeout(()=>URL.revokeObjectURL(url),1000);};
(async()=>{try{files=(await req('/api/list')).files;await load('waypoints.json');}catch(x){say(x.message);}})();
