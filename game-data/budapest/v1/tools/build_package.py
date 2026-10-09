from pathlib import Path
import json,re,hashlib,zipfile,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[1]
def save(path,obj):
 p=P/path;p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(obj,ensure_ascii=False,indent=2)+'\n')
def doc(path,text):
 p=P/path;p.parent.mkdir(parents=True,exist_ok=True);p.write_text(text.rstrip()+'\n')
status='accepted_for_design'
acts=[
 dict(slug='00-signal',title='Порука која није требало да стигне',wp='BP_START',minutes=[8,12],opening='opening',interaction='compare_versions',
 objective='Разликовати запажање разлике од закључка о истинитости.',
 instruction='Упореди две ауторске карте. Означи да се силуета појављује само на једној; сачувај обе верзије.',
 answer=['version_a','version_b'],question='Шта доказују две карте које различито приказују исто место?',choices=['Да је старија карта увек тачна','Да прикази нису исти и да треба проверити изворе','Да зграда никада није постојала'],correct=1,
 feedback='Разлика је повод за проверу, а не довољан доказ о прошлости.',sources=['SRC_USER_CORE'],history='Фикционални увод у истраживање; нема историјске тврдње о настанку града.',
 hints=['Погледај исти део обе карте.','На једној је силуета, на другој празно место.','Сачувај обе картице; не проглашавај ниједну истинитом само по изгледу.'],
 visual='Мирна туристичка карта; једна силуета нестане тек при пребацивању верзије. Три мала импулса и празан четврти слот. Без лажне системске грешке телефона.',
 reward='Прва маргина',scenes=['opening','refusal','completion']),
 dict(slug='01-embassy',title='Писмо без коверте',wp='BP_EMBASSY',minutes=[12,16],opening='opening',interaction='inspect_provenance',
 objective='Препознати порекло записа и стварни идентитет локације.',
 instruction='Отвори две дигиталне архивске картице и изабери ону са видљивим ланцем порекла. Потпис сам није довољан.',answer=['card_with_provenance'],
 question='Амбасада које државе је на овој станици, према званичној контакт-картици?',choices=['Републике Србије','Румуније','Бугарске'],correct=0,feedback='Контакт Амбасаде Републике Србије потврђује идентитет станице. То не доказује измишљену причу о тајном кругу.',sources=['SRC_EMBASSY'],history='Станица је Амбасада Републике Србије, Dózsa György út 92. Игра користи јавни прилаз; Дучићев ехо је драматизација.',
 hints=['Погледај полеђину обе картице.','Једна наводи одакле је преузета; друга има само потпис.','Изабери card_with_provenance. Упореди адресу са званичном контакт-картицом.'],
 visual='Дучић се појављује у висини човека на локалној, потврђеној равни. Мастило кратко постаје светлосна линија у маргини; фасада није прекривена гигантском пројекцијом.',reward='Коверта са пореклом',scenes=['opening','puzzle','completion']),
 dict(slug='02-heroes',title='Чувари и савршени круг',wp='BP_HEROES',minutes=[15,20],opening='opening',interaction='connect_evidence',
 objective='Одвојити стварни трг од сценске интерпретације и сачувати везе извор–тврдња.',
 instruction='Повежи три картице са ознакама: стварни локалитет, наша сценска допуна, непотврђен податак. Не бирај по боји или снази гласа.',answer=['square:location','tesla:dramatization','closed_ring:unverified'],
 question='Шта је у овој сцени стварни локалитет, а не наш дигитални додатак?',choices=['Теслин холограм','Светлосни струјни круг','Hősök tere — Трг хероја'],correct=2,feedback='Hősök tere је стварна будимпештанска локација. Холограми и борба канала припадају фикцији игре.',sources=['SRC_HEROES'],history='Hősök tere је стварни Трг хероја у Будимпешти. Приказани електрични круг и сукоб ликова су ауторски дигитални слој.',
 hints=['На свакој картици отвори ознаку порекла.','Холограм не постаје историјска чињеница зато што стоји поред споменика.','Повежи трг са стварним местом, Теслу са драматизацијом, затворени круг са непотврђеним.'],
 visual='Широки лукови преко видног поља трга, Тесла у предњем плану; Варгин правилни бели круг постепено поравнава разнолике картице. Три везе враћају појединачне ознаке. Права архитектура остаје видљива.',reward='Отворени круг',scenes=['opening','confrontation','completion']),
 dict(slug='03-central',title='Једна година вишка',wp='BP_CENTRAL',minutes=[12,18],opening='opening',interaction='compare_revision',
 objective='Исправити проверљиву историјску грешку, сачувати траг исправке и преиспитати ментора.',
 instruction='Упореди Теслину белешку са извором кафеа. Обележи погрешну годину, сачувај исправку и претходну верзију.',answer=['1886:rejected','1887:supported','revision_log:kept'],
 question='Која година отварања Centrál-а стоји у историјату самог кафеа?',choices=['1886','1887','1907'],correct=1,feedback='Centrál наводи 1887. Погрешних 1886 у претходном дијалогу је намерно подметање у фикцији, не историјски податак.',sources=['SRC_CENTRAL'],history='Centrál је отворен 1887. године. Тај проверљив податак служи да играч открије измењену менторску белешку.',
 hints=['Не бирај глас ком више верујеш; отвори извор.','Теслин запис и историјат кафеа разликују се за једну годину.','1887 је година из извора. Сачувај 1886 као одбачену верзију, уз исправку.'],
 visual='Све стаје на виртуелни сто или екран. Топао тон, два папира и траг шоље. На открићу грешке музика се гаси, без црвеног аларма. Нема глумаца преко стварних гостију.',reward='Лист исправке',scenes=['opening','crisis','challenge','trust','completion']),
 dict(slug='04-tekelijanum',title='Стране које држе град',wp='BP_TEKELIJANUM',minutes=[15,20],opening='opening',interaction='match_historical_layers',
 objective='Разликовати оснивање установе, изградњу садашње зграде и приказ на рељефу.',
 instruction='Споји 1838. са оснивањем Текелијанума, 1907–1908. са данашњом зградом, а 1907. са полагањем камена темељца приказаним на рељефу.',answer=['1838:institution','1907-1908:building','1907:relief_foundation'],
 question='На шта се односи 1838. у причи о Текелијануму?',choices=['На оснивање установе','На завршетак данашње зграде','На рушење Табанске цркве'],correct=0,feedback='1838. се односи на установу; данашња зграда подигнута је 1907–1908. Два датума не морају бити противречна ако описују различите ствари.',sources=['SRC_TEKELIJANUM'],history='Текелијанум је основан 1838. Данашња зграда настала је 1907–1908. Фасадни рељефи приказују и полагање камена темељца 1907.',
 hints=['Прочитај именицу поред сваког датума.','Установа може бити старија од зграде у којој се сада налази.','1838 → установа; 1907–1908 → зграда; 1907 → рељеф полагања камена темељца.'],
 visual='Три странице пред фасадом, повезане концем у дигиталном приказу. Без физичког отварања капије. Студентски ехо враћа бележницу; једна страна намерно остаје непопуњена.',reward='Повезана бележница',scenes=['opening','puzzle','reveal','completion']),
 dict(slug='05-taban',title='Град који није пристао да нестане',wp='BP_TABAN',minutes=[18,25],opening='opening',interaction='reconstruct_with_uncertainty',
 objective='Сачувати доказе несталог наслеђа и разликовати документовани приказ од претпостављених делова модела.',
 instruction='Разврстај три дела: видљиво на референтној фотографији, непозната геометрија ван кадра, ауторска звучна допуна. Изабери завршетак који чува ознаке и изворе.',answer=['visible_front:photo_supported','hidden_side:unverified','bell:artistic'],
 question='Које године је срушена Табанска српска црква, према наведеном тексту националне библиотеке OSZK?',choices=['1838','1887','1949'],correct=2,feedback='Извор OSZK наводи рушење 1949. Сачувана дела и документација омогућавају истраживање; наш модел није замена за њих.',sources=['SRC_TABAN'],history='Табанска српска црква срушена је 1949. OSZK описује сачуване уметнине и документе који омогућавају да њена прича остане доступна.',
 hints=['Провери који део фотографија заиста показује.','Оно што је изван кадра није потврђено само зато што модел изгледа складно.','Предњу видљиву страну означи као поткрепљену сликом, скривену као непотврђену, звук као ауторску допуну.'],
 visual='Најпре празно место и референтна слика; затим Варгин савршени модел. После избора играча израња наша слојевита реконструкција, непознати делови остају контуре. Торањ расте последњи, затим 5 секунди тишине и повратак данашњем граду.',reward='Торањ са отвореном маргином',scenes=['opening','temptation','ordeal','false_end','ending_evidence','ending_both','return','optional_knight'])]
