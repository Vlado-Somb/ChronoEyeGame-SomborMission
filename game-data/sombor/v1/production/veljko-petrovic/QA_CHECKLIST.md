# QA CHECKLIST — VP / Golubica koja se vraća

## Urednički i istorijski
- [x] SO_VP je spomenik, biblioteka je samo dokumentovani prostorni orijentir.
- [x] Izvori za 1884 / 1902 / 2017 / Igora Šetera navedeni u brief-u.
- [x] „Golubica sa crnim srcem“ precizirana kao pripovetka i naslov izbora; originalni odgovor VP-02 indeks 0.
- [x] VP-03 Igor Šeter, indeks 1; ponuđene opcije nisu menjane.
- [x] Originalna tri Unity zadatka sa svim poljima sačuvana u legacy-vp.json.
- [x] Izmišljeni govor ptice i „pisma“ nisu predstavljeni kao autentična svedočanstva.
- [ ] Nezavisni urednički lektorat i završna provera prava na slike/skulpturu.

## Struktura i statička kontrola
- [x] VP-01 → VP-AR-01 → VP-02 → VP-03, jedinstveni ID-jevi.
- [x] Isti redosled i isti skor AR/2D; bez kazne za AR ponavljanje.
- [x] Pet originalnih SVG koncepata, nijedan GLB/OGG lažno označen kao napravljen.
- [ ] Test stvarnog Web/Android parsera, event dispatcher-a i renderera.
- [ ] Test svih referenci iz manifest.json i registry-ja nakon spajanja PR-a.

## GPS, AR i pristupačnost
- [ ] Na terenu proveriti koordinate, 12 m / 2 s, pešačku dostupnost i sigurnu tačku gledanja.
- [ ] Ne ulaziti u biblioteku, ne hodati sa podignutim telefonom, ne dodirivati skulpturu.
- [ ] Android ARCore: permission denied, tracking loss, low light, resume i ručni anchor.
- [ ] 2D bez kamere: iste kartice, činjenice, uslov i bodovi.
- [ ] TalkBack, tastatura, kontrast, titlovi, mute, skip, ponovno pozivanje Profesora.
- [ ] Nagrade ne dupliraju poene posle restarta, dvostrukog tap-a ili GPS ponovnog ulaska.
- [ ] Herbarijum: kartica relic-a i bonusa, izvori i statusi, AR/2D isti pristup.

## Produkcija i izdavanje
- [ ] Napraviti i proveriti originalne OGG zvukove i zajednički GLB mentora.
- [ ] Opcionu pticu napraviti kao originalan stilizovani lik, ne kopiju skulpture.
- [ ] Opcionu WebP fotografiju samo sa proverom autorskih prava.
- [ ] End-to-end Web+Android+GPS+AR+2D test; upisati rezultate u PRACTICE_LOG.
- [ ] Tek nakon stvarnog testa status sme biti verified_in_test.

**Status:** urednički i grafički koncept pripremljen; sve runtime/terenske provere ostaju otvorene.
