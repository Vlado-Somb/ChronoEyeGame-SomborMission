# CHRONO EYE — SOMBOR | Produkcioni paket 03: Muzej podunavskih Švaba — Kofer sećanja

**Datum:** 9. oktobar 2026. **Status:** `approved_for_design`, sadržaj i originalne SVG skice postoje; Web/Android/ARCore i audio/GLB nisu implementirani ili testirani.  
**Misija:** `sombor-psm` · **GPS:** `SO_PŠM` · **Tok:** `PSM-01` → `PSM-AR-01` → `PSM-02` → `PSM-03`.  
**Lokacija:** Grašalkovićeva palata, Sombor. Muzej je depandans Gradskog muzeja, ali nije isto što i zasebna misija `sombor-gms`.  
**Referenca:** `docs/mission-standard/MISSION_STANDARD_v0.2.md` i `PRODUCTION_PACKAGE_TEMPLATE_v0.2.md`.

## 1. Kićma priče: jedna kuća, mnogo glasova

Ovo nije „pronađi palatu i pogodi godinu“. Priča počinje zvukom **zamišljenog kofera**. Profesor Vlada podseća da su Dunavom stizale porodice koje su u Bačkoj tražile novi život. Njihove biografije ne smeju se svesti na jednu nacionalnu etiketu ili jedan period. Igrač prepoznaje tri simbolična traga: **putovanje**, **svakodnevicu**, **muzejsko sećanje**. Nakon slaganja, profesor otkriva važnu komplikaciju: istorija uključuje i stradanja, progone i gubitke. Na kraju igrač otkriva ime zgrade i godinu osnivanja muzeja, a nagrada je simbolični **Ključ sećanja**. On ne predstavlja stvarni muzejski eksponat.

**Dramaturški luk:** poziv (kofer) → prag (stara palata) → iskušenje (tri fragmenta vremena) → preokret (iza lepih ilustracija stoje i teške sudbine) → saznanje (palata, 2019) → povratak (ključ i pitanje „čije priče još nedostaju?“). Bez senzacionalizma i bez gamifikovanja ratnog stradanja.

### Istorijska osnova

1. **Doseljavanje u talasima.** U XVIII veku nemačko stanovništvo naseljavalo se u Bačku kroz više kolonizacionih talasa, počev od kraja četrdesetih godina. Dunav je bio važan put, a Sombor komorsko administrativno čvorište. „Podunavske Švabe“ je ustaljen istorijski naziv za nemačke zajednice u ovom prostoru, ne tvrdnja da su sve porodice iz jedne oblasti.
2. **Grašalkovićeva palata.** Barokna komorska upravna zgrada služila je i prihvatu i raspoređivanju doseljenika. Postoje **različiti datumi**: turistička stranica navodi 1763, dok lokalni istoričar navodi kamen temeljac 24. jula 1750. i postepenu gradnju. Ne biramo proizvoljno jednu godinu kao nesporan datum izgradnje.
3. **Svakodnevica i teški XX vek.** Muzejska zbirka predstavlja rad, zanate, porodice i višegeneracijski suživot, ali i ratne i posleratne događaje. Ne izjednačavati individualnu odgovornost i pripadnost zajednici; ne prikazivati samo „romantičnu kolonizaciju“, niti glumiti glas žrtava.
4. **Muzej.** Odlukom Skupštine grada Sombora **9. jula 2019.** osnovan je Muzej podunavskih Švaba kao muzejska jedinica Gradskog muzeja Sombor. Donacije potomaka i udruženja „Gerhard“ bile su važne za zbirku. Stalna postavka kombinuje autentične predmete i multimediju.

### Izvori i pouzdanost

