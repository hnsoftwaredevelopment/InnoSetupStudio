# Ontwerp: vaste knopbreedte in de voorvertoning

Datum: 2026-10-06. Status: goedgekeurd door Herbert, inclusief de indeling van de knoppen (sectie 4). Gebouwd in PR #33.

## 1. Aanleiding

De knoppen in de voorvertoning van de IDE groeien mee met hun tekst. Een lange tekst maakt de knop breder, soms breder dan het venster. Setup werkt anders: de knoppen hebben een vaste breedte en een te lange tekst wordt afgekapt. Een voorvertoning die iets anders laat zien dan Setup doet, geeft een verkeerd beeld van het resultaat. Dit is de eerste van twee stappen. De tweede stap, een eigen breedte per knop, staat buiten deze wijziging (zie sectie 8).

## 2. Wat Setup doet (gemeten)

Gemeten met Inno Setup 7.1.0 op 96 DPI, lettertype Segoe UI 9, met een testscript dat de maten uitleest en een schermafdruk van het echte Setup-venster. `modern` en `classic` geven hetzelfde resultaat.

| Meting | Uitkomst |
|---|---|
| Afmeting Terug, Volgende, Annuleren | 75 bij 23 pixels |
| Afmeting Bladeren (doelmap en startmenumap) | 75 bij 23 pixels |
| Breedte na een lange tekst | Blijft 75. De knop groeit niet mee. |
| Breedte na `Width := 200` in `[Code]` | Wordt 200. De breedte is instelbaar, dat is de basis voor de tweede stap. |
| Lange tekst | Gecentreerd en aan beide kanten afgekapt bij de rand van de knop. Geen puntjes, geen regelafbreking. |
| Venster | Clientgebied 596 bij 432. Terug begint op 351, Volgende op 426 (direct naast Terug), Annuleren op 511 (10 pixels ruimte), 10 pixels marge rechts. |

## 3. Wat de voorvertoning nu doet

| Plek | Nu |
|---|---|
| Terug, Volgende, Annuleren in de installervoorvertoning (`ScreenEditorControl.xaml`) | `MinWidth="80"` met `Padding="12,4"`. De knop groeit mee met de tekst. |
| Bladeren op de doelmap- en startmenumappagina (`SelectDestinationPagePreview.xaml`, `SelectProgramGroupPagePreview.xaml`) | Kolom op `Auto` met `Padding="10,3"`. Groeit mee. |
| Voorvertoning in Knopeigenschappen (`ButtonPropertiesWindow.xaml`) | `MinWidth="80"` met `Padding="12,4"`. Groeit mee. |

## 4. Besluit

Alle zes de voorvertoningsknoppen krijgen de vaste maat van Setup: 75 bij 23 (apparaatonafhankelijke eenheden, gelijk aan pixels op 96 DPI). De tekst staat gecentreerd op één regel, zonder terugloop en zonder puntjes. Wat niet past wordt aan beide kanten afgekapt, zoals Setup dat doet.

Waarom op ware grootte en niet geschaald: de voorvertoning is 497 breed, het echte clientgebied 596. De tekst in de voorvertoning heeft wel de ware grootte. Het punt waarop een tekst wordt afgekapt hangt af van tekstbreedte tegen knopbreedte. Alleen met een knop van 75 klopt dat punt met Setup.

**Indeling.** Herbert besloot de indeling van Setup ook over te nemen: Annuleren staat rechts, Terug en Volgende liggen tegen elkaar aan (0 ruimte) en tussen Volgende en Annuleren zit 10 eenheden ruimte, met 10 eenheden marge rechts en onder (de opvulling van de balk was al 10). Elke knop krijgt een vaste kolom in een `Grid`. Een verborgen knop laat zijn plaats dus leeg, zoals Setup dat doet. Eerder schoof Volgende bij een verborgen Terug naar rechts op.

**Lettergrootte zonder eigen instelling.** De knopstijl van de app zet 13 eenheden. Setup gebruikt Segoe UI 9 punten, dat is 12 eenheden op 96 DPI. De zes knoppen krijgen 12 als terugvalwaarde (`FallbackValue`), anders valt het punt waarop de tekst wordt afgekapt nog steeds niet samen met Setup. Een door jou ingestelde lettergrootte blijft zoals het was.

## 5. Wat verandert

