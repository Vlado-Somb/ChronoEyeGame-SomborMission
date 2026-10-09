/* CHRONO EYE Maps adapter v0.1 — no scoring, no AR, no persistent GPS history. */
(function () {
  "use strict";
  const C = window.CHRONO_MAPS_CONFIG || {};
  const G = window.ChronoMapsGeo;
  const $ = id => document.getElementById(id);
  const S = {data:null,points:[],byId:new Map(),missions:[],selected:null,map:null,info:null,markers:new Map(),
    shapes:[],zoneCircles:[],playerMarker:null,accuracyCircle:null,player:null,mode:"preview",simTimer:null,
    simIndex:0,browserWatch:null,locationSource:null};
  const el = (tag, text, cls) => {const e=document.createElement(tag);if(text!==undefined)e.textContent=String(text);if(cls)e.className=cls;return e;};
  const emit = (name, detail) => window.dispatchEvent(new CustomEvent("chronoeye:map:"+name,{detail}));
  async function json(path) {const r=await fetch(path,{cache:"no-store"});if(!r.ok)throw new Error(path+": HTTP "+r.status);return r.json();}
  async function loadData() {
    let source="candidate snapshot",data;
    try {
      const root=(C.gameDataRoot||"../../game-data/sombor/v1").replace(/\/$/,"");
      const [manifest,wps]=await Promise.all([json(root+"/manifest.json"),json(root+"/waypoints.json")]);
      if(!Array.isArray(manifest.missions)||!Array.isArray(wps.waypoints))throw new Error("Invalid canonical schema");
      const missions=await Promise.all(manifest.missions.map(async m=>{
        const d=await json(root+"/"+m.path);
        return {id:d.id,title:d.title,primaryWaypointId:m.primaryWaypointId,reviewStatus:d.reviewStatus,
          taskCount:d.tasks?.length||0,setting:d.narrative?.setting||"",summary:d.tasks?.find(t=>t.type==="gps")?.instruction||"",
          gpsRequiredZone:d.tasks?.find(t=>t.type==="gps")?.gps?.requiredZone||3};
      }));
      data={gameId:manifest.gameId,name:manifest.name,waypoints:wps.waypoints,missions};
      source="canonical game-data ("+manifest.schemaVersion+")";
    } catch (err) {
      data=await json(C.snapshotUrl||"./data/sombor-demo.json");
      source="candidate snapshot ("+(data.source?.ref||"local")+")";
    }
    const index=G.indexData(data);
    S.data=data;S.points=index.points.filter(p=>p.id!=="SO_GLOBAL");S.byId=index.byId;S.missions=index.missions;
    $("dataSource").textContent="Извор: "+source;
    $("missionCount").textContent=S.missions.length+" мисија";
    return data;
  }
  function missionPoint(m){return S.byId.get(m.primaryWaypointId);}
  function missionForPoint(p){return S.missions.find(m=>m.primaryWaypointId===p.id);}
  function setStatus(s){$("status").textContent=s;}
  function missionButton(m){
    const b=el("button",undefined,"mission"+(S.selected===m.id?" selected":""));
    b.type="button";b.setAttribute("aria-pressed",String(S.selected===m.id));
    b.append(el("strong",m.title),el("small",m.taskCount+" задатака · "+m.primaryWaypointId));
    b.addEventListener("click",()=>selectMission(m.id,"list"));
    return b;
  }
  function renderList(){
    const host=$("missionList");host.replaceChildren(...S.missions.map(missionButton));
  }
  function renderDetail(){
    const host=$("detail");host.replaceChildren();
    const m=S.missions.find(x=>x.id===S.selected);
    if(!m){host.append(el("p","Изабери мисију на мапи или у листи."));return;}
    const p=missionPoint(m);
    host.append(el("span",m.id,"chip"),el("h3",m.title),el("p",m.setting||p.name));
    if(m.summary)host.append(el("p",m.summary));
    host.append(el("p","Waypoint: "+p.id+" · "+p.lat.toFixed(6)+", "+p.lng.toFixed(6)));
    host.append(el("p","GPS зоне: "+p.zones.outerMeters+" m / "+p.zones.approachMeters+" m / "+p.zones.closeMeters+" m"));
    if(!p.fieldVerified)host.append(el("p","Координате и зоне нису теренски верификоване.","warning"));
    if(S.player)renderDistance(host,p);
  }
  function renderDistance(host,p){
    const dist=G.distanceMeters(S.player,{lat:p.lat,lng:p.lng});
    const zone=G.zoneForDistance(dist,p.zones);
    const accuracy=S.player.accuracyMeters;
    const uncertain=Number.isFinite(accuracy)&&accuracy>Math.max(p.zones.closeMeters,dist);
    host.append(el("p","Од играча: "+G.formatDistance(dist)+" · зона (само приказ): "+zone+(uncertain?" · GPS непрецизан":"")));
  }
  function selectMission(id,source="external"){
    const m=S.missions.find(x=>x.id===id);if(!m)return false;
    S.selected=id;renderList();renderDetail();renderZones();
    const p=missionPoint(m);
    if(S.map){S.map.panTo({lat:p.lat,lng:p.lng});S.map.setZoom(Math.max(S.map.getZoom()||15,16));}
    else drawPreview();
    emit("mission-selected",{gameId:S.data.gameId,missionId:m.id,waypointId:p.id,source});
    return true;
  }
  function createPopup(p){
    const wrap=el("div");wrap.style.maxWidth="240px";
    wrap.append(el("strong",p.name),el("p",p.id+" · "+p.category));
    const m=missionForPoint(p);
    if(m){const b=el("button","Изабери мисију");b.type="button";b.addEventListener("click",()=>selectMission(m.id,"marker"));wrap.append(b);}
    else wrap.append(el("p","Додатна локација, без активне мисије."));
    return wrap;
  }
  function showPopup(p,marker){
    if(!S.info)return;
    S.info.setContent(createPopup(p));S.info.open({map:S.map,anchor:marker});
  }
  function mapPoint(p){return {lat:p.lat,lng:p.lng};}
  function renderZones(){
    S.zoneCircles.forEach(c=>c.setMap(null));S.zoneCircles=[];
    if(!S.map||!$("showZones").checked||!S.selected)return;
    const m=S.missions.find(x=>x.id===S.selected),p=missionPoint(m);
    [["outerMeters","#d1a553",0.08],["approachMeters","#368da4",0.10],["closeMeters","#15846c",0.16]].forEach(([key,color,opacity])=>{
      S.zoneCircles.push(new google.maps.Circle({map:S.map,center:mapPoint(p),radius:p.zones[key],
        strokeColor:color,strokeWeight:1.5,strokeOpacity:.9,fillColor:color,fillOpacity:opacity,clickable:false}));
    });
  }
  function renderGeometry(p){
    const geometry=p.geometry;if(!geometry)return;
    const toLatLng=coord=>({lat:Number(coord[1]),lng:Number(coord[0])});
    const opts={map:S.map,strokeColor:"#d5a257",strokeWeight:2,strokeOpacity:.85};
    try{
      if(geometry.type==="Polygon"&&Array.isArray(geometry.coordinates?.[0]))
        S.shapes.push(new google.maps.Polygon({...opts,paths:geometry.coordinates.map(ring=>ring.map(toLatLng)),fillColor:"#d5a257",fillOpacity:.13}));
      if(geometry.type==="LineString"&&Array.isArray(geometry.coordinates))
        S.shapes.push(new google.maps.Polyline({...opts,path:geometry.coordinates.map(toLatLng)}));
    }catch(err){console.warn("Skipped invalid optional geometry for",p.id);}
  }
  function renderGoogle(){
    S.mode="google";$("mapMode").textContent="Google Maps API";$("mapNote").textContent="Google подлога · CHRONO EYE waypoint-и и GPS зоне";
    S.map=new google.maps.Map($("map"),{center:C.defaultCenter||{lat:45.7727,lng:19.1143},zoom:C.defaultZoom||16,
      mapTypeControl:false,streetViewControl:false,fullscreenControl:true,gestureHandling:"greedy"});
    S.info=new google.maps.InfoWindow();
    for(const p of S.points){
      const m=missionForPoint(p);
      const marker=new google.maps.Marker({position:mapPoint(p),map:S.map,title:p.name,
        label:m?{text:String(S.missions.indexOf(m)+1),color:"#fff",fontWeight:"bold"}:undefined,
        icon:{path:google.maps.SymbolPath.CIRCLE,scale:m?13:9,fillColor:m?"#126f82":"#ac8447",
          fillOpacity:1,strokeColor:"#fff",strokeWeight:2}});
      marker.addListener("click",()=>{if(m)selectMission(m.id,"marker");showPopup(p,marker);});
      S.markers.set(p.id,marker);renderGeometry(p);
    }
    fitAll();renderZones();renderPlayer();
  }
  function previewProjection(){
    const coords=S.points.map(p=>({lat:p.lat,lng:p.lng}));
    if(S.player)coords.push(S.player);
    const midLat=coords.reduce((a,p)=>a+p.lat,0)/coords.length;
    const cos=Math.cos(midLat*Math.PI/180);
    const xs=coords.map(p=>p.lng*cos),ys=coords.map(p=>p.lat);
    const minX=Math.min(...xs),maxX=Math.max(...xs),minY=Math.min(...ys),maxY=Math.max(...ys);
    const span=Math.max((maxX-minX)*1.15,(maxY-minY)*1.15,0.002);
    const cx=(minX+maxX)/2,cy=(minY+maxY)/2;
    const xy=p=>({x:400+(p.lng*cos-cx)*620/span,y:350-(p.lat-cy)*620/span});
    return {xy,metersToPx:620/(span*111195)};
  }
  function svgEl(tag,attrs,text){
    const e=document.createElementNS("http://www.w3.org/2000/svg",tag);
    Object.entries(attrs||{}).forEach(([k,v])=>e.setAttribute(k,String(v)));
    if(text!==undefined)e.textContent=text;return e;
  }
  function drawPreview(){
    if(S.map)return;
    S.mode="preview";$("mapMode").textContent="Координатни преглед";
    $("mapNote").textContent="Није Google мапа: координатни тест без улица. За Google подлогу потребан је засебан ограничен API кључ.";
    const svg=svgEl("svg",{viewBox:"0 0 800 700",class:"coordinate-preview",role:"img","aria-label":"Схематски координатни приказ Сомбора"});
    svg.append(svgEl("rect",{x:0,y:0,width:800,height:700,fill:"#e2edf0"}));
    for(let x=80;x<800;x+=80)svg.append(svgEl("line",{x1:x,y1:0,x2:x,y2:700,class:"preview-grid"}));
    for(let y=70;y<700;y+=70)svg.append(svgEl("line",{x1:0,y1:y,x2:800,y2:y,class:"preview-grid"}));
    svg.append(svgEl("text",{x:26,y:33,class:"preview-title"},"СОМБОР — WGS84 ПРЕГЛЕД"));
    svg.append(svgEl("text",{x:26,y:54,class:"preview-subtitle"},"Схематски приказ · без картографске подлоге"));
    const projection=previewProjection();
    if(S.selected&&$("showZones").checked){
      const m=S.missions.find(x=>x.id===S.selected),p=missionPoint(m),q=projection.xy(p);
      [["outerMeters","#d7a24b",.09],["approachMeters","#3d9eb2",.13],["closeMeters","#128c75",.2]].forEach(([key,color,opacity])=>{
        svg.append(svgEl("circle",{cx:q.x,cy:q.y,r:p.zones[key]*projection.metersToPx,fill:color,"fill-opacity":opacity,stroke:color,"stroke-width":1.3}));
      });
    }
    for(const p of S.points){
      const q=projection.xy(p),m=missionForPoint(p),sel=m&&m.id===S.selected;
      const group=svgEl("g",{class:"preview-point",role:"button",tabindex:"0","aria-label":p.name});
      group.append(svgEl("circle",{cx:q.x,cy:q.y,r:sel?13:10,fill:m?"#127e8e":"#9d834c",stroke:sel?"#efb95e":"white","stroke-width":sel?4:2}));
      group.append(svgEl("title",{},p.name));
      if(m)group.append(svgEl("text",{x:q.x,y:q.y+4,"font-size":10,"text-anchor":"middle",fill:"white","font-weight":"bold"},S.missions.indexOf(m)+1));
      group.append(svgEl("text",{x:q.x+14,y:q.y-10,class:"preview-label"},p.name));
      group.addEventListener("click",()=>m?selectMission(m.id,"preview-marker"):setStatus(p.name+" — додатна локација"));
      group.addEventListener("keydown",e=>{if(e.key==="Enter"||e.key===" "){e.preventDefault();group.dispatchEvent(new Event("click"));}});
      svg.append(group);
    }
    if(S.player){
      const q=projection.xy(S.player),radius=Math.max(5,Math.min(150,(S.player.accuracyMeters||0)*projection.metersToPx));
      if(S.player.accuracyMeters)svg.append(svgEl("circle",{cx:q.x,cy:q.y,r:radius,fill:"#337bd6","fill-opacity":.14,stroke:"#337bd6","stroke-opacity":.5}));
      svg.append(svgEl("circle",{cx:q.x,cy:q.y,r:8,fill:"#2375d2",stroke:"#fff","stroke-width":3}));
    }
    $("map").replaceChildren(svg);
  }
  function fitAll(){
    if(!S.map){drawPreview();return;}
    const bounds=new google.maps.LatLngBounds();S.points.forEach(p=>bounds.extend(mapPoint(p)));
    S.map.fitBounds(bounds,50);
  }
  function renderPlayer(){
    if(!S.map){drawPreview();return;}
    if(!S.player){
      if(S.playerMarker){S.playerMarker.setMap(null);S.playerMarker=null;}
      if(S.accuracyCircle){S.accuracyCircle.setMap(null);S.accuracyCircle=null;}
      return;
    }
    const pos={lat:S.player.lat,lng:S.player.lng};
    if(!S.playerMarker)S.playerMarker=new google.maps.Marker({map:S.map,title:"Позиција играча",zIndex:999,
      icon:{path:google.maps.SymbolPath.CIRCLE,scale:8,fillColor:"#2777d2",fillOpacity:1,strokeColor:"#fff",strokeWeight:3}});
    S.playerMarker.setPosition(pos);
    if(!S.accuracyCircle)S.accuracyCircle=new google.maps.Circle({map:S.map,strokeColor:"#347cda",strokeOpacity:.55,
      strokeWeight:1,fillColor:"#347cda",fillOpacity:.12,clickable:false});
    S.accuracyCircle.setCenter(pos);S.accuracyCircle.setRadius(S.player.accuracyMeters||0);
    if($("followPlayer").checked)S.map.panTo(pos);
  }
  function setPlayerLocation(location){
    if(!location||typeof location!=="object")return false;
    const lat=Number(location.lat??location.latitude),lng=Number(location.lng??location.longitude);
    if(!G.validLatLng(lat,lng))return false;
    const timestampMs=location.timestampMs===undefined?Date.now():Number(location.timestampMs);
    const now=Date.now();
    if(!Number.isFinite(timestampMs)||timestampMs>now+300000||now-timestampMs>(C.maxLocationAgeMs||120000))return false;
    if(S.player&&timestampMs<S.player.timestampMs)return false;
    const accuracyMeters=location.accuracyMeters==null?null:Number(location.accuracyMeters);
    if(accuracyMeters!==null&&(!Number.isFinite(accuracyMeters)||accuracyMeters<0))return false;
    const source=["native","browser","simulation"].includes(location.source)?location.source:"external";
    S.player={lat,lng,accuracyMeters,timestampMs,source};
    S.locationSource=source;renderPlayer();renderDetail();
    const selected=S.missions.find(m=>m.id===S.selected),p=selected&&missionPoint(selected);
    const dist=p?G.distanceMeters(S.player,p):null,zone=p?G.zoneForDistance(dist,p.zones):null;
    $("gpsStatus").textContent="Локација: "+source+(accuracyMeters!==null?" · ±"+Math.round(accuracyMeters)+" m":" · без податка о тачности")+
      (p?" · "+G.formatDistance(dist)+" до мисије":"");
    setStatus("Приказ позиције ажуриран · "+(zone?"зона "+zone+" (непотврђена)":"без изабране мисије"));
    emit("position-updated",{gameId:S.data?.gameId,source,accuracyMeters,timestampMs,selectedMissionId:selected?.id||null,
      distanceMeters:dist,zoneCandidate:zone,confirmed:false});
    return true;
  }
  function clearPlayerLocation(){
    S.player=null;S.locationSource=null;renderPlayer();renderDetail();
    $("gpsStatus").textContent="Локација није укључена.";
  }
  function stopSources(){
    if(S.simTimer){clearInterval(S.simTimer);S.simTimer=null;}
    if(S.browserWatch!==null&&navigator.geolocation){navigator.geolocation.clearWatch(S.browserWatch);S.browserWatch=null;}
    $("simPlay").classList.remove("active");
  }
  function simulateNext(){
    if(!S.missions.length)return;
    const m=S.missions[S.simIndex%S.missions.length],p=missionPoint(m);S.simIndex++;
    setPlayerLocation({lat:p.lat,lng:p.lng,accuracyMeters:8,timestampMs:Date.now(),source:"simulation"});
    setStatus("СИМУЛАЦИЈА: "+m.title+" · без потврде GPS задатка");
  }
  function startSimulation(){
    stopSources();simulateNext();S.simTimer=setInterval(simulateNext,2200);$("simPlay").classList.add("active");
  }
  function startBrowserLocation(){
    if(!navigator.geolocation){setStatus("Browser geolocation није доступан.");return;}
    stopSources();
    S.browserWatch=navigator.geolocation.watchPosition(pos=>{
      setPlayerLocation({lat:pos.coords.latitude,lng:pos.coords.longitude,accuracyMeters:pos.coords.accuracy,
        timestampMs:pos.timestamp,source:"browser"});
    },err=>setStatus("Грешка геолокације: "+err.message),{enableHighAccuracy:true,maximumAge:0,timeout:15000});
  }
  function loadGoogle(){
    const key=(C.googleMapsApiKey||"").trim();
    if(!key)return Promise.resolve(false);
    if(window.google?.maps?.Map)return Promise.resolve(true);
    return new Promise((resolve,reject)=>{
      const cb="__chronoEyeMapsReady";
      const script=document.createElement("script");
      const params=new URLSearchParams({key,v:"weekly",language:C.googleMapsLanguage||"sr",
        region:C.googleMapsRegion||"RS",loading:"async",callback:cb,auth_referrer_policy:"origin"});
      const timer=setTimeout(()=>{delete window[cb];reject(new Error("Google Maps timeout"));},16000);
      window[cb]=()=>{clearTimeout(timer);delete window[cb];resolve(true);};
      script.async=true;script.src="https://maps.googleapis.com/maps/api/js?"+params.toString();
      script.onerror=()=>{clearTimeout(timer);delete window[cb];reject(new Error("Google Maps JavaScript API failed"));};
      document.head.append(script);
    });
  }
  function wire(){
    $("fitAll").addEventListener("click",fitAll);
    $("showZones").addEventListener("change",()=>S.map?renderZones():drawPreview());
    $("simNext").addEventListener("click",()=>{stopSources();simulateNext();});
    $("simPlay").addEventListener("click",startSimulation);
    $("simStop").addEventListener("click",()=>{stopSources();clearPlayerLocation();setStatus("Симулација/праћење заустављени.");});
    $("locate").addEventListener("click",startBrowserLocation);
    window.addEventListener("pagehide",stopSources);
  }
  async function init(){
    wire();
    try{
      await loadData();renderList();
      const googleReady=await loadGoogle().catch(err=>{setStatus(err.message+" — координатни преглед");return false;});
      if(googleReady)renderGoogle();else drawPreview();
      if(S.missions.length)selectMission(S.missions[0].id,"initial");
      setStatus(googleReady?"Google мапа учитана; GPS/AR нису повезани.":"Координатни преглед учитан; Google кључ није подешен.");
      emit("ready",{gameId:S.data.gameId,mode:S.mode,missions:S.missions.length,waypoints:S.points.length});
    }catch(err){$("map").replaceChildren(el("p","Грешка учитавања: "+err.message,"map-empty"));setStatus("Подаци нису учитани.");}
  }
  window.ChronoEyeMaps=Object.freeze({setPlayerLocation,clearPlayerLocation,selectMission,fitAll,
    getStatus:()=>({ready:!!S.data,mode:S.mode,gameId:S.data?.gameId||null,selectedMissionId:S.selected,
      locationSource:S.locationSource})});
  document.addEventListener("DOMContentLoaded",init);
})();