# Dialogue text is the authoring source. Stable line IDs derive from scene and position;
# append new lines or use explicit migration if lines are inserted in a released scene.
scenes={};current=None
for line in (P/'narrative/DIALOGUE_SCRIPT.txt').read_text().splitlines():
 if line.startswith('@@ '):
  _,act,key,title=line.split(' ',3);current=f'BP_D{act}_{key}';scenes[current]={'id':current,'act':int(act),'title':title,'fictionLabel':'Ауторска драматизација; није архивски снимак или историјски цитат.','lines':[]}
 elif '|' in line and current:
  who,text=line.split('|',1);scenes[current]['lines'].append({'id':f'{current}_L{len(scenes[current]["lines"])+1:02}','speakerId':'BP_CH_'+who,'text':text,'subtitle':text,'voiceAssetId':None})
for i,a in enumerate(acts):save(f'dialogues/act-{i}.json',{'id':f'BP_DIALOGUE_{i}','language':'sr-Cyrl','scenes':[s for s in scenes.values() if s['act']==i]})
source_list=[
 ('SRC_USER_CORE','Core_Story и актови 0–5','user_supplied',None,'Наративни извор, не доказ историјских тврдњи.'),
 ('SRC_EMBASSY','Амбасада Републике Србије — контакт','official','https://budapest.mfa.gov.rs/ambasada/kontakt','Идентитет и адреса станице; 92/92b варијанта адресе захтева теренску проверу прилаза.'),
 ('SRC_HEROES','Budapestinfo — туристичка карта','official','https://www.budapestinfo.hu/storage/files/GBm6FrGupWdIt02ktNeA30WVJalrL3tVD5oUN7nV.pdf','Назив Hősök tere и локација трга; не потврђује садашње стање сваке статуе.'),
 ('SRC_CENTRAL','Centrál — историјат','institutional','https://centralgrandcafe.hu/en/central-history/','Година отварања 1887; не потврђује фикционални разговор за столом.'),
 ('SRC_TEKELIJANUM','Српски институт — Текелијанум','institutional','https://srpskiinstitut.hu/текелијанум/','1838 установа; 1907–1908 зграда; теме фасадних рељефа.'),
 ('SRC_TABAN','OSZK — Pillantás a Tabánra, Голуб Ксенија, 2019','institutional','https://nemzetikonyvtar.blog.hu/2019/03/07/pillantas_a_tabanra_az_eltunt_racvaros_emlekezete_nemzeti_konyvtarunkbol_nezve','Рушење 1949, сачувана дела и документација. Није геодетски план.'),
 ('SRC_USER_PHOTO','Корисникова фотографија Табанске цркве','user_supplied',None,'Визуелна референца у поруци; датовање, аутор, права, размера и невидљиве стране непотврђени.'),
 ('SRC_GOOGLE_GEO','Google ARCore Geospatial','official','https://developers.google.com/ar/develop/geospatial','Проверити подршку на свакој станици.'),
 ('SRC_GOOGLE_DEPTH','Google Geospatial Depth','official','https://developers.google.com/ar/develop/java/depth/geospatial-depth','Geospatial Depth зависи од VPS/Streetscape и способности уређаја.')]