- Een kleine statische klasse `SetupButtonMetrics` in het project `InnoSetupStudio.Wizard` met `Width = 75` en `Height = 23`. App en Wizard gebruiken dezelfde waarde via `{x:Static}`. Er is dan één plek waar de maat staat en de tweede stap (breedte per knop) vervangt alleen de vaste waarde door een binding met 75 als terugval.
- Elke van de zes knoppen krijgt `Width`, `Height` en `Padding="3,0"`. `MinWidth` vervalt. De `TextBlock` krijgt `HorizontalAlignment="Center"`, zodat een te brede tekst aan beide kanten buiten de ruimte van de knop valt. WPF knipt wat buiten die ruimte valt zelf af.
- De drie knoppen onder in de installervoorvertoning staan in een `Grid` met vier kolommen (75, 75, 10, 75), rechts uitgelijnd. `SetupButtonMetrics` bevat daarvoor ook de kolombreedtes en de terugvalwaarde 12 voor de lettergrootte.
- De Bladeren-kolom op de twee pagina's wordt `Auto` met de vaste knopbreedte erin, zodat de tekstvakken ernaast niet verschuiven.
- Geen wijziging in Core, generator, projectbestand of resourcebestanden.

Punt dat ik tijdens de bouw met een schermafdruk controleer: dat WPF een te brede tekst in een knop echt aan beide kanten afknipt en niet links uitlijnt. Als dat niet zo blijkt, zet ik `HorizontalAlignment="Center"` met een vaste breedte op de `TextBlock` of gebruik ik een `Grid` met `ClipToBounds`. Ik meld wat het werd.

## 6. Tests

De editorlogica heeft geen eenheidstests, omdat het testproject alleen naar Core verwijst. Deze wijziging is XAML en heeft geen logica. De controle gebeurt zo:

- Build zonder waarschuwingen en alle bestaande tests groen (286).
- Een los testprogramma buiten de repo tekent dezelfde knoppen (75 bij 23, opvulling 3,0, lettergrootte 12, tekst gecentreerd) met de knopstijl van de app naast het resultaat van het Setup-testscript, met drie lange teksten. WPF knipt aan beide kanten af op dezelfde plek als Setup: Terug toont "te lang voor", Volgende "eel erg lange" en Annuleren "JKLMNOPQR", in Setup en in WPF identiek. De knoppen liggen in de nieuwe indeling.

## 7. Handmatige testpunten voor Herbert

- Vul bij Volgende een lange tekst in, bijvoorbeeld "Dit is een heel erg lange knoptekst". De knop blijft even breed als Terug en de tekst wordt aan beide kanten afgekapt.
- Dezelfde proef bij Terug en Annuleren, en bij Bladeren op Select Destination en Select Start Menu Folder.
- Een korte tekst ("OK") staat gecentreerd in een knop van dezelfde breedte.
- Onder in de voorvertoning staat Annuleren rechts, met 10 eenheden ruimte naar Volgende. Terug en Volgende liggen tegen elkaar aan.
- Zet bij een scherm Terug op onzichtbaar: Volgende en Annuleren blijven op hun plaats staan.
- De voorvertoning in het venster Knopeigenschappen gedraagt zich hetzelfde, ook als je lettertype, lettergrootte of vet wijzigt.
- Een grotere lettergrootte maakt de knop niet breder. Ik heb dat in Setup niet apart gemeten, maar Setup zet de breedte alleen via de indeling van het formulier.
- Vergelijk met de gegenereerde installer: de tekst moet op dezelfde plek afgekapt worden.

## 8. Buiten deze wijziging

- Een eigen breedte per knop (tweede stap): nieuw veld in het model, de editor, de generator (`Width` in `[Code]`) en de voorvertoning. `Width := 200` werkt in Setup, gemeten.
- De lettergrootte die je zelf instelt: Inno Setup rekent in punten, de voorvertoning leest het getal als eenheden. Een ingestelde 9 geeft in de voorvertoning dus kleinere tekst (9 eenheden) dan in Setup (12 eenheden). Dat heb ik gezien, maar niet gewijzigd.
- De totale afmeting van de voorvertoning (497 bij 400, het echte clientgebied is 596 bij 432).
- Een tooltip op een uitgeschakelde knop (je meet dat zelf).

## 9. Documentatie

Bij de bouw: nieuwe sectie 35 in `Architectuur-en-Ontwerp.md` met de metingen uit sectie 2, en een spiegel van de gewijzigde Markdown-bestanden naar Obsidian.
