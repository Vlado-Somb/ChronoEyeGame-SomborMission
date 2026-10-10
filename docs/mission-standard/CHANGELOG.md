# CHRONO EYE — историја стандарда

За сваку измену верзије стандарда назначити датум, ID одлуке, измењене фајлове, компатибилност и потребне миграције.

## [0.2.0-draft] — 2026-10-08

**Статус:** `accepted_for_design`; ниједан нови Web/Android/ARCore end-to-end тест није овим записом извршен.

- **DEC-2026-012:** обавезан наративни историјски слој и право образовно питање поред AR туторијала; GK чува 1718/1749/1842, са квизом о 1842.
- **DEC-2026-013:** пет прихваћених, у подацима активних AR интеракција и увек доступан једнако вредан 2D fallback; `active` НЕ доказује имплементацију.
- **DEC-2026-014:** регистар AR ментора, Професор Влада и будуће историјске ехо-појаве/гласови, уз захтев за титлове, права и безбедно постављање.
- **DEC-2026-015:** четири класе колекционарских предмета — `relic`, `golden_seal`, `heart`, `joker` — опционални бонус и идемпотентно бодовање.
- **DEC-2026-016:** албум/инвентар „Herbarijum Sombora“, трајно откључавање и читање кратких прича у видљивом UX.
- **DEC-2026-017:** обавезна четири документа по новој мисији; регистар стварних и планираних медијских асета и QA.
- **Ажурирано:** `MISSION_STANDARD_v0.2.md`, `DECISION_LOG.md`, `CHRONO_EYE_STANDARD.md`, `AGENTS.md`, `CHANGELOG.md`, нови шаблон продукционог пакета.
- **Миграција:** v0.1 остаје доступан; стари пакети остају читљиви. Нови AR/collectible/mentor слој тражи експлицитно допуњене JSON ресурсе. Постојећи state се проширује без дуплирања награда.
- **Сервиси:** нема нових API избора, credentials, Cloud активације нити промене `SERVICE_REGISTRY.md`.
- **Доказ/референца:** [GK production, PR #2](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/pull/2), још није имплементациони Android тест.


## [0.1.0-draft] — 2026-10-08

**Статус:** `accepted_for_design`, још није `verified_in_test`.

- Први јединствени рецепт за CHRONO EYE мисије независне од Unity runtime-а.
- Везе са Sombor Data Export PR #2: 9 мисија, 26 задатака, 13 waypoint-а, дијалози, помоћи, timeline и тригери.
- Дефинисани стабилни ID-јеви, WGS84 GPS зоне, правила бодовања, наратив, AR fallback и одвојен player state.
- Уведени `DECISION_LOG.md`, `SERVICE_REGISTRY.md` и `PRACTICE_LOG.md` како би се забележиле измене механике, провере и сервиса.
- Google Cloud и мапе нису активиране у оквиру ове измене. Избор Maps JavaScript / Android SDK / алтернативног адаптера је отворен (DEC-009).
- Шема за нове AR/collectible задатке још је предлог; не нарушавати Sombor v1 структуру без нове одлуке.

### Услови за наредни release

- Постоји end-to-end Android/Web/ARCore тест који учитава и завршава најмање једну мисију.
- Најмање један GPS улазак је теренски проверен, са тачношћу и условима.
- ARCore capability-check и fallback раде.
- Тек након тога ажурирати верзију стандарда/шеме и унети `verified_in_test` доказ.

## Шаблон

```md
## [x.y.z] — YYYY-MM-DD
- Измене:
- DEC-референце:
- Утицај на претходне game-data пакете:
- Обавезна миграција:
- TEST / доказ:
```

## Budapest candidate contract — 2026-10-09
- DEC-BP-2026-001: први будимпештански наративни пакет; локална candidate schema 1.2.0-budapest-draft.1, content 1.0.0.
- Експлицитни graph/choices/evidence и gate завршетка; миграција описана у пакетском DATA_CONTRACT.md.
- Глобални стандард остаје v0.2; нема аутоматске миграције Сомбора нити потврде runtime подршке.

## Организација репозиторијума — 2026-10-09
- DEC-REPO-2026-001: одобрени један репозиторијум и заједничка апликација са одвојеним пакетима градова; привремене радне гране и PR ток.
- Додат REPOSITORY_STRUCTURE.md и везе из смерница. Без премештања прототипова, измене workflow-а или активирања продукције.

## 2026-10-10 — Repository stabilization preparation
- Add PR/branch/CI snapshot and private Unity archive inventory/verification procedure.
- Record accepted phase boundaries in DEC-STAB-2026-001. No main merge, runtime implementation, provider configuration or archive-completion claim.
- Budapest content and editor candidates are separate local review branches; their checks do not constitute remote CI or device verification.
