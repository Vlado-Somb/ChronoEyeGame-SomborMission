import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { flowTaskErrors } from '../lib/identity.mjs';
const root=path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../../game-data/budapest/v2');
const read=async p=>JSON.parse(await readFile(path.join(root,p),'utf8'));
test('all seven scene flows reference tasks and dialogue in their own acts',async()=>{
 const missions=[],flows=[],dialogues=[];
 for(let i=0;i<7;i++){
  missions.push(await read('missions/act-'+i+'.json'));
  flows.push(await read('production/act-'+i+'/scene-flow.json'));
  dialogues.push(await read('dialogues/act-'+i+'.json'));
 }
 assert.deepEqual(flowTaskErrors(missions,flows,dialogues),[]);
 const broken=JSON.parse(JSON.stringify(flows));
 broken[0].missionId='WRONG_MISSION';
 assert.ok(flowTaskErrors(missions,broken,dialogues).length>0);
});
