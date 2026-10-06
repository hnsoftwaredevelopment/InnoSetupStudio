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

CodeRabbit's review op PR #10 leverde drie bevindingen op, alle drie verwerkt: (1) de drie
Caption-TextBoxen in het gedeelde knoppenpaneel misten een toegankelijke naam voor
schermlezers — opgelost met hetzelfde `AutomationProperties.LabeledBy`-patroon dat
`ProjectSettingsWindow.xaml` al gebruikt (een `x:Name` op het bijbehorende `TextBlock`-label,
waarnaar de TextBox verwijst); (2) een handmatig bewerkt of ouder projectbestand met expliciete
JSON-null voor `WelcomeScreenButtons`/`LicenseScreenButtons`/`SelectDestinationScreenButtons` gaf
een NullReferenceException zodra de schermeditor werd geopend — opgelost door dezelfde
`??= new()`-normalisatie toe te passen die `WizardScreens` al had, met een nieuwe regressietest;
(3) de round-trip-test dekte alleen ingevulde waarden voor `WelcomeScreenButtons`, niet voor
`LicenseScreenButtons`/`SelectDestinationScreenButtons` — als nitpick optioneel, maar meegenomen
omdat het weinig moeite kostte en de dekking van alle negen velden per scherm compleet maakt.
Testsuite na deze wijzigingen: 14/14 groen.

### 11.9 Fase 4: Standaardscherm en drielaags-resolutie voor knoppen (2026-09-04, vervolg)

Vervolg op §13's "Nu"-beslissing: het Standaardscherm en de drielaags-resolutie uit §12.6/§12.7
gebouwd, bewust beperkt tot wat er al is — de knoppen (`WizardScreenButtonSettings`). Kleuren,
lettertypen en het verplaatsen van de wizardafbeeldingen blijven bij §13's "Later".

**Model.** `InstallerProject.DefaultScreenButtons` (`WizardScreenButtonSettings`, net als de drie
bestaande schermvelden), met dezelfde `??= new()`-normalisatie in `JsonInstallerProjectService.
LoadAsync` als de andere drie tegen een expliciete JSON-null.

