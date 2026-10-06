# Ontwerp: knopinstellingen in het gegenereerde script (stap 4)

Status: gebouwd (2026-10-06). De keuzes uit sectie 11 zijn op 2026-10-05 door Herbert gemaakt. Dit is stap 4 uit `docs/Ontwerp-Dunne-Generator.md`. Stap 1 tot en met 3 (projectvelden, `IssGenerator`, de knop "Genereer .iss") zijn gemerged. Afwijkingen van dit ontwerp tijdens de bouw: sectie 5 (geen vergelijkingstest met de editor), sectie 6 regel 8 (teksten worden getrimd) en sectie 11 punt 6 (`Visible` alleen als `False`). Verwijzing naar de bouw: sectie 32 van `docs/Architectuur-en-Ontwerp.md`.

## 1. Doel

Wat je in de schermeditor bij Terug, Volgende en Annuleren instelt, en bij de Bladeren-knoppen van de mappenpagina's, moet in het gegenereerde `.iss` terechtkomen en in de installer zichtbaar zijn. Het gaat om tekst, ingeschakeld, zichtbaar, lettertype, lettergrootte, vet en tooltip, per taal waar dat is ingevuld. Het script moet daarbij zonder fouten en zonder waarschuwingen compileren met ISCC (de eerste eis voor elke versie).

In Inno Setup zijn dit geen `[Setup]`-richtlijnen. Het zijn eigenschappen van objecten in de wizard (`WizardForm.NextButton` en andere), die je alleen met Pascal Script in een `[Code]`-sectie kunt zetten.

## 2. Wat is uitgezocht en hoe

Alles hieronder is met Inno Setup 7.1.0 gemeten, niet alleen uit de documentatie gehaald. Ik heb kleine proefscripts gecompileerd met ISCC, stil laten draaien met een logbestand (`/VERYSILENT /LOG`), en een proefinstaller met het venster zichtbaar gestart en het venster als afbeelding vastgelegd. De proefscripts zijn niet opgenomen in de repository. De installers installeerden niets (`CreateAppDir=no`, `Uninstallable=no`, tijdelijke map).

| Onderwerp | Uitkomst |
|---|---|
| `Caption`, `Enabled`, `Visible`, `Font.Name`, `Font.Size`, `Font.Style`, `Hint`, `ShowHint` op Terug, Volgende en Annuleren, en `DirBrowseButton`, `GroupBrowseButton` | Compileren zonder fout of waarschuwing. |
| Lettertype en vet | Werken. Op een Volgende-knop met Consolas 12 en vet was de tekst zichtbaar in dat lettertype en vet, in `WizardStyle=modern` en `classic`. |
| Tekstkleur (`Font.Color`) | Geen effect. De knoptekst bleef zwart, in beide wizardstijlen. Windows tekent een gewone knop met de themakleur. Zie sectie 8. |
| Lettergrootte en lange tekst | De knop groeit niet mee. Een tekst die niet past wordt afgekapt. |
| Gedrag bij paginawissel | Setup zet `Caption`, `Enabled` en `Visible` van de drie knoppen vóór elke `CurPageChanged` terug naar zijn eigen waarde. `Font`, `Hint` en `ShowHint` blijven staan en lekken dus door naar volgende pagina's. Zie sectie 4. |
| `CustomMessage('Naam')` | Geeft de tekst letterlijk terug. `%%`, `%n`, `%1` en `{app}` worden niet omgezet, spaties aan het begin en einde blijven, evenals `;`, `=`, `'` en `"`. Er is dus geen escaping nodig voor teksten. |
| Onbekende berichtnaam in `CustomMessage` | Runtime-fout: `InitializeWizard raised an exception (fatal)`. De generator mag nooit een naam gebruiken die niet gedefinieerd is. |
| Bericht dat niet voor alle talen is gedefinieerd | ISCC geeft een waarschuwing per ontbrekende taal. Setup gebruikt dan de tekst van de eerste taal waarin het bericht is gedefinieerd. |
| Bericht zonder taalvoorvoegsel plus berichten met taalvoorvoegsel | Geen waarschuwing, mits de regel zonder voorvoegsel vóór de regels met voorvoegsel staat. Staat hij erna, dan overschrijft hij de vertaling voor alle talen. |
| Lege waarde bij het bericht zonder voorvoegsel | Toegestaan. `CustomMessage` geeft in talen zonder vertaling een lege tekst terug. |
| Beginwaarden van de knoppen | Gemeten bij de start van Setup, in `modern` en `classic`: `Hint` is leeg, `ShowHint` is `False` en `Font.Style` is leeg, voor Terug, Volgende, Annuleren en beide Bladeren-knoppen; `WizardForm.ShowHint` is ook `False`. De generator legt lettertype, grootte en stijl toch vast en zet ze terug. Voor `Hint` en `ShowHint` zet de reset de gemeten beginwaarden (leeg en `False`) direct neer. |
| Tooltip op een uitgeschakelde knop | Nog niet gecontroleerd. Zie sectie 11. |

