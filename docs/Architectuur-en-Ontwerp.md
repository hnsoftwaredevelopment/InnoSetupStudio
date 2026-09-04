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

### 11.6 Fase 4: schermeditor, eerste PR (2026-09-03)

Eerste stap van de schermeditor: de inhoud van losse wizardschermen bewerken, in plaats van ze
zoals in fase 3 alleen aan of uit te zetten. Elf standaardschermen in één keer bouwen werd een te
grote PR, dus deze PR bevat de herbruikbare basis (linkerlijst, voorvertoning, instellingenpaneel)
plus drie representatieve schermen om dat patroon te bewijzen: Welkom (geen instellingen, puur
voorvertoning op basis van naam/versie), Licentieovereenkomst (bestand kiezen, voorvertoning toont
de inhoud) en Installatiemap kiezen (standaardmap en of de gebruiker die mag wijzigen). De overige
acht standaardschermen volgen in latere PR's van deze fase; een aangevinkt scherm zonder editor
verschijnt nog niet in de schermeditor, met een toelichting in het venster als geen van de drie
al-ondersteunde schermen aan staat.

Nieuw project `InnoSetupStudio.Wizard` (WPF class library, verwijst alleen naar
`InnoSetupStudio.Core`), zoals bij de kickoff voorgesteld: de voorvertoning-UserControls staan
hier apart van `InnoSetupStudio.App`, die er zelf naar verwijst. Deze UserControls kennen de
ViewModel-klassen niet rechtstreeks (dat zou een cirkelverwijzing met App geven); de binding werkt
via de DataContext die WPF automatisch doorgeeft aan een DataTemplate, dezelfde reden waarom het
hele project overal `DynamicResource` in plaats van `StaticResource` gebruikt voor thema-brushes.
Belangrijk ontwerpbesluit: de voorvertoning zelf gebruikt bewust vaste, niet-thema-afhankelijke
kleuren (wit/zwart, zoals Inno Setup's eigen standaard wizardstijl) in plaats van de brushes van
het actieve Inno Setup Studio-thema — Inno Setup's installer-UI is zelf niet geskind door het
thema van de tool waarmee hij gemaakt is, dus de voorvertoning moet dat ook niet doen. Een
disclaimer-tekst in het venster maakt dat expliciet: dit is een benadering, geen pixel-perfecte
weergave van de echte installer (zie ook de kickoff-notitie hierover in §1).

`InstallerProject` (Core) kreeg drie nieuwe velden voor deze schermen: `LicenseFilePath`,
`DefaultDirName` en `AllowUserToChangeDir`. Rond generieke JSON-serialisatie hoefde niets aan te
passen, die velden serialiseren automatisch mee. `WizardEditorViewModel` (nieuw,
`DirtyTrackingViewModel`) bouwt de schermenlijst op basis van welke van de drie schermen aan staan
in `WizardScreenSelection`, met Terug/Volgende-navigatie tussen de voorvertoningen; net als
`WizardScreensViewModel` in fase 3 staat Opslaan hier niet uit zolang er niets gewijzigd is. Elk
scherm heeft een eigen editor-ViewModel (`WelcomePageEditorViewModel`,
`LicensePageEditorViewModel`, `SelectDestinationPageEditorViewModel`, in
`InnoSetupStudio.App.ViewModels.Screens`) die zowel de voorvertoning (via een keyless, op type
gebaseerde `DataTemplate`) als het instellingenpaneel rechts (via een expliciete
`PropertyPanelTemplateSelector`, nodig omdat hetzelfde VM-type daar een andere template heeft dan
in de voorvertoning) van data voorziet.

Nieuwe knop "Schermen bewerken" in `MainWindow`, naast de bestaande "Wizardschermen"-knop (die
blijft aan/uit vinken; deze nieuwe knop bewerkt de inhoud), met een nieuw potlood-icoon in
`Icons.xaml`. Build en de bestaande testsuite (negen tests) blijven ongewijzigd groen; net als de
rest van de schermeditor is dit niet los geautomatiseerd getest, wel handmatig geverifieerd door
de app te starten, te bevestigen dat hij reageert, en weer te stoppen — de daadwerkelijke UI-flow
(schermen aan/uit zetten, bewerken, Opslaan/Sluiten) is aan Herbert om in de draaiende app te
testen, zoals gebruikelijk bij dit soort WPF-schermen in dit project.

**Later toegevoegd aan dezelfde PR:** Herbert's uiteindelijke doel is dat bewerkbare plekken in de
voorvertoning zelf zichtbaar worden (een potlood-icoon, rechtermuisknop-menu erop, bijvoorbeeld om
een achtergrondafbeelding te kiezen). Dat rechtsklik-interactiepatroon zelf komt als aparte feature
zodra er meer schermen zijn om het op te beproeven, maar één bouwsteen die daar los van staat en nu
al nuttig is, is meteen meegenomen: `IProjectAssetService`/`ProjectAssetService` (Core, vier nieuwe
tests) kopieert een door de gebruiker gekozen bestand naar een vaste `Assets`-submap naast het
projectbestand zodra dat bestand van buiten de projectmap komt, zodat een project zelf verplaatsbaar
blijft (een verwijzing naar een bestand ergens anders op de oorspronkelijke schijf zou bij het
verplaatsen van de projectmap stukgaan). Bij een nog niet opgeslagen project, of een bestand dat al
in de projectmap staat, gebeurt er niets. Deze voorziening is nu gekoppeld aan de bestaande
Bladeren-knop van de licentiepagina; toekomstige "kies een bestand"-knoppen (zoals een
achtergrondafbeelding) kunnen dezelfde voorziening hergebruiken in plaats van elk hun eigen
kopieerlogica te bouwen.

**CodeRabbit-ronde op dezelfde PR:** vijf van de zes opmerkingen zijn tegen de actuele code
geverifieerd en direct verwerkt: (1) `ProjectSettingsViewModel.SaveAsync` bouwde een nieuw
`InstallerProject` op zonder `LicenseFilePath`/`DefaultDirName`/`AllowUserToChangeDir` mee te
nemen (hetzelfde patroon als `_wizardScreens` al oploste voor de wizardschermen-selectie) —
zonder deze fix zette het simpelweg openen en opslaan van het algemene instellingenscherm de net
in de schermeditor gekozen licentie/installatiemap-instellingen stilzwijgend terug; opgelost door
dezelfde bewaar-en-hernemen-aanpak als `_wizardScreens`. (2) `LicensePageEditorViewModel` las
`LicenseFilePath` rechtstreeks met `File.ReadAllText`, ook wanneer dat pad uit een geladen
`.issproj`-bestand komt in plaats van de eigen bladerdialoog; een UNC-pad (`\\host\share\...`) in
een gedeeld projectbestand zou dan zonder gebruikersactie een SMB-verbinding naar die host
opzetten — geblokkeerd met een kleine `IsUncOrDevicePath`-check vóór elke bestandstoegang. (3) de
Bladeren-knop in de schermeditor had geen `AutomationProperties.Name` (een `ToolTip` is geen
vervanging voor wat schermlezers gebruiken). (4) de twee decoratieve keuzerondjes in de
licentievoorvertoning waren met `IsHitTestVisible="False"` wel muisveilig maar nog met Tab te
focussen; `Focusable`/`IsTabStop` op `False` toegevoegd. (5) de "Bladeren"-knop in de
installatiemap-voorvertoning stond vast op `IsEnabled="False"`; deze volgt nu
`AllowUserToChangeDir` (met `IsHitTestVisible`/`Focusable` op `False` blijft de voorvertoning zelf
niet-interactief), zodat de voorvertoning laat zien dat Inno Setup deze knop uitschakelt wanneer de
gebruiker de installatiemap niet mag wijzigen.

Bewust nog niet opgepakt: CodeRabbit's zesde punt dat `ProjectAssetService.Import` een absoluut pad
teruggeeft, waardoor een `LicenseFilePath`-verwijzing na het verplaatsen van de hele projectmap naar
een andere locatie op dezelfde schijf niet meer klopt. Projectrelatieve paden oplossen is een
grotere aanpassing (elke lezer van zo'n pad, inclusief de latere iss-generatie in fase 5, moet dan
tegen de actuele projectmap resolven) die beter in samenhang met die fase 5 wordt ontworpen dan er
nu apart doorheen gefietst; dit is een openstaand punt om apart met Herbert te bespreken.

Ook bewust niet toegevoegd: een geautomatiseerde regressietest voor fix (1). De testsuite dekt tot
nu toe alleen `InnoSetupStudio.Core`; ViewModels in `InnoSetupStudio.App` (zoals
`ProjectSettingsViewModel`) hebben nog geen enkele testdekking, en dat gat dichten vergt een eigen
afweging (project-referentie vanuit de test-assembly naar een WPF-project, mogelijk een STA-thread
in de testrunner) die niet in deze CodeRabbit-opruiming hoort. Handmatig geverifieerd: build (0
warnings, 0 errors), bestaande testsuite (13/13 groen, ongewijzigd) en het opstarten van de app
zonder crash.

### 11.7 Fase 4: wizardafbeeldingen, WizardImageFile/WizardSmallImageFile (2026-09-03)

Herbert merkte op dat een echte Inno Setup-installer op de Welkomstpagina een afbeelding over de
volledige hoogte links toont, wat in de eerste versie van `WelcomePagePreview.xaml` bewust was
weggelaten. Voor dit gat sloten, is uitgezocht wat de twee voorbeeldbestanden in
`C:\Program Files\Inno Setup 7` (`WizClassicImage.bmp`/`WizClassicImage-IS.bmp` en de kleine
variant) precies betekenen: niet iets automatisch geselecteerd op DPI, donkere modus of taal (in
de compiler-broncode van `jrsoftware/issrc` op GitHub zit daar geen logica voor), maar gewoon twee
kant-en-klare varianten van dezelfde afbeelding op verschillende kleurdiepte (4-bit/16 kleuren
versus 8-bit/256 kleuren bij exact dezelfde 164×314- en 55×55-pixelafmetingen). Zelfs Inno Setup's
eigen installer (`setup.iss`) gebruikt geen van beide; de echte ingebouwde standaardafbeelding zit
als resource in de compiler zelf. Herbert heeft zwart/wit-versies van beide (`-IS`, 256 kleuren op
zijn keuze) klaargezet in zijn Obsidian-map; deze zijn overgenomen als meegeleverde
standaardafbeelding.

Nieuwe velden `WizardImageFile`/`WizardSmallImageFile` op `InstallerProject` (Core), zelfde
leeg-is-nog-niet-aangepast-gedrag als `LicenseFilePath`. De twee standaardafbeeldingen
(`WizardImage-Default.bmp`/`WizardSmallImage-Default.bmp`) zijn als WPF `Resource` ingebed in
`InnoSetupStudio.Wizard/Assets`, zodat `InnoSetupStudio.App` (die al naar dat project verwijst
voor de preview-UserControls) ze via een `pack://application:,,,/...`-URI kan laden zonder dat
`InnoSetupStudio.Wizard` iets van `InnoSetupStudio.App` hoeft te weten — dezelfde eenrichtings-
afhankelijkheid die de rest van de schermeditor-architectuur al gebruikt. Nieuwe
`WizardImageResolver` (App/Services) vertaalt een projectpad (of leeg) naar een bindbare
`ImageSource`, met dezelfde val-terug-op-de-standaardtekst-aanpak als
`LicensePageEditorViewModel.LoadLicenseText`: een ontbrekend of onleesbaar bestand crasht de
schermeditor niet, maar toont de standaardafbeelding.

Omdat beide afbeeldingen projectbrede instellingen zijn (niet gebonden aan één scherm), staan ze
nu als alleen-lezen `WizardImage`/`WizardSmallImage`-eigenschappen op de basisklasse
`WizardScreenEditorViewModel` in plaats van op een specifiek scherm: `WizardEditorViewModel`
bepaalt ze één keer bij het openen van de schermeditor en geeft ze aan elk scherm door, zodat een
toekomstig scherm dat ze nodig heeft ze automatisch al beschikbaar heeft. `WelcomePagePreview.xaml`
is herschikt van een gecentreerde `StackPanel` naar een `DockPanel` met de afbeelding links
(Width="150" bij 290px hoogte, ter benadering van de 164:314-verhouding) en de tekst ernaast.
`LicensePagePreview.xaml` en `SelectDestinationPagePreview.xaml` kregen een nieuwe kopregel:
titel/omschrijving links, de kleine afbeelding (55×55) rechtsboven, met een dunne scheidingslijn
eronder — zo ziet elke niet-Welkomst/Voltooid-pagina er in een echte installer uit.

De twee bestanden zijn bewerkbaar gemaakt in het projectinstellingen-scherm (niet in de
schermeditor zelf, want het zijn project-brede instellingen zoals `SetupIconFile`, niet
scherminhoud): `ProjectSettingsViewModel` kreeg `WizardImageFile`/`WizardSmallImageFile` plus
`BrowseWizardImageCommand`/`BrowseWizardSmallImageCommand`, die net als de licentiepagina
`IProjectAssetService` gebruiken om een extern gekozen bestand naar de projectmap te kopiëren —
precies het hergebruik dat bij het bouwen van die service al was voorzien. `SaveAsync` is
uitgebreid met de twee nieuwe velden (rechtstreeks vanuit de bindbare eigenschappen, niet via het
bewaar-en-hernemen-patroon van `_wizardScreens`/`_licenseFilePath`, omdat deze velden wél in dit
venster zelf bewerkt worden). Bij het toevoegen van de twee nieuwe Bladeren-knoppen in
`ProjectSettingsWindow.xaml` is meteen `AutomationProperties.Name` meegenomen (de CodeRabbit-les
van de vorige PR), en de bestaande, sinds langer aanwezige Installer-icon-knop in hetzelfde venster
kreeg die toevoeging als kleine bijvangst ook mee.

Build (0 warnings, 0 errors), bestaande testsuite (13/13 groen, ongewijzigd — geen nieuwe Core-
functionaliteit met eigen testbehoefte) en het opstarten van de app zonder crash zijn geverifieerd,
plus een directe controle dat de twee standaardafbeeldingen daadwerkelijk als
`assets/wizardimage-default.bmp`/`assets/wizardsmallimage-default.bmp` in de gecompileerde
`InnoSetupStudio.Wizard.dll` terechtkomen (via `ResourceReader` op de manifest-resources), dus
precies op het pad dat de pack-URI in `WizardImageResolver` verwacht. De daadwerkelijke visuele
weergave in de schermeditor is, zoals gebruikelijk bij dit soort WPF-schermen in dit project, aan
Herbert om in de draaiende app te bevestigen — bevestigd: "de schermen zien er nu uit als uit de
installer".

**Aandachtspunt voor later (nog niet opgepakt):** Herbert vroeg zich af of de Vorige/Volgende-
knoppen (die de schermeditor gebruikt om tussen schermen te navigeren, en die ook in een echte
installer voorkomen) net als de wizardafbeeldingen aanpasbaar zijn. Gecontroleerd in de
runtime-broncode (`Projects/Src/Compiler.ScriptClasses.pas` in `jrsoftware/issrc`):
`WizardForm.NextButton`/`BackButton`/`CancelButton` zijn inderdaad benaderbaar vanuit Pascal
Script, als `TNewButton` (afgeleid van het standaard `TButton`), dus een scriptauteur kan
bijvoorbeeld de knoptekst, zichtbaarheid of lettertype aanpassen (typisch in
`InitializeWizard`/`CurPageChanged`). Dit is dus geen declaratieve `[Setup]`-instelling zoals
`WizardImageFile`, maar puur Pascal Scripting — net als de al eerder besproken rechtsklik-
bewerkpatroon-visie voor afbeeldingen. Voor later: of en hoe dit in de schermeditor wordt
blootgesteld.

**CodeRabbit-ronde op PR #9:** twee van de vier opmerkingen tegen de actuele code geverifieerd en
verwerkt. (1) `WizardImageResolver.Resolve` decodeerde een gekozen afbeelding op volle
bronresolutie voordat de voorvertoning hem verkleind toont (150×290/55×55) — bij een grote foto
als bronbestand onnodig geheugengebruik. Opgelost met `DecodePixelHeight`, ingesteld tussen
`BeginInit`/`EndInit`, op Inno Setup's eigen afmetingen (314 voor de grote afbeelding, 55 voor de
kleine) in plaats van de voorvertoning se eigen pixelmaat, zodat dit losstaat van eventuele
toekomstige lay-outwijzigingen. (2) Zelfde UNC-padrisico als eerder bij `LicenseFilePath`:
`WizardImageResolver` deed `File.Exists`/`BitmapImage.UriSource` rechtstreeks op een pad dat ook
uit een geladen projectbestand kan komen, dus een UNC-pad in een gedeeld project zou zonder
gebruikersactie een SMB-verbinding opzetten. Geblokkeerd met dezelfde `IsUncOrDevicePath`-check
als `LicensePageEditorViewModel`.

