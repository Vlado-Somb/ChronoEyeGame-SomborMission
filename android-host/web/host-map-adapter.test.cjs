const {test}=require('node:test');
const assert=require('node:assert/strict');
const vm=require('node:vm');
const fs=require('node:fs');
test('map selection forwards intent; state reselect does not loop; position never becomes evidence',()=>{
 const listeners={},sent=[],locations=[];
 const window={ChronoHost:{postMessage:s=>sent.push(JSON.parse(s))},
  addEventListener:(n,f)=>listeners[n]=f,dispatchEvent:e=>listeners[e.type]?.(e),
  ChronoEyeMaps:{selectMission:id=>listeners['chronoeye:map:mission-selected']({detail:{missionId:id,source:'external'}}),
   setPlayerLocation:p=>locations.push(p),clearPlayerLocation:()=>{}}};
 vm.runInNewContext(fs.readFileSync(__dirname+'/host-map-adapter.js','utf8'),{window,CustomEvent:class {constructor(type,o){this.type=type;this.detail=o.detail;}}});
 listeners['chronoeye:map:mission-selected']({detail:{gameId:'g',missionId:'m',source:'marker'}});
 assert.equal(sent[0].type,'MISSION_SELECT');
 window.ChronoHost.onmessage({data:JSON.stringify({version:1,type:'STATE',payload:{activeMissionId:'m'}})});
 assert.equal(sent.length,1);
 assert.equal(listeners['chronoeye:map:position-updated'],undefined);
 window.ChronoHost.onmessage({data:JSON.stringify({version:1,type:'LOCATION',payload:{latitude:1,longitude:2}})});
 assert.equal(locations[0].source,'native');
 window.ChronoHost.onmessage({data:'invalid'});
 window.ChronoSession.openAr();assert.equal(sent[1].type,'AR_REQUEST');
 assert.deepEqual(Object.keys(sent[1].payload),[]);
});
