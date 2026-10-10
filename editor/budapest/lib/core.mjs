import { createHash } from 'node:crypto';

const FILES = /^(?:waypoints|manifest|sources|evidence|hints|mechanics|signals|timeline|triggers)\.json$|^(?:dialogues|missions)\/act-[0-6]\.json$|^production\/act-[0-6]\/(?:scene-flow|asset-manifest)\.json$|^systems\/(?:characters|collectibles|album)\.json$|^assets\/index\.json$|^media\/cues\.json$/;

export function allowedPath(path) {
  return typeof path === 'string' && FILES.test(path);
}
export function digest(content) {
  return createHash('sha256').update(content).digest('hex');
}
function issue(errors, location, message) {
  errors.push({ location, message });
}
function coordinatePair(errors, value, location, optional = false) {
  if (value === null && optional) return;
  if (!value || typeof value !== 'object' || Array.isArray(value)) {
    issue(errors, location, 'Expected coordinate object'); return;
  }
  const lat = value.latitude, lon = value.longitude;
  if (lat === null && lon === null && optional) return;
  if (typeof lat !== 'number' || !Number.isFinite(lat) || lat < -90 || lat > 90 ||
      typeof lon !== 'number' || !Number.isFinite(lon) || lon < -180 || lon > 180) {
    issue(errors, location, 'Latitude/longitude must both be finite WGS84 numbers within range');
  }
}
export function validateDocument(path, data) {
  const errors = [], warnings = [];
  if (!allowedPath(path)) return { ok: false, errors: [{ location: path, message: 'Path not editable' }], warnings };
  if (!data || typeof data !== 'object' || Array.isArray(data)) {
    return { ok: false, errors: [{ location: '$', message: 'JSON root must be an object' }], warnings };
  }
  if (path === 'manifest.json') {
    if (data.gameId !== 'chrono-eye-budapest' || data.missionType !== 'game_mission')
      issue(errors, '$.missionType', 'Budapest editor requires chrono-eye-budapest / game_mission; migrate explicitly before saving');
  } else if (path === 'waypoints.json') {
    if (!Array.isArray(data.waypoints)) issue(errors, '$.waypoints', 'Expected array');
    const ids = new Set();
    for (const [i, w] of (data.waypoints || []).entries()) {
      const at = '$.waypoints[' + i + ']';
      if (!w || typeof w.id !== 'string' || !w.id) issue(errors, at + '.id', 'Stable ID required');
      else if (ids.has(w.id)) issue(errors, at + '.id', 'Duplicate ID');
      else ids.add(w.id);
      coordinatePair(errors, w, at, true);
      if (w.safeStandingPoint !== null && w.safeStandingPoint !== undefined) coordinatePair(errors, w.safeStandingPoint, at + '.safeStandingPoint');
      if (w.arAnchor !== null && w.arAnchor !== undefined) coordinatePair(errors, w.arAnchor, at + '.arAnchor');
      if (!Array.isArray(w.zones)) issue(errors, at + '.zones', 'Expected zones array');
      if (w.navigationEnabled && !w.fieldVerified) issue(errors, at + '.navigationEnabled', 'Cannot enable unverified navigation');
      if (!w.fieldVerified) warnings.push({ location: at, message: 'Survey pending: do not use for production routing' });
    }
  } else if (/^dialogues\//.test(path)) {
    if (!Array.isArray(data.scenes)) issue(errors, '$.scenes', 'Expected scenes array');
    const sceneIds = new Set();
    for (const [i, s] of (data.scenes || []).entries()) {
      const at = '$.scenes[' + i + ']';
      if (!s.id || sceneIds.has(s.id)) issue(errors, at + '.id', 'Scene ID missing or duplicated');
      sceneIds.add(s.id);
      const refs = new Set();
      for (const [type, key] of [['line','lines'],['stage','stageDirections'],['cue','mediaCues'],['beat','gameplayBeats']]) {
        if (!Array.isArray(s[key])) issue(errors, at + '.' + key, 'Expected array');
        for (const item of s[key] || []) {
          if (!item.id || refs.has(type + ':' + item.id)) issue(errors, at + '.' + key, 'Missing/duplicate item ID');
          refs.add(type + ':' + item.id);
          if (type === 'line' && (typeof item.text !== 'string' || typeof item.subtitle !== 'string' || !item.speakerId)) {
            issue(errors, at + '.' + key, 'Line requires speakerId, text and subtitle');
          }
        }
      }
      if (!Array.isArray(s.sequence)) issue(errors, at + '.sequence', 'Expected sequence array');
      for (const step of s.sequence || []) {
        if (!refs.has(step.type + ':' + step.id)) issue(errors, at + '.sequence', 'Broken sequence ref ' + step.type + ':' + step.id);
      }
    }
  } else if (/^missions\//.test(path)) {
    if (!data.id || !Array.isArray(data.tasks)) issue(errors, '$', 'Mission requires id and tasks');
    const ids = new Set();
    for (const t of data.tasks || []) {
      if (!t.id || ids.has(t.id)) issue(errors, '$.tasks', 'Task ID missing or duplicated');
      ids.add(t.id);
      if (t.type === 'multiple_choice' && t.question) {
        const q = t.question;
        if (!Array.isArray(q.choices) || !Number.isInteger(q.correctIndex) || q.correctIndex < 0 || q.correctIndex >= q.choices.length) issue(errors, '$.tasks.' + t.id, 'Invalid quiz correctIndex/choices');
      }
      if (t.type === 'ar_interaction' && t.ar && t.fallback && JSON.stringify(t.ar.successCondition) !== JSON.stringify(t.fallback.successCondition)) {
        issue(errors, '$.tasks.' + t.id, 'AR/2D success conditions must match');
      }
    }
  } else if (/^production\/.*scene-flow/.test(path)) {
    if (!Array.isArray(data.nodes) || !data.entryNodeId) issue(errors, '$', 'Flow needs nodes and entryNodeId');
    const ids = new Set((data.nodes || []).map(n => n.id));
    if (!ids.has(data.entryNodeId)) issue(errors, '$.entryNodeId', 'Missing entry node');
    for (const n of data.nodes || []) {
      if (n.next && !ids.has(n.next)) issue(errors, '$.nodes.' + n.id, 'Missing next node');
      for (const c of n.choices || []) if (c.next && !ids.has(c.next)) issue(errors, '$.nodes.' + n.id, 'Missing choice target');
    }
  }
  return { ok: errors.length === 0, errors, warnings };
}
export function verifyLinks(data) {
  const errors = [], warnings = [];
  const waypoints = new Set((data.waypoints?.waypoints || []).map(x => x.id));
  const sources = new Set((data.sources?.sources || []).map(x => x.id));
  const characters = new Set((data.characters?.characters || []).map(x => x.id));
  const missions = data.missions || [], dialogues = data.dialogues || [];
  const missionIds = new Set(missions.map(m => m.id));
  const sceneIds = new Set(dialogues.flatMap(d => (d.scenes || []).map(s => s.id)));
  for (const m of missions) {
    if (!waypoints.has(m.primaryWaypointId)) issue(errors, m.id, 'Missing primary waypoint ' + m.primaryWaypointId);
    for (const id of m.prerequisiteMissionIds || []) if (!missionIds.has(id)) issue(errors, m.id, 'Missing prerequisite ' + id);
    for (const id of m.narrative?.sources || []) if (!sources.has(id)) issue(errors, m.id, 'Missing source ' + id);
    for (const t of m.tasks || []) {
      if (t.waypointId && !waypoints.has(t.waypointId)) issue(errors, t.id, 'Missing waypoint ' + t.waypointId);
      if (t.gps?.waypointId && !waypoints.has(t.gps.waypointId)) issue(errors, t.id, 'Missing GPS waypoint ' + t.gps.waypointId);
      for (const id of t.sources || []) if (!sources.has(id)) issue(errors, t.id, 'Missing source ' + id);
    }
  }
  for (const d of dialogues) for (const s of d.scenes || []) for (const l of s.lines || []) {
    if (characters.size && !characters.has(l.speakerId)) issue(errors, l.id, 'Missing character ' + l.speakerId);
  }
  for (const f of data.flows || []) for (const n of f.nodes || []) {
    if (n.dialogueId && !sceneIds.has(n.dialogueId)) issue(errors, n.id, 'Missing dialogue scene ' + n.dialogueId);
  }
  for (const w of data.waypoints?.waypoints || []) if (!w.fieldVerified) warnings.push({ location: w.id, message: 'Waypoint not field-verified' });
  return { ok: errors.length === 0, errors, warnings, counts: { waypoints: waypoints.size, missions: missions.length, scenes: sceneIds.size } };
}