Bewust nog niet opgepakt, allebei een uitbreiding van al bestaande, al eerder afgewogen punten:
(3) `ProjectAssetService.Import` wordt in `BrowseForImage` al bij het klikken op Bladeren
aangeroepen (niet pas bij Opslaan), dus bij Annuleren na een keuze kan een ongebruikte kopie in
`Assets` achterblijven, en bij een nog niet opgeslagen project wordt het externe pad ongewijzigd
bewaard. Precies hetzelfde patroon zit al in `LicensePageEditorViewModel.Browse` sinds PR #8 en is
daar door Herbert getest en goedgekeurd; dit nu alleen voor de twee nieuwe afbeeldingsvelden
anders maken zou die twee bladeerknoppen inconsistent met de licentiepagina maken. Hoort bij een
bredere herziening van `ProjectAssetService` (importeren pas bij Opslaan, wezen opruimen bij een
mislukte save), niet bij deze PR. (4) Zelfde projectrelatieve-paden-punt als bij `LicenseFilePath`
in PR #8 (zie hierboven), nu ook van toepassing op `WizardImageFile`/`WizardSmallImageFile` omdat
die dezelfde `ProjectAssetService.Import` gebruiken. Blijft één en dezelfde openstaande
architectuurvraag, niet drie losse.