## 3. Welk scherm hoort bij welke pagina in Setup

`CurPageChanged(CurPageID)` krijgt een pagina-ID. De acht schermen met een eigen knopmodel in `InstallerProject`:

| Scherm in de Studio | Eigenschap | Pagina-ID in Setup |
|---|---|---|
| Welkom | `WelcomeScreenButtons` | `wpWelcome` |
| Licentie | `LicenseScreenButtons` | `wpLicense` |
| Info voor installatie | `InfoBeforeScreenButtons` | `wpInfoBefore` |
| Gebruikersgegevens | `UserInfoScreenButtons` | `wpUserInfo` |
| Doelmap | `SelectDestinationScreenButtons` | `wpSelectDir` |
| Startmenumap | `SelectProgramGroupScreenButtons` | `wpSelectProgramGroup` |
| Klaar om te installeren | `ReadyScreenButtons` | `wpReady` |
| Info na installatie | `InfoAfterScreenButtons` | `wpInfoAfter` |

De Bladeren-knoppen horen bij `wpSelectDir` (`SelectDestinationBrowseButton`, `WizardForm.DirBrowseButton`) en `wpSelectProgramGroup` (`SelectProgramGroupBrowseButton`, `WizardForm.GroupBrowseButton`).

Pagina's zonder knopmodel: Select Components, Select Tasks, Preparing, Installing en Finished. Het Standaardscherm (`DefaultScreenButtons`) is geen echte pagina. Wat daarmee op deze vijf pagina's moet gebeuren is een open vraag (sectie 11).

Een scherm dat in het project uit staat (bijvoorbeeld `ShowLicensePage = false`) krijgt geen code. Heeft zo'n scherm wel eigen instellingen, dan meldt de generator dat met een Info-melding.

## 4. Wat Setup bij een paginawissel doet, en wat dat voor de generator betekent

Gemeten met een proefinstaller die bij elke pagina de toestand van de drie knoppen logde (Welkom, Licentie, Ready, Preparing, Installing, Finished):

- `Caption`, `Enabled` en `Visible` worden door Setup bij elke paginawissel opnieuw bepaald, vóór `CurPageChanged`. Een Annuleren-knop die ik op de welkomstpagina uitzette en verborg, stond op de volgende pagina weer aan en zichtbaar. Volgende heette op de Ready-pagina `&Install` en op de laatste pagina `&Finish`.
- `Font.Name`, `Font.Size`, `Font.Style`, `Hint` en `ShowHint` worden niet teruggezet. Een Volgende-knop die op de welkomstpagina Consolas, 11 en vet kreeg, bleef dat op alle volgende pagina's, ook op Installing en Finished.

Gevolgen:

1. Caption, Enabled en Visible zet de generator alleen op de pagina's waar een waarde is ingesteld. Terugzetten is niet nodig. `Enabled` schrijft de generator uitsluitend als `False`, en `Visible` ook (zie sectie 11 punt 6). `True` is wat Setup's eigen reset al oplevert, en een expliciet `True` kan een controle van Setup omzeilen: op de Licentie-pagina houdt Setup Volgende uitgeschakeld tot de licentie is geaccepteerd (gemeten: uitgeschakeld bij binnenkomst op die pagina), en een `NextButton.Enabled := True` in `CurPageChanged` zou die acceptatie overslaan. Het kan dus ook niet per ongeluk via het Standaardscherm gebeuren.
2. Lettertype, lettergrootte, vet en tooltip moeten bij elke paginawissel eerst worden teruggezet naar de beginwaarde en daarna per pagina worden ingesteld. De beginwaarden worden in `InitializeWizard` vastgelegd in variabelen. Dit geldt alleen voor de combinaties van knop en eigenschap die ergens in het project worden gebruikt, zodat het script niet groter wordt dan nodig.
3. Zonder dat terugzetten krijgt elke pagina na de eerste aangepaste pagina ook die aanpassing. Dat is de fout die deze stap moet voorkomen.

