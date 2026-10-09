# CHRONO EYE — SOMBOR | Produkcioni paket 05: Veljko Petrović — Golubica koja se vraća

**9. oktobar 2026.** Status: `approved_for_design`, SVG koncepti i podaci postoje, Android/ARCore, GLB, OGG, GPS i terenski test nisu obavljeni.  
**Waypoint:** `SO_VP` — **SPOMENIK VELJKU PETROVIĆU**, 45.772193708091905, 19.1161010794803. **Nije** misija Gradske biblioteke; spomenik se nalazi **ispred** Gradske biblioteke „Karlo Bijelicki“.  
**Tok:** VP-01 → VP-AR-01 → VP-02 → VP-03. Standard: `docs/mission-standard/MISSION_STANDARD_v0.2.md`. Original: `legacy-vp.json`.

## Dramaturgija: tri pisma i jedan povratak

Igra počinje malim, jasno izmišljenim misterioznim signalom: tri prazna pisma i šum krila, dok se na spomeniku nalazi golubica. Profesor Vlada poziva igrača da pomogne ptici da pronađe tri stanice u vremenu. **Poziv:** ko nosi priču? **Prag:** igrač se zaustavlja ispred spomenika. **Iskušenje:** 1884. Sombor, 1902. Budimpešta/Tekelijanum, 2017. Sombor. **Preokret:** 2017. nije povratak pisca, nego njegovog dela u javni prostor. **Dokaz:** postojeća pitanja o pripoveci i vajaru. **Nagrada:** simbolično Pero povratka. **Povratak:** igrač nosi priču u svoj Herbarijum.

Ovo nije niz trivijalnih klikova. AR redosled gradi razumevanje tri različite vrste događaja: rođenje, školovanje, javno sećanje. Završna pitanja vraćaju pažnju na stvarnu književnost i umetnika spomenika. Ne koristiti lažne citate pisca, autentična pisma ili izmišljenu istorijsku naraciju.

## Istorijska osnova

- **1884 i 1902.** Veljko Petrović rođen je u Somboru 1884, završio gimnaziju na mađarskom i od 1902. studirao pravo u Budimpešti, boraveći u Tekelijanumu. Izvor: Digitalizacija NS.
- **Naslov knjige.** „Golubica sa crnim srcem“ postoji kao Petrovićeva pripovetka u sadržaju izdanja *Pripovetke* (1963), a naslov je korišćen i za kasnije izbore pripovedaka. Ne svoditi bibliografiju na tvrdnju da je to njegova jedina/prva zbirka.
- **Spomenik.** Otkriven je 15. decembra 2017. ispred biblioteke „Karlo Bijelicki“; autor skulpture sa golubicom je Igor Šeter. Biblioteka je samo orijentir, mada ima i izdavačku vezu sa piščevim sabranim delima. Ovo ne otvara novu bibliotečku misiju.
- **Izvori:**
- Digitalizacija NS — biografija: https://www.digitalizacija.ns.rs/lat/veljko-petrovic/tab-os-informacije
- SOinfo — spomenik ispred biblioteke: https://www.soinfo.org/vesti/vest/19355/veljko-petrovic-sa-golubicom-ispred-gradske-biblioteke/
- 025.rs — otkrivanje 15.12.2017.: https://www.025.rs/otkriven-spomenik-veljku-petrovicu/
- Open Library — Pripovetke (1963): https://openlibrary.org/works/OL38429853W/Pripovetke
- Radio Apatin — skulpture Igora Šetera: https://www.radioapatin.com/kultura/od-17-aprila-izlozba-somborskog-vajara-igor-seter-18107

## Scenarij: Profesor Vlada

**Dolazak, VP-01:** „Pogledaj bronzanog pisca i golubicu. Čudno... na našem ekranu ptica nosi tri prazna pisma. Nisu to stvarna Petrovićeva pisma — već naša igra o tome kako priča putuje. Hoćeš li joj pomoći?“  
**Pre AR:** „Tri stanice: rodni grad, mesto studija i spomenik podignut mnogo kasnije. Složi kartice po godinama. Ne moraš nigde da ideš — samo mirno posmatraj.“  
**Pogrešan redosled:** „Golubica je skrenula na pogrešnu stranu vremena. Pogledaj godine i pokušaj ponovo, bez kazne.“  
**Preokret:** „Poslednja kartica nije povratak samog pisca, nego povratak njegove priče u javni prostor. Sada otkrij naslov pripovetke i ime vajara.“  
**Pred finale:** „Jedna pripovetka dala je ime knjizi, a jedan vajar dao je njenom simbolu mesto među prolaznicima. Ko čuva priču kad autora više nema?“  
**Završetak:** „Dobio si Pero povratka. Ono je naš izmišljeni relikt, ne Petrovićevo pero. Grad pamti kroz knjige, ljude i umetnost. Sačuvaj ovaj trag u Herbarijumu Sombora.“

