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
  `Project/` bevat het projectmodel (`InstallerProject`) en de opslag ervan (zie §10).
- `InnoSetupStudio.App` — WPF-shell: `Themes/` (9 kleurthema's + Styles.xaml + ThemeManager),
  `Localization/` (LocalizationManager + LocExtension), `Resources/` (Strings.*.resx + Icons.xaml),
  `Services/` (LicenseService), `ViewModels/` (CommunityToolkit.Mvvm-gebaseerde viewmodels),
  `Views/` (SplashWindow, MainWindow, ProjectSettingsWindow).
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

**Fase 2 — Projectinstellingen**
Scherm voor naam, ontwikkelaar, contactgegevens, bestandslocaties en installer-icon.

**Fase 3 — Schermselectie**
Overzicht met checkboxen en preview-thumbnails om wizardschermen aan/uit te zetten.

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
`SourceFilesPath`, `OutputPath`, `CustomImagesPath` en `SetupIconFile`. Dit model breidt in latere
fasen uit met schermselectie (fase 3) en schermelementen (fase 4); de uiteindelijke generator
(fase 5) zet het geheel om naar een `.iss`-bestand.

`JsonInstallerProjectService` bewaart een project als JSON naar een bestand met extensie
`.issproj` (niet te verwarren met het uiteindelijk gegenereerde `.iss`-bestand zelf), met hetzelfde
tijdelijk-bestand-dan-verplaatsen patroon als `JsonSettingsService` voor de app-instellingen.
`ProjectSettingsViewModel`/`ProjectSettingsWindow` (CommunityToolkit.Mvvm) vormen het scherm eromheen,
geopend vanuit `MainWindow` via "Nieuw project" (leeg project, vers AppId) of "Project openen…"
(bestaand `.issproj`-bestand inladen).

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