Verwacht gebruik (Herbert, 2026-10-05): de meeste installers stellen de knoppen via het Standaardscherm in en wijken hooguit incidenteel op één pagina af. Voor dat incidentele geval is het terugzetten bedoeld. Wil je een knop bewust op alle schermen aanpassen, dan komt dat in het script neer op dezelfde instelling per scherm, omdat de generator de waarde van het Standaardscherm per getoond scherm uitschrijft.

De Bladeren-knoppen worden niet door Setup teruggezet en bestaan maar op één pagina. Die worden eenmalig in `InitializeWizard` ingesteld.

## 5. Welke waarde geldt per knop: dezelfde drie lagen als in de editor

De schermeditor bepaalt de waarde van een veld in drie lagen: eigen waarde op het scherm, anders de waarde van het Standaardscherm, anders het eigen gedrag van Setup (zie sectie 11.9 en 17 van de architectuurdoc). De generator doet exact hetzelfde, zodat de installer laat zien wat de voorvertoning toonde.

- Tekst, lettertype en tooltip: eigen waarde als die niet leeg is of alleen uit spaties bestaat, anders de waarde van het Standaardscherm, anders niets instellen.
- Lettergrootte, vet, ingeschakeld en zichtbaar: eigen waarde als die is ingesteld (`null` is niet ingesteld), anders de waarde van het Standaardscherm, anders niets instellen.
- Vertalingen per taal (`*ByLanguage`) cascaderen sinds sectie 34 van de architectuurdoc wel via het Standaardscherm (ontwerp in `Ontwerp-Vertalingen-Standaardscherm.md`). In deze stap (4) gebeurde dat nog niet.
- De Bladeren-knoppen hebben geen Standaardscherm en geen cascade.

De bepaling van de effectieve waarde staat als losse, pure klasse in Core (`ButtonSettingsResolver`), met eigen tests. De editor behoudt zijn eigen `Effective*`-eigenschappen. Een test die beide vergelijkt, zoals dit document eerst voorstelde, is niet mogelijk: het testproject verwijst alleen naar Core en niet naar de app, waar de editor in staat. De regels uit deze sectie liggen daarom vast in de tests van de resolver. Tot de editor de resolver zelf gebruikt (backlog, sectie 13) moet een wijziging van deze regels op twee plaatsen worden doorgevoerd.

Een veld dat na de bepaling niet is ingesteld, levert geen code op. Een project zonder knopaanpassingen krijgt dus nog steeds geen `[Code]`- en geen `[CustomMessages]`-sectie, en het Minimal-goldenbestand verandert niet.

## 6. Teksten en talen: alles via `[CustomMessages]`

Elke tekst (knoptekst en tooltip) gaat via een bericht in `[CustomMessages]` en wordt in Pascal opgehaald met `CustomMessage('Naam')`. Ook in een project met één taal. Dat geeft één werkwijze voor alle projecten en maakt Pascal-escaping van teksten overbodig, omdat `CustomMessage` de tekst letterlijk teruggeeft (sectie 2).

Regels voor de gegenereerde berichten:

