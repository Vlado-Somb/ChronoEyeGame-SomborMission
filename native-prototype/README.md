# ChronoEye AR Lab 0.2.0

Дијагностички Android прототип за следећи S25 Ultra тест. Нова верзија има
камера/heatmap/overlay, независно заклањање, до пет обојених предмета, временски
усклађене приватне логове и depth снимке, и ZIP дељење из AR екрана.

- [Кратко упутство за тест](docs/FIELD_TEST_SR.md)
- [Шема и ограничења дијагностике](docs/LOG_SCHEMA.md)
- [Одлуке / интеграционе тачке](docs/DECISION_LOG.md)
- [Провере и стварни статус](docs/VALIDATION.md)
- [Offline анализатор](tools/analyze.py)

Нема аутоматског upload-а нити снимања RGB/аудија. Обавезно поделити дијагностику
пре напуштања текућег AR екрана. Локална сидра се не чувају после затварања.
Код је припремљен за тест; теренска исправност није потврђена изградњом APK-а.

## Изградња

JDK 17, Android SDK 35 + build-tools 35.0.0, Python 3.

```
python3 prepare_assets.py
./gradlew :app:assembleDebug :app:lintDebug
python3 -m unittest discover -s tools -v
```

Излаз: `app/build/outputs/apk/debug/app-debug.apk`. Debug потпис зависи од машине;
други потпис може захтевати деинсталацију старог тестног APK-а (брише податке).
Ниједан signing key или приватни endpoint credential није у јавном репозиторијуму.

Верзије: Gradle 8.11.1, AGP 8.9.1, Kotlin 2.1.0, ARCore 1.56.0. ARM64,
Android 8+, OpenGL ES 3.0. Пакет `rs.chronoeye.prototype`.

## Границе и порекло

Задржан постојећи ограничени WebView/native мост и Activity result; пуни
оркестратор, мапе, GPS/VPS, Streetscape, мисије и Unity нису део ове измене.

Google ARCore Android SDK `samples/hello_ar_java`, commit
`3abfeb18669c2cbb2d07057f135d117ee9d91826`, Apache-2.0:
https://github.com/google-ar/arcore-android-sdk . Видети `LICENSE-GOOGLE-ARCORE.txt`.
Бинарни ресурси и wrapper имају SHA-256 проверу у `prepare_assets.py`.

Праг свежине 100 ms остаје. Нове корекције: stride-aware DEPTH16 upload,
нулта дубина не заклања, исправан почетни aspect ratio, nearest byte sampling,
унапред припремљени shader-и. Ово нису тврдње о узроку ранијег теренског проблема.
