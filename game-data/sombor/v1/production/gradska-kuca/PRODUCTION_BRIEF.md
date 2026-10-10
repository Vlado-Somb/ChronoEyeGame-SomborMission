# CHRONO EYE — SOMBOR | Produkcioni paket 01: Gradska kuća i Ćelavi trg

**Status:** usvojen u igri / sadržaj v1; 2D konceptualni aseti pripremljeni, 3D i audio za produkciju.  
**Misija:** `sombor-gk` · **GPS:** `SO_GK` · **Koraci:** `GK-01` → `GK-AR-01` → `GK-02`.  
**Podaci:** [misija](../../missions/gk.json) · [waypoints](../../waypoints.json) · [mentor](../../systems/characters.json) · [kolekcije](../../systems/collectibles.json) · [album](../../systems/album.json).

## Urednički koncept

**Prva lokacija je istovremeno i uvod u igru i prva istorijska lekcija.** AR nije zamena za istorijsko pitanje. Primenjujemo petlju: **GPS dolazak → narator/istorijski kontekst → AR traženje tornja → čitanje godine na fasadi → stvarno istorijsko pitanje → narativni završetak → jedinstveni token + album**.

Mesto je Trg Svetog Trojstva, koji stanovnici zovu Ćelavi trg. U 18. veku postojala je palata/kaštel somborskog kapetana Jovana Brankovića (1718); u 1749. mesto je prešlo u upotrebu gradske vlasti i Magistrata; dogradnjama je nastao neoklasicistički izgled Gradske kuće sa godinom **MDCCCXLII (1842)** u zabatu pročelja. Na trgu je stajao spomenik Svetom Trojstvu iz 1774, uklonjen 1947. godine. Taj izgubljeni spomenik je **budući opcioni AR „istorijski eho“**, nije preduslov misije.

### Izvori i istorijska pažnja