- **Muzejski sadržaj / Heritage Guide** — [Stari zanati](https://myheritageguide.com/sr/object/stari-zanati-2/): 9. jul 2019, muzejska jedinica, sadržaj postavke, istorijski okvir.
- **Turistička organizacija Vojvodine** — [Grašalkovićeva palata](https://vojvodina.travel/atrakcije/grasalkoviceva-palata-sombor/): palata, imigracioni centar, turistički datum 1763.
- **Ravnoplov / Milan Stepanović** — [Grašalkovićeva palata](https://www.ravnoplov.rs/grasalkoviceva-palata-u-somboru-zgrada-iz-koje-je-naseljena-backa/): 1750 kamen temeljac, komorska uprava, kolonizacioni talasi.
- **Heritage Guide** — [Muzej podunavskih Švaba](https://myheritageguide.com/sr/tour/gradski-muzej-sombor-muzej-podunavskih-svaba/): depandans Gradskog muzeja, donacije i multimedijalna zbirka.
- **SOinfo, 2019** — [Muzej moli za pomoć građana](https://www.soinfo.org/vesti/vest/20947/muzej-moli-za-pomoc-gradjana/): javni poziv za porodičnu građu i muzejski kontekst.

**Ključna urednička korekcija:** izvorni Unity zadatak `PSM-03` tražio je „1751“ kao godinu početka doseljavanja Podunavskih Švaba u Sombor. To nije jedinstven istorijski događaj potvrđen navedenim izvorima; kolonizacija je trajala u talasima. Zato je originalno pitanje **sačuvano u `legacyOriginal`** sa originalnim odgovorom 1751, a **aktivni zadatak pita proverenu godinu osnivanja muzeja — 2019**. Ovo je sadržinska korekcija, ne skriveno brisanje. `PSM-02` zadržava tačan odgovor **Grašalkovićeva palata** (indeks 1). Stara `unusedUnityFields.waypointId=SO_GK` ne sme se koristiti kao aktivni GPS.

## 2. Scenarij i dijalozi

| Korak | Događaj / sadržaj | Rezultat |
|---|---|---|
| 1 | `PSM-01`: sa bezbednog javnog pešačkog mesta pogledaj Grašalkovićevu palatu; ne moraš ulaziti | GPS zona prilaza 2, privremeno 15 m / 2 s |
| 2 | Profesor Vlada: „Kao da je neko spustio stari kofer pred palatu...“ | uvodni titl / planirani glas |
| 3 | `PSM-AR-01`: tri ilustracije redom **DOLAZAK → SVAKODNEVICA → SEĆANJE** | `AR_INTERACTION_COMPLETED`; isti 2D zadatak |
| 4 | Mentor: „Ali istorija nije samo uredan niz lepih slika...“ | obavezni istorijski kontekst; bez trivijalizacije |
| 5 | `PSM-02`: Kako se zove palata? | **Grašalkovićeva** |
| 6 | `PSM-03`: Koje godine je osnovan muzej? | **2019** |
| 7 | „Ključ sećanja“ → „Moja zbirka“ | jedinstveni relic; opcioni mikroistorijski pečati |

**Govor na dolasku:** „Kao da je neko spustio stari kofer pred palatu. To je samo naša zamišljena priča, ali putovanja su ovde bila stvarna. Danas ova zgrada čuva mnogo glasova. Hoćeš li da pratiš njihove tragove?“

**Pre AR-a:** „Tri kartice: putovanje Dunavom, svakodnevni život i muzej. Složi ih redom. To su naši simboli, a ne pravi predmeti neke porodice.“

**Posle AR-a:** „Bravo. Ali istorija nije samo uredan niz lepih slika. Postoje i teške priče o ratovima i gubicima. Muzej nas uči da ih čitamo pažljivo. Sada proveri ime palate i godinu osnivanja muzeja.“

**Kraj:** „Dobio si Ključ sećanja. On ne otvara prava vrata, već novo pitanje: čije priče znamo, a čije tek treba saslušati?“

**Jedan kanonski izvor za titlove:** `missions/psm.json#narrative`. Tekstovi su autorski i nisu navodni citati istorijskih ljudi.

## 3. AR scena „Kofer sećanja“

- **Aktivacija:** `PSM-01` završen, igrač miruje, ručno pritiska „Pokreni AR“. Nema obaveznog ulaska u muzej, radnog vremena, skeniranja eksponata ili hodanja sa telefonom.
- **Prikaz:** tri originalne SVG kartice kao camera-facing bilbordi ili na korisnički potvrđenoj bezbednoj ravni. Svaka ima naziv i vremenski okvir, ne oslanja se samo na boju.
- **Interakcija:** kartice se pojavljuju u promenljivom rasporedu; igrač bira stabilne ID-jeve `arrival`, `daily_life`, `memory`. Ispravan izbor ostavlja kratki vizuelni trag. Pogrešan izbor daje blag hint, ponavljanje bez AR kazne. Posle tri tačna izbora prikazuje se poruka o ograničenjima pojednostavljene vremenske slike i nastavlja stvarni kviz.
- **Fallback:** identičan zadatak u 2D, bez kamere ili ARCore, sa istim uslovom završetka, osnovnim poenima i sadržajem. Nema automatskog prepoznavanja fasade, istorijskih predmeta ili ljudskih lica.
- **Scenski ton:** ne „otključavati“ logore, stradanje ili gubitke kao kolekcionarski bonus; ta objašnjenja su **obavezan narativ** posle AR-a.

## 4. Mentor, likovi, zvuk

**Profesor Vlada** koristi zajednički `profesor-vlada` model (GLB još nije napravljen), 2D portret, automatski uvod jednom i dugme za ponovno pozivanje. Obavezni su titlovi, mute i skip. Nema posebnog istorijskog eho-lika: izmišljeni „glas doseljenika“ ili imitacija svedoka lako bi delovali kao falsifikovan izvor. Ako se doda putnički kofer, to je **simbolički rekvizit**, ne autentičan predmet. Zvuk: kratak ambijentalni šum vode i diskretno otvaranje kofera, oba originalna i opciona. Nema pozajmljenih arhivskih snimaka bez prava.

## 5. Kolekcija / Herbarijum

- **`psm-memory-key-relic` — Ključ sećanja:** jedinstveni glavni predmet po završetku `PSM-03`, bez drugog dodeljivanja +30 misijskih poena.
- **`gold-psm-001`:** palata kao komorska uprava i prihvatni centar; izvor Turistička organizacija Vojvodine.
- **`gold-psm-002`:** osnivanje muzeja 9. jula 2019; izvor muzejska objava na Heritage Guide.
- **Nema srca ni džokera u ovom paketu.** Ne forsirati sve četiri kategorije tamo gde je tema osetljiva. Herbarijum i даље приказује глобалне четири категорије; бонуси су опциони, без дуплирања бодова и без опасних GPS spawn места.

`scoreAwardsByKey` + `pickedSpawnIds` čuvaju jedinstvenost, a mali pečati koriste postojeći globalni limit. Kartica relic-a prikazuje naziv, mesto, datum otkrića, kratku istorijsku priču, izvor i status otključavanja.

## 6. Vizuelna i tehnička produkcija

**Napravljeno:** četiri originalna SVG-a (`psm-danube-voyage.svg`, `psm-workshop.svg`, `psm-museum-memory.svg`, `psm-memory-key-relic.svg`); zajednički reticle i gold-seal SVG postoje. Grafike su simbolične, nisu rekonstrukcija porodične imovine ili stvarnih muzejskih predmeta.

**Planirano, ne napravljeno:** GLB kofer (ako se prihvati), zajednički GLB mentora, originalni OGG zvukovi i četiri mentorska govora, fotografija fasade samo uz proveru prava. Detalji i statusi u `asset-manifest.json`.

**GPS:** `SO_PŠM` iz `waypoints.json`, WGS84, originalni radijusi 50/15/7 m, `fieldVerified:false`. Aktivna misija privremeno traži zonu 2 (prilaz) umesto 7 m uz zgradu. Koordinate i pristup treba proveriti na terenu, bez navođenja na kolovoz ili privatni ulaz. Legacy visina 90 m nije AR geospatial kalibracija.

## 7. QA i granice

Pre release-a proći `QA_CHECKLIST.md`: izvori, legacy pitanje 1751, stabilni ID-jevi, GPS bezbednost, 2D ekvivalencija, pristupačnost, bezbedan prikaz, jednokratno bodovanje, nema istorijskog falsifikata. Nisu menjani `SERVICE_REGISTRY`, API konfiguracija ni Cloud. **Ne tvrditi da je paket testiran u Android/ARCore-u.**
