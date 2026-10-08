# CHRONO EYE — SOMBOR | Portable game data v1.1

Samostalan **sadržaj igre**, izveden iz Unity ScriptableObject resursa i dopunjen urednički prihvaćenim AR zadacima, namenjen kombinaciji **Web + Android + ARCore**. Ovo **nije** gotova Android aplikacija i ne zahteva Unity kao runtime.

## Počni ovde

- [`manifest.json`](manifest.json) — ulazna tačka za sav sadržaj i verziju šeme.
- [`production/gradska-kuca/PRODUCTION_BRIEF.md`](production/gradska-kuca/PRODUCTION_BRIEF.md) — potpuni pilot-paket Gradske kuće na Ćelavom trgu, sa istorijskim narativom.
- [`production/gradska-kuca/asset-manifest.json`](production/gradska-kuca/asset-manifest.json) — šta je vizuelno napravljeno, a šta još čeka 3D i zvuk.
- [`production/gradska-kuca/scene-flow.json`](production/gradska-kuca/scene-flow.json) — redosled GPS → profesor → AR toranj → istorijski kviz → nagrada.
- [`production/gradska-kuca/QA_CHECKLIST.md`](production/gradska-kuca/QA_CHECKLIST.md) — kriterijumi prihvatanja pre release-a.

## Sadržaj

- `missions/*.json`: **9 misija / 31 zadatak**: 9 GPS, 5 AR, 11 multiple-choice, 6 text-input. AR dolazi odmah posle GPS-a i ima jednakovredan 2D fallback.
- `waypoints.json`: 13 WGS84 lokacija, probni GPS radijusi i vreme potvrde zona.
- `dialogues/*.json`: tri narativna dijaloga i jedan neaktivan testni.
- `hints.json`: 13 tekstova pomoći; GK ima HINTGK1/HINTGK2/HINTGK3.
- `timeline.json`: registar izvornih 26 blokova, a **ne** obavezni linearni redosled.
- `triggers.json`: 10 grupa i 18 originalnih event-uslova; treba posebno testirati semantiku pri portovanju.
- `systems/characters.json`: klasa AR mentora (Profesor Vlada) i budući istorijski eho-likovi.
- `systems/collectibles.json`: glavni relic po lokaciji, zlatni pečati iz 1749, srca i džokeri.
- `systems/album.json`: UX „Herbarijum Sombora“, kolekcija, otključane kartice i zaštita od duplih poena.
- `mechanics.json`: zadaci, skoring, GPS, AR i fallback.
- `ar-proposals.json`: evidencija **prihvaćenih pet AR ideja**, sve povezane sa aktivnim zadacima.
- `assets/*.svg`: sedam originalnih konceptnih 2D grafika koje ne zahtevaju licencu spoljašnjih fotografija.
- `CHANGELOG.md`: urednički i produkcioni trag odluka.

## Podela odgovornosti

**U ovom toku** uređujemo istoriju, tekstove, model mentora, AR dizajn, kolekciju, opise aseta i UX. **U tehničkom toku** gradimo Web/Android/ARCore implementaciju, GPS poligone, API-je, GLB, zvuk, kamerni prikaz i test. Staru Unity logiku koristimo kao izvor pravila igre, ne kao obavezan engine.

## Šta je stvarno gotovo

Zapisani i provereni JSON zadaci, produkcioni brief, evidencija scena, definicije albumskih nagrada, inicijalni SVG koncepti. **Nisu još isporučeni** 3D mentor, snimljen glas, fotografija za 2D fallback, geospatial anchoring ni aplikacioni kod za ove AR zadatke.

## Istorijski izvori i autorstvo

GK igra zadržava 1718 / 1749 / 1842 i originalno pitanje, uz uredničku korekciju teksta i izvorne linkove. Zlatni token jeste **stilizovani savremeni znak inspirisan pečatom Sombora**, a ne reprodukcija originala; istorijsku osnovu proveriti na Ravnoplovu i u Istorijskom arhivu.

## Trajnost podataka

Sve napredovanje, poeni i otključani predmeti pripadaju stanju igrača (lokalno ili u zasebnom backend-u), a **nikada se ne upisuju u ove javne JSON fajlove**. Stare, slučajno popunjene Unity kolone ostaju samo za reviziju; izvorne vrednosti prve misije sačuvane su pod `legacyOriginal`.
