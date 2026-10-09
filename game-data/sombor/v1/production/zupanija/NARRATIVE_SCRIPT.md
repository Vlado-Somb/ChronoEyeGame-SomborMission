# NARRATIVE SCRIPT — Županija / Tri sata jedne slike

**Status:** originalni scenario za budući Web/AR/2D runtime; ne predstavlja snimljen glas ili testiranu AR scenu. Svi istorijski iskazi proveriti uz PRODUCTION_BRIEF.md.

## Akt I — Poziv (GPS → mentor)

**Vizuelni efekat:** mali simbol Chrono Eye treperi tri puta; preko njega se pojave tri providna časovnika. Ne prikazivati ih kao deo stvarne fasade, niti tvrditi da ih kamera prepoznaje.

**Profesor Vlada:** „Neobično. Satovi obično kasne ili žure, ali ova tri pokazuju tri različita vremena odjednom. Vidiš li ovu zgradu? Njena istorija počela je mnogo kasnije od bitke koju čuva na slici.“

**Igrač:** bira „Šta se dogodilo?“ ili „Pokaži trag“. Prvi izbor otvara kratak kontekst o 1786, 1805–1808. i 1882; drugi prikazuje tri kartice. Obe putanje vode u isti zadatak i ne daju duple poene.

## Akt II — Ispit: tri vrste vremena

**Kartica 1:** „1697 — događaj“. Stilizovane zastavice i linija reke, bez ratne scene.  
**Kartica 2:** „1896 — slika“. Prazan slikarski okvir, bez reprodukcije Ajzenhutovog dela.  
**Kartica 3:** „1898 — sala“. Simbolična galerijska prostorija, ne rekonstrukcija enterijera.

**Profesor:** „Pogledaj pažljivo: da li je slika isto što i bitka? Da li je datum nastanka slike isto što i datum kada je postavljena ovde?“

**Pogrešan potez, prvi put:** „Jedno je događaj, drugo umetnički prikaz. Potraži mesto koje govori o slici.“  
**Pogrešan potez, drugi put:** označiti dve kategorije koje se mešaju, bez automatskog rešavanja.  
**Treći pokušaj:** dugme „Objasni mi razliku“, pristupačno i u 2D; ne oduzima poene.

**Uspeh:** tri sata se razdvoje; čuje se zamišljeni meki ton (OGG pending), prikazuju se datumi i izvor. Centralni motor beleži `ZUP-AR-01` jednom.

## Akt III — Lažna pobeda i tiši trag

**Profesor:** „Bravo! Događaj je 1697, platno nastaje 1896, a ovde stiže 1898. Ali još ne znamo kako se zove.“  
**Kviz ZUP-02:** igrač unosi „Bitka kod Sente“. Bez ulaska u zgradu ili fotografisanja slike.

Po prihvaćenom odgovoru tri sata nestaju. Pojavljuje se nenametljiv tekst: „Jedna zgrada može čuvati i teška sećanja.“ Nema mračnih zvučnih efekata ili šok-slike.

**Profesor:** „Godine 1941. u ovoj zgradi se odlučivalo i o sudbini mladog čoveka. Ime ne sme da se izgubi iza datuma.“

**Kviz ZUP-03:** originalna četiri odgovora, tačan indeks 0. Nakon izbora prikazati mirnu istorijsku belešku, bez rekonstrukcije događaja.

## Akt IV — Povratak

**Profesor:** „Znaš sada tri načina na koja prošlost dolazi do nas: kroz ono što se dogodilo, kroz ono što je neko naslikao i kroz ono što grad odluči da sačuva.“

Na ekranu se otključava simbolični **Ključ tri vremena**. Kartica u Herbarijumu navodi tri datuma, kratko objašnjenje i izvore. Dva istorijska pečata (1786 / 1882) su opciona i dostupna i u 2D; ne povezivati nagradu sa događajima 1941.

## Tehničke režijske zabrane

Bez hodanja s kamerom, prepoznavanja slike, geospatial sidra bez terenskog testa, ulaska u zgradu, lažnog istorijskog glasa ili tvrdnje da je proizveden OGG/GLB. U oba moda isti card IDs, validacija, feedback, retry, poeni i centralni event.