Build (0 warnings, 0 errors), bestaande testsuite (13/13 groen) en het opstarten van de app zonder
crash zijn opnieuw geverifieerd na deze wijzigingen.

### 11.8 Fase 4: Terug-/Volgende-/Annuleren-knop per scherm aanpasbaar (2026-09-04)

Vervolg op §11.7: Herbert wil dat elk aanpasbaar element van een wizardscherm in Inno Setup Studio
bewerkbaar wordt, ongeacht of Inno Setup dat zelf via een [Setup]-richtlijn (property) of via
Pascal Scripting aanstuurt. De Terug-/Volgende-/Annuleren-knop is het eerste element van de tweede
soort: `WizardForm.BackButton`/`NextButton`/`CancelButton` zijn `TNewButton`-objecten die alleen
via Pascal Script (meestal in een `CurPageChanged`-event) te benaderen zijn, zie het onderzoek
hierover in §11.6.

**Belangrijke constatering vooraf:** fase 5 (.iss-generatie) en fase 6 (Pascal Script-editor)
bestaan nog helemaal niet — er is nog geen enkele plek in de code die een `.iss`-bestand of
Pascal Script genereert, ook niet voor de al bestaande velden zoals `WizardImageFile` (dat wél een
gewone [Setup]-richtlijn is). Deze PR bouwt daarom, net als alle voorgaande fase 4-PR's, alleen het
datamodel en de schermeditor-UI; de daadwerkelijke omzetting naar een `CurPageChanged`-procedure in
het gegenereerde `.iss`-bestand is werk voor fase 5/6, niet voor nu.