save('sources.json',{'sources':[{'id':i,'title':t,'type':k,'url':u,'scope':s,'checkedAt':'2026-10-09','mediaRedistribution':'not_granted_by_citation'} for i,t,k,u,s in source_list]})
wp_data=[('BP_START','Почетак са било ког безбедног места',None,None,'not_applicable'),('BP_EMBASSY','Амбасада Републике Србије',47.51396155360761,19.077311677113567,'legacy_unity_candidate'),('BP_HEROES','Hősök tere',47.5149,19.0779,'approximate_design_candidate'),('BP_CENTRAL','Centrál, Károlyi utca 9',47.4916,19.0567,'approximate_design_candidate'),('BP_TEKELIJANUM','Текелијанум, Veres Pálné 17–19',47.4899,19.0562,'approximate_design_candidate'),('BP_TABAN','Табан — историјска парцела цркве',None,None,'survey_required')]
save('waypoints.json',{'crs':'WGS84','waypoints':[{'id':i,'name':n,'latitude':lat,'longitude':lon,'coordinateStatus':st,'fieldVerified':False,'navigationEnabled':False,'zones':[{'zone':1,'radiusMeters':90,'confirmSeconds':2},{'zone':2,'radiusMeters':35,'confirmSeconds':3},{'zone':3,'radiusMeters':25,'confirmSeconds':3}],'zoneStatus':'proposed_field_test_required','arAnchor':None,'safeStandingPoint':None,'arrivalFallback':'explicit_manual_confirmation_logged_as_manual_not_gps'} for i,n,lat,lon,st in wp_data]})
assets=[]
for i,a in enumerate(acts):
 aid=f'BP_ART_{i}'; svg=f'assets/act-{i}-motif.svg'
 # Original graphic motifs, no claim of architectural reconstruction.
 color=['#d4b477','#dfc98b','#7ed7e8','#dbaa70','#dbb4e6','#aacbb9'][i]
 (P/svg).write_text(f'<svg xmlns="http://www.w3.org/2000/svg" width="800" height="480" viewBox="0 0 800 480"><title>{a["title"]} — ауторски сценски мотив</title><rect width="800" height="480" fill="#101c28"/><g fill="none" stroke="{color}" stroke-width="3"><path d="M100 310 L270 150 L470 220 L680 100"/><circle cx="100" cy="310" r="30"/><circle cx="270" cy="150" r="30"/><circle cx="470" cy="220" r="30"/><circle cx="680" cy="100" r="30" stroke-dasharray="5 10"/></g><text x="54" y="410" font-family="sans-serif" font-size="24" fill="{color}">CHRONO EYE · BUDAPEST · ACT {i}</text></svg>')
 assets.append({'id':aid,'type':'svg','path':svg,'status':'created_concept','rights':'Original project graphic created for this package','purpose':'Симболички мотив; није историјски цртеж ни завршен storyboard.'})
