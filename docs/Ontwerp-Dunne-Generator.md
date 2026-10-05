# Ontwerp: dunne .iss-generator (fase 5, versie 1)

Status: voorstel, 2026-10-05. Er is nog geen code geschreven. Beslispunten staan in sectie 9.

## 1. Doel

Een eerste generator die een `InstallerProject` omzet in een `.iss`-bestand dat ISCC.exe zonder fouten compileert en dat een werkende installer oplevert met de gekozen wizardschermen, de gekozen talen en de bestanden uit `SourceFilesPath`.

"Dun" betekent: alleen wat Inno Setup zonder Pascal Script kan (richtlijnen in `[Setup]`, plus `[Languages]`, `[Files]`, `[Icons]`, `[Tasks]`). Alles wat een `[Code]`-blok nodig heeft (knopteksten, knopkleuren, lettertypen, tooltips, Bladeren-knoppen) valt buiten versie 1. Wat de generator niet kan vertalen verdwijnt niet stilzwijgend: het komt in een lijst met meldingen die de gebruiker te zien krijgt (sectie 6).

## 2. Waar de code komt

- `InnoSetupStudio.Core/Generation/IssGenerator.cs`: pure functie `Generate(InstallerProject) -> GenerationResult`. Geen bestandstoegang, geen WPF, daardoor volledig unit-testbaar.
- `IssWriter.cs`: kleine hulpklasse die secties, `Sleutel=Waarde`-regels en parameterregels (`Name: "x"; Flags: y`) opbouwt en het escapen centraal doet.
- `GenerationResult.cs` en `GenerationIssue.cs`: de tekst plus een lijst meldingen (ernst, code, bericht, optioneel de bron-eigenschap).
- App-kant (stap 3 van sectie 8): een menu-item of knop "Genereer .iss" in `MainWindow`, een SaveFileDialog, en een venster of dialoog met de meldingen.
- ISCC.exe aanroepen vanuit de app blijft fase 7. Voor versie 1 draait ISCC alleen in een test en handmatig door Herbert.

## 3. Wat het model al levert en hoe het in het .iss komt

Een regel hieronder staat alleen in het resultaat als de waarde niet leeg is, tenzij anders vermeld.

