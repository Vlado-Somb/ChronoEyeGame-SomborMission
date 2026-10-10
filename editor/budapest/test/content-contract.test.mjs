import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { verifyLinks } from '../lib/core.mjs';

const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../../game-data/budapest/v2');
const read=async name=>JSON.parse(await readFile(path.join(root,name),'utf8'));
test('Budapest v2 identity and canonical AR mapping regression', async () => {
  const waypoints=await read('waypoints.json');
  const sources=await read('sources.json');
  const characters=await read('systems/characters.json');
  const missions=[],dialogues=[],flows=[];
  for(let i=0;i<7;i++){
    missions.push(await read('missions/act-'+i+'.json'));
    dialogues.push(await read('dialogues/act-'+i+'.json'));
    flows.push(await read('production/act-'+i+'/scene-flow.json'));
  }
  const scenes=dialogues.flatMap(d=>d.scenes);
  const lines=scenes.flatMap(s=>s.lines);
  assert.equal(waypoints.waypoints.length,11);
  assert.equal(missions.length,7);
  assert.equal(scenes.length,48);
  assert.equal(lines.length,375);
  assert.equal(new Set(waypoints.waypoints.map(w=>w.id)).size,11);
  assert.equal(new Set(scenes.map(s=>s.id)).size,48);
  assert.equal(new Set(lines.map(l=>l.id)).size,375);
  const explore=missions[0].tasks.find(t=>t.id==='BP2_A0_EXPLORE');
  assert.equal(explore?.ar?.interaction,'evidence_mapping');
  assert.deepEqual(explore.ar.successCondition, explore.fallback.successCondition);
  assert.ok(waypoints.waypoints.every(w=>!w.fieldVerified && !w.navigationEnabled));
  const qa=verifyLinks({waypoints,sources,characters,missions,dialogues,flows});
  assert.equal(qa.ok,true,JSON.stringify(qa.errors));
});
