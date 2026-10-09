# CHRONO EYE — регистар сервиса и начина коришћења

**Стање на дан:** 2026-10-08. Ово је пројектни регистар, **не** извештај прочитан из Google Cloud Console. Не тврдимо да су Cloud пројекат, billing, API или креденцијали креирани/активни: немамо потврђен приступ том налогу.

**Статуси интеграције:** `not_configured` — није потврђена конфигурација; `configured` — потврђена конфигурација, али није тестирана; `verified` — интеграција доказано ради; `disabled` — свесно искључена или не користи се; `decision_pending` — није изабран начин интеграције. Статус означава оно што **знамо**, не оно што претпостављамо.

## Регистар

| Компонента / сервис | Статус | Предвиђена употреба | Начин/граница | Одлука |
|---|---|---|---|---|
| **WGS84 JSON waypoint база** | `configured` у извезеном пакету, runtime непроверен | Канонске координате, 3 зоне, GPS услови | Сопствена база; без Places API зависности | DEC-003/004 |
| **Android Location / GPS** | `decision_pending` runtime | Очитавање локације, тачности, тригера | Дозвола током рада; локална обрада, не делити трагове | DEC-002/004 |
| **ARCore SDK (Android)** | `decision_pending` runtime | Поза камере, просторна сидра, интеракције | AR Optional/fallback док не постоји друга одлука; runtime capability-check | DEC-002/008 |
| **ARCore API (Google Cloud)** | `not_configured` | VPS/Geospatial/Cloud Anchors ако их употребимо | Укључити само када је потребно; Android Keyless OAuth по package name + SHA-1 | DEC-007/008 |
| **ARCore Geospatial/VPS** | `not_configured` | Геопросторно AR постављање | Захтева ARCore API, локацијске дозволе и проверу VPS доступности | DEC-008 |
| **ARCore Depth** | `decision_pending` | Заклањање виртуелних објеката стварним | ARCore SDK режим, **није** засебан Cloud Maps API; check device support | DEC-004/008 |
| **ARCore Streetscape Geometry** | `decision_pending` | Геометрија фасада/терена и occlusion | ARCore Geospatial + session режим; доступност зависи од локације | DEC-008 |
| **Google Maps JavaScript API** | `decision_pending` | Веб мапа ако изаберемо Google подлогу | Само за проверени HTTPS/referrer WebView сценарио; одвојен restricted browser key | DEC-009 |
| **Maps SDK for Android** | `decision_pending` | Алтернатива за native Android мапу | Android package/certificate ограничења API кључа | DEC-009 |
| **Независни Web map adapter** | `decision_pending` | Преносиви приказ сопствених WGS84 маркера | Библиотека, подлога и лиценца бирају се посебно; не злоупотребљавати јавне OSM tile сервере | DEC-009 |
| **Google My Maps** | `decision_pending` за уређивачки workflow | Визуелна теренска корекција координата | Само редакторски алат / ручни извоз и ревизија; није runtime извор истине | DEC-009 |
| **Places API (New)** | `disabled` у почетном плану | Евентуално претрага установа/адреса | Не за аутоматско померање waypoint-а | DEC-010 |
| **Routes API** | `disabled` у почетном плану | Евентуално пешачка навигација | Укључити само уз стварну функцију рутирања и прорачун трошка | DEC-010 |
| **Geocoding API** | `disabled` у почетном плану | Конверзија адреса у координате | Не треба за ручно проверене WGS84 координате | DEC-010 |
| **Roads API** | `disabled` | Није применљиво за пешачке историјске мисије | Не активирати без посебне нове одлуке | DEC-010 |

**Планирани Cloud пројекат:** радни назив `CHRONO EYE PLATFORM`, **Project ID: није додељен / није проверен**. Посебне игре остају у својим JSON пакетима; о евентуалном раздвајању cloud пројеката по окружењу/домену одлучити након анализе права и трошкова.

## Како приступамо мапи

1. Само наше координате `waypoints.json` контролишу мисије и GPS тригере. Google Place ID, адреса или нацртана иконица не могу их аутоматски заменити.
2. Web мапа и native Android мапа су **изборни адаптери** на исте координате. Платформа/SDK и лиценца бирају се кроз DEC-009.
3. Google Maps JavaScript у локално упакованом WebView-у **не претпостављати** као радно решење без пробе restricted кључа; локални URI и referrer могу правити проблеме.
4. За Google Maps се користе одвојени кључеви и ограничења по API-ју, типу апликације и окружењу. Никад не стављати server key/service-account credential у клиентски JS.
5. Ми смо власници GeoJSON/JSON мисијских координата; Google сервиси могу помоћи при претрази, навигацији или AR сидрима, не замењују наш садржај.

