const bridge = window.ChronoNative;
const start = document.getElementById('start');
window.refreshState = () => {
 const state = bridge ? JSON.parse(bridge.getState()) : {collected:0,tested:false};
 document.getElementById('tag').textContent = state.collected ? 'ЗАВРШЕНО ✓' : 'СПРЕМАН';
 document.getElementById('title').textContent = state.collected ? 'Временски траг је сачуван' : 'Ухвати временски траг';
 document.getElementById('description').textContent = state.collected ? 'AR интеракција је стигла у интерфејс. Можеш поново да отвориш камеру и поновиш тест.' : 'Помери телефон да препозна површину, затим додирни под или сто.';
 document.getElementById('stats').textContent = state.tested ? `Преузетих трагова: ${state.collected} · Последњи узорак: ${state.lastFps.toFixed(0)} FPS · Depth: ${state.depthSupported ? 'подржан' : 'није подржан'}` : 'Још нема AR сесије.';
};
start.addEventListener('click', () => { if (bridge) bridge.openAr(); });
document.getElementById('reset').addEventListener('click', () => { if (bridge) bridge.resetProgress(); });
if (!bridge) { start.disabled = true; start.textContent = 'AR је доступан у Android апликацији'; }
window.refreshState();