**Drielaags-resolutie.** `WizardScreenEditorViewModel` (de basisklasse van Welkom/Licentie/
Bestemming) kreeg een `required DefaultScreenEditorViewModel Defaults`-eigenschap naast de
bestaande `ButtonSettings`. De Effective*/Is*-eigenschappen zijn uitgebreid van twee naar drie
lagen: eigen waarde op het scherm zelf → anders de waarde van `Defaults` → anders pas Inno Setup's
eigen ingebouwde standaard (zoals in PR #10). `Defaults` is, anders dan `ButtonSettings`, geen
eenmalige kopie maar een levende referentie naar dezelfde `DefaultScreenEditorViewModel`-instantie
voor de hele schermeditor-sessie: de custom init-accessor abonneert zich op `PropertyChanged` van
die instantie, zodat een wijziging op het Standaardscherm meteen in de andere schermen'
voorvertoning doorwerkt zonder dat de gebruiker iets opnieuw hoeft te openen.

**Nieuwe klasse: `DefaultScreenEditorViewModel`.** Erft bewust NIET van `WizardScreenEditorViewModel`:
die basisklasse vraagt om `WizardImage`/`WizardSmallImage` voor een live installervoorvertoning, en
het Standaardscherm heeft (nog) geen voorvertoning — §12.6 liet die vraag open, "geen voorvertoning"
is voorlopig de eenvoudigste van de twee genoemde opties. Heeft verder dezelfde negen knopvelden,
een `Title`/`IconKey` (icoon "Edit", bewust anders dan Document/Folder van de echte schermen) en
een `ReadButtonSettings()`, in dezelfde vorm als de basisklasse.

**`WizardEditorViewModel`.** Maakt één `DefaultScreenEditorViewModel` per sessie, geeft die aan elk
scherm door via `Defaults`, en telt wijzigingen erop mee voor de dirty-status. `SelectedScreen` is
verbreed van `WizardScreenEditorViewModel?` naar `object?`, want het Standaardscherm deelt bewust
geen basisklasse met de echte schermen; `SelectedIndex`/`Back`/`Next` gaan expliciet met een
type-check om met het geval dat het Standaardscherm geselecteerd is (dan altijd buiten de
Terug-/Volgende-navigatie van de echte schermen, `SelectedIndex` = -1). Twee nieuwe eigenschappen
`IsDefaultScreenSelected`/`IsRealScreenSelected` (zelfde niet-inverterende-`BooleanToVisibilityConverter`-
patroon als `HasScreens`/`HasNoScreens`) sturen de UI hieronder aan.

**UI (`WizardEditorWindow.xaml`).** Linkerlijst: het Standaardscherm in een eigen `ListBox` met
precies één item, een `Separator`, en daaronder de bestaande lijst met echte schermen — twee
losse `ListBox`en die allebei two-way naar dezelfde `SelectedScreen` binden (selecteren in de ene
lijst laat de andere vanzelf zijn markering verliezen, geen extra code nodig). Beide lijsten delen
nu `ScreenRowTemplate` (uit de eerder inline `ListBox.ItemTemplate` getrokken), want
`DefaultScreenEditorViewModel` heeft dezelfde `Title`/`IconKey`-eigenschapsnamen als de echte
schermen. De installervoorvertoning (met de knoppenbalk) is verborgen zodra het Standaardscherm
geselecteerd is en vervangen door een toelichtende tekst in hetzelfde kader; het instellingenpaneel
rechts kreeg een vierde `DataTemplate` (`DefaultScreenPropertyPanelTemplate`) die dezelfde gedeelde
`ButtonSettingsSectionTemplate` van PR #10 hergebruikt.

**Bewust nog niet gedaan (§13 "Later", ongewijzigd).** Geen zwart/wit-versus-kleur-signalering
(§12.7) — dat is voor tekst-/kleurvelden sowieso nog niet uitgewerkt, en voor de knoppen bewust
uitgesteld tot na dit patroon zelf beproefd is. Geen rechtermuisklik-contextmenu (§12.7); de negen
velden blijven voorlopig gewone tekstvelden/CheckBoxen, hetzelfde als op de echte schermen. Geen
voorvertoning van het Standaardscherm zelf (§12.6, expliciet nog open) — de toelichtende tekst is
de bewust eenvoudigste tussenoplossing.

**Verificatie.** Build (0 warnings, 0 errors), testsuite (14/14 groen — de bestaande round-trip- en
null-normalisatie-tests uitgebreid met `DefaultScreenButtons` in plaats van nieuwe tests erbij) en
het opstarten van de app zonder crash zijn gecontroleerd. De schermeditor zelf (linkerlijst met de
nieuwe rij, omschakelen tussen voorvertoning en toelichting, drielaags-resolutie in de
voorvertoning) is nog niet interactief doorgeklikt in deze sessie, zelfde beperking als bij eerdere
PR's in deze fase — dat is Herberts eigen visuele controle. Die controle ving meteen een echte
regressie op, zie hieronder.

**Bugfix: InvalidCastException bij het openen van de schermeditor (2026-09-04, zelfde dag).** Bij
het eerste handmatige doorklikken (bestaand project → Schermen bewerken) crashte de schermeditor
direct met `Unable to cast object of type 'WelcomePageEditorViewModel' to type
'DefaultScreenEditorViewModel'`. De volledige stacktrace (verkregen door `App.xaml.cs`'s
`OnDispatcherUnhandledException` uit te breiden met een `crash-log.txt` naast de .exe — voorheen
toonde de MessageBox alleen `e.Exception.Message`, zonder stacktrace, wat root-causen tot dan toe
onmogelijk maakte) wees de oorzaak aan:

```
at <>z__ReadOnlySingleElementList`1.System.Collections.IList.Contains(Object value)
at System.Windows.Controls.Primitives.Selector.CoerceSelectedItem(...)
```

`WizardEditorViewModel` vulde `DefaultScreenRow` met de collectie-expressie `[_defaultScreen]`.
Omdat de eigenschap van het type `IReadOnlyList<T>` is en de expressie precies één element bevat,
bakt de C#-compiler dit in tot een intern eenmalig-element-type
(`<>z__ReadOnlySingleElementList<T>`). De expliciete `IList.Contains(object)`-implementatie van
dát type cast het argument ongeconditioneerd naar `T` in plaats van eerst te controleren of het
argument wel van dat type is. `WizardEditorWindow.xaml` bindt twee `ListBox`en (het Standaardscherm
in zijn eigen rij, de echte schermen eronder) two-way aan dezelfde `SelectedScreen`-eigenschap
(§11.9 hierboven) — zodra WPF's `Selector` die gedeelde waarde coert, roept het voor de
Standaardscherm-`ListBox` `Contains(SelectedScreen)` aan op `DefaultScreenRow` om te bepalen of de
huidige selectie daar wel in zit. Zodra `SelectedScreen` een echt scherm is (bijvoorbeeld het
Welkomstscherm, standaard al geselecteerd bij het openen) crasht die aanroep, in plaats van gewoon
"nee" terug te geven.

**Fix.** `DefaultScreenRow = new List<DefaultScreenEditorViewModel> { _defaultScreen };` in plaats
van de collectie-expressie. Een gewone `List<T>` heeft wél een veilige `IList.Contains`
(`IsCompatibleObject`-controle vóór het casten), dus dat gebruiken we hier bewust in plaats van de
kortere `[...]`-syntax. Build en testsuite (14/14) blijven groen na de fix; Herbert heeft de
schermeditor daarna zelf opnieuw doorlopen en bevestigd dat de crash weg is.

**CodeRabbit-feedback op deze fix-commit.** Vier bevindingen, alle vier verwerkt:
1. *Crashlogboek zonder schrijfbare fallback (minor).* `File.WriteAllText` naast de .exe kan een
   `UnauthorizedAccessException` geven als de installatiemap (bijvoorbeeld Program Files) niet
   schrijfbaar is; de lege `catch` verborg dat stilletjes. Fix: bij een fout terugvallen op
   `%LocalAppData%\InnoSetupStudio\crash-log.txt`, die altijd schrijfbaar is.
2. *Standaardscherm onbereikbaar zonder echte schermen (major).* Met alle wizardschermen uit
   (`HasScreens` false) klapte de hele `Grid` in — inclusief de rij van het Standaardscherm, dat
   nochtans altijd bestaat. Fix: de `Grid` is niet langer aan `HasScreens` gekoppeld en blijft
   altijd zichtbaar; `ScreenEditorNoScreens` is nu een aanvullende melding erboven in plaats van
   een vervanging. `WizardEditorViewModel`'s initiële `SelectedScreen` valt bij nul echte schermen
   nu op het Standaardscherm terug (in plaats van op `null`), zodat de schermeditor meteen iets
   bewerkbaars toont.
3. *Foutieve overervingstekst op het Standaardscherm zelf (minor).* De toelichting onder de negen
   knopvelden ("Leeg = neemt de tekst van het Standaardscherm over...") verscheen via de gedeelde
   `ButtonSettingsSectionTemplate` ook op het Standaardscherm-paneel zelf — waar die onzin is, want
   dat scherm kan niet van zichzelf erven. Fix: twee nieuwe resources per taal
   (`HintButtonCaptionEmptyDefaultScreen`/`HintButtonTriStateDefaultScreen`, tekst "... = Inno
   Setup's eigen tekst/standaardgedrag voor dit veld", zonder de overervingszin) en twee
   naam-zonder-gedeelde-basisklasse-eigenschappen (`HintButtonCaptionEmptyText`/
   `HintButtonTriStateText`, zelfde patroon als `Title`/`IconKey`) die elk VM-type zijn eigen tekst
   laten teruggeven; de template bindt nu aan die eigenschappen in plaats van rechtstreeks aan
   `{loc:Loc ...}`.
4. *Inconsistente naamgeving "tweelaags" (minor).* De keten heeft feitelijk drie lagen (eigen
   waarde → Standaardscherm → Inno Setup's ingebouwde standaard); alleen de eerste twee zijn
   instelbaar. Hernoemd naar "drielaags-resolutie" in alle plekken die de huidige stand
   beschrijven (`InstallerProject.cs`, `WizardScreenEditorViewModel.cs`,
   `DefaultScreenEditorViewModel.cs`, §11.9's titel/kopjes hierboven). De historische
   `tweelaags`/`tweetraps`-vermeldingen in §12.4/§12.6/§13 blijven ongewijzigd — die leggen vast
   wat op dát moment in het gesprek de aanpak was, niet de huidige stand.

Testsuite na deze vier fixes: 14/14 groen.

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

**Bijgewerkt in §12.6:** de terugzoekende keten hierboven (kijk naar het vorige scherm, dat naar
zijn vorige scherm, enzovoort) is vervangen door een eenvoudiger tweelaags model met een apart
Standaardscherm. §12.4 blijft staan als vastlegging van hoe het gesprek is verlopen; §12.6 is de
huidige aanpak.

### 12.5 Vervolg

Voor de overige acht standaardschermen (elf in totaal uit §11.6, min de drie die al een editor
hebben: Welkomst-, licentie- en bestemmingsscherm) volgt dezelfde inventarisatie (§12.1-mechanisme
× generiek/scherm-specifiek) zodra hun editor aan de beurt is in fase 4 — zelfde
scope-afbakening-per-PR-aanpak als tot nu toe, nu alleen vooraf uitgezocht in plaats van tijdens
het bouwen.

### 12.6 Standaardscherm: één centrale plek voor cascaderende standaardwaarden (2026-09-04, vervolg)

Herberts vervolgvoorstel op §12.4: in plaats van dat de eerste scherm-aanpassing die de gebruiker
toevallig doet impliciet als standaard voor latere schermen gaat gelden, komt er een apart
"Standaardscherm" naast de echte installerschermen (vóór het Welkomstscherm), waar de gebruiker
achtergrondkleur, tekstkleur, lettertype en standaardwaarden voor de knoppen in één keer vastlegt.
Elk volgend scherm neemt dat over, tenzij de gebruiker op dat ene scherm zelf iets anders instelt.

**Beoordeling.** Dit is een verbetering ten opzichte van het §12.4-voorstel, niet alleen een andere
invulling ervan. Het §12.4-idee (impliciet vanaf het eerste scherm dat je aanpast) heeft een
verrassingsrisico: een gebruiker die scherm 2 aanpast, verwacht niet per se dat scherm 5 daardoor
ook meeverandert — dat voelt als een neveneffect. Een apart, herkenbaar Standaardscherm maakt de
bedoeling expliciet: de gebruiker begrijpt "dit scherm bepaalt de rest, tenzij ik afwijk", in plaats
van dat gedrag impliciet af te leiden uit wélk scherm toevallig het eerst bewerkt is. Vergelijkbaar
met een masterpagina in Word of een basisstijl in CSS — een bekend patroon.

**Herziene resolutie (vervangt de terugzoekende keten uit §12.4).** Twee lagen in plaats van een
keten door alle voorgaande schermen: (1) de expliciete waarde op het scherm zelf, indien ingevuld;
(2) anders de waarde van het Standaardscherm; (3) anders pas Inno Setup's eigen ingebouwde
standaard (zoals nu, "Next >"). Simpeler te begrijpen en te implementeren dan terugzoeken door de
schermenlijst, en zonder het verrassingsrisico hierboven: het aanpassen van scherm 3 raakt nooit
scherm 5, alleen het aanpassen van het Standaardscherm zelf doet dat.

**Waar dit wel en niet op van toepassing is.** Alleen op de Pascal-Script-mechanisme-elementen uit
§12.1, punt 3 (knoppen, teksteigenschappen van labels, per-scherm achtergrondkleur) — dat zijn de
elementen die Inno Setup zelf al toestaat per pagina te laten verschillen. Niet op de platte
[Setup]-richtlijnen uit §12.2 (`WizardImageFile`, `WizardBackColor`, `WizardStyle` en dergelijke):
die zijn in Inno Setup zelf altijd projectbreed, ongeacht wat wij bouwen, dus daar is geen
per-scherm-afwijking mogelijk om te faciliteren. Dat blijft gewoon bij de bestaande
projectinstellingen horen.

**Twee openstaande UI-vragen, nog niet te beslissen, wel te noteren:**

- Hoe laat de UI zien of een veld de standaardwaarde erft of hier expliciet is overschreven?
  Voorstel: het veld toont altijd de opgeloste (geërfde) waarde, met een klein "terug naar
  standaard"-icoon dat verschijnt zodra de gebruiker op dat scherm zelf iets wijzigt. Voor tekst
  werkt "leeg = erft de standaard" al (bestaand patroon sinds PR #10); voor kleuren en lettertypen
  bestaat er geen "leeg", dus die hebben een expliciete null/geen-eigen-waarde-status nodig, zelfde
  aanpak als `Enabled`/`Visible` (`bool?`) bij de knoppen.
- Wat toont de voorvertoning van het Standaardscherm zelf? Het stelt geen bestaand installerscherm
  voor, dus geen 1-op-1 Inno Setup-nabootsing zoals de andere schermen. Kan een generieke
  mockup-pagina worden die de gekozen kleuren/lettertype toont, of voorlopig alleen een
  toelichtende tekst zonder voorvertoning — beide werkbaar, latere keuze.

**Positionering in de schermenlijst.** Niet als "scherm nul" tussen de echte installerschermen,
want dat wekt de indruk dat de eindgebruiker dit ook als scherm te zien krijgt, wat niet zo is. Wel
duidelijk visueel gescheiden (bijvoorbeeld een eigen rij boven een scheidingslijn, ander icoon),
zodat helder blijft dat dit meta-instellingen zijn en geen scherm dat ooit getoond wordt.

**Reactie op het datumveld-punt uit het vorige gesprek:** Herbert benadrukt terecht dat wij geen
nieuwe UI-elementen moeten verzinnen die Inno Setup zelf niet biedt — bestaat het niet als
kant-en-klare Pascal Script-control, dan bouwen wij het ook niet. Dat is al hoe §12.3 het
datumveld-voorbeeld behandelde (geen `TDateTimePicker` in `ISHelp/isxclasses.pas`, dus geen
ondersteuning) en blijft het uitgangspunt voor elk toekomstig "kan de gebruiker element X
toevoegen"-vraag: eerst verifiëren dat Inno Setup het als control aanbiedt, pas dan vastleggen dat
we het kunnen ondersteunen.

Nog niet gebouwd — vastgelegd ter voorbereiding op de keuze voor de eerstvolgende stap.

### 12.7 Visuele taal en interactie: standaard versus aangepast (2026-09-04, vervolg)

Antwoord op de eerste van de twee openstaande UI-vragen uit §12.6 (zwart/wit versus kleur, en het
contextmenu dat daarbij hoort). De tweede vraag — wat de voorvertoning van het Standaardscherm zelf
toont — blijft open zoals in §12.6 vastgelegd; dat is nog geen ontwerpbeslissing, alleen twee
werkbare richtingen.

**Zwart/wit versus kleur.** Herberts voorstel: het meegeleverde standaardbeeld (de zwart/wit
conversie die hij al bij PR #9 koos) blijft het visuele signaal voor "dit is de out-of-the-box
standaard"; zodra de gebruiker zelf iets instelt, wordt dat in volledige kleur getoond. Een eigen
`WizardSmallImageFile` verschijnt dus meteen in kleur. Dit werkt letterlijk voor afbeeldingen, en
sluit direct aan bij een keuze die al in het project zit. Voor tekst- en kleurvelden bestaat geen
letterlijke zwart/wit-versie; daar vertaalt hetzelfde onderliggende principe (gedempt voor
standaard, nadrukkelijk voor aangepast) naar een per-elementtype passende uitwerking, bijvoorbeeld
een gedempte tekstkleur voor een geërfde knopcaption tegenover de volle themakleur voor een
expliciete. De voorvertoning zelf blijft altijd de daadwerkelijk opgeloste waarde tonen (nooit een
kleur die de gebruiker wél gekozen heeft kunstmatig grijs maken) — alleen de visuele nadruk
verschilt tussen geërfd en expliciet.

**Contextmenu in plaats van een reset-icoontje.** Rechtermuisklik op een element geeft een menu dat
per elementtype verschilt, bijvoorbeeld Bewerken/Terug naar standaard voor een afbeelding of
tekstveld, Aan/Uit/Verbergen voor een schakelbaar element. Vervangt het "terug naar
standaard"-icoontje uit §12.6: consistenter (één interactiepatroon voor alle elementen) en
flexibeler (elk elementtype vult het menu met wat daar relevant is).

**Positionering van het Standaardscherm.** Bevestigd: duidelijk visueel gescheiden van de echte
installerschermen (§12.6), niet als scherm nul ertussenin.

**Nieuw punt: platte richtlijnen mogelijk ook via het Standaardscherm.** Herbert opent de vraag of
de platte [Setup]-richtlijnen (§12.2, nu bewerkbaar in de projectinstellingen) misschien ook via
het Standaardscherm ingesteld zouden moeten worden, met de onderliggende schermen die de opgeloste
waarde dan alleen tonen, niet meer bewerkbaar. Dat zou `WizardImageFile`/`WizardSmallImageFile` uit
de projectinstellingen naar het Standaardscherm verplaatsen. Nog geen besluit — zie §13 voor de
afweging of dit nu of later aan de orde komt.

## 13. Bouwvolgorde: wat nu, wat later (2026-09-04)

Herbert heeft gevraagd om, nu §12 goed is vastgelegd, als development-specialist te bepalen wat
handig is om nu al op te zetten en wat beter kan wachten.

**Nu.** Eerst PR #10 en #11 afronden (CodeRabbit-feedback verwerken, Herberts visuele controle,
mergen) — geen van beide is nog gemerged. Daarna het Standaardscherm en de tweelaags-resolutie uit
§12.6 bouwen, maar bewust beperkt tot wat er al is: de knoppen (`WizardScreenButtonSettings`).
Reden: dat is het enige element waar al een datamodel en een schermeditor voor bestaat, dus het is
de kleinste manier om het hele nieuwe patroon (Standaardscherm als apart, visueel gescheiden
scherm; tweelaags-resolutie; zwart/wit-of-kleur-signalering; contextmenu) in de praktijk te
beproeven vóór we het uitbreiden naar elementen die nog niet bestaan.

**Later.** Achtergrond-/tekstkleur en lettertype zijn nieuwe elementen zonder enig bestaand
datamodel (geen `WizardScreenColorSettings` of vergelijkbaar) — dat is een eigen, volwaardige PR
(inclusief hoe een "geen eigen kleur"-status eruitziet, zie §12.6), niet iets om erbij te nemen
zolang het Standaardscherm-patroon zelf nog niet beproefd is. Het verplaatsen van
`WizardImageFile`/`WizardSmallImageFile` van de projectinstellingen naar het Standaardscherm raakt
al werkende, geteste functionaliteit uit PR #9; dat verdient een eigen afweging zodra we zien hoe
het Standaardscherm in de praktijk aanvoelt, niet een meegenomen wijziging in dezelfde PR. De
zwart/wit-signalering voor afbeeldingen hangt af van die keuze en volgt dus ook later. Nieuwe
elementen toevoegen aan een bestaand scherm (via Pascal Script aangemaakte controls) en hele nieuwe
pagina's (`TWizardPage`) blijven ver weg, geen actie nu. De inventarisatie van de overige acht
standaardschermen (§12.5) kan gewoon doorlopen, onafhankelijk van dit alles.

## 14. Backlog na Herberts visuele controle van het Standaardscherm (2026-09-04)

Na het mergen van PR #12 heeft Herbert de schermeditor met het Standaardscherm er echt bekeken en
vijf punten genoemd. Geen van alle is dringend, hij heeft zelf aangegeven dat ze allemaal later
kunnen. Vastgelegd hier zodat ze niet kwijtraken, met een voorstel voor volgorde.

**1. Wizard-afbeeldingen (groot/klein) instelbaar maken in het Standaardscherm.** Nu nog in de
projectinstellingen (`InstallerProject.WizardImageFile`/`WizardSmallImageFile`, PR #9). Dit is
letterlijk de open vraag uit §12.6 hierboven, nu bevestigd als iets dat Herbert wil.

**2. Ontbrekende knop-eigenschappen.** De drie knoppen (Terug/Volgende/Annuleren) hebben nu alleen
Caption/Enabled/Visible. Herbert mist tekstkleur, achtergrondkleur en eventueel een bitmap per
knop. Nieuwe velden op `WizardScreenButtonSettings`, met dezelfde drielaags-resolutie als de
bestaande drie.

**3. Ontbrekende Bladeren-knop op de Bestemmingspagina.** Anders dan de eerste twee punten: dit is
geen nieuw element, maar een gat in wat al gebouwd is. De echte Inno Setup-installer heeft op de
bestemmingspagina een eigen Bladeren-knop waarmee de eindgebruiker een map kan kiezen; die knop
staat niet in `SelectDestinationPageEditorViewModel` of de voorvertoning. Scherm-specifiek (zoals
de licentiepagina's eigen tekst), geen onderdeel van het generieke Terug-/Volgende-/
Annuleren-patroon.

**4. Meertaligheid.** Bij het aanmaken van een project (of in het Standaardscherm) aangeven of de
installer meertalig is en welke talen. Inno Setup's eigen taalbestanden staan op Herberts machine
in `C:\Program Files\Inno Setup 7\Languages\`; een kopie staat ook in
`C:\DevOps\hnsoftwaredevelopment\Inno Setup Studio\Languages\`. Raakt méér dan alleen een nieuw
projectveld: zodra een project meertalig is, wordt elke tekst (knoppen-Caption incluis) in
principe een waarde per taal in plaats van één vaste string — dat vraagt om een eigen ontwerp
voordat het gebouwd wordt, niet iets om terloops mee te nemen.

**5. Knoppen in de voorvertoning tonen in plaats van rechts in het paneel.** In plaats van alle
knop-eigenschappen los in het instellingenpaneel te zetten (wat met punt 2 erbij al snel te veel
wordt), de knoppen zelf in de voorvertoning klikbaar maken; een klik opent een popup met alle
eigenschappen van die knop, met de voorvertoning die direct meeverandert. Een herontwerp van het
instellingenpaneel, geen nieuw datamodel.

**Voorgestelde volgorde, met reden.**

1. **Punt 3 eerst** — kleinste en laagste risico van de vijf. Volgt exact het patroon dat al drie
   keer gebouwd is (Caption/Enabled/Visible + voorvertoning), dicht een gat in bestaand werk, geen
   nieuw ontwerp nodig.
2. **Punt 4 (talenselectie) vóór punt 2 (kleur/bitmap).** Reden: zodra meertaligheid er is, wordt
   Caption op de knoppen mogelijk een waarde per taal in plaats van één string. Eerst kleur/bitmap
   toevoegen aan het huidige (eentalige) model en dáárna meertaligheid bouwen betekent twee keer
   aan diezelfde velden werken. Dit hoeft niet de volledige vertaal-UX te zijn — alleen "welke
   talen ondersteunt dit project" (een meerkeuzelijst uit de Languages-map) volstaat om die
   volgorde-vraag te beantwoorden; hóe teksten per taal worden ingevoerd kan zelf nog later.
3. **Punt 2 (tekstkleur/achtergrondkleur/bitmap).** Natuurlijke uitbreiding van het bestaande
   model, nu tegen de uiteindelijke (mogelijk meertalige) vorm van Caption aan gebouwd in plaats
   van ertegenin.
4. **Punt 1 (wizard-afbeeldingen naar het Standaardscherm).** Kleine verplaatsing, en profiteert
   van dezelfde Bladeren-knop-UI die dan al gebouwd is voor de knop-bitmaps uit punt 2.
5. **Punt 5 (knoppen-in-voorvertoning + popup) als laatste.** Pas zinvol te ontwerpen zodra de
   volledige set knop-eigenschappen bekend is; anders wordt de popup twee keer gebouwd.

Nog geen besluit genomen om hiermee te starten — dit is alleen vastlegging plus een voorstel,
Herbert bepaalt de daadwerkelijke volgorde.

**Update 2026-09-04: uitvoering gestart.** Herbert koos zijn eigen volgorde: eerst punt 3
(Bladeren-knop), dan punt 2 (tekstkleur/achtergrondkleur/bitmap) — bewust vóór punt 4
(meertaligheid), anders dan het voorstel hierboven adviseerde. Reden voor het advies om punt 4
eerst te doen was Caption die mogelijk per taal gaat verschillen; die zorg geldt niet voor
tekstkleur/achtergrondkleur/bitmap, dat zijn geen tekstvelden, dus deze twee punten kunnen zonder
extra rework in willekeurige volgorde.

- Punt 3 (Bladeren-knop op de Bestemmingspagina): PR #13, gemerged. `SelectDestinationPageEditorViewModel.BrowseCommand`
  met `OpenFolderDialog`, zelfde patroon als de licentiepagina.
- Punt 2 (tekstkleur/lettertype), zoals uiteindelijk gescoped — zie de scope-wijziging hieronder:
  op `feature/button-color-bitmap-properties`. Twaalf nieuwe velden op `WizardScreenButtonSettings`
  (vier per knop, voor drie knoppen: `TextColor`, `FontFamily`, `FontSize`, `FontBold`), dezelfde
  drielaags-resolutie
  (eigen waarde → Standaardscherm → Inno Setup's eigen gedrag) als de bestaande negen velden.
  `TextColor` is hex-tekst (`#RRGGBB`) in plaats van `System.Windows.Media.Color`, zodat
  `InnoSetupStudio.Core` WPF-vrij blijft. Vier converters (`HexColorToBrushConverter`,
  `FontFamilyOrUnsetConverter`, `FontSizeOrUnsetConverter`, `NullableBoolToFontWeightConverter`)
  tonen het resultaat live in de voorvertoning, met UnsetValue (niet Transparent/een vaste
  standaardwaarde) als terugvalwaarde bij leeg/ongeldig/null, zodat een niet-ingevulde knop gewoon
  zijn eigen WPF-standaarduiterlijk houdt.

**Scope-wijziging 2026-09-04, na review door Herbert van bovenstaande kanttekening.** Herbert
besloot achtergrondkleur en bitmap op de knop volledig te schrappen (niet alleen uit te stellen):
de extra generatorwerk (zelf getekende knop) staat niet in verhouding tot hoe vaak dit gebruikt
zou worden, met name de bitmap. Tegelijk wees hij op een ontbrekend veld: lettertype
(Font.Name/Size/Style), wél gewone `TFont`-eigenschappen die op een standaard `TNewButton` direct
werken, dus zonder de extra generatorwerk van achtergrondkleur/bitmap. Verder gaf hij aan dat de
Bestemmingspagina zijn eigen, schermspecifieke "Bladeren"-knop (Inno Setup's
`WizardForm.DirBrowseButton`) mist in de eigenschappenlijst — expliciet losstaand van het
Standaardscherm-model, want die knop komt maar op één scherm voor. Alle drie in dezelfde PR #14
meegenomen in plaats van als apart vervolg:

- `BackgroundColor`/`BitmapFilePath` (en de bijbehorende `ButtonBackgroundConverter`,
  knopbitmap-Bladerknoppen en `IProjectAssetService`-plumbing op de basisklasse) volledig
  verwijderd uit `WizardScreenButtonSettings`, `WizardScreenEditorViewModel`,
  `DefaultScreenEditorViewModel` en de schermeditor-XAML.
- `FontFamily` (string)/`FontSize` (int?)/`FontBold` (bool?) toegevoegd aan
  `WizardScreenButtonSettings` en `DefaultScreenEditorViewModel`, met dezelfde drielaags-Effective*-
  resolutie als de overige knopvelden op `WizardScreenEditorViewModel` (`FontFamily` via
  `ResolveCaption`, `FontSize`/`FontBold` via eenvoudige `??`-val omdat er geen "Inno-ingebouwde"
  derde laag bestaat voor deze twee).
- Nieuw model `BrowseButtonSettings` (Enabled/Visible/TextColor/FontFamily/FontSize/FontBold, geen
  Caption) voor de Bestemmingspagina's eigen Bladeren-knop, met een eigen property
  `InstallerProject.SelectDestinationBrowseButton` — bewust géén drielaags-resolutie via het
  Standaardscherm, deze knop komt maar op één scherm voor.

**Kanttekening voor de generator (fase 5/6), nog niet gebouwd.** Inno Setup's eigen knopklasse
(`TNewButton`) ondersteunt `Font.Color`/`Font.Name`/`Font.Size`/`Font.Style` rechtstreeks via
Pascal Script — de generator zet deze velden voor Caption/Enabled/Visible/TextColor/Font* op
dezelfde manier om als de bestaande Caption-velden, geen extra werk nodig. Achtergrondkleur en
bitmap zijn dus ook geen openstaand punt meer voor de generator: die zijn uit het model geschrapt,
niet alleen uitgesteld.

**UX-feedback 2026-09-04, na het bekijken van het resultaat.** Herbert: het aparte
eigenschappenpaneel per knop is de moeite waard (houdt het scherm later rustiger), maar vrije
tekstinvoer voor TextColor/FontFamily is foutgevoelig — je moet toevallig weten dat je
"#08BDA1" nodig hebt, en een lettertypenaam intikken kan altijd een typefout zijn. Twee
toevoegingen, nog in dezelfde PR #14:

- Lettertype-keuzelijst: `SystemFontCatalog` (nieuwe klasse, `InnoSetupStudio.App.Services`) geeft
  `Fonts.SystemFontFamilies` van deze machine terug; de FontFamily-velden zijn nu een bewerkbare
  ComboBox in plaats van een vrije TextBox, met de systeemlettertypen als suggestielijst in plaats
  van verplichte keuze (bewerkbaar gelaten omdat de lettertypen op de doelmachine tijdens
  installatie sowieso kunnen afwijken van deze ontwikkelmachine — zie ook de vraag hieronder over
  lettertypebestanden).
- Kleurenkiezer: een "Kies…"-knop naast elk TextColor-veld (Back/Next/Cancel-knoppen en de
  Bladeren-knop) opent `System.Windows.Forms.ColorDialog` — WPF heeft zelf geen ingebouwde
  kleurenkiezer. Vereist `<UseWindowsForms>true</UseWindowsForms>` in
  `InnoSetupStudio.App.csproj`, wat op zijn beurt de impliciete globale usings voor
  `System.Windows.Forms`/`System.Drawing` moest laten vervallen (`<Using Remove="..." />`): beide
  botsen anders met de gelijknamige WPF-typen (`Application`, `Color`, `ColorConverter`,
  `FontFamily`) die de rest van de app al gebruikt.

**Vraag van Herbert, beantwoord: moet een gekozen lettertype naar de projectmap gekopieerd worden
(zoals License/eerder ook Bitmap via IProjectAssetService)?** Nee — FontFamily is, anders dan
LicenseFilePath, geen bestandspad maar een naamverwijzing naar een lettertype dat op de
doelmachine zelf al geïnstalleerd moet zijn (Pascal Script's `Font.Name` verwijst simpelweg naar
een systeemlettertype, net zoals "Segoe UI" nu al gebruikt wordt zonder dat dat lettertype ooit
in het installerproject terechtkomt). Inno Setup heeft geen ingebouwd mechanisme om een eigen
lettertypebestand mee te installeren en meteen daarna, tijdens de wizard zelf, te gebruiken voor
de knoppen — de wizard-UI is al getekend voordat een eventuele custom-font-installatie zou
kunnen draaien. Een lettertype dat niet op de doelmachine staat, valt in de praktijk terug op
Windows' eigen font-substitutie; dat is een acceptabele beperking, geen bug om op te lossen.

**Twee bugs gemeld door Herbert 2026-09-04, na het uitproberen van de lettertype-keuzelijst.**

1. Een lettertype kiezen in de FontFamily-ComboBox paste de voorvertoning op de knop wél aan, maar
   de ComboBox zelf toonde de gekozen naam niet in zijn eigen tekstveld. Oorzaak: de app heeft een
   eigen `ComboBox`-`ControlTemplate` (`Themes/Styles.xaml`) voor de thema-kleuren, en die had geen
   element met de naam `PART_EditableTextBox` — WPF's `ComboBox` vereist dat exacte template-part
   om tekst te tonen/bewerken zodra `IsEditable="True"` staat. Zonder dat part verandert de
   onderliggende `Text`-eigenschap (en dus de gebonden FontFamily) wél degelijk, maar toont de
   ComboBox nooit wat er feitelijk gekozen is. Opgelost door een `TextBox x:Name="PART_EditableTextBox"`
   toe te voegen (bewust met een `Binding`+`RelativeSource TemplatedParent`+`Mode=TwoWay` in plaats
   van een `TemplateBinding`, want die laatste kan geen tekst terugschrijven naar de ComboBox) plus
   een `IsEditable`-trigger die hem verwisselt met de bestaande alleen-lezen `ContentPresenter`.
2. De Schermeditor (en twee andere popup-vensters: Wizardschermen kiezen, Projectinstellingen)
   stonden vast op `ResizeMode="NoResize"`, waardoor sommige van de nieuwe knopvelden (met name de
   TextColor-rij met de nieuwe "Kies…"-knop erbij) niet meer in de vaste 230px-breedte van het
   instellingenpaneel pasten. Alle drie nu `ResizeMode="CanResizeWithGrip"` met een `MinWidth`/
   `MinHeight` op minstens de oorspronkelijke vaste afmeting. Bij de Schermeditor is de rechter
   Grid-kolom (instellingenpaneel) bovendien van een vaste `230` naar `*` met `MinWidth="260"`
   gegaan, zodat extra vensterbreedte daar terechtkomt in plaats van bij de linkerlijst of de vaste
   497px-brede Inno Setup-voorvertoning ernaast; de standaardbreedte van het venster ging van 980
   naar 1050 zodat het meteen al iets meer ruimte heeft.

## 15. Toolbar voor het hoofdscherm met eigen iconen (vastgelegd 2026-09-04, nog niet gepland)

Herbert heeft drie eigen SVG-iconen gemaakt voor "Nieuw project", "Project openen" en "Project
bijwerken": `project-new.svg`, `project-open.svg` en `project-edit.svg`, in
`C:\DevOps\hnsoftwaredevelopment\Inno Setup Studio\`. Bedoeld voor een toolbar boven in
`MainWindow`, ter vervanging van de huidige knoppen met icon en tekst midden op het scherm.

Geen actie nu, alleen vastgelegd zodat het niet kwijtraakt. Bij het bouwen van deze toolbar hoort
ook gekeken te worden naar `CreateVectorResourceDictionary` (zie de kickoff-werkafspraken, sectie
over het icon-systeem) als basis om deze SVG's als vector-resource in te laden in plaats van losse
bestanden.

## 16. Wizardafbeeldingen naar het Standaardscherm (backlogitem 1, sectie 14) (2026-09-28)

Herbert koos bij het hervatten van het project (na vakantie) zijn eigen volgorde voor de
resterende backlogpunten uit sectie 14: eerst punt 1 (wizardafbeeldingen), daarna punt 4
(meertaligheid).

**Reversal van §12.6.** §12.6 concludeerde destijds nog expliciet dat `WizardImageFile`/
`WizardSmallImageFile` "gewoon bij de bestaande projectinstellingen" horen, omdat het platte
`[Setup]`-richtlijnen zijn (altijd projectbreed, geen per-scherm-afwijking mogelijk). Diezelfde
dag, later in sectie 14, bevestigde Herbert desondanks dat hij deze twee velden op het
Standaardscherm in de schermeditor wil bewerken, niet meer in de projectinstellingen — dat
voorstel is nu uitgevoerd. De projectbrede aard van de velden zelf verandert niet (zie hieronder),
alleen waar de gebruiker ze bewerkt.

**Wat is verplaatst.** `ProjectSettingsWindow`/`ProjectSettingsViewModel` tonen `WizardImageFile`/
`WizardSmallImageFile` niet langer; `DefaultScreenEditorViewModel` heeft nu de twee velden, met
dezelfde Bladeren-knop-en-kopieer-naar-projectmap-flow (`IProjectAssetService`) als een
licentiebestand. `ProjectSettingsViewModel` geeft de twee velden nog wel ongewijzigd door bij het
opslaan (`_wizardImageFile`/`_wizardSmallImageFile`, zelfde pass-through-patroon als
`_wizardScreens`/`_licenseFilePath`/`_defaultDirName`/`_allowUserToChangeDir` al deden) — zonder
die velden zou opslaan vanuit de projectinstellingen een eerder op het Standaardscherm gekozen
afbeelding stilzwijgend terugzetten naar leeg.

**Architectuurwijziging: van eenmalige init-waarde naar live berekende waarde.**
`WizardScreenEditorViewModel.WizardImage`/`WizardSmallImage` waren `required init`-eigenschappen:
`WizardEditorViewModel` loste ze één keer op bij het openen van de schermeditor (via
`WizardImageResolver`) en gaf ze aan elk scherm mee, wijzigbaar pas na een nieuwe sessie. Nu de
gebruiker ze tijdens dezelfde sessie op het Standaardscherm kan wijzigen, moesten ze live
meeveranderen in elk scherm dat ze toont (Welkomst-/Voltooid-pagina's voor `WizardImage`, de
overige pagina's voor `WizardSmallImage`). Beide zijn nu berekende eigenschappen die rechtstreeks
van `Defaults` (de gedeelde `DefaultScreenEditorViewModel`-instantie) lezen, en
`RaiseEffectivePropertiesChanged` — die al bestond voor de Effective*-knopvelden — meldt nu ook
deze twee door zodra het Standaardscherm wijzigt. Geen drielaags-resolutie zoals de knoppen: er is
geen "eigen waarde per scherm" voor een projectbrede afbeelding om naar terug te vallen, de twee
velden op het Standaardscherm zijn de enige waarde.

**Kleine thumbnail, geen mockup-voorvertoning.** De tweede openstaande UI-vraag uit §12.6 (wat
toont de voorvertoning van het Standaardscherm zelf) blijft bewust open — geen volledige
mockup-pagina. Wel een kleine thumbnail naast elk van de twee velden op het Standaardscherm zelf,
met dezelfde breedte:hoogte-verhouding als Inno Setup's eigen afmetingen (164:314 / 55×55), puur
als directe bevestiging van de gekozen afbeelding — geen nieuwe UI-vraag, alleen hergebruik van
hetzelfde swatch-naast-het-veld-patroon dat de tekstkleurvelden al gebruiken.

**Update: bijgevoegde bugfix, zelfde PR.** Tijdens dit werk viel op dat `ProjectSettingsViewModel.SaveAsync` de
schermspecifieke knopinstellingen (`WelcomeScreenButtons`, `LicenseScreenButtons`,
`SelectDestinationScreenButtons`, `SelectDestinationBrowseButton`, `DefaultScreenButtons`) niet
doorgaf bij het opbouwen van het opgeslagen project — alleen `WizardScreens`/`LicenseFilePath`/
`DefaultDirName`/`AllowUserToChangeDir` (en nu de twee wizardafbeeldingen) hadden een
pass-through-veld. Opslaan vanuit de projectinstellingen ná het aanpassen van knopkleuren/
lettertype in de schermeditor zou die aanpassingen dus stilzwijgend hebben teruggezet naar leeg.
Eerst als apart punt vastgelegd, op Herberts verzoek alsnog in dezelfde PR meegenomen: alle vijf
knopinstellingen-velden hebben nu hetzelfde pass-through-patroon als de andere vier, zodat Opslaan
vanuit de projectinstellingen niets meer stilzwijgend terugzet — alles wat op dit moment in de
schermeditor in te stellen is, blijft nu ook bewaard.

## 17. Twee bugs en zichtbare defaultwaardes in de knopinstellingen (2026-09-28)

Herberts feedback na het testen van PR #15: het lettertype van een knop was direct zichtbaar in
de voorvertoning zodra je het wijzigde, de tekstkleur niet. En belangrijker: de
instellingenvelden per scherm (Terug-/Volgende-/Annuleren-knop) toonden alleen wat dat scherm
zelf had ingevuld, nooit wat er via het Standaardscherm (of Inno Setup's eigen ingebouwde tekst)
daadwerkelijk gold zolang het eigen veld leeg is, terwijl de drielaags-resolutie (SS12.6/SS12.7)
daar juist voor bestaat.

**Bug: tekstkleur niet live.** Oorzaak: een globale `Style TargetType="TextBlock"` in
Styles.xaml geeft elke TextBlock een vaste `Brush.TextPrimary`-kleur. Knoptekst is intern ook
een TextBlock (WPF's eigen string-naar-TextBlock-sjabloon), en een Style-Setter wint het van een
via overerving doorgegeven waarde, dus de globale TextBlock-stijl overschreef altijd de
Foreground die de knop zelf zou moeten hebben. Voor lettertype bestaat geen vergelijkbare
globale regel, dus dat werkte toevallig altijd al goed. Fix: de Button-ControlTemplate in
Styles.xaml krijgt een `ControlTemplate.Resources` met een TextBlock-stijl die Foreground weer
terugbindt naar de knop zelf (`RelativeSource TemplatedParent`), zonder `BasedOn` op de globale
stijl (StaticResource kan niet vooruitwijzen binnen hetzelfde ResourceDictionary, en die globale
stijl zet toch alleen Foreground).

**Defaultwaardes zichtbaar maken.** Nieuw: `InnoSetupStudio.App.Controls.Placeholder`, een
bindbare attached property (`Placeholder.Text`) die een lichtgrijze, niet-interactieve
spooktekst toont zolang het echte veld leeg is, nooit onderdeel van de opgeslagen waarde, puur
ter info, zoals Herbert vroeg. Uitgebreid in de bestaande TextBox- en ComboBox-ControlTemplates
in Styles.xaml (de editable ComboBox had al een los `PART_EditableTextBox` met `Style="{x:Null}"`
voor de lettertypevelden, dus die kreeg een eigen kopie van hetzelfde mechanisme). In
WizardEditorWindow.xaml's ButtonSettingsSectionTemplate (gedeeld door alle vier de schermtypen)
gebruikt elk veld nu `Placeholder.Text`, gebonden aan de bijbehorende `EffectiveXxx`-eigenschap:
Terug-/Volgende-/Annuleren-tekst, tekstkleur (inclusief het kleurvlakje ernaast, dat nu ook de
effectieve kleur toont in plaats van alleen de eigen), lettertype en lettergrootte. Voor de drie
echte schermen bestonden die EffectiveXxx-eigenschappen al (WizardScreenEditorViewModel);
`DefaultScreenEditorViewModel` kreeg ze er nu ook bij: voor Caption twee lagen (eigen tekst,
anders Inno Setup's eigen ingebouwde tekst), voor tekstkleur/lettertype/lettergrootte altijd
leeg/null (er is op het Standaardscherm zelf geen zinvolle terugvalwaarde om te tonen), puur
zodat de gedeelde template overal dezelfde bindingen kan gebruiken zonder WPF-bindingsfouten.
De schermspecifieke Bladerknop (SelectDestinationPageEditorViewModel) heeft bewust geen
Effective*-resolutie (SS12.6) en dus ook geen placeholder, alleen de lettergrootte daar is
verbreed, zie hieronder.

**Lettergrootteveld verbreed.** Herberts feedback: het veld toonde maar een cijfer. Breedte van
alle vier de voorkomens (Terug/Volgende/Annuleren + Bladerknop) van 50 naar 70.

**Niet meegenomen: Properties-knop per knop.** Herbert stelde ook voor om de instellingenvelden
per knop (nu zeven regels: tekst, aan/uit, zichtbaar/verborgen, kleur, lettertype, grootte, vet)
compact te maken tot alleen de knoptekst met een "Properties"-knop ernaast die de rest in een
apart paneel toont. Hij is daar zelf nog naar een geschikt mockup-gereedschap aan het zoeken,
dus bewust niet in deze PR meegenomen, om dat werk niet dubbel te doen. Volgt als apart
backlogpunt zodra hij een ontwerp heeft.


**Update: twee vervolgfixes, zelfde PR.** CodeRabbit's review op PR #16 en Herbert's eigen test
brachten nog twee gaten aan het licht in de hierboven beschreven fix.

Ten eerste: de knoppen in de voorvertoning (Annuleren/Terug/Volgende, links in de schermeditor)
gebruikten nog een kale string als Content. WPF's automatische omzetting van zo'n string naar een
interne TextBlock erft niet gegarandeerd de Foreground-binding van de knop zelf, dus de
tekstkleur kwam daar alsnog niet altijd door, ook al werkte de ControlTemplate.Resources-fix
elders wel. Elke voorvertoningsknop heeft nu een expliciete TextBlock als content, met Foreground
rechtstreeks gebonden aan de eigen Button (RelativeSource AncestorType=Button) - dat sluit de
twijfel over WPF's interne string-omzetting helemaal uit.

Ten tweede: de spooktekst-trigger vergeleek Text met een exacte lege string (""), terwijl
ResolveCaption een waarde van uitsluitend witruimte al als "niet ingevuld" behandelde. Bij zo'n
veld bleef de spooktekst dus verborgen, of - erger - verscheen hij wel (na de eerste fix hieronder)
maar bleven de echte spaties in de TextBox staan als klikbare tekst, wat een verwarrende
invoegcursor middenin de spooktekst gaf (Herberts screenshot: "Vo|lgende >" i.p.v. aan het begin).
Twee nieuwe converters (BlankStringToVisibilityConverter voor de gewone TextBox,
EditableBlankToVisibilityConverter voor de editable ComboBox) vervangen de exacte
Text=""-triggers door een IsNullOrWhiteSpace-check. En een nieuwe NormalizeWhitespaceOnly-helper
in WizardScreenEditorViewModel (Caption/TextColor/FontFamily) en DefaultScreenEditorViewModel
(Caption) zet een alleen-witruimte-waarde meteen terug naar leeg in de OnXxxChanged-hook zelf, zodat
er geen "onzichtbare" spaties meer achterblijven om de cursor te verwarren.


**Update: derde vervolgfix, zelfde PR.** Herberts vorige screenshot bleek geen witruimte-
probleem: hij bevestigde expliciet dat het veld leeg was. Een nieuw vergelijkingsscreenshot
(eigen tekst in het ene veld naast de spooktekst in een leeg veld ernaast, met een verticale
referentielijn) liet zien dat de spooktekst een paar tekenposities te ver naar links stond t.o.v.
waar de echte tekst/cursor landt.

Root cause: PART_ContentHost (de ScrollViewer die de echte tekst toont) kreeg in onze
ControlTemplate een expliciete `Margin="{TemplateBinding Padding}"`. Maar WPF past de
Padding-eigenschap van een TextBox altijd al automatisch toe op de echte tekst/cursor daarbinnen,
los van wat de ControlTemplate daar zelf op zet - geverifieerd tegen het officiele WPF-
broncjabloon (PresentationFramework's eigen TextBox.xaml zet nergens een Margin op
PART_ContentHost, terwijl Padding daar wel degelijk werkt). Onze eigen Margin kwam er dus gewoon
bovenop: de echte tekst kreeg de Padding dubbel toegepast, de spooktekst (een gewone TextBlock,
geen TextBoxBase, dus zonder dat automatisme) maar enkel - vandaar het verschil.

Fix: Margin weg bij PART_ContentHost. PlaceholderText houdt zijn eigen `Margin="{TemplateBinding
Padding}"` als enige inspringing, en beide elementen zijn verplaatst naar dezelfde Grid ín de
Border (waren eerst op twee verschillende boomniveaus genest, zie de vorige update) zodat ze
gegarandeerd exact dezelfde oorsprong delen.


## 18. Properties-knop per knop voor het Schermeditor-paneel (backlogitem 3, sectie 17) (2026-09-28)