assets += [{'id':'BP_MODEL_CHURCH','type':'glb','path':None,'legacyRepositoryPath':'Assets/Model/Cathedral.FBX','legacyBlobSha':'9ca3b6951cf1c4a014fa9b3e28e4e13a021aa1b5','status':'integration_pending','identityStatus':'candidate_not_visually_verified_as_taban','rights':'owner_and_license_review_required','requiredWork':['inspect_geometry_against_user_reference','confirm_scale_orientation_period','export_glb_lods','label_hypothetical_geometry']},
 {'id':'BP_MODEL_TESLA','type':'glb','path':None,'legacyRepositoryPath':'Assets/MATERIJALI/Likovi 3D  FBX models/Nikola Tesla.fbx','status':'integration_pending','rights':'owner_and_license_review_required'},
 {'id':'BP_MODEL_DUCIC','type':'glb','path':None,'legacyRepositoryPath':'Assets/MATERIJALI/Likovi 3D  FBX models/jovan_ducic_ghost.fbx','status':'integration_pending','rights':'owner_and_license_review_required'},
 {'id':'BP_PHOTO_TABAN','type':'webp','path':None,'status':'planned_not_created','sourceId':'SRC_USER_PHOTO','rights':'reference_only_public_redistribution_unverified'},
 {'id':'BP_VOICE','type':'ogg','path':None,'status':'script_ready_audio_pending','rights':'commission_original_performances_no_authentic_voice_claim'},
 {'id':'BP_SFX','type':'ogg','path':None,'status':'planned_not_created','rights':'original_or_licensed_production_required'}]
