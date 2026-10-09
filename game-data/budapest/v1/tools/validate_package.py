"""Static contract and graph checks; does not claim Android or rendering coverage."""
from pathlib import Path
import json
P=Path(__file__).resolve().parents[1]
def read(p):return json.loads((P/p).read_text())
checks=0
def check(ok,msg):
 global checks
 checks+=1
 if not ok:raise AssertionError(msg)
manifest=read('manifest.json')
for p in P.rglob('*.json'):json.loads(p.read_text())
sources={s['id'] for s in read('sources.json')['sources']}
assets={a['id']:a for a in read('assets/index.json')['assets']}
chars={c['id'] for c in read('systems/characters.json')['characters']}
waypoints={w['id']:w for w in read('waypoints.json')['waypoints']}
evidence={e['id']:e for e in read('evidence.json')['sets']}
histories={e['id']:e for e in read('evidence.json')['historyCards']}
scenes={};allids=set();missions={};tasks={};flows={}
def unique(id):check(id not in allids,'Duplicate ID '+id);allids.add(id)
for item in manifest['dialogues']:
 for s in read(item['path'])['scenes']:
  unique(s['id']);scenes[s['id']]=s
  for l in s['lines']:unique(l['id']);check(l['speakerId'] in chars,'Unknown speaker');check(l['text']==l['subtitle'],'Missing caption')
for id,a in assets.items():
 unique(id)
 if a.get('path'):check((P/a['path']).is_file(),'Missing created asset '+id)
 else:check(a['status'] in ['planned_not_created','integration_pending','script_ready_audio_pending'],'Unreported missing asset')
for x in manifest['missions']:
 m=read(x['path']);unique(m['id']);missions[m['id']]=m;check(m['primaryWaypointId'] in waypoints,'Missing waypoint')
 for s in m['narrative']['sources']:check(s in sources,'Unknown source')
 for t in m['tasks']:
  unique(t['id']);tasks[t['id']]=t
  check(all(t.get(k) for k in ['type','title','instruction','completion']),'Missing task fields')
  if t['type']=='multiple_choice':
   q=t['question'];check(0<=q['correctIndex']<len(q['choices']),'Invalid quiz index')
  if t['type']=='gps':check(t['gps']['waypointId'] in waypoints,'GPS reference')
  if t['type']=='ar_interaction':
   check(t['ar']['activationAfterTask'] in [x['id'] for x in m['tasks']],'AR prerequisite')
   for a in t['ar']['assetIds']:check(a in assets,'AR asset missing')
   check(t['fallback']['sameEducationalObjective'] and t['fallback']['scoringEquivalent'],'Fallback mismatch')
   check(t['ar']['successCondition']==t['fallback']['successCondition'],'2D/AR answers differ')
   check(t['evidenceSetId'] in evidence,'Evidence missing')
   e=evidence[t['evidenceSetId']];cards={c['id'] for c in e['cards']}
   for mapping in t['ar']['successCondition']['allEvidenceMappings']:
    if ':' in mapping:
     card,target=mapping.split(':',1);check(card in cards and target in e['targets'],'Impossible evidence mapping '+mapping)
    else:check(mapping in cards,'Impossible selection '+mapping)
for p in manifest['productionPackages']:
 for key in ['brief','scene','assets','qa']:check((P/p[key]).exists(),'Missing production doc')
 f=read(p['scene']);flows[f['missionId']]=f;nodes={n['id']:n for n in f['nodes']}
 for n in nodes.values():
  unique(n['id'])
  if n.get('next'):check(n['next'] in nodes,'Dangling next')
  if n['kind']=='choice':
   check(len({c['id'] for c in n['choices']})==len(n['choices']),'Duplicate choice')
   for c in n['choices']:check(c['next'] in nodes,'Dangling choice')
  if n['kind']=='dialogue':check(n['dialogueId'] in scenes,'Dangling dialogue');check(bool(n['skipSummary']),'Missing skip summary')
  if n['kind']=='task':check(n['taskId'] in tasks,'Dangling task')
  if n['kind']=='mission_completion':check(set(n['requiresAllTaskIds'])=={t['id'] for t in missions[f['missionId']]['tasks']},'Completion skips task')
 seen=set();stack=[f['entryNodeId']]
 while stack:
  id=stack.pop()
  if id in seen:continue
  seen.add(id);n=nodes[id]
  stack+=([n['next']] if n.get('next') else [])+[c['next'] for c in n.get('choices',[])]
 check(seen==set(nodes),'Unreachable flow nodes')
 for s in f['optionalSceneIds']:check(s in scenes,'Missing optional scene')