| Model | Inno Setup | Opmerking |
|---|---|---|
| `AppId` | `AppId={{GUID}` | Openingsaccolade verdubbelen. Dat patroon staat in echte scripts op de pc (Inno-All-in-One-Setup). Bevestigen met ISCC-test. |
| `AppName`, `AppVersion` | `AppName`, `AppVersion` | Verplicht. Leeg is een fout. |
| `Publisher` | `AppPublisher` | |
| `PublisherUrl` | `AppPublisherURL` | `AppSupportURL` en `AppUpdatesURL` ook vullen, zoals het HNSoftwareInstallerFramework doet? Zie beslispunt. |
| `PublisherEmail` | `AppContact` | Directive bestaat (geverifieerd in de docs), toont in Programma's en onderdelen. |
| `DefaultDirName` | `DefaultDirName` | Leeg wordt `{autopf}\<AppName>`. De waarde is bewust een Inno-constanten-tekst en wordt niet geëscaped. |
| `DefaultGroupName` | `DefaultGroupName` | Leeg wordt `<AppName>`. |
| `ShowSelectDestinationPage`, `DirPageMode` | `DisableDirPage` | Pagina uit: `yes`. Anders AlwaysShow = `no`, NeverShow = `yes`, AutoSkipIfKnown = `auto`. |
| `ShowSelectProgramGroupPage`, `GroupPageMode` | `DisableProgramGroupPage` | Zelfde mapping. |
| `ShowWelcomePage` | `DisableWelcomePage` | Inno-standaard is `yes` (geverifieerd). Dus bij een aangevinkte pagina expliciet `no` schrijven. |
| `ShowReadyPage`, `ShowFinishedPage` | `DisableReadyPage`, `DisableFinishedPage` | Inno-standaard is `no`. Alleen schrijven als de pagina uit staat. |
| `ShowLicensePage`, `LicenseFilePath` | `LicenseFile` | Pagina aan zonder bestand: melding, geen directive. |
| `ShowInfoBeforePage`, `InfoBeforeFilePath` | `InfoBeforeFile` | Zelfde regel. |
| `ShowInfoAfterPage`, `InfoAfterFilePath` | `InfoAfterFile` | Zelfde regel. |
| `ShowUserInfoPage` | `UserInfoPage=yes` | |
| `DefaultUserInfoName/Org/Serial` | `DefaultUserInfoName/Org/Serial` | |
| `UsePreviousUserInfo`, `UsePreviousAppDir`, `UsePreviousGroup`, `UsePreviousSetupType`, `UsePreviousTasks`, `UsePreviousLanguage` | `UsePrevious...` | Alleen schrijven als de waarde afwijkt van Inno's standaard (`yes`). |
| `AppendDefaultGroupName`, `AlwaysUsePersonalGroup` | zelfde namen | Alleen bij afwijking van Inno's standaard. |
| `DisableReadyMemo`, `AlwaysShowDirOnReadyPage`, `AlwaysShowGroupOnReadyPage` | zelfde namen | Alleen bij afwijking van Inno's standaard (`no`). |
| `SetupIconFile`, `WizardImageFile`, `WizardSmallImageFile` | zelfde namen | Leeg betekent: Inno-standaard. De meegeleverde standaardafbeelding uit de editor komt dus niet in het .iss. |
| `OutputPath` | `OutputDir` | |
| `SourceFilesPath` | `[Files]` | `Source: "<pad>\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs`, zoals `Files.iss` in het HNSoftwareInstallerFramework. |
| `CreateStartMenuIcon` | `[Icons]` | `Name: "{group}\<AppName>"; Filename: "{app}\<hoofdbestand>"`. Heeft het hoofdbestand nodig (sectie 4). |
| `CreateDesktopIcon` | `[Tasks]` en `[Icons]` | Taak `desktopicon` (Flags: unchecked) plus `{autodesktop}`-icoon met `Tasks: desktopicon`. Taaltekst via `{cm:CreateDesktopIcon}`, die in elke taalbestand al vertaald is (geverifieerd in de docs). |
| `SupportedLanguageIds` | `[Languages]` | Eerste regel altijd Engels: `Name: "english"; MessagesFile: "compiler:Default.isl"`. Andere talen: `Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"`. Inno gebruikt de eerste taal als terugval. Alle 32 bestandsnamen uit `InnoLanguageCatalog` bestaan in de installatie van Inno Setup 7.1.0 op de pc (gecontroleerd). |

Rond het schrijven van `UsePrevious...`: Inno's standaard is `yes`, het project volgt dat. Alleen afwijkingen schrijven houdt het bestand kort. De alternatieve keuze is alles expliciet schrijven, zodat je in het .iss ziet wat de Studio beheert. Beide werken technisch.

## 4. Wat het model mist voor een werkend .iss

Zonder deze velden komt er wel een compileerbaar .iss uit, maar geen bruikbare installer.

1. **Hoofduitvoerbestand** (bijvoorbeeld `MijnApp.exe`, relatief aan `SourceFilesPath`). Nodig voor `[Icons]` en straks voor `[Run]` (programma starten na installatie). Zonder dit veld geen snelkoppelingen, ook al staat `CreateStartMenuIcon` aan.
2. **Naam van het uitvoerbestand** (`OutputBaseFilename`). Inno's standaard is `setup`. Voorstel: standaardwaarde `<AppName>-<AppVersion>-Setup`, aanpasbaar.
3. **64-bit installatie** (`ArchitecturesInstallIn64BitMode`). Zonder deze richtlijn draait Setup in 32-bit modus en wijst `{autopf}` op Program Files (x86). Dat is bekend Inno-gedrag, maar de officiële pagina gaf daar via de fetch geen tekst over. Ik bevestig dit in stap 2 met een ISCC-test. Voorstel: een keuzelijst "32-bit", "64-bit" in Projectinstellingen met 64-bit als standaard voor nieuwe projecten.
4. **WizardStyle**. Inno's standaard is `classic` (geverifieerd). Het HNSoftwareInstallerFramework gebruikt `modern`. Voorstel: `modern` als vaste waarde in versie 1, geen UI.
5. **PrivilegesRequired**. Inno's standaard is admin. Niet nodig voor versie 1, wel het bedoelde gedrag voor `{autopf}`.

