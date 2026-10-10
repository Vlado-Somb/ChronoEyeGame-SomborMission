import test from 'node:test';
import assert from 'node:assert/strict';
import {mkdtemp, cp, mkdir, readFile, writeFile, rm} from 'node:fs/promises';
import {tmpdir} from 'node:os';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
import {spawn} from 'node:child_process';

test('HTTP save gate preserves bytes for invalid edits and rejects profile drift', {timeout:20000}, async t => {
  const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '../../..');
  const temp = await mkdtemp(path.join(tmpdir(), 'chrono-save-gate-'));
  const editor = path.join(temp, 'editor/budapest');
  const data = path.join(temp, 'game-data/budapest/v2');
  await mkdir(path.dirname(editor), {recursive:true});
  await mkdir(path.dirname(data), {recursive:true});
  await cp(path.join(root, 'editor/budapest'), editor, {recursive:true});
  await cp(path.join(root, 'game-data/budapest/v2'), data, {recursive:true});
  const child = spawn(process.execPath, ['server.mjs'], {cwd:editor, env:{...process.env,PORT:'44172'}, stdio:['ignore','pipe','pipe']});
  t.after(async () => { child.kill(); await rm(temp, {recursive:true,force:true}); });
  await new Promise((resolve,reject) => {
    const timeout = setTimeout(() => reject(Error('server startup timeout')), 10000);
    child.stdout.on('data', c => { if(c.toString().includes('Budapest Editor')) {clearTimeout(timeout);resolve();} });
    child.on('exit', code => {clearTimeout(timeout);reject(Error('server exited '+code));});
  });
  const origin = 'http://127.0.0.1:44172';
  const get = async p => (await fetch(origin+'/api/file?path='+encodeURIComponent(p))).json();
  const post = (route, body) => fetch(origin+route, {method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify(body)});
  const p = 'missions/act-0.json', before = await get(p);
  const bytes = await readFile(path.join(data,p),'utf8');
  for (const mutate of [d=>d.primaryWaypointId='MISSING', d=>d.tasks[0].id='RENAMED']) {
    const edited = structuredClone(before.data); mutate(edited);
    assert.equal((await post('/api/save',{path:p,data:edited,revision:before.revision})).status,422);
    assert.equal(await readFile(path.join(data,p),'utf8'),bytes);
    const check = await post('/api/validate',{path:p,data:edited});
    assert.equal((await check.json()).ok,false);
  }
  const good = structuredClone(before.data); good.tasks[0].instruction += ' (draft)';
  assert.equal((await post('/api/save',{path:p,data:good,revision:before.revision})).status,200);
  assert.equal((await get(p)).data.tasks[0].instruction,good.tasks[0].instruction);
  assert.equal((await post('/api/save',{path:p,data:before.data,revision:before.revision})).status,409);
  const manifest = await get('manifest.json');
  for (const value of ['edu_mission','',null]) {
    const bad = structuredClone(manifest.data);
    if(value===null) delete bad.missionType; else bad.missionType=value;
    assert.equal((await post('/api/save',{path:'manifest.json',data:bad,revision:manifest.revision})).status,422);
  }
  const invalid = {...manifest.data,missionType:'edu_mission'};
  await writeFile(path.join(data,'manifest.json'),JSON.stringify(invalid));
  assert.equal((await fetch(origin+'/api/file?path='+p)).status,409);
  assert.equal((await fetch(origin+'/api/list')).status,409);
});