1. Naam: `Btn` + schermsleutel + knop + veld, alleen letters, bijvoorbeeld `BtnWelcomeNextCaption`, `BtnReadyNextTooltip`, `BtnSelectDirBrowseCaption`. Het voorvoegsel `Btn` voorkomt een botsing met Setup's eigen berichtnamen.
2. Eerst één regel zonder taalvoorvoegsel met de universele (Engelse) tekst: `BtnWelcomeNextCaption=Start`. Dit is de tekst voor Engels en voor elke taal zonder eigen vertaling.
3. Daarna per taal met een ingevulde vertaling een regel met voorvoegsel, in catalogusvolgorde: `dutch.BtnWelcomeNextCaption=Begin`. Alleen talen die in `[Languages]` staan.
4. Zonder voorvoegsel eerst, anders overschrijft die regel alle vertalingen (gemeten). Met deze volgorde geeft ISCC ook geen waarschuwing voor talen zonder vertaling.
5. Is de universele tekst leeg maar bestaat er wel een vertaling, dan staat er een lege regel (`BtnWelcomeNextCaption=`) en controleert de code vóór het toewijzen of de tekst niet leeg is. Zo houdt Setup in talen zonder vertaling zijn eigen tekst. Is er geen enkele vertaling, dan staat er geen bericht en geen toewijzing.
6. Een tekst met een regeleinde krijgt dezelfde behandeling als elders: het veld wordt weggelaten en er komt een `ValueContainsLineBreak`-melding.
7. Taal-ids in een vertalingenlijst die niet meer in het project voorkomen worden genegeerd.
8. Teksten worden getrimd, zoals alle andere tekstvelden van de generator. Spaties aan het begin of einde van een knoptekst komen dus niet in het script.

Lettertypenamen zijn de enige tekst die in Pascal-code komt (`Font.Name := '...'`). Daar wordt een enkele aanhalingsteken verdubbeld.

## 7. Hoe het gegenereerde script eruit komt te zien

Voorbeeld voor een project met Engels en Nederlands, een eigen Volgende-tekst en tooltip op Welkom, een eigen Volgende-tekst met lettergrootte 10 op Ready, en een aangepaste Bladeren-tekst op de doelmappagina (die verder niets aanpast):

```
[CustomMessages]
BtnWelcomeNextCaption=Start
dutch.BtnWelcomeNextCaption=Begin
BtnWelcomeNextTooltip=Ga verder
BtnSelectDirBrowseCaption=Find...
dutch.BtnSelectDirBrowseCaption=Zoeken...
BtnReadyNextCaption=Install now
dutch.BtnReadyNextCaption=Nu installeren

[Code]
var
  InitNextFontSize: Integer;

procedure InitializeWizard;
begin
  InitNextFontSize := WizardForm.NextButton.Font.Size;
  WizardForm.DirBrowseButton.Caption := CustomMessage('BtnSelectDirBrowseCaption');
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  { Setup zet Font en Hint niet terug bij een paginawissel: eerst naar de beginwaarde. }
  WizardForm.NextButton.Font.Size := InitNextFontSize;
  WizardForm.NextButton.Hint := '';
  WizardForm.NextButton.ShowHint := False;
  case CurPageID of
    wpWelcome:
      begin
        WizardForm.NextButton.Caption := CustomMessage('BtnWelcomeNextCaption');
        WizardForm.NextButton.Hint := CustomMessage('BtnWelcomeNextTooltip');
        WizardForm.NextButton.ShowHint := True;
      end;
    wpReady:
      begin
        WizardForm.NextButton.Caption := CustomMessage('BtnReadyNextCaption');
        WizardForm.NextButton.Font.Size := 10;
      end;
  end;
end;
```

Dit patroon (zonder de `Btn`-berichtnamen) is gecompileerd met ISCC 7.1.0 in een proefscript, zonder fout en zonder waarschuwing. Het voorbeeld zelf staat als test in `IssGeneratorButtonTests` (`Design_document_example_is_generated_as_documented`). De berichten staan in de volgorde van de schermen (sectie 3), per scherm eerst Terug, Volgende, Annuleren en dan de Bladeren-knop; daarom staat de doelmap vóór Ready.

Afspraken voor de uitvoer, in de lijn van de bestaande generator:

