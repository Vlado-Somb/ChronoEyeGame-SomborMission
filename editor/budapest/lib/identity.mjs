// The editor may change prose, but must not silently delete or rename canonical IDs.
function ids(path, doc) {
  if (path === 'waypoints.json') return (doc.waypoints || []).map(w => w.id);
  if (path === 'sources.json') return (doc.sources || []).map(s => s.id);
  if (/^missions\/act-[0-6]\.json$/.test(path)) return [doc.id, ...(doc.tasks || []).map(t => t.id)];
  if (/^dialogues\/act-[0-6]\.json$/.test(path)) return [
    doc.id,
    ...(doc.scenes || []).flatMap(s => [s.id,
      ...['lines','stageDirections','mediaCues','gameplayBeats'].flatMap(k => (s[k] || []).map(x => k + ':' + x.id))
    ])
  ];
  if (/^production\/act-[0-6]\/scene-flow\.json$/.test(path)) return [
    doc.id, ...(doc.nodes || []).map(n => n.id)
  ];
  return null;
}
export function identityErrors(path, before, after) {
  const a = ids(path, before), b = ids(path, after);
  if (a === null) return [];
  if (b === null || a.length !== b.length ||
      a.slice().sort().some((id, i) => id !== b.slice().sort()[i])) {
    return [{location: path, message: 'Stable IDs or item count changed; use an explicit content migration, not the local editor'}];
  }
  return [];
}
export function flowTaskErrors(missions, flows, dialogues) {
  const errors = [];
  for (let i = 0; i < flows.length; i++) {
    const flow = flows[i], mission = missions[i], dialogue = dialogues[i];
    if (flow.missionId !== mission.id)
      errors.push({location: 'production/act-' + i + '/scene-flow.json', message: 'Flow missionId does not match mission'});
    const taskIds = new Set((mission.tasks || []).map(t => t.id));
    const sceneIds = new Set((dialogue.scenes || []).map(s => s.id));
    for (const node of flow.nodes || []) {
      if (node.taskId && !taskIds.has(node.taskId))
        errors.push({location: node.id, message: 'Missing task ' + node.taskId + ' in act ' + i});
      if (node.dialogueId && !sceneIds.has(node.dialogueId))
        errors.push({location: node.id, message: 'Dialogue belongs to another act or is missing: ' + node.dialogueId});
    }
    for (const id of flow.taskOrder || []) if (!taskIds.has(id))
      errors.push({location: flow.id, message: 'Task order references missing task ' + id});
  }
  return errors;
}