Uitgevoerd: het "Niet meegenomen" punt uit sectie 17 hierboven. Herbert leverde een mockup
(properties.pdf) aan: elk van de drie tekstvelden (Terug/Volgende/Annuleren) wordt teruggebracht
tot alleen de knoptekst zelf - bewerkbaar, getoond in de knop zijn eigen (of overgeërfde, als
grijze hint) tekstkleur - met een klein eigenschappenknopje ernaast. Dat knopje opent een "Knop
eigenschappen"-dialoogvenster met de overige velden: tekstkleur (met kleurvlakje en
kleurenkiezer-knop), lettertype (dezelfde doorzoekbare ComboBox als voorheen), lettergrootte,
vet, een nieuw Tooltip-veld, Ingeschakeld/Zichtbaar-checkboxes en een live voorvertoning
onderaan. Dezelfde dialoog wordt hergebruikt op alle plekken waar een knop bewerkt wordt: de drie
echte schermen (Welkom/Licentie/Bestemming, drielaagse Effective*-resolutie via SS12.6/SS12.7),
het Standaardscherm zelf (eigen, tweelaagse Terug/Volgende/Annuleren-waarden) en de Bladerknop op
het Bestemmingsscherm (geen cascade, geen Caption).

Herberts scope-beslissingen (bevestigd 2026-09-28): de knopachtergrondkleur en afgeronde hoeken
uit een eerdere mockup-versie zijn geschrapt (TNewButton is en blijft een door Windows getekende
standaardknop, zie sectie 12.6/WizardScreenButtonSettings), de Ingeschakeld/Zichtbaar-checkboxes
horen er wél bij, en de overgeërfde/effectieve waarde blijft als grijze hint zichtbaar zodra een
veld leeg is - hetzelfde patroon als PR #16 al voor het oude paneel gebruikte.

**ButtonPropertiesViewModel: één dialoog, acht contexten.** In plaats van deze ene dialoog aan
een specifiek schermtype te binden (die drie types - WizardScreenEditorViewModel,
DefaultScreenEditorViewModel, SelectDestinationPageEditorViewModel - hebben bewust geen gedeelde
basisklasse, zie sectie 11.8), werkt de nieuwe `ButtonPropertiesViewModel` als een pure adapter:
elk veld krijgt een get/set-delegatenpaar mee, opgebouwd in `WizardEditorWindow.xaml.cs`
(`BuildForScreenButton`/`BuildForDefaultScreenButton`/`BuildForBrowseButton`) aan de hand van de
Tag ("Back"/"Next"/"Cancel") op het aangeklikte eigenschappenknopje en het DataContext-type
erachter. De dialoog werkt met een momentopname: de get-delegates vullen de velden eenmalig bij
het openen, de set-delegates schrijven pas terug bij een geslaagde Opslaan - dezelfde aanpak als
`WizardEditorViewModel.ApplyTo`, zodat Annuleren/Sluiten de lokale wijzigingen simpelweg
weggooit zonder aparte revert-logica per veld (zie `DirtyTrackingViewModel`).

**Nieuwe iconen.** Herberts handgetekende `properties.svg`/`selectcolor.svg` (Inkscape, met
geneste groep-transforms en Bezier-curves) zijn omgezet naar de WPF Geometry-mini-taal die
`Icons.xaml` al gebruikt, via een Python-script (`svgelements` voor transform-resolutie en
curve-afvlakking naar dichte polylijnen, `cairosvg` voor visuele verificatie vooraf) - dezelfde
"F1"-voorvoegsel-conventie (NonZero fill-rule) als de bestaande geïmporteerde iconen
(BuildInstaller/Renew/Search).

**Bouw- en testresultaat.** `dotnet build` slaagt zonder waarschuwingen of fouten, alle 14
bestaande tests slagen. De gebouwde `InnoSetupStudio.exe` start zonder crash (geen nieuwe regel
in `crash-log.txt`); een volledige interactieve doorloop van de nieuwe dialoogvensters zelf kon
in deze sessie niet automatisch worden getest (geen UI-automatiseringstool voor dit
bureaubladvenster beschikbaar) - Herbert wordt gevraagd dit handmatig te controleren voordat de
PR wordt samengevoegd.

## 19. Meertaligheid: talenselectie per project (backlogitem 4, sectie 14) (2026-09-29)

Eerste stap van backlogitem 4 uit sectie 14: "welke talen ondersteunt dit project" — bewust niet
de volledige vertaal-UX (hóe teksten per taal worden ingevoerd), dat blijft voor later. Doel van
deze stap was uitsluitend de volgorde-vraag uit sectie 14 beantwoorden: een toekomstige
tekst-per-taal-uitbreiding op de knop-Captions (sectie 17/18) hoeft nu niet twee keer gebouwd te
worden.

**Model.** Eén nieuw veld op `InstallerProject`: `SupportedLanguageIds` (`List<string>`),
standaard `["english"]`. Bewust géén apart `IsMultilingual`-vlaggetje: dat zou een tegenstrijdige
status met de lijst kunnen opleveren (vlag aan, lijst leeg — of andersom). Meer dan één taal in de
lijst betekent gewoon dat de installer meertalig is.