**Datamodel:** nieuwe `WizardScreenButtonSettings` (Core) met negen velden: per knop (Back/Next/
Cancel) een `Caption` (string, leeg = Inno Setup's eigen standaardtekst voor die knop op dat
scherm), `Enabled` en `Visible` (beide `bool?`, null = Inno Setup's eigen standaardgedrag blijft
intact, bijvoorbeeld dat Terug op het eerste scherm vanzelf uitstaat). `InstallerProject` krijgt
drie van dit type — `WelcomeScreenButtons`, `LicenseScreenButtons`,
`SelectDestinationScreenButtons` — één per scherm dat al een editor heeft, zelfde opzet als
`WizardScreenSelection`'s elf losse Show*Page-velden: geen dictionary/enum-key, gewoon een
benoemde eigenschap per scherm. De overige acht standaardschermen krijgen zo'n eigenschap zodra hun
editor gebouwd wordt.

**ViewModel-laag:** `WizardScreenEditorViewModel` (basisklasse) werd `abstract partial class` en
kreeg de negen velden als gewone (niet required init) `[ObservableProperty]`'s — anders dan
`WizardImage`/`WizardSmallImage`, want dit zijn per-scherm gegevens, geen projectbrede waarde die
overal hetzelfde is. Een nieuwe `required WizardScreenButtonSettings ButtonSettings`-init-
eigenschap (schrijfalleen, geen backing field) zet die negen velden in één keer, zodat
`WizardEditorViewModel` ze net als `WizardImage`/`WizardSmallImage` via object-initializer-syntax
kan meegeven (`new XPageEditorViewModel(...) { WizardImage = ..., ButtonSettings = ... }`) in
plaats van negen losse constructorparameters. `EffectiveBackButtonCaption`/`EffectiveNextButton
Caption`/`EffectiveCancelButtonCaption` lossen de leeg-is-standaardtekst-regel op (virtuele
`DefaultBackButtonCaption` e.d., overschrijfbaar door een toekomstig scherm zoals Klaar-om-te-
installeren waar Inno Setup zelf al "Install" in plaats van "Next" toont); `IsBackButtonVisible`/
`IsBackButtonEnabled` e.d. lossen de null-is-standaardgedrag-regel op. `ReadButtonSettings()` is de
tegenhanger die de negen velden terugleest voor `WizardEditorViewModel.ApplyTo`.

