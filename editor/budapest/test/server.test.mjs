import test from 'node:test';
import assert from 'node:assert/strict';
import { spawn } from 'node:child_process';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const dir=path.resolve(path.dirname(fileURLToPath(import.meta.url)),'..');
test('localhost API smoke: read, validate, reject unsafe requests', {timeout:20000}, async t=>{
 const port=44171,origin='http://127.0.0.1:'+port;
 const child=spawn(process.execPath,['server.mjs'],{cwd:dir,env:{...process.env,PORT:String(port)},stdio:['ignore','pipe','pipe']});
 t.after(()=>child.kill());
 let output='';
 child.stderr.on('data',c=>output+=c.toString());
 await new Promise((resolve,reject)=>{
  const timer=setTimeout(()=>reject(Error('Server did not start: '+output)),12000);
  child.stdout.on('data',c=>{if(c.toString().includes('Budapest Editor')){clearTimeout(timer);resolve();}});
  child.on('exit',code=>{clearTimeout(timer);reject(Error('Server exited '+code+': '+output));});
 });
 const listing=await fetch(origin+'/api/list');
 assert.equal(listing.status,200);
 const files=(await listing.json()).files;
 assert.ok(files.includes('waypoints.json'));
 const response=await fetch(origin+'/api/file?path=waypoints.json');
 assert.equal(response.status,200);
 const current=await response.json();
 assert.ok(current.revision);
 assert.equal(current.data.waypoints.length,11);
 const checked=await fetch(origin+'/api/validate',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({path:'waypoints.json',data:current.data})});
 assert.equal(checked.status,200);
 assert.equal((await checked.json()).ok,true);
 const bad=await fetch(origin+'/api/file?path=../../.env');
 assert.equal(bad.status,400);
 const cross=await fetch(origin+'/api/validate',{method:'POST',headers:{Origin:'https://invalid.example','Content-Type':'application/json'},body:'{}'});
 assert.equal(cross.status,403);
});