- Terugzetten gebeurt alleen voor de eigenschappen die ergens in het project worden gebruikt (in het voorbeeld alleen de lettergrootte van Volgende en de tooltip). Wordt vet gebruikt, dan legt `InitializeWizard` de beginstijl vast in een `TFontStyles`-variabele, zet de reset die terug en zet een pagina met vet `[fsBold]` (CodeRabbit, PR #30). Een expliciet "niet vet" op een pagina vraagt dan geen extra code. Wordt een lettertype gebruikt, dan wordt `Font.Name` op dezelfde manier vastgelegd en teruggezet.
- Deterministisch: vaste volgorde van schermen (zoals in sectie 3), knoppen (Terug, Volgende, Annuleren) en velden, en catalogusvolgorde voor talen.
- Sectievolgorde in het script: `[Setup]`, `[Languages]`, `[CustomMessages]`, `[Tasks]`, `[Files]`, `[Icons]`, `[Code]`.
- CRLF en UTF-8 met BOM, net als de rest.
- `CurPageChanged` ontbreekt als er alleen Bladeren-aanpassingen zijn. `InitializeWizard` ontbreekt als er niets vast te leggen of in te stellen valt.
- Het `[Code]`-blok is volledig gegenereerd. De kop van het bestand zegt al dat handmatige wijzigingen verloren gaan bij opnieuw genereren. Eigen Pascal-code naast het gegenereerde blok is niet mogelijk in deze stap (zie de backlog).

## 8. Wat niet kan, en welke meldingen er komen

**Tekstkleur kan niet.** `Font.Color` compileert maar heeft geen effect op de knoppen van Setup: Windows tekent een gewone knop met de themakleur, in `modern` en in `classic` (gemeten, zie sectie 2). Dezelfde reden waarom de achtergrondkleur al eerder uit het model is geschrapt. Een kleur zou alleen werken met een zelf getekende knop, en dat is een veel grotere ingreep dan deze stap. Besloten: de generator schrijft geen kleurcode, en het kleurveld is in een aparte kleine PR uit het model, de editor en de voorvertoning gehaald (2026-10-06, zie sectie 33 van de architectuurdoc). Er is daarom geen melding voor een tekstkleur: er is niets meer in te stellen. Een oud projectbestand met kleurvelden opent nog gewoon, de velden worden genegeerd en bij het opslaan niet meer weggeschreven.

Meldingen die in stap 4 veranderen. Elke nieuwe code krijgt teksten in NL, EN en DE en een regel in `GenerationIssueResourceTests.ArgumentCounts`, zoals bij stap 3.

| Code | Ernst | Argumenten | Wanneer |
|---|---|---|---|
| `ButtonSettingsNotGenerated` | verwijderd | | Er is niets meer dat niet wordt vertaald. De code, de teksten en de testregel zijn verwijderd. |
| `ButtonTextColorNotSupported` | verwijderd | | Heeft kort bestaan in de eerste versie van stap 4. Met het kleurveld uit de IDE kan er geen tekstkleur meer zijn ingesteld, dus de melding, de teksten en de testregel zijn weer verwijderd. |
| `ButtonSettingsForHiddenScreen` | Info | veldnaam van het scherm, bijvoorbeeld `LicenseScreenButtons` | Een scherm met eigen instellingen staat uit in het project. De instellingen worden niet gebruikt. |
| `NextButtonUnusable` | Warning | veldnaam van het scherm | Na de cascade is Volgende op een getoond scherm uitgeschakeld of verborgen. De gebruiker kan dan niet verder, want het gegenereerde script zet de knop nergens weer aan. |

Bestaande meldingen die hier opnieuw gelden: `ValueContainsLineBreak` voor knoptekst, tooltip en lettertype met een regeleinde. De veldnaam in die melding is `<eigenschap van het project>.<veld van de knop>`, bijvoorbeeld `WelcomeScreenButtons.NextButtonCaption`, en bij een vertaling `WelcomeScreenButtons.NextButtonCaptionByLanguage[dutch]`. Een regeleinde in het Standaardscherm wordt één keer gemeld, niet per scherm.

## 9. Tests

- **Eenheidstests** voor `ButtonSettingsResolver`: eigen waarde wint, dan het Standaardscherm, dan niets; tekst met alleen spaties telt als leeg; `null` bij lettergrootte, vet, ingeschakeld en zichtbaar; vertalingen cascaderen sinds sectie 34 wel (zie `Ontwerp-Vertalingen-Standaardscherm.md`).
- **Generatortests**: `Enabled := True` wordt nooit geschreven (ook niet als het scherm True instelt en het Standaardscherm False); geen `[Code]` en geen `[CustomMessages]` zonder aanpassingen; volgorde van de regels zonder en met taalvoorvoegsel; lege universele tekst met vertaling; pagina-ID per scherm; reset van lettertype en tooltip alleen waar het nodig is; Bladeren-knoppen in `InitializeWizard`; scherm dat uit staat; elke nieuwe melding in beide richtingen; regeleinde in een tekst; aanhalingsteken in een lettertypenaam.
- **Samenhang**: elke berichtnaam die de code gebruikt is gedefinieerd en elke definitie wordt gebruikt, en elk bericht zonder taalvoorvoegsel staat vóór zijn vertalingen. Een onbekende naam in `CustomMessage` is een fatale fout tijdens het draaien van Setup die ISCC niet ziet. De eerder geplande vergelijking met de `Effective*`-eigenschappen van de editor vervalt, zie sectie 5.
- **Goldenbestand** `Buttons.iss` met een meertalig project met alle acht schermen en beide Bladeren-knoppen. De bestaande goldenbestanden blijven ongewijzigd.
- **ISCC-compilatietests** (in `IssCompilerTests`, zonder `/Q`, met controle op `Warning:`): één taal, drie talen met een gedeeltelijke vertaling (geen waarschuwing), speciale tekens in teksten (`'`, `"`, `%`, `%n`, `{`, `{{`, `;`, `=`, accenten, Duits en Frans), een lettertypenaam met `'`, alle acht schermen tegelijk, en een Standaardscherm dat meespeelt.
- **Geen geautomatiseerde test van het gedrag in het installatievenster.** Een stille run (`/VERYSILENT`) roept `CurPageChanged` niet aan. De proefscripts uit sectie 2 laten zien dat dit wel met een zichtbare run kan, maar dat is te kwetsbaar voor de testsuite. Dat gedeelte test Herbert met de lijst in sectie 12.

## 10. Bouwvolgorde

Eén PR op een eigen branch, in deze volgorde, elke stap met tests voordat de volgende begint:

1. `ButtonSettingsResolver` in Core met tests, plus de vergelijking met de editor.
2. De `[CustomMessages]`-uitvoer (regels, volgorde, lege universele tekst).
3. De `[Code]`-uitvoer: `InitializeWizard`, `CurPageChanged`, reset, Bladeren-knoppen.
4. Nieuwe meldingen en de teksten in NL, EN en DE; `ButtonSettingsNotGenerated` verwijderen.
5. Goldenbestand en ISCC-tests.
6. Documentatie bijwerken (dit document, architectuurdoc sectie 30 en 32, featurechecklist) en spiegelen naar Obsidian.

Alle zes stappen zijn gebouwd (2026-10-06).

## 11. Beslissingen en open punten

1. **Tekstkleur. Besloten: niet genereren en uit de IDE verwijderen (gebouwd op 2026-10-06, aparte kleine PR).** De generator schrijft geen kleurcode. Het kleurveld is uit de editor, het model en de voorvertoning verwijderd. Een instelling die niet kan werken hoort niet in de IDE te staan, ook niet met een toelichting. Verworpen: toch genereren (compileert, maar de knoppen blijven zwart, dus misleidend) en zelf getekende knoppen of een eigen knopbibliotheek in de gegenereerde installer. Herbert wil de standaardfunctionaliteit van Inno Setup gebruiken en niets nabouwen.
2. **Standaardscherm op pagina's zonder knopmodel. Besloten: alleen de acht schermen uit sectie 3.** Select Components, Select Tasks, Preparing, Installing en Finished houden Setup's eigen knoppen, omdat je in de editor alleen de acht schermen ziet. Gevolg: de pagina Select Tasks (die Setup toont zodra er een bureaubladpictogram-taak is) en Finished hebben dan Setup's eigen knoppen, ook als het Standaardscherm iets anders instelt. Verworpen: het Standaardscherm op alle pagina's toepassen met een `else`-tak, want dat toont een instelling die je in de editor niet kunt zien, en een verborgen of uitgeschakelde Volgende op Finished kan de gebruiker laten vastlopen.
3. **Melding `NextButtonUnusable`. Besloten: ja, als Warning.** Het gegenereerde script heeft geen logica die Volgende later aanzet. Op de Licentie-pagina bepaalt Setup zelf wanneer Volgende aan gaat, dus daar volgt Setup zijn eigen regels.
4. **Afgekapte tekst. Besloten: niet in versie 1, maar wel verplicht daarna.** Een knop in Setup groeit niet mee met een lange tekst of een grote letter. De knoppen in de IDE-voorvertoning mogen dat dan ook niet doen, anders krijgt de gebruiker een verkeerd beeld van het resultaat. Het gaat om een belangrijk onderdeel van de gebruikerservaring. In versie 1 van stap 4 verandert er niets en staat het als testpunt in sectie 12. Het eerstvolgende vervolg daarna: de voorvertoning geeft knoppen dezelfde vaste breedte als Setup en kapt tekst af, en daarna een knopbreedte per knop (zie sectie 13).
5. **Tooltip op een uitgeschakelde knop. Nog te meten.** Dat controleert Herbert in de handmatige test (sectie 12). Blijkt het niet te werken, dan wordt het een opmerking in de documentatie en geen codewijziging.

6. **`Visible` alleen als `False` geschreven. Besloten tijdens de bouw (2026-10-06), ter bevestiging door Herbert.** Net als bij `Enabled` (sectie 4) schrijft de generator `Visible` alleen als `False`. Setup bepaalt zelf of een knop zichtbaar is (Terug is op de welkomstpagina verborgen, Annuleren op de laatste pagina) en zet `Visible` bij elke paginawissel terug, dus een expliciet `True` voegt niets toe. Het kan Setup's eigen keuze zelfs doorbreken, bijvoorbeeld Terug zichtbaar maken op de eerste pagina als dat via het Standaardscherm binnenkomt. Gevolg: een scherm met `Visible = true` boven een Standaardscherm met `false` laat de knop zichtbaar (geen code, Setup's eigen toestand), en een `true` zonder `false` erboven verandert niets.

