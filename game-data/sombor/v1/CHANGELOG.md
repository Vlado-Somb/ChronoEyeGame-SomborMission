# CHANGELOG — CHRONO EYE SOMBOR

## 2026-10-09 — Županija, production package 06

- SO_ŽUP remains 45.770009385415605, 19.117732572954402; zone 2 / 66 m is provisional, not field-verified.
- Added ZUP-AR-01 immediately after GPS: map 1697 historical event, 1896 painting, 1898 installation; equivalent 2D fallback.
- Full original Unity mission archived unchanged in production/zupanija/legacy-zup.json.
- ZUP-02 expectedAnswer "Bitka kod Sente" and ZUP-03 four choices / correctIndex=0 preserved.
- Professor Vlada story, four production documents, five original SVG concepts and unique Ključ tri vremena relic.
- Two sourced golden seals (1786 / 1882); no hearts or jokers.
- Updated manifest, AR registry, collectibles, characters, mechanics, README. Inventory: 9 missions / 34 tasks / 8 AR.
- No Web/Android/ARCore, GPS field, GLB/OGG or photo-rights verification. No main/runtime/cloud changes.



## 2026-10-09 — Veljko Petrović, production package 05

- Waypoint SO_VP is **spomenik Veljku Petroviću**, not the Gradska biblioteka; sources confirm the monument stands in front of the library.
- Added VP-AR-01 after GPS: original three-card chronology **Sombor 1884 → Budimpešta/Tekelijanum 1902 → Sombor memorial 2017** with equal 2D fallback.
- Full original mission archived unchanged in `production/veljko-petrovic/legacy-vp.json`; original quiz answer indices VP-02=0 and VP-03=1 retained.
- Professor Vlada's dramatic script, five original SVG concepts, `vp-feather-relic`, two sourced golden seals and a fictional joker.
- Four production documents and updated registers, manifest, AR proposals, mechanics and README. Inventory: **9 missions / 33 tasks / 7 AR**.
- GPS zone 2 / 12 m is provisional. GLB, OGG, WebP, Web/Android/ARCore integration and field tests pending. No runtime or Cloud credentials changed.


## 2026-10-09 — Narodno pozoriste, production package 04

- Stage role mapping (curtain, actor, audience) with equivalent 2D fallback.
- Original NPS task archive preserved at production/narodno-pozoriste/legacy-nps.json; quiz answers 1882 and choice index 1 unchanged.
- Historical facts 1882 / 1946 / 1952 reviewed; 1877/1879 society date conflict flagged.
- Four production documents, five original SVG concepts, symbolic Promptbook relic, two sourced gold seals and a fictional joker.
- GPS zone 2 provisional; no Android/ARCore, GLB, OGG or field verification.


## 2026-10-09 — production package 03: Muzej podunavskih Švaba

- Nova AR mini-igra `PSM-AR-01` između GPS i dva istorijska kviza: tri originalne simbolične kartice (dolazak → svakodnevica → muzej), jednakovredan 2D fallback.
- Originalni kviz o Grašalkovićevoj palati ostaje (tačan odgovor indeks 1). Originalni odgovor **1751** za jedinstven početak nemačkog doseljavanja nije istorijski pouzdan; sačuvan je pod `PSM-03.legacyOriginal`, a aktivni odgovor je proverena godina osnivanja muzeja **2019**.
- Napisana četiri produkciona dokumenta, mentorski narativ, četiri originalna SVG koncepta, `psm-memory-key-relic` i dva istorijska zlatna pečata. Nema srca, džokera ni istorijskog eha zbog dostojanstvenog pristupa stradanju.
- Privremena GPS zona prilaza 2 (15 m), `fieldVerified=false`. Inventar: 9 misija, 32 zadatka, 6 AR.
- Web/Android/ARCore integracija, GLB/OGG i terenska provera nisu obavljeni. Servisi i credentials nisu menjani.


## 2026-10-08 — production package 02: Milan Konjović

