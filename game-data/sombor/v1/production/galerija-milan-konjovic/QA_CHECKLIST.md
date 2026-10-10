# CHRONO EYE SOMBOR — Galerija Milan Konjović | QA / Acceptance v1

## Uredništvo / istorija
- [ ] Potvrđen Milan Konjović, rođen 28. januara **1898.**, Sombor; prve učeničke slike izložene 1914.
- [ ] Boravak u Parizu **1924–1932** istorijski izvorno objašnjen.
- [ ] Plava 1929–1933, crvena 1934–1940, siva 1945–1952 označavaju **faze**, a ne definitivne jedine boje njegovih platana.
- [ ] Galerija otvorena **10. septembra 1966** i osnovni poklon **500** dela potvrđeni; ne mešati osnivanje, ugovor i javno otvaranje.
- [ ] Očuvana originalna pitanja: slikarstvo i **1898**, sa originalnim odgovorima u `legacyOriginal`.
- [ ] U novoj naraciji ne ponavlja se izvorni tipfeler „David Konjević“ ili nekritički nadimak „Poslednji fovista“.
- [ ] Svaka istorijska micro-story kartica ima autoritativan izvor; ne kopirati stvarne slike bez dozvole.

## AR / fallback
- [ ] Proximity **zone2** omogućava stajanje sa javne bezbedne pešačke strane bez ulaska u galeriju; GPS uslov nije konačno kalibrisan.
- [ ] `MKG-AR-01` je odmah posle GPS, ali kamera se otvara isključivo na korisnikovu radnju.
- [ ] Vidljive kartice BLUE/RED/GREY sa **nazivima i periodima**, ne samo bojom.
- [ ] Igrač bira hronološki **blue → red → grey**. Nepravilno ne oduzima bodove pre mini-igre.
- [ ] AR mode koristi billboard/manual surface, ne obećava CV prepoznavanje Konjovićevih dela.
- [ ] 2D fallback identičan sadržaj, događaj i broj bodova.
- [ ] Nakon AR-a automatski slede prava istorijska pitanja, ne zamena „koju boju si video“.

## Mentor / lik
- [ ] Profesor Vlada pojavljivanje jednom, ponovni ručni poziv, titl i mute; nema blokiranja kamere.
- [ ] `milan-konjovic-echo` je opcion, skromna silueta sa eksplicitnom oznakom „Istorijska interpretacija“; nikada ne glumi pravi istorijski glas.
- [ ] Ako GLB ili voice ne postoje, aplikacija koristi 2D portret/titl i normalno nastavlja zadatke.

## Album / bonusi
- [ ] `mkg-palette-relic` tačno jednom posle `MKG-03`.
- [ ] `gold-mkg-001` i `gold-mkg-002` imaju mikropriče, ograničene poene i idempotentne `spawnIds`.
- [ ] `joker-mkg-001` je označen kao igra/fikcija, može se uzeti ali se ne traži za misiju.
- [ ] Herbarijum ima prikaz glavnog predmeta/bonus kartica i potpunu kontrolu duplog bodovanja.

## Produkcija / uređaj
- [x] Originalni SVG izvori: tri fazne karte, paleta-token, brush trail i stilizovana silueta.
- [ ] GLB model mentora/eha, caption/audio paketi, licence i prava.
- [ ] Integracioni test Web → GPS → native ARCore/2D → Web → 2 pitanja → album/persist.
- [ ] Android uređaj, build, GPS accuracy, svetlo i uslovi dokumentovani u Practice Log-u.
- [ ] Režim za slab vid/rastojanje/boje i sistemski back/resume.
- [ ] Nikada ne navoditi korisnika na kolovoz ili na hodanje sa kamerom.

**Status:** editorijalno pripremljeno i konceptni SVG resursi postoje; Web/Android/ARCore implementacija i terenska potvrda NISU obavljene.
