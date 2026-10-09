// Contract test for the packaged WebView shell with a stubbed native bridge.
const fs = require('node:fs');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const path = require('node:path');
const source = fs.readFileSync(path.join(__dirname, '../app/src/main/assets/web/app.js'), 'utf8');
const html = fs.readFileSync(path.join(__dirname, '../app/src/main/assets/web/index.html'), 'utf8');
assert.match(html, /id="app-version"/);
assert.doesNotMatch(html, /AR LAB · 0\.3/);
function harness(native = true) {
  const ids = ['start', 'tag', 'title', 'description', 'stats', 'reset', 'app-version'];
  const elements = Object.fromEntries(ids.map(id => [id, {textContent: '', addEventListener(event, fn) { this[event] = fn; }}]));
  let state = {appVersion: '0.4.1', collected: 0, lastFps: 29.5, depthSupported: true, tested: true, testFinished: false}, opened = 0;
  const window = {};
  if (native) window.ChronoNative = {
    getState: () => JSON.stringify(state),
    openAr: () => opened++,
    resetProgress: () => { state.collected = 0; window.refreshState(); }
  };
  vm.runInNewContext(source, {window, document: {getElementById: id => elements[id]}});
  return {window, elements, state, opened: () => opened};
}
const h = harness();
assert.equal(h.elements['app-version'].textContent, 'AR LAB · 0.4.1');
assert.equal(h.elements.tag.textContent, 'ТЕСТИРАЊЕ');
h.elements.start.click();
assert.equal(h.opened(), 1);
h.state.collected = 1;
h.window.refreshState();
assert.match(h.elements.title.textContent, /кристале/);
assert.match(h.elements.stats.textContent, /1/);
h.elements.reset.click();
assert.equal(h.state.collected, 0);
h.state.testFinished = true;
h.window.refreshState();
assert.equal(h.elements.tag.textContent, 'ТЕСТ ЗАВРШЕН ✓');
assert.equal(harness(false).elements.start.disabled, true);
assert.equal(harness(false).elements['app-version'].textContent, 'AR LAB');
console.log('PASS: package version, launch, collection, reset, finish and browser fallback');