- Razrađena galerijska misija `sombor-mkg` kao urednička i produkciona celina po Mission Standard v0.2, bez promene broja glavnih zadataka. Tok: GPS `MKG-01` → AR `MKG-AR-01` → ABC `MKG-02` → tekst `MKG-03`.
- Sačuvani stari kvizovi `Slikarstvo` i `1898` sa izvornim formulacijama u `legacyOriginal`, ispravljene pravopisne nepravilnosti u aktivnim tekstovima; dodata biografija 1898–1993, evropski put i galerijski legat.
- AR „Genije boja“ sada je jasno tipizovana **redosledna igra** sa plavom fazom 1929–1933, crvenom 1934–1940 i sivom 1945–1952, bez korišćenja reprodukcija zaštićenih slika, sa 2D fallback-om iste vrednosti.
- Napravljeno **6 originalnih SVG asset koncepata**, audio i GLB resursi planirani, ne proglašeni proizvedenim.
- Novi `mkg-palette-relic`, zlatni pečati `gold-mkg-001` / `gold-mkg-002` i fikcionalni šaljivi `joker-mkg-001`; poseban `milan-konjovic-echo` bez imitacije istorijskog glasa.
- Četiri produkciona dokumenta i kontrolna lista `production/galerija-milan-konjovic/`, uz istorijske izvore i terenske QA zadatke.
- `SO_MKG` koristi privremenu GPS zonu prilaza 2, bez izmišljanja da je raniji 7 m radijus bezbedno kalibrisan. Google servisi nisu dirani.
- **Status:** sadržaj i konceptni resursi u Git kandidat grani; Web/Android/ARCore integracija, prava za realne slike, audio i stvarni GLB čekaju.

## 2026-10-08 — v1.1 editorial and production package

**Decision log** (data-only branch `content/sombor-portable-game-data-20261008`):

1. Kept existing 9 mission locations and original historical quiz tasks. Five AR interactions are now **active mission tasks**, one per GK, MKG, NPS, LK and EB; total **31** tasks.
2. GK is the reference production mission; the **historical quiz about 1842 remains**, with original answer order and preserved `legacyOriginal`. Ćelavi trg and Branković/Magistrat history are documented in mission narrative.
3. Set GK GPS approach-zone completion temporarily to zone 2 rather than forcing a noisy 7 m inner circle; final radius and polygon must be field-verified.
4. Created Professor Vlada mentor class and placeholders for local historical shadows/voices. The first mentor has prepared voice scripts, summon behavior and separate 2D and 3D asset requirements.
5. Formalized four collectible classes: unique per-location relic, repeatable golden 1749 seal (small sourced history bytes and limited score), romantic heart, and comedic joker. Created user-facing Herbarijum Sombora album specs and one-time reward keys.
6. Added original SVG concept files for AR target, tower, token categories and a temporary mentor portrait. No historical seal facsimile, 3D `glb`, voice file, or captured 2D fallback photo exists yet.
7. Created production brief, scene flow, asset manifest and acceptance checklist for GK.
8. For LK, the accepted AR mini-game orders the words **Santa / Maria / della Salute**; further poetic fragments may be explored editorially later.
9. No Unity application code, frontend, Android APK, API keys or external maps have been modified.

**Validated:** JSON cross-references, mission/waypoint/hint identifiers, task count, sample score rules, SVG basic syntax/availability on the GitHub branch. Validation is static, not an on-device AR test.

**Pending:** visual references and safe GPS location calibration, final 3D models/animations, sound production, original/permission-cleared facade photo, independent Web+Android+ARCore task runtime, accessible album UI, tests.

## 2026-10-08 — v1.0 extraction

Imported existing Unity ScriptableObject/YAML contents into portable JSON: 9 missions / 26 tasks, 13 GPS waypoints, 4 dialogue resources (one inactive test), 13 hint texts, 26 timeline registry blocks, 10 trigger groups, 18 conditions, and mechanics summary. All unedited historical sources remained in repository.