**Voorvertoning:** de knoppenbalk onderaan de schermeditor-preview toonde tot nu toe alleen een
vaste Terug/Volgende (de eigen navigatie van de schermeditor, niet gekoppeld aan scherminhoud). Nu
tonen Terug/Volgende de `Effective*Caption` van het geselecteerde scherm, en is er een Annuleren-
knop bij gekomen (links, net als in de echte installer) die alleen bestaat om `Effective
CancelButtonCaption`/`IsCancelButtonEnabled`/`IsCancelButtonVisible` te kunnen voorvertonen — hij
heeft geen Command, dus geen eigen functie in de schermeditor. Bewuste asymmetrie tussen de drie
knoppen op het punt Enabled: Terug/Volgende zijn ook de echte navigatie van de schermeditor zelf
(`WizardEditorViewModel.Back/Next`), dus hun `IsEnabled` blijft altijd gestuurd door
`CanGoBack`/`CanGoNext` (anders zou "Volgende uitschakelen op dit scherm" ook navigeren door de
schermeditor blokkeren); hun eventuele uitgeschakeld-staan voor de installer wordt in plaats
daarvan alleen als gedimd uiterlijk getoond (nieuwe `BooleanToOpacityConverter`, 0.4 bij expliciet
`false`). Annuleren heeft geen navigatiefunctie, dus die knop gebruikt `IsCancelButtonEnabled`
gewoon als echte `IsEnabled`. Zichtbaarheid (`Visibility`) is voor alle drie knoppen wél echt: de
linkerlijst met schermen blijft altijd een alternatieve manier om te navigeren, dus een verborgen
Terug/Volgende in de preview kan de schermeditor niet vastlopen.

