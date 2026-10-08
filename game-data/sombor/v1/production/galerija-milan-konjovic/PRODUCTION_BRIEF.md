# CHRONO EYE — SOMBOR | Produkcioni paket 02: Milan Konjović, genije boja

**Status:** urednički pripremljeno / prihvaćeno za dizajn, neizvedeno u Android/ARCore runtime-u.  
**Misija:** `sombor-mkg` · **WayPoint:** `SO_MKG` · **Lokacija:** Trg Svetog Trojstva 2, Sombor.  
**Tok:** `MKG-01` → `MKG-AR-01` → `MKG-02` → `MKG-03`.  
**Prateći podaci:** `missions/mkg.json`, `systems/characters.json`, `systems/collectibles.json`, `systems/album.json`.  
**Izvor:** originalni Unity MKG MissionSO u `source` polju misije, originalni tekstovi u `legacyOriginal`.

## 1. Osnovni dramaturški princip

U Gradskoj kući igrač je otkrio istorijsku godinu u kamenu. U galeriji mu se otkriva **drugačiji trag: istorija kroz boju**. Bez ulaska u galeriju (igra ne zavisi od radnog vremena ili trenutne izložbe), igrač otkriva zašto je umetnik sa evropskim iskustvom svom rodnom gradu ostavio jednu od najvažnijih somborskih umetničkih zbirki. AR doprinosi pogledu na umetnost, ali **ne zamenjuje** pitanja „kojom umetnošću se bavio?“ i „kada je rođen?“.

Urednički naslov *Genije boja* je naša dramaturška formulacija, a **ne doslovan dokumentovani umetnikov nadimak**.

### Istorijski narativ

1. Milan Konjović rođen je 28. januara 1898. u Somboru. Još kao gimnazijalac izlagao je 1914. Studirao je u Pragu i Beču, u Parizu boravio 1924–1932, potom se posvetio motivima Sombora, Vojvodine i mediteranskog podneblja.
2. Njegov opus nije jedinstvena fiksna paleta: plava faza (1929–1933), crvena (1934–1940), siva (1945–1952) i kasnija koloristička i asocijativna razdoblja predstavljaju umetničku evoluciju. Didaktičke AR nijanse su **izvorne ilustracije, ne kopije slika niti kriterijum za identifikovanje svakog dela**.
3. Galerija je javnosti otvorena 10. septembra 1966, na osnovu prvobitnog umetnikovog legata od 500 radova. Umeće se ne završava životom autora — ona ostaje gradu.

### Proverljivi izvori