## ARCore / VPS

- ARCore SDK функције, нпр. Depth, нису засебни Maps Platform API-ји; проверавају се у AR сесији на уређају.
- За геопросторну AR функцију омогућити ARCore API када буде одобрен Cloud пројекат. За Android предност има Keyless OAuth 2.0; упарити package name и SHA-1 за сваки важећи потпис (debug, release, Play App Signing ако се користи).
- VPS availability и tracking quality су динамични и зависе од места и светла; обавезан је алтернативни AR режим.
- Не користити стару Unity надморску висину као проверену geospatial висину.

## Cloud безбедност, billing и одобравање

**Пре било које активације:** забележити у `DECISION_LOG.md` конкретан случај употребе, Cloud Project ID (несекретан), одговорног, потребан API, обрачун трошка, дозвољене домене/package/signing fingerprints, квоте и план гашења. Тек после власниковог одобрења мењати Cloud Console.

- Google Cloud IAM: minimum privilege; не слати креденцијале у Git/чат ако нису неопходни.
- API keys: ограничити по API и типу апликације.
- Billing alerts (alerts-only) нису аутоматски лимит потрошње; размотрити quotas/spend cap само ако су за услугу доступни.
- По свакој промени статуса услуге овде унети датум, шта је потврђено и линк на TEST доказ.

## Проверена документација

- ARCore Geospatial: https://developers.google.com/ar/develop/java/geospatial/enable
- Android ARCore authorization: https://developers.google.com/ar/develop/authorization?platform=android
- ARCore Depth: https://developers.google.com/ar/develop/java/depth/developer-guide
- ARCore Streetscape: https://developers.google.com/ar/develop/java/geospatial/streetscape-geometry
- Google Maps Platform security / WebView: https://developers.google.com/maps/api-security-best-practices
- Maps SDK for Android: https://developers.google.com/maps/documentation/android-sdk/get-api-key
- Routes API: https://developers.google.com/maps/documentation/routes/route-usecases
- Roads API: https://developers.google.com/maps/documentation/roads/
- Cloud Billing budgets: https://docs.cloud.google.com/billing/docs/how-to/budgets

## Образац за измену регистра

```md
- Датум:
- Сервис:
- Претходни / нови статус:
- Нови начин коришћења:
- Project ID / регион / API name (без credential-а):
- Кључ/ауторизација: само тип и ограничење; НЕ тајна
- Очекује ли се трошак? Колики / којим лимитом?
- Одлука DEC-...:
- Доказ конфигурације / TEST-...:
- Rollback / деактивација:
```


## Инцидент: наслеђени Unity Google кључеви (2026-10-09)

- **Сервиси / извор:** Legacy Unity `ARCoreExtensionsProjectSettings.json` и сцене са URL-ом `tile.googleapis.com/v1/3dtiles`.
- **Проверено у Git-у:** два различита кључа пронађена у шест текстуалних фајлова јавне `main` гране; припремљено чишћење у `fix/google-api-key-exposure-20261009`. Не објављивати стварне вредности.
- **Google Cloud статус:** `not_configured` за НОВУ CHRONO EYE платформу и **unverified** за nasleđeni Unity credential-e. Присуство кључева у репозиторијуму не доказује да су активни, исправно ограничени нити да их је неко злоупотребио.
- **Безбедносни статус:** `remediation_pending` — власник мора ротирати/опозвати оба изложена кључа и проверити usage, API ограничења и обрачун. Стари комитови/гране и APK остају предмет засебне контроле.
- **Одлука:** `DEC-SEC-2026-001`; поступак и контролна листа: `docs/security/GOOGLE_API_KEY_INCIDENT_2026-10-09.md`.
- **Нови credential модел:** посебна Android ARCore ауторизација (где је применљиво Keyless OAuth), одвојена Map Tiles HTTP/backend стратегија, одвојен restricted browser Maps кључ ако буде потребан. Ништа не сматрати тестираним пре праве конфигурације и тестова.
