# Inno Setup Studio — Architectuur en Ontwerp

## 1. Doel

Een WPF-IDE om Inno Setup `.iss`-installerscripts te bouwen en onderhouden via aparte, herkenbare
schermen (projectinstellingen, wizardschermen, elementen per scherm) in plaats van kale
scripttekst, inclusief het compileren en direct kunnen draaien van de installer.

## 2. Technologiekeuze

- .NET 10 (LTS, ondersteund tot november 2028) met WPF.
- MVVM via CommunityToolkit.Mvvm.
- Syncfusion 34.x waar een control écht toegevoegde waarde heeft; geen harde eis om Syncfusion-
  controls te gébruiken. `Syncfusion.Licensing` zelf is wel een vaste `PackageReference` (nodig om
  de licentie te registreren zodra ergens een Syncfusion-control wordt toegevoegd), niet iets wat
  pas later optioneel binnenkomt.
- Kleurthema's en lokalisatie volgens hetzelfde patroon als FontManager/SVGViewer: losse
  `ResourceDictionary`-bestanden per thema (`DynamicResource`, geen herstart nodig) en een
  `LocalizationManager` + `{loc:Loc ...}`-markup-extensie voor live taalwissel.

## 3. Projectstructuur (solution)

- `InnoSetupStudio.Core` — datamodel van een .iss-project en instellingen, geen UI-afhankelijkheden.
  `Project/` bevat het projectmodel (`InstallerProject`, `WizardScreenSelection`) en de opslag
  ervan (zie §10).
