// CHRONO EYE Budapest v2 — standalone static content QA (Node 18+)
// Run from any directory: node game-data/budapest/v2/tools/validate-v2.mjs
import { readFileSync, existsSync } from "node:fs";
import { dirname, resolve, join } from "node:path";
import { fileURLToPath } from "node:url";
const ROOT=resolve(dirname(fileURLToPath(import.meta.url)),"..");
const errors=[],warnings=[];let checks=0;
const ok=(condition,message)=>{checks++;if(!condition)errors.push(message);};
const json=(path)=>JSON.parse(readFileSync(join(ROOT,path),"utf8"));
const exists=(path)=>existsSync(join(ROOT,path));
const manifest=json("manifest.json");
const waypoints=json(manifest.waypoints).waypoints;
const sources=json(manifest.sources).sources;
const assets=json(manifest.assets).assets;
const evidence=json(manifest.evidence).sets;
const histories=json(manifest.evidence).historyCards;
const characters=json(manifest.systems.characters).characters;
const collectibles=json(manifest.systems.collectibles);
const hints=json(manifest.hints).hints;
const timeline=json(manifest.timeline).blocks;
const triggers=json(manifest.triggers).triggers;
const mechanics=json(manifest.mechanics);
const signals=json(manifest.signals).signals;
const media=json(manifest.media).cues;
const byId=(arr)=>new Map(arr.map(v=>[v.id,v]));
const waypointMap=byId(waypoints),sourceMap=byId(sources),assetMap=byId(assets);
const evidenceMap=byId(evidence),characterMap=byId(characters),relicMap=byId(collectibles.items);
const blockMap=byId(timeline);
const scenario={acts:0,tasks:0,ar:0,scenes:0,lines:0,sequenceEntries:0,flowNodes:0,reachableFlowNodes:0,flowPaths:0};
const everyDistinct=(arr)=>new Set(arr).size===arr.length;
ok(manifest.missionType==="game_mission","Budapest v2 requires explicit game_mission profile");
// Remaining counts/scoring/task checks pin this authored draft, not the shared GAME contract.
ok(manifest.gameId==="chrono-eye-budapest","incorrect game id");
ok(manifest.language==="sr-Cyrl","language must be sr-Cyrl");
ok(manifest.runtimeStatus==="not_integrated","do not claim runtime integration");
ok(manifest.missions.length===7 && manifest.dialogues.length===7 && manifest.productionPackages.length===7,"seven mission/dialogue/production packages required");
ok(manifest.requiredCapabilities.includes("dialogue_sequence_id_v2"),"v2 dialogue capability not advertised");
ok(waypoints.every(w=>!w.fieldVerified && w.latitude==null && w.longitude==null),"unexpected surveyed waypoint: review navigation release policy");
ok(waypoints.length===11,"unexpected waypoint count");
ok(assets.every(a=>a.status==="planned_not_created" && !a.path),"media registry status needs manual verification");
ok(mechanics.scoring.perTask===10 && mechanics.scoring.completionBonus===30,"point schedule changed");
ok(mechanics.scoring.quizRetryPenalty===0,"retry penalty must be zero in this package");
ok(!String(mechanics.scoring.awardPolicy).includes("versioned"),"award key must not change with content version");
ok(["relic","golden_seal","heart","joker"].every(cat=>collectibles.categories.includes(cat)),"missing supported category");
ok(collectibles.items.length===7 && collectibles.items.every(x=>x.category==="relic"),"seven main relics required, no fictional bonus pickups yet");
ok(hints.length===21,"three progressive hints for seven acts");
ok(triggers.length===7 && timeline.length===7,"seven triggers and timeline blocks expected");
for(const t of triggers)ok(blockMap.has(t.targetBlockId),"unknown target timeline block "+t.targetBlockId);
for(const e of evidence){
 ok(Array.isArray(e.cards) && e.cards.length>0,"empty evidence set "+e.id);
 for(const c of e.cards)ok(sourceMap.has(c.sourceId),"unknown evidence source "+c.sourceId);
}
for(const h of histories)for(const src of h.sourceIds||[])ok(sourceMap.has(src),"unknown history source "+src);
const sceneIds=[];
for(let i=0;i<7;i++){
 const entry=manifest.missions[i],dialogueRef=manifest.dialogues[i],production=manifest.productionPackages[i];
 ok(exists(entry.path),"missing "+entry.path);
 ok(exists(dialogueRef.path),"missing "+dialogueRef.path);
 const mission=json(entry.path),dialogue=json(dialogueRef.path),flow=json(production.scene);
 scenario.acts++;
 ok(mission.id===entry.id,"mission id mismatch act "+i);
 ok(mission.primaryWaypointId===entry.primaryWaypointId,"manifest waypoint differs act "+i);
 ok(mission.runtimeStatus==="not_integrated","false runtime status act "+i);
 ok(mission.dialoguePath===dialogueRef.path,"mission dialogue link differs act "+i);
 ok(Array.isArray(mission.narrative.historyParagraphs)&&mission.narrative.historyParagraphs.length>0,"missing history paragraphs act "+i);
 ok(!!mission.narrative.educationalObjective && !!mission.narrative.fictionDisclosure,"missing narrative fields act "+i);
 for(const ref of mission.narrative.sources||[])ok(sourceMap.has(ref),"unknown narrative source "+ref+" in act "+i);
 for(const id of [mission.primaryWaypointId,...(mission.intermediateWaypointIds||[])])ok(waypointMap.has(id),"unknown waypoint "+id+" in act "+i);
 ok(evidenceMap.has(mission.evidenceSetId),"missing evidence set act "+i);
 ok(mission.tasks.length===3 && everyDistinct(mission.tasks.map(t=>t.id)),"three unique tasks act "+i);
 const types=mission.tasks.map(t=>t.type);
 ok(types.join(",")==="gps,ar_interaction,multiple_choice","mission task order act "+i);
 scenario.tasks+=mission.tasks.length;scenario.ar++;
 for(const t of mission.tasks){
  ok(!!t.completion?.narrative,"missing educational completion "+t.id);
  ok(t.requiredForMission===true,"task must be required "+t.id);
  if(t.type==="gps"){
   ok(waypointMap.has(t.waypointId)&&t.arrivalMode==="gps_or_explicit_manual","invalid GPS/manual contract "+t.id);
   ok(t.gps?.fieldVerificationRequired===true,"GPS survey guard absent "+t.id);
  }
  if(t.type==="ar_interaction"){
   ok(t.ar?.interaction==="evidence_mapping","unsupported AR mapping "+t.id);
   ok(t.ar?.automaticRecognition===false,"misleading automatic object recognition "+t.id);
   ok(t.implementationStatus==="editorially_authored_assets_pending","misleading AR asset status "+t.id);
   ok(JSON.stringify(t.ar?.successCondition)===JSON.stringify(t.fallback?.successCondition),"AR/2D success condition drift "+t.id);
   ok(t.fallback?.sameEducationalObjective===true&&t.fallback?.scoringEquivalent===true,"AR/2D scoring or learning drift "+t.id);
   for(const id of t.ar?.assetIds||[])ok(assetMap.has(id),"missing registered AR asset "+id);
  }
  if(t.type==="multiple_choice"){
   ok(Number.isInteger(t.question?.correctIndex)&&t.question.correctIndex>=0&&t.question.correctIndex<t.question.choices.length,"invalid correctIndex "+t.id);
   for(const id of t.sources||[])ok(sourceMap.has(id),"unknown quiz source "+id);
  }
 }
 ok(dialogue.id===dialogueRef.id,"dialogue manifest mismatch act "+i);
 ok(dialogue.scenes.length>0,"empty dialogue act "+i);
 const localSceneIds=dialogue.scenes.map(s=>s.id);
 ok(everyDistinct(localSceneIds),"duplicate local dialogue ids act "+i);
 for(const s of dialogue.scenes){
  sceneIds.push(s.id);scenario.scenes++;scenario.lines+=s.lines.length;
  ok(!!s.fictionLabel,"missing dramatization disclosure "+s.id);
  ok(everyDistinct(s.lines.map(x=>x.id)),"duplicate line id "+s.id);
  for(const l of s.lines)ok(characterMap.has(l.speakerId),"unknown speaker "+l.speakerId+" in "+s.id);
  const pools={line:s.lines,stage:s.stageDirections,cue:s.mediaCues,beat:s.gameplayBeats};
  for(const step of s.sequence||[]){
   scenario.sequenceEntries++;
   ok((pools[step.type]||[]).some(item=>item.id===step.id),"invalid dialogue sequence link "+s.id+" "+step.type+"/"+step.id);
  }
 }
 for(const path of [production.brief,production.scene,production.assets,production.qa])ok(exists(path),"missing production deliverable "+path);
 const productionAssets=json(production.assets);
 ok(productionAssets.missionId===mission.id,"asset manifest mission mismatch act "+i);
 const byNode=byId(flow.nodes);
 ok(byNode.has(flow.entryNodeId),"missing flow entry act "+i);
 ok(everyDistinct(flow.nodes.map(n=>n.id)),"duplicate flow node id act "+i);
 scenario.flowNodes+=flow.nodes.length;
 for(const n of flow.nodes){
  if(n.kind==="dialogue"){
   ok(localSceneIds.includes(n.dialogueId),"unlinked scene "+n.dialogueId);
   ok(!!n.skipSummary && n.skipPreservesTranscript===true,"missing reliable skip summary "+n.id);
  }
  if(n.kind==="task")ok(mission.tasks.some(t=>t.id===n.taskId),"unknown flow task "+n.taskId);
  for(const target of [n.next,...(n.choices||[]).map(c=>c.next)].filter(Boolean))ok(byNode.has(target),"dangling flow edge "+n.id+" -> "+target);
 }
 const queue=[flow.entryNodeId],visited=new Set();
 while(queue.length){
  const id=queue.shift();if(visited.has(id))continue;visited.add(id);
  const node=byNode.get(id);if(!node)continue;
  queue.push(...[node.next,...(node.choices||[]).map(c=>c.next)].filter(Boolean));
 }
 scenario.reachableFlowNodes+=visited.size;
 ok(visited.size===flow.nodes.length,"unreachable flow nodes act "+i);
 const finalNodes=flow.nodes.filter(n=>n.kind==="mission_completion");
 ok(finalNodes.length===1&&visited.has(finalNodes[0]?.id),"missing reachable completion act "+i);
 ok(JSON.stringify(finalNodes[0]?.requiresAllTaskIds)===JSON.stringify(mission.tasks.map(t=>t.id)),"completion task gate drift act "+i);
 ok(relicMap.has(finalNodes[0]?.collectibleId),"completion relic not registered act "+i);
 const walk=(id,chain)=>{
  if(chain.length>40||chain.includes(id)){errors.push("unsafe flow loop act "+i+" at "+id);return;}
  const n=byNode.get(id);if(!n){errors.push("missing flow walk node "+id);return;}
  if(n.kind==="mission_completion"){scenario.flowPaths++;return;}
  const next=[n.next,...(n.choices||[]).map(c=>c.next)].filter(Boolean);
  if(!next.length){errors.push("dead end act "+i+" at "+id);return;}
  for(const target of next)walk(target,[...chain,id]);
 };
 walk(flow.entryNodeId,[]);
}
ok(everyDistinct(sceneIds),"duplicate scene ID across acts");
for(const s of signals){
 for(const ins of s.instances||[])ok(sceneIds.includes(ins.sceneId),"signal references absent scene "+ins.sceneId);
 if(s.audioAssetId)ok(assetMap.has(s.audioAssetId),"unregistered signal asset "+s.audioAssetId);
}
for(const cue of media)if(cue.sceneId)ok(sceneIds.includes(cue.sceneId),"media cue references missing scene "+cue.sceneId);
ok(scenario.acts===manifest.contentInventory.acts,"manifest acts counter mismatch");
ok(scenario.tasks===manifest.contentInventory.tasks,"manifest task counter mismatch");
ok(scenario.ar===manifest.contentInventory.arInteractions,"manifest AR counter mismatch");
ok(scenario.scenes===manifest.contentInventory.dialogueScenes,"manifest scene counter mismatch");
ok(scenario.lines===manifest.contentInventory.dialogueLines,"manifest line counter mismatch");
ok(scenario.flowPaths>=11,"missing intended final branches");
const unverifiedWaypoints=waypoints.filter(w=>!w.fieldVerified).length;
const uncreatedAssets=assets.filter(a=>a.status==="planned_not_created").length;
warnings.push("Not device tested: all "+unverifiedWaypoints+" waypoints require a field survey; "+uncreatedAssets+" assets are planned, not binary media.");
warnings.push("Historical fact checking, media rights, ARCore, Android Host, Web Maps, GPS accuracy and save/restore remain external release gates.");
const report={status:errors.length?"FAIL":"PASS_STATIC_ONLY",checks,errors,warnings,scenario,unverifiedWaypoints,uncreatedAssets,releaseEligible:false};
console.log(JSON.stringify(report,null,2));
if(errors.length)process.exitCode=1;
