/* Load in the same trusted top-level page as Maps. Android injects ChronoHost. */
(function () {
  'use strict';
  const bridge = window.ChronoHost;
  if (!bridge || typeof bridge.postMessage !== 'function') return;
  function send(type, payload) {
    bridge.postMessage(JSON.stringify({version:1, type, payload}));
  }
  window.addEventListener('chronoeye:map:ready', () => send('STATE_REQUEST', {}));
  window.addEventListener('chronoeye:map:mission-selected', e => {
    if (['external','initial'].includes(e.detail.source)) return;
    send('MISSION_SELECT', {gameId:e.detail.gameId, missionId:e.detail.missionId});
  });
  bridge.onmessage = e => {
    let m; try { m=JSON.parse(e.data); } catch (_) { return; }
    if (m.version!==1) return;
    if (m.type==='LOCATION') window.ChronoEyeMaps?.setPlayerLocation({...m.payload,source:'native'});
    if (m.type==='LOCATION_CLEAR') window.ChronoEyeMaps?.clearPlayerLocation();
    if (m.type==='STATE') {
      if(m.payload.activeMissionId) window.ChronoEyeMaps?.selectMission(m.payload.activeMissionId);
      window.dispatchEvent(new CustomEvent('chronoeye:host:state',{detail:m.payload}));
    }
    if (m.type==='ERROR') window.dispatchEvent(new CustomEvent('chronoeye:host:error',{detail:m.payload}));
  };
  // UI buttons may call these intents; no completion/points/GPS evidence API is exposed.
  window.ChronoSession = Object.freeze({
    start:()=>send('SESSION_START',{}), pause:()=>send('SESSION_PAUSE',{}),
    resume:()=>send('SESSION_RESUME',{}), openAr:()=>send('AR_REQUEST',{}),
    refresh:()=>send('STATE_REQUEST',{})
  });
})();
