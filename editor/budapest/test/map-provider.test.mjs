import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import path from 'node:path';
import vm from 'node:vm';

const dir = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const source = await readFile(path.join(dir, 'public/map-provider.js'), 'utf8');
const win = {};
vm.runInNewContext(source, { window: win });
const providers = win.BudapestMapProviders;

test('OSM adapter uses official tile endpoint, attribution, no Google key', () => {
  assert.equal(providers.osm.tileUrl, 'https://tile.openstreetmap.org/{z}/{x}/{y}.png');
  assert.match(providers.osm.attribution, /OpenStreetMap contributors/);
  assert.equal(Array.from(providers.available).join(','), 'osm');
  assert.throws(() => providers.create('google', 'map', [47, 19], 12), /not configured/);
});
test('OSM adapter keeps drag/click events independent of tile engine', () => {
  const events = {};
  let marker, tileOptions, tileUrl, center;
  const fakeMap = { setView(c) { center=c; return this; }, on(event, fn) { events[event]=fn; }, invalidateSize() {}, remove() {} };
  const fakePins = { clearLayers() {}, addTo() { return this; } };
  const L = {
    map() { return fakeMap; },
    tileLayer(url, options) { tileUrl=url; tileOptions=options; return {addTo() {}}; },
    layerGroup() { return fakePins; },
    marker(point, options) {
      assert.equal(options.draggable, true);
      marker={point, handlers:{}, addTo() { return this; }, bindTooltip() {return this;}, on(type, cb) {this.handlers[type]=cb; return this;}};
      return marker;
    }
  };
  const adapter=providers.create('osm', 'map', [47.5,19.04], 12, L);
  assert.equal(tileUrl, providers.osm.tileUrl);
  assert.match(tileOptions.attribution, /OpenStreetMap/);
  assert.equal(center[0], 47.5);
  let clicked, dragged;
  adapter.onClick(p=>clicked=p);
  events.click({latlng:{lat:47.1,lng:19.1}});
  assert.equal(clicked.lat,47.1);
  adapter.addPin({latitude:47.5,longitude:19.04,label:'pending',onDragEnd:p=>dragged=p});
  marker.handlers.dragend({target:{getLatLng(){return {lat:47.6,lng:19.2};}}});
  assert.equal(dragged.lng,19.2);
});
test('HTML and localhost headers permit OSM tiles without leaking credentials', async () => {
  const html=await readFile(path.join(dir,'public/index.html'),'utf8');
  const server=await readFile(path.join(dir,'server.mjs'),'utf8');
  assert.ok(html.indexOf('/map-provider.js')<html.indexOf('/app.js'));
  assert.match(server, /strict-origin-when-cross-origin/);
  assert.match(server, /https:\/\/tile\.openstreetmap\.org/);
  assert.doesNotMatch(server, /'Referrer-Policy': 'no-referrer'/);
});