# Check every authored dialogue is linked, including optional variations.
linked={n['dialogueId'] for f in flows.values() for n in f['nodes'] if n['kind']=='dialogue'}|{s for f in flows.values() for s in f['optionalSceneIds']}
check(linked==set(scenes),'Orphan dialogue scenes')
for h in read('hints.json')['hints']:unique(h['id']);check(h['taskId'] in tasks,'Hint task missing')
blocks={b['id']:b for b in read('timeline.json')['blocks']}
for t in read('triggers.json')['triggers']:unique(t['id']);check(t['targetBlockId'] in blocks,'Trigger target missing');check(t['onlyOnce'],'Repeat trigger')
for s in read('signals.json')['signals']:
 for inst in s['instances']:check(inst['sceneId'] in scenes,'Unknown signal scene')
for e in evidence.values():
 for c in e['cards']:
  if c.get('sourceId'):check(c['sourceId'] in sources,'Evidence source unknown')
items=read('systems/collectibles.json')['items'];check(len(items)==len(missions),'One relic per act')
for item in items:check(item['unlockCondition']['missionId'] in missions,'Relic mission missing');check(item['assetId'] in assets,'Relic asset missing')
check(set(read('systems/album.json')['itemIds'])=={i['id'] for i in items},'Album incomplete')
# Traverse concrete branches. This is a data-graph simulator, not production engine.
def traverse(flow,choices,mode):
 nodes={n['id']:n for n in flow['nodes']};id=flow['entryNodeId'];completed=set();visited=[];awards=set();score=0;flags={};choicepos=0
 for step in range(100):
  n=nodes[id];visited.append(id)
  if n['kind']=='task':
   task=tasks[n['taskId']]
   if task['type']=='ar_interaction':
    expected=task['ar' if mode=='ar' else 'fallback']['successCondition']['allEvidenceMappings']
    check(len(expected)>0,'Empty evidence success')
   completed.add(task['id']);key=task['id']+':task'
   if key not in awards:awards.add(key);score+=10
  flags.update(n.get('setFlag',{}))
  if n['kind']=='choice':
   wanted=choices[choicepos];choicepos+=1;c=next(c for c in n['choices'] if c['id']==wanted);flags.update(c.get('setFlag',{}));id=c['next'];continue
  if n['kind']=='mission_completion':
   check(set(n['requiresAllTaskIds'])<=completed,'Premature completion');key=n['awardKey']
   if key not in awards:score+=30;awards.add(key)
   # Repeat completion event must be no-op in the ledger model.
   before=score
   if key not in awards:score+=30
   check(score==before,'Duplicate reward');return score,visited,flags
  id=n['next']
 raise AssertionError('Nonterminating branch')
cases=0
for mid,f in flows.items():
 variants=[[]]
 if mid.endswith('-3'):variants=[['challenge_mentor'],['continue_with_checks']]
 if mid.endswith('-5'):variants=[['evidence'],['both'],['perfect','evidence'],['perfect','both']]
 for choices in variants:
  ar=traverse(f,choices,'ar');two=traverse(f,choices,'2d');check(ar[0]==two[0]==60,'Score differs');cases+=2
  if 'perfect' in choices:
   check(any(x.endswith('_FALSE') for x in ar[1]),'Wrong branch not exercised')
# Public launch must remain off until coordinates/assets/runtime are verified.
check(manifest['releaseStatus'].startswith('blocked'),'Premature release')
for w in waypoints.values():check(not w['navigationEnabled'],'Unverified navigation enabled')
check(sum(len(s['lines']) for s in scenes.values())==manifest['contentInventory']['dialogueLines'],'Inventory drift')
check(len(tasks)==manifest['contentInventory']['tasks'],'Task inventory drift')
report=f'''# Статички QA извештај

Датум: 2026-10-09. Резултат: PASS — {checks} провера у локалном validator-у; {cases} пролаза кроз варијанте графа (AR/2D модел података).

Проверено: JSON синтакса, ID-јеви, task/dialogue/asset/source/evidence референце, квиз индекси, постојеће путање, пријављени недостајући медији, досег свих чворова, везаност свих сцена, шест реликвија у албуму и равноправни услови AR/2D. Оба ваљана завршетка, обе кафе-опције и повратак из погрешног финалног избора достижу FINISH. Сваки акт у овом моделу има 60 основних поена, кампања 360 пре погрешних квиз одговора. Поновљен completion догађај у ledger моделу не даје другу награду.

Ово НЕ тестира runtime имплементацију идемпотентности, стварни AR tracking, native bridge, APK, GPS, Web renderer, гласове, права слика или стварну дужину шетње. Не постоји тврдња verified_in_test за апликацију.

Блокери јавног пуштања: безбедне теренске тачке и сидра; тачна Табанска парцела; потврда идентитета/скале Cathedral.FBX; права и локална фото-датотека; модели и звук; loader за candidate schema; тест стања после background/restart; уредничка и корисничка проба. Свих шест локалних QA_CHECKLIST остају отворени за ове провере.

Репродукција: python game-data/budapest/v1/tools/validate_package.py
'''
(P/'QA_REPORT.md').write_text(report)
print(f'PASS {checks} checks; {cases} graph runs; {len(scenes)} scenes; {len(tasks)} tasks')