- [Srpska enciklopedija — Galerija „Milan Konjović”](https://srpskaenciklopedija.rs/books/slovo-g/page/galerija-milan-konjovic): legat, osnivanje i dar.
- [Turistička organizacija Sombora — Galerija](https://visitsombor.org/ponuda/id56/culture/art-studios-and-ateliers/gallery-milan-konjovic.html): adresa, 10. septembar 1966, početni legat 500.
- [Enciklopedija Srpskog narodnog pozorišta — Milan Konjović](https://snp.org.rs/enciklopedija/?p=6247): 1898–1993, rani radovi i evropski put.
- [Heritage Guide / Galerija Konjović — Red Phase](https://myheritageguide.com/en/object/the-red-phase/): 1934–1940, odnos toplih boja i crnih kontura.
- [Galerija21 — umetnik](https://galerija21.com/artist/milan-konjovic/): periodizacija plava/crvena/siva.

**Urednička rezerva:** izvori razlikuju datum ideje, pravnog osnivanja, ugovora o poklonu i otvaranja. U reprodukovanoj priči ne mešati ove pojmove; na fasadi ne pretpostavljati natpis o 1966. Broj dela u kolekciji raste — izbegavati tvrdnju o tačnom sadašnjem ukupnom broju. Originalna referenca na porodicu sa prezimenom „Konjević“ u Unity narativu je najverovatnije omaška; original sačuvan u `legacyOriginal`, a ne prepisan u definitivnu priču.

## 2. Scenarij: šest narativnih otkucaja

| Korak | Tok priče i ekrana | Rezultat |
|---|---|---|
| 1. `MKG-01` | Na bezbednoj pešačkoj površini igrač pronalazi galeriju; kratka istorijska poruka otvara priču o slikaru i poklonu gradu | GPS approach zona (privremeno 15 m / 2 s) |
| 2. Profesor Vlada | „U ovoj kući nisu sakrivene samo slike nego i jedan neobičan jezik — jezik boja.“ Kratak „ding“ i avatar/potpisi | Narativ, preskakanje, ponovno pozivanje |
| 3. `MKG-AR-01` | Na kameri lebde plavi, crveni i sivi originalni potezi četkicom; igrač tapne **plava → crvena → siva**, prema fazama umetnika | `AR_INTERACTION_COMPLETED`; identično 2D slaganje |
| 4. Istorijski eho (opciono) | Prozirna **simbolička** figura slikara kratko načini potez i nestane; natpis „Istorijska interpretacija“ | Atmosfera, bez biografske tvrdnje i bez blokiranja zadatka |
| 5. `MKG-02` | „U kojoj umetnosti se proslavio Milan Konjović?“ — **slikarstvo**; profesor dopunjava priču o Pragu, Beču i Parizu | Stvarni edukativni kviz |
| 6. `MKG-03` | „Koje je godine rođen u Somboru?“ — **1898**; završna priča o prvoj izložbi i legatu | Misija završena; glavna paleta u Herbarijumu |

Opcionalni zlatni pečati otkrivaju dve kratke činjenice (otvaranje 1966; prvih 500 radova), a **„Pobegla mrlja“** je izmišljena komična kap boje koja pobegne sa palete. Ovi predmeti nisu obavezni; ni jedan ne sme omogućiti preskakanje stvarnog kviza.

## 3. Govor / titlovi mentora — autorstvo scenarija

**Po dolasku:** „Dobro došao pred galeriju Milana Konjovića! U ovoj kući nisu sakrivene samo slike nego i jedan neobičan jezik — jezik boja. Ovaj Somborac je putovao daleko, ali mu se rodni grad stalno vraćao na platna. Hoćeš li da pratiš tragove njegove palete?“

**Pre AR-a:** „Pogledaj: plava, crvena i siva! To nisu sve njegove boje, ali označavaju tri važne slikarske faze. Dodirni ih redosledom kojim su se smenjivale kroz vreme.“

**Posle AR-a:** „Bravo! Plava faza dolazi pre crvene, a zatim sledi siva. Umetnik je kasnije iznova oslobađao boje i njihove mogućnosti. A znaš li ko je bio Milan Konjović i kada je rođen?“

**Kraj:** „Odlično! Nisi samo pronašao galeriju — otkrio si umetnika koji je Somboru ostavio čitav jedan svet slika. Tvoja paleta sada ima svoje mesto u Herbarijumu Sombora!“

Sva četiri teksta su jedan kanonski izvor u `missions/mkg.json#narrative`. Zvuk će se napraviti naknadno, uz titlove; bez izmišljanja Konjovićevih istorijskih rečenica i bez imitacije „autentičnog snimka“.

## 4. AR iskustvo „Tri epohe“, precizno

**Mesto:** odmah po bezbednom GPS dolasku; kameru igrač uključuje sam, dok stoji. Nije potrebna bezbednosno problematična 3D koordinata iz stare Unity nadmorske visine.

**Prikaz:** tri kartice sa originalnim apstraktnim potezima (plava, crvena, siva) lebde kao camera-facing billboards ili stoje na ručno potvrđenoj ravni. Svaka ima čitljiv naziv i godine; igra nije zasnovana samo na razlikovanju boja (pristupačnost).

**Interakcija:** prikazati kartice nasumičnim položajem, ali proveravati stabilne ID-jeve `blue`, `red`, `grey` iz `ar.content.correctSequence`. Nakon svakog ispravnog tapa karta ostavlja „trag“ boje; pogrešan tap prikaže diskretnu poruku i omogući ponovni pokušaj bez kazne. Posle tri pravilna koraka potezi se stapaju u simboličku paletu, svira kratak signal i nastavlja se istorijski ABC zadatak.

**Ishod / fallback:** ako ARCore, kamera, tracking ili svetlo ne odgovaraju, ista tri objekta su u 2D ekranu i igrač ih bira istim redom; isti bodovi, isto otključavanje. Nema lažnog prepoznavanja umetničkih dela na kameri — ovo je didaktička scena, ne AI klasifikator slika.

**Buduće proširenje:** korisnički postavljen slikarov eho model u razmeri prilagođenoj prostoru ili samo grafička silueta; a zatim, uz dozvolu galerije/autorska prava, licencirana reprodukcija stvarnog dela koja bi mogla omogućiti novu vrstu zadatka. To **nije uslov ove verzije**.

## 5. Kolekcionarski sloj / album

- **Glavna nagrada:** `mkg-palette-relic` — Paleta genija boja; jedinstveni predmet, pojavljuje se po `MKG-03` uspehu, ne daje dupli mission bonus.
- **Zlatni pečat 1:** `gold-mkg-001` — datum otvaranja galerije (1966); +2 poena jednom po spawn ID, uz dnevni limit.
- **Zlatni pečat 2:** `gold-mkg-002` — poklon od 500 radova; +2 poena jednom.
- **Džoker:** `joker-mkg-001` „Pobegla mrlja“, +1 poen prvi put; fikcionalna igra, jasno odvojena od istorije.
- **Srce:** u sistemu već postoji za druga mesta; ovoj lokaciji ga još ne dodeljujemo bez posebne narativne odluke.

U Herbarijumu predmet sadrži ime, galerijski kontekst, datum otkrića, mali istorijski tekst sa izvorom i evidenciju da je nagrada već obračunata; kada je predmet zaključan, pokazati siluetu, ne rešenje.

## 6. Asset režim

**Stvarno napravljeno (originalan SVG izvor):** 3 fazne karte, paleta-relic, dekorativni trag boje, apstraktna silueta slikara. Zajednički `ar-focus-reticle.svg`, `gold-city-seal.svg`, `joker-token.svg` već postoje.

**Čeka izradu:** `glb` model slikarskog eha, animacija četkice, snimljen govor P. Vlade, „brush“ i „success“ zvukovi, autorski čista fotografija fasade (opciono 2D prikaz galeriје), Android/Web/ARCore loader i mapa. Postojeće slike Konjovićevih umetničkih dela su **istraživački izvori, ne naš asset**; reprodukcije se ne koriste bez dozvole.

U svim registrima jasno razdvajati `created_concept`, `planned_not_created`, `script_ready_audio_pending`, `integration_pending` i `verified_in_test`.

## 7. GPS, mapa, bezbednost

Postojeći waypoint `SO_MKG` ima WGS84 koordinate i eksperimentalne radijuse **50 / 15 / 7 m**, potvrdu **2 s** po zoni; `fieldVerified:false`. Kao i GK, misija privremeno koristi `requiredZone:2` (prilaz, ne automatski 7 m uz fasadu), jer stajanje preko puta ulice može već biti potpuno dobar pogled. Ne prebacivati AR kartice na postojeću `legacyAltitudeMeters:90` kao geospatial visinu. Stajanje na kolovozu, jurnjava za tokenima i hodanje sa podignutim telefonom su zabranjeni scenariji.

## 8. Prihvatanje produkcionog paketa

Urednički scenario, originalni asset koncepti, opis interakcije, nagrade, audio tekstovi i dokumentovani izvori čine **pripremljen produkcioni paket**. Nije napravljena Android AR implementacija; ona mora naknadno da prođe [QA_CHECKLIST.md](QA_CHECKLIST.md).

**Povezani standard:** [Mission Building Standard v0.2](https://github.com/Vlado-Somb/ChronoEyeGame-SomborMission/blob/main/docs/mission-standard/MISSION_STANDARD_v0.2.md). Povratna iskustva iz stvarnog testa upisuju se u `PRACTICE_LOG.md`; nove nedokazane ideje ostaju `accepted_for_design`.