**Talencatalogus (`InnoLanguageCatalog`, `InnoSetupStudio.Core`).** Een statische lijst van 33
talen (Engels plus de 32 .isl-bestanden uit Herberts kopie van de Languages-map), in plaats van die
map zelf op schijf in te lezen. Twee redenen: (1) de app weet nog niet waar Inno Setup
geïnstalleerd staat — dat is pas vanaf de build-integratie (fase 7) relevant — en aannames over een
vast pad (`C:\Program Files\Inno Setup 7\Languages\`) zouden op een andere machine (of een andere
Inno Setup-versie) kunnen breken; (2) de niet-Latijnse .isl-bestanden (Arabisch, Chinees, Thai,
Hebreeuws, enzovoort) hebben elk hun eigen `LanguageCodePage`, en die inlezen zonder mojibake is
extra werk voor iets dat de statische lijst net zo goed oplevert. De leesbare naam per taal is de
Engelse naam (bijvoorbeeld "Brazilian Portuguese"), niet de vertaling in de taal zelf
(`LanguageName=` in het .isl-bestand): dat blijft leesbaar ongeacht welke UI-taal (nl/en/de)
actief is, zonder 32 extra vertaalregels per resx-bestand voor iets dat een eigennaam is. Elke
taal-id is de exacte kleine-letters spelling die Inno Setup zelf in het `Name:`-veld van
`[Languages]` verwacht (bijvoorbeeld `german`, `brazilianportuguese`) — dezelfde spelling als de
bestandsnaam van het .isl-bestand, zodat een latere generator (fase 5/6) deze id's zonder mapping
kan hergebruiken.

**Engels: ingebouwd en altijd aan.** Inno Setup toont Engels ook zonder eigen
`[Languages]`-sectie (via `compiler:Default.isl`, geen los bestand nodig), dus
`SupportedLanguageIds` bevat na elke wijziging altijd minstens "english" — in de UI als
aangevinkt-en-uitgeschakeld vinkje, en in `JsonInstallerProjectService.LoadAsync` als
normalisatie (zelfde soort null-vangnet als `WizardScreens`/de knopinstellingen: een expliciete
JSON-null of een handmatig bewerkte lijst zonder "english" wordt hersteld in plaats van een
NullReferenceException te geven of stilzwijgend een installer zonder Inno Setup's eigen
standaardtaal op te leveren).

**UI.** Een nieuw venster `LanguagesWindow`/`LanguagesViewModel`, één-op-één gebouwd naar het
patroon van `WizardScreensWindow`/`WizardScreensViewModel` (losse rij-objecten met een vinkje,
geabonneerd op `PropertyChanged` naar `MarkDirty`, `Opslaan`/`Annuleren` via `RequestClose`) — maar
zonder icoon per rij (talen hebben geen herkenningspictogram zoals wizardschermen dat wel hebben)
en met het Engels-rijtje vast aangevinkt via een `DataTrigger` op `IsLocked`. Geopend via een
nieuwe knop "Talen" op het hoofdscherm, naast Wizardschermen/Schermen bewerken — bewust een eigen
venster op hoofdschermniveau in plaats van een sectie in het Projectinstellingen-scherm (dat zou
dat scherm met 33 aan/uit-vinkjes flink laten groeien) of in het Standaardscherm (de andere optie
die sectie 14 noemde; het Standaardscherm gaat over knopinstellingen, een talenlijst hoort daar
inhoudelijk niet bij). Hergebruikt het bestaande "Document"-icoon voor de knop: geen eigen
"talen"-icoon getekend voor deze stap, en dat icoon staat elders al voor meerdere verschillende
schermtypes, dus hergebruik hiervoor past bij hoe de rest van de iconenset al wordt ingezet.

**Nog niet gebouwd, bewust uitgesteld.** Hóe teksten per taal worden ingevoerd (Caption-per-taal
op de knoppen, en breder), en het schrijven van de `[Languages]`-sectie zelf in de generator
(fase 5/6) — dat laatste kan de taal-id's uit `SupportedLanguageIds` rechtstreeks hergebruiken
als `Name:`-waarden, met `compiler:Default.isl` voor Engels en `compiler:Languages\<Bestand>.isl`
voor de rest.

**Bouw- en testresultaat.** `dotnet build` slaagt zonder waarschuwingen of fouten. Alle 21 tests
slagen (17 bestaande plus 4 nieuwe: standaardwaarde bij een nieuw project, round-trip van
`SupportedLanguageIds`, en de twee null-/ontbrekend-Engels-normalisaties in
`JsonInstallerProjectService`, plus 4 losse tests voor `InnoLanguageCatalog` zelf: Engels eerst en
ingebouwd, 33 entries, unieke kleine-letters id's, alleen Engels `IsBuiltIn`). De gebouwde
`InnoSetupStudio.exe` start zonder crash. Een volledige interactieve doorloop van het nieuwe
Talen-venster zelf kon in deze sessie niet automatisch worden getest (geen
UI-automatiseringstool voor dit bureaubladvenster beschikbaar) — Herbert wordt gevraagd dit
handmatig te controleren (Nieuw project → Talen: Engels vast aangevinkt, een paar talen aan/uit
zetten, Opslaan, project opnieuw openen en controleren dat de keuze bewaard is gebleven) voordat
de PR wordt samengevoegd.

## 20. Meertalige knopteksten (Caption/Tooltip) per taal — backlogitem, nog niet ontworpen (2026-09-29)

**Aanleiding.** Herbert testte de talenselectie uit sectie 19 en bevestigde dat deze correct
opslaat. Daarbij het eerste vervolgpunt dat sectie 19 zelf al noemde als bewust uitgesteld: de
knoppen (`WizardScreenButtonSettings.BackButtonCaption`/`NextButtonCaption`/`CancelButtonCaption`
en de bijbehorende Tooltip-velden) ondersteunen nu eigen tekst, maar die tekst is nog altijd één
vaste string — niet per taal.

**Antwoord op Herberts vraag: ondersteunt Inno Setup dit?** Ja, via `[CustomMessages]`, niet via
`[Messages]`. `[Messages]` is voor Inno Setup's eigen ingebouwde teksten (de standaard Terug/
Volgende/Annuleren-knoppen, foutmeldingen) en wordt automatisch gevuld vanuit de `.isl`-bestanden
van de gekozen taal; daar heeft een projectmaker geen invloed op. `[CustomMessages]` is bedoeld
voor eigen teksten en ondersteunt een `.taalid`-suffix per regel:

```
[CustomMessages]
MyCancelCaption.english=Cancel
MyCancelCaption.dutch=Annuleren
MyCancelCaption.german=Abbrechen
```

In Pascal Script haal je de juiste waarde op met `CustomMessage('MyCancelCaption')`; Inno Setup
kiest automatisch de regel die hoort bij de taal die de eindgebruiker bij het starten van de
installer koos. Ontbreekt een taal-suffix, dan valt Inno Setup terug op de eerst genoemde taal in
`[Languages]` — bij ons dus Engels, wat aansluit bij hoe Engels al vast eerste/verplichte taal is
in `SupportedLanguageIds` (sectie 19).

**Scope, zoals Herbert vandaag heeft aangegeven.** Alleen relevant wanneer een knop al eigen tekst
heeft (niet de Inno Setup-standaardtekst) én het project meertalig is (meer dan alleen Engels in
`SupportedLanguageIds`). In dat geval een scherm waarin per taal de tekst van die knop kan worden
ingevoerd. Twee niveaus, zoals de bestaande drielaags-resolutie dat al kent:

- **Voor alle schermen** — de Caption/Tooltip op het Standaardscherm, per taal.
- **Voor één individueel scherm** — een eigen Caption/Tooltip die alleen op dat scherm geldt, per
  taal, met voorrang op de Standaardscherm-waarde (zelfde principe als de bestaande drielaags-
  resolutie: eigen waarde → Standaardscherm → Inno Setup's ingebouwde tekst — straks dus per taal
  in plaats van één string per laag).

**Wat dit raakt aan het datamodel (nog niet ontworpen, alleen de impact benoemd).** Caption/
Tooltip op `WizardScreenButtonSettings` zijn nu `string?`. Zodra een project meertalig is, wordt
dat in principe een waarde per taal-id (vergelijkbaar met hoe `[CustomMessages]` zelf werkt) in
plaats van één vaste string. Vragen die dat oproept en die een eigen ontwerp vragen voordat dit
gebouwd wordt:

- Wat gebeurt er met een al ingevulde, eentalige Caption zodra een project van eentalig naar
  meertalig gaat — blijft die de Engelse waarde, of moet Herbert die bevestigen/overzetten?
- Hoe blijft het invoerscherm overzichtelijk bij veel geselecteerde talen (tot 33 mogelijk) —
  waarschijnlijk een tabel/lijst per taal in plaats van 33 losse tekstvelden naast elkaar, maar de
  precieze vorm is nog niet uitgewerkt.
- Generatorwerk (fase 5/6): naast het al genoemde `[Languages]`-sectiewerk uit sectie 19 moet de
  generator nu ook `[CustomMessages]`-regels schrijven én de Pascal Script-aanroepen voor de
  drie/vier knoppen omzetten van een vaste string naar `CustomMessage('...')`.

**Status.** Backlogitem, vastgelegd op Herberts expliciete verzoek. Nog niet gepland, nog niet
ontworpen — net als sectie 19 zelf al aangaf: dit vraagt om een eigen ontwerp voordat het gebouwd
wordt, geen aanpassing om terloops mee te nemen bij een andere feature.

## 21. IDE-schil herontwerp: van losse vensters naar één werkgebied — voorstel, nog niet besloten (2026-09-29)

**Aanleiding.** Herbert, direct na het testen van de talenselectie: de huidige opzet van het
hoofdscherm voelt niet meer ideaal aan, en wordt rommeliger zodra het scherm voor meertalige
knopteksten (sectie 20) erbij komt. Concreet genoemd: de taal- en thema-keuze van de IDE zelf staan
nu bovenaan naast de projectacties, terwijl dat eigenlijk instellingen zijn; en de knoppenrij in
het midden van `MainWindow` groeit met elke feature (nu zes knoppen: Nieuw project, Project
openen, Wizardschermen, Schermeditor, Talen, Installer bouwen — de laatste nog niet eens
aangesloten).

**Huidige structuur (ter referentie, voor dit voorstel).** Drie vensters diep: `MainWindow` opent
`WizardScreensWindow` (welke schermen doen mee) en `WizardEditorWindow` (schermlijst 180px |
voorvertoning ~497px | eigenschappenpaneel `*`, min 260px) als losse dialogen; binnen
`WizardEditorWindow` opent een knopje per knop weer een derde venster, `ButtonPropertiesWindow`
(kleur, lettertype, tooltip, enabled/zichtbaar). Nog een vierde laag (talen per knoptekst,
sectie 20) bovenop dat derde venster zou dat nesten verder verdiepen.

**Voorstel.** Eén blijvend zichtbaar werkgebied in `MainWindow` in plaats van losse dialogen:

- **Bovenbalk (vast, over de volledige breedte).** Projecttitel/icoon links. Daarna de
  projectacties die er nu ook al zijn: Nieuw project, Project openen, Installer bouwen. Nieuw
  daarbij: een knop Projectinstellingen die alleen actief is bij een geopend project — dat bestaat
  vandaag niet als losse actie, Projectinstellingen opent nu alleen automatisch direct na Nieuw/
  Openen, er is geen weg terug erin zonder het project opnieuw te openen. Helemaal rechts een
  tandwiel-knop voor Instellingen: taal en thema van de IDE zelf, weg van de projectacties.
- **Linkerkolom (vast, zoals de huidige 180px-schermlijst).** De schermselectie: dezelfde lijst als
  nu in `WizardEditorWindow` (Standaardscherm vast bovenaan, daaronder de echte installerschermen
  in Inno Setup's volgorde), maar nu met een aan/uit-vinkje per rij — dat vervangt de aparte
  `WizardScreensWindow`. Eronder een compacte voorvertoning (de "scherminhoud") van het
  geselecteerde scherm, kleiner dan de huidige 497px-voorvertoning in `WizardEditorWindow`.
- **Middendeel (de rest van de breedte).** Het eigenschappenpaneel dat nu rechts in
  `WizardEditorWindow` staat (Caption/kleur/lettertype per knop, de twee wizardafbeeldingen bij het
  Standaardscherm) verhuist hierheen en krijgt daarmee de meeste ruimte in het venster — precies de
  ruimte die het scherm voor meertalige knopteksten (sectie 20) nodig heeft, zonder dat daar een
  eigen venster of een zevende hoofdscherm-knop voor nodig is.

**Wat dit oplost.** Geen twee aparte dialoogvensters meer (`WizardScreensWindow`,
`WizardEditorWindow`) voor iets dat inhoudelijk bij elkaar hoort. Taal/thema van de IDE apart van
de projectacties. Ruimte voor sectie 20 ontstaat vanzelf in het middenpaneel in plaats van er nog
een laag bovenop te stapelen.

**Nog open, Herbert beslist.**

- **Talen (sectie 19).** Projecteigenschap, geen IDE-instelling, hoort dus niet achter het
  tandwiel. Twee opties: een tweede tabblad boven de schermselectie-lijst links ("Schermen" /
  "Talen"), of terug een sectie in Projectinstellingen.
- **`ButtonPropertiesWindow`.** Blijft dat een eigen (derde) venster zoals nu, of gaan die velden
  ook rechtstreeks in het middenpaneel nu daar toch meer ruimte is? Dat raakt sectie 20 direct: een
  taal-tabblad zou dan in dit venster komen in plaats van in het middenpaneel.
- **Installer bouwen.** In de bovenbalk zoals hierboven voorgesteld, of ergens anders?
- **Projectinstellingen.** Blijft dat een eigen venster (zoals nu), of ook inline in het
  hoofdscherm?

**Volgorde.** Eerst deze schil bouwen, dan pas sectie 20 (meertalige knopteksten) — anders wordt
dat scherm eerst in het oude patroon gebouwd en kort daarna overgedaan in het nieuwe. Nog geen
besluit genomen om te starten; dit is vastlegging plus voorstel, net als sectie 14 destijds.

**Beslissingen van Herbert (2026-09-29), na het voorstel hierboven.**

- **Installer bouwen** — bevestigd in de bovenbalk, zoals voorgesteld.
- **Talen (sectie 19) én Wizardschermen (welke schermen meedoen)** — beide naar
  Projectinstellingen, niet als aparte laag in de linkerkolom. Herberts eigen woorden: "Dit zie ik
  allemaal als projectinstellingen." Dat wijzigt de linkerkolom uit het voorstel hierboven: die
  wordt puur een navigatielijst van de al ingeschakelde schermen (Standaardscherm vast bovenaan),
  zonder aan/uit-vinkjes — welk scherm meedoet, wordt voortaan in Projectinstellingen bepaald,
  samen met de talenselectie. De compacte voorvertoning ("scherminhoud") onder die lijst blijft
  zoals voorgesteld. Projectinstellingen wordt zo de plek voor alles wat maar zelden verandert
  (algemene projectgegevens, welke schermen, welke talen); de linkerkolom en het middenpaneel in
  het hoofdscherm blijven voor het daadwerkelijke, veelvuldige bewerken van een scherm dat al aan
  staat.

**Nog open.** `ButtonPropertiesWindow` (kleur/lettertype/tooltip/enabled/zichtbaar per knop) —
blijft dat een eigen venster zoals nu, of gaan die velden rechtstreeks in het middenpaneel nu daar
meer ruimte is? Dit bepaalt waar sectie 20 (talen per knoptekst) landt: in dat venster, of in het
middenpaneel zelf.

**Laatste beslissing (2026-09-29): optie A.** `ButtonPropertiesWindow` blijft een eigen venster.
De talen-tabbladen voor sectie 20 komen daar terecht, niet in het middenpaneel — dat blijft zo
rustig als het nu is (één tekstregel per knop met een eigenschappenknopje ernaast). Reden: de
knoppenrij in het hoofdscherm is net opgeruimd door Talen en Wizardschermen naar
Projectinstellingen te verplaatsen; alle knopdetails in het middenpaneel zetten zou diezelfde
rommeligheid één laag dieper terugbrengen.

**Ontwerp compleet, nog niet gebouwd.** Alle openstaande vragen uit dit voorstel zijn nu
beantwoord:

1. Bovenbalk: Nieuw project, Project openen, Projectinstellingen (alleen actief bij open project),
   Installer bouwen, en rechts een tandwiel voor Instellingen (taal/thema van de IDE).
2. Projectinstellingen: bestaande projectgegevens, plus Talen (sectie 19) en Wizardschermen (welke
   schermen meedoen) — beide overgeheveld vanuit hun huidige losse knop/venster.
3. Linkerkolom hoofdscherm: navigatielijst van de ingeschakelde schermen (Standaardscherm vast
   bovenaan), zonder aan/uit-vinkjes, met een compacte voorvertoning eronder.
4. Middenpaneel: het huidige eigenschappenpaneel uit `WizardEditorWindow` (Caption per knop,
   wizardafbeeldingen bij het Standaardscherm), ongewijzigd qua inhoud.
5. `ButtonPropertiesWindow` blijft bestaan; wordt de plek voor sectie 20 (talen per knoptekst).

Nog geen besluit genomen om hiermee te starten — Herbert bepaalt wanneer.

**Gebouwd (2026-09-29), op `feature/ide-shell-redesign`.** Het ontwerp hierboven is één-op-één
geïmplementeerd:

- `SettingsWindow` (nieuw): taal/thema van de IDE, letterlijk verhuisd uit MainWindow.xaml.cs. Er
  hoeft bij het openen niets opnieuw toegepast te worden — App.xaml.cs past de opgeslagen taal/
  thema al toe vóórdat MainWindow ooit verschijnt, dit venster toont alleen de huidige keuze.
- `ScreenEditorControl` (nieuw, UserControl): de volledige inhoud van het voormalige
  `WizardEditorWindow` (schermlijst, voorvertoning, instellingenpaneel, alle resources/templates),
  nu permanent zichtbaar in MainWindow in plaats van een dialoogvenster. De Opslaan/Annuleren-
  dialoogbalk is vervangen door een inline Opslaan-knop (actief zolang `IsDirty`) met een "niet-
  opgeslagen wijzigingen"-label ernaast — bewust géén automatisch opslaan bij elke toetsaanslag,
  zie de toelichting in ScreenEditorControl.xaml. `ButtonPropertiesWindow` blijft ongewijzigd een
  eigen venster (optie A, Herberts beslissing hierboven).
- `ProjectSettingsWindow`: drie tabbladen (Algemeen/Schermen/Talen) in plaats van één lange
  ScrollViewer. Schermen en Talen hergebruiken `WizardScreensViewModel`/`LanguagesViewModel` als
  sub-viewmodel in `ProjectSettingsViewModel` — hun eigen Save/Cancel/RequestClose blijven
  ongebruikt, dit venster stuurt zijn eigen Opslaan/Annuleren aan.
- **Bugfix, gevonden tijdens het bouwen:** `ProjectSettingsViewModel.SaveAsync` bouwde altijd een
  volledig nieuw `InstallerProject`-object en gaf `SupportedLanguageIds` daarbij nooit door. Elke
  keer dat iemand Projectinstellingen opsloeg, viel de talenselectie stilzwijgend terug op alleen
  Engels (de eigen standaardwaarde van dat veld) — dezelfde soort bug als de knopinstellingen-bug
  uit sectie 16, nu voor Talen. Opgelost als onderdeel van dezelfde wijziging die Talen sowieso al
  bewerkbaar moest maken in dit scherm.
- `MainWindow`: bovenbalk (Nieuw project, Project openen, Projectinstellingen — nieuw: nu ook
  bruikbaar bij een al actief project, niet alleen automatisch na Nieuw/Openen —, Installer
  bouwen, en het instellingen-tandwiel rechts) plus `ScreenEditorControl` als hoofdinhoud zodra er
  een actief project is, anders de welkomsttekst. `WizardScreensWindow`/`LanguagesWindow`/
  `WizardEditorWindow` zijn verwijderd.
- `WizardScreensViewModel`/`LanguagesViewModel`/`WizardEditorViewModel` zelf zijn ongewijzigd
  gebleven (alleen hun vensters zijn vervangen); geen van de bestaande 21 tests raakte hierdoor.

**Build- en testresultaat.** `dotnet build`: 0 waarschuwingen, 0 fouten. `dotnet test`: 21/21
geslaagd (ongewijzigd — deze wijziging raakt alleen WPF-vensters/viewmodel-bekabeling in
`InnoSetupStudio.App`, niet de geteste logica in `InnoSetupStudio.Core`). De gebouwde
`InnoSetupStudio.exe` start zonder crash. Een volledige interactieve doorloop van de nieuwe
indeling (bovenbalk, schermeditor inline, Projectinstellingen-tabbladen) kon in deze sessie niet
automatisch getest worden (geen UI-automatiseringstool voor dit bureaubladvenster beschikbaar) —
Herbert wordt gevraagd dit handmatig te controleren voordat de PR wordt samengevoegd.


**Polish na Herberts eerste doorloop (2026-09-29), zelfde branch `feature/ide-shell-redesign`.**
Herbert heeft de gebouwde schil in fullscreen bekeken en vier concrete verbeterpunten gegeven, in
`ScreenEditorControl.xaml`:

1. *Voorvertoning naar boven.* De preview-`Border` (Grid.Column="2", zowel de variant voor een
   echt scherm als de Standaardscherm-infovariant) had geen expliciete `VerticalAlignment`, dus
   centreerde WPF hem verticaal in zijn kolom zodra het venster hoger was dan de vaste
   preview-hoogte (400px) — dezelfde WPF-regel als bij de knoppenbalk uit sectie 16: een element
   met een expliciete `Height` en de standaard `VerticalAlignment="Stretch"` wordt gecentreerd
   binnen de beschikbare ruimte in plaats van bovenaan te blijven. Fix: `VerticalAlignment="Top"`
   toegevoegd aan beide `Border`-instanties, zodat de preview altijd bovenaan naast het
   eigenschappenpaneel staat.
2. *Lengtelimiet knopomschrijvingen.* De drie tekstvelden in `ButtonSettingsSectionTemplate`
   (Terug/Volgende/Annuleren) hadden geen `MaxLength`. Herbert: "de gebruiker mag toch geen
   onbeperkte tekst invullen als buttontekst" — circa 30 tekens. `MaxLength="30"` toegevoegd aan
   alle drie.
3. *Standaard installatiemap ongewijzigd.* Op Herberts expliciete verzoek is het `DefaultDirName`-
   veld niet aangepast: geen lengtelimiet, en een breder veld dan de knopvelden is daar niet
   hinderlijk.
4. *Eigenschappenknopjes dichter bij de velden.* Alle vier de knoprijen (Terug/Volgende/Annuleren
   + de losse Bladeren-knoprij) gebruikten een Grid met `ColumnDefinition Width="*"` gevolgd door
   `Width="Auto"` voor het eigenschappenknopje — de `*`-kolom vult altijd de volledige resterende
   breedte van het middendeel, dus het knopje stond bij een breed venster steeds helemaal rechts,
   los van hoe lang de tekst in het veld was. Om dat knopje daadwerkelijk mee naar links te laten
   komen, moest de kolombreedte zelf vast worden gemaakt, niet alleen het tekstveld: de eerste
   kolom van alle vier de rijen is nu `Width="224"` (tekstveld 220px + 4px marge) in plaats van
   `Width="*"`, en de drie tekstvelden hebben zelf ook `Width="220"` + `HorizontalAlignment="Left"`
   gekregen. Zo staan alle vier eigenschappenknopjes nu consequent op dezelfde, vaste positie
   direct naast hun veld, ook op een breed scherm — precies wat Herbert bedoelde met "dat ziet er
   wel strak uit".

**Feitencheck: Bladeren-knop tekst wél aanpasbaar via Pascal Script.** Herbert vroeg of de tekst
op de Bladeren-knop (Select Destination-pagina) net als Terug/Volgende/Annuleren met Pascal Script
kan worden aangepast, of dat zijn vermoeden klopte dat dit niet zomaar kan. Geverifieerd via
webzoekopdracht (niet uit geheugen beantwoord): in Inno Setup's `TWizardForm`-objectmodel is
`DirBrowseButton` net als `BackButton`/`NextButton`/`CancelButton` gedeclareerd als `TNewButton`,
en `TNewButton` heeft een `Caption`-property. De tekst is dus wél instelbaar, bijvoorbeeld met
`WizardForm.DirBrowseButton.Caption := '...';` in een `CurPageChanged`-event. Herberts vermoeden
klopte dus niet.

Dit is een bewuste afwijking tussen wat Inno Setup toestaat en wat de app op dit moment
modelleert: `BrowseButtonSettings` heeft opzettelijk geen `Caption`-veld (zie het codecommentaar
bij de Bladeren-knoprij in `ScreenEditorControl.xaml`, dat er nu ten onrechte van uitgaat dat deze
knop geen Caption heeft). Dit wordt hier vastgelegd als nieuw, nog niet ontworpen backlogitem —
een `Caption`/placeholder-veld toevoegen aan `BrowseButtonSettings` net als bij de andere drie
knoppen — en pas opgepakt als Herbert daarvoor kiest, conform het "eerst ontwerpen, dan bouwen"-
principe.

**Build- en testresultaat (polish).** `dotnet build`: 0 waarschuwingen, 0 fouten. `dotnet test`:
21/21 geslaagd (ongewijzigd). Smoke-test: `InnoSetupStudio.exe` gestart en reageerde (`Responding:
True`), daarna weer afgesloten. Interactieve controle van de vier verbeterpunten blijft aan
Herbert.


**CodeRabbit-bevindingen PR #19, geverifieerd en verwerkt (2026-09-29).** Vier "actionable
comments" op commit `3d48a13` (de eerste implementatiecommit van sectie 21), elk tegen de code
zelf gecontroleerd vóór toepassing:

1. *Genuine, opgelost.* `ScreenEditor_SaveClicked` zette `viewModel.IsDirty = false` pas ná de
   `await SaveActiveProjectAsync()`, onvoorwaardelijk. Getypte wijzigingen die tijdens die lopende
   opslag binnenkwamen, werden zo als "opgeslagen" getoond terwijl ze niet in de zojuist gestarte
   `ApplyTo`-snapshot zaten. Fix: `IsDirty = false` verplaatst naar vóór de `await`, direct na
   `ApplyTo` — een latere wijziging zet via de normale `MarkDirty`-route zelf `IsDirty` weer op
   `true`.
2. *Genuine, opgelost.* Diezelfde regel zette `IsDirty` ook op `false` als het opslaan zelf
   mislukte (bijvoorbeeld bestand in gebruik, schijf vol): de foutmelding verscheen wel, maar het
   scherm oogde daarna toch als "opgeslagen". Fix: `SaveActiveProjectAsync` geeft nu een `bool`
   terug (`true` bij succes of niets-te-doen, `false` bij een fout); bij `false` zet
   `ScreenEditor_SaveClicked` `IsDirty` expliciet weer op `true`.
3. *Genuine, maar bewust NIET automatisch opgelost.* `SetActiveProject` bouwt bij elke aanroep
   (ook bij het heropenen van Projectinstellingen voor hetzelfde, al actieve project) een
   compleet nieuwe `WizardEditorViewModel`, zonder te controleren of de vorige nog
   niet-opgeslagen wijzigingen had (`IsDirty == true`). Voorbeeld: een knopomschrijving typen in
   de schermeditor zonder op Opslaan te klikken, dan via de bovenbalk Projectinstellingen openen
   en daar opslaan — de getypte knopomschrijving verdwijnt dan stilletjes. Dit is een echt,
   bevestigd dataverlies-risico, maar de juiste oplossing (negeren, vragen om op te slaan/te
   verwerpen, of automatisch samenvoegen) is een ontwerpkeuze die bij Herbert hoort te liggen —
   dezelfde afweging als steeds bij dit project. Vastgelegd als nieuw, nog niet ontworpen
   backlogitem; niet aangepast in deze sessie.
4. *Genuine, opgelost.* `BuildInstallerButton` werd via `SetProjectActionButtonsEnabled`
   ingeschakeld zodra er een actief project was, maar heeft geen `Click`-handler — "Installer
   bouwen" bestaat nog niet (bewust buiten scope van sectie 21). Een schijnbaar werkende knop die
   niets deed. Fix: `SetProjectActionButtonsEnabled` schakelt deze knop niet meer in; blijft
   `IsEnabled="False"` totdat de bouwfunctionaliteit er daadwerkelijk is.

Niet overgenomen: de "Docstring Coverage"-check (30% vs. vereiste 80%) — deze repo documenteert
bewust in doorlopende Nederlandse commentaarblokken in plaats van XML-`///`-docstrings per functie
(zie de rest van dit document en alle voorgaande secties); dat consequent omzetten naar
XML-docstrings zou een stijlwijziging zijn, geen bugfix, en is niet opgepakt.

**Build- en testresultaat (CodeRabbit-fixes).** `dotnet build`: 0 waarschuwingen, 0 fouten.
`dotnet test`: 21/21 geslaagd (ongewijzigd). Smoke-test: `InnoSetupStudio.exe` gestart en
reageerde (`Responding: True`), daarna afgesloten. PR #19 blijft open in afwachting van Herberts
handmatige doorloop; niet gemerged.


**Nog twee velden verbreed, en Bladeren-knop krijgt een Caption (2026-09-29), zelfde branch.**
Herbert ging akkoord met de schermopbouw, met twee aanvullende punten:

1. *Wizardafbeelding (groot)/(klein) niet inkorten.* Dezelfde regel als bij "Standaard
   installatiemap" (§21-polish hierboven): deze twee padvelden onder het Standaardscherm mogen
   ook niet ingekort worden. Ze hadden een vaste `Width="180"` staan (ouder dan sectie 21, uit
   backlogitem 1/sectie 14) — omgebouwd van een `StackPanel` naar een `Grid` met een sterretjes-
   kolom voor het tekstveld, zelfde patroon als het installatiemap-veld: de tekst vult nu de
   resterende breedte.
2. *Bladeren-knop krijgt een eigen tekstveld, net als de andere drie.* Direct gevolg van de
   feitencheck hierboven: omdat `WizardForm.DirBrowseButton` net als Terug/Volgende/Annuleren een
   `TNewButton` met `Caption` is, kan dat nu ook in de studio. Doorgevoerd door de hele keten:
   - `BrowseButtonSettings` (Core): nieuwe `Caption`-property, zelfde leeg-is-onveranderd-conventie
     als de rest van dat model. Bestaande opgeslagen projecten blijven werken (JSON-deserialisatie
     vult een ontbrekend veld gewoon met de lege standaardwaarde).
   - `SelectDestinationPageEditorViewModel`: nieuwe `BrowseButtonCaption`-eigenschap plus
     `EffectiveBrowseButtonCaption` (tweelaags: eigen tekst, anders Inno Setup's eigen
     standaardtekst via de nieuwe taalsleutel `ButtonWizardBrowse` — geen derde,
     Standaardscherm-laag, want die bestond al niet voor deze knop, zie `BrowseButtonSettings`).
   - `ScreenEditorControl.xaml`: de Bladeren-knoprij is niet langer een label-met-eigenschappen-
     knopje, maar een echt tekstveld (zelfde `MaxLength="30"`/breedte-aanpak als de polish
     hierboven), met een kleine sectiekop erboven.
   - `ButtonPropertiesViewModel`/`ButtonPropertiesWindow`: `HasCaption` stond al generiek in de
     dialoog (verbergt het Knoptekst-veld als een knop er geen heeft) — voor de Bladerknop nu
     gewoon op `true` gezet in plaats van een lege no-op-delegate.
   - `SelectDestinationPagePreview.xaml` (InnoSetupStudio.Wizard): de gesimuleerde Bladeren-knop
     in de voorvertoning toonde altijd het vaste "Browse..." — nu gebonden aan
     `EffectiveBrowseButtonCaption`, zodat getypte tekst daadwerkelijk zichtbaar wordt, net als bij
     Terug/Volgende/Annuleren.
   - Niet meegenomen: TextColor/lettertype van de Bladeren-knop worden in deze voorvertoningspagina
     nog niet toegepast (alleen Content/tekst) — dat was al zo vóór deze wijziging (een bestaande,
     kleinere hiaat, niet iets wat deze wijziging heeft veroorzaakt) en is niet aangepakt, want niet
     gevraagd.

**Build- en testresultaat.** `dotnet build`: 0 waarschuwingen, 0 fouten. `dotnet test`: 21/21
geslaagd (ongewijzigd — geen bestaande test verwijst naar `BrowseButtonSettings`/
`SelectDestinationBrowseButton`). Smoke-test: `InnoSetupStudio.exe` gestart en reageerde
(`Responding: True`), daarna afgesloten.


**Volledige knop-pariteit voor de Bladeren-knop (2026-09-30), zelfde branch.**
Herbert's instructie: "Alle knoppen moeten dezelfde bewerkingsfunctionaliteiten krijgen als de
knoppen '< Vorige', 'Volgende >', 'Annuleren' tenzij bepaalde functionaliteit niet beschikbaar is
in InnoSetup" — expliciet ook bedoeld voor eventuele toekomstige schermspecifieke knoppen, niet
alleen Bladeren. Bij de vorige wijziging (Caption, hierboven) bleven Enabled/Visible/Tooltip/
TextColor/lettertype van de Bladeren-knop achter: de velden bestonden al in
`BrowseButtonSettings`/`ButtonPropertiesWindow`, maar zonder de leeg-is-terugval-resolutie die
Terug/Volgende/Annuleren wél hebben, en zonder dat de voorvertoning er ook maar iets mee deed.
Omdat `WizardForm.DirBrowseButton` een `TNewButton` is — structureel identiek aan de andere drie
knoppen (geverifieerd via jrsoftware.org/ishelp) — is er voor geen van deze eigenschappen een
InnoSetup-beperking die pariteit in de weg staat. Doorgevoerd:

- `SelectDestinationPageEditorViewModel`: nieuwe `IsBrowseButtonVisible`/`IsBrowseButtonEnabled`
  (leeg/null is "aan", zelfde conventie als de drie gedeelde knoppen) en
  `IsBrowseButtonEnabledInPreview`, die dat combineert met Inno Setup's eigen ingebouwde gedrag
  (`AllowUserToChangeDir`) — een expliciete "Bladeren-knop uitschakelen"-instelling en Inno Setup's
  eigen automatische uitschakeling werken nu allebei, onafhankelijk van elkaar.
- `SelectDestinationPagePreview.xaml` (InnoSetupStudio.Wizard): de gesimuleerde Bladeren-knop
  bindt nu ook `Visibility`, `Foreground` (TextColor), `FontFamily`, `FontSize`, `FontWeight`
  (Bold) en `ToolTip` — voorheen bond alleen `Content`/`IsEnabled`. Vereiste vier nieuwe, kleine
  converters in een nieuwe map `src/InnoSetupStudio.Wizard/Converters/`
  (`HexColorToBrushConverter`, `FontFamilyOrUnsetConverter`, `FontSizeOrUnsetConverter`,
  `NullableBoolToFontWeightConverter`) — letterlijke kopieën van de gelijknamige converters in
  InnoSetupStudio.App, omdat het Wizard-project niet naar App mag verwijzen (circulaire
  referentie). Dezelfde soort bewuste, kleine duplicatie die al elders in het project voorkomt
  (bijv. `PickColor`/`NormalizeWhitespaceOnly`).
- Bewust NIET toegevoegd: een derde, Standaardscherm-cascadelaag voor de Bladeren-knop (zoals
  Terug/Volgende/Annuleren die wel hebben via `Defaults`/`RaiseEffectivePropertiesChanged`). Dat is
  geen InnoSetup-beperking maar een structureel verschil: de cascade bestaat om een instelling over
  meerdere schermen heen te kunnen hergebruiken, en de Bladeren-knop komt maar op één scherm voor
  (Bestemmingspagina). Er is dus geen tweede scherm om vanuit/naartoe te cascaderen. Blijft
  tweelaags: eigen waarde, anders Inno Setup's eigen ingebouwde gedrag/tekst. Als Herbert deze laag
  toch wil (bijvoorbeeld met het oog op toekomstige knoppen die wél op meerdere schermen
  voorkomen), is dat een aparte, gerichte uitbreiding.

**Build- en testresultaat.** `dotnet build`: 0 waarschuwingen, 0 fouten. `dotnet test`: 21/21
geslaagd (ongewijzigd). Smoke-test: `InnoSetupStudio.exe` gestart en reageerde
(`Responding: True`), daarna afgesloten.


**Standaardscherm was na selectie van een echt scherm niet meer terug te selecteren (2026-09-30),
zelfde branch.** Herbert's testfeedback op de vorige twee wijzigingen:

1. Akkoord met de tweelaagse resolutie voor knoppen die maar op één scherm voorkomen (geen
   Standaardscherm-cascadelaag) — bevestigt de eerder gemaakte keuze, geen codewijziging nodig.
2. De TextColor-wijziging van de Bladeren-knop leek niet te worden overgenomen. Vermoedelijke
   oorzaak: de vier nieuwe converters in `InnoSetupStudio.Wizard/Converters/` (vorige sectie
   hierboven) bleken tijdens het opstellen van die sectie nooit daadwerkelijk op schijf
   terechtgekomen te zijn ondanks een geslaagde melding — de map bestond niet, en `dotnet build`
   faalde daardoor eerst met `CS0234` bij het begin van deze sessie. Herbert heeft dus vermoedelijk
   een `.exe` getest die dateert van vóór deze preview-koppeling (TextColor stond toen inderdaad
   nog los van de voorvertoning, zoals expliciet gedocumenteerd in de vorige sectie). Na het
   opnieuw aanmaken van de vier bestanden bouwt/test/start alles weer correct, met TextColor
   zichtbaar gekoppeld aan de voorvertoning (zie vorige sectie). Geen aparte codewijziging nodig
   voor dit punt — wel gevraagd aan Herbert om na deze push opnieuw te bouwen en te testen.
3. Genuine bug, wel gevonden en gefixt: het Standaardscherm was, eenmaal een echt scherm
   geselecteerd, niet meer terug te selecteren door erop te klikken — de markering van
   Standaardscherm bleef bovendien zichtbaar staan alsof het nog steeds geselecteerd was.

   Oorzaak: `DefaultScreenListBox` en `ScreensListBox` (in `ScreenEditorControl.xaml`) binden
   allebei two-way naar dezelfde `WizardEditorViewModel.SelectedScreen`-eigenschap. Dat is een
   bekende WPF-eigenaardigheid: `Selector.SelectedItem` negeert een toewijzing die geen match
   vindt in de eigen `ItemsSource`, in plaats van de markering naar niets te wissen. Zodra je dus
   een echt scherm selecteerde in `ScreensListBox`, bleef `DefaultScreenListBox` intern nog steeds
   denken dat Standaardscherm geselecteerd was (zichtbaar aan de blijvende markering) — en een
   volgende muisklik daarop leverde voor WPF geen wijziging op (het was voor die ListBox toch al
   "geselecteerd"), dus er kwam geen `SelectionChanged`-event en dus ook nooit een nieuwe
   `SelectedScreen`-waarde.

   Dit staat los van de eerdere §12.7-beslissing om het Standaardscherm in een eigen rij, duidelijk
   visueel gescheiden door een scheidingslijn, te tonen (dat is een bewuste, blijvende keuze om
   duidelijk te maken dat het geen "scherm nul" tussen de echte installerschermen is) — die
   ontwerpkeuze is niet de oorzaak van deze bug en blijft ongewijzigd.

   Fix: een gedeelde `SelectionChanged`-handler (`ScreenListBox_SelectionChanged` in
   `ScreenEditorControl.xaml.cs`) die bij een selectie in de ene lijst expliciet de `SelectedItem`
   van de andere lijst op `null` zet (dat wist de markering altijd, ook als de ListBox zelf niet
   "weet" van de nieuwe waarde) en `SelectedScreen` daarna expliciet opnieuw zet. Een
   `_isSyncingScreenSelection`-guard voorkomt dat het nullen van de andere lijst zelf weer een
   (lege, dus genegeerde) heropvoering van deze handler veroorzaakt.

**Build- en testresultaat.** `dotnet build`: 0 waarschuwingen, 0 fouten. `dotnet test`: 21/21
geslaagd. Smoke-test: `InnoSetupStudio.exe` gestart en reageerde (`Responding: True`), daarna
afgesloten. Navigatie tussen Standaardscherm en de echte schermen kon niet door mij handmatig in
de UI doorgeklikt worden (dat blijft aan Herbert) — de fix is beoordeeld op basis van code-analyse
van het exacte WPF-mechanisme, niet op basis van visuele bevestiging.


**Tekstkleur van knoppen kwam nergens in een voorvertoning terecht (2026-09-30), zelfde branch.**
Herbert testte de Tekstkleur van de Bladeren-knop (screenshot: #008000 groen gekozen, zwatch en
hex-veld tonen correct groen) maar de knoptekst bleef zwart, zowel in de "Voorvertoning" onderaan
het Knop-eigenschappenscherm als in de echte voorvertoning van het Bestemmingsscherm.

Oorzaak gevonden: de gedeelde Button-stijl (`Themes/Styles.xaml`) heeft sinds 2026-09-28 een
`ControlTemplate.Resources`-stijl die knoptekst-TextBlocks via
`{Binding Foreground, RelativeSource={RelativeSource TemplatedParent}}` de Foreground van de knop
probeert te geven — bedoeld om te voorkomen dat de app-brede TextBlock-stijl (die overal
`Brush.TextPrimary` afdwingt) knoptekst overschrijft. Dat werkt alleen voor elementen die
letterlijk in de ControlTemplate zelf staan; `TemplatedParent` lost niet op voor Content dat van
buiten de template komt (een eigen `<TextBlock>` als knopinhoud, of een kale string die WPF impliciet
in een TextBlock verpakt) — precies wat de "Voorvertoning"-knop in `ButtonPropertiesWindow.xaml` en
de Bladeren-knop in `SelectDestinationPagePreview.xaml` allebei doen. Bij een niet-oplossende
binding valt de tekstkleur terug op zwart in plaats van de bedoelde kleur.

Interessant genoeg trof dit niet de "echte" Terug/Volgende/Annuleren-knoppen in de installer-
voorvertoning in `ScreenEditorControl.xaml`: die gebruiken al langer een ander, wél werkend patroon
(`RelativeSource AncestorType=Button` rechtstreeks op de content-TextBlock, in plaats van de
TemplatedParent-truc in de gedeelde stijl) — dat patroon nu ook toegepast op de twee kapotte
plekken:

- `ButtonPropertiesWindow.xaml`: de "Voorvertoning"-knop (gebruikt voor alle vier de knoppen:
  Terug/Volgende/Annuleren/Bladeren, want het is één herbruikbaar dialoogvenster) krijgt nu een
  expliciete `Foreground="{Binding Foreground, RelativeSource={RelativeSource AncestorType=Button}}"`
  op zijn interne TextBlock.
- `SelectDestinationPagePreview.xaml`: `Content="{Binding EffectiveBrowseButtonCaption}"`
  (impliciete string-naar-TextBlock, dus hetzelfde probleem) vervangen door een expliciete
  TextBlock met dezelfde Foreground-binding. Bewust ook dit project geraakt: `Application.Resources`
  werkt proces-breed, dus de gedeelde Button-stijl uit InnoSetupStudio.App geldt ook voor knoppen in
  InnoSetupStudio.Wizard, ook al verwijst dat project niet naar App.

De onderliggende `ControlTemplate.Resources`-stijl in `Themes/Styles.xaml` zelf is NIET aangepast —
die blijft voor nu ongebruikt/inert liggen. Ik heb bewust niet geprobeerd die te herstellen of te
verwijderen: dat raakt de Button-stijl voor de hele applicatie (inclusief donkere thema's die ik
niet zelf kan zien renderen), en het risico van een brede, moeilijk te overziene regressie weegt
niet op tegen het gerichte, al bewezen werkende patroon dat de twee daadwerkelijk gemelde plekken
nu gebruiken.

**Build- en testresultaat.** `dotnet build`: 0 waarschuwingen, 0 fouten. `dotnet test`: 21/21
geslaagd. Smoke-test: `InnoSetupStudio.exe` gestart en reageerde (`Responding: True`), daarna
afgesloten. De daadwerkelijke tekstkleur kon ik niet zelf visueel controleren (geen UI-doorklik) —
gevraagd aan Herbert om opnieuw te testen.

**Herbert bevestigd (2026-09-30):** tekstkleur wordt nu correct toegepast, ook voor de knoppen
(Terug/Volgende/Annuleren) waar dit al langer "werkte" — bevestigt dat de root-cause-analyse
hierboven klopte: de zwarte tekst was een sluimerende bug die ook die knoppen al raakte, niet iets
dat alleen de Bladeren-knop trof.


## 22. Backlog: developer-template / projectdefaults (2026-09-30, nog niet ontworpen)

Herbert wil op termijn een manier om een soort "developer template" te maken: een set defaults
(kleurgebruik, company logo, directorystructuur) die bij elk nieuw project automatisch worden
toegepast, in plaats van steeds opnieuw hetzelfde in te stellen. Expliciet voor later — hier alleen
vastgelegd zodat het idee niet kwijtraakt, net als de andere backlogitems in dit document (secties
14 en 20). Nog niet ontworpen: geen besluit over hoe zo'n template wordt opgeslagen (los bestand,
onderdeel van instellingen, per-gebruiker of gedeeld binnen Voortman), hoe die inwerkt op een nieuw
project (kopiëren bij aanmaken, of een levend sjabloon dat overschrijfbaar blijft), en welke
projectvelden precies "template-waardig" zijn.

Raakt vermoedelijk `WizardEditorViewModel`'s project-aanmaakpad (`New project`) en, afhankelijk van
scope, ook de wizard-afbeeldingen/kleuren van het Standaardscherm. Logisch pas op te pakken nadat
de projectinstellingen en de schermeditor verder zijn uitgekristalliseerd (meer velden = duidelijker
wat een zinvolle default is) — vandaar Herberts eigen inschatting dat dit voor later is.


## 23. Waarschuwing bij niet-opgeslagen schermwijzigingen (2026-09-30)

Oplossing voor het dataverlies-risico dat als CodeRabbit-bevinding #3 (PR #19) bewust NIET
automatisch werd opgelost, maar als nieuw backlogitem werd vastgelegd (zie sectie hierboven,
"CodeRabbit-bevindingen PR #19, geverifieerd en verwerkt"): `SetActiveProject` (MainWindow.xaml.cs)
bouwt bij elke aanroep een gehele nieuwe `WizardEditorViewModel`, zonder te controleren of de
vorige nog niet-opgeslagen wijzigingen had (`IsDirty == true`). Voorbeeld: een knopomschrijving
typen in de schermeditor zonder op Opslaan te klikken, dan via de bovenbalk Projectinstellingen
(her)openen — de getypte wijziging verdween dan stilletjes.

Herbert koos hiervoor expliciet voor de eenvoudigste variant: vragen of de wijzigingen opgeslagen
moeten worden, in plaats van automatisch samenvoegen of stilzwijgend negeren (een instelbare
"Vragen"/"Automatisch opslaan"-voorkeur is expliciet voor later, zie onderaan).