save('assets/index.json',{'assets':assets})
hints=[];timeline=[];triggers=[];collectibles=[];manifest_m=[];productions=[]
for i,a in enumerate(acts):
 mid=f'budapest-act-{i}'; prefix=f'BP_A{i}'; tasks=[]
 def task(suffix,typ,title,instruction,**extra):
  t={'id':prefix+'_'+suffix,'type':typ,'title':title,'instruction':instruction,'active':True,'requiredForMission':True,'completion':{'title':'Траг сачуван','narrative':a['feedback'] if suffix=='QUIZ' else 'Запис је додат у локални досије. Настави када желиш.','delaySeconds':0},**extra};tasks.append(t);return t
 if i:
  task('ARRIVE','gps','Стигни и стани','Потврди безбедно место у јавном простору; ако GPS није употребљив, изабери ручну потврду.',gps={'waypointId':a['wp'],'requiredZone':2},arrivalMode='gps_or_explicit_manual',waypointId=a['wp'])
 else:
  task('START','multiple_choice','Сачувај отворено питање','Одговори пре него што изабереш верзију.',question={'text':'Када се два записа разликују, који је добар први корак?','choices':['Избрисати старији','Сачувати оба и проверити порекло'],'correctIndex':1})
 t=task('EXPLORE','ar_interaction',a['title'],a['instruction'],ar={'interaction':a['interaction'],'activationAfterTask':tasks[0]['id'],'assetIds':[f'BP_ART_{i}'],'mode':'relative_to_camera' if i in [0,3] else 'guided_photo_overlay','successCondition':{'allEvidenceMappings':a['answer']},'automaticRecognition':False},fallback={'mode':'2d_evidence_cards','sameEducationalObjective':True,'scoringEquivalent':True,'successCondition':{'allEvidenceMappings':a['answer']}},implementationStatus='editorially_active_assets_pending')
 task('QUIZ','multiple_choice','Провери закључак','Користи досије и извор наведен уз картицу.',question={'text':a['question'],'choices':a['choices'],'correctIndex':a['correct']},sources=a['sources'])
 save(f'missions/act-{i}.json',{'id':mid,'title':a['title'],'primaryWaypointId':a['wp'],'language':'sr-Cyrl','reviewStatus':status,'actIndex':i,'prerequisiteMissionIds':[] if i==0 else [f'budapest-act-{i-1}'],'tasks':tasks,'narrative':{'setting':a['title'],'historyParagraphs':[a['history']],'mentorOpening':f'BP_D{i}_opening','mentorCompletion':f'BP_D{i}_completion' if i<5 else 'BP_D5_return','educationalObjective':a['objective'],'sources':a['sources']},'runtimeStatus':'not_integrated'})
 for tier,txt in enumerate(a['hints'],1):hints.append({'id':f'{prefix}_H{tier}','taskId':prefix+'_EXPLORE','tier':tier,'text':txt,'cost':0,'availability':'on_request','autoOfferAfterIdleSeconds':60 if tier==1 else None})
 # Story flow explicit nodes; dialogues are blocking until acknowledged/skipped;
 # skip preserves critical plot notes. Choices never count as quiz errors.
 nodes=[]
 def node(key,kind,next=None,**kw):nodes.append({'id':prefix+'_'+key,'kind':kind,'next':prefix+'_'+next if next else None,**kw})
 node('ENTRY','dialogue','ARRIVAL',dialogueId=f'BP_D{i}_opening')
 node('ARRIVAL','task','PREPARE',taskId=tasks[0]['id'])
 pre={0:None,1:'puzzle',2:'confrontation',3:'crisis',4:'puzzle',5:'temptation'}[i]
 node('PREPARE','dialogue' if pre else 'instruction','EXPLORE',**({'dialogueId':f'BP_D{i}_{pre}'} if pre else {'text':a['instruction']}))
 node('EXPLORE','task','QUIZ',taskId=prefix+'_EXPLORE')
 node('QUIZ','task','POST',taskId=prefix+'_QUIZ')
 if i==3:
  node('POST','choice',choices=[{'id':'challenge_mentor','label':'Покажи ми извор убудуће','setFlag':{'mentorChallenged':True},'next':prefix+'_CHALLENGE'},{'id':'continue_with_checks','label':'Настављамо уз проверу','setFlag':{'mentorChallenged':False},'next':prefix+'_TRUST'}])
  node('CHALLENGE','dialogue','CLOSE',dialogueId='BP_D3_challenge');node('TRUST','dialogue','CLOSE',dialogueId='BP_D3_trust')
 elif i==4:
  node('POST','dialogue','CLOSE',dialogueId='BP_D4_reveal')
 elif i==5:
  node('POST','dialogue','DECIDE',dialogueId='BP_D5_ordeal',setFlag={'mentorChannel':'offline'})
  node('DECIDE','choice',choices=[{'id':'evidence','label':'Сачувај доказе и означи непознато','setFlag':{'ending':'evidence'},'next':prefix+'_EVIDENCE'},{'id':'both','label':'Сачувај обе означене верзије','setFlag':{'ending':'both'},'next':prefix+'_BOTH'},{'id':'perfect','label':'Прихвати савршену верзију као истину','next':prefix+'_FALSE'}])
  node('FALSE','dialogue','DECIDE',dialogueId='BP_D5_false_end')
  node('EVIDENCE','dialogue','CLOSE',dialogueId='BP_D5_ending_evidence');node('BOTH','dialogue','CLOSE',dialogueId='BP_D5_ending_both')
 else:node('POST','checkpoint','CLOSE')
 node('CLOSE','dialogue','FINISH',dialogueId=f'BP_D{i}_completion' if i<5 else 'BP_D5_return')
 node('FINISH','mission_completion',requiresAllTaskIds=[x['id'] for x in tasks],awardKey=mid+':complete',collectibleId=f'BP_RELIC_{i}',emits='MISSION_COMPLETED',nextMissionId=f'budapest-act-{i+1}' if i<5 else None)
 # Starting exposition appears only once safe arrival is confirmed in field mode.
 nodes[0]['requiresSafeStationary']=True
 flow={'id':prefix+'_FLOW','missionId':mid,'entryNodeId':prefix+'_ENTRY','nodes':nodes,'taskOrder':[t['id'] for t in tasks],'dialoguePolicy':{'skipAllowed':True,'skipWritesPlotSummary':True,'transcriptAlwaysAvailable':True,'completionWaitsForFlowFinish':True},'resumePolicy':'resume_current_node; do_not_replay_awards','optionalSceneIds':['BP_D0_refusal'] if i==0 else ['BP_D5_optional_knight'] if i==5 else [],'fallback':'2d_evidence_cards','status':'data_contract_candidate_not_runtime'}
 slug=a['slug'];prod=f'production/{slug}'
 save(f'{prod}/scene-flow.json',flow)
 aset=[f'BP_ART_{i}','BP_VOICE','BP_SFX']+(['BP_MODEL_DUCIC'] if i==1 else ['BP_MODEL_TESLA'] if i==2 else ['BP_MODEL_CHURCH','BP_PHOTO_TABAN'] if i==5 else [])
 save(f'{prod}/asset-manifest.json',{'missionId':mid,'assetRegistry':'assets/index.json','assetIds':aset,'textAssets':[f'dialogues/act-{i}.json',f'missions/act-{i}.json'],'missingAssetsDoNotBlockTextReview':True,'publicArReleaseBlocked':True})
 brief=f'''# Акт {i} — {a['title']}

Статус: accepted_for_design. Повезано: missions/act-{i}.json, dialogues/act-{i}.json, овај scene-flow.json. Процена активног трајања: {a['minutes'][0]}–{a['minutes'][1]} минута, без трансфера. Циљна публика: 12+ и одрасли; млађи са пратиоцем.

## Место и образовни циљ

{a['history']}

Извори: {', '.join(a['sources'])}, пуни опсег у sources.json. Циљ: {a['objective']}

## Сцена и радња

{a['visual']}

Играч ради: {a['instruction']}

Тачно решење интеракције: {', '.join(a['answer'])}. Нетачна веза остаје на екрану уз објашњење; нема губитка предмета. 2D приказ даје исте картице и исти услов успеха. Нема дугмета „AR успешно“ које би заменило стварно решавање.

## Провера знања и завршетак

{a['question']}

Одговор: {a['choices'][a['correct']]}. Објашњење: {a['feedback']}

Главни предмет: {a['reward']}; без додатних поена преко мисијског бонуса. Завршетак чека FINISH чвор, не само последњи квиз. Посебно у V акту одлука и последица долазе пре награде.

## Хинтови и приступачност

1. {a['hints'][0]}
2. {a['hints'][1]}
3. {a['hints'][2]}

Сви су бесплатни, на захтев, доступни као текст. Титл не зависи од звука. Покрет и хаптика могу се искључити. Застој нуди помоћ, не казну. Избор карактера није оцењиван као тачан/погрешан.

## Простор и продукциона граница

Waypoint: {a['wp']}; само предложене зоне у waypoints.json, без теренске потврде. Нема обавезе уласка у зграду, куповине, скенирања туђег лица или додиривања споменика. Јавна безбедна тачка стајања и AR сидро морају бити засебно снимљени. Стани пре AR-а; прелаз између станица је обичан навигациони режим.

Сценарио и оригинални SVG мотив постоје. GLB/глас/геопросторне поставке нису готови. Велики призор је продукциона спецификација у narrative/VISUAL_DIRECTION.md; овај commit није испорука финалне анимације.
'''
 doc(f'{prod}/PRODUCTION_BRIEF.md',brief)
 doc(f'{prod}/QA_CHECKLIST.md',f'''# QA — {a['title']}

- [ ] Уредник одобрио текст и све изворе; драматизација се видљиво разликује од чињеница.
- [ ] Статичка провера пакета пролази; видети ../../QA_REPORT.md за стварни резултат.
- [ ] На телефону прођени сваки чвор и поновни улазак из background-а.
- [ ] Сви хинтови доступни; нетачан одговор има feedback и поновни покушај.
- [ ] AR и 2D решавају исти задатак и додељују исте поене само једном.
- [ ] Прескакање говора чува сажетак; награда не прескаче завршни избор.
- [ ] Камера/GPS одбијени: 2D и ручна потврда завршавају мисију.
- [ ] Ручна потврда није у логу названа GPS потврдом.
- [ ] Положај, светло, гужва, натписи, приступачност и дозвољен јавни прилаз проверени на терену.
- [ ] Модели, права, звук и одсуство сенсорног преоптерећења проверени.
- [ ] Албум видљив; поновљени догађај, reload и replay не дуплирају поене.

Теренски тест: датум / уређај / APK / тачка / светло / исход / screenshot / log. Тренутно НИЈЕ ИЗВРШЕН.
''')
 timeline.append({'id':prefix+'_BLOCK','missionId':mid,'flowPath':f'{prod}/scene-flow.json','entryNodeId':prefix+'_ENTRY'})
 triggers.append({'id':prefix+'_TRIGGER','type':'event','event':'CAMPAIGN_STARTED' if i==0 else 'MISSION_COMPLETED','targetBlockId':prefix+'_BLOCK','conditions':[] if i==0 else [{'field':'missionId','equals':f'budapest-act-{i-1}'}],'conditionMode':'all','onlyOnce':True})
 collectibles.append({'id':f'BP_RELIC_{i}','category':'relic','title':a['reward'],'assetId':f'BP_ART_{i}','storyByte':a['feedback'],'sourceIds':a['sources'],'unlockCondition':{'event':'MISSION_COMPLETED','missionId':mid},'scoring':{'points':0,'awardKey':f'BP_RELIC_{i}'},'spawnRules':{'mode':'album','requiresPhysicalMovement':False},'repeatability':'once_per_profile'})
 manifest_m.append({'id':mid,'path':f'missions/act-{i}.json','primaryWaypointId':a['wp']})
 productions.append({'id':prefix+'_PRODUCTION','missionId':mid,'brief':f'{prod}/PRODUCTION_BRIEF.md','scene':f'{prod}/scene-flow.json','assets':f'{prod}/asset-manifest.json','qa':f'{prod}/QA_CHECKLIST.md'})
