import http from 'node:http';
import { readFile, writeFile, mkdir, open, rename, copyFile, unlink, realpath, stat } from 'node:fs/promises';
import { existsSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { randomUUID } from 'node:crypto';
import { allowedPath, digest, validateDocument, verifyLinks } from './lib/core.mjs';
import { checkCandidate } from './lib/candidate.mjs';

const DIR = path.dirname(fileURLToPath(import.meta.url));
const DATA = path.resolve(DIR, '../../game-data/budapest/v2');
const PUBLIC = path.join(DIR, 'public');
const BACKUPS = path.join(DIR, '.editor-backups');
const PORT = Number(process.env.PORT || 4177);
if (!Number.isInteger(PORT) || PORT < 1024 || PORT > 65535) throw Error('Invalid PORT');
if (!existsSync(path.join(DATA, 'manifest.json'))) throw Error('Budapest v2 package not found: ' + DATA);
const DATA_REAL = await realpath(DATA);
async function assertProfile() {
  const manifest = JSON.parse(await readFile(await filePath('manifest.json'), 'utf8'));
  if (!validateDocument('manifest.json', manifest).ok)
    throw Object.assign(Error('Budapest package profile mismatch: explicit migration required'), {status: 409});
}
const MAX = 2 * 1024 * 1024;
const json = (res, status, body) => send(res, status, 'application/json; charset=utf-8', JSON.stringify(body));
function send(res, status, mime, body) {
  res.writeHead(status, { 'Content-Type': mime, 'Cache-Control': 'no-store', 'X-Content-Type-Options': 'nosniff',
    'Referrer-Policy': 'strict-origin-when-cross-origin', 'X-Frame-Options': 'DENY',
    'Content-Security-Policy': "default-src 'self'; script-src 'self' https://unpkg.com; style-src 'self' https://unpkg.com 'unsafe-inline'; img-src 'self' data: https://tile.openstreetmap.org https://unpkg.com; connect-src 'self'; object-src 'none'; base-uri 'none'; frame-ancestors 'none'" });
  res.end(body);
}
function assertClient(req) {
  const host = req.headers.host;
  if (host !== '127.0.0.1:' + PORT && host !== 'localhost:' + PORT) throw Object.assign(Error('Invalid host'), { status: 403 });
  const origin = req.headers.origin;
  if (origin && origin !== 'http://127.0.0.1:' + PORT && origin !== 'http://localhost:' + PORT) throw Object.assign(Error('Invalid origin'), { status: 403 });
}
async function filePath(relative) {
  if (!allowedPath(relative)) throw Object.assign(Error('Not an editable JSON file'), { status: 400 });
  const full = path.resolve(DATA, relative);
  if (!full.startsWith(DATA + path.sep)) throw Object.assign(Error('Outside data root'), { status: 400 });
  const actual = await realpath(full);
  if (!actual.startsWith(DATA_REAL + path.sep)) throw Object.assign(Error('Symlink outside data root'), { status: 403 });
  const s = await stat(actual);
  if (!s.isFile() || s.size > MAX) throw Object.assign(Error('Not a regular small JSON file'), { status: 400 });
  return actual;
}
async function body(req) {
  let size = 0, chunks = [];
  for await (const part of req) {
    size += part.length;
    if (size > MAX) throw Object.assign(Error('Request too large'), { status: 413 });
    chunks.push(part);
  }
  return JSON.parse(Buffer.concat(chunks).toString('utf8'));
}
async function read(relative) {
  const full = await filePath(relative);
  const raw = await readFile(full, 'utf8');
  return { path: relative, data: JSON.parse(raw), revision: digest(raw) };
}
async function qa() {
  const readData = async p => (await read(p)).data;
  const all = { waypoints: await readData('waypoints.json'), sources: await readData('sources.json'),
    characters: await readData('systems/characters.json'), missions: [], dialogues: [], flows: [] };
  for (let i = 0; i <= 6; i++) {
    all.missions.push(await readData('missions/act-' + i + '.json'));
    all.dialogues.push(await readData('dialogues/act-' + i + '.json'));
    all.flows.push(await readData('production/act-' + i + '/scene-flow.json'));
  }
  const result = verifyLinks(all);
  for (const p of ['waypoints.json', ...Array.from({length:7}, (_,i)=>'dialogues/act-'+i+'.json'),
    ...Array.from({length:7}, (_,i)=>'missions/act-'+i+'.json'),
    ...Array.from({length:7}, (_,i)=>'production/act-'+i+'/scene-flow.json')]) {
    const checked = validateDocument(p, await readData(p));
    for (const e of checked.errors) result.errors.push({ location: p + ':' + e.location, message: e.message });
  }
  result.ok = result.errors.length === 0;
  return result;
}
const routes = { '/': ['index.html','text/html; charset=utf-8'], '/app.js': ['app.js','text/javascript; charset=utf-8'], '/map-provider.js': ['map-provider.js','text/javascript; charset=utf-8'],
  '/style.css': ['style.css','text/css; charset=utf-8'] };
const server = http.createServer(async (req,res) => {
  try {
    assertClient(req);
    await assertProfile();
    const u = new URL(req.url, 'http://127.0.0.1:' + PORT);
    if (req.method === 'GET' && routes[u.pathname]) {
      const [filename, mime] = routes[u.pathname];
      return send(res, 200, mime, await readFile(path.join(PUBLIC, filename)));
    }
    if (req.method === 'GET' && u.pathname === '/api/list') {
      return json(res, 200, { files: ['waypoints.json','sources.json','manifest.json','evidence.json','hints.json','mechanics.json',
        'signals.json','timeline.json','triggers.json','systems/characters.json','systems/collectibles.json','systems/album.json',
        'assets/index.json','media/cues.json',
        ...Array.from({length:7},(_,i)=>'missions/act-'+i+'.json'),
        ...Array.from({length:7},(_,i)=>'dialogues/act-'+i+'.json'),
        ...Array.from({length:7},(_,i)=>'production/act-'+i+'/scene-flow.json'),
        ...Array.from({length:7},(_,i)=>'production/act-'+i+'/asset-manifest.json')] });
    }
    if (req.method === 'GET' && u.pathname === '/api/file') return json(res, 200, await read(u.searchParams.get('path')));
    if (req.method === 'GET' && u.pathname === '/api/qa') return json(res, 200, await qa());
    if (req.method === 'POST' && u.pathname === '/api/validate') {
      if (!req.headers['content-type']?.startsWith('application/json')) return json(res,415,{error:'JSON required'});
      const b = await body(req);
      await filePath(b.path);
      return json(res,200,await checkCandidate(b.path,b.data,(await read(b.path)).data,async p => (await read(p)).data));
    }
    if (req.method === 'POST' && u.pathname === '/api/save') {
      if (!req.headers['content-type']?.startsWith('application/json')) return json(res,415,{error:'JSON required'});
      const b = await body(req), full = await filePath(b.path);
      if (typeof b.revision !== 'string') return json(res,400,{error:'Revision required'});
      const original = await readFile(full,'utf8');
      const validation = await checkCandidate(b.path,b.data,JSON.parse(original),async p => (await read(p)).data);
      if (!validation.ok) return json(res,422,validation);
      if (digest(original) !== b.revision) return json(res,409,{error:'File changed since load; reload and compare before saving'});
      const updated = JSON.stringify(b.data,null,2) + '\n';
      if (updated.length > MAX) return json(res,413,{error:'Document too large'});
      await mkdir(BACKUPS,{recursive:true,mode:0o700});
      const backup = path.join(BACKUPS,b.path.replaceAll('/','__') + '.' + Date.now() + '.' + randomUUID() + '.json');
      await copyFile(full,backup);
      const tmp = full + '.' + randomUUID() + '.tmp';
      try {
        const handle = await open(tmp,'wx',0o600);
        try { await handle.writeFile(updated); await handle.sync(); } finally { await handle.close(); }
        // Recheck for edits during backup/write (best-effort; single-editor local workflow).
        if (digest(await readFile(full,'utf8')) !== b.revision) throw Object.assign(Error('Concurrent edit; backup retained'),{status:409});
        await rename(tmp,full);
      } catch(e) { await unlink(tmp).catch(()=>{}); throw e; }
      return json(res,200,{saved:true,revision:digest(updated),backup:path.basename(backup),warnings:validation.warnings});
    }
    return json(res,404,{error:'Not found'});
  } catch(e) {
    console.error('[editor]',e.message);
    return json(res,e.status || (e instanceof SyntaxError?400:500),{error:e.message});
  }
});
await assertProfile();
server.listen(PORT,'127.0.0.1',()=>console.log('CHRONO EYE Budapest Editor: http://127.0.0.1:'+PORT));