**Gekozen aanpak.** Een nieuwe hulpmethode `ConfirmDiscardUnsavedScreenChangesAsync` in
MainWindow.xaml.cs, aangeroepen aan het begin van elk van de drie aanroeppaden die uiteindelijk op
`SetActiveProject` uitkomen:

- `NewProjectButton_Click` (Nieuw project)
- `OpenProjectButton_Click` (Project openen)
- `ProjectSettingsButton_Click` (Projectinstellingen heropenen voor het al actieve project — dit is
  exact het CodeRabbit-scenario hierboven)

Als `ScreenEditor.ViewModel.IsDirty` niet waar is (geen actief project, of de schermeditor is niet
gewijzigd), gaat de aanroeper direct door — geen dialoog voor niets. Anders toont
`MessageBox.Show` met `MessageBoxButton.YesNoCancel`:

- **Ja** — dezelfde volgorde als `ScreenEditor_SaveClicked`: `ApplyTo` schrijft de bewerkte velden
  terug naar `_activeProject`, `IsDirty` gaat vóór de `await` op false, en `SaveActiveProjectAsync`
  wordt aangeroepen. Mislukt het opslaan, dan gaat `IsDirty` weer op true en breekt de aanroeper de
  actie af (dezelfde bestaande logica als bij de inline Opslaan-knop, nu hergebruikt).
- **Nee** — wijzigingen worden verworpen, de aanroeper gaat door (de eigen aanroep bouwt zo dadelijk
  toch een nieuwe `WizardEditorViewModel` via `SetActiveProject`).
- **Annuleren** (of het venster gesloten) — de aanroeper breekt de actie af; het actieve project en
  de schermeditor blijven ongewijzigd, alsof er niets gebeurd is.

**Waarom vóór `SetActiveProject` zelf, niet erin.** `ProjectSettingsWindow` is modaal
(`ShowDialog`), dus tussen de aanroep van `ConfirmDiscardUnsavedScreenChangesAsync` in bijvoorbeeld
`ProjectSettingsButton_Click` en de latere `SetActiveProject`-aanroepen binnen `OpenProjectSettings`
kan de schermeditor niet alsnog dirty worden — de gebruiker kan er in die tussentijd niet bij. Eén
controlepunt per gebruikersactie volstaat dus; een controle binnen `SetActiveProject` zelf zou
hetzelfde afvangen maar minder duidelijk maken welke gebruikersactie de vraag veroorzaakt (relevant
voor de dialoogtekst/titel).

**Nieuwe vertaalstrings** (`Strings.resx`/`.en-US`/`.de-DE`, na `ButtonClose`, zelfde patroon als de
rest van dit bestand): `UnsavedScreenChangesTitle` en `UnsavedScreenChangesMessage`. De
Ja/Nee/Annuleren-knoppen van `MessageBoxButton.YesNoCancel` zelf zijn NIET apart vertaald — die
komen van Windows' eigen gelokaliseerde resources op basis van `CultureInfo.CurrentUICulture`, die
`LocalizationManager.SetLanguage` al proces-breed instelt (zie `LocalizationManager.cs`), dus die
volgen vanzelf de actieve schermtaal.

**Bewust niet gedaan.** Een instelbare voorkeur tussen "Vragen" en "Automatisch opslaan" bij
niet-opgeslagen wijzigingen — Herbert noemde dit zelf expliciet als iets voor een later stadium, niet
onderdeel van deze fix. Vastgelegd als nieuw, nog niet ontworpen backlogitem.

**Build- en testresultaat.** `dotnet build`: 0 waarschuwingen, 0 fouten. `dotnet test`: 21/21
geslaagd (ongewijzigd). Smoke-test: `InnoSetupStudio.exe` gestart en reageerde (`Responding: True`),
daarna afgesloten. De daadwerkelijke dialoog (tekst, knoppen, het Ja/Nee/Annuleren-gedrag) kon ik
niet zelf visueel doorklikken — gevraagd aan Herbert om te testen.

**Aanvulling (2026-09-30, zelfde branch): niet-opgeslagen wijzigingen ín Projectinstellingen
zelf.** Herbert testte sectie 23 hierboven handmatig en vond een gerelateerd maar apart gat: de
waarschuwing hierboven dekt alleen de schermeditor (`WizardEditorViewModel.IsDirty`), niet
`ProjectSettingsWindow`'s eigen velden (`ProjectSettingsViewModel.IsDirty`, bijvoorbeeld een
getypte Applicatienaam). Zijn testresultaten, letterlijk:

- Nieuw project, Applicatienaam getypt, gesloten via X → verwachtte een melding, kreeg er geen.
  Niet goed.