save('hints.json',{'hints':hints});save('timeline.json',{'blocks':timeline});save('triggers.json',{'triggers':triggers})
save('systems/collectibles.json',{'categories':['relic','golden_seal','heart','joker'],'items':collectibles,'optionalCategoriesStatus':{'golden_seal':'not_used_in_v1_to_preserve_pacing','heart':'not_used_in_v1','joker':'humour_in_dialogue_not_pickups'}})
save('systems/album.json',{'id':'BP_ALBUM','title':'Досије отвореног града','button':'Мој досије','alwaysAvailable':True,'modes':['2d','ar'],'sections':['Трагови','Исправке','Извори','Отворена питања'],'itemIds':[x['id'] for x in collectibles],'cardFields':['title','storyByte','sourceIds','firstDiscoveredAt','locked','scoring'],'privateNote':'optional_device_local_never_in_public_git'})
chars={'SYSTEM':('Систем','interface'),'PLAYER':('Инспектор','player'),'MIRA':('Мира','fictional_archivist'),'UNKNOWN':('Непознат сигнал','fictional_signal'),'TESLA':('Никола Тесла','dramatized_mentor'),'DUCIC':('Јован Дучић','dramatized_echo'),'AUREL':('Аурел Варга','fictional_antagonist'),'ILO':('Ило','fictional_cafe_echo'),'STUDENT':('Студентски ехо','fictional_composite'),'STEFAN':('Стефан Лазаревић','optional_literary_echo')}
save('systems/characters.json',{'characters':[{'id':'BP_CH_'+k,'name':n,'role':r,'availableIn':[f'budapest-act-{i}' for i in range(6) if any(l['speakerId']=='BP_CH_'+k for s in scenes.values() if s['act']==i for l in s['lines'])],'appearance':{'trigger':'scripted_dialogue_or_manual_recall'},'modes':['text','2d','ar_when_asset_ready'],'modelAsset':'BP_MODEL_TESLA' if k=='TESLA' else 'BP_MODEL_DUCIC' if k=='DUCIC' else None,'modelStatus':'integration_pending' if k in ['TESLA','DUCIC'] else 'planned_not_created','animationStates':['appear','idle','speak','point','vanish'],'captionsRequired':True,'safety':'stationary_confirmed_surface_or_2d','historicalVoiceClaim':False} for k,(n,r) in chars.items()]})
save('mechanics.json',{'taskPoints':10,'missionCompletionPoints':30,'wrongQuizAnswerPoints':-2,'scoreFloor':0,'hintsCost':0,'arFallbackScoreEqual':True,'narrativeChoicePenalty':0,'relicBonusPoints':0,'idempotencyKey':'gameId:contentVersion:taskOrMissionId:awardType','replayAwards':False,'requiredState':['completedTaskIds','completedMissionIds','firedTriggerIds','scoreAwardsByKey','unlockedCollectibleIds','currentFlowNodeId','storyFlags','savedAt'],'timers':{'travel':'none','dialogue':'user_paced','hints':'offer_only'},'flowAuthority':'Only FINISH emits MISSION_COMPLETED; task completion alone cannot finish act','manualArrival':'allowed_and_logged','contentVersionMigration':'see DATA_CONTRACT.md'})
save('manifest.json',{'schemaVersion':'1.2.0-budapest-draft.1','baseSchemaVersion':'1.1.0','contentVersion':'1.0.0','gameId':'chrono-eye-budapest','name':'Град који није пристао да нестане','language':'sr-Cyrl','runtimeTargets':['web','android','arcore'],'reviewStatus':status,'runtimeStatus':'not_integrated','releaseStatus':'blocked_pending_field_assets_editorial_qa','missions':manifest_m,'waypoints':'waypoints.json','dialogues':[{'id':f'BP_DIALOGUE_{i}','path':f'dialogues/act-{i}.json'} for i in range(6)],'hints':'hints.json','timeline':'timeline.json','triggers':'triggers.json','mechanics':'mechanics.json','sources':'sources.json','systems':{x:f'systems/{x}.json' for x in ['characters','collectibles','album']},'productionPackages':productions,'requiredCapabilities':['budapest_scene_flow_v1','evidence_mapping_v1','manual_arrival_v1'],'assets':'assets/index.json','contentInventory':{'acts':6,'tasks':18,'arInteractions':6,'dialogueScenes':len(scenes),'dialogueLines':sum(len(s['lines']) for s in scenes.values()),'hints':len(hints),'relics':6}})
print('Built',len(scenes),'scenes;',sum(len(s['lines']) for s in scenes.values()),'dialogue lines')
# Typed evidence inputs: UI displays these, not the correct mapping tokens.
evidence=[
 {'act':0,'cards':[{'id':'version_a','text':'Ауторска карта A: на означеном месту видљива је силуета зграде.','kind':'fictional_training_card'},{'id':'version_b','text':'Ауторска карта B: на истом месту нема силуете.','kind':'fictional_training_card'}],'options':['Сачувај обе','Избриши карту A','Избриши карту B'],'submitMode':'select_both_ids'},
 {'act':1,'cards':[{'id':'card_with_provenance','text':'Копија поруке: „Остави место следећем читаоцу.“ Порекло: ауторски досије CHRONO EYE → картица I → ова копија.','kind':'fictional_training_card'},{'id':'card_signature_only','text':'Иста порука. Потпис: Д. Порекло копије није наведено.','kind':'fictional_training_card'}],'options':['Картица са ланцем порекла','Картица само са потписом'],'submitMode':'select_card_id'},
 {'act':2,'cards':[{'id':'square','text':'Hősök tere — Трг хероја. Извор: званична туристичка карта Budapestinfo.','sourceId':'SRC_HEROES'},{'id':'tesla','text':'Теслин холограм у овој игри. Ауторски лик; није историјски снимак.','sourceId':'SRC_USER_CORE'},{'id':'closed_ring','text':'„Затварањем круга сви записи постају истинити.“ Порекло: ПАЛИМПСЕСТ; независна потврда није приложена.','kind':'fictional_claim'}],'targets':{'location':'Стварни локалитет','dramatization':'Сценска допуна','unverified':'Непотврђена тврдња'},'submitMode':'map_card_to_target'},
 {'act':3,'cards':[{'id':'1886','text':'Менторска белешка са Трга: „1886.“ Ознака: непроверено.','kind':'fictional_corrupted_note'},{'id':'1887','text':'Историјат Centrál-а: кафе отворен 1887.','sourceId':'SRC_CENTRAL'},{'id':'revision_log','text':'Дневник: претходна белешка 1886; исправка на 1887 са извором.','kind':'player_revision'}],'targets':{'rejected':'Одбачена тврдња','supported':'Подржано наведеним извором','kept':'Сачувати уз исправку'},'submitMode':'map_card_to_target'},
 {'act':4,'cards':[{'id':'1838','text':'1838','sourceId':'SRC_TEKELIJANUM'},{'id':'1907-1908','text':'1907–1908','sourceId':'SRC_TEKELIJANUM'},{'id':'1907','text':'1907, мотив на рељефу','sourceId':'SRC_TEKELIJANUM'}],'targets':{'institution':'Оснивање установе','building':'Данашња зграда','relief_foundation':'Полагање камена темељца приказано на рељефу'},'submitMode':'map_card_to_target'},
 {'act':5,'cards':[{'id':'visible_front','text':'Облик прочеља видљив на приложеној референтној слици; размера није потврђена.','sourceId':'SRC_USER_PHOTO'},{'id':'hidden_side','text':'Страна коју та слика не приказује; допуњена према визуелној претпоставци.','sourceId':'SRC_USER_PHOTO'},{'id':'bell','text':'Нови звучни ефекат звона направљен за сцену. Није историјски снимак.','kind':'artistic_audio'}],'targets':{'photo_supported':'Видљиви облик поткрепљен сликом','unverified':'Непотврђена геометрија','artistic':'Уметничка допуна'},'submitMode':'map_card_to_target'}]