- [Turistička organizacija Vojvodine — Gradska kuća](https://vojvodina.travel/atrakcije/gradska-kuca-sombor/)
- [Turistička organizacija Vojvodine — Trg Svetog Trojstva](https://vojvodina.travel/atrakcije/trg-svetog-trojstva-sombor/)
- [Ravnoplov — istorija Gradske kuće](https://www.ravnoplov.rs/gradska-kuca-u-somboru/)
- [Grb i pečat iz 1749 — Ravnoplov](https://www.ravnoplov.rs/grb-sombora/)
- [Istorijski arhiv Sombor — Povelja](https://www.arhivsombor.org.rs/povelja/povela-slobodnog-kralevskog-grada)

Neka lokalna tumačenja drugačije opisuju naknadne dorade tornja: zato ne tvrdimo da je svaki njegov element nepromenjen od 1842. Kviz je o nastanku prepoznatljivog izgleda i o rimskim brojevima sa pročelja. **Staru istorijsku priču i pitanje nismo obrisali**; nalaze se u `missions/gk.json` i u `legacyOriginal` poljima, gde je izvršena urednička korekcija.

## Tok scena i tekstovi

| Događaj | Narativ / akcija | Ishod |
|---|---|---|
| `on_approach_gk` | Dobro došao na Ćelavi trg. Odavde se vidi Gradska kuća. | Uvod Profesora Vlade, najpre titl |
| `GK-01` | Stani na bezbedno mesto odakle vidiš fasadu; pročitaj uvodni pasus o istoriji trga | GPS dolazak potvrđen tolerancijom |
| `mentor_spawn` | Ding-ding, mentor stoji na **korisnički potvrđenoj** ravnoj površini, pokazuje prema tornju | Kratka animacija; može se pozvati ponovo |
| `GK-AR-01` | Usmeri kameru prema pročelju i pronađi toranj; zatim pronađi natpis iznad stubova | Tap nišana/konture; završena AR interakcija |
| `mentor_hint` | Pogledaj rimske brojeve — to nisu samo ukrasi | `HINTGK2` i po potrebi `HINTGK3` |
| `GK-02` | **Koje godine je Gradska kuća, nakon dogradnje, dobila prepoznatljivi današnji izgled?** | `1718.` / `1749.` / **`1842.`** |
| `on_success` | 1718 = Brankovićev kaštel; 1749 = Magistrat; 1842 = završetak velike dogradnje | Završni edukativni tekst |
| `reward` | Mentor pokazuje jedinstveni token sa tornjem, „Sakupljeno!“ | `gk-tower-relic` u Herbarijumu |
| `optional_pickup` | Zlatni pečati donose male priče o gradu | Odvojeni, ograničeni dodatni poeni |

### Izgovoreni tekst / titl mentora (SR)

- **Prvi susret**: „Dobro došao na Ćelavi trg! Vidiš Gradsku kuću? Na njenoj fasadi sakriven je broj koji vodi pravo u istoriju grada. Hajde prvo da pronađemo toranj, a onda da pročitamo ono što nam zgrada sama govori.“
- **Posle AR-a**: „Odlično! Sada potraži natpis iznad stubova. Nije običan ukras: rimski brojevi kriju važnu godinu.“
- **Završetak**: „Tačno, 1842! Video si da arhitektura nije samo lep dekor — ona čuva istorijske tragove. U album je stigao tvoj prvi veliki predmet Sombora!“

Tekstovi su u `missions/gk.json`; audio identifikatori i produkcijske specifikacije su u `asset-manifest.json`. Duplikat u izvornom globalnom uvodnom dijalogu ne brišemo ovim paketom, nego ga obeležavamo za kasniju redakciju.

## AR — šta zapravo radi i šta NE tvrdimo da radi

`GK-AR-01` je **guided overlay**, ne lažno obećanje da kamera automatski prepoznaje toranj. Kad je GPS blizu trga i igrač bezbedno stoji, telefon otvara kadar i AR konturu / nišan. Igrač ručno poravna kadar i tapne konturu tornja; potom vizuelni trag usmerava pogled prema stvarnom natpisu **MDCCCXLII**.

**Moguće buduće unapređenje** je prepoznavanje obeležja kroz referentne slike/geospatial anchors, posle terenskog testiranja. **Ne vezivati 3D mentora za nasleđenu Unity visinu od 90 m**. Mentor se postavlja na detektovanu podlogu ili ručno potvrđenu površinu, na pristojnom odstojanju.

Fallback: fotografija cele fasade sa interaktivnim tačkama „toranj“ i „natpis“. Isti cilj i ista nagrada; ne kažnjavati igrača zbog slabog AR tracking-a, mraka, GPS šuma ili nepodržanog telefona. Tek kada je ovo testirano na terenu, dozvoljeno je postepeno uključivanje boljeg vizuelnog trackinga.

## Sistem kolekcionabila i UX

1. **Glavni predmet** (1 po glavnoj misiji): `gk-tower-relic` — toranj Gradske kuće, neponovljiv, ulazi u „Velike tragove“.
2. **Zlatni pečati** — 1749 stilizacija gradskog pečata, ne zvanična reprodukcija; do 3 istovremeno, mini-priče, +2 poenа po registraciji, ograničeni dodatni poeni po danu.
3. **Srca** — ljubavne priče / poezija (npr. Laza), posebna galerija; ne postoje još na ovoj lokaciji.
4. **Džokeri** — šaljive izmišljene pojave, posebna boja i galerija; ne ubacivati izmišljene činjenice u istorijski kviz.

**Herbarijum Sombora** je stalno dostupan kao dugme „Moja zbirka“. Otvara kartice sa minijaturom, kratkom pričom, lokacijom, godinom otkrivanja i istorijskim izvorom. Zaključani glavni predmeti imaju siluetu. Uređaj pamti `unlockedCollectibleIds`, `pickedSpawnIds`, `scoreAwardsByKey`, `completedMissionIds`. **Ne dodeljivati ponovo bodove pri ponovnom otvaranju!**

## Likovi

Prvi AR mentor je **Profesor Vlada**: auto-pojava jednom kada je bezbedno, ponovni ručni priziv, animacije dolaska, pokazivanja, govora, nestajanja; sinhronizovani titlovi i zvuk koji se može isključiti. Mogućnost istorijskih likova/senki je **sistemski prihvaćena za kasnije**: npr. Milan Konjović, prolaznik iz druge epohe, lokalni glas. Svi takvi likovi jasno su predstavljeni kao umetnička rekonstrukcija, ne kao autentični snimci.

## Fizika i bezbednost prostora

- `SO_GK` je privremeni GPS centar; radijusi **75 m / 21 m / 7 m**, potvrda po **2 s**. Oni nisu tačnost koju obećavamo.
- Za dolazak je sada odabrana **zona prilaska (2)**, umesto ranije stroge zone (3), kako igrača ne bismo terali na fizički centar trga.
- Igra ne zahteva prelazak kolovoza, hodanje uz podignut telefon niti stajanje na nepristupačnoj tački.
- GPS otključava mogućnost scene, ali korisnik pokreće kameru samo kada miruje i potvrdi da je bezbedno.

## Definicija završenosti paketa

**Gotovo u podacima:** postoje istorijski narativ, odgovori, AR zadatak, mentor scena, kolekcionarski katalog, album, tekstovi za audio i originalni SVG vizuelni koncepti.  
**Za sledeći tehnički tok:** napraviti mentorov `GLB`, `GLB` eventualnog tokena, originalne zvučne fajlove, fotografiju za 2D fallback, završni tower target kalibrisan sa terena, AR/Web most, test telefon i pristupačni UI.

Ovaj dokument je **produkcioni ugovor sadržaja**, ne tvrdnja da je Android/AR kod već implementiran.
