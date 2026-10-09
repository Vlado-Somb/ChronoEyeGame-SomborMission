# CHRONO EYE — Ernest Bošnjak: Nedosanjani kadar

**Misija:** `sombor-eb` · **Waypoint:** `SO_EB` (45.772966468741664, 19.11547583884617) · **Status:** editorial production and SVG concepts; device runtime pending.

## Dramaturgija
**Poziv:** Profesor Vlada pronalazi zamišljeni projektor kome nedostaju tri kadra. Igrač je montažer koji mora da ih složi. Zvuk projektora i filmska traka su autorski scenski motivi, ne istorijski zapis.

**Prag:** Igrač dolazi do spomenika, ali ostaje na javnoj pešačkoj površini. Aktivna zona 2 / 12 m je privremena i zahteva terensku proveru; ne ulazi se u objekat i ne prelazi kolovoz.

**Igra:** tri originalne simbolične kartice — povratak s projektorom (1906), početak snimanja kamerom (1909), plan filmskog studija koji rat prekida (1914). Igrač ih poređa hronološki, zatim pokreće petosekundnu stilizovanu vektorsku animaciju. Nema arhivskih kadrova, automatskog prepoznavanja spomenika ni istorijskog glasa.

**2D:** iste tri kartice, isti redosled i uslov uspeha, iste informacije, ista nagrada i isti broj pokušaja; kamera nije potrebna. Pogrešan redosled daje pomoć bez kazne.

**Provera znanja:** sačuvani Unity zadaci `EB-02` (reditelj, izbor 2) i `EB-03` (Holivud). Potpun original, uključujući neaktivna Unity polja, sačuvan je u `legacy-eb.json`.

**Preokret:** Filmski studio nije ostvaren. Ipak, ideja se vraća u pozorišnoj predstavi `Kad bi Sombor bio Holivud` (premijera 17. februara 2018). Finale otključava simbolični relikt `Traka nedosanjanog filma` u Herbarijumu Sombora.

## Istorijski okvir i uredničke rezerve
Bošnjak se vratio 1906. sa projektorom; godina prvih javnih projekcija nije jednako datirana u svim izvorima. Snimanje počinje 1909. Godine 1914. želeo je filmski studio, ali planove je prekinuo rat. Godina rođenja razlikuje se u biografskim izvorima (1876/1878) i zato nije pitanje u igri. Za rani film pojavljuju se nazivi `U carstvu Terpsihore` i `U državi Terpsihore`; naslov se ne koristi kao strogo pitanje.

## Profesor Vlada — dramaturški signali
- Dolazak: „Stari projektor je utihnuo. Nedostaju mu tri kadra. Hoćeš li da ih složiš?“
- Pred igru: „Projektor, kamera, studio — pronađi pravi redosled.“
- Greška: „Pogledaj godine i pokušaj ponovo. Nema kazne.“
- Posle igre: „Neki planovi nisu postali stvarnost, ali mogu da promene istoriju grada.“
- Kraj: „Traka koju dobijaš naš je simbol, a ne sačuvani Ernestov film.“

## Pravila i bodovanje
`EB-01 → EB-AR-01 → EB-02 → EB-03`. Četiri obavezna zadatka po 10, završetak misije +30 = **70 osnovnih poena**. Pogrešan odgovor na kviz -2 prema globalnom pravilu; AR/2D pogrešan raspored 0. Relikt +0, dva opciona istorijska pečata +2 svaki uz dnevni limit, jedan opcioni fikcionalni džoker +1. `TASK_COMPLETED`, `MISSION_COMPLETED` i pickup dodeljuju poene samo jednom po stabilnom ključu.

## Pristupačnost, bezbednost i prava
AR koristi camera-relative bilborde ili ručno potvrđenu bezbednu površinu, bez geospatial zahteva. Potrebni su titlovi, mute, veliki touch targets, tastatura za 2D, skip animacije i ponovni poziv Profesora. Pet SVG koncepta su originalni; GLB, OGG, WebP i foto prava su pending. Nisu sprovedeni Android, ARCore, GPS terenski, zvučni ni 3D testovi.

## Izvori
- Filmska enciklopedija: https://filmska.lzmk.hr/clanak/bosnjak-ernest
- Ravnoplov: https://www.ravnoplov.rs/somborski-bioskopi/
- RTV: https://www.rtv.rs/sr_lat/vojvodina/backa/holivudski-san-somborca-ernesta-bosnjaka_882542.html
- SOinfo: https://www.soinfo.org/vesti/vest/19568/da-je-sombor-postao-holivud/