Tekstovi su autorski, ne istorijski citati. Kanonski titlovi i skript: `missions/vp.json#narrative`. Reusable `profesor-vlada`: auto-once na bezbednom mestu, potom „Pozovi profesora“, mute, skip, titlovi i 2D portret. Ne uvoditi istorijski eho koji glumi glas Petrovića.

## AR/2D: Golubičin let

**Aktivacija:** posle VP-01, samo kada igrač miruje i potvrdi bezbedno mesto; privremena GPS zona 2 = **12 m**, potvrda 2 s, `fieldVerified=false`. Kamera je opciona, AR model postavlja se relativno prema kameri ili na površinu koju potvrdi korisnik; nema prepoznavanja statue, ulaska u biblioteku, kretanja sa podignutim telefonom, ni izmišljene visine iz Unity-a.

**Tri kartice:** `sombor_1884`, `budapest_1902`, `monument_2017`; početni raspored se meša, svaki izbor dodaje simbolično „pismo“. Prikazati godine i kratke činjenice, ne samo boje. Ispravan redosled emituje `AR_INTERACTION_COMPLETED` jednom; greška daje blag hint i ponavljanje **bez kazne**. Prelaz na postojeći kviz je obavezan.

**2D fallback:** identične tri kartice, isti redosled, isti tekst, isti događaj završetka i isti poeni. Funkcioniše bez kamere, ARCore-a, zvuka ili fotografije spomenika. Tastatura, čitač ekrana, veliki touch targeti.

**Obavezna izvorna pitanja:** VP-02: `GOLUBICA SA CRNIM SRCEM` (izvorni izbor 0), VP-03: `Igor Šeter` (izvorni izbor 1). Ponuđeni odgovori ostaju neizmenjeni. Originalne formulacije, narativi i neaktivna Unity polja su **u potpunosti sačuvani** u `legacy-vp.json`. VP-02 je samo bibliografski preciziran, VP-03 usmeren na konkretan spomenik.

## Nagrade i Herbarijum

- **`vp-feather-relic` — Pero povratka:** jedinstveni izmišljeni predmet; otključava se posle VP-03 i ne duplira misijskih +30.
- **`gold-vp-001` i `gold-vp-002`:** istorijske mikropriče o Tekelijanumu 1902. i spomeniku 2017, po +2, uz dnevni limit i jedinstveni `spawnId`.
- **`joker-vp-001`:** „Golubica bez karte“, jasno izmišljena šala, +1 jednom. **Bez srca** — ne forsirati romantični motiv.
- **Skor:** četiri glavna zadatka × 10 + 30 završetak = **70** pre eventualne −2 kazne na pogrešnom kviz odgovoru. AR pogrešan pokušaj = 0 kazne; 2D isti skor. Sačuvati `scoreAwardsByKey`, `pickedSpawnIds`, `unlockedCollectibleIds`, sprečiti ponovno nagrađivanje nakon GPS re-entry, AR replay ili reload-a.
- **Album:** „Moja zbirka“ → „Veliki tragovi“ → Pero povratka; prikaz izvora, datuma, lokacije, stanja i kratke priče; sve dostupno i bez kamere.

## Produkcija, prava i bezbednost

**Napravljeno:** pet originalnih SVG koncepta `assets/vp-*.svg`, kompletan narativ, AR/2D specifikacija, četiri produkciona dokumenta i originalna misija u arhivi. Grafike nisu kopije Šeterove skulpture ni istorijske fotografije Tekelijanuma.

**Planirano:** `models/profesor-vlada.glb`, opcioni `models/vp-symbolic-pigeon.glb`, četiri mentorska OGG govora, originalni zvuk krila, opciona WebP fotografija samo uz pravo korišćenja. Nema ARCore runtime-a, VPS sidra, terenskog GPS testa, izgrađenog Android paketa ni testiranog Herbarijuma. Svaki status u `asset-manifest.json`.

**Terenski acceptance:** pešačka zona 12 m, bez kolovoza i dodirivanja skulpture, noćni signal, odstupanje GPS-a, kamera odbijena, slabo praćenje, 2D ekvivalent, titlovi i trajnost bodova. Videti `QA_CHECKLIST.md`. Ne aktivirati Cloud/API ključeve.