## 12. Handmatige testpunten na de bouw (voor Herbert)

Testproject voor de handmatige tests: `C:\DevOps\hnsoftwaredevelopment\Test\Herbies Test.issproj`. Genereer een `.iss` voor een project met knopaanpassingen, compileer het in Inno Setup en draai de installer. Let op:

- Een aangepaste Volgende-tekst op Welkom staat daar, en op de pagina erna staat weer Setup's eigen tekst.
- Een aangepast lettertype of vet op één pagina blijft niet hangen op de volgende pagina's (inclusief Installing en Finished).
- Een tooltip verschijnt alleen op de pagina waarvoor hij is ingesteld, ook als je met Terug naar die pagina terugkeert.
- Tooltip op een uitgeschakelde knop: verschijnt hij of niet?
- Meertalig: start de installer met `/LANG=dutch` en `/LANG=german`. Een taal zonder vertaling toont de universele tekst, en als die leeg is Setup's eigen tekst.
- Een lange knoptekst of grote letter: wordt de tekst afgekapt?
- Bladeren-knop op de doelmap en op de startmenumap: tekst, uitgeschakeld en verborgen.
- Annuleren verborgen op een pagina, en terug op de volgende.
- Alleen het Standaardscherm ingesteld (bijvoorbeeld Volgende-tekst): elk getoond scherm volgt die instelling.
- Volgende uitgeschakeld of verborgen op een scherm: het resultaatvenster van "Genereer .iss" toont de waarschuwing, en de installer laat op dat scherm niet verder gaan.
- De editor en het venster Knopeigenschappen tonen geen tekstkleur meer, en een oud projectbestand met een tekstkleur opent zonder fout.
- Het script compileert zonder waarschuwingen.

## 13. Backlog na stap 4

- Eigen Pascal-code naast het gegenereerde blok (een projectveld voor extra `[Code]`).
- Verplicht na versie 1: de voorvertoning in de IDE kapt knoppen af zoals Setup (vaste breedte, geen meegroeien), daarna een knopbreedte per knop.
- De editor laten rekenen met `ButtonSettingsResolver` in plaats van met eigen `Effective*`-logica.
- Knopmodellen en editors voor Select Components, Select Tasks en Finished.
- Het script splitsen over meerdere `.iss`-bestanden (Inno Setup kent `#include`) zodra het aantal opties het gegenereerde bestand onoverzichtelijk maakt. Nu nog niet aan de orde.
- Cascade van vertalingen via het Standaardscherm: gebouwd, zie sectie 34 van de architectuurdoc.