Het instellingenpaneel kreeg een gedeelde `ButtonSettingsSectionTemplate` (drie subsecties Terug/
Volgende/Annuleren, elk een Caption-veld plus twee driewaardige (IsThreeState) CheckBoxen voor
Enabled/Visible — onbepaald = Inno Setup's eigen gedrag, aan/uit = expliciete override), gebruikt
door alle drie de bestaande schermtemplates via `ContentControl ContentTemplate="{StaticResource
...}"`, in plaats van de negen velden drie keer uit te schrijven. Werkt voor elk schermtype zonder
aanpassing, want de negen velden staan op de basisklasse.

Negen nieuwe vertaalsleutels (NL/EN/DE): `ButtonWizardCancel` (Annuleren-knop's standaardtekst,
zelfde patroon als de bestaande `ButtonWizardBack`/`ButtonWizardNext`), `SectionWizardButtons` en
drie `Label*ButtonSection`-koppen, `LabelButtonEnabled`/`LabelButtonVisible`, en twee hints
(`HintButtonTriState`, `HintButtonCaptionEmpty`) die het leeg/onbepaald-is-standaardgedrag uitleggen
— zelfde soort hint als `LabelDefaultDirNameHint` uit fase 4's eerste PR.

Build (0 warnings, 0 errors), bestaande testsuite (13/13 groen, uitgebreid met round-trip-assertions
voor de negen nieuwe velden) en het opstarten van de app zonder crash zijn geverifieerd. De
schermeditor zelf (knoppenbalk-preview, instellingenpaneel) is nog niet interactief doorgeklikt in
deze sessie — geen schermafbeelding-tooling beschikbaar voor een Windows-desktopapp — dus dat is nog
Herberts eigen visuele controle, zoals bij eerdere PR's in deze fase.

## 12. Configureerbaarheidscatalogus per wizardscherm (2026-09-04)

Herbert wil dat elk aanpasbaar element van elk wizardscherm uiteindelijk bewerkbaar wordt in Inno
Setup Studio, en vroeg om dat eerst per scherm te inventariseren voordat we verder bouwen — welke
elementen zijn generiek (gelden voor de hele wizard) versus scherm-specifiek, en welk mechanisme
(property, vertaalbare tekst, of Pascal Script) zet elk element om. Dit is bewust alleen onderzoek
en vastlegging, geen implementatie: net als §11.8 al vaststelde, bestaat de generator (fase 5) nog
niet, dus er is nog niets om deze elementen ook daadwerkelijk naartoe te vertalen.

Bronnen (Inno Setup 7 is nieuw genoeg dat trainingskennis onbetrouwbaar is; alles hieronder is
geverifieerd tegen de daadwerkelijke `jrsoftware/issrc`-broncode, niet uit het geheugen): de
Pascal Script-klassedefinities in `ISHelp/isxclasses.pas` (dit is letterlijk het bestand waaruit
Inno Setup's eigen "Support Functions"-documentatie wordt gegenereerd), de complete
[Setup]-richtlijnenlijst in `Projects/Src/Shared.SetupSectionDirectives.pas`, en de standaard
Engelse teksten in `Files/Default.isl`.

### 12.1 Drie mechanismen, los van welk scherm

1. **[Setup]-richtlijnen (properties).** Eén waarde in het .iss-bestand, door de generator simpel
   als sleutel-waarde-regel weg te schrijven — zoals `WizardImageFile` nu al werkt.
2. **[Messages]/[CustomMessages] (vertaalbare tekst).** Anders dan een richtlijn: dit zijn
   strings met plaatshouders (`[name]`, `[name/ver]`, automatisch vervangen door Inno Setup zelf)
   die per taal kunnen verschillen, in een apart sectie-blok. Belangrijke constatering hierbij:
   onze huidige Welkomstpagina-voorvertoning (`WelcomePageEditorViewModel`) bootst deze teksten na
   met hardcoded Engelse strings in C#, maar in een echte installer zijn dit zelf ook aanpasbare
   velden (`WelcomeLabel1`/`WelcomeLabel2`) — geen vaste tekst. Iets om rekening mee te houden
   zodra dit scherm een echte editor krijgt.
3. **Pascal Script.** `WizardForm.<Control>.<Eigenschap>`, alleen te zetten via code in een
   `[Code]`-blok, meestal in `CurPageChanged` (per-scherm gedrag) of `InitializeWizard` (eenmalig).
   Zelfde categorie als de Terug-/Volgende-/Annuleren-knop uit PR #10.

### 12.2 Generiek: geldt voor de hele wizard, niet één scherm

Alle onderstaande zijn [Setup]-richtlijnen, bevestigd in `Shared.SetupSectionDirectives.pas`. Een
flink deel is nieuw in Inno Setup 7 (dark-mode-varianten, opacity, achtergrondafbeelding) en dus
niet uit oudere documentatie of trainingskennis te halen:

- `WizardStyle`, `WizardStyleFile` (+ `WizardStyleFileDynamicDark`) — algehele visuele stijl.
- `WizardResizable`, `WizardSizePercent` — venstergedrag/-grootte.
- `WizardImageFile` (+ `WizardImageFileDynamicDark`), `WizardImageStretch`,
  `WizardImageBackColor` (+ `DynamicDark`), `WizardImageOpacity`, `WizardImageAlphaFormat`,
  `WizardKeepAspectRatio` — de grote afbeelding, uitgebreider dan wat PR #9 gebruikt.
- `WizardSmallImageFile` (+ `WizardSmallImageFileDynamicDark`), `WizardSmallImageBackColor`
  (+ `DynamicDark`) — de kleine afbeelding.
- `WizardBackColor` (+ `DynamicDark`), `WizardBackImageFile` (+ `DynamicDark`),
  `WizardBackImageOpacity` — achtergrondkleur/-afbeelding van de hele wizard, los van
  `WizardImageFile`.

**Aandachtspunt:** `WizardImageFile`/`WizardSmallImageFile` uit PR #9 hebben geen
`DynamicDark`-tegenhanger geïmplementeerd — Inno Setup 7 ondersteunt dus een apart donker-thema-
beeld dat we nu niet vastleggen. Mogelijke aanvulling zodra fase 5 dit gaat genereren.

De Terug-/Volgende-/Annuleren-knop (PR #10) is generiek qua mechanisme (drie vaste
WizardForm-knoppen) maar scherm-specifiek qua waarde (elke pagina kan een eigen caption tonen) —
precies het onderscheid dat Herbert voorstelt, en het patroon waar §12.4 op voortbouwt.

### 12.3 Scherm-specifiek: het Welkomstscherm als uitgewerkt voorbeeld

- **Property/tekst:** `WelcomeLabel1`/`WelcomeLabel2` in `[Messages]`/`[CustomMessages]`
  (§12.1-mechanisme 2), met de placeholders `[name]` en `[name/ver]`.
- **Pascal Script — labels:** `WizardForm.WelcomeLabel1`/`WelcomeLabel2` zijn `TNewStaticText`:
  `Caption`, `Color`, `Font` (naam/grootte/stijl/kleur), `Alignment`, `WordWrap`, `Visible`,
  `Left`/`Top`/`Width`/`Height` zijn allemaal schrijfbaar.
- **Pascal Script — afbeelding:** `WizardForm.WizardBitmapImage` (`TBitmapImage`, gedeeld met de
  Voltooid-pagina): `Bitmap`/`PngImage` (dus per code-moment te wisselen, ook al is het
  [Setup]-veld projectbreed), `BackColor`, `Stretch`, `Center`, `ReplaceColor`.
- **Achtergrondkleur van dit ene scherm:** `WizardForm.WelcomePage` is zelf een
  `TNewNotebookPage` met een eigen `Color`-eigenschap — dus ja, een andere achtergrondkleur voor
  alleen de Welkomstpagina kan, los van de generieke `WizardBackColor` uit §12.2.
- **Extra elementen toevoegen:** ja, in principe. Pascal Script kan een nieuwe
  `TNewStaticText`/`TNewEdit`/`TBitmapImage`/`TNewCheckBox` (etc.) aanmaken en op
  `WizardForm.WelcomePage.Surface` parenten — bijvoorbeeld een extra tekstblok. Een kant-en-klare
  datumveld-/kalendercontrol bestaat niet in Inno Setup's Pascal Script-klassen
  (`ISHelp/isxclasses.pas` heeft geen `TDateTimePicker` of vergelijkbaar); dat zou zelf met een
  `TNewEdit` plus validatie gebouwd moeten worden, niet met een ingebouwde control.
- **Een heel nieuw scherm (in plaats van een element op een bestaand scherm):** apart mechanisme,
  de `TWizardPage`/`CreateCustomPage`-familie — groter dan "een element toevoegen aan een
  bestaand scherm", een eigen toekomstige stap, niet iets om nu al in deze catalogus in detail uit
  te werken.

### 12.4 Voorstel: cascaderend standaardgedrag

Herberts idee, uitgewerkt tot een concreet ontwerp: in plaats van "leeg/onbepaald = Inno Setup's
eigen standaard" (het huidige gedrag sinds PR #10), wordt de regel "leeg/onbepaald = de
dichtstbijzijnde eerdere scherm in Inno Setup's eigen volgorde dat wél een expliciete waarde heeft,
en anders pas Inno Setup's eigen standaard". Geen kopieeractie nodig — er wordt niets naar latere
schermen weggeschreven, alleen de *resolutie* (de bestaande `Effective*`-eigenschappen uit PR #10)
zoekt straks terug door de schermenlijst. Voordelen: een scherm dat afwijkt breekt de keten alleen
vanaf dat punt ("wie wil afwijken kan dat"), en er is geen aparte boekhouding nodig voor
"expliciet ingesteld" versus "overgenomen" — dat volgt vanzelf uit of het veld op dat scherm zelf
leeg is. Van toepassing op scherm-specifieke instellingen (knoppen, tekst, kleur van één scherm);
niet op de generieke §12.2-instellingen, die zijn toch al projectbreed en hebben dus geen "vorige
scherm"-keten nodig.

Nog niet gebouwd — dit is een ontwerprichting, geen implementatie. Zodra we de knoppen-resolutie
(of een volgend scherm-specifiek element) uitbreiden, is dit de aanpak.

### 12.5 Vervolg

Voor de overige tien standaardschermen volgt dezelfde inventarisatie (§12.1-mechanisme × generiek/
scherm-specifiek) zodra hun editor aan de beurt is in fase 4 — zelfde scope-afbakening-per-PR-
aanpak als tot nu toe, nu alleen vooraf uitgezocht in plaats van tijdens het bouwen.