- `InnoSetupStudio.App` — WPF-shell: `Themes/` (9 kleurthema's + Styles.xaml + ThemeManager),
  `Localization/` (LocalizationManager + LocExtension), `Resources/` (Strings.*.resx + Icons.xaml),
  `Converters/` (`IconKeyToGeometryConverter`), `Services/` (LicenseService), `ViewModels/`
  (CommunityToolkit.Mvvm-gebaseerde viewmodels), `Views/` (SplashWindow, MainWindow,
  ProjectSettingsWindow, WizardScreensWindow).
- `InnoSetupStudio.Tests` — xUnit-tests voor Core.

## 4. Theming-systeem

Negen thema's: Licht, Donker, Blauw, Blauw donker, Rood, Rood donker, Groen, Groen donker, Sepia.
Elk thema is een `Colors.<Naam>.xaml` met dezelfde kleursleutels (`Color.Background`,
`Color.Surface`, `Color.SurfaceAlt`, `Color.Border`, `Color.TextPrimary`, `Color.TextSecondary`,
`Color.Accent`, `Color.AccentHover`, `Color.Success`, `Color.Danger`, `Color.Warning`) plus de
bijbehorende `Brush.*`-versies. `ThemeManager.ApplyTheme` verwisselt de dictionary in
`Application.Resources.MergedDictionaries`; alle stijlen in `Styles.xaml` gebruiken
`DynamicResource`, dus een themawissel werkt direct, zonder herstart. Licht/Donker/Blauw/Blauw
donker/Rood donker/Groen donker/Sepia zijn hergebruikt uit FontManager voor een consistente
look tussen de HNSoftware-apps; Rood en Groen (de lichte varianten) zijn nieuw voor dit project.

Naamgeving is bewust: "Blauw"/"Rood"/"Groen" zijn de lichte (Light) varianten met een gekleurd
accent, "Blauw donker"/"Rood donker"/"Groen donker" zijn de bijbehorende donkere (Dark) varianten
met dezelfde accentkleur. Strikt genomen zou "Blauw" dus "Blauw licht" moeten heten, maar de
suffix "donker" is gekozen als het enige onderscheid tussen de light/dark-paren, zodat in één
oogopslag duidelijk is welke van de twee de donkere variant is. Dit is een expliciete keuze van
Herbert, geen inconsistentie.

## 5. Lokalisatie (i18n)

Eén resx-set per taal onder `Resources/`: `Strings.resx` (Nederlands = standaard/fallback),
`Strings.en-US.resx`, `Strings.de-DE.resx`, beheerd via ResXManager (`ResXManager.config.xml` in
de solution root). Taalwissel gaat via `LocalizationManager.SetLanguage`, die zowel een eigen
actieve cultuur bijhoudt (niet de ambient `CurrentUICulture`, om een cross-thread bug te vermijden
die FontManager eerder tegenkwam) als de thread-cultuur zet voor getal-/datumnotatie. De
taaldropdown toont nu Nederlands/English/Deutsch als tekst; vlaggen bij de dropdown-items volgen
in de lokalisatie-verfijningsfase (feature/screen-editor of later), niet in de scaffolding-fase.

## 6. Releasenummering

Formaat `YYYY.MM.dd.xxx`. `build\buildnumber.txt` houdt de laatste builddatum en teller bij
(teller opnieuw op 1 bij een nieuwe dag). `build\Update-Version.ps1` berekent het nummer en
schrijft `version.generated.props` (niet in git), dat `Directory.Build.targets` importeert zodat elk
project dezelfde `AssemblyVersion`/`FileVersion`/`InformationalVersion` krijgt (zie hieronder voor
waarom `.targets` en niet `.props`). `build\Build.ps1`
roept dit vóór `dotnet build` aan, dus het nummer is al correct binnen diezelfde build — een
build via Visual Studio zelf (zonder `build\Build.ps1`) hoogt de teller niet verder op en gebruikt
het laatst berekende nummer uit `version.generated.props`. Die fallback werkt dus alleen als dat
bestand al eerder door `build\Build.ps1` is aangemaakt; bestaat het nog niet (bijvoorbeeld een
verse clone die nog nooit via `build\Build.ps1` is gebouwd), dan valt `Directory.Build.props` terug
op `1.0.0.0 (dev)`. Zie §8 voor deze afweging.

`version.generated.props` wordt bewust geïmporteerd vanuit `Directory.Build.targets`, niet vanuit
`Directory.Build.props`. De SDK importeert `.props`-bestanden vóór en `.targets`-bestanden ná de
inhoud van het eigen `.csproj`; staat het berekende versienummer in `.props`, dan wint een
letterlijke `AssemblyVersion`/`FileVersion` die ergens in een `.csproj` terechtkomt (bijvoorbeeld
via Visual Studio's Assembly Information/Package-scherm) altijd. Dat is precies gebeurd tijdens het
testen van fase 1: Visual Studio had `2026.9.2.4` als vaste waarde in beide `.csproj`-bestanden
weggeschreven, waardoor `FileVersion` niet meer meegroeide met nieuwe builds. Door de import in
`.targets` te zetten, wint het berekende versienummer altijd, ook als dat opnieuw gebeurt.

## 7. Syncfusion-licentie

`syncfusionlicense.txt` staat bewust buiten de repo op
`%LocalAppData%\InnoSetupStudio\license\syncfusionlicense.txt`. `LicenseService` leest en
registreert die bij het opstarten, vóórdat er iets wordt getoond; ontbreekt het bestand, dan
start de app gewoon door (Syncfusion-controls tonen dan een watermerk).

## 8. Fasering

**Fase 1 — Solution scaffolding (gebouwd)**
Solution/projectstructuur, 9 kleurthema's, lokalisatie NL/EN/DE, splashscreen met releasenummer,
automatische versienummering, Syncfusion-licentie verplaatst en ingeladen, startvenster met
werkende thema-/taalwissel als bewijs dat alles live doorwerkt.

**Fase 2 — Projectinstellingen (gebouwd)**
Scherm voor naam, ontwikkelaar, contactgegevens, bestandslocaties en installer-icon.

**Fase 3 — Schermselectie (gebouwd)**
Overzicht met checkboxen en herkenningsiconen om wizardschermen aan/uit te zetten (zie §11.3 voor
de scope-afbakening ten opzichte van de pixel-perfecte preview uit fase 4).

**Fase 4 — Schermeditor**
Klikbare elementen per wizardscherm, property panel, live doorwerken in de preview.

**Fase 5 — .iss-generatie**
Generator (datamodel → .iss) en parser (bestaand .iss-bestand → datamodel).

**Fase 6 — Pascal Script-editor**
AvalonEdit-gebaseerde editor met syntax highlighting en snippets voor het `[Code]`-blok; validatie
blijft aan ISCC.exe zelf (geen eigen Pascal-compiler/parser).

**Fase 7 — Build-integratie**
ISCC.exe aanroepen vanuit de app, compileerlog tonen, installer direct kunnen starten.

**Fase 8 — Handleiding**
PDF-handleiding per taal, te openen via een help-knop.

## 9. Aannames & open vragen

- De preview van wizardschermen is een eigen WPF-nabootsing van elk standaardscherm, geen live
  render van de echte `setup.exe` — Inno Setup biedt daar geen API voor.
- Deze cloud-sessie schrijft de C#/XAML-code en bouwt/test rechtstreeks op Herberts Windows-pc via
  `mcp__remote-devices__Desktop_Commander`, zodat `dotnet build`/`dotnet test` vanuit deze sessie
  zijn uitgevoerd en gecontroleerd vóór hij het zelf in Visual Studio opent.
- Naamgeving/mapstructuur is afgestemd op de bestaande HNSoftware-projecten (FontManager,
  SVGViewer): Core/App/Tests-split, Themes-map, Localization-map, ResXManager.config.xml.

## 10. Projectmodel en -bestand

`InstallerProject` (in `InnoSetupStudio.Core/Project/`) bevat de algemene projectinformatie uit
fase 2: `AppId` (vast GUID, eenmalig gegenereerd via `InstallerProject.CreateNew`, nodig zodat Inno
Setup een upgrade van een eerdere installatie herkent in plaats van een dubbele installatie),
`AppName`, `AppVersion`, `Publisher`, `PublisherEmail`, `PublisherUrl`, en de bestandslocaties
`SourceFilesPath`, `OutputPath`, `CustomImagesPath` en `SetupIconFile`. Sinds fase 3 bevat het ook
`WizardScreens` (`WizardScreenSelection`): welke van de elf standaard Inno Setup-wizardschermen de
installer toont. Dit model breidt in fase 4 verder uit met schermelementen; de uiteindelijke
generator (fase 5) zet het geheel om naar een `.iss`-bestand.

`JsonInstallerProjectService` bewaart een project als JSON naar een bestand met extensie
`.issproj` (niet te verwarren met het uiteindelijk gegenereerde `.iss`-bestand zelf), met hetzelfde
tijdelijk-bestand-dan-verplaatsen patroon als `JsonSettingsService` voor de app-instellingen.
`ProjectSettingsViewModel`/`ProjectSettingsWindow` (CommunityToolkit.Mvvm) vormen het scherm eromheen,
geopend vanuit `MainWindow` via "Nieuw project" (leeg project, vers AppId) of "Project openen…"
(bestaand `.issproj`-bestand inladen). Zodra een project actief is (nieuw en opgeslagen, of
geopend) onthoudt `MainWindow` het in `_activeProject`/`_activeProjectFilePath` en schakelt de knop
"Wizardschermen" in. De knop naast Opslaan heet "Openen" bij een al bestaand project (die knop
sluit dan alleen het venster, het project blijft actief) en "Annuleren" bij een nieuw, nog niet
opgeslagen project (die knop verwerpt het project dan echt) — `CancelButtonText` bepaalt dit
eenmalig bij het openen van het venster op basis van of er een projectbestandspad is meegegeven.
Opslaan is pas enabled zodra de gebruiker daadwerkelijk een veld wijzigt (dirty-vlag, bijgehouden
via de `On<Property>Changed`-hooks van CommunityToolkit.Mvvm); het openen van een bestaand project
zonder iets te wijzigen laat Opslaan dus uitgeschakeld staan.

`WizardScreensViewModel`/`WizardScreensWindow` vormen het schermenoverzicht uit fase 3: één rij per
standaard wizardscherm (in de volgorde waarin Inno Setup ze doorloopt) met een vinkje, een klein
herkenningsicoon (`Icons.xaml`: `Document`, `Folder`, `List` of `Check`, via
`IconKeyToGeometryConverter`) en een vertaalde naam. Dit is bewust geen pixel-perfecte
voorvertoning van elk scherm — dat is de eigen WPF-nabootsing die in fase 4 wordt gebouwd (zie §1
van de kickoff: Inno Setup heeft geen API om zijn eigen wizardschermen te hergebruiken) — maar een
klein herkenningsicoon per scherm. Opslaan schrijft de gekozen `WizardScreenSelection` terug
naar `_activeProject` en bewaart die meteen naar het actieve `.issproj`-bestand.

## 11. Status

### 11.1 Fase 1: solution scaffolding (2026-09-02)

Solution met `InnoSetupStudio.Core`, `InnoSetupStudio.App` en `InnoSetupStudio.Tests` opgezet op
.NET 10. Thema's, lokalisatie, splashscreen, automatische versienummering en de Syncfusion-licentie
zijn gebouwd en lokaal getest (`build\Build.ps1`, `dotnet test`, app handmatig gestart en weer
gesloten). Bewuste vereenvoudiging: de taaldropdown toont nog geen vlaggen (zie §5); dat volgt in
een latere fase. Nog niet gecontroleerd: de leesbaarheid van de lichte Rood- en Groen-thema's is
alleen visueel steekproefsgewijs bekeken, geen aparte contrastcheck per tekst/achtergrond-
combinatie; staat open als aandachtspunt voor een latere fase.

### 11.2 Fase 2: projectinstellingen (2026-09-02)

`InstallerProject`-model, JSON-opslag (`.issproj`) en het projectinstellingen-scherm gebouwd en
lokaal getest (`build\Build.ps1`, `dotnet test`, app handmatig gestart en weer gesloten).
`InstallerProjectTests` voegt zeven nieuwe fase-2-tests toe (twee voor `AppId`-generatie, één
round-trip-test voor de JSON-opslag, twee voor het retry-gedrag bij vergrendelde bestanden en twee
voor het afwijzen van een ongeldig projectbestand — te groot of JSON null); samen met
`AppSettingsTests` uit fase 1 telt de suite in totaal acht tests. Nog niet automatisch getest: het scherm zelf
(velden invullen, bladeren-knoppen, opslaan/annuleren) — dat vraagt om handmatige verificatie in de
draaiende app, zie de testpunten in de pull request.

### 11.3 Fase 3: wizardschermen-selectie (2026-09-02)

`WizardScreenSelection`-model, het schermenoverzicht (`WizardScreensViewModel`/
`WizardScreensWindow`) en de knop "Wizardschermen" in `MainWindow` gebouwd en lokaal getest
(`build\Build.ps1`, `dotnet test`, app handmatig gestart en weer gesloten). De round-trip-test voor
`JsonInstallerProjectService` is uitgebreid met alle elf `WizardScreenSelection`-velden; geen
nieuwe test-methoden, dus de suite blijft op acht tests. `WizardScreensViewModel` heeft, net als
`ProjectSettingsViewModel` in fase 2, geen eigen unit tests: de weinige logica erin (rijen opbouwen,
`ToSelection`) leent zich niet goed voor losstaand testen zonder de WPF-app zelf op te starten, dat
vraagt net als het scherm zelf om handmatige verificatie in de draaiende app. Bewuste
vereenvoudiging: de knop "Wizardschermen" wordt pas actief zodra een project actief is (nieuw
project opgeslagen, of een bestaand project geopend); dat raakt ook een bestaande beperking uit
fase 2 die hier verholpen is — een geopend project werd pas "actief" na een expliciete Opslaan-klik
in het projectinstellingen-scherm, ook als de gebruiker daar niets wilde wijzigen en meteen
Annuleren klikte.

### 11.4 UX-verfijning: projectinstellingen-scherm (2026-09-03)

Naar aanleiding van handmatig testen: de knop naast Opslaan heette altijd "Annuleren", terwijl die
bij een al geopend (bestaand) project feitelijk alleen het venster sluit zonder iets te wijzigen —
het project blijft actief, er wordt niets verworpen. De knop toont nu "Openen" in dat geval en
"Annuleren" alleen nog bij een nieuw, nog niet opgeslagen project (waar de knop het project wél
echt verwerpt). Daarnaast staat Opslaan pas aan zodra er echt een veld gewijzigd is, in plaats van
zodra alleen de naam ingevuld is: een net geopend, ongewijzigd project liet Opslaan eerder al
enabled zien terwijl er niets te bewaren viel. Beide punten zitten in `ProjectSettingsViewModel`
(`CancelButtonText`, dirty-tracking via `_isDirty`/`MarkDirty`) en zijn niet los geautomatiseerd
getest — net als de rest van dit scherm vraagt dit om handmatige verificatie in de draaiende app.
Build en de bestaande testsuite (acht tests) blijven ongewijzigd groen. Een CodeRabbit-review op
deze wijziging vond nog een regressie (de "Openen"-knop zette het project niet meer actief, een
bijwerkingsfout van het hernoemen zonder de onderliggende logica aan te passen) en twee kleinere
punten (Opslaan kon actief blijven staan na het leegmaken van AppName; de invoervelden waren niet
beschermd tegen een wijziging tijdens de lopende save); alle drie gefixt vóór het mergen naar main.

### 11.5 Herbruikbare dirty-tracking basisklasse (2026-09-03)

De "Openen"/"Annuleren"-aanpassing uit §11.4 gold alleen voor het projectinstellingen-scherm. Op
verzoek is hetzelfde principe nu ook toegepast op het wizardschermen-scherm, en generiek gemaakt
voor toekomstige bewerkschermen: de nieuwe abstracte basisklasse `DirtyTrackingViewModel` houdt bij
of de gebruiker sinds het openen daadwerkelijk iets heeft gewijzigd (`IsDirty`, met een
`BeginInit`/`EndInit`-guard zodat het vullen van de velden bij het openen zelf niet als wijziging
telt) en stelt op basis daarvan `CancelButtonText`/`CancelButtonIconKey` beschikbaar: "Sluiten" met
een nieuw pijltje-icoon (`ArrowLeft` in `Icons.xaml`) zolang er niets te verliezen valt, "Annuleren"
met het bestaande kruis zodra dat wel zo is. `ProjectSettingsViewModel` en `WizardScreensViewModel`
erven nu allebei van deze basisklasse. `ProjectSettingsViewModel` overschrijft beide leden om zijn
eigen, specifiekere gedrag te behouden (het gaat daar niet om de dirty-status maar om of het project
al bestaat: "Openen" met een map-icoon versus "Annuleren" met een kruis, ongewijzigd sinds §11.4).
`WizardScreensViewModel` gebruikt het standaardgedrag van de basisklasse: elke rij (`WizardScreenRow`)
is een los object buiten het source-generated eigenschapssysteem van de ViewModel zelf, dus in
plaats van de gebruikelijke `On<Property>Changed`-hook abonneert de constructor zich na het opbouwen
van de rijen op ieders `PropertyChanged` om `MarkDirty()` aan te roepen. Beide vensters
(`ProjectSettingsWindow.xaml`, `WizardScreensWindow.xaml`) binden de knop naast Opslaan nu via de
bestaande `IconKeyToGeometryConverter` aan `CancelButtonIconKey`, in plaats van de eerdere
Style/DataTrigger-opzet in het projectinstellingen-scherm. Bewuste afbakening: het Opslaan-commando
van het wizardschermen-scherm is niet aan `IsDirty` gekoppeld (dat viel buiten het gevraagde). Build
en de bestaande testsuite (negen tests) blijven ongewijzigd groen; net als de rest van deze twee
schermen is dit niet los geautomatiseerd getest, maar wel handmatig geverifieerd door de app te
starten en te stoppen.
