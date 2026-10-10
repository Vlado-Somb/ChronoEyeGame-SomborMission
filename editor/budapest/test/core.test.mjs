import test from 'node:test';
import assert from 'node:assert/strict';
import { allowedPath, validateDocument, verifyLinks, digest } from '../lib/core.mjs';
test('allowlist rejects traversal and other cities',()=>{
  assert.equal(allowedPath('waypoints.json'),true);
  assert.equal(allowedPath('dialogues/act-6.json'),true);
  for (const p of ['../../.env','../sombor/v1/manifest.json','missions/act-7.json','../../ProjectSettings/a.json','waypoints.json/../secret']) assert.equal(allowedPath(p),false);
});
test('digest detects changes',()=>assert.notEqual(digest('a'),digest('b')));
test('WGS84 pair and unverified navigation',()=>{
  const good={waypoints:[{id:'BP_X',latitude:null,longitude:null,fieldVerified:false,navigationEnabled:false,zones:[],safeStandingPoint:null,arAnchor:null}]};
  assert.equal(validateDocument('waypoints.json',good).ok,true);
  good.waypoints[0].longitude=19;
  assert.equal(validateDocument('waypoints.json',good).ok,false);
  good.waypoints[0].latitude=47;
  good.waypoints[0].navigationEnabled=true;
  assert.equal(validateDocument('waypoints.json',good).ok,false);
});
test('dialogue sequence referential integrity',()=>{
  const d={scenes:[{id:'S',lines:[{id:'L',speakerId:'C',text:'t',subtitle:'t'}],stageDirections:[],mediaCues:[],gameplayBeats:[],sequence:[{type:'line',id:'L'}]}]};
  assert.equal(validateDocument('dialogues/act-1.json',d).ok,true);
  d.scenes[0].sequence[0].id='MISSING';
  assert.equal(validateDocument('dialogues/act-1.json',d).ok,false);
});
test('quiz index and AR/2D parity',()=>{
  const m={id:'M',tasks:[{id:'T',type:'multiple_choice',question:{choices:['a','b'],correctIndex:1}},{id:'A',type:'ar_interaction',ar:{successCondition:{x:1}},fallback:{successCondition:{x:1}}}]};
  assert.equal(validateDocument('missions/act-1.json',m).ok,true);
  m.tasks[1].fallback.successCondition={x:2};
  assert.equal(validateDocument('missions/act-1.json',m).ok,false);
});
test('cross-file missing refs',()=>{
  const q=verifyLinks({waypoints:{waypoints:[{id:'A',fieldVerified:false}]},sources:{sources:[]},characters:{characters:[]},missions:[{id:'M',primaryWaypointId:'MISSING',tasks:[],prerequisiteMissionIds:[]}],dialogues:[],flows:[]});
  assert.equal(q.ok,false);
  assert.ok(q.warnings.length);
});