Vaste waarden die de generator zonder UI schrijft: `Compression=lzma2`, `SolidCompression=yes`, `WizardStyle=modern`, `UninstallDisplayName`, `UninstallDisplayVersion` (zoals `Base.iss`).

## 5. Gedrag van de generator

- **Deterministisch.** Zelfde project, zelfde tekst. Geen tijdstempel, geen willekeurige volgorde. Dat houdt git-diffs bruikbaar. De kop zegt wel "gegenereerd door Inno Setup Studio, handmatige wijzigingen gaan verloren".
- **Eenrichting.** Versie 1 schrijft alleen. Het inlezen van een bestaand .iss (de parser uit fase 5) komt later.
- **Escaping.** Waarden uit het project zoals `AppName` die een `{` bevatten, krijgen `{{`. Parameters tussen aanhalingstekens in `[Files]`, `[Icons]` en `[Languages]` krijgen `""` voor een aanhalingsteken. Regeleinden in een waarde zijn een fout. `DefaultDirName` en `DefaultGroupName` zijn de uitzondering: dat zijn bewust constanten-teksten.
- **Codering.** UTF-8 met BOM en CRLF-regeleinden, zodat niet-ASCII-tekens (bijvoorbeeld "é" in een bedrijfsnaam) goed compileren. Dit verifieer ik in een test met zo'n teken, het is een aanname tot dan.
- **Padseparatoren.** Windows-paden blijven met backslash.
- **Volgorde van secties.** `[Setup]`, `[Languages]`, `[Tasks]`, `[Files]`, `[Icons]`. Binnen `[Setup]` gegroepeerd (toepassing, mappen, pagina's, uiterlijk, uitvoer) met commentaarregels.

## 6. Meldingen (GenerationIssue)

Drie niveaus. Een fout blokkeert het schrijven van het bestand, een waarschuwing niet.

- Fout: lege `AppName`, `AppVersion` of `AppId`; lege `SourceFilesPath`; regeleinde in een waarde.
- Waarschuwing: pagina aangevinkt maar bestand ontbreekt (Licentie, Info Before, Info After); `SourceFilesPath` bestaat niet op schijf; `CreateStartMenuIcon` aan zonder hoofdbestand; Select Components of Select Tasks aangevinkt terwijl daar nog niets voor gegenereerd wordt.
- Info: knopinstellingen die in versie 1 niet worden vertaald (aantal schermen en knoppen dat instellingen heeft), per-taal-teksten die niet worden vertaald, Bladeren-knopinstellingen.

De meldingen krijgen een stabiele code (bijvoorbeeld `ISS001`) en een resx-sleutel, zodat ze in NL, EN en DE verschijnen zoals de rest van de UI.

## 7. Tests

1. **Golden-file tests** in `InnoSetupStudio.Tests`: een minimaal project, een volledig project (alle schermen, drie talen) en randgevallen voor escaping. De verwachte tekst staat als testbestand in de repo.
2. **Mappingtests** per beslistabel: paginamodus naar `no/yes/auto`, lege waarden, afwijkingen van de standaard.
3. **ISCC-integratietest.** Genereer uit een voorbeeldproject met een dummy-bronbestand in een tijdelijke map, draai `ISCC.exe`, controleer exitcode 0 en dat het setup-bestand bestaat. De test zoekt ISCC op `C:\Program Files\Inno Setup 7\` en in een omgevingsvariabele, en slaat zichzelf over als het programma er niet is. Op de pc van Herbert draait de test dus echt mee, wat de belangrijkste controle is dat de gegenereerde tekst klopt.
4. Een test dat alle ids uit `InnoLanguageCatalog` een bestaand `.isl`-bestand hebben (alleen als Inno Setup aanwezig is).

Voor `InnoLanguageCatalog` is een kleine wijziging nodig: het record bewaart nu alleen de id in kleine letters, niet de bestandsnaam in de juiste hoofdletters (`BrazilianPortuguese.isl`). Windows is niet hoofdlettergevoelig, maar een nette `MessagesFile`-regel gebruikt de echte bestandsnaam.

## 8. Stappen en PR-indeling

Elke stap is een eigen feature-branch en PR, met handmatige test door Herbert vóór de merge.

1. **Projectvelden.** `MainExecutable`, `OutputBaseFilename` (leeg = `<AppName>-<AppVersion>-Setup`), architectuurkeuze 32-bit of 64-bit (standaard 64-bit voor nieuwe projecten, bestaande projecten zonder deze JSON-sleutel krijgen ook 64-bit) en `WizardStyle` (classic of modern, standaard modern). Daarbij de UI in Projectinstellingen, de drie resx-bestanden, JSON-compatibiliteit voor oudere `.issproj`-bestanden en tests. Het `ProjectSettingsViewModel` heeft hier geen pass-through nodig, want deze velden worden in Projectinstellingen zelf bewerkt.
2. **Generator in Core.** `IssGenerator`, `IssWriter`, meldingen, de aanpassing aan `InnoLanguageCatalog`, golden-file tests en de ISCC-integratietest. Geen UI, dus geen handmatige UI-test; Herbert controleert wel een gegenereerd voorbeeld in Inno Setup's IDE.
3. **Genereer .iss in de app.** Menu-item of knop, SaveFileDialog met standaardlocatie naast het `.issproj`, weergave van de meldingen. Daarna test Herbert de volledige keten: project maken, genereren, compileren in ISIDE of met ISCC, installer draaien.
4. **Knopinstellingen via `[Code]`** (aparte PR na stap 3). Een `CurPageChanged`-procedure per scherm voor captions, enabled, visible, kleur, lettertype en tooltip, plus de twee Bladeren-knoppen en de per-taal-teksten via `[CustomMessages]`. Dat vergt een eigen ontwerp: toewijzing van scherm naar `wpWelcome`, `wpSelectDir` enzovoort, Pascal-escaping, en de Standaardscherm-cascade.

## 9. Beslissingen van Herbert (2026-10-05)

| Onderwerp | Keuze |
|---|---|
| Knopinstellingen | Eigen vervolgstap (stap 4). Versie 1 meldt wat is overgeslagen. |
| Nieuwe projectvelden | Hoofduitvoerbestand, naam uitvoerbestand, 32-bit of 64-bit, WizardStyle als keuzelijst. |
| Paden | Absolute paden zoals ze nu in het project staan. Projectrelatieve paden volgen als aparte stap (het openstaande CodeRabbit-punt over `ProjectAssetService.Import`, zie de architectuurdoc). |
| Uitvoerlocatie | Naast het `.issproj`, via een SaveFileDialog die Herbert kan aanpassen. |

Nog te beslissen, klein genoeg om met mijn voorstel te starten tenzij Herbert iets anders wil:

- `AppSupportURL` en `AppUpdatesURL` vullen met `PublisherUrl`, zoals het HNSoftwareInstallerFramework. Voorstel: ja, zolang er geen eigen velden zijn.
- `UsePrevious...`-richtlijnen alleen bij afwijking van Inno's standaard schrijven (voorstel) of altijd.

## 10. Buiten versie 1

- `[Code]`, dus alle knopinstellingen en de Bladeren-knoppen (stap 4).
- `[Components]`, `[Types]` en een eigen `[Tasks]`-beheer. De pagina's Select Components en Select Tasks hebben nog geen editor en geen model.
- `[Run]` (programma starten na installatie), `[Registry]`, `[INI]`, `[UninstallDelete]`, handtekeningen, aangepaste uninstall-instellingen.
- Per-veld en meertalige User Info-teksten, `CheckSerial`.
- Het inlezen van een bestaand `.iss` (parser).
- ISCC.exe vanuit de app aanroepen en de compilelog tonen (fase 7).
- Projectrelatieve paden.

## 11. Risico's

- **Aanname over escaping en codering** (`{{`, UTF-8 met BOM, `""`). De ISCC-integratietest vangt dit af; zonder Inno Setup op een machine draait die test niet.
- **Inno Setup 7 tegenover 6.** Herbert heeft 7.1.0 geïnstalleerd. De gecontroleerde documentatie wees op 6.5.0 voor `Default.isl`. Directives die in 7 anders zijn dan in 6 vallen pas op bij de ISCC-test; daarom valideert die test tegen de versie die Herbert gebruikt.
- **`WizardStyle=modern`** bestaat in Inno 6 en 7. Het bestaan van andere stijlnamen (`polar`, `slate`, enzovoort) in de docs van de nieuwste versie laat ik bewust buiten de keuzelijst.
