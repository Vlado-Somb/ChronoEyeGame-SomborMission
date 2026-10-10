# CHRONO EYE — 01 Repository Stabilization

Провера: 10. октобар 2026. Репозиторијум: [Vlado-Somb/ChronoEyeGame-SomborMission](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission).

**Допуна — Connector First:** 2026-10-10 поновни get_repo, create_branch и create_tree позиви су успели преко GitHub App. Претходна блокада аргумената није доказ недостатка дозвола. Објављивање кандидата је настављено конектором, без браузера. ChronoEye-Legacy је потврђен као private (repo ID 1413515532). Почетни аудит испод остаје историјски snapshot; актуелне remote SHA/PR/CI резултате пратити у PUBLICATION_STATUS.md.

**Првобитни исход:** припремљени су локални интеграциони комити, провере и план архивирања. `main` није промењен. Нема merge-а, затварања PR-ова, брисања Unity извора, cloud измена или deploy-а. Remote гране и нови Draft PR-ови **нису направљени**: GitHub write операције враћају `InvalidActionArgumentsError` пре извршења, а `git push` нема доступну аутентификацију. Ово није одбијање власниковог одобрења, него техничка блокада приступа.

## Проверено полазиште

- `main`: `814b85d19909bb07bc07b65da784acd9d05dfaaa`, после squash merge-а PR #20.
- 20 PR-ова у укупном попису: 15 отворених Draft PR-ова, пет већ спојених (#3, #4, #5, #10, #20). #5 је спојен у сомборску content грану, не у main.
- 36 remote грана. Попис укључује гране без PR-а; локалне нове гране су одвојено наведене.
- Main нема CI workflow у `.github/workflows`; у 25 доступних Actions покретања није нађен run на овом main SHA. **Нема CI доказа да је main зелен.**
- За сваки отворени PR проверени су SHA, база, списак diff фајлова, зависности и `git merge-tree --write-tree main head`. Та провера не мења ниједну грану. Детаљи су у `pr-snapshot.json`; нема ослањања на застарели connector `mergeable` резултат.
- Ово је интеграциони и статички аудит. Није нови Unity build, Android теренски тест, визуелни browser аудит нити потпуна редакторска провера свих реплика/историјских тврдњи.

## Матрица свих отворених PR-ова

„Чисто“ значи да Git тренутно не пријављује конфликт са наведеним main SHA; не значи дозволу за merge. „Нема“ у CI колони значи да међу свих 25 доступних Actions run-ова нема run-а на тачном head SHA, а не да је тест прошао.

| PR | Предлог статуса | Зависност / конфликт | CI на head / главна препрека |
|---|---|---|---|
| [#1 Unity поправке](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/1) | **Архивирати**, не интегрисати у активни main | Чисто; засебно стање Unity извора мора у приватну архиву | Нема; Unity compile/уређај непроверени |
| [#2 Sombor export](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/2) | **Интегрисати кроз консолидовани EDU кандидат** | Чисто; основа целог сомборског низа; #5 већ у тој грани | Нема; сам овај snapshot нема најновије мисије/missionType |
| [#6 Позориште](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/6) | **Интегрисати кроз исти EDU кандидат** | База #2; предак #7 и каснијих грана | Нема; садржај и runtime QA остају одвојени |
| [#7 Вељко Петровић](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/7) | **Интегрисати кроз исти EDU кандидат** | База #6; после њега иде Жупанија, која нема PR | Нема; GPS/AR није теренски проверен |
| [#8 Web Maps](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/8) | **Задржати као експеримент** | Конфликт `DECISION_LOG.md`; Google Maps адаптер није активни пут без кључева | Нема; локална 4 теста пролазе, али проверавају податке/геометрију, не Google приказ |
| [#9 AR Lab 0.2](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/9) | **Предложити затварање као застарело** након прихватања архивског статуса | Његов head је предак #18 преко V3 touch гране | Зелен run 37885458955; новији #18 садржи овај рад. Сада остаје отворен |
| [#11 Жупанија/PШМ](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/11) | **Интегрисати кроз EDU кандидат** | База `content/zup-production-20261009-0500`; не прескочити ту грану | Нема; историјске измене остају предмет редакторске провере |
| [#12 Android Host](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/12) | **Интегрисати касније, уз решење конфликата и profile adapter** | Конфликти `CHANGELOG.md` и `DECISION_LOG.md`; сачувати оба низа записа | Нема; локални Java session тест и JS bridge тест пролазе; није склопљена Android апликација |
| [#13 Ернест Бошњак](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/13) | **Интегрисати кроз EDU кандидат** | База #11; предак #14/#15 | Нема; није независан PR за директно спајање без свог садржајног низа |
| [#14 Градски музеј](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/14) | **Интегрисати довршену верзију из консолидоване гране**, не стари incomplete snapshot | База #13; PR мења и GK/LK, не само музеј; новије гране довршавају регистре | Нема; старо тело PR-а каже incomplete и није доказ актуелне интеграције |
| [#15 Лаза Костић](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/15) | **Интегрисати кроз EDU кандидат** | База #14; QA грана и каснији профилски комити су новији | Нема; мисијски и production регистри захтевају заједничку проверу |
| [#16 Budapest v2](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/16) | **Интегрисати припремљени поправљени кандидат**, па тек одлучити о старом PR-у | Чисто; полази од старог main; пренет цео v2 садржај без замене v1 или Сомбора | Нема CI на старом head; оригинални Node validator стварно пада. Припремљена исправка пролази |
| [#17 Табански храм](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/17) | **Интегрисати селективно после провере модела и везе са v2 актом VI** | Чисто, али мења v1 регистре; build инспектор чита стари FBX из Unity Assets | Нема run-а на head `d1846a6`; претходни `f8aa7bb1` зелен. После њега мењана два документа/регистра |
| [#18 AR Lab V4](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/18) | **Задржати као експеримент** | Чисто; садржи native → V2 → V3 touch → V4 низ | Зелен build/lint/analyzer/bridge/signature run на тачном head. Не доказује теренску стабилност depth-а |
| [#19 Безбедност](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/19) | **Први интеграциони приоритет**, уз посебну одлуку о брисањима | Чисто; уклања изворне credential вредности, али и APK и UserSettings фајл | Зелен тачан head. Не спајати цео PR под садашњим ограничењима без одобрења брисања; може се припремити издвојени non-deleting containment PR |

Већ спојени PR-ови: **#3** основни стандард, **#4** стандард v0.2, **#10** Budapest v1 наратив и **#20** EDU/GAME уговор су у main преко squash интеграција. **#5** Конјовић је интегрисан у #2 content грану. Њих не отварати поново; архивирање старих грана може се одобрити касније. Ниједна постојећа грана није обрисана.

## Зависности и гране без PR-а

Сомборски низ: **#2 → #6 → #7 → zup-production → #11 → #13 → #14 → #15 → sombor-integration-qa → editor/backend и v0.2 дораде**. Не спајати све старе PR-ове један за другим у main ако се прихвати консолидовани кандидат: тиме се враћају застарели регистри и непотпуни snapshot-и. Прво интегрисати један проверен садржајни snapshot, затим уз одобрење разрешити старе PR-ове као обухваћене.

Будимпештански низ: **screenplay-v2 → #16 editorial-finalization → local-editor/integration → integration-fix → save-restore → integration-save-gate**. Последњи save-gate helper постоји, али у затеченом серверу није био позван. Припремљена засебна интеграција га повезује.

AR низ: **native-ar-prototype → #9 diagnostics → diagnostics-touch → #18 depth-v4**. Нема разлога за одвојено спајање свих генерација.

- `feature/sombor-editor-v02-main-integration-20261010` има само `INTEGRATION_STATUS.md`: изричито пише да код и подаци нису интегрисани.
- `feature/sombor-editor-v02-20261010` садржи новије EDU регистре и `missionType`, али сам `tools/sombor-editor/` садржи само README. Извршни сомборски едитор није пронађен у том Git snapshot-у.
- `content/sombor-integration-qa-20261009`: 51 JSON фајл се парсира; 9 мисија/35 задатака, док manifest тврди 34/8 AR. Новија v0.2 грана пријављује 35/9 AR и `edu_mission`.
- На новијој Жупанији постоје нови визуелни фајлови, али мисија још референцира стара имена `zup-three-clocks-board`, `zup-hall-1898`, `zup-painting-1896`, а scene-flow користи `2d_three_traces_role_mapping` насупрот мисијском `2d_three_clocks_role_mapping`. Не означавати ту консолидацију као завршену без усаглашавања визуелних ознака/путања и fallback уговора.
- Тачан попис свих 36 грана, head SHA и ancestor односа је у `branch-snapshot.json` и табели испод. Squash merge значи да непостојање ancestor односа само по себи није доказ да садржај није интегрисан.

## PR #19 и изложени кључеви

На познатим изворним путањама актуелног main поново су потврђене **две различите Google API вредности у шест фајлова** и **три различита JWT облика у четири локације**. Вредности нису исписане, коришћене или пренете у овај извештај. На истим путањама head-а #19 нема таквих вредности. То је циљана изворна провера, не нови потпуни историјски/бинарни audit.

[CI #19](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/actions/runs/37989475588) је успешан на `97af06dc0c2650f333843fa17af22a19492296aa`; прегледани су job/step исходи. Scanner експлицитно прескаче бинарне/архивске фајлове и не чита историју. Зелени scan не доказује да су credentials опозвани нити да APK/history немају старе вредности.

Власник је раније навео да је обрисао три стара Google кључа; њихово подударање са нађеним вредностима и статус Cesium токена овде нису независно потврђени. Google Cloud/Cesium нису отварани. Нови кључеви нису потребни у овој фази. Не проглашавати инцидент затвореним само на основу овог PR-а.

## EDU/GAME после #20

Main има коректне профилске смернице и `game_mission` на Budapest **v1** manifest-у. **v2 уопште није у main**, а #16 manifest нема `missionType`. Сомборски пакет такође није у main; тамо постоји профилски AGENTS, не интегрисани EDU dataset.

Припремљени v2 кандидат додаје `missionType: game_mission`, content `2.0.0-draft.2`, задржава candidate schema version према адитивном правилу Mission Type Contract v0.1 и одбија missing/invalid/EDU профил. Не мења стабилне ID-јеве нити преноси player state из v1.

Затечени Budapest validator намеће 3 задатка по акту, GPS→AR→quiz и фиксно бодовање. Ово је задржано као регресиона провера **већ написаног кандидата**, уз експлицитну напомену да то није универзални GAME услов. Ова фаза није преписивала причу или уклањала ауторске задатке. Будућа измена GAME механике треба да ажурира те пакетске провере.

Фиксни Budapest editor root је `game-data/budapest/v2`; сервер сада проверава gameId/missionType при покретању и сваком захтеву, а manifest save одбија конфликт. Додата је провера кандидата пре стварног save-а. Нема аутоматске замене едитора. Android/Web runtime dispatch по missionType и потпун end-to-end још нису имплементационо потврђени.

## Провере и CI докази

| Објекат | Резултат | Граница |
|---|---|---|
| Main `814b85d` | Нема Actions run-а/workflow-а | Не називати зеленим main |
| #19 `97af06dc` | [SUCCESS](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/actions/runs/37989475588) | Text source scan, не history/binary/provider |
| #18 `b6763e04` | [SUCCESS](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/actions/runs/37985787225) | Build, lint, analyzer, bridge, APK signature; није теренски AR тест |
| Budapest editor `5a260dfe` | [SUCCESS](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/actions/runs/38050684547) | Helper тестови нису доказ да је helper повезан са save сервером |
| Tabán претходни `f8aa7bb1` | [SUCCESS](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/actions/runs/37971637198) | Head `d1846a6` нема run; последњи diff мења регистар и MODEL_REVIEW |
| Стари #16 Node validator | **FAIL**: `unsupported AR mapping BP2_A0_EXPLORE` | Стварно поновљено, не само прочитан PR опис |
| Нови v2 кандидат | **PASS_STATIC_ONLY**, 1575 провера | 79/79 чворова, 11 путања, 48 сцена, 375 реплика, 492 sequence везе |
| Негативни profile тестови | 4/4 одбијена: missing, EDU, empty, invalid | Ниједан fallback на подразумевани GAME |
| Нови editor | **15/15 PASS** + syntax | Укључује HTTP save, dangling refs, ID rename, valid prose save, stale revision, profile drift; изолован temp dataset |
| #12 локално | Java session + JS bridge PASS | Нема Android APK/device теста |
| #8 локално | **4/4 PASS** | Геометрија/зоне/референце; није учитавање Google мапе |

Нови локални комити **немају remote CI**, пошто push/Draft PR није успео. Постојећи зелени run-ови не приписују се новим комитима. Нема новог APK-а. Извештаји и job/step snapshot су у овом директоријуму.

Једанаест Budapest waypoint-а остаје непроверено, 13 медијских ставки остаје планирано. Усвојен је сомборски модел GPS зона за сценске/звучне догађаје, али координате, runtime adapter и теренски пријем нису измишљени. Медији остају одложени, осим засебног Табанског модела. Нема потребе за Google кључевима да би се наставио овај посао.

## Нове локалне гране и редослед

| Грана | Комит | Предвиђена база Draft PR-а | Шта је припремљено |
|---|---|---|---|
| `integration/repository-stabilization-20261010` | Видети handoff manifest | `main` | Матрица, докази, усвојене границе, Unity inventory и read-only verifier |
| `integration/budapest-v2-content-20261010` | `3bd25d6` (претходни `1ae993a`) | `main` | #16 v2 садржај, постојећа prologue исправка са новије гране, missionType и content CI |
| `integration/budapest-editor-20261010` | `302efd8` | `integration/budapest-v2-content-20261010` | Новији editor плус серверски profile/save gate; засебан преглед од content-а |

Први v2 комит намерно издваја велики већ написани пакет (78 фајлова) од мале интеграционе исправке у другом комиту. Исправка `evidence_map`→`evidence_mapping` преузета је са `5a260dfe`; остали ауторски фајлови #16 очувани. Две затечене trailing-space напомене у ауторском тексту нису суштински мењане. Сомбор и Unity немају ниједну измену у овим content/editor гранама.

Предложени редослед прихватања:

1. Прегледати овај audit и приватну архивску процедуру; ово само по себи не проглашава main стабилним.
2. Безбедносно обуздавање #19: одвојити брисања или их посебно одобрити. Нема нових кључева. Тек тада full source scan може бити main gate.
3. Budapest v2 content Draft; поновити CI на remote SHA и прегледати уреднички пакет.
4. Budapest editor stacked Draft; remote CI + ручна browser провера, одвојено од неимплементираног backup restore-а.
5. Консолидовани Sombor EDU пакет са новијим профилом/инвентаром, усаглашеним Жупанија assets/fallback и потпуним validator-ом; затим стварни извор едитора када буде доступан.
6. Android Host #12 после спајања оба низа дневника и експлицитног GAME/EDU adapter теста. #8 остаје Google експеримент; садашњи key-free map приказ едитора није замена за player runtime.
7. Tabán #17: независни преглед геометрије, права и mapping-а на v2 акт VI; очувати FBX зависност пре Unity уклањања.
8. AR #18 задржати лабораторијски; нови теренски докази пре употребе у главном player-у.
9. Тек након потпуно проверене приватне архиве — засебни removal PR и власниково одобрење за Unity из активног репозиторијума.

## Одлуке које захтевају власниково одобрење

- Сваки merge у main; сада није извршен ниједан.
- Затварање #9 као превазиђеног и, касније, старих садржајних PR-ова након доказане консолидоване интеграције. Ниједан сада није затворен.
- Брисања APK/UserSettings у #19 и касније уклањање Unity извора, тек после потврде архиве.
- Избор да се #1 чува искључиво у архиви и #8/#18 задрже као експерименти.
- Ако се жели browser fallback за прављење већ припремљених Draft PR-ова: потребна је потврда тог начина приступа, јер правила browser алата захтевају одобрење када довољан конектор понављано не ради. Ово није поновни захтев за дозволу да се сами PR-ови направе.

Архивски репозиторијум није креиран и потпуност архиве није потврђена. План је у `UNITY_ARCHIVE_PLAN.md`; инвентар није архива. Не приступати cloud-у, не мењати credentials, не покретати јавни deploy.

## Потпун попис remote грана

| Грана | SHA | Предложени статус |
|---|---|---|
| `chronoeye/ar-depth-v4-20261009` | `b6763e04` | Експеримент; V4 обухвата старије генерације |
| `chronoeye/ar-diagnostics-20261009` | `bc1f9acb` | Експеримент; V4 обухвата старије генерације |
| `chronoeye/ar-diagnostics-touch-20261009` | `dc3b1456` | Експеримент; V4 обухвата старије генерације |
| `chronoeye/native-ar-prototype` | `0fef5c98` | Експеримент; V4 обухвата старије генерације |
| `content/budapest-narrative-20261009` | `9b499f98` | Историјска/squash интеграција; предложити архивирање гране |
| `content/budapest-screenplay-v2-20261009` | `c96c712e` | Задржати до консолидоване интеграције; потом предложити архивирање |
| `content/budapest-v2-editorial-finalization-20261009` | `418a205f` | Задржати до консолидоване интеграције; потом предложити архивирање |
| `content/editor-backend-integration-20261009` | `1a35ca4e` | Задржати до консолидоване интеграције; потом предложити архивирање |
| `content/ernest-bosnjak-production-20261009-0900` | `7d778e6a` | Задржати до консолидоване интеграције; потом предложити архивирање |
| `content/gms-production-20261009-1500` | `e3c292ec` | Задржати до консолидоване интеграције; потом предложити архивирање |
| `content/laza-kostic-production-20261009` | `be48f088` | Задржати до консолидоване интеграције; потом предложити архивирање |
| `content/nps-production-20261009-0300` | `9caa77f8` | Задржати до консолидоване интеграције; потом предложити архивирање |
| `content/revision-zup-psm-20261009` | `d5aed718` | Задржати до консолидоване интеграције; потом предложити архивирање |
| `content/sombor-integration-qa-20261009` | `7098e83c` | Задржати до консолидоване интеграције; потом предложити архивирање |
| `content/sombor-portable-game-data-20261008` | `aa74a2bc` | Задржати до консолидоване интеграције; потом предложити архивирање |
| `content/vp-production-20261009-0400` | `71d52a13` | Задржати до консолидоване интеграције; потом предложити архивирање |
| `content/zup-production-20261009-0500` | `d335108e` | Задржати до консолидоване интеграције; потом предложити архивирање |
| `docs/chrono-eye-mission-standard-v0.1` | `ec807939` | Историјска/squash интеграција; предложити архивирање гране |
| `docs/mission-standard-v0.2-production-gk` | `5ad5a7d9` | Историјска/squash интеграција; предложити архивирање гране |
| `feature/android-host-session-20261009` | `1bc4bf9f` | Будућа интеграција #12, конфликти дневника |
| `feature/chronoeye-web-maps-package-20261009` | `f46b1630` | Google Maps експеримент #8 |
| `feature/mission-type-contract-20261010` | `f3ac29c9` | Историјска/squash интеграција; предложити архивирање гране |
| `feature/sombor-editor-backend-20261009` | `27861099` | Задржати до консолидоване интеграције; потом предложити архивирање |
| `feature/sombor-editor-v02-20261010` | `8b3ac189` | Задржати до консолидоване интеграције; потом предложити архивирање |
| `feature/sombor-editor-v02-main-integration-20261010` | `25a02f68` | Само preflight документ; није едитор |
| `feature/taban-church-historical-reconstruction` | `d1846a63` | Селективна интеграција модела после QA |
| `fix/google-api-key-exposure-20261009` | `97af06dc` | Безбедносни приоритет #19, брисања на чекању |
| `fix/sombor-resource-stabilization-20261008` | `44db83c1` | Приватна Unity архива, PR #1 |
| `main` | `814b85d1` | Заштићена интеграциона основа; нема измена |
| `production/milan-konjovic-genije-boja-20261008` | `833fd268` | Задржати до консолидоване интеграције; потом предложити архивирање |
| `production/muzej-podunavskih-svaba-20261009` | `aa74a2bc` | Задржати до консолидоване интеграције; потом предложити архивирање |
| `tooling/budapest-editor-integration-20261010` | `2a1afabd` | Користити најновији save-gate; старије snapshot-е предложити за архиву |
| `tooling/budapest-editor-integration-fix-20261010` | `61e6044c` | Користити најновији save-gate; старије snapshot-е предложити за архиву |
| `tooling/budapest-editor-integration-save-gate-20261010` | `5a260dfe` | Користити најновији save-gate; старије snapshot-е предложити за архиву |
| `tooling/budapest-editor-save-restore-20261010` | `db22be39` | Користити најновији save-gate; старије snapshot-е предложити за архиву |
| `tooling/budapest-local-editor-20261010` | `2a1afabd` | Користити најновији save-gate; старије snapshot-е предложити за архиву |
