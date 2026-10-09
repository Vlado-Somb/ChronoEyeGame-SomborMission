const bridge = window.ChronoNative;
const start = document.getElementById('start');
window.refreshState = () => {
 const state = bridge ? JSON.parse(bridge.getState()) : {collected:0,tested:false};
 document.getElementById('tag').textContent = state.testFinished ? 'ТЕСТ ЗАВРШЕН ✓' : state.tested ? 'ТЕСТИРАЊЕ' : 'СПРЕМАН';
 document.getElementById('title').textContent = state.testFinished ? 'Теренски тест је завршен' : 'Испитај AR кристале';
 document.getElementById('description').textContent = state.testFinished ? 'Можеш поново отворити AR камеру: свих 5 дијаманата поново је доступно. Подели ZIP дијагностику за анализу.' : 'Додирни под или сто да поставиш до 5 дијаманата. Додирни дијамант да га сакупиш.';
 document.getElementById('stats').textContent = state.tested ? `Преузетих трагова: ${state.collected} · Последњи узорак: ${state.lastFps.toFixed(0)} FPS · Depth: ${state.depthSupported ? 'подржан' : 'није подржан'}` : 'Још нема AR сесије.';
};
start.addEventListener('click', () => { if (bridge) bridge.openAr(); });
document.getElementById('reset').addEventListener('click', () => { if (bridge) bridge.resetProgress(); });
if (!bridge) { start.disabled = true; start.textContent = 'AR је доступан у Android апликацији'; }
window.refreshState();
