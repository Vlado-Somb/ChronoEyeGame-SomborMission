# CHRONO EYE SOMBOR — Muzej podunavskih Švaba | QA / Acceptance v1

**Datum:** 9.10.2026 · **Stanje:** urednički i grafički koncepti postoje, terenski i Android testovi NISU izvedeni.

## Istorijski sadržaj i etika
- [ ] Potvrditi da je Muzej podunavskih Švaba depandans Gradskog muzeja Sombor i da je osnovan odlukom grada **9. jula 2019** (muzejski tekst na Heritage Guide).
- [ ] Grašalkovićeva palata ispravno prepoznata; izvorni ABC odgovor **indeks 1** ostaje, ispravljen samo jezik pitanja.
- [ ] Izvorni `PSM-03` sa odgovorom **1751** sačuvan u `legacyOriginal`; aktivni odgovor **2019**. Nikada ne predstaviti 1751 kao pouzdano jedinstveno „prvo doseljavanje“.
- [ ] Zabeležiti razliku izvora o zgradi: **1750** kamen temeljac (Ravnoplov) naspram **1763** u turističkom opisu. Ne praviti kviz iz spornih datuma.
- [ ] Priča objašnjava nemačko doseljavanje u talasima i raznolikost porekla.
- [ ] Postoji uravnotežen, dostojanstven pasus o ratu, stradanjima i posleratnom kažnjavanju; bez poistovećivanja svih pripadnika zajednice sa krivicom pojedinaca.
- [ ] Nema lažnih porodičnih pisama, arhivskih slika, lažnih autentičnih glasova ili komičnih bonus predmeta vezanih za stradanje.
- [ ] Proveriti izvorne linkove i licence pre javne objave.

## Tok / AR / 2D
- [ ] `PSM-01` (GPS) → `PSM-AR-01` (AR/2D) → `PSM-02` (palata) → `PSM-03` (2019).
- [ ] Kartice nose tekstualne oznake i vremenske periode; redosled stabilnih ID-jeva: `arrival` → `daily_life` → `memory`.
- [ ] AR scene su simbolične ilustracije, a ne prepoznavanje muzejske fasade, lica ili istorijskih predmeta.
- [ ] Nepravilan izbor AR kartice ima retry bez AR kazne; pogrešni istorijski odgovori po pravilima zajedničkog engine-a.
- [ ] Nakon AR-a prikazuje se **obavezan** istorijski kontekst pre pitanja.
- [ ] ARCore nije podržan / nema dozvole / loš tracking / korisnik bira 2D → potpuno isti zadatak, bodovi i nagrada.
- [ ] Back, pause, resume i ponovno otvaranje ne dodeljuju bodove dvaput.

## Lokacija / sigurnost
- [ ] `SO_PŠM` koordinate na terenu proverene, **fieldVerified=false** dok se ne testira.
- [ ] `approachMeters=15`, `requiredZone=2` su privremeni; izmeriti pešačku bezbednu tačku i GPS šum.
- [ ] Ne terati igrača u ulicu, ulaz, unutrašnjost muzeja, dvorište ili privatni prostor.
- [ ] Kamera se uključuje samo po korisnikovoj radnji dok stoji.
- [ ] Stara `legacyAltitudeMeters=90` nije korišćena kao ARCore visina.
- [ ] Pristupačni veliki tap-targeti, titl, mute i ručno prizivanje mentora.

## Lik / kolekcija
- [ ] Profesor Vlada ima titlove i 2D portret, audio/GLB status jasno označen kao pending.
- [ ] Nema istorijskog eho-lika bez posebno odobrenog i dokumentovanog autorskog koncepta.
- [ ] `psm-memory-key-relic` dobija se **jednom** po završenom `PSM-03`, bez duplog mission bonusa.
- [ ] `gold-psm-001`, `gold-psm-002` su opcioni, izvori navedeni, primenjuju se globalni dnevni limit i `pickedSpawnIds`.
- [ ] Album „Moja zbirka“ pokazuje relic i dva pečata sa nazivom, opisom, izvorom i vremenom otkrivanja.

## Produkcioni status
- [x] Originalne četiri SVG ilustracije kreirane i u registru.
- [x] JSON tok, izvorni istorijski kviz, 2D fallback i kolekcija uređeni kao produkcioni sadržaj.
- [ ] Audio snimci / GLB / licencirana fasadna fotografija / stvarna animacija.
- [ ] Web UI → Android GPS → ARCore/2D → Web kviz → trajno stanje → Herbarijum: stvarni integracioni test.
- [ ] Stvarni teren, dan/noć, uređaji i logovi upisani u `docs/mission-standard/PRACTICE_LOG.md` tek posle testiranja.
- [ ] Bez novih Cloud servisa i tajni u Git-u.

**Uredničko prihvatanje nije isto što i `verified_in_test`.**
