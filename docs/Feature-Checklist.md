# Feature-checklist: Inno Setup-richtlijnen vs. Inno Setup Studio

Bron: de officiële Inno Setup-documentatie op [jrsoftware.org/ishelp](https://jrsoftware.org/ishelp/). Dit document somt (voor zover onderzocht) alle richtlijnen, sectie-parameters, Pascal Script-events en command-line-parameters van Inno Setup op, elk met:

- **Scherm / Tabblad** — hoort dit bij één specifiek wizardscherm (en welk), bij "Schermen algemeen" (het uiterlijk/gedrag van de wizard als geheel), of bij een generieke instelling — en zo ja, onder welk tabblad in Inno Setup Studio dat dan zou vallen (bestaand of nog te maken).
- **Mechanisme** — hoe de gebruiker dit in Inno Setup Studio zou instellen: **Eigenschap** (een los veld/checkbox in een bestaand of toekomstig scherm), **Sectie-item** (een rij in een lijst, zoals [Files] of [Registry]), **Pascal** (een Pascal Script-event of -functie, voor fase 6), of **CLI** (een command-line-parameter, voor fase 7/build-integratie).
- **Status** — ✅ Gedekt (al een werkend veld/scherm in de huidige app, met verwijzing naar de C#-eigenschap), 🔶 Gepland (hoort bij een al afgesproken fase uit de roadmap), ⬜ Nog niet gepland, of ➖ Verouderd (Inno Setup zelf noemt dit obsolete — geen actie nodig).

Dit is een levend document: vink af (verander ⬜/🔶 naar ✅ met een verwijzing) zodra een feature in de app landt, en corrigeer gerust de categorisering als die ergens niet klopt.

## Inhoud

1. [Tabblad: Algemeen](#1-tabblad-algemeen)
2. [Tabblad: Schermen](#2-tabblad-schermen)
3. [Wizardscherm: Welcome](#3-wizardscherm-welcome)
4. [Wizardscherm: License](#4-wizardscherm-license)
5. [Wizardscherm: Password](#5-wizardscherm-password)
6. [Wizardscherm: Info Before](#6-wizardscherm-info-before)
7. [Wizardscherm: User Info](#7-wizardscherm-user-info)
8. [Wizardscherm: Select Destination Location](#8-wizardscherm-select-destination-location)
9. [Wizardscherm: Select Components](#9-wizardscherm-select-components)
10. [Wizardscherm: Select Start Menu Folder](#10-wizardscherm-select-start-menu-folder)
11. [Wizardscherm: Select Tasks](#11-wizardscherm-select-tasks)
12. [Wizardscherm: Ready to Install](#12-wizardscherm-ready-to-install)
13. [Wizardscherm: Preparing to Install](#13-wizardscherm-preparing-to-install)
14. [Wizardscherm: Installing](#14-wizardscherm-installing)
15. [Wizardscherm: Info After](#15-wizardscherm-info-after)
16. [Wizardscherm: Setup Completed](#16-wizardscherm-setup-completed)
17. [Schermen algemeen (uiterlijk en gedrag van de wizard)](#17-schermen-algemeen-uiterlijk-en-gedrag-van-de-wizard)
18. [Tabblad: Talen](#18-tabblad-talen)
19. [Tabblad: Overige instellingen](#19-tabblad-overige-instellingen)
20. [Tabblad (toekomst): Herstart en lopende applicaties](#20-tabblad-toekomst-herstart-en-lopende-applicaties)
21. [Tabblad (toekomst): Uninstall-instellingen](#21-tabblad-toekomst-uninstall-instellingen)
22. [Tabblad (toekomst): Versie-informatie](#22-tabblad-toekomst-versie-informatie)
23. [Tabblad (toekomst): Systeemeisen en architectuur](#23-tabblad-toekomst-systeemeisen-en-architectuur)
24. [Tabblad (toekomst): Beveiliging](#24-tabblad-toekomst-beveiliging)
25. [Tabblad (toekomst): Code signing](#25-tabblad-toekomst-code-signing)
26. [Tabblad (toekomst): Compiler en compressie](#26-tabblad-toekomst-compiler-en-compressie)
27. [Sectie: Bestanden en bronnen ([Files]/[Dirs]/[InstallDelete]/[UninstallDelete])](#27-sectie-bestanden-en-bronnen-filesdirsinstalldeleteuninstalldelete)
28. [Sectie: Snelkoppelingen en programma's uitvoeren ([Icons]/[Run]/[UninstallRun])](#28-sectie-snelkoppelingen-en-programmas-uitvoeren-iconsrununinstallrun)
29. [Sectie: Registry en INI ([Registry]/[INI])](#29-sectie-registry-en-ini)
30. [Sectie: Taken, componenten en installatietypes ([Tasks]/[Types]/[Components])](#30-sectie-taken-componenten-en-installatietypes-taskstypescomponents)
31. [Taalbestanden per taal ([Languages]-bestanden, [LangOptions], [Messages], [CustomMessages])](#31-taalbestanden-per-taal-languages-bestanden-langoptions-messages-custommessages)
32. [Pascal Script: Setup Event Functions](#32-pascal-script-setup-event-functions)
33. [Pascal Script: Uninstall Event Functions](#33-pascal-script-uninstall-event-functions)
34. [Preprocessor (ISPP)](#34-preprocessor-ispp)
35. [Command-line: Setup.exe](#35-command-line-setupexe)
36. [Command-line: ISCC.exe (compiler)](#36-command-line-isccexe-compiler)
37. [Verouderd (obsolete richtlijnen)](#37-verouderd-obsolete-richtlijnen)

## 1. Tabblad: Algemeen

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| AppId | Unieke identificatie voor update-herkenning tussen versies. | Eigenschap | ✅ `InstallerProject.AppId` |
| AppName | Weergavenaam van de applicatie. | Eigenschap | ✅ `InstallerProject.AppName` |
| AppVerName | Appnaam + versie samen, getoond op Welcome-pagina en in Programma's en onderdelen. | Eigenschap | ⬜ (nu losse AppName/AppVersion, geen gecombineerd veld) |
| AppVersion | Versienummer van de applicatie. | Eigenschap | ✅ `InstallerProject.AppVersion` |
| AppPublisher | Naam van de uitgever. | Eigenschap | ✅ `InstallerProject.Publisher` |
| AppPublisherURL | Website van de uitgever, getoond in Programma's en onderdelen. | Eigenschap | ✅ `InstallerProject.PublisherUrl` |
| AppContact | Contactgegevens getoond in het Support-dialoogvenster van Programma's en onderdelen. | Eigenschap | ✅ `InstallerProject.PublisherEmail` (deels — Inno toont dit als los tekstveld, niet per se e-mail) |
| AppSupportURL | Support-URL getoond in Programma's en onderdelen. | Eigenschap | ✅ geschreven door de generator uit `InstallerProject.PublisherUrl` (nog geen eigen veld) |
| AppSupportPhone | Supporttelefoonnummer getoond in Programma's en onderdelen. | Eigenschap | ⬜ |
| AppUpdatesURL | Updates-URL getoond in Programma's en onderdelen. | Eigenschap | ✅ geschreven door de generator uit `InstallerProject.PublisherUrl` (nog geen eigen veld) |
| AppComments | Opmerkingentekst in het Support-dialoogvenster. | Eigenschap | ⬜ |
| AppModifyPath | Commando achter de "Wijzigen"-knop in Programma's en onderdelen. | Eigenschap | ⬜ |
| AppReadmeFile | Leesmij-link in Programma's en onderdelen. | Eigenschap | ⬜ |
| AppCopyright | Standaard copyrighttekst voor de versie-informatie. | Eigenschap | ⬜ (zie ook categorie 22, Versie-informatie) |
| ExtraDiskSpaceRequired | Extra benodigde schijfruimte boven op de bestanden zelf. | Eigenschap | ⬜ |
| SourceDir | Basismap met bronbestanden. | Eigenschap | ✅ `InstallerProject.SourceFilesPath` |
| OutputDir | Map waar de gecompileerde installer terechtkomt. | Eigenschap | ✅ `InstallerProject.OutputPath` |
| SetupIconFile | Icoonbestand voor Setup/Uninstall. | Eigenschap | ✅ `InstallerProject.SetupIconFile` |
| (eigen veld) | Map met eigen afbeeldingen voor de installer. | Eigenschap | ✅ `InstallerProject.CustomImagesPath` (geen directe Inno-richtlijn, eigen gemak) |

## 2. Tabblad: Schermen

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| DisableWelcomePage | Bepaalt of de Welcome-pagina wordt getoond. | Eigenschap | ✅ `WizardScreenSelection.ShowWelcomePage` |
| (LicenseFile aanwezig) | License-pagina wordt alleen getoond als er een licentiebestand is opgegeven. | Eigenschap | ✅ `WizardScreenSelection.ShowLicensePage` |
| (InfoBeforeFile aanwezig) | Info Before-pagina wordt alleen getoond als er een bestand is opgegeven. | Eigenschap | ✅ `WizardScreenSelection.ShowInfoBeforePage` |
| UserInfoPage | Bepaalt of de User Info-pagina wordt getoond. | Eigenschap | ✅ `WizardScreenSelection.ShowUserInfoPage` |
| DisableDirPage | Bepaalt of de Select Destination Location-pagina wordt getoond. | Eigenschap | ✅ `WizardScreenSelection.ShowSelectDestinationPage` |
| ([Components] aanwezig) | Select Components-pagina wordt alleen getoond als er componenten zijn. | Eigenschap | ✅ `WizardScreenSelection.ShowSelectComponentsPage` |
| DisableProgramGroupPage | Bepaalt of de Select Start Menu Folder-pagina wordt getoond. | Eigenschap | ✅ `WizardScreenSelection.ShowSelectProgramGroupPage` |
| ([Tasks] aanwezig) | Select Tasks-pagina wordt alleen getoond als er taken zijn. | Eigenschap | ✅ `WizardScreenSelection.ShowSelectTasksPage` |
| DisableReadyPage | Bepaalt of de Ready to Install-pagina wordt getoond. | Eigenschap | ✅ `WizardScreenSelection.ShowReadyPage` |
| (InfoAfterFile aanwezig) | Info After-pagina wordt alleen getoond als er een bestand is opgegeven. | Eigenschap | ✅ `WizardScreenSelection.ShowInfoAfterPage` |
| DisableFinishedPage | Bepaalt of de Setup Completed-pagina wordt getoond. | Eigenschap | ✅ `WizardScreenSelection.ShowFinishedPage` |
| Password (aanwezig) | Password-pagina wordt alleen getoond als er een wachtwoord is ingesteld. | Eigenschap | ⬜ (zie categorie 24, Beveiliging) |

## 3. Wizardscherm: Welcome

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| DisableWelcomePage | Aan/uit-schakeling, zie tabblad Schermen hierboven. | Eigenschap | ✅ (zie categorie 2) |
| (knoppen Terug/Volgende/Annuleren) | Tekst/tooltip/lettertype/zichtbaarheid per taal. | Eigenschap | ✅ `InstallerProject.WelcomeScreenButtons` |
| WizardImageFile / WizardImageFileDynamicDark | Afbeelding links op de Welcome-pagina (en Finished-pagina). | Eigenschap | ✅ `InstallerProject.WizardImageFile` (alleen lichte variant, geen dark-mode-variant) |

## 4. Wizardscherm: License

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| LicenseFile | Pad naar het licentiebestand. | Eigenschap | ✅ `InstallerProject.LicenseFilePath` |
| (knoppen Terug/Volgende/Annuleren) | Tekst/tooltip/lettertype/zichtbaarheid per taal. | Eigenschap | ✅ `InstallerProject.LicenseScreenButtons` |

## 5. Wizardscherm: Password

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| Password | Het wachtwoord zelf (gehasht opgeslagen in het script). | Eigenschap | ⬜ (zie categorie 24, Beveiliging) |
| CheckPassword | Pascal-event om een wachtwoord zelf te valideren i.p.v. Inno's eigen vergelijking. | Pascal | ⬜ (fase 6) |

## 6. Wizardscherm: Info Before

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| InfoBeforeFile | Pad naar het leesmij-/infobestand vóór installatie. | Eigenschap | ✅ `InstallerProject.InfoBeforeFilePath` |
| (knoppen Terug/Volgende/Annuleren) | Tekst/tooltip/lettertype/zichtbaarheid per taal. | Eigenschap | ✅ `InstallerProject.InfoBeforeScreenButtons` |

## 7. Wizardscherm: User Info

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| UserInfoPage | Aan/uit-schakeling, zie tabblad Schermen hierboven. | Eigenschap | ✅ (zie categorie 2) |
| DefaultUserInfoName | Standaard vooringevulde naam. Ondersteunt Inno Setup-constants (zie [Gebruikersdocumentatie/Inno-Setup-Constants.md](Gebruikersdocumentatie/Inno-Setup-Constants.md)). | Eigenschap | ✅ `InstallerProject.DefaultUserInfoName` |
| DefaultUserInfoOrg | Standaard vooringevulde organisatie. Ondersteunt constants. | Eigenschap | ✅ `InstallerProject.DefaultUserInfoOrg` |
| DefaultUserInfoSerial | Standaard vooringevuld serienummer. Ondersteunt constants. | Eigenschap | ✅ `InstallerProject.DefaultUserInfoSerial` |
| UsePreviousUserInfo | Onthoudt eerder ingevulde naam/organisatie/serienummer bij een update. | Eigenschap | ✅ `InstallerProject.UsePreviousUserInfo` |
| (knoppen Terug/Volgende/Annuleren) | Tekst/tooltip/lettertype/zichtbaarheid per taal. | Eigenschap | ✅ `InstallerProject.UserInfoScreenButtons` |
| CheckSerial | Pascal-event om een serienummer zelf te valideren. Zonder deze functie toont Inno Setup het serienummerveld niet. | Pascal | ⬜ (fase 6) |

## 8. Wizardscherm: Select Destination Location

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| DefaultDirName | Standaard voorgestelde installatiemap. | Eigenschap | ✅ `InstallerProject.DefaultDirName` |
| DisableDirPage | Hoe de pagina zich gedraagt: altijd tonen (`no`), nooit tonen (`yes`) of overslaan als dezelfde applicatie al geïnstalleerd is (`auto`, Inno Setup's eigen standaard). Het project staat standaard op altijd tonen. | Eigenschap | ✅ `InstallerProject.DirPageMode` (`DisablePageMode`; JSON-sleutel nog `AllowUserToChangeDir`) |
| (Bladerknop) | Tekst/tooltip/lettertype per taal van de Bladeren-knop. | Eigenschap | ✅ `InstallerProject.SelectDestinationBrowseButton` |
| (knoppen Terug/Volgende/Annuleren) | Tekst/tooltip/lettertype/zichtbaarheid per taal. | Eigenschap | ✅ `InstallerProject.SelectDestinationScreenButtons` |
| AllowNetworkDrive | Mag de map op een netwerkschijf staan. | Eigenschap | ⬜ |
| AllowRootDirectory | Mag de map de root van een schijf zijn. | Eigenschap | ⬜ |
| AllowUNCPath | Mag de gebruiker een UNC-pad invoeren. | Eigenschap | ⬜ |
| AppendDefaultDirName | Voegt de standaard mapnaam toe als de gebruiker bladert. | Eigenschap | ⬜ |
| CreateAppDir | Maakt Setup überhaupt een eigen applicatiemap aan. | Eigenschap | ⬜ |
| DirExistsWarning | Waarschuwing tonen als de gekozen map al bestaat. | Eigenschap | ⬜ |
| EnableDirDoesntExistWarning | Waarschuwing tonen als de gekozen map nog niet bestaat. | Eigenschap | ⬜ |
| UsePreviousAppDir | Onthoudt de eerder gekozen installatiemap bij een update. | Eigenschap | ✅ `InstallerProject.UsePreviousAppDir` (tabblad Overige instellingen) |

## 9. Wizardscherm: Select Components

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| [Components] | De lijst met selecteerbare onderdelen zelf. | Sectie-item | ⬜ (fase 5) |
| [Types] | De installatietypes (Full/Compact/Custom e.d.) waar componenten aan hangen. | Sectie-item | ⬜ (fase 5) |
| AlwaysShowComponentsList | Toont de componentenlijst ook bij niet-aangepaste installatietypes. | Eigenschap | ⬜ (fase 5) |
| FlatComponentsList | Platte in plaats van 3D-achtige checkboxstijl. | Eigenschap | ⬜ (fase 5) |
| ShowComponentSizes | Toont de grootte per component. | Eigenschap | ⬜ (fase 5) |
| UsePreviousSetupType | Onthoudt het eerder gekozen installatietype/componenten bij een update. | Eigenschap | ✅ `InstallerProject.UsePreviousSetupType` (tabblad Overige instellingen) |

## 10. Wizardscherm: Select Start Menu Folder

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| DisableProgramGroupPage | Aan/uit-schakeling, zie tabblad Schermen. | Eigenschap | ✅ (zie categorie 2) |
| DisableProgramGroupPage (gedrag) | Hoe de pagina zich gedraagt: altijd tonen (`no`), nooit tonen (`yes`) of overslaan als dezelfde applicatie al geïnstalleerd is (`auto`, standaard). | Eigenschap | ✅ `InstallerProject.GroupPageMode` (`DisablePageMode`) |
| DefaultGroupName | Standaard voorgestelde startmenugroep. | Eigenschap | ✅ `InstallerProject.DefaultGroupName` |
| AppendDefaultGroupName | Stuurt Inno Setup's eigen Bladeren-dialoog: kiest de gebruiker daar een bestaande map, dan plakt Setup de laatste component van `DefaultGroupName` erachter. | Eigenschap | ✅ `InstallerProject.AppendDefaultGroupName` |
| AlwaysUsePersonalGroup | Startmenugroep altijd voor de huidige gebruiker, nooit "Alle gebruikers". | Eigenschap | ✅ `InstallerProject.AlwaysUsePersonalGroup` |
| (Bladerknop) | Tekst/tooltip/lettertype/zichtbaarheid per taal van de Bladeren-knop (`WizardForm.GroupBrowseButton`). | Eigenschap | ✅ `InstallerProject.SelectProgramGroupBrowseButton` |
| (knoppen Terug/Volgende/Annuleren) | Tekst/tooltip/lettertype/zichtbaarheid per taal. | Eigenschap | ✅ `InstallerProject.SelectProgramGroupScreenButtons` |
| AllowNoIcons | Voegt een aanvinkvakje toe waarmee de eindgebruiker tijdens installatie zelf van alle snelkoppelingen kan afzien (apart van de bouwtijd-keuze `CreateStartMenuIcon`, zie categorie 19). | Eigenschap | ⬜ |
| UsePreviousGroup | Onthoudt de eerder gekozen startmenugroep bij een update. | Eigenschap | ✅ `InstallerProject.UsePreviousGroup` (tabblad Overige instellingen) |

## 11. Wizardscherm: Select Tasks

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| [Tasks] | De lijst met aan-/uit-vinkbare taken zelf (incl. de bureaublad-snelkoppeling-taak). | Sectie-item | 🔶 `InstallerProject.CreateDesktopIcon` dekt alleen de bureaublad-taak zelf; de generieke [Tasks]-lijst is fase 5/6 |
| ShowTasksTreeLines | Toont verbindingslijntjes tussen hoofd- en subtaken. | Eigenschap | ⬜ (fase 5) |
| UsePreviousTasks | Onthoudt de eerder aangevinkte taken bij een update. | Eigenschap | ✅ `InstallerProject.UsePreviousTasks` (tabblad Overige instellingen) |

## 12. Wizardscherm: Ready to Install

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| DisableReadyPage | Aan/uit-schakeling, zie tabblad Schermen. | Eigenschap | ✅ (zie categorie 2) |
| DisableReadyMemo | Verbergt de samenvattingstekst op deze pagina. | Eigenschap | ✅ `InstallerProject.DisableReadyMemo` |
| AlwaysShowDirOnReadyPage | Toont de gekozen installatiemap altijd in de samenvatting. | Eigenschap | ✅ `InstallerProject.AlwaysShowDirOnReadyPage` |
| AlwaysShowGroupOnReadyPage | Toont de gekozen startmenugroep altijd in de samenvatting. | Eigenschap | ✅ `InstallerProject.AlwaysShowGroupOnReadyPage` |
| (knoppen Terug/Volgende/Annuleren) | Tekst/tooltip/lettertype/zichtbaarheid per taal. | Eigenschap | ✅ `InstallerProject.ReadyScreenButtons` |
| UpdateReadyMemo | Pascal-event om de samenvattingstekst zelf samen te stellen. | Pascal | ⬜ (fase 6) |

## 13. Wizardscherm: Preparing to Install

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| CloseApplications | Sluit automatisch applicaties die bestanden gebruiken die Setup moet bijwerken. | Eigenschap | ⬜ (zie categorie 20, Herstart en lopende applicaties) |
| CloseApplicationsFilter | Bestandspatronen waarop Setup controleert of ze in gebruik zijn. | Eigenschap | ⬜ (categorie 20) |
| CloseApplicationsFilterExcludes | Uitzonderingen op CloseApplicationsFilter. | Eigenschap | ⬜ (categorie 20) |
| PrepareToInstall | Pascal-event om vereisten te controleren/installeren vóór de installatie start. | Pascal | ⬜ (fase 6) |
| RegisterExtraCloseApplicationsResources | Pascal-event om extra te controleren bestanden op te geven. | Pascal | ⬜ (fase 6) |

## 14. Wizardscherm: Installing

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| CurInstallProgressChanged | Pascal-event dat de voortgang tijdens het kopiëren volgt. | Pascal | ⬜ (fase 6) |
| CurStepChanged | Pascal-event voor eigen acties vóór/na installatie. | Pascal | ⬜ (fase 6) |

## 15. Wizardscherm: Info After

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| InfoAfterFile | Pad naar het leesmij-/infobestand na installatie. | Eigenschap | ✅ `InstallerProject.InfoAfterFilePath` |
| (knoppen Terug/Volgende/Annuleren) | Tekst/tooltip/lettertype/zichtbaarheid per taal. | Eigenschap | ✅ `InstallerProject.InfoAfterScreenButtons` |

## 16. Wizardscherm: Setup Completed

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| DisableFinishedPage | Aan/uit-schakeling, zie tabblad Schermen. | Eigenschap | ✅ (zie categorie 2) |
| WizardImageFile | Zelfde afbeelding als op de Welcome-pagina. | Eigenschap | ✅ (zie categorie 3) |
| AlwaysRestart | Vraagt altijd om een herstart, ook als dat niet strikt nodig is. | Eigenschap | ⬜ (categorie 20) |
| NeedRestart | Pascal-event om zelf te bepalen of een herstart nodig is. | Pascal | ⬜ (fase 6) |
| [Run] met postinstall-vlag | De "Programma starten"-checkbox en vergelijkbare acties na installatie. | Sectie-item | ⬜ (fase 6, zie categorie 28) |

## 17. Schermen algemeen (uiterlijk en gedrag van de wizard)

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| WizardStyle | Algehele visuele stijl van de wizard (classic/modern). | Eigenschap | ✅ `InstallerProject.WizardStyle` (alleen classic en modern; de overige stijlen en modifiers ⬜) |
| WizardStyleFile / WizardStyleFileDynamicDark | Eigen VCL-stijlbestand voor thema, incl. dark-mode-variant. | Eigenschap | ⬜ |
| WizardSizePercent | Schaalt het wizardvenster met een percentage. | Eigenschap | ⬜ |
| WizardResizable | *(verouderd, zie categorie 37)* | — | ➖ |
| DefaultDialogFontName | Standaardlettertype voor dialogen zonder taalspecifiek lettertype. | Eigenschap | ⬜ |
| WizardBackColor / WizardBackColorDynamicDark | Achtergrondkleur van wizardpagina's, incl. dark-mode-variant. | Eigenschap | ⬜ |
| WizardBackImageFile / WizardBackImageFileDynamicDark | Achtergrondafbeelding(en) op wizardpagina's, incl. dark-mode-variant. | Eigenschap | ⬜ |
| WizardBackImageOpacity | Doorzichtigheid van de achtergrondafbeelding. | Eigenschap | ⬜ |
| WizardImageAlphaFormat | Interpretatie van het alfakanaal in 32-bit wizardafbeeldingen. | Eigenschap | ⬜ |
| WizardImageBackColor / WizardImageBackColorDynamicDark | Achtergrondkleur achter transparante delen van de wizardafbeelding. | Eigenschap | ⬜ |
| WizardImageOpacity | Doorzichtigheid van de wizardafbeelding zelf. | Eigenschap | ⬜ |
| WizardImageStretch | Uitrekken vs. bijsnijden bij een formaatverschil. | Eigenschap | ⬜ |
| WizardKeepAspectRatio | Beeldverhouding behouden bij schalen. | Eigenschap | ⬜ |
| WizardSmallImageBackColor / WizardSmallImageBackColorDynamicDark | Achtergrondkleur achter transparante delen van de kleine wizardafbeelding. | Eigenschap | ⬜ |
| WizardSmallImageFile / WizardSmallImageFileDynamicDark | Afbeelding rechtsboven op de overige wizardpagina's, incl. dark-mode-variant. | Eigenschap | ✅ `InstallerProject.WizardSmallImageFile` (alleen lichte variant) |
| AllowCancelDuringInstall | Mag de gebruiker tijdens installatie annuleren. | Eigenschap | ⬜ |
| SetupLogging | Maakt automatisch een logbestand van de installatie. | Eigenschap | ⬜ |
| RestartIfNeededByRun | Detecteert herstartbehoefte door [Run]-vermeldingen die bestanden voor herstart klaarzetten. | Eigenschap | ⬜ (zie ook categorie 20) |
| TerminalServicesAware | Houdt rekening met Terminal Services bij speciale-mappen-paden. | Eigenschap | ⬜ |
| UseSetupLdr | Eén bestand vs. meerdere bestanden (Setup Loader) voor de gecompileerde installer. | Eigenschap | ⬜ |
| UsedUserAreasWarning | Onderdrukt de waarschuwing over per-gebruiker-installatiegebieden. | Eigenschap | ⬜ |
| ShowLanguageDialog | Toont een taalkeuzedialoog bij opstarten van Setup. | Eigenschap | ⬜ (zie ook categorie 18, Talen) |
| LanguageDetectionMethod | Bepaalt hoe Setup automatisch een standaardtaal kiest. | Eigenschap | ⬜ (zie ook categorie 18, Talen) |
| InitializeWizard | Pascal-event om de wizard/wizardpagina's bij opstarten aan te passen. | Pascal | 🔶 gegenereerd voor knopinstellingen (Bladeren-knoppen en beginwaarden); eigen code volgt in fase 6 |
| ShouldSkipPage | Pascal-event om een wizardpagina over te slaan. | Pascal | ⬜ (fase 6) |
| CurPageChanged | Pascal-event dat vuurt nadat een nieuwe pagina getoond is. | Pascal | 🔶 gegenereerd voor knopinstellingen per scherm; eigen code volgt in fase 6 |
| NextButtonClick / BackButtonClick / CancelButtonClick | Pascal-events voor eigen navigatielogica. | Pascal | ⬜ (fase 6) |
| InitializeSetup / DeinitializeSetup | Pascal-events bij start/einde van Setup. | Pascal | ⬜ (fase 6) |

## 18. Tabblad: Talen

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| [Languages] (Name) | Welke talen de installer aanbiedt. | Eigenschap | ✅ `InstallerProject.SupportedLanguageIds` |
| ShowLanguageDialog | Toont een taalkeuzedialoog bij opstarten. | Eigenschap | ⬜ |
| LanguageDetectionMethod | Hoe Setup automatisch een standaardtaal kiest. | Eigenschap | ⬜ |
| UsePreviousLanguage | Onthoudt de eerder gekozen installertaal bij een update. | Eigenschap | ✅ `InstallerProject.UsePreviousLanguage` (tabblad Overige instellingen) |
| (knopteksten per taal) | Vertaalde Terug/Volgende/Annuleren/Bladeren-teksten en tooltips. | Eigenschap | ✅ `WizardScreenButtonSettings`/`BrowseButtonSettings` `*ByLanguage`-dictionaries, gegenereerd via `[CustomMessages]` (stap 4) |
| [Languages] MessagesFile | Welk(e) .isl-bestand(en) de standaardteksten voor een taal levert. | Eigenschap | ⬜ |
| [Languages] LicenseFile | Taalspecifiek licentiebestand, overschrijft het algemene LicenseFile. | Eigenschap | ⬜ |
| [Languages] InfoBeforeFile / InfoAfterFile | Taalspecifieke Info Before/After-bestanden. | Eigenschap | ⬜ |
| [LangOptions] | Lettertype, LCID, RightToLeft e.d. per taal. | Eigenschap | ⬜ (zie categorie 31) |
| [Messages] / [CustomMessages] | Alle overige, losse interfaceteksten per taal. | Sectie-item | 🔶 `[CustomMessages]` gegenereerd voor knopteksten en tooltips; `[Messages]` en eigen berichten ⬜ (zie categorie 31) |

## 19. Tabblad: Overige instellingen

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| (eigen veld, geen directe Inno-richtlijn) | Of de generator een startmenu-snelkoppeling opneemt in `[Icons]`. Geen tegenhanger van `AllowNoIcons` (dat is de eindgebruiker-opt-out tijdens installatie, zie categorie 10). | Eigenschap | ✅ `InstallerProject.CreateStartMenuIcon` |
| (desktopicon-taak) | Optionele bureaublad-snelkoppeling-taak. | Eigenschap | ✅ `InstallerProject.CreateDesktopIcon` |
| UsePreviousAppDir | Onthoudt eerder gekozen installatiemap. | Eigenschap | ✅ `InstallerProject.UsePreviousAppDir` |
| UsePreviousGroup | Onthoudt eerder gekozen startmenugroep. | Eigenschap | ✅ `InstallerProject.UsePreviousGroup` |
| UsePreviousSetupType | Onthoudt eerder gekozen installatietype. | Eigenschap | ✅ `InstallerProject.UsePreviousSetupType` |
| UsePreviousTasks | Onthoudt eerder aangevinkte taken. | Eigenschap | ✅ `InstallerProject.UsePreviousTasks` |
| UsePreviousLanguage | Onthoudt eerder gekozen installertaal. | Eigenschap | ✅ `InstallerProject.UsePreviousLanguage` |

Zie sectie 25 van `Architectuur-en-Ontwerp.md` voor de volledige toelichting op dit tabblad. De overige instellingen uit Herberts Inno Script Studio-referentie (categorieën 20-26 hieronder) horen hier op termijn mogelijk ook bij, of krijgen een eigen tabblad — nog te beslissen.

## 20. Tabblad (toekomst): Herstart en lopende applicaties

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| CloseApplications | Sluit automatisch applicaties die bestanden gebruiken die Setup moet bijwerken. | Eigenschap | ⬜ |
| CloseApplicationsFilter | Bestandspatronen waarop gecontroleerd wordt of ze in gebruik zijn. | Eigenschap | ⬜ |
| CloseApplicationsFilterExcludes | Uitzonderingen op CloseApplicationsFilter. | Eigenschap | ⬜ |
| RestartApplications | Herstart automatisch gesloten applicaties na installatie. | Eigenschap | ⬜ |
| AppMutex | Mutex(en) waarmee Setup detecteert dat de applicatie draait. | Eigenschap | ⬜ |
| SetupMutex | Mutex(en) om meerdere gelijktijdige Setup-instanties te voorkomen. | Eigenschap | ⬜ |
| AlwaysRestart | Vraagt altijd om een herstart na installatie. | Eigenschap | ⬜ |
| NeedRestart | Pascal-event om zelf te bepalen of een herstart nodig is. | Pascal | ⬜ (fase 6) |
| UninstallNeedRestart | Zelfde, maar dan voor de uninstaller. | Pascal | ⬜ (fase 6) |
| RegisterExtraCloseApplicationsResources | Pascal-event om extra te controleren bestanden op te geven. | Pascal | ⬜ (fase 6) |

## 21. Tabblad (toekomst): Uninstall-instellingen

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| Uninstallable | Bepaalt of Setup überhaupt uninstall-functionaliteit meeneemt. | Eigenschap | ⬜ |
| CreateUninstallRegKey | Registreert de app in Programma's en onderdelen. | Eigenschap | ⬜ |
| UninstallDisplayIcon | Icoon voor de vermelding in Programma's en onderdelen. | Eigenschap | ✅ vaste waarde van de generator: `{app}\<hoofdprogramma>` (nog geen eigen veld) |
| UninstallDisplayName | Weergavenaam in Programma's en onderdelen. | Eigenschap | ✅ vaste waarde van de generator: de `AppName` (nog geen eigen veld) |
| UninstallDisplaySize | Weergegeven installatiegrootte, automatisch of handmatig. | Eigenschap | ⬜ |
| UninstallFilesDir | Map waar uninstaller-ondersteuningsbestanden komen. | Eigenschap | ⬜ |
| UninstallLogging | Maakt automatisch een logbestand bij het verwijderen. | Eigenschap | ⬜ |
| UninstallLogMode | Aanvullen, overschrijven of nieuw logbestand bij verwijderen. | Eigenschap | ⬜ |
| UninstallRestartComputer | Vraagt om herstart na het verwijderen. | Eigenschap | ⬜ |
| UpdateUninstallLogAppName | Werkt de appnaam in het uninstall-log bij een herinstallatie bij. | Eigenschap | ⬜ |
| InitializeUninstall / DeinitializeUninstall | Pascal-events bij start/einde van Uninstall. | Pascal | ⬜ (fase 6) |
| CurUninstallStepChanged | Pascal-event voor eigen acties tijdens het verwijderen. | Pascal | ⬜ (fase 6) |
| InitializeUninstallProgressForm | Pascal-event om het voortgangsscherm van Uninstall aan te passen. | Pascal | ⬜ (fase 6) |

## 22. Tabblad (toekomst): Versie-informatie

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| AppCopyright | Standaard copyrighttekst. | Eigenschap | ⬜ |
| VersionInfoCompany | Bedrijfsnaamveld in de versie-informatie. | Eigenschap | ⬜ |
| VersionInfoCopyright | Copyrightveld in de versie-informatie. | Eigenschap | ⬜ |
| VersionInfoDescription | Bestandsomschrijvingveld in de versie-informatie. | Eigenschap | ⬜ |
| VersionInfoOriginalFileName | Oorspronkelijke-bestandsnaamveld. | Eigenschap | ⬜ |
| VersionInfoProductName | Productnaamveld. | Eigenschap | ⬜ |
| VersionInfoProductTextVersion | Tekstuele productversie. | Eigenschap | ⬜ |
| VersionInfoProductVersion | Binaire productversie. | Eigenschap | ⬜ |
| VersionInfoTextVersion | Tekstuele bestandsversie. | Eigenschap | ⬜ |
| VersionInfoVersion | Binaire bestandsversie (tot 4 cijfergroepen). | Eigenschap | ⬜ |

## 23. Tabblad (toekomst): Systeemeisen en architectuur

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| MinVersion | Minimaal vereiste Windows-versie. | Eigenschap | ⬜ |
| OnlyBelowVersion | Maximale Windows-versie waarboven Setup weigert te draaien. | Eigenschap | ⬜ |
| ArchitecturesAllowed | Toegestane processorarchitecturen. | Eigenschap | ⬜ |
| ArchitecturesInstallIn64BitMode | Welke architecturen een 64-bit installatie triggeren. | Eigenschap | ✅ `InstallerProject.Architecture` (alleen x86 of x64; arm64 ⬜) |
| SetupArchitecture | Of de gecompileerde Setup zelf 32-bit of 64-bit draait. | Eigenschap | ⬜ |
| PrivilegesRequired | Vereist Setup beheerdersrechten. | Eigenschap | ⬜ |
| PrivilegesRequiredOverridesAllowed | Mag het rechtenniveau via command-line/dialoog overruled worden. | Eigenschap | ⬜ |
| UsePreviousPrivileges | Onthoudt het eerder gekozen rechtenniveau bij een update. | Eigenschap | ⬜ |

## 24. Tabblad (toekomst): Beveiliging

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| Password | Wachtwoord vereist bij opstarten van Setup. | Eigenschap | ⬜ |
| Encryption | Schakelt bestandsversleuteling (XChaCha20) in, gekoppeld aan Password. | Eigenschap | ⬜ |
| EncryptionKeyDerivation | Functie/iteraties om de encryptiesleutel uit het wachtwoord af te leiden. | Eigenschap | ⬜ |
| RedirectionGuard | Beschermt tegen onveilige junction/symlink-paden. | Eigenschap | ⬜ |
| ASLRCompatible | Dynamic Base (ASLR)-vlag in Setup/Uninstall. | Eigenschap | ⬜ |
| DEPCompatible | NX Compatible (DEP)-vlag in Setup. | Eigenschap | ⬜ |
| DisablePrecompiledFileVerifications | Slaat verificatie van opgegeven voorgecompileerde bestanden over. | Eigenschap | ⬜ |
| CheckPassword | Pascal-event om een wachtwoord zelf te valideren. | Pascal | ⬜ (fase 6, zie ook categorie 5) |

## 25. Tabblad (toekomst): Code signing

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| SignTool | Naam/parameters van de digitale ondertekeningstool. | Eigenschap | ⬜ |
| SignToolMinimumTimeBetween | Minimale wachttijd tussen ondertekeningen. | Eigenschap | ⬜ |
| SignToolRetryCount | Aantal nieuwe pogingen bij een mislukte ondertekening. | Eigenschap | ⬜ |
| SignToolRetryDelay | Wachttijd vóór een nieuwe ondertekeningspoging. | Eigenschap | ⬜ |
| SignToolRunMinimized | Draait de ondertekeningstool geminimaliseerd. | Eigenschap | ⬜ |
| SignedUninstaller | Ondertekent de uninstaller digitaal. | Eigenschap | ⬜ |
| SignedUninstallerDir | Locatie van het ondertekende uninstaller-bestand. | Eigenschap | ⬜ |
| --signtool / -s (ISCC) | Benoemt een Sign Tool op de command-line. | CLI | ⬜ (fase 7, zie categorie 36) |
| --no-signing / --no-signcheck (ISCC) | Schakelt ondertekening/signcheck tijdelijk uit. | CLI | ⬜ (fase 7, zie categorie 36) |

## 26. Tabblad (toekomst): Compiler en compressie

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| Compression | Compressiemethode en -niveau. | Eigenschap | ✅ vaste waarde van de generator: `lzma2` (nog geen eigen veld) |
| CompressionThreads | Multi-threaded compressie voor LZMA2. | Eigenschap | ⬜ |
| InternalCompressLevel | Compressieniveau van Setup's interne datastructuren. | Eigenschap | ⬜ |
| LZMAAlgorithm | Snel vs. normaal LZMA-algoritme. | Eigenschap | ⬜ |
| LZMABlockSize | Blokgrootte voor multi-threaded LZMA2-compressie. | Eigenschap | ⬜ |
| LZMADictionarySize | Woordenboekgrootte, afweging compressieratio vs. snelheid/geheugen. | Eigenschap | ⬜ |
| LZMAMatchFinder | Match-finder-algoritme voor LZMA. | Eigenschap | ⬜ |
| LZMANumBlockThreads | Aantal parallelle threads voor LZMA2-compressie. | Eigenschap | ⬜ |
| LZMANumFastBytes | "Fast bytes"-parameter, snelheid vs. efficiëntie. | Eigenschap | ⬜ |
| LZMAUseSeparateProcess | Compressie in een apart proces. | Eigenschap | ⬜ |
| SolidCompression | Alle bestanden samen als één blok comprimeren. | Eigenschap | ✅ vaste waarde van de generator: `yes` (nog geen eigen veld) |
| DiskClusterSize | Clustergrootte voor het opvullen van losse schijven. | Eigenschap | ⬜ |
| DiskSliceSize | Maximale grootte per schijf-slice/.bin-bestand. | Eigenschap | ⬜ |
| DiskSpanning | Verdeelt gecomprimeerde data over meerdere schijven. | Eigenschap | ⬜ |
| ReserveBytes | Gereserveerde vrije ruimte op de eerste schijf. | Eigenschap | ⬜ |
| SlicesPerDisk | Aantal slice-bestanden per schijf. | Eigenschap | ⬜ |
| Output | Genereert de compiler daadwerkelijk bestanden, of alleen een foutencontrole. | Eigenschap | ⬜ |
| OutputBaseFilename | Bestandsnaam van de gecompileerde installer. | Eigenschap | ✅ `InstallerProject.OutputBaseFilename` (leeg = `<AppName>-<AppVersion>-Setup`, zie `GetEffectiveOutputBaseFilename`) |
| OutputManifestFile | Genereert een manifestbestand met de outputbestanden. | Eigenschap | ⬜ |
| ChangesAssociations | Meldt dat Setup bestandskoppelingen wijzigt (Explorer-refresh). | Eigenschap | ⬜ |
| ChangesEnvironment | Meldt draaiende applicaties dat omgevingsvariabelen gewijzigd zijn. | Eigenschap | ⬜ |
| ArchiveExtraction | Welke archiefextractiemethode gebruikt wordt. | Eigenschap | ⬜ |
| MergeDuplicateFiles | Identieke bestanden eenmalig opslaan. | Eigenschap | ⬜ |
| MissingMessagesWarning | Waarschuwt bij ontbrekende taalteksten. | Eigenschap | ⬜ |
| MissingRunOnceIdsWarning | Waarschuwt bij ontbrekende RunOnceId's. | Eigenschap | ⬜ |
| NotRecognizedMessagesWarning | Waarschuwt bij onbekende/aangepaste berichtsleutels. | Eigenschap | ⬜ |

## 27. Sectie: Bestanden en bronnen ([Files]/[Dirs]/[InstallDelete]/[UninstallDelete])

Alle vier bewust in fase 6 (generator), omdat ze stuk voor stuk rijen in een lijst zijn (sectie-items), niet losse eigenschappen. Hangt samen met [Setup]-richtlijnen `TimeStampRounding`, `TimeStampsInUTC`, `TouchDate`, `TouchTime` (beïnvloeden bestandstijdstempels, horen bij dezelfde toekomstige editor).

### [Files] — parameters

| Parameter | Omschrijving | Status |
|---|---|---|
| Source | Bronbestand of wildcard. | ⬜ |
| DestDir | Doelmap op het doelsysteem. | ⬜ |
| DestName | Nieuwe bestandsnaam op het doelsysteem. | ⬜ |
| Excludes | Uit te sluiten wildcardpatronen. | ⬜ |
| ExternalSize | Grootte van een extern bestand (met flag `external`). | ⬜ |
| Attribs | Extra bestandsattributen (readonly/hidden/system/notcontentindexed). | ⬜ |
| Permissions | Extra ACL-rechten. | ⬜ |
| FontInstall | Markeert het bestand als lettertype. | ⬜ |
| StrongAssemblyName | Sterke assembly-naam, gebruikt door de uninstaller. | ⬜ |
| ISSigAllowedKeys | Toegestane sleutel(s) voor issigverify. | ⬜ |
| Hash | SHA-256-hash om te verifiëren i.p.v. een volledige handtekeningcontrole. | ⬜ |
| ExtractArchivePassword | Wachtwoord voor het te extraheren archief. | ⬜ |
| DownloadISSigSource | URL van het .issig-handtekeningbestand. | ⬜ |
| DownloadUserName / DownloadPassword | Basisauthenticatie voor het downloaden van het bestand. | ⬜ |

### [Files] — Flags

| Flag | Omschrijving | Status |
|---|---|---|
| 32bit / 64bit | Dwingt 32-bit of 64-bit gedrag af voor deze rij. | ⬜ |
| allowunsafefiles | Schakelt de automatische onveilig-bestand-controle uit. | ⬜ |
| comparetimestamp | Vergelijkt tijdstempels i.p.v. versie-info. | ⬜ |
| confirmoverwrite | Vraagt altijd bevestiging vóór overschrijven. | ⬜ |
| createallsubdirs | Maakt alle submappen van DestDir aan. | ⬜ |
| deleteafterinstall | Installeert en verwijdert daarna weer. | ⬜ |
| dontcopy | Compileert mee, maar kopieert niet tijdens normale installatie. | ⬜ |
| dontverifychecksum | Slaat checksumcontrole na extractie over. | ⬜ |
| download | Downloadt het bestand i.p.v. het mee te compileren. | ⬜ |
| external | Kopieert vanaf een externe locatie. | ⬜ |
| extractarchive | Extraheert de bron als archief. | ⬜ |
| fontisnttruetype | Markeert een FontInstall-lettertype als niet-TrueType. | ⬜ |
| gacinstall | Installeert in de .NET Global Assembly Cache. | ⬜ |
| ignoreversion | Overschrijft ongeacht versienummer. | ⬜ |
| issigverify | Verifieert de handtekening van het bronbestand. | ⬜ |
| isreadme | Markeert als het installatie-leesmij-bestand. | ⬜ |
| nocompression | Comprimeert dit bestand niet. | ⬜ |
| noencryption | Slaat onversleuteld op, ook bij ingeschakelde encryptie. | ⬜ |
| notimestamp | Slaat op zonder tijdstempel (reproduceerbare builds). | ⬜ |
| noregerror | Onderdrukt foutmeldingen bij mislukte regserver/regtypelib. | ⬜ |
| onlyifdestfileexists | Alleen installeren als er al een gelijknamig bestand bestaat. | ⬜ |
| onlyifdoesntexist | Alleen installeren als het bestand nog niet bestaat. | ⬜ |
| overwritereadonly | Overschrijft ook alleen-lezen bestanden. | ⬜ |
| promptifolder | Vraagt de gebruiker of een bestaand bestand vervangen mag worden. | ⬜ |
| recursesubdirs | Doorzoekt ook submappen van Source. | ⬜ |
| regserver / regtypelib | Registreert de DLL/OCX resp. typebibliotheek. | ⬜ |
| replacesameversion | Vervangt gelijke versies als de inhoud echt verschilt. | ⬜ |
| restartreplace | Vervangt het bestand pas bij de eerstvolgende herstart. | ⬜ |
| setntfscompression / unsetntfscompression | Schakelt NTFS-compressie aan/uit op het bestand. | ⬜ |
| sharedfile | Markeert als gedeeld bestand (refcount bij verwijderen). | ⬜ |
| sign / signcheck / signonce | Ondertekent het bronbestand resp. controleert/voorkomt dubbel ondertekenen. | ⬜ |
| skipifsourcedoesntexist | Slaat de rij stilzwijgend over als de bron ontbreekt. | ⬜ |
| solidbreak | Start een nieuwe compressiestroom vóór dit bestand. | ⬜ |
| sortfilesbyextension / sortfilesbyname | Sorteert gematchte bestanden op extensie resp. naam. | ⬜ |
| touch | Zet de tijdstempel volgens TouchDate/TouchTime. | ⬜ |
| uninsnosharedfileprompt | Verwijdert een gedeeld bestand bij refcount nul zonder te vragen. | ⬜ |
| uninsremovereadonly | Haalt alleen-lezen weg vóór verwijderen bij uninstall. | ⬜ |
| uninsrestartdelete | Plant verwijdering bij de eerstvolgende herstart. | ⬜ |
| uninsneveruninstall | Verwijdert dit bestand nooit bij uninstall. | ⬜ |

### [Dirs]

| Parameter / Flag | Omschrijving | Status |
|---|---|---|
| Name | Aan te maken map. | ⬜ |
| Attribs | Extra mapattributen. | ⬜ |
| Permissions | Extra ACL-rechten op de map. | ⬜ |
| Flags: deleteafterinstall | Maakt de map aan en verwijdert die na installatie als die leeg is. | ⬜ |
| Flags: setntfscompression / unsetntfscompression | Schakelt NTFS-compressie aan/uit op de map. | ⬜ |
| Flags: uninsalwaysuninstall | Probeert de map altijd te verwijderen bij uninstall als die leeg is. | ⬜ |
| Flags: uninsneveruninstall | Verwijdert de map nooit bij uninstall. | ⬜ |

### [InstallDelete] / [UninstallDelete]

Identiek van vorm — het eerste wist bestanden vóór installatie, het tweede bij het verwijderen.

| Parameter / Type-waarde | Omschrijving | Status |
|---|---|---|
| Type: files | Verwijdert een specifiek bestand of bestanden via wildcard. | ⬜ |
| Type: filesandordirs | Zoals files, maar verwijdert ook mappen recursief. | ⬜ |
| Type: dirifempty | Verwijdert de map alleen als die leeg is. | ⬜ |
| Name | Bestands-/mapnaam of wildcard om te verwijderen. | ⬜ |

## 28. Sectie: Snelkoppelingen en programma's uitvoeren ([Icons]/[Run]/[UninstallRun])

### [Icons]

| Parameter / Flag | Omschrijving | Status |
|---|---|---|
| Name | Naam en locatie van de snelkoppeling. | ⬜ |
| Filename | Doelbestand van de snelkoppeling. | ⬜ |
| Parameters | Command-line-argumenten. | ⬜ |
| WorkingDir | Werkmap ("Start in"). | ⬜ |
| HotKey | Sneltoetscombinatie. | ⬜ |
| Comment | Tooltip-tekst van de snelkoppeling. | ⬜ |
| IconFilename / IconIndex | Eigen icoonbestand en index daarin. | ⬜ |
| AppUserModelID | Application User Model ID (Windows 7+). | ⬜ |
| AppUserModelToastActivatorCLSID | Toast Activator CLSID (Windows 10+). | ⬜ |
| Flags: closeonexit / dontcloseonexit | "Close on Exit" voor MS-DOS-snelkoppelingen. | ⬜ |
| Flags: createonlyiffileexists | Alleen aanmaken als Filename bestaat. | ⬜ |
| Flags: excludefromshowinnewinstall | Voorkomt uitlichten in het Start-menu (Windows 7+). | ⬜ |
| Flags: preventpinning | Voorkomt vastpinnen aan taakbalk/Startmenu. | ⬜ |
| Flags: runmaximized / runminimized | Start gemaximaliseerd resp. geminimaliseerd. | ⬜ |
| Flags: uninsneveruninstall | Uninstaller verwijdert deze snelkoppeling niet. | ⬜ |
| Flags: useapppaths | Lost Filename op via de App Paths-registersleutel. | ⬜ |

### [Run]

| Parameter / Flag | Omschrijving | Status |
|---|---|---|
| Filename | Programma of bestand/map om te openen. | ⬜ |
| Parameters | Command-line-argumenten. | ⬜ |
| WorkingDir | Werkmap van het gestarte proces. | ⬜ |
| Description | Label van de postinstall-checkbox. | ⬜ |
| StatusMsg | Statusbericht tijdens het draaien. | ⬜ |
| OnLog | Pascal-procedure die procesoutput ontvangt. | ⬜ (grenst aan Pascal, fase 6) |
| Verb | Shell-actie bij shellexec. | ⬜ |
| Flags: postinstall | Checkbox op de Setup Completed-pagina. | ⬜ (zie ook categorie 16) |
| Flags: unchecked | Postinstall-checkbox start uitgevinkt. | ⬜ |
| Flags: shellexec | Opent via de shell-geassocieerde applicatie. | ⬜ |
| Flags: nowait / waituntilidle / waituntilterminated | Wachtgedrag op het gestarte proces. | ⬜ |
| Flags: runhidden / runmaximized / runminimized | Venstergedrag van het gestarte proces. | ⬜ |
| Flags: hidewizard | Verbergt het wizardvenster tijdens het draaien. | ⬜ |
| Flags: runascurrentuser / runasoriginaluser | Rechten waarmee het proces draait. | ⬜ |
| Flags: skipifdoesntexist | Onderdrukt fout als Filename ontbreekt. | ⬜ |
| Flags: skipifsilent / skipifnotsilent | Overslaan afhankelijk van (very) silent-modus. | ⬜ |
| Flags: dontlogparameters / logoutput | Loggedrag van deze rij. | ⬜ |
| Flags: 32bit / 64bit | Dwingt bitness af (niet te combineren met shellexec). | ⬜ |

### [UninstallRun]

Zelfde idee als [Run], maar dan tijdens het verwijderen; geen Description/StatusMsg/OnLog/Verb/postinstall/unchecked/skipifnotsilent/skipifsilent/runasoriginaluser.

| Parameter / Flag | Omschrijving | Status |
|---|---|---|
| Filename / Parameters / WorkingDir | Zelfde betekenis als bij [Run]. | ⬜ |
| RunOnceId | Zorgt dat deze rij over alle uninstall-runs heen maar één keer draait. | ⬜ |
| Flags | Grotendeels gedeeld met [Run] (shellexec, wachtgedrag, venstergedrag, bitness, logging). | ⬜ |

## 29. Sectie: Registry en INI

### [Registry]

| Parameter | Omschrijving | Status |
|---|---|---|
| Root | Registerhive (zie waarden hieronder). | ⬜ |
| Subkey | Pad van de registersleutel. | ⬜ |
| ValueType | Type waarde (zie waarden hieronder). | ⬜ |
| ValueName | Naam van de waarde (leeg = standaardwaarde). | ⬜ |
| ValueData | Toe te kennen data, inclusief `{olddata}`/`{break}`. | ⬜ |
| Permissions | Extra ACL-rechten op de sleutel. | ⬜ |

| Root-waarde | Omschrijving | Status |
|---|---|---|
| HKCR / HKCU / HKLM / HKU / HKCC | De bekende registerhives. | ⬜ |
| HKA | HKLM in beheerdersmodus, anders HKCU. | ⬜ |
| HKxx32 / HKxx64 | Hive geforceerd naar de 32-bit resp. 64-bit registerweergave. | ⬜ |

| ValueType-waarde | Omschrijving | Status |
|---|---|---|
| none | Alleen de sleutel aanmaken, geen waarde. | ⬜ |
| string / expandsz / multisz | REG_SZ / REG_EXPAND_SZ / REG_MULTI_SZ. | ⬜ |
| dword / qword | 32-bit resp. 64-bit integer. | ⬜ |
| binary | REG_BINARY. | ⬜ |

| Flag | Omschrijving | Status |
|---|---|---|
| createvalueifdoesntexist | Alleen zetten als de waarde nog niet bestaat. | ⬜ |
| deletekey / deletevalue | Verwijdert eerst de sleutel resp. waarde vóór het (opnieuw) aanmaken. | ⬜ |
| dontcreatekey | Maakt de sleutel niet aan als die nog niet bestaat. | ⬜ |
| noerror | Negeert fouten bij deze rij. | ⬜ |
| preservestringtype | Behoudt het bestaande REG_SZ/REG_EXPAND_SZ-type. | ⬜ |
| uninsclearvalue | Leegt (i.p.v. verwijdert) de waarde bij uninstall. | ⬜ |
| uninsdeletekey / uninsdeletekeyifempty | Verwijdert de sleutel bij uninstall, eventueel alleen als die leeg is. | ⬜ |
| uninsdeletevalue | Verwijdert de waarde bij uninstall. | ⬜ |

### [INI]

| Parameter / Flag | Omschrijving | Status |
|---|---|---|
| Filename | Het te wijzigen .ini-bestand. | ⬜ |
| Section / Key / String | Sectie, sleutel en waarde om te zetten. | ⬜ |
| Flags: createkeyifdoesntexist | Alleen zetten als de sleutel nog niet bestaat. | ⬜ |
| Flags: uninsdeleteentry | Verwijdert deze vermelding bij uninstall. | ⬜ |
| Flags: uninsdeletesection / uninsdeletesectionifempty | Verwijdert de hele sectie bij uninstall, eventueel alleen als leeg. | ⬜ |

## 30. Sectie: Taken, componenten en installatietypes ([Tasks]/[Types]/[Components])

Gedeelde parameters `Components`/`Tasks`/`Languages`/`MinVersion`/`OnlyBelowVersion` gelden voor meerdere van deze secties; hieronder per sectie genoemd waar relevant.

### [Tasks]
| Parameter / Flag | Omschrijving | Status |
|---|---|---|
| Name | Interne naam; `\`/`/` bepaalt bovenliggende/onderliggende taken. | 🔶 (alleen de eigen bureaublad-taak zelf is nu gedekt, een generieke takenlijst nog niet) |
| Description / GroupDescription | Tekst op de Select Tasks-pagina resp. groepslabel. | ⬜ |
| Components | Alleen tonen als een van deze componenten gekozen is. | ⬜ |
| Flags: checkablealone | Mag aangevinkt zijn zonder dat een subtaak aangevinkt is. | ⬜ |
| Flags: checkedonce | Start uitgevinkt als er al een vorige installatie gedetecteerd is. | ⬜ |
| Flags: dontinheritcheck | Subtaak vinkt niet automatisch mee met de hoofdtaak. | ⬜ |
| Flags: exclusive | Wederzijds uitsluitend met andere taken met deze flag. | ⬜ |
| Flags: restart | Vraagt om herstart als deze taak gekozen is. | ⬜ |
| Flags: unchecked | Start uitgevinkt. | ⬜ |

### [Types]

| Parameter / Flag | Omschrijving | Status |
|---|---|---|
| Name / Description | Interne naam resp. getoonde tekst van het installatietype. | ⬜ |
| Flags: iscustom | Markeert het "aangepast"-type (precies één toegestaan). | ⬜ |

### [Components]

| Parameter / Flag | Omschrijving | Status |
|---|---|---|
| Name | Interne naam; `\`/`/` bepaalt hiërarchie, zelfde schema als Tasks. | ⬜ |
| Description | Getoonde tekst op de Select Components-pagina. | ⬜ |
| Types | Welke installatietypes dit component meenemen. | ⬜ |
| ExtraDiskSpaceRequired | Extra gerapporteerde schijfruimte boven de eigen bestanden. | ⬜ |
| Flags: checkablealone / dontinheritcheck | Zelfde betekenis als bij Tasks. | ⬜ |
| Flags: exclusive | Wederzijds uitsluitend met andere componenten met deze flag. | ⬜ |
| Flags: fixed | Gebruiker kan dit component niet handmatig (de)selecteren. | ⬜ |
| Flags: restart | Vraagt om herstart als dit component geïnstalleerd wordt. | ⬜ |
| Flags: disablenouninstallwarning | Onderdrukt de "wordt niet verwijderd"-waarschuwing. | ⬜ |

Gedeelde parameters (Tasks/Types/Components): `Languages`, `MinVersion`, `OnlyBelowVersion` — beperkt een rij tot bepaalde talen resp. Windows-versies. ⬜

## 31. Taalbestanden per taal ([Languages]-bestanden, [LangOptions], [Messages], [CustomMessages])

| Richtlijn | Omschrijving | Status |
|---|---|---|
| [Languages] MessagesFile / LicenseFile / InfoBeforeFile / InfoAfterFile | Taalspecifieke bestanden (zie ook categorie 18). | ⬜ |
| LangOptions: LanguageName / LanguageID / LanguageCodePage | Weergavenaam, LCID en codepage van de taal. | ⬜ |
| LangOptions: DialogFontName / DialogFontSize | Lettertype en -grootte voor dialogen in deze taal. | ⬜ |
| LangOptions: DialogFontBaseScaleWidth / DialogFontBaseScaleHeight | Basis-schaalwaarden voor lettertype/lay-outberekeningen. | ⬜ |
| LangOptions: WelcomeFontName / WelcomeFontSize | Lettertype en -grootte van de grote kop op Welcome/Finished. | ⬜ |
| LangOptions: RightToLeft | Markeert de taal als rechts-naar-links. | ⬜ |
| [Messages] | Overschrijft elke berichtsleutel uit Default.isl (100+ stuks), plus BeveledLabel en HelpTextNote. | ⬜ |
| [CustomMessages] | Eigen Key=Tekst-paren, aan te roepen via `{cm:KeyName}`. | 🔶 de generator schrijft knopteksten en tooltips als `Btn...`-berichten met `CustomMessage()` (sectie 32 Architectuur-en-Ontwerp.md); eigen berichten van de gebruiker ⬜ |

## 32. Pascal Script: Setup Event Functions

Alle hieronder genoemde functies zijn optionele event-functies die in de `[Code]`-sectie van een `.iss`-bestand gedefinieerd kunnen worden; Inno Setup roept ze aan tijdens Setup.exe als ze aanwezig zijn. Dit hoort bij fase 6 (Pascal-code-editor) uit de roadmap — tot die fase gebouwd is, is dit alleen mogelijk via de (nog te bouwen) vrije scripttekst, niet via een los scherm/eigenschap.

| Functie | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| InitializeSetup | Aangeroepen direct bij start van Setup, vóór enig scherm. Kan `False` teruggeven om Setup af te breken. | Pascal | 🔶 (fase 6) |
| InitializeWizard | Aangeroepen na InitializeSetup, voordat de wizard getoond wordt — meestal gebruikt om eigen pagina's aan de wizard toe te voegen. | Pascal | 🔶 gegenereerd voor knopinstellingen; eigen code (fase 6) |
| DeinitializeSetup | Aangeroepen vlak voordat Setup.exe afsluit (ook na annuleren of een fout). | Pascal | 🔶 (fase 6) |
| CurStepChanged | Aangeroepen bij elke overgang tussen installatiestappen (ssInstall, ssPostInstall, enzovoort) — de meest gebruikte hook voor eigen installatielogica. | Pascal | 🔶 (fase 6) |
| CurInstallProgressChanged | Aangeroepen telkens als de voortgangsbalk op de Installing-pagina verandert. | Pascal | 🔶 (fase 6) |
| NextButtonClick | Aangeroepen als de gebruiker op Volgende klikt; kan `False` teruggeven om de overgang te blokkeren (eigen validatie). | Pascal | 🔶 (fase 6) |
| BackButtonClick | Aangeroepen als de gebruiker op Terug klikt; kan `False` teruggeven om te blokkeren. | Pascal | 🔶 (fase 6) |
| CancelButtonClick | Aangeroepen als de gebruiker op Annuleren klikt, vóór de bevestigingsdialoog. | Pascal | 🔶 (fase 6) |
| ShouldSkipPage | Aangeroepen per pagina om te bepalen of die pagina overgeslagen moet worden. | Pascal | 🔶 (fase 6) |
| CurPageChanged | Aangeroepen nadat de wizard naar een nieuwe pagina is gegaan. | Pascal | 🔶 gegenereerd voor knopinstellingen; eigen code (fase 6) |
| CheckPassword | Aangeroepen om een door de gebruiker ingevoerd wachtwoord zelf te valideren (naast/in plaats van de ingebouwde Password-richtlijn-check). | Pascal | 🔶 (fase 6, zie ook categorie 24 Beveiliging) |
| NeedRestart | Aangeroepen aan het eind van de installatie om te bepalen of een herstart nodig is. | Pascal | 🔶 (fase 6) |
| UpdateReadyMemo | Aangeroepen om de samenvattingstekst op de Ready to Install-pagina zelf samen te stellen. | Pascal | 🔶 (fase 6) |
| RegisterPreviousData | Aangeroepen om eigen gegevens op te slaan die bij een volgende update weer opgehaald kunnen worden (vergelijkbaar met de `UsePrevious*`-richtlijnen, maar dan voor eigen data). | Pascal | 🔶 (fase 6, zie ook categorie 19 Overige instellingen) |
| CheckSerial | Aangeroepen om een serienummer/licentiesleutel te valideren (eigen invoerscherm nodig). | Pascal | 🔶 (fase 6) |
| GetCustomSetupExitCode | Aangeroepen om de exitcode van Setup.exe zelf te bepalen. | Pascal | 🔶 (fase 6) |
| PrepareToInstall | Aangeroepen vlak voor de daadwerkelijke bestandsinstallatie — geschikt om bijvoorbeeld lopende processen af te sluiten. | Pascal | 🔶 (fase 6, zie ook categorie 20 Herstart en lopende applicaties) |
| RegisterExtraCloseApplicationsResources | Aangeroepen om extra bestanden/processen aan de ingebouwde "sluit lopende applicaties"-detectie toe te voegen. | Pascal | 🔶 (fase 6, zie ook categorie 20 Herstart en lopende applicaties) |

## 33. Pascal Script: Uninstall Event Functions

Zelfde mechanisme als categorie 32, maar dan voor Uninstall.exe.

| Functie | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| InitializeUninstall | Aangeroepen direct bij start van de deïnstallatie. Kan `False` teruggeven om af te breken. | Pascal | 🔶 (fase 6) |
| InitializeUninstallProgressForm | Aangeroepen voordat het voortgangsvenster van de deïnstallatie getoond wordt — geschikt om dat venster zelf aan te passen. | Pascal | 🔶 (fase 6) |
| DeinitializeUninstall | Aangeroepen vlak voordat Uninstall.exe afsluit. | Pascal | 🔶 (fase 6) |
| CurUninstallStepChanged | Aangeroepen bij elke overgang tussen deïnstallatiestappen (usAppMutexCheck, usUninstall, usPostUninstall, enzovoort). | Pascal | 🔶 (fase 6, zie ook categorie 21 Uninstall-instellingen) |
| UninstallNeedRestart | Aangeroepen aan het eind van de deïnstallatie om te bepalen of een herstart nodig is. | Pascal | 🔶 (fase 6) |

## 34. Preprocessor (ISPP)

De Inno Setup Preprocessor breidt de scripttaal uit met conditionele compilatie, variabelen en macro's — bedoeld voor geavanceerde/multi-configuratie scripts. Dit hoort, net als categorie 32/33, bij fase 6 (vrije scripttekst/code-editor); het is geen los scherm of eigenschap maar tekst in de `.iss`-brontekst zelf.

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| #define | Definieert een preprocessorconstante of -macro. | Pascal | 🔶 (fase 6) |
| #dim / #redim | Declareert resp. herdimensioneert een preprocessor-array. | Pascal | 🔶 (fase 6) |
| #undef | Verwijdert een eerder gedefinieerde constante. | Pascal | 🔶 (fase 6) |
| #include | Voegt de inhoud van een ander bestand in op compileertijd. | Pascal | 🔶 (fase 6) |
| #file | Leest de inhoud van een bestand in een preprocessorvariabele. | Pascal | 🔶 (fase 6) |
| #emit / #echo | Schrijft tekst naar het script resp. naar de compiler-console (debug/logging tijdens compileren). | Pascal | 🔶 (fase 6) |
| #expr / #call | Evalueert een Pascal-expressie resp. roept een preprocessorfunctie aan tijdens het compileren. | Pascal | 🔶 (fase 6) |
| #env | Leest een omgevingsvariabele. | Pascal | 🔶 (fase 6) |
| #insert | Voegt tekst in op de huidige positie in het script. | Pascal | 🔶 (fase 6) |
| #append | Voegt tekst toe aan een bestaand bestand. | Pascal | 🔶 (fase 6) |
| #if / #elif / #else / #endif | Conditionele compilatie op basis van een expressie. | Pascal | 🔶 (fase 6) |
| #ifdef / #ifndef / #ifexist / #ifnexist | Conditionele compilatie op basis van het bestaan van een definitie resp. bestand. | Pascal | 🔶 (fase 6) |
| #for | Herhaalt een blok preprocessorinstructies (bijvoorbeeld om over een lijst bestanden te itereren). | Pascal | 🔶 (fase 6) |
| #sub / #endsub | Definieert een herbruikbare preprocessorsubroutine. | Pascal | 🔶 (fase 6) |
| #pragma (parseroption/verboselevel/message/include/warning/error/link/resource) | Diverse compileertijd-instellingen en -diagnostiek. | Pascal | 🔶 (fase 6) |
| #error | Breekt het compileren af met een eigen foutmelding. | Pascal | 🔶 (fase 6) |

## 35. Command-line: Setup.exe

Parameters waarmee de gecompileerde installer zelf (stil) aangestuurd kan worden — relevant voor deployment/scripting rond de installer, niet voor het `.iss`-bestand zelf. Hoort bij een mogelijke toekomstige "build en deployment"-functionaliteit (fase 7, nog niet afgesproken/gepland), niet bij de IDE/editor-kant van de app.

| Parameter | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| /HELP, /? | Toont een overzicht van de command-line-parameters. | CLI | ⬜ |
| /SP- | Onderdrukt de "Dit zal ... installeren"-bevestigingsprompt vooraf. | CLI | ⬜ |
| /SILENT, /VERYSILENT | Stille installatie: geen schermen (SILENT toont nog een voortgangsvenster, VERYSILENT niets). | CLI | ⬜ |
| /SUPPRESSMSGBOXES | Onderdrukt berichtvensters tijdens een stille installatie (gebruikt standaardantwoorden). | CLI | ⬜ |
| /NOCANCEL | Verwijdert de Annuleren-knop en reactie op Alt+F4/sluiten. | CLI | ⬜ |
| /NORESTART | Voorkomt een automatische herstart, zelfs als die nodig is. | CLI | ⬜ |
| /RESTARTEXITCODE=code | Overschrijft de exitcode als een herstart nodig is. | CLI | ⬜ |
| /CLOSEAPPLICATIONS / /NOCLOSEAPPLICATIONS | Schakelt het automatisch sluiten van gedetecteerde lopende applicaties aan/uit. | CLI | ⬜ (zie ook categorie 20 Herstart en lopende applicaties) |
| /FORCECLOSEAPPLICATIONS / /NOFORCECLOSEAPPLICATIONS | Forceert resp. verbiedt het geforceerd sluiten van lopende applicaties. | CLI | ⬜ |
| /LOGCLOSEAPPLICATIONS | Logt welke applicaties gesloten zouden worden, zonder ze echt te sluiten. | CLI | ⬜ |
| /RESTARTAPPLICATIONS / /NORESTARTAPPLICATIONS | Schakelt het heropstarten van gesloten applicaties na installatie aan/uit. | CLI | ⬜ |
| /ALLUSERS / /CURRENTUSER | Forceert installatie voor alle gebruikers resp. alleen de huidige gebruiker. | CLI | ⬜ |
| /LOG / /LOG="bestand" | Schrijft een logbestand van de installatie, naar een tijdelijk bestand resp. een opgegeven pad. | CLI | ⬜ |
| /NOICONS | Slaat de "snelkoppelingen aanmaken"-taken over. | CLI | ⬜ (vergelijkbaar effect als `CreateStartMenuIcon`/`CreateDesktopIcon` uit, zie categorie 19) |
| /TYPE=naam | Kiest een installatietype ([Types]) voor stille installatie. | CLI | ⬜ |
| /COMPONENTS="lijst" | Kiest expliciete componenten voor stille installatie. | CLI | ⬜ |
| /TASKS="lijst" | Kiest expliciete taken voor stille installatie. | CLI | ⬜ |
| /MERGETASKS="lijst" | Zelfde als /TASKS, maar behoudt de standaardtaken i.p.v. ze te vervangen. | CLI | ⬜ |
| /PASSWORD=wachtwoord | Geeft het Setup-wachtwoord mee voor stille installatie. | CLI | ⬜ |
| /DIR="pad" | Overschrijft de installatiemap. | CLI | ⬜ |
| /GROUP="naam" | Overschrijft de Startmenu-map. | CLI | ⬜ |
| /NOCANCEL, /LOADINF="bestand", /SAVEINF="bestand" | Laadt resp. slaat wizardantwoorden op in een .inf-bestand voor latere herhaling. | CLI | ⬜ |
| /LANG=taalcode | Kiest de installatietaal voor stille/onbemande installatie. | CLI | ⬜ |
| /REDIRECTIONGUARD / /NOREDIRECTIONGUARD | Schakelt extra bescherming tegen bestandssysteem-redirectie aan/uit (sinds recentere Inno Setup-versies). | CLI | ⬜ |

## 36. Command-line: ISCC.exe (compiler)

Parameters waarmee de Inno Setup-compiler zelf vanaf de command line (buiten de Inno Setup IDE/Compiler GUI om) aangestuurd wordt — relevant voor een eigen build/CI-integratie, niet voor het scriptbestand zelf.

| Parameter | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| /O"map" (--output-dir) | Overschrijft de OutputDir uit het script. | CLI | ⬜ |
| /F"naam" (--output-base-filename) | Overschrijft de OutputBaseFilename uit het script. | CLI | ⬜ |
| /Sparam (--signtool) | Registreert een signtool-commando (zie ook categorie 25 Code signing). | CLI | ⬜ |
| --no-ide-signtools | Negeert signtools die alleen in de Compiler-IDE zijn geregistreerd. | CLI | ⬜ |
| /Cc, --no-compression | Schakelt compressie uit (sneller compileren, groter bestand — handig tijdens ontwikkelen). | CLI | ⬜ |
| --no-signing | Slaat alle digitale ondertekening over, ook als het script die vraagt. | CLI | ⬜ |
| --no-signcheck | Slaat de controle op geldige signtools over. | CLI | ⬜ |
| /Jparam (define) | Geeft een preprocessorconstante mee (equivalent aan `#define` vanaf de command line). | CLI | ⬜ |
| /Dparam | Alias voor /J (oudere naamgeving). | CLI | ⬜ |
| --messages-jsonl | Schrijft compileermeldingen (fouten/waarschuwingen) als JSON Lines — geschikt om in een eigen IDE te tonen. | CLI | ⬜ (interessant voor een toekomstige "compileren vanuit de IDE"-knop) |
| /P, --preprocess | Toont alleen het resultaat van het preprocessen, compileert niet echt. | CLI | ⬜ |
| /Qp, --quiet-and-no-prompt / /Q, --quiet | Onderdrukt voortgangsuitvoer resp. ook de "druk op een toets"-prompt aan het eind. | CLI | ⬜ |
| /?, --help | Toont een overzicht van de command-line-parameters. | CLI | ⬜ |
| --version | Toont de compilerversie. | CLI | ⬜ |

## 37. Verouderd (obsolete richtlijnen)

Inno Setup noemt deze `[Setup]`-richtlijnen zelf expliciet verouderd/obsolete in de documentatie (vervangen door een modernere richtlijn, of functioneel zinloos geworden door latere Windows-versies). Geen actie nodig in Inno Setup Studio.

| Richtlijn | Omschrijving | Mechanisme | Status |
|---|---|---|---|
| AlwaysCreateUninstallIcon | Verouderd; moderne Inno Setup-versies maken het uninstall-snelkoppeling-gedrag impliciet correct. | — | ➖ |
| BackColor | Verouderd; achtergrondkleur van het klassieke (niet-modernere) wizarduiterlijk. | — | ➖ |
| BackColor2 | Verouderd; tweede kleur voor het verloop van BackColor. | — | ➖ |
| BackColorDirection | Verouderd; richting van het BackColor/BackColor2-verloop. | — | ➖ |
| BackSolid | Verouderd; effen in plaats van verlopen achtergrondkleur. | — | ➖ |
| DisableAppendDir | Verouderd; betrof oud gedrag rond het automatisch toevoegen van de appnaam aan het doelpad. | — | ➖ |
| DontMergeDuplicateFiles | Verouderd; betrof een oude optimalisatie bij dubbele bestanden in de uitvoer. | — | ➖ |
| MessagesFile | Verouderd; vervangen door losse taalbestanden via [Languages]/MessagesFile per taal (zie categorie 31). | — | ➖ |
| UninstallIconFile | Verouderd; icoon wordt nu afgeleid van SetupIconFile. | — | ➖ |
| UninstallIconName | Verouderd; betrof weergavenaam van het uninstall-icoon. | — | ➖ |
| UninstallStyle | Verouderd; betrof het klassieke wizarduiterlijk voor deïnstallatie. | — | ➖ |
| WindowResizable | Verouderd; vervangen door modernere wizard-vensterschaling. | — | ➖ |
| WindowShowCaption | Verouderd; betrof het tonen van de titelbalktekst in het klassieke uiterlijk. | — | ➖ |
| WindowStartMaximized | Verouderd; betrof het gemaximaliseerd starten in het klassieke uiterlijk. | — | ➖ |
| WindowVisible | Verouderd; betrof de zichtbaarheid van het hoofdvenster in het klassieke uiterlijk. | — | ➖ |
| WizardResizable | Verouderd; vervangen door modernere, altijd schaalbare wizardvensters. | — | ➖ |