save('evidence.json',{'sets':[{'id':f'BP_EVIDENCE_{e["act"]}',**e} for e in evidence],'historyCards':[{'id':f'BP_HISTORY_{i}','text':a['history'],'sourceIds':a['sources']} for i,a in enumerate(acts)],'finaleImagePolicy':'Text-only desk review can use descriptions; public visual release waits for licensed reference image.'})
for i in range(6):
 p=f'missions/act-{i}.json';m=json.loads((P/p).read_text());m['tasks'][1]['evidenceSetId']=f'BP_EVIDENCE_{i}';m['tasks'][2]['historyCardId']=f'BP_HISTORY_{i}'
 m['tasks'][2]['question']['wrongAnswerFeedback']='Погледај историјску картицу и њен извор. Можеш поново одговорити; хинт је бесплатан.'
 if i==5:m['tasks'][1]['ar']['assetIds']+=['BP_PHOTO_TABAN','BP_MODEL_CHURCH']
 save(p,m)
 # Arrival before scene opening; the opening remains before subsequent puzzles.
 p=f'production/{acts[i]["slug"]}/scene-flow.json';f=json.loads((P/p).read_text())
 if i:
  f['entryNodeId']=f'BP_A{i}_ARRIVAL'
  for n in f['nodes']:
   if n['id'].endswith('_ARRIVAL'):n['next']=f'BP_A{i}_ENTRY'
   if n['id'].endswith('_ENTRY'):n['next']=f'BP_A{i}_PREPARE'
 for n in f['nodes']:
  if n['kind']=='dialogue':
   n['skipSummary']={'0':'Сачувај две различите карте; непознат сигнал оставља четврто место празним.','1':'Картица има проверљиво порекло; Дучић и Тесла су драматизовани ликови.','2':'Напад на приказ је одбијен; непроверена менторска белешка наводи 1886 за Centrál.','3':'Извор наводи 1887. Мира је писала модул допуњавања, Теслина белешка била је измењена.','4':'Оснивање установе и нова зграда имају различите датуме. Празан четврти слот означава непознато.','5':'Противник нуди савршену слику без извора. Сачувај доказе и означи претпоставке; локална копија остаје.'}[str(i)]
 f['launchPolicy']='unlock_after_trigger; start_only_on_user_continue; pause_during_travel'
 if i==5:
  f['endingCallbacks']=[{'condition':{'mentorChallenged':True},'speakerId':'BP_CH_TESLA','text':'Оно питање у кафани било је почетак овога.'},{'condition':{'mentorChallenged':False},'speakerId':'BP_CH_TESLA','text':'Сада знаш и које питање треба поставити мени.'}]
 save(p,f)
