# CHRONO EYE — шаблон продукционог пакета за нову мисију v0.2

**Користи уз:** [MISSION_STANDARD_v0.2.md](MISSION_STANDARD_v0.2.md) · [DECISION_LOG.md](DECISION_LOG.md).  
**Референтан пример:** Сомбор, Градска кућа — [PRODUCTION_BRIEF.md](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/blob/content/sombor-portable-game-data-20261008/game-data/sombor/v1/production/gradska-kuca/PRODUCTION_BRIEF.md).

Копирај овај списак у `game-data/<city-id>/v<major>/production/<mission-slug>/PRODUCTION_BRIEF.md` и приложи `scene-flow.json`, `asset-manifest.json`, `QA_CHECKLIST.md`. Сви непознати подаци морају бити `pending` — не измишљати потврђену локацију, текст, 3D модел, лиценцу, аудио или тест.

## A. Идентификација / статус

- `gameId`: [попуни]
- `missionId`: [попуни]
- `primaryWaypointId`: [попуни]
- Стабилни `taskIds[]`: [попуни]
- `editorialStatus`: draft / reviewed / approved_for_design
- `implementationStatus`: planned / assets_partial / implemented_not_tested / verified_in_test
- `contentVersion`: [попуни]
- Датум, уредник, Git branch/commit, извор старе верзије ако постоји

## B. Наратив и стварно историјско знање

- [ ] Место и историјски контекст, **извор/ауторска права за сваку важну тврдњу**
- [ ] Историјски увод, језички узраст, порука „зашто овде“
- [ ] Шта играч реално види у простору — на шта обраћа пажњу
- [ ] Прави едукативни задатак после туторијалског/AR корака
- [ ] `question` / `expectedAnswer` / одобрене алтернативе / помоћ
- [ ] Објашњење тачног одговора и завршни наратив
- [ ] Временски осетљиве чињенице имају датум провере
- [ ] Разлике у изворима или спорне чињенице видљиво обележене
- [ ] Сачуван `legacyOriginal` ако је коригован стари садржај

## C. Gameplay петља (упиши редослед и све услове)

| Корак | Task/event ID | Окидач | Промена стања | UI/аудио/AR | Fallback |
|---|---|---|---|---|---|
| GPS долазак | [ID] | WGS84 Zone 1/2/3 и време | [state] | [текст] | [лош GPS] |
| Ментор/нарација | [ID] | корисник је стао / призив | [state] | [говор + титл] | [2D портрет] |
| AR сусрет | [ID] | после GPS-а | AR_COMPLETED | [модел/контуре] | [еквивалент 2D] |
| Историјско питање | [ID] | после AR-а | TASK_COMPLETED | [ABC/упис] | [приступачан UI] |
| Награда | [ID] | после успешне мисије | ITEM_UNLOCKED | [предмет] | [албум] |

Свака грана мора да води ка истом образовном исходу. Bonus collectible не сме да прескочи обавезни историјски задатак.

## D. Локација и AR сцена

- WGS84 ширина/дужина и њихов извор/време провере
- Outer, Approach, Close радијуси и `confirmSeconds` по зони; ознака `fieldVerified`
- Безбедна зона стајања/камера/прелаз коловоза, приступачност и приватни простор
- Просторни мод: `guided_photo_overlay`, `manual_surface`, `relative_to_camera`, `geospatial_anchor`, `streetscape_anchor`
- Ако је guided overlay, **не тврдити да је то аутоматско препознавање објекта**
- Механика геста/тапа/постављања; услов успеха и грешке
- Capability-check, camera permission, mute, subtitles
- 2D fallback са **истим исходом/основним бодовима**

## E. Ментори / ехо-ликови

- `characterId`, улога и стил; постоји ли глобални модел или је нов
- Текст говора, титлови, језик, глас / права
- `appearance`: ауто, на позив, по догађају; 2D/GLB статус
- Анимације: appear, idle, point, speak, vanish (ако су применљиве)
- Никада се не појављује на небезбедном месту; подлога коју одобрава играч
- Историјски лик = јасно означена реконструкција, не лажни архивски снимак

## F. Колекције / Herbarijum

- Главни `relicId` (1 јединствени по мисији), опис и историјски извор
- Опциони `golden_seal`, `heart`, `joker`: spawn ID, услов открића, story byte, права
- Бонус поени / лимит / `scoreAwardsByKey` / `pickedSpawnIds`
- Картица у видљивом албуму (слика/име/локалитет/микроприча/историјски извор)
- Bonus објекти не смеју блокирати мисију или бити на опасној физичкој локацији
- Одвојити број бодова за мисију од награде за главни предмет

## G. Asset manifest и реалност продукције

За сваки ресурс уписати:

| ID | Тип | Фајл/путања | Власник/извор/лиценца | Статус | Користи се у |
|---|---|---|---|---|---|
| [ID] | svg/webp/glb/ogg/srt | [путања] | [лиценца] | `planned_not_created` | [ID сцене] |

Дозвољени статуси: `planned_not_created`, `script_ready_audio_pending`, `created_concept`, `integration_pending`, `verified_in_test`. Само „фajл је предвиђен“ није доказ да постоји. 3D модел има различите статусе за modeling/animation/integration; звучни сценарио није снимљен звук.

## H. QA, лог и пуштање

- [ ] JSON валидан; сви ID-јеви јединствени; референце постоје
- [ ] GPS зону не условљава немогућа прецизност; не тера на коловоз
- [ ] AR и 2D fallback чувају исти образовни и scoring исход
- [ ] Quiz остаје историјски проверљив и после AR туторијала
- [ ] Ментор се може призвати, титлови доступни и без гласа
- [ ] Колекционарска награда и бодови се не дуплирају након reload-а
- [ ] Не постоје непријављена права/лиценце ни Google credentials у Git-у
- [ ] Статички резултат раздвојен од теренског Android/AR теста
- [ ] Нове одлуке у `DECISION_LOG.md`, услуге у `SERVICE_REGISTRY.md` само када стварно мењамо интеграцију
- [ ] Тестове са уређајем/датумом/доказом бележимо у `PRACTICE_LOG.md`

**Release услов:** `verified_in_test` тек после описаног теста стварног runtime-а; `accepted_for_design` и `created_concept` нису исто што и готова апликација.
