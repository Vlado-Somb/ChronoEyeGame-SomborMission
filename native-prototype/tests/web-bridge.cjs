// Contract test for the web shell with a stubbed Android bridge.
const fs = require('node:fs');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const path = require('node:path');
const source = fs.readFileSync(path.join(__dirname, '../app/src/main/assets/web/app.js'), 'utf8');
function harness(native = true) {
  const ids = ['start', 'tag', 'title', 'description', 'stats', 'reset'];
  const elements = Object.fromEntries(ids.map(id => [id, {textContent: '', addEventListener(event, fn) { this[event] = fn; }}]));
  let state = {collected: 0, lastFps: 29.5, depthSupported: true, tested: true}, opened = 0;
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
assert.equal(h.elements.tag.textContent, 'СПРЕМАН');
h.elements.start.click();
assert.equal(h.opened(), 1);
h.state.collected = 1;
h.window.refreshState();
assert.match(h.elements.title.textContent, /сачуван/);
assert.match(h.elements.stats.textContent, /1/);
h.elements.reset.click();
assert.equal(h.state.collected, 0);
assert.equal(h.elements.tag.textContent, 'СПРЕМАН');
assert.equal(harness(false).elements.start.disabled, true);
console.log('PASS: AR launch request, initial/collected/reset states, browser fallback');