m=json.loads((P/'mechanics.json').read_text());m['idempotencyKey']='gameId:taskOrMissionId:awardType';m['requiredState']+=['processedAttemptIds','evidenceSelections'];save('mechanics.json',m)
m=json.loads((P/'manifest.json').read_text());m['evidence']='evidence.json';m['signals']='signals.json';save('manifest.json',m)
save('signals.json',{'signals':[{'id':'BP_SIGNAL_OPEN_SLOT','pattern':'three_pulses_then_empty_slot','durationMs':1800,'isHistoricalCode':False,'audioAssetId':'BP_SFX','textEquivalent':'Три кратка импулса. Четврто место остаје празно.','instances':[{'sceneId':s,'purpose':p} for s,p in [('BP_D0_opening','mystery'),('BP_D1_puzzle','reinforcement'),('BP_D2_confrontation','antagonist_closes_gap'),('BP_D4_reveal','payoff')]],'replay':'manual_only','captionRequired':True}]})
# Separate task IDs from flow-node IDs so cross-file references are unambiguous.
for i,a in enumerate(acts):
 p=f'production/{a["slug"]}/scene-flow.json';f=json.loads((P/p).read_text());mapping={n['id']:n['id'].replace(f'BP_A{i}_',f'BP_A{i}_NODE_',1) for n in f['nodes']}
 f['entryNodeId']=mapping[f['entryNodeId']]
 for n in f['nodes']:
  n['id']=mapping[n['id']]
  if n.get('next'):n['next']=mapping[n['next']]
  for c in n.get('choices',[]):c['next']=mapping[c['next']]
 save(p,f)
 p='timeline.json';tl=json.loads((P/p).read_text());tl['blocks'][i]['entryNodeId']=f['entryNodeId'];save(p,tl)
