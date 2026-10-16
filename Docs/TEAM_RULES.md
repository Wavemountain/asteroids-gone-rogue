# Spelregler för teamet – Asteroids gone rogue

Version: 1.0 (2026-10-16) · Ägare: SpelPM · Godkänd av: Wagge

Ändringar görs via PR mot `main`; bumpa versionen och lägg till en rad i ändringsloggen längst ner. Wagge godkänner ändringar.

## Beslut och kommunikation
1. **Wagge bestämmer det viktiga.** Riktning, scope, tagg/release, pris, Steam. SpelPM avgör mindre saker som projektledare.
2. **Information via SpelPM.** Status, beslut och gates går via SpelPM. Direktkontakt med Wagge är okej när det behövs, ingen kopia krävs.
3. **Ingen spam.** Skriv bara när något har hänt eller ett beslut behövs.
4. **Kort statustext.** Börja med resultatet (PASS/FAIL/kvar), detaljer efter.
5. **Ingen tyst överlämning.** Överlämning innehåller SHA, vad som ändrats och vad som återstår.
6. **En ägare per fråga.** GameBot = kod, SpeltestBot = test, AtmosBot = look/ljud, BlenderBot = 3D, Vågskål = balans. Oklart → SpelPM avgör. Bygg inte i någon annans område utan avstämning.
7. **Ärlighet.** Skriv det som faktiskt är testat, inte det som borde fungera.

## Kvalitetsgrindar
8. **Kompilera först.** Ingen PASS utan felfri Unity-kompilering (Editor/batchmode) på HEVONEN.
9. **Bilder före test.** Speltest jämför sida vid sida mot förra versionen och kör lager-QA. Wagge testar först vid 0 FAIL.
10. **Alltid bättre.** Varje version förbättrar System, Stil och Ljud; ingen regress mot förra versionen.
11. **Rätta orsaken.** När ett fel når Wagge läggs en automatisk kontroll till som stoppar det nästa gång.
12. **Mät med siffror och bilder.** Balans, kontrast och prestanda bedöms med mätvärden och side-by-side, inte magkänsla.

## Spelaren
13. **Spelaren först.** Ändringar ska förbättra första 5 minuterna, rättvisa dödar eller synlig progress (mål: grymma Steam-reviews).
14. **Läsbarhet före snyggt.** Fiender, skott och skepp syns alltid (kontrast ≥ 2.5). Glöd får aldrig dölja spelet.
15. **Reduce effects fungerar på riktigt.** Allt nytt kan tonas ner; Speltest kontrollerar.
16. **Inga soft-locks.** Man kan alltid klara en våg eller avbryta till hangaren.

## Process och release
17. **Planera innan bygge.** Före ny version: kort plan (System · Stil · Ljud) som Wagge godkänner.
18. **Små och säkra steg.** Tydlig skillnad per version, småfixar rullar in i nästa tagg, stora ändringar på egen gren så `main` alltid är spelbar.
19. **Ingen release utan Wagges OK.** Varje tagg får release notes på svenska och engelska, och Wagge får länken.
20. **Rör inte Wagges grejer.** Kör på HEVONEN bara när Wagge inte spelar; säkerhetskopiera alltid saves först.
21. **Bara fria licenser.** CC0 eller eget material, krediteras i CREDITS.

## Ändringslogg
- 1.0 (2026-10-16): Första versionen, godkänd av Wagge.