- Nieuw project, Applicatienaam getypt, Annuleren gekozen → verwachtte bewust GEEN melding ("ik
  kies bewust voor annuleren"), kreeg er ook geen. Prima zo.
- Bestaand project geopend, Applicatienaam gewijzigd, gesloten via X → verwachtte een melding,
  kreeg er geen. Niet goed.
- Bestaand project geopend, Applicatienaam gewijzigd, Openen gekozen → verwachtte een melding,
  kreeg er geen. Niet goed.
- Projectinstellingen heropend voor een al actief project, Applicatienaam gewijzigd, gesloten via
  X → verwachtte een melding, kreeg er geen. Niet goed.
- Zelfde, maar Openen gekozen → verwachtte een melding, kreeg er geen. Niet goed.
- (Ter controle, drie scenario's met een schermwijziging in plaats van een projectinstelling, via
  Nieuw project/Project openen/Projectinstellingen — alle drie toonden terecht de melding: sectie
  23 hierboven werkte hier al correct.)

**Belangrijke nuance uit Herberts eigen testresultaten:** Annuleren op een NIEUW project mag
bewust ZONDER melding blijven — die knopklik ís zelf al de expliciete keuze om te verwerpen (zie
`CancelButtonText`: toont dan ook letterlijk "Annuleren"). Bij een bestaand project heet diezelfde
knop "Openen" (het project blijft open, de instellingen blijven zoals ze op schijf staan) — dat is
dubbelzinnig zodra er nog niet-opgeslagen veldwijzigingen zijn, dus daar wél een melding. X-sluiten
(native titelbalk) is in alle gevallen dubbelzinnig — daar dus altijd een melding, ongeacht
nieuw/bestaand project.

**Aanpak.** Twee wijzigingen, beide in de bestaande call-structuur:

1. `ProjectSettingsViewModel.Cancel()` omgezet naar `CancelAsync()` (CommunityToolkit's
   `[RelayCommand]` genereert dezelfde `CancelCommand`-naam, dus geen XAML-wijziging nodig). Bij
   `IsExistingProject` roept deze nu eerst de nieuwe `ConfirmDiscardChangesAsync()` aan; bij een
   nieuw project blijft het gedrag ongewijzigd (direct `RequestClose(false)`, geen vraag).
2. `ProjectSettingsWindow.xaml.cs` kreeg een `Closing`-event-handler — er was voorheen geen enkele
   hook op X-sluiten, dus dat pad ging altijd rechtstreeks langs `RequestClose`/`CancelCommand`
   heen. Deze handler roept, ongeacht nieuw/bestaand project, ook `ConfirmDiscardChangesAsync()`
   aan zodra `ViewModel.IsDirty` waar is. Een `_programmaticClose`-vlag (gezet in `OnRequestClose`,
   dus bij elke Opslaan/Annuleren/Openen-afhandeling) onderscheidt "dit Close()-aanroep komt al
   via een knop die zijn eigen afweging al maakte" van "dit is een echte X-klik" — zonder die vlag
   zou een programmatische `Close()` na een geslaagde Opslaan de Closing-handler opnieuw laten
   vragen.

`ConfirmDiscardChangesAsync()` (nieuw op `ProjectSettingsViewModel`, publiek, gedeeld door beide
aanroeppaden hierboven) volgt hetzelfde Ja/Nee/Annuleren-patroon als
`MainWindow.ConfirmDiscardUnsavedScreenChangesAsync` (sectie 23): Ja slaat op (met een eigen
`CanSave()`-controle vooraf — een lege verplichte Applicatienaam kan niet stilzwijgend als
"opgeslagen" gelden), Nee verwerpt en gaat door, Annuleren houdt het venster open. Omdat Opslaan
hier zelf al `RequestClose(true)` vuurt bij succes, geeft de methode een driewaardig resultaat
terug (`UnsavedChangesDecision`: `Proceed`/`Abort`/`AlreadyClosing`) zodat de aanroeper weet of hij
zelf nog `RequestClose(false)` moet vuren, moet stoppen (venster blijft open), of niets meer hoeft
te doen (al gesloten via Opslaan).

**Vertaalstrings.** `UnsavedScreenChangesTitle` (sectie 23) hernoemd naar het generieke
`UnsavedChangesTitle`, nu hergebruikt door beide dialogen — de tekst zelf ("Niet-opgeslagen
wijzigingen") was al generiek genoeg, alleen de sleutelnaam verwees nog specifiek naar de
schermeditor. Twee nieuwe strings toegevoegd in alle drie `Strings*.resx`:
`UnsavedProjectSettingsMessage` (de Ja/Nee/Annuleren-vraag) en
`UnsavedProjectSettingsCannotSaveMessage` (getoond als Ja gekozen wordt maar Opslaan niet kan,
bijvoorbeeld een lege Applicatienaam).

**Build- en testresultaat.** `dotnet build`: 0 waarschuwingen, 0 fouten. `dotnet test`: 21/21
geslaagd (ongewijzigd). Smoke-test: `InnoSetupStudio.exe` gestart en reageerde (`Responding:
True`), daarna afgesloten. De zes hersteldialogen (drie scenario's × X-sluiten/Openen) kon ik niet
zelf visueel doorklikken — gevraagd aan Herbert om opnieuw te testen, inclusief het scenario waarin
hij bij de Ja/Nee/Annuleren-vraag voor "Ja" (opslaan) kiest.


**Bugfix (2026-10-01, zelfde branch): "Cannot ... Close ... while a Window is closing" bij Nee
op de X-sluit-vraag.** Herbert testte de vorige aanvulling (sectie hierboven) en kreeg op alle zes
genoemde plekken terecht de waarschuwing, maar bij Nee (wijzigingen verwerpen) op de X-sluit-vraag
verscheen in plaats van het venster dat sloot deze WPF-foutmelding:

> Cannot set Visibility to Visible or call Show, ShowDialog, Close, or
> WindowInteropHelper.EnsureHandle while a Window is closing.

**Oorzaak.** `ProjectSettingsWindow_Closing` zet `e.Cancel = true` en `await`
`ConfirmDiscardChangesAsync()`. Bij Nee (of bij een `CanSave()`-mislukking) bevat die methode geen
enkele échte asynchrone operatie vóór haar return — `MessageBox.Show` is een synchrone, blokkerende
aanroep met een eigen geneste berichtenlus, geen `await`. Daardoor keert de `async`-methode feitelijk
synchroon terug, nog steeds binnen dezelfde aanroepstack als WPF's eigen Closing-dispatch. Een
rechtstreekse `Close()`-aanroep (en, bleek bij nader inzien, ook het zetten van `DialogResult`, dat
intern zelf `Close()` aanroept) ví·n die stack raakt WPF's interne "venster is aan het sluiten"-
bewaking, vandaar de foutmelding. Bij Ja (opslaan) trad dit toevallig niet op, omdat `SaveAsync`'s
echte bestands-I/O wél een echte `await`-onderbreking veroorzaakt — de melding was dus
inputafhankelijk, niet bij elke keuze reproduceerbaar.

**Fix.** Zowel `OnRequestClose` (het gedeelde sluitpad voor Opslaan/Annuleren/Openen, inclusief een
geslaagde save vanuit `ConfirmDiscardChangesAsync`) als de `Proceed`-tak van
`ProjectSettingsWindow_Closing` zelf stellen de daadwerkelijke `DialogResult`/`Close()`-aanroep nu
uit via `Dispatcher.BeginInvoke`, in plaats van die rechtstreeks te doen. Dat plaatst de aanroep op
een nieuwe dispatcher-cyclus, altijd ná volledige afhandeling van de huidige (eventuele) Closing-
dispatch — ongeacht of de weg ernaartoe een echte `await` passeerde of niet. Voor de knop-paden
(Opslaan/Annuleren buiten een X-klik om, dus sowieso al buiten elke Closing-dispatch) is dit
onmerkbaar: één dispatcher-tick later sluit het venster, zoals voorheen.

**Build- en testresultaat.** `dotnet build`: 0 waarschuwingen, 0 fouten (de CS4014-waarschuwing
over de niet-afgewachte `DispatcherOperation` is weggenomen met een expliciete `_ =`-discard, want
fire-and-forget is hier precies de bedoeling). `dotnet test`: 21/21 geslaagd. Smoke-test:
`InnoSetupStudio.exe` gestart en reageerde (`Responding: True`), daarna afgesloten. Het specifieke
Nee-pad op de X-sluit-vraag (waar de fout optrad) kon ik niet zelf visueel doorklikken — gevraagd
aan Herbert om dat scenario opnieuw te testen, samen met de overige vijf uit de vorige ronde.


**CodeRabbit-bevindingen PR #20, geverifieerd en verwerkt (2026-10-01).** Vier "actionable
comments" over twee reviewrondes (op de eerste en de laatste commit van deze branch), elk tegen de
code zelf gecontroleerd vóór toepassing:

1. *Genuine, maar bewust NIET automatisch opgelost.* `ConfirmDiscardUnsavedScreenChangesAsync`
   (MainWindow) roept `viewModel.ApplyTo(_activeProject)` aan vóórdat de save-poging start. Mislukt
   die save (bestand in gebruik, schijf vol), dan blijft `_activeProject` in het geheugen toch al
   gewijzigd staan — `IsDirty` gaat weliswaar terug op true, maar het onderliggende project-object
   draagt de nooit-bevestigde wijziging al met zich mee. Een latere `SetActiveProject`-aanroep met
   datzelfde (nog niet herladen) `_activeProject` zou die nooit-opgeslagen wijziging dan ongemerkt
   als "huidige staat" tonen. Dit patroon bestond al vóór deze branch, identiek, in
   `ScreenEditor_SaveClicked` (sectie 21) — niet iets dat met deze PR is geïntroduceerd. De juiste
   oplossing (een snapshot van het project bewaren en bij mislukking terugzetten, of ApplyTo pas na
   een geslaagde save uitvoeren wat een herontwerp van `SaveActiveProjectAsync` vergt) is een
   bredere wijziging die beide aanroepplekken raakt — vastgelegd als nieuw, nog niet ontworpen
   backlogitem, dezelfde afweging als steeds bij dit project.
2. *Genuine, opgelost.* Diezelfde methode gaf bij een geslaagde save onvoorwaardelijk `true` terug,
   zonder te controleren of `viewModel.IsDirty` intussen (tijdens de save-await) opnieuw op true was
   gezet. De ScreenEditor blijft namelijk interactief tijdens deze save (in tegenstelling tot
   ProjectSettingsWindow, waar `CanEdit`/`IsSaving` de velden uitschakelt) — typt de gebruiker
   tijdens het opslaan zelf nog iets, dan zou de aanroeper die nieuwe wijziging alsnog stilzwijgend
   weggooien via `SetActiveProject`. Fix: `return !viewModel.IsDirty;` in plaats van
   onvoorwaardelijk `true` ná een geslaagde save.
3. *Genuine, maar bewust NIET automatisch opgelost.* Tussen de eerste
   `ConfirmDiscardUnsavedScreenChangesAsync`-aanroep in `OpenProjectButton_Click` en de
   daadwerkelijke `SetActiveProject`-aanroep zit nog een `await _projectService.LoadAsync(...)`,
   waartijdens de (oude, nog niet vervangen) ScreenEditor weer interactief is. Een CodeRabbit-
   gesuggereerde tweede confirm-aanroep ná die LoadAsync zou, zoals letterlijk voorgesteld, een
   nieuwe bug introduceren: na een expliciete "Nee" (verwerpen) op de eerste vraag wordt
   `viewModel.IsDirty` niet teruggezet, dus een tweede controle zou **dezelfde**, al beantwoorde
   vraag opnieuw tonen. Een correcte fix vergt dus eerst ook IsDirty resetten bij "Nee" — en het
   venster waarin dit kan misgaan is bovendien extreem smal (een lokale bestandslezing duurt
   doorgaans een fractie van een seconde). Vastgelegd als nieuw, nog niet ontworpen backlogitem in
   plaats van een haastige fix die een nieuwe regressie riskeert.
4. *Genuine (robuustheid, geen aantoonbaar bereikbare bug), opgelost.* `ConfirmDiscardChangesAsync`
   (ProjectSettingsViewModel) leidde succes van `SaveAsync` af via `IsDirty` achteraf, in plaats van
   een expliciet resultaat. In de praktijk klopt dat zijkanaal hier altijd — `CanEdit`/`IsSaving`
   schakelt de velden uit tijdens het opslaan, dus de race uit punt 2 hierboven is hier niet
   mogelijk — maar een expliciet resultaat is ondubbelzinniger en blijft dat ook als die aanname
   ooit wijzigt. Fix: `SaveAsync` opgesplitst in de bestaande knop-aanroep (ongewijzigde
   `Task`-vorm voor `[RelayCommand]`) en een nieuwe `SaveCoreAsync` die een `Task<bool>` teruggeeft;
   `ConfirmDiscardChangesAsync` gebruikt nu rechtstreeks dat resultaat.

**Build- en testresultaat.** `dotnet build`: 0 waarschuwingen, 0 fouten. `dotnet test`: 21/21
geslaagd (ongewijzigd). Smoke-test: `InnoSetupStudio.exe` gestart en reageerde (`Responding:
True`), daarna afgesloten. Punt 2 en 4 wijzigen geen zichtbaar gedrag in de door Herbert al
bevestigde scenario's (alleen interne robuustheid); geen nieuwe handmatige doorloop nodig vóór
merge.

## 24. Meertalige knopteksten (2026-10-01)

Tweede item van de na PR #20 afgesproken roadmap (direct gestart op Herberts expliciete verzoek:
"Ik wil daar nu direct mee beginnen"). Doel: voor een meertalig project (meer dan één
geselecteerde taal, zie Talen-tab in Projectinstellingen) ook de knopteksten (Caption) en tooltips
van Terug/Volgende/Annuleren en de Bladerknop per taal kunnen invullen, in plaats van één
Engelstalige tekst die voor elke installatietaal getoond wordt.

### Ontwerpkeuze: per-knop-lijst in het bestaande Knop-eigenschappenscherm

Drie opties besproken via AskUserQuestion vóór implementatie (project-principe "eerst ontwerpen,
dan bouwen"): (a) een centrale matrix-achtige vertalingentabel elders in de app, (b) een
losstaand "Vertalingen"-venster per scherm, (c) per knop een lijst met één rij per taal, in het
bestaande ButtonPropertiesWindow. Herbert koos (c), "aanbevolen": knop en vertaling blijven zo op
dezelfde plek zichtbaar, geen nieuw venstertype nodig, en de sectie verschijnt vanzelf alleen voor
een project dat al meer dan één taal heeft.

### Datamodel: bestaand veld = Engelse/universele terugvalwaarde

Zowel `WizardScreenButtonSettings` (Terug/Volgende/Annuleren, per scherm) als
`BrowseButtonSettings` (de Bladerknop) kregen per bestaand Caption/Tooltip-veld een nieuwe
`Dictionary<string, string> XxxByLanguage`-eigenschap (zes nieuwe velden op
WizardScreenButtonSettings, twee op BrowseButtonSettings). Sleutel is een taal-id uit
InnoLanguageCatalog (bijvoorbeeld "dutch"), nooit `InnoLanguageCatalog.EnglishId`: Engels blijft
gewoon het bestaande Caption/Tooltip-veld gebruiken. Een lege waarde of ontbrekende sleutel
betekent "deze taal gebruikt ook gewoon de Engelse/universele tekst" — exact dezelfde leeg-is-
onveranderd-conventie als de rest van WizardScreenButtonSettings, en bewust hetzelfde terugvalgedrag
als Inno Setup's eigen `CustomMessage()`-mechanisme: ontbreekt een taalspecifieke
`[CustomMessages]`-regel, dan valt Inno Setup terug op de EERSTE taal in `[Languages]`, en dat is
in deze app altijd Engels (`InstallerProject.SupportedLanguageIds` bevat altijd
`InnoLanguageCatalog.EnglishId`, als eerste).

Dit is volledig backward-compatible: een ouder .issproj-bestand zonder deze velden deserialiseert
via System.Text.Json gewoon naar een lege dictionary (de parameterloze constructor van
`WizardScreenButtonSettings`/`BrowseButtonSettings` zet het veld al op `new()`, en een ontbrekende
JSON-sleutel overschrijft dat nooit met null) — geen migratie nodig, geen aanpassing aan
`JsonInstallerProjectService` (anders dan bij eerdere, vergelijkbare uitbreidingen zoals
WizardScreens/SupportedLanguageIds, waar wél een expliciete `??=`-normalisatie nodig was voor het
geval van een expliciete JSON-`null`). Zie de nieuwe test
`LoadAsyncDefaultsLanguageOverrideDictionariesForOlderProjectFileWithoutThem`.

**Bewuste vereenvoudiging: geen cascade via het Standaardscherm.** Caption/Tooltip zelf cascaderen
drielaags (eigen scherm → Standaardscherm → Inno Setup's ingebouwde tekst, zie §12.6/§12.7). De
nieuwe per-taal-dictionaries doen dat niet: een vertaling geldt alleen voor het scherm waarop hij
is ingevuld. Wil je bijvoorbeeld de Nederlandse Annuleren-tekst op elk scherm hetzelfde laten zijn,
dan vul je die nu op elk scherm apart in. Zonder deze vereenvoudiging had elke taal ook zijn eigen
Standaardscherm-laag nodig (een extra set dictionaries op `DefaultScreenEditorViewModel`, plus
live-doormelding naar elk scherm net als `RaiseEffectivePropertiesChanged` dat voor de bestaande
Effective*-eigenschappen doet) — dat vergroot de omvang van deze eerste versie aanzienlijk. Kan
later alsnog toegevoegd worden als Herbert daar in de praktijk behoefte aan blijkt te hebben
(genoteerd als backlogitem hieronder). Sinds sectie 34 is deze vereenvoudiging opgeheven: vertalingen cascaderen nu wel via het Standaardscherm.

### UI: nieuwe sectie in ButtonPropertiesWindow, alleen zichtbaar voor een meertalig project

`ButtonPropertiesViewModel` kreeg een nieuwe geneste `LanguageOverrideRow`-klasse (zelfde eenvoudige
rij-object-aanpak als `LanguageRow` in de Talen-tab) en een `LanguageOverrides`-lijst: één rij per
niet-Engelse, geselecteerde taal van het project, in InnoLanguageCatalog-volgorde. Opgebouwd in de
constructor uit een nieuwe `NonEnglishLanguageIds`-parameter plus vier nieuwe get/set-delegates
(Caption/Tooltip-dictionary), zelfde "adapter met delegates"-patroon als de bestaande acht velden.
`HasLanguageOverrides` (`LanguageOverrides.Count > 0`) bepaalt in ButtonPropertiesWindow.xaml of de
hele sectie getoond wordt — een eentalig project (het gebruikelijke geval) laat hem dus gewoon weg.
Elke rij heeft een eigen Caption/Tooltip-tekstvak; bij Opslaan filtert `Save()` lege/witruimte-
waarden eruit (geen sleutel in de dictionary, consistent met de leeg-is-onveranderd-conventie)
vóórdat de twee set-delegates worden aangeroepen.

`NonEnglishLanguageIds` (project.SupportedLanguageIds minus Engels) wordt één keer per
schermeditor-sessie berekend in `WizardEditorViewModel`'s constructor en aan elk scherm meegegeven
— als nieuwe required-init-eigenschap op de basisklasse `WizardScreenEditorViewModel` (erft dus ook
door naar `SelectDestinationPageEditorViewModel`), en als nieuwe constructorparameter op
`DefaultScreenEditorViewModel` (geen gedeelde basisklasse, zie die klassencommentaar). Omdat
`MainWindow.SetActiveProject` deze hele `WizardEditorViewModel` na elke Projectinstellingen-opslag
opnieuw opbouwt (zie sectie 23), komt een gewijzigde talenselectie hier vanzelf weer vers binnen —
geen aparte verversingslogica nodig.

`ScreenEditorControl.xaml.cs`'s drie bouwmethoden (`BuildForScreenButton`,
`BuildForDefaultScreenButton`, `BuildForBrowseButton`) geven nu ook `vm.NonEnglishLanguageIds` en de
vier nieuwe delegates door aan `ButtonPropertiesViewModel`'s constructor.

### Build- en testresultaat

`dotnet build` (volledige oplossing): 0 waarschuwingen, 0 fouten. `dotnet test`: 22/22 geslaagd (21
bestaand + 1 nieuwe: `LoadAsyncDefaultsLanguageOverrideDictionariesForOlderProjectFileWithoutThem`).
De bestaande round-trip-test (`JsonInstallerProjectServiceRoundTripsAllFields`) is uitgebreid met
twee gevulde per-taal-dictionaries op `WelcomeScreenButtons`, plus assertions dat de overige
dictionaries op dat scherm na een save/load-cyclus leeg (niet null) blijven. Smoke-test:
`InnoSetupStudio.exe` gestart en reageerde (`Responding: True`), daarna afgesloten.

### Backlog

- Cascade van per-taal-vertalingen via het Standaardscherm: gebouwd, zie sectie 34.
- De generator (fase 5/6, nog niet gebouwd) moet deze dictionaries omzetten naar een
  `[CustomMessages]`-sectie (`MyBackCaption.dutch=Terug` enz.) plus `CustomMessage(...)`-aanroepen
  in de Pascal Script `CurPageChanged`-event-handler, in plaats van de huidige aanname (vóór dit
  item) dat Caption altijd een vaste string was.

### CodeRabbit-bevindingen PR #21, geverifieerd en verwerkt (2026-10-01)

1. *Genuine, opgelost.* `ButtonPropertiesViewModel.Save()` herbouwde de hele
   `BackButtonCaptionByLanguage`-dictionary (en de andere vijf) uit louter de zichtbare
   `LanguageOverrides`-rijen. `LanguageOverrides` bevat alleen rijen voor de talen die bij het
   OPENEN van dit scherm geselecteerd waren (`NonEnglishLanguageIds`) — een taal die al een
   vertaling had maar intussen in de Talen-tab is uitgevinkt, kreeg dus geen rij, en de
   eerstvolgende Opslaan van dit knop-scherm (voor eender welke wijziging, ook een die niets met
   vertalingen te maken had) wiste die vertaling stilzwijgend. Fix: `Save()` merget nu via de
   nieuwe `MergeLanguageOverrides`-methode tegen `_originalCaptionByLanguage`/
   `_originalTooltipByLanguage` (een kopie van de dictionary zoals die bij het openen was) — alleen
   de talen die als rij zichtbaar waren worden aangepast/verwijderd, elke andere sleutel blijft
   ongewijzigd staan.
2. *Genuine, opgelost.* Dezelfde wortel maakte een tweede, op het eerste gezicht onschuldig lijkend
   gat zichtbaar: `BuildForDefaultScreenButton` (ScreenEditorControl.xaml.cs) gaf ook
   `vm.NonEnglishLanguageIds` door voor de drie knoppen van het Standaardscherm, dus toonde ook
   dáár de per-taal-vertalingensectie. Maar de per-taal-dictionaries op het Standaardscherm
   cascaderen bewust niet door naar de echte schermen (zie sectie 24 hierboven) — alleen de
   gewone, Engelse Caption/Tooltip-velden doen dat. Een daar ingevulde vertaling zou dus stil
   niets doen, wat verwarrend is naast de rest van dat scherm (waar alles juist WEL overal
   doorwerkt). Fix: `BuildForDefaultScreenButton` geeft nu een vaste lege lijst
   (`NoLanguageOverridesOnDefaultScreen`) door in plaats van `vm.NonEnglishLanguageIds`, zodat
   `ButtonPropertiesViewModel.HasLanguageOverrides` daar altijd false is en de sectie verborgen
   blijft. Punt 1's merge-fix beschermt daarnaast ook meteen de dictionaries van het
   Standaardscherm zelf: met een lege rijenlijst raakt `MergeLanguageOverrides` daar nooit iets
   aan, dus een bestaand gevulde dictionary (bijvoorbeeld uit een handmatig bewerkt projectbestand,
   of een toekomstige Standaardscherm-cascade) overleeft een Opslaan van dat scherm ongewijzigd.

**Build- en testresultaat na deze fix.** `dotnet build`: 0 waarschuwingen, 0 fouten. `dotnet test`:
22/22 geslaagd (ongewijzigd — beide fixes wijzigen geen bestaand, al geteste gedrag, alleen de
nieuwe meertalige-vertalingen-functionaliteit uit deze PR zelf).


### Layout-verduidelijking na Herberts handmatige UI-test (2026-10-01)

Herbert heeft PR #21 handmatig getest en bevestigd dat het beheer van meertalige knopteksten
functioneel goed werkt ("Het beheer werkt op dit moment goed"). Hij meldde daarbij twee
layout-punten in `ButtonPropertiesWindow.xaml` die de bruikbaarheid verminderden, zonder dat er
iets functioneel fout was:

1. **Dubbelzinnigheid welke taal de bovenste Tekst/Tooltip-velden zijn.** Bij een meertalig
   project met bijvoorbeeld Engels + Duits geselecteerd, en de interface van Inno Setup Studio
   zelf ook in het Duits, leek het net of de bovenste velden (die altijd de Engelse/universele
   terugvalwaarde zijn, zie sectie 24 hierboven) de Duitse knoptekst waren — er stond geen enkele
   aanduiding bij welke taal dat veld bedient. Herberts voorstel: "tussen haakjes (Engels) achter
   zetten, in de geselecteerde taal." Fix: nieuwe resourcesleutel `LabelEnglishSuffix`
   ("(Engels)" / "(English)" / "(Englisch)" in de drie Strings*.resx-bestanden) die als kleine,
   secundair gekleurde `TextBlock` naast zowel het Tekst- als het Tooltip-label staat. Deze
   marker is net als de hele vertalingensectie alleen zichtbaar als `HasLanguageOverrides` waar
   is (dus alleen bij een meertalig project) — bij een eentalig project is er toch geen andere
   taal om mee te verwarren, dus blijft het scherm daar ongewijzigd zonder extra ruis.
2. **Tooltip stond niet direct onder Tekst voor de standaardtaal.** Elke rij in de sectie
   Vertalingen per taal toont Tooltip direct onder Tekst (logisch: TextColor/FontFamily/FontSize
   zijn niet taalafhankelijk, dus horen daar niet tussen). De bovenste, Engelse Tekst/Tooltip
   volgden dat patroon niet: Tooltip stond pas na TextColor/FontFamily/FontSize/FontBold.
   Herberts opmerking: "Ik den dat het logischer is ook de Tooltip voor de standaard taal direct
   onder de tekst te zetten." Fix: de Tooltip-`StackPanel` is verplaatst naar direct na de
   Tekst-`StackPanel`, vóór TextColor — dezelfde volgorde als elke taalrij hieronder.

Beide wijzigingen raken alleen XAML-structuur en resourcesleutels, geen ViewModel-logica: er is
geen nieuwe property, geen nieuwe databinding-pad en geen wijziging aan `Save()`/
`MergeLanguageOverrides`. Daarom was er geen nieuwe testcode nodig.

**Build- en testresultaat na deze layout-wijziging.** `dotnet build`: 0 waarschuwingen, 0 fouten.
`dotnet test`: 22/22 geslaagd (ongewijzigd, zoals verwacht bij een zuivere layout-aanpassing).


## 25. Overige instellingen: bureaublad-snelkoppeling, startmenu, update capability (2026-10-01)

Roadmapitem 3 (zie de projectkickoff): een nieuw tabblad "Overige instellingen" in
`ProjectSettingsWindow`, voor instellingen die niet bij één specifiek wizardscherm horen.

### Referentie en scope-afbakening

Herbert gaf als referentie een aantal schermafbeeldingen van Inno Script Studio (Kymoto
Solutions) door, een ouder vergelijkbaar programma. Dat programma biedt tientallen van dit soort
instellingen aan in een boomstructuur (Appearance, Program Group, Uninstall Settings, Restart
Manager, Compiler Settings, Code Signing, Compression, Disk Spanning, Log File, System
Requirements, en meer). Herbert zelf: "Er zijn er veel meer, misschien ook een aantal voor een
eigen tabblad, maar dat komt later allemaal wel." Dit tabblad is daarom bewust beperkt tot
precies de drie groepen die hij noemde; de rest volgt later, mogelijk als eigen tabbladen.

### Datamodel (`InstallerProject`)

Zeven nieuwe `bool`-eigenschappen, elk met een directe tegenhanger in een Inno Setup-richtlijn:

- **`CreateDesktopIcon`** (standaard `false`) — biedt een optionele taak "Maak een snelkoppeling
  op het bureaublad" aan. Komt overeen met de desktopicon-taak uit
  HNSoftwareInstallerFramework's `Shortcuts.iss` (`CreateDesktopIcon == "yes"`, `Flags:
  unchecked`): staat standaard uit, de eindgebruiker kiest het bewust aan op de
  Aanvullende-taken-pagina.
- **`CreateStartMenuIcon`** (standaard `true`) — of de generator een startmenu-snelkoppeling
  opneemt in de `[Icons]`-sectie. Geen tegenhanger van Inno Setup's `AllowNoIcons`-richtlijn: die
  voegt alleen een aanvinkvakje toe waarmee de eindgebruiker tijdens de installatie zelf van
  snelkoppelingen kan afzien, terwijl dit veld een bouwtijd-keuze is die bepaalt of de
  snelkoppeling-entry er is. `true` is zowel Inno Setup's eigen standaard (zonder
  `AllowNoIcons`) als wat `Base.iss` altijd doet.
- **`UsePreviousAppDir`**, **`UsePreviousGroup`**, **`UsePreviousSetupType`**,
  **`UsePreviousTasks`**, **`UsePreviousLanguage`** (allemaal standaard `true`) — "update
  capability": onthoudt bij een update over een bestaande installatie de eerder gekozen
  installatiemap/startmenugroep/installatietype/taken/taal, in plaats van die opnieuw te vragen.
  Komt rechtstreeks overeen met Inno Setup's gelijknamige `UsePrevious*`-richtlijnen, die ook
  zonder deze instelling al standaard "yes" zijn — deze velden maken die keuze alleen zichtbaar
  en per project aanpasbaar. `UsePreviousTasks` is in het bijzonder relevant voor de
  `CreateDesktopIcon`-taak hierboven: die blijft bij een update aangevinkt als de gebruiker hem
  eerder aanvinkte.

Geen van deze zeven velden heeft een `??=`-normalisatie nodig in `JsonInstallerProjectService`:
het zijn allemaal `bool`-waardetypes (geen referentietype dat expliciet JSON `null` kan zijn), en
een ontbrekende JSON-sleutel in een ouder projectbestand laat de property-initializer-
standaardwaarde gewoon staan — zelfde redenering als bij de meertalige-knopteksten-dictionaries
uit sectie 24. Bevestigd met een eigen test
(`LoadAsyncDefaultsOtherSettingsForOlderProjectFileWithoutThem`).

### UI

Derde tabblad in `ProjectSettingsWindow.xaml`, na Schermen en Talen: drie gegroepeerde secties
(Bureaublad-snelkoppeling, Startmenu, Update capability) met een korte toelichtende tekst per
sectie en gewone `CheckBox`-besturingselementen, in lijn met de rest van het venster. Geen eigen
sub-viewmodel nodig (in tegenstelling tot Schermen/Talen, sectie 21): dit zijn zeven simpele
onafhankelijke booleans, direct als `[ObservableProperty]` op `ProjectSettingsViewModel`, met
dezelfde `MarkDirty()`-koppeling als de bestaande tekstvelden.

### Build- en testresultaat

`dotnet build`: 0 waarschuwingen, 0 fouten. `dotnet test`: 23/23 geslaagd (22 bestaand + 1
nieuwe backward-compatibility-test; de bestaande round-trip-test is uitgebreid met deze zeven
velden, elk bewust op de tegenovergestelde waarde van hun standaardwaarde gezet).

### Backlog

- De overige Inno Script Studio-referentie-instellingen (Compiler Settings, Code Signing,
  Compression, Disk Spanning, Log File, System Requirements, wizardvenster-uiterlijk,
  Program-Group-naam/DisableProgramGroupPage-pagina, enzovoort) zijn nog niet gebouwd. Mogelijk
  eigen tabbladen, te plannen zodra Herbert dat oppakt.
- De generator (fase 5/6, nog niet gebouwd) moet deze zeven velden straks vertalen naar de
  juiste `[Setup]`-richtlijnen (de vijf `UsePrevious*`-richtlijnen) en de
  `[Tasks]`/`[Icons]`-secties voor de bureaublad- en startmenu-snelkoppeling, naar het patroon
  van HNSoftwareInstallerFramework's `Shortcuts.iss`/`Base.iss`. `CreateStartMenuIcon` wordt dus
  vertaald naar het al dan niet aanwezig zijn van een Start Menu-`[Icons]`-regel, niet naar
  `AllowNoIcons`.

## 26. Vijf nieuwe schermeditors: Info Before, User Info, Select Start Menu Folder, Ready to Install, Info After (2026-10-02)

Roadmapitem 4 (vervolg op fase 4): dedicated schermeditors voor de vijf resterende "eenvoudige"
wizardschermen die nog geen editor hadden, bepaald aan de hand van de Feature-Checklist
(`docs/Feature-Checklist.md`, sectie 1): elk scherm waarvoor categorie 2 van die checklist nog
onbeantwoorde (`?`) velden toonde, behalve Setup Completed (dat heeft voor nu geen nieuwe velden
nodig — zijn enige open punt, `AlwaysRestart`, hoort bij een toekomstig "Herstart en lopende
applicaties"-tabblad, niet bij dit scherm zelf). Herbert koos er bewust voor om alle vijf in één
branch/PR te bouwen in plaats van incrementeel.

### Volgorde in de schermeditor

De vijf nieuwe schermen zijn ingevoegd op hun eigen plek in Inno Setup's vaste paginavolgorde,
niet achteraan: Welkom → Licentie → **Info Before** → **User Info** → Installatiemap kiezen →
**Select Start Menu Folder** → **Klaar om te installeren** → **Info After**. Select Components en
Select Tasks ontbreken nog in deze volgorde (geen editor), dus Select Start Menu Folder sluit in
de lijst rechtstreeks aan op Installatiemap kiezen, en Klaar-om-te-installeren rechtstreeks op
Select Start Menu Folder — zie de toelichting in `WizardEditorViewModel`'s constructor. Dit is
belangrijk voor Terug/Volgende-navigatie binnen de schermeditor: die navigeert simpelweg door de
`_screens`-lijst in volgorde van toevoegen.

### Datamodel (`InstallerProject`)

Veertien nieuwe eigenschappen, elk met een directe tegenhanger in een Inno Setup-richtlijn (zie
categorie 2 van de Feature-Checklist):

- **`InfoBeforeFilePath`** / **`InfoAfterFilePath`** (`string`, standaard leeg) — pad naar het
  leesmij-/infobestand vóór/na de bestemmingspagina (`InfoBeforeFile`/`InfoAfterFile`). Zelfde
  patroon als `LicenseFilePath`: leeg totdat de gebruiker een bestand kiest.
- **`DefaultUserInfoName`**, **`DefaultUserInfoOrg`**, **`DefaultUserInfoSerial`** (`string`,
  standaard leeg) — vooringevulde velden op de User Info-pagina (`DefaultUserInfoName/Org/
  Serial`).
- **`UsePreviousUserInfo`** (`bool`, standaard `true`) — onthoudt bij een update de eerder
  ingevulde User Info-gegevens (`UsePreviousUserInfo`). Zelfde conventie als de vijf
  `UsePrevious*`-vlaggen uit sectie 25, maar hier bewust bij de User Info-velden zelf gehouden
  (niet toegevoegd aan het tabblad Overige instellingen): de gebruiker bewerkt alles over dit
  scherm op één plek, in de schermeditor.
- **`DefaultGroupName`** (`string`, standaard leeg) — voorgestelde startmenugroep
  (`DefaultGroupName`); leeg valt terug op `AppName`, net als `DefaultDirName`.
- **`AppendDefaultGroupName`** (`bool`, standaard `true` — zelfde standaard als Inno Setup zelf,
  geverifieerd via de officiële documentatie op 2026-10-02) — stuurt specifiek Inno Setup's eigen
  Bladeren-dialoog op de Select Start Menu Folder-pagina (een boomweergave van bestaande
  startmenu-mappen, niet het tekstveld zelf): kiest de gebruiker daar een bestaande map, dan plakt
  Setup bij `true` automatisch de laatste component van `DefaultGroupName` erachter; bij `false`
  gebruikt Setup precies de gekozen map en krijgt die Bladeren-dialoog zelf een "Nieuwe map
  maken"-knop. Gaat dus niet over het combineren van een getypte naam met de standaardnaam (de
  oorspronkelijke, te korte omschrijving in de Feature-Checklist suggereerde dat en klopte niet —
  zie Herberts vraag hierover, backlogitem hieronder).
- **`AlwaysUsePersonalGroup`** (`bool`, standaard `false`, zelfde standaard als Inno Setup zelf) —
  laat de `{group}`-constante altijd naar het persoonlijke startmenu van de huidige gebruiker
  wijzen, ook bij een "voor alle gebruikers"-installatie (die wijst anders naar het
  Alle-gebruikers-startmenu). Inno Setup's eigen documentatie waarschuwt dat dit "mogelijk niet
  het beoogde effect heeft" en de compiler geeft er een waarschuwing bij (tenzij
  `UsedUserAreasWarning` is uitgezet) — nog niet vertaald naar een eigen waarschuwing in deze IDE,
  generatorwerk voor fase 5/6.
- **`DisableReadyMemo`**, **`AlwaysShowDirOnReadyPage`**, **`AlwaysShowGroupOnReadyPage`**
  (`bool`, standaard `false`) — bepalen wat de samenvatting op de Klaar-om-te-installeren-pagina
  toont (`DisableReadyMemo`/`AlwaysShowDirOnReadyPage`/`AlwaysShowGroupOnReadyPage`).

Plus vijf nieuwe `WizardScreenButtonSettings`-eigenschappen (`InfoBeforeScreenButtons`,
`UserInfoScreenButtons`, `SelectProgramGroupScreenButtons`, `ReadyScreenButtons`,
`InfoAfterScreenButtons`), zelfde patroon als `WelcomeScreenButtons` — elk met een `??=`-
normalisatie in `JsonInstallerProjectService` voor een expliciete JSON-`null`. De veertien
scalaire velden hierboven hebben geen normalisatie nodig: `string`-velden hebben een
niet-`null`-initializer en `bool`-velden kunnen sowieso geen JSON-`null` zijn, zelfde redenering
als sectie 25.

### ViewModels en voorvertoning

Vijf nieuwe `WizardScreenEditorViewModel`-subklassen in
`InnoSetupStudio.App.ViewModels.Screens`, elk naar het dichtstbijzijnde bestaande patroon:

- **`InfoBeforePageEditorViewModel`** / **`InfoAfterPageEditorViewModel`** — kopie van
  `LicensePageEditorViewModel`'s bestandskeuze-patroon (inclusief de `IsUncOrDevicePath`-
  beveiliging tegen automatische SMB-toegang vanuit een geladen projectbestand), zonder de
  "akkoord"-keuzerondjes van de licentiepagina.
- **`UserInfoPageEditorViewModel`**, **`SelectProgramGroupPageEditorViewModel`**,
  **`ReadyPageEditorViewModel`** — kopie van `WelcomePageEditorViewModel`'s eenvoudige patroon:
  alleen `[ObservableProperty]`-velden, geen bestandsdialoog of eigen knop.

Vijf nieuwe voorvertoning-`UserControl`s in `InnoSetupStudio.Wizard.Screens`
(`InfoBeforePagePreview`, `UserInfoPagePreview`, `SelectProgramGroupPagePreview`,
`ReadyPagePreview`, `InfoAfterPagePreview`), met Inno Setup's eigen (vaste, niet-thema-
afhankelijke) Engelstalige paginateksten, zelfde aanpak als de drie bestaande voorvertoningen.
De Klaar-om-te-installeren-voorvertoning toont een illustratieve, statische samenvattingstekst
(geen live gegenereerde tekst — dat is generatorwerk, fase 5/6) puur om te laten zien dat
`DisableReadyMemo` het hele memo-vak verbergt (via een `DataTrigger` op `Border.Visibility`, geen
nieuwe inverse-boolean-converter nodig). `AppendDefaultGroupName`/`AlwaysUsePersonalGroup`/
`AlwaysShowDirOnReadyPage`/`AlwaysShowGroupOnReadyPage` hebben geen eigen zichtbaar element in de
voorvertoning: het zijn bouwtijd-gedragskeuzes zonder een eigen aanvinkvakje op de echte
Inno Setup-pagina.

`ScreenEditorControl.xaml` kreeg vijf nieuwe keyless preview-`DataTemplate`s en vijf nieuwe
`x:Key`'d `PropertyPanelTemplate`s (instellingenpaneel rechts), en `PropertyPanelTemplateSelector`
vijf nieuwe `DataTemplate`-eigenschappen — exact dezelfde twee-lagen-aanpak (keyless voor de
voorvertoning, `x:Key` + expliciete selector voor het instellingenpaneel) als de drie bestaande
schermen, zie sectie 21.

### Wiring (`WizardEditorViewModel`)

Vijf nieuwe conditionele `_screens.Add(...)`-blokken in de constructor (op hun plek in Inno
Setup's volgorde, zie hierboven) en vijf nieuwe `case`-blokken in `ApplyTo`, naar het bestaande
patroon van Welkom/Licentie/Installatiemap kiezen.

### Lokalisatie

Zestien nieuwe resourcesleutels (NL/EN/DE) voor labels, dialoogfilters en toelichtende teksten
van de vijf nieuwe instellingenpanelen. De vijf schermnamen zelf (`WizardScreenInfoBefore`,
`WizardScreenUserInfo`, `WizardScreenSelectProgramGroup`, `WizardScreenReady`,
`WizardScreenInfoAfter`) bestonden al (fase 3, wizardschermen-overzicht) en zijn hergebruikt.
`ScreenEditorNoScreens` (de toelichting die verschijnt als geen van de door de schermeditor
ondersteunde schermen aan staat) is bijgewerkt om alle acht nu ondersteunde schermen te noemen in
plaats van alleen de oorspronkelijke drie.

### Build- en testresultaat

`dotnet build`: 0 waarschuwingen, 0 fouten. `dotnet test`: 24/24 geslaagd (23 bestaand + 1 nieuwe
backward-compatibility-test voor de veertien nieuwe scalaire velden; de bestaande round-trip-test
is uitgebreid met alle veertien velden plus de vijf nieuwe knopinstellingen, en de bestaande
explicite-JSON-`null`-test voor knopinstellingen is uitgebreid met de vijf nieuwe
`WizardScreenButtonSettings`-eigenschappen).

### Backlog

- Select Components en Select Tasks (roadmapitem 5) hebben nog geen editor; volgen in een latere
  PR.
- De generator (fase 5/6, nog niet gebouwd) moet deze veertien velden vertalen naar de
  bijbehorende `[Setup]`-richtlijnen.
- Setup Completed/Finished-scherm zijn enige openstaande veld, `AlwaysRestart`, hoort bij een
  toekomstig "Herstart en lopende applicaties"-tabblad (categorie 20 van de Feature-Checklist),
  niet bij dit scherm zelf.


### Correcties na Herberts handmatige UI-test (2026-10-02)

Herbert testte de vijf nieuwe schermen in de UI en meldde vier punten. Twee waren concrete fouten
in deze eerste versie, inmiddels gefixt op dezelfde branch:

- **Layoutfout op Select Start Menu Folder**: het invoerveld en de Bladeren-knop waren veel te
  hoog. Oorzaak: `SelectProgramGroupPagePreview.xaml`'s buitenste `DockPanel` had geen
  `LastChildFill="False"` staan, waardoor WPF het laatste kind (de map-rij) liet uitrekken over
  alle resterende ruimte in plaats van zijn eigen `Dock="Top"`-hoogte aan te houden —
  Installatiemap kiezen viel dit niet op omdat diens laatste kind toevallig een korte TextBlock
  is. Gefixt door `LastChildFill="False"` toe te voegen, net als bij Installatiemap kiezen. Build
  opnieuw gecontroleerd: 0 waarschuwingen, 0 fouten.
- **`AppendDefaultGroupName` had de verkeerde standaardwaarde**: stond in de code op `false`,
  terwijl Inno Setup's eigen documentatie (jrsoftware.org/ishelp, geverifieerd 2026-10-02)
  "Default value: yes" vermeldt. Gefixt naar `true`, inclusief de bijbehorende round-trip- en
  backward-compatibility-tests hierboven in dit document.

De overige twee punten van Herbert vragen om echt nieuw ontwerpwerk, nog niet gebouwd:

- **User Info-scherm voelt aan als invullen in plaats van bewerken**: de voorvertoning toont de
  waarde van `DefaultUserInfoName`/`Org`/`Serial` live in het invoerveld zelf, zonder dat de
  veldlabels ("Full Name", "Organization", "Serial Number") apart aan te passen zijn. Herbert wil
  per veld kunnen aan/uitvinken of het getoond wordt, en mogelijk de labels zelf (meertalig)
  kunnen aanpassen. Uitgezocht via de officiële documentatie: Full Name/Organization hebben geen
  eigen aan/uitvinkvakje in Inno Setup zonder Pascal Script; het Serial Number-veld wordt alleen
  getoond als het script een `CheckSerial`-event-functie bevat (fase 6-werk, nog niet gebouwd). De
  labels zelf komen uit Inno Setup's eigen, per taal overschrijfbare `[CustomMessages]`-sleutels
  (`UserInfoName`/`UserInfoOrg`/`UserInfoSerial`), dus een meertalige-captioneditor zou dezelfde
  `Dictionary<string,string> XxxCaptionByLanguage`-aanpak kunnen volgen als de bestaande
  knoplabels (sectie 20/24).
- **Bladeren-knop op Select Start Menu Folder nog niet volledig aanpasbaar**: Herbert wil daar
  dezelfde volledige aanpasbaarheid (caption/enabled/visible/tekstkleur/lettertype/tooltip, plus
  per-taal-varianten) als bij Installatiemap kiezen (`SelectDestinationPageEditorViewModel`). Nog
  niet gebouwd.
- **Verwarring over `AppendDefaultGroupName`**: Herbert vroeg zich af wat dit vinkje eigenlijk
  doet, en verwachtte eerder een optie om aan te geven of de gebruiker de voorgestelde map mag
  wijzigen. Dat laatste bestaat al wel in Inno Setup, als `DisableProgramGroupPage` — de directe
  tegenhanger van `AllowUserToChangeDir`/`DisableDirPage` bij Installatiemap kiezen — maar is nog
  niet als project-veld gebouwd. Voorstel: een nieuwe `AllowUserToChangeGroup`-eigenschap
  toevoegen die dit omkeert, naar exact hetzelfde patroon als `AllowUserToChangeDir`.

Scope en volgorde van deze drie laatste punten zijn met Herbert afgestemd voordat ze gebouwd
worden.


## 27. Drie-waardige paginazichtbaarheid: "Auto" slaat de pagina over bij een bekende update (2026-10-02)

### Aanleiding en verificatie

Herbert vroeg om een derde optie naast "altijd tonen"/"nooit tonen" voor zowel Installatiemap
kiezen als Select Start Menu Folder: bij een update van een al geïnstalleerde applicatie moet
Setup de pagina automatisch overslaan wanneer de map/groep al uit een eerdere installatie bekend
is (via het register). Herbert noemde hiervoor aanvankelijk `AlwaysShowDirOnReadyPage=auto`/
`AlwaysShowGroupOnReadyPage=auto` — die twee bestaande velden (sectie 25) ondersteunen echter
alleen `yes`/`no` en gaan over een heel ander scherm (de samenvatting op de Ready-pagina, niet
Installatiemap kiezen/Select Start Menu Folder zelf). Uitgezocht via de officiële Inno
Setup-documentatie (jrsoftware.org/ishelp, 2026-10-02) en onafhankelijk bevestigd door Herberts
eigen documentatiecitaat: de juiste richtlijnen zijn `DisableDirPage`/`DisableProgramGroupPage`,
allebei met drie waarden (`no`/`yes`/`auto`), die in Inno Setup allebei standaard op `auto`
staan (zie "Correcties" onderaan sectie 28: hier stond eerder ten onrechte dat `DisableDirPage`
standaard `no` is). Dit lost tegelijk het laatste backlogpunt
van sectie 26 op ("Verwarring over `AppendDefaultGroupName`"): in plaats van het voorgestelde
`AllowUserToChangeGroup`-veld is het nu `GroupPageMode` geworden, naar hetzelfde patroon als
Installatiemap kiezen.

### Datamodel: nieuwe `DisablePageMode`-enum, niet twee losse bool's

Nieuw bestand `DisablePageMode.cs` (`InnoSetupStudio.Core.Project`, zelfde één-bestand-per-type-
conventie als de rest van die map): een enum met drie waarden, `AlwaysShow`/`NeverShow`/
`AutoSkipIfKnown`, die rechtstreeks overeenkomen met Inno Setup's `no`/`yes`/`auto`. Eén
gedeelde enum voor beide pagina's (in plaats van twee aparte types) omdat de drie waarden en hun
betekenis identiek zijn; alleen de **standaardwaarde** verschilt per gebruiksplek.

`InstallerProject` kreeg:

- `DirPageMode` (vervangt de oude `AllowUserToChangeDir`-bool), standaard `AlwaysShow` —
  bewust gekozen om het gedrag van de oude `AllowUserToChangeDir=true` te behouden. Dit wijkt af
  van Inno Setup's eigen standaard (`auto`).
- `GroupPageMode` (nieuw veld, geen eerdere bool-tegenhanger), standaard `AutoSkipIfKnown` —
  overeenkomstig `DisableProgramGroupPage`'s eigen standaard (`auto`).

Beide zijn `[JsonConverter(typeof(DisablePageModeJsonConverter))]`. `DirPageMode` draagt
daarnaast `[JsonPropertyName("AllowUserToChangeDir")]`: de eigenschap heet in C# nu anders (het
is geen simpel vinkje meer), maar bewaart bewust de oude JSON-sleutel, zodat een bestaand
.issproj-bestand zonder enige migratiecode blijft laden.

### Backward-compatible migratie via een eigen `JsonConverter`

Nieuw bestand `DisablePageModeJsonConverter.cs`: een `JsonConverter<DisablePageMode>` die bij het
lezen drie gevallen onderscheidt — een JSON-`true`/`false` (het oude bool-formaat, voor
`DirPageMode`'s `AllowUserToChangeDir`-sleutel: `true`→`AlwaysShow`, `false`→`NeverShow`, nooit
automatisch `AutoSkipIfKnown`, want die waarde bestond in het oude model niet), een tekstwaarde
die met `Enum.TryParse` naar een van de drie enum-namen wordt omgezet (het nieuwe formaat), of
anders een `JsonException` (bewust hard falen in plaats van stilzwijgend een gok te doen bij een
echt onverwachte waarde). Schrijven gebeurt altijd als tekstwaarde. Dit is dezelfde
"doorgeef + converter doet het vertaalwerk"-aanpak als elders in dit project, maar dan voor een
type-wijziging (bool → enum) in plaats van een simpele `??=`-normalisatie zoals bij
WizardScreens/SupportedLanguageIds (sectie 14).

Nieuwe tests in `InstallerProjectTests.cs`: een parametrische test
(`LoadAsyncMigratesLegacyBooleanAllowUserToChangeDirToDirPageMode`) voor beide boolean-waarden,
plus uitgebreide assertions in de bestaande round-trip- en ouder-projectbestand-tests voor de
tekstwaarde-vorm en de twee verschillende standaardwaarden.

### UI: één dropdown per pagina, Herberts expliciete keuze

Gevraagd via `AskUserQuestion` hoe de derde toestand in de UI te tonen (een los vinkje naast de
bestaande twee, of één keuzelijst met drie opties); Herbert koos de keuzelijst. Het bestaande
vinkje "Gebruiker mag map wijzigen" op Installatiemap kiezen is vervangen door een `ComboBox`
(`ScreenEditorControl.xaml`, `SelectDestinationPropertyPanelTemplate`); Select Start Menu Folder
kreeg een nieuwe, identiek opgebouwde `ComboBox` (`SelectProgramGroupPropertyPanelTemplate`, had
voorheen geen enkele zichtbaarheidsinstelling). Beide gebruiken `SelectedValuePath="Tag"` met
`ComboBoxItem Tag="AlwaysShow"` etc.: WPF's ingebouwde `EnumConverter` zet de Tag-tekst vanzelf om
naar de `DisablePageMode`-eigenschap, dus geen eigen `IValueConverter` nodig. Onder elke keuzelijst
staat een toelichtende hint-tekst die wisselt met de gekozen waarde (`IsNeverShowHintVisible`/
`IsAutoSkipHintVisible` op `SelectProgramGroupPageEditorViewModel`, `ChangeDirHintVisibility`/
`IsAutoSkipHintVisible` op `SelectDestinationPageEditorViewModel` — de eerste twee namen zijn
bewust verschillend gebleven, `ChangeDirHintVisibility` bestond al vóór deze feature).

**Voorvertoning bij "Auto"**: Inno Setup's auto-overslaan-gedrag hangt af van het register op het
moment van installeren, iets wat de ontwerptijd-voorvertoning niet kan nabootsen. Gekozen
conventie: `AutoSkipIfKnown` wordt in de voorvertoning behandeld als een eerste installatie (het
scherm blijft dus net als bij `AlwaysShow` bewerkbaar/zichtbaar), met alleen een aparte,
onderscheidende hint-tekst erbij (letterlijk Engels, net als de bestaande
`ChangeDirHintVisibility`-hint in `SelectDestinationPagePreview.xaml` — deze voorvertoning
simuleert Inno Setup's eigen paginatekst, niet de taal van de editor-UI zelf, dus geen
resx-binding voor deze twee specifieke `TextBlock`-elementen in de voorvertoningen).

### Lokalisatie

Nieuwe resx-sleutels (NL/EN/DE, alle drie bijgewerkt): `LabelDirPageMode`/`LabelGroupPageMode`
(de koptekst boven elke keuzelijst), de gedeelde `LabelDisablePageModeAlwaysShow`/
`LabelDisablePageModeNeverShow`/`LabelDisablePageModeAuto` (de drie keuzelijst-opties, hergebruikt
op beide pagina's omdat de tekst identiek is), en de vier schermspecifieke hint-teksten
`HintDirPageModeNeverShow`/`HintDirPageModeAuto`/`HintGroupPageModeNeverShow`/
`HintGroupPageModeAuto`. De oude `LabelAllowUserToChangeDir`-sleutel is verwijderd uit alle drie
de resx-bestanden (nergens meer naar verwezen). XML-validatie van alle drie de resx-bestanden na
bewerking: geen fouten, geen dubbele sleutels, gelijk aantal items in elk bestand.

### Wiring (`WizardEditorViewModel`, `ProjectSettingsViewModel`)

`WizardEditorViewModel`'s constructor en `ApplyTo`-switch zijn bijgewerkt voor beide
eigenschappen (`DirPageMode` i.p.v. `AllowUserToChangeDir`, `GroupPageMode` nieuw). Daarnaast is
`ProjectSettingsViewModel`'s bestaande doorgeef-patroon (zie sectie 16/26: velden die elders — in
de schermeditor — bewerkt worden, moeten hier ongewijzigd worden meegenomen bij Opslaan, anders
zet een algemene naam-/padwijziging ze stilzwijgend terug) aangevuld: het bestaande
`_allowUserToChangeDir`-veld is omgezet naar `_dirPageMode` (`DisablePageMode`), en er is een
nieuw `_groupPageMode`-doorgeefveld toegevoegd — zonder dat laatste zou Opslaan vanuit het
algemene projectinstellingenscherm de in de schermeditor gekozen Start Menu-paginazichtbaarheid
stilzwijgend hebben teruggezet naar de standaardwaarde, exact dezelfde bugklasse als CodeRabbit's
bevinding in sectie 26.

### Build- en testresultaat

`dotnet build`: 0 waarschuwingen, 0 fouten. `dotnet test`: 26/26 geslaagd (24 bestaand + 2 nieuwe
parametrische gevallen voor de boolean-naar-enum-migratie).

### Backlog

- De drie overige, nog niet gebouwde punten uit sectie 26 (User Info per-veld/meertalige
  captions, volledige Bladeren-knop-aanpasbaarheid op Select Start Menu Folder) blijven open.
- Nog te testen door Herbert in de UI: de keuzelijst zelf (alle drie waarden selecteerbaar,
  hint-tekst wisselt correct), en dat een bestaand project met het oude boolean-veld nog gewoon
  opent.

## 28. Bladeren-knop op Select Start Menu Folder volledig aanpasbaar (2026-10-02)

### Aanleiding

Herbert testte de vijf nieuwe schermeditors uit sectie 26/27 in de UI en bevestigde dat de rest
van de functionaliteit goed werkt, op één concreet gemis na: de Bladeren-knop op Select Start Menu
Folder kon nog niet bewerkt worden zoals de Bladeren-knop op Installatiemap kiezen (caption/
enabled/visible/tekstkleur/lettertype/vet/tooltip, plus per-taal-varianten via
`ButtonPropertiesWindow`). Dit was het nog openstaande backlogpunt uit sectie 26 ("Bladeren-knop
op Select Start Menu Folder nog niet volledig aanpasbaar") en wordt hiermee opgelost.

**Verificatie.** Voordat dit gebouwd werd, is tegen Inno Setup's eigen broncode gecontroleerd
(`Setup.WizardForm.pas`, `jrsoftware/issrc` op GitHub) dat deze knop ook daadwerkelijk bestaat als
een losse `TNewButton`, net als `DirBrowseButton` op Installatiemap kiezen:
`FGroupBrowseButton: TNewButton` / `property GroupBrowseButton: TNewButton read FGroupBrowseButton;`
op `TWizardForm`. Dat bevestigt dat deze knop een legitiem, bestaand bewerkingsdoel is en niet een
verzonnen eigenschap.

### Datamodel: hergebruik van `BrowseButtonSettings`, geen nieuwe klasse

Nieuwe eigenschap `InstallerProject.SelectProgramGroupBrowseButton` (`BrowseButtonSettings`,
standaard `new()`), rechtstreeks naast `GroupPageMode` geplaatst. Hergebruikt bewust hetzelfde
`BrowseButtonSettings`-model als `SelectDestinationBrowseButton` in plaats van een tweede, bijna
identieke klasse: de velden en hun betekenis (leeg/null laat Inno Setup's eigen standaardgedrag
intact, geen drielaagse Effective*-cascade via het Standaardscherm omdat deze knop maar op één
scherm voorkomt) zijn voor beide knoppen exact hetzelfde. `BrowseButtonSettings`'s klasse-
doccomment is bijgewerkt om dit gedeelde gebruik te weerspiegelen.

### ViewModel: exacte mirror van `SelectDestinationPageEditorViewModel`

`SelectProgramGroupPageEditorViewModel` kreeg dezelfde Bladerknop-laag als
`SelectDestinationPageEditorViewModel` al had: een zesde constructorparameter
(`BrowseButtonSettings browseButtonSettings`), de acht scalaire velden
(`BrowseButtonCaption`/`Enabled`/`Visible`/`TextColor`/`FontFamily`/`FontSize`/`FontBold`/
`Tooltip`) plus de twee per-taal-dictionaries, `PickBrowseButtonTextColor`,
`DefaultBrowseButtonCaption`/`EffectiveBrowseButtonCaption`, `IsBrowseButtonVisible`/
`IsBrowseButtonEnabled`, en `ReadBrowseButtonSettings()`. Enige inhoudelijke verschil met de
Bestemmingspagina: `IsBrowseButtonEnabledInPreview` combineert hier Inno Setup's eigen ingebouwde
gedrag met `GroupPageMode != DisablePageMode.NeverShow` in plaats van `DirPageMode` — elk scherm
gebruikt zijn eigen paginazichtbaarheidsveld (sectie 27) om te bepalen of de knop in de
voorvertoning ook daadwerkelijk bewerkbaar lijkt.

### Wiring, UI en voorvertoning

`WizardEditorViewModel`'s constructor geeft `project.SelectProgramGroupBrowseButton` nu als zesde
argument mee, en de `ApplyTo`-switch schrijft `programGroup.ReadBrowseButtonSettings()` terug —
zelfde patroon als bij `SelectDestinationPageEditorViewModel`. `ScreenEditorControl.xaml` kreeg
een nieuwe "Bladerknop"-sectie onderaan `SelectProgramGroupPropertyPanelTemplate` (tekstveld +
eigenschappenknopje), met een eigen, kleine `ProgramGroupBrowseButtonProperties_Click`-handler en
`BuildForProgramGroupBrowseButton`-fabrieksmethode in `ScreenEditorControl.xaml.cs` — exacte
mirrors van `BrowseButtonProperties_Click`/`BuildForBrowseButton`, maar getypeerd op
`SelectProgramGroupPageEditorViewModel`. `SelectProgramGroupPagePreview.xaml`'s Bladeren-knop was
voorheen volledig decoratief (`Content="Browse..."`, geen enkele binding); deze is nu volledig
gebonden (Content/IsEnabled/Visibility/Foreground/FontFamily/FontSize/FontWeight/ToolTip), met
dezelfde vier converters als `SelectDestinationPagePreview.xaml` toegevoegd aan
`UserControl.Resources` — en blijft, net als die andere voorvertoning, niet-interactief
(`IsHitTestVisible="False" Focusable="False" IsTabStop="False"`: bewerken gebeurt in het
instellingenpaneel, niet in de voorvertoning zelf).

Geen nieuwe resx-sleutels nodig: `SectionBrowseButton` en `ButtonWizardBrowse` waren al generiek/
gedeeld tussen beide schermen.

### Doorgeefveld in `ProjectSettingsViewModel`

Zelfde reden als bij elk eerder doorgeefveld in dit document (secties 16/26/27): een veld dat
alleen in de schermeditor bewerkt wordt, moet bij het opslaan vanuit het algemene
Projectinstellingenscherm ongewijzigd worden teruggeschreven, anders zet een gewone naam-/
padwijziging het stilzwijgend terug naar de standaardwaarde. Nieuw doorgeefveld
`_selectProgramGroupBrowseButton`, naast het bestaande `_selectDestinationBrowseButton`.

### Testdekking

`InstallerProjectTests.JsonInstallerProjectServiceRoundTripsAllFields` zet
`SelectProgramGroupBrowseButton` nu op niet-standaardwaarden voor alle acht velden plus beide
per-taal-dictionaries, en controleert de volledige round-trip na opslaan/laden — dezelfde
dekkingsgraad die `SelectDestinationBrowseButton` zelf tot nu toe niet had (dat blijft zo; deze
sessie breidde alleen de nieuwe eigenschap uit). De bestaande
`LoadAsyncDefaultsLanguageOverrideDictionariesForOlderProjectFileWithoutThem`-test kreeg twee
extra assertions voor `SelectProgramGroupBrowseButton`'s per-taal-dictionaries, naast de al
bestaande assertions voor `SelectDestinationBrowseButton`.

### Build- en testresultaat

`dotnet build`: 0 waarschuwingen, 0 fouten. `dotnet test`: 26/26 geslaagd (geen nieuwe
testmethoden, wel uitgebreide assertions in twee bestaande tests).

### Backlog

- De twee overige, nog niet gebouwde punten uit sectie 26 (User Info per-veld/meertalige
  captions) blijven open.
- Nog te testen door Herbert in de UI, samen met de rest van PR #23: de nieuwe
  eigenschappenknop bij de Start Menu-Bladerknop (caption/kleur/lettertype/tooltip/per-taal), en
  dat de knop in de voorvertoning uitgeschakeld raakt zodra `GroupPageMode` op "Nooit tonen"
  staat.

### Correcties n.a.v. CodeRabbit-review van 2026-10-02 (verwerkt op 2026-10-05)

Alle vijf bevindingen zijn gecontroleerd tegen de officiële Inno Setup-documentatie
(jrsoftware.org/ishelp) en bleken terecht:

- **Standaardwaarde `DisableDirPage` was fout beschreven.** Sectie 27 en de code-commentaren zeiden
  dat Inno Setup's standaard `no` is. De documentatie noemt voor `DisableDirPage` en
  `DisableProgramGroupPage` allebei `auto` als standaard. Dit was mijn fout. Het model verandert
  niet: `DirPageMode` blijft bewust standaard `AlwaysShow`, zodat oude projecten met
  `AllowUserToChangeDir=true` zich hetzelfde blijven gedragen. Alleen de toelichting in
  `DisablePageMode.cs`, `InstallerProject.cs` en sectie 27 is aangepast.
- **Voorwaarde van `auto`.** Setup slaat de pagina over wanneer dezelfde applicatie al
  geïnstalleerd is (volgens het register), niet wanneer de map of groep "bekend" is. De hints in de
  drie resx-bestanden en in beide voorvertoningen zeggen dat nu zo.
- **`{code:FunctieNaam|...}`.** De tekst na de `|` is de parameter voor de Pascal-functie en geen
  terugvalwaarde. Gecorrigeerd in `Inno-Setup-Constants.md`, inclusief de algemene uitleg van `|`.
- **`DisablePageModeJsonConverter`.** Numerieke tekst zoals `"999"` werd door `Enum.TryParse`
  geaccepteerd en als ongedefinieerde waarde teruggeschreven. De converter accepteert nu alleen
  gedefinieerde enumnamen. Nieuwe test: `LoadAsyncRejectsNumericTextForDisablePageMode`.

Daarnaast leverde de review van de correctiecommit (2026-10-05) nog één bevinding op, ook terecht:
een expliciete JSON-`null` voor `SelectProgramGroupBrowseButton` gaf een `NullReferenceException`
zodra de schermeditor `Caption` las. `JsonInstallerProjectService.LoadAsync` normaliseert deze
eigenschap nu naar `new()`, net als de schermknoppen. Hetzelfde gat zat al in
`SelectDestinationBrowseButton`; die is in dezelfde stap meegenomen. Nieuwe test:
`LoadAsyncNormalizesExplicitNullBrowseButtonSettings`.


## 29. Projectvelden voor de generator: hoofdprogramma, bestandsnaam, architectuur, wizardstijl (2026-10-05)

Stap 1 van `docs/Ontwerp-Dunne-Generator.md`. De generator bestaat nog niet; dit zijn de velden die
zij nodig heeft en die het projectmodel miste. Alle vier staan in Projectinstellingen, tabblad
Algemeen, onder de nieuwe kop "Installer".

| Veld | Model | Standaard | Inno Setup |
|---|---|---|---|
| Hoofdprogramma | `InstallerProject.MainExecutable` | leeg | `[Icons]` en later `[Run]` |
| Bestandsnaam installer | `InstallerProject.OutputBaseFilename` | leeg, dus `<AppName>-<AppVersion>-Setup` | `OutputBaseFilename` |
| Architectuur | `InstallerProject.Architecture` (`InstallerArchitecture`) | `X64` | `ArchitecturesInstallIn64BitMode` |
| Wizardstijl | `InstallerProject.WizardStyle` (`InstallerWizardStyle`) | `Modern` | `WizardStyle` |

**Keuzes en gedrag.**

- `MainExecutable` is een pad relatief aan `SourceFilesPath`. De Bladeren-knop opent een
  bestandsdialoog in die map en slaat het relatieve pad op. Een bestand buiten de map wordt
  geweigerd met een melding, omdat de installer alleen die map meeneemt. Is er nog geen map met
  bronbestanden gekozen, dan vraagt de knop daar eerst om. Bevat de gekozen map (ook in
  submappen) geen enkel `.exe`-bestand, dan meldt de knop dat ook meteen en opent de
  bestandsdialoog niet (aangescherpt na Herberts handmatige test, 2026-10-05: eerst kon hij in een
  map zonder programma alsnog een bestand zoeken). Een hoofdprogramma zonder `.exe`-extensie kan de
  gebruiker rechtstreeks in het tekstveld typen.
- Het tekstveld Hoofdprogramma kan ook met de hand gevuld worden. `Opslaan` controleert daarom de
  vorm met `InstallerProject.IsValidMainExecutablePath`: relatief, zonder `.`, `..`, lege onderdelen
  of ongeldige tekens (CodeRabbit, PR #26). Of het bestand ook in de huidige bronbestandenmap
  staat, controleert dit scherm bewust niet, want dat hangt af van de volgorde waarin de velden
  worden ingevuld; dat is een waarschuwing van de generator. Hetzelfde geldt voor een
  bronbestandenmap die niet bestaat: het scherm controleert die niet (Herbert: netter, maar geen
  must), de generator meldt het.
- `GetEffectiveOutputBaseFilename` vermijdt ook gereserveerde Windows-apparaatnamen (`CON`,
  `PRN`, `AUX`, `NUL`, `COM1` tot `COM9`, `LPT1` tot `LPT9`, ook met extensie) door er een
  onderstrepingsteken voor te zetten, en laat een punt of spatie aan het einde weg (CodeRabbit,
  PR #26).
- `GetEffectiveOutputBaseFilename()` staat in Core, niet in het ViewModel, zodat de generator
  dezelfde regel gebruikt als de uitleg onder het veld. Tekens die Windows niet toestaat in een
  bestandsnaam worden `_`. Lege naam en lege versie vallen terug op `Setup` of `<AppName>-Setup`.
- De standaard voor `WizardStyle` is `Modern`, terwijl Inno Setup zelf `classic` als standaard heeft
  (geverifieerd in de officiële documentatie). Dat is bewust, zoals het
  HNSoftwareInstallerFramework. De generator schrijft de waarde dus altijd expliciet.
- Bestaande projectbestanden zonder deze sleutels krijgen `X64` en `Modern`. Dat verandert niets
  aan bestaande installers, want er is nog geen generator.
- De twee enums gebruiken `StrictEnumJsonConverter<T>`: alleen echte enumnamen als tekst. Getallen,
  numerieke tekst zoals `"999"`, onbekende namen en `null` geven een laadfout, dezelfde
  strengheid als bij `DisablePageMode`. Expliciete JSON-`null` voor de twee tekstvelden wordt bij
  het laden naar een lege tekst genormaliseerd.
- Dit zijn de eerste velden in `ProjectSettingsViewModel` die niet door een schermeditor worden
  bewerkt maar in dit scherm zelf. Ze hebben daarom geen pass-through nodig.

**Tests.** Nieuwe tests (68 in totaal): round trip van de vier velden, standaardwaarden voor een
ouder bestand, normalisatie van `null`, zes ongeldige JSON-varianten, negentien gevallen voor
`GetEffectiveOutputBaseFilename` en dertien voor `IsValidMainExecutablePath`. De 3 resx-bestanden hebben nu elk 168 sleutels (voorheen 151),
gevalideerd met een ElementTree-script op identieke sleutelverzameling en `{0}`-plaatsaanduidingen.

**Wat de generator hiermee doet** (gebouwd in stap 2, zie sectie 30): `Architecture.X64` wordt
`ArchitecturesInstallIn64BitMode=x64compatible`, `X86` laat de richtlijn weg.

## 30. De dunne generator: IssGenerator in Core (2026-10-05)

Stap 2 van `docs/Ontwerp-Dunne-Generator.md`. Een pure functie zonder UI: `new IssGenerator().Generate(project)`
geeft een `GenerationResult` met de scripttekst en een lijst meldingen. De app roept hem nog niet aan
(stap 3). De prioriteit van versie 1 was dat de Inno Setup-compiler het resultaat accepteert; de
ISCC-tests bewijzen dat tegen Inno Setup 7.1.0.

**Bestanden in `InnoSetupStudio.Core/Generation/`.**

| Bestand | Rol |
|---|---|
| `IssGenerator.cs` | De generator. `Generate` is deterministisch en heeft geen toestand; per aanroep draait een eigen `Run`. |
| `IssWriter.cs` | Bouwt het script: CRLF, een lege regel voor elke sectie, kopjes (`Heading`) die alleen verschijnen als er een richtlijn onder komt. |
| `IssEscape.cs` | `Constants` (`{` wordt `{{`), `Quoted` (`"` wordt `""`), `ContainsLineBreak` en `FileSystemName` (tekens die Windows niet toestaat worden `_`, uit een vaste lijst en dus op elk besturingssysteem gelijk; blijft er niets over, dan is het resultaat `App`). |
| `GenerationIssue.cs` | `GenerationSeverity` (Info, Warning, Error), `GenerationIssueCode` en het record `GenerationIssue(Severity, Code, Arguments)`. |
| `GenerationResult.cs` | `Script`, `Issues` en `HasErrors`. |
| `IGeneratorEnvironment.cs` | `DirectoryExists` en `FileExists`, met `DiskGeneratorEnvironment`. Alleen hier raakt de generator de schijf, zodat tests een nepomgeving gebruiken. |

`InnoLanguageOption` heeft er een veld bij gekregen: `MessagesFile` (`compiler:Default.isl` voor Engels,
`compiler:Languages\<Naam>.isl` voor de rest, met de hoofdletters van de echte bestandsnaam).

**Wat er in het script komt.** Volgorde: `[Setup]`, `[Languages]`, `[Tasks]`, `[Files]`, `[Icons]` (sinds stap 4 met `[CustomMessages]` na `[Languages]` en `[Code]` als laatste, zie sectie 32). In
`[Setup]` staan alleen richtlijnen die van Inno Setup's eigen standaard afwijken, behalve de basisgegevens.
De mapping per model-eigenschap staat in sectie 3 van het ontwerp. Engels staat altijd als eerste in
`[Languages]` (Setup valt daarop terug); de andere gekozen talen volgen in de volgorde van de catalogus,
niet in de volgorde waarin ze in het project staan, zodat de uitvoer stabiel is.

**Escaping, per richtlijn.** Dit is het belangrijkste wat de ISCC-test heeft opgeleverd. Waarden die Setup
tijdens de installatie als constante-tekst leest (`AppId`, `AppName`, `AppVersion`, `AppPublisher`, de
URL's, `AppContact`, `DefaultUserInfo*`, `UninstallDisplay*`, de namen en doelen in `[Icons]`) krijgen
`{{` voor een `{`. Waarden die de compiler zelf leest (`Source`, `OutputDir`, `OutputBaseFilename`, de
bestandsrichtlijnen) krijgen dat niet: daar is `{{` letterlijke tekst. De documentatie bevestigt voor
de eerste groep dat de waarde constanten kan bevatten; voor de tweede groep is dit door ISCC zelf
aangetoond (zie sectie 12 van het ontwerp).

**Meldingen.** Codes en ernst staan in `GenerationIssueCode`; de tekst komt in stap 3 uit de resx-bestanden
(NL, EN, DE), zodat de generator zelf taalloos blijft. Fouten: ontbrekende `AppId`, `AppName`,
`AppVersion` of `SourceFilesPath`, en een regeleinde in een waarde (die waarde wordt dan weggelaten).
Waarschuwingen: pagina aan zonder bestand, bestand of map die niet bestaat, ongeldig of onvindbaar
hoofdprogramma, snelkoppelingen zonder hoofdprogramma, Select Components aan (nog niet ondersteund),
Select Tasks aan zonder taken en andersom (bureaubladpictogram zonder Select Tasks-pagina), onbekende
taal-id. De meldingen over knopinstellingen staan in sectie 32.

**Bewuste keuzes.**

- Een bestandsnaam of mapnaam die Setup aanmaakt (de terugvalwaarden van `DefaultDirName` en
  `DefaultGroupName`, en de naam van een snelkoppeling) wordt bestandsnaam-veilig gemaakt. ISCC weigert
  een aanhalingsteken in de Name-parameter van `[Icons]`. `AppName` zelf blijft ongewijzigd. Een naam die
  daarna leeg zou zijn (bijvoorbeeld `...`) wordt `App`, zodat er geen `{group}\` zonder naam ontstaat
  (CodeRabbit, PR #27).
- `UninstallDisplayVersion` wordt niet geschreven: het is geen `[Setup]`-richtlijn.
- Codering UTF-8 met BOM: Inno Setup 7.1.0 leest ook UTF-8 zonder BOM goed, Inno Setup 6 niet.
- De golden file `Golden/Full.iss` is een vergelijkingsbestand voor de test en moet ongewijzigd blijven. Wie
  een gegenereerd script met eigen paden wil compileren, werkt op een kopie buiten de repo.
- Een pad met een accolade in `Source` of `OutputDir` compileert, maar een pad dat zelf een bestaande
  Inno-constante bevat (bijvoorbeeld een map die `{app}` heet) is niet getest en zeer onwaarschijnlijk.

**Tests** (`tests/InnoSetupStudio.Tests/Generation/`, 111 nieuwe testgevallen, 179 in totaal).

| Bestand | Inhoud |
|---|---|
| `IssGeneratorTests.cs` | Mapping per beslistabel: paginamodi, standaardwaarden, bestandsrichtlijnen, User Info, afbeeldingen. |
| `IssGeneratorIssuesTests.cs` | Escaping, meldingen, snelkoppelingen en taken, talen, knopinstellingen, eigenschappen van de uitvoer (deterministisch, alleen CRLF, BOM). |
| `IssGeneratorGoldenTests.cs` | Vergelijkt drie gegenereerde scripts met de bestanden in `Golden/`. Met de omgevingsvariabele `UPDATE_GOLDEN=1` schrijft de test de bestanden opnieuw en faalt hij één keer, zodat een gewijzigd bestand nooit ongezien door een test komt. |
| `IssCompilerTests.cs` | Laat ISCC 18 scripts compileren: minimaal, alle pagina's aan en uit, vier combinaties van architectuur en wizardstijl, alle 33 talen, speciale tekens, accolades in zeven paden, een naam met accent, en een `AppId` zonder `{{` dat juist niet mag compileren. Controleert exitcode 0, het bestaan van de installer en dat de uitvoer geen regel met `Warning:` bevat. Elke catalogustaal moet bovendien een bestaand `.isl`-bestand hebben. |
| `InnoSetupInstallation.cs` | Zoekt `ISCC.exe` (omgevingsvariabele `INNO_SETUP_DIR`, anders Inno Setup 7 of 6 in Program Files). `IsccFact` en `IsccTheory` slaan de tests over als het programma ontbreekt, zoals op een build-server. |

De ISCC-tests bouwen een installer maar voeren hem niet uit: Setup vraagt standaard om
beheerdersrechten, wat een geautomatiseerde test niet kan afhandelen.

## 31. Genereer .iss in de app (2026-10-05)

Stap 3 van `docs/Ontwerp-Dunne-Generator.md`: de generator uit sectie 30 is nu vanuit de app te gebruiken.

**Gedrag.** De knop "Genereer .iss" staat in de bovenbalk, tussen Projectinstellingen en Installer bouwen,
en is actief zodra er een project open is.

1. Zijn er niet-opgeslagen wijzigingen in de schermeditor, dan vraagt de app: Ja slaat ze eerst op en
   genereert daarna, Nee genereert van de laatst opgeslagen versie, Annuleren stopt. De schermeditor
   schrijft zijn velden pas bij Opslaan terug naar het projectobject, dus zonder deze vraag zou het script
   stilzwijgend zonder die wijzigingen ontstaan. Het opslaan volgt dezelfde volgorde als
   `ScreenEditor_SaveClicked`: `IsDirty` pas na een geslaagde save op `false`.
2. De generator draait op een achtergrondthread (`Task.Run`), omdat hij controleert of mappen en bestanden
   bestaan en dat op een netwerkschijf even kan duren. De knop staat in die tijd uit.
3. Bij een fout (`GenerationResult.HasErrors`) wordt er geen bestand geschreven. Het resultaatvenster toont
   alleen de meldingen en zegt dat er geen script is geschreven.
4. Anders opent een `SaveFileDialog`, standaard in de map van het `.issproj` met de naam
   `<projectnaam>.iss` (Herbert koos op 2026-10-05 voor de uitvoerlocatie naast het `.issproj`, aanpasbaar in
   de dialoog). Het script wordt geschreven met `IssGenerator.ScriptEncoding` (UTF-8 met BOM). Mislukt het
   schrijven (bestand in gebruik, geen rechten), dan volgt een foutmelding.
5. Daarna toont `GenerationResultWindow` het pad van het script, een knop "Map openen" (Verkenner met het
   bestand geselecteerd) en de meldingen, fouten eerst, dan waarschuwingen, dan info. Zijn er geen meldingen,
   dan staat er dat het script klaar is om te compileren.

**Taal van de meldingen.** `GenerationIssueFormatter` (in `InnoSetupStudio.App/Localization`) zoekt voor elke
`GenerationIssueCode` de resx-sleutel `GenIssue_<Code>` en vult de plaatsaanduidingen `{0}`, `{1}` met de
argumenten van de melding. De ernst heeft `GenSeverity_<Severity>`. De drie resx-bestanden hebben er 28
sleutels bij gekregen (nu 198 per taal, inclusief de melding over de lege bestandsnaam en de weigering van het projectbestand als doel). Een beschadigde vertaling (`FormatException`) laat het venster
niet crashen: dan verschijnt de ongeformatteerde tekst.

**Bewuste keuzes.**

- De meldingen noemen waar nodig de technische veldnaam, zoals `LicenseFilePath` of `AppName`. Dat is voor
  een ontwikkelaar duidelijk genoeg. Een vertaling per veld naar het label in het scherm kan later.
- De ernst staat altijd als tekst (Fout, Waarschuwing, Info) in een eigen kolom. De kleur komt uit
  `Brush.Danger` en `Brush.Warning`, die alle negen thema's al definiëren; kleur is dus alleen een
  extra aanwijzing, geen enige drager van de betekenis.
- Het script overschrijven vraagt bevestiging via `OverwritePrompt`. Het bestand is bewust een
  gegenereerd bestand: de koptekst zegt dat handmatige wijzigingen verloren gaan.
- Het script wordt eerst naar een tijdelijk bestand in dezelfde map geschreven en pas na een geslaagde
  write over het doelbestand heen gezet, zodat een mislukte write een bestaand script niet afkapt. Kiest de
  gebruiker in de dialoog het `.issproj` zelf als doel (mogelijk via "Alle bestanden"), dan weigert de app dat
  met een melding. Project en projectpad worden bij de klik samen vastgelegd, zodat het script niet onder
  de naam van een ander project terechtkomt als de gebruiker intussen een ander project opent
  (CodeRabbit, PR #28).
- Een leeg veld "Bestandsnaam installer" blokkeert niets (de standaardnaam `<AppName>-<AppVersion>-Setup`
  wordt gebruikt), maar de generator meldt het sinds de test van 2026-10-05 met de Info-melding
  `OutputBaseFilenameDefaulted`. Het argument is de naam die in het script komt. Alleen een leeg of
  uit spaties bestaand veld geeft de melding; een ingevulde naam die tot een andere naam wordt
  schoongemaakt (ongeldige tekens worden `_`) geeft geen melding.

**Tests** (10 nieuwe testgevallen in deze stap, plus 2 voor de melding over de lege bestandsnaam:
191 in totaal). `GenerationIssueResourceTests` leest de drie
resx-bestanden rechtstreeks en controleert dat elke `GenerationIssueCode` een tekst heeft in NL, EN en DE
met precies de plaatsaanduidingen die de generator aan argumenten meegeeft, dat elke ernst een tekst heeft
en dat de teksten van de knop en het resultaatvenster bestaan. Voegt iemand een code toe, dan faalt de test
totdat teksten en argumentenaantal er zijn. De UI zelf (knop, dialoog, venster) is niet geautomatiseerd
getest; dat doet Herbert handmatig.

## 32. Knopinstellingen in de generator (2026-10-06)

Stap 4 van `docs/Ontwerp-Dunne-Generator.md`. Het ontwerp en de metingen staan in
`docs/Ontwerp-Knopinstellingen-Generator.md`; deze sectie beschrijft wat er is gebouwd. De generator
schrijft nu de knopinstellingen van de schermeditor weg: tekst, tooltip, lettertype, lettergrootte,
vet, ingeschakeld en zichtbaar voor Terug, Volgende en Annuleren op de acht schermen met een
knopmodel, en dezelfde velden voor de twee Bladeren-knoppen. Tekstkleur kan niet: Setup tekent zijn
knoppen met de themakleur (gemeten, `Font.Color` heeft geen effect). De generator schrijft geen kleurcode;
sectie 33 beschrijft hoe het kleurveld daarna uit de IDE is gehaald.

**Nieuwe en gewijzigde bestanden in `InnoSetupStudio.Core/Generation/`.**

| Bestand | Rol |
|---|---|
| `ButtonSettingsResolver.cs` | Publiek: `WizardButton`, `EffectiveButtonSettings` en `ButtonSettingsResolver.Resolve`. Bepaalt per knop de waarde uit eigen instelling, Standaardscherm en niets, met dezelfde regels als de `Effective*`-eigenschappen van de editor. Puur en zonder toestand. |
| `ButtonScript.cs` | Intern. Bouwt de berichten voor `[CustomMessages]` en de regels voor `[Code]`, en meldt de nieuwe waarschuwingen. Weet niets van de tekstopmaak van het bestand. |
| `IssWriter.cs` | Nieuwe methode `CodeLine` voor vrije tekst in `[Code]`. |
| `IssGenerator.cs` | Roept `ButtonScript` aan na `[Languages]`. `WriteLanguagesSection` geeft de talen terug die erin staan (zonder Engels), voor de vertalingen. `[CustomMessages]` komt na `[Languages]`, `[Code]` als laatste. De oude Info-melding is weg, evenals de reflectiecontrole die de aanpassingen telde (die staat nu in `ButtonScript` voor de melding bij een scherm dat uit staat). |

**Hoe het script ontstaat.**

1. Alle instellingen worden eerst gekopieerd met getrimde teksten. Een tekst met een regeleinde wordt
   gemeld (`ValueContainsLineBreak`, veldnaam `<eigenschap>.<veld>`) en weggelaten. Een regeleinde in het
   Standaardscherm wordt daardoor één keer gemeld in plaats van per scherm.
2. Voor elk getoond scherm, in vaste volgorde (Welkom, Licentie, Info voor, Gebruikersgegevens, Doelmap,
   Startmenumap, Ready, Info na), en per knop (Terug, Volgende, Annuleren) bepaalt de resolver de
   effectieve waarden. Schermen die uit staan krijgen geen code.
3. Elke tekst en tooltip gaat als bericht in `[CustomMessages]` met de naam `Btn<scherm><knop><veld>`
   (bijvoorbeeld `BtnWelcomeNextCaption`). Eerst de regel zonder taalvoorvoegsel, dan de vertalingen in
   cataloguvolgorde voor de talen die in `[Languages]` staan. Is de universele tekst leeg en is er wel een
   vertaling, dan staat er een lege regel en controleert de code vóór het toewijzen of de tekst niet leeg
   is. Zo behouden talen zonder vertaling Setup's eigen tekst.
4. `InitializeWizard` legt de beginwaarden van lettertype, -grootte en -stijl vast en stelt de
   Bladeren-knoppen in. `CurPageChanged` zet eerst lettertype, vet en tooltip terug naar de beginwaarde
   (Setup doet dat niet zelf; voor de tooltip zijn dat de gemeten beginwaarden leeg en `False`) en stelt
   daarna per pagina (`case CurPageID of`) de waarden in. Terugzetten gebeurt alleen voor de
   combinaties van knop en eigenschap die ergens in het project worden gebruikt.
5. `Enabled` en `Visible` worden alleen als `False` geschreven. Setup zet beide bij elke paginawissel zelf
   terug, en zet Volgende op de Licentie-pagina uit tot de licentie is geaccepteerd; een expliciet `True`
   zou dat kunnen omzeilen. De afweging voor `Visible` staat in sectie 11 punt 6 van het ontwerp.

**Meldingen** (`GenerationIssueCode`, teksten in NL, EN en DE; na sectie 33 196 sleutels per taal).

| Code | Ernst | Argumenten | Wanneer |
|---|---|---|---|
| `ButtonSettingsForHiddenScreen` | Info | veldnaam | Een scherm of Bladeren-knop met eigen instellingen staat uit in het project. |
| `NextButtonUnusable` | Waarschuwing | veldnaam van het scherm | Volgende is op een getoond scherm uitgeschakeld of verborgen. Het script zet hem nergens weer aan. |

`ButtonSettingsNotGenerated` (stap 3) is verwijderd, met zijn teksten en testregel. De waarschuwing
`ButtonTextColorNotSupported` uit de eerste versie van deze stap is in sectie 33 weer verwijderd.

**Tests.** Het totaal was na deze stap 268 testgevallen (265 na sectie 33).

| Bestand | Inhoud |
|---|---|
| `ButtonSettingsResolverTests.cs` | Eigen waarde wint, dan het Standaardscherm, dan niets; spaties tellen als leeg; `false` van het scherm wint van `true` van het Standaardscherm; vertalingen cascaderen sinds sectie 34 wel; elke knop leest zijn eigen velden; de Bladeren-knop heeft geen cascade. |
| `IssGeneratorButtonTests.cs` | Geen code zonder aanpassingen; volgorde van de secties; berichten met en zonder taalvoorvoegsel; lege universele tekst; letterlijke teksten met speciale tekens; regeleinden; Standaardscherm; `Enabled` en `Visible` alleen als `False`; vastleggen en terugzetten van lettertype, grootte, vet en tooltip; vaste volgorde van de regels per knop; Bladeren-knoppen; schermen die uit staan; de drie nieuwe meldingen; samenhang tussen gebruikte en gedefinieerde berichtnamen; het voorbeeld uit het ontwerp. |
| `IssGeneratorGoldenTests.cs` | Nieuw goldenbestand `Golden/Buttons.iss` (acht schermen, drie talen, beide Bladeren-knoppen). De drie bestaande bestanden zijn ongewijzigd. |
| `IssCompilerTests.cs` | ISCC compileert zonder waarschuwing: alle schermen en drie talen in modern en classic, één taal, alleen het Standaardscherm, dertien teksten met speciale tekens (`'`, `"`, `%`, `%n`, `{`, `{{`, `{app}`, `{cm:...}`, `;`, `=`, accenten), een lettertypenaam met `'` en knoppen op schermen die uit staan. |

**Handmatig gecontroleerd door mij, eenmalig.** Het goldenscript is met kleine aanpassingen
(`PrivilegesRequired=lowest`, tijdelijke doelmap, bestaande dummybestanden) gecompileerd en de installer
stil gedraaid (`/VERYSILENT`) in Engels, Nederlands en Duits: geen uitzondering in `InitializeWizard`.
Een kopie met een bewust onbekende berichtnaam gaf in dezelfde run wel een fout (exitcode 1), dus de
controle slaat aan. `CurPageChanged` draait niet in een stille run; dat gedeelte test Herbert met de lijst
in sectie 12 van het ontwerp.

**Bewuste keuzes en afwijkingen van het ontwerp.**

- De tests verwijzen alleen naar Core, dus de geplande vergelijkingstest tussen resolver en editor kon niet.
  Tot de editor de resolver zelf gebruikt (backlog) staan de regels op twee plaatsen.
- Teksten worden getrimd, zoals de rest van de generator.
- `Visible` alleen als `False` (zie boven).
- Een Standaardscherm-instelling wordt per getoond scherm uitgeschreven. Dat maakt het script langer
  (acht keer dezelfde regel), maar houdt de logica eenvoudig en voorspelbaar. Het Standaardscherm geldt
  alleen voor de acht schermen met een knopmodel.

## 33. Tekstkleur uit de IDE verwijderd (2026-10-06)

Stap 4 (sectie 32) toonde met een meting dat Setup de tekstkleur van knoppen niet toepast: `Font.Color`
compileert, maar de knop blijft in de themakleur, in `modern` en in `classic`. Een instelling die niet
kan werken hoort niet in de IDE te staan. Herbert besloot daarom het veld te verwijderen en geen eigen
knoppen te tekenen: de IDE gebruikt de standaardfunctionaliteit van Inno Setup. De oudere secties (14
tot en met 24) beschrijven de tekstkleur nog zoals die toen is gebouwd; deze sectie is leidend.

**Wat is verwijderd.**

| Onderdeel | Verwijderd |
|---|---|
| Model (Core) | `BackButtonTextColor`, `NextButtonTextColor` en `CancelButtonTextColor` uit `WizardScreenButtonSettings`; `TextColor` uit `BrowseButtonSettings`. |
| Generator (Core) | `TextColor` uit `EffectiveButtonSettings` en de resolver, de telling en de melding `ButtonTextColorNotSupported` in `ButtonScript`. De uitvoer van het script verandert niet: de goldenbestanden zijn ongewijzigd. |
| Viewmodels (App) | De kleurvelden en `Effective*TextColor` in `WizardScreenEditorViewModel`, `DefaultScreenEditorViewModel`, `SelectDestinationPageEditorViewModel` en `SelectProgramGroupPageEditorViewModel`, de kleurenkiezer (`PickColor` en de `Pick...TextColor`-opdrachten) en in `ButtonPropertiesViewModel` de kleurvelden, de voorvertoningskleur en de kiezer. |
| Schermen (App en Wizard) | De kleurrij in het venster Knopeigenschappen, alle `Foreground`-bindingen op de tekstkleur in de schermeditor en in de voorvertoningen van de doelmap- en startmenumappagina, en de twee `HexColorToBrushConverter`-klassen met hun registraties. |
| Bronnen | Het icoon `SelectColor`, en in het csproj `UseWindowsForms` met de twee `Using Remove`-regels (die waren alleen nodig voor het WinForms-dialoogvenster van de kleurenkiezer). |
| Teksten | `LabelButtonTextColor`, `GenIssue_ButtonTextColorNotSupported`, en `ButtonSelectColor` en `ButtonPickColor` (die waren daarna ongebruikt), in NL, EN en DE. Dat zijn 196 sleutels per taal. |
| Tests | De drie tekstkleurtests in `IssGeneratorButtonTests`, de ISCC-test voor een tekstkleur, de regel in `GenerationIssueResourceTests.ArgumentCounts` en de kleurvelden in de overige tests. |

**Voorvertoning.** De knoppen in de voorvertoning gebruiken nu de themakleur. De `TextBlock` in elke knop
behoudt zijn `Foreground`-binding aan de knop zelf (`RelativeSource AncestorType=Button`), omdat de
app-brede knopstijl de kleur anders niet doorgeeft aan inhoud van buiten de `ControlTemplate`.

**Oude projectbestanden.** `JsonInstallerProjectService` gebruikt de standaard serializeropties en
negeert daardoor onbekende eigenschappen. Een `.issproj` met `...TextColor`-velden opent dus zonder
fout, de overige knopinstellingen blijven intact en bij opslaan verdwijnen de oude velden. De test
`LoadAsyncIgnoresTextColorFieldsFromAnOlderProjectFile` bewijst dat voor het Standaardscherm, een gewoon scherm en beide Bladeren-knoppen.

**Tests.** Het totaal is nu 265 testgevallen (268 min drie generatortests, min één ISCC-test, plus de
test voor het oude projectbestand). Build zonder waarschuwingen.


## 34. Vertalingen van knopteksten via het Standaardscherm (2026-10-06)

Herbert zag bij de test van sectie 33 dat het Standaardscherm de knopteksten en tooltips wel kent, maar geen
vertalingen per taal. Dat was de bewuste vereenvoudiging uit sectie 24. Het ontwerp staat in
`Ontwerp-Vertalingen-Standaardscherm.md`; deze sectie is leidend en vervangt de vereenvoudiging uit sectie 24
en de oplossing met `NoLanguageOverridesOnDefaultScreen` uit dezelfde sectie.

**Regels.** Per knop, per taal, voor tekst en tooltip apart. Voor één taal wint de eerste regel die van toepassing is:

1. Het scherm heeft een eigen vertaling voor die taal.
2. Het scherm heeft een eigen tekst (Engels of universeel) maar geen vertaling voor die taal: die eigen tekst geldt.
3. Het scherm heeft niets: de vertaling van het Standaardscherm voor die taal, anders de universele tekst van het Standaardscherm, anders de eigen tekst van Setup.

Alleen spaties tellen als leeg. De Bladeren-knoppen hebben geen cascade.

**Wat is gewijzigd.**

| Onderdeel | Wijziging |
|---|---|
| `ButtonSettingsResolver` (Core) | `Resolve` bepaalt de vertalingenlijsten met de drie regels, uit de lijsten van het scherm en van het Standaardscherm. De regel zit in de publieke methode `ResolveTranslation(ownText, ownTranslation, defaultTranslation)`, zodat de generator en de editor dezelfde regel gebruiken. |
| `ButtonScript` (Core) | Ongewijzigd. Die schrijft de vertalingen uit het resolverresultaat al weg, ook als de universele tekst leeg is (lege regel plus beveiligde toewijzing). Een regeleinde in een vertaling van het Standaardscherm wordt één keer gemeld. |
| `ButtonPropertiesViewModel` (App) | `LanguageOverrideRow` heeft `CaptionPlaceholder` en `TooltipPlaceholder`. Het record `InheritedTranslations` brengt de waarden van het Standaardscherm binnen. De voorinvulling gebruikt `ResolveTranslation` en wordt bijgewerkt zodra je de eigen tekst of tooltip wijzigt. |
| `ButtonPropertiesWindow.xaml` en `ScreenEditorControl.xaml.cs` | De vertaalrijen tonen de voorinvulling als grijze tekst. De knoppen van het Standaardscherm tonen nu de vertaalrijen (`vm.NonEnglishLanguageIds`), `NoLanguageOverridesOnDefaultScreen` is vervallen. De hint bevat op gewone schermen een extra zin (`HintLanguageOverridesFromDefaultScreen`). |
| Teksten | Eén nieuwe sleutel in NL, EN en DE. Dat zijn 197 sleutels per taal. |

**Tests.** Het totaal is nu 286 testgevallen (265 na sectie 33). Nieuw of vervangen:

| Bestand | Dekking |
|---|---|
| `ButtonSettingsResolverTests.cs` | De vier voorbeeldrijen uit het ontwerp, talen die alleen het Standaardscherm heeft, lege vertalingen, tekst en tooltip apart, elke knop eigen vertalingen, en `ResolveTranslation` in de volgorde van de regels. |
| `IssGeneratorButtonTests.cs` | Vertalingen van het Standaardscherm bereiken elk getoond scherm, eigen vertaling wint, een Standaardscherm zonder universele tekst geeft een beveiligde toewijzing, talen buiten het project en verborgen schermen worden genegeerd, een regeleinde wordt één keer gemeld. |
| `IssCompilerTests.cs` | Twee ISCC-tests: vertalingen van het Standaardscherm met speciale tekens (`It's`, `100% {app}`, `Größe é; a=b`) en alleen een vertaling zonder universele tekst. Beide compileren zonder waarschuwing. |

Het goldenbestand `Buttons.iss` is niet veranderd. De editorlogica heeft geen eenheidstests, omdat het testproject alleen naar Core verwijst. De regel zelf is wel getest via `ResolveTranslation`.

**Oude projectbestanden.** Het bestandsformaat verandert niet. Vertalingen die al op het Standaardscherm stonden (bijvoorbeeld door handmatig bewerken) tellen nu mee. Een taal die je uit het project haalt houdt haar vertalingen in het bestand.
