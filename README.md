# Inno Setup Studio

Een WPF-IDE om Inno Setup `.iss`-installerscripts te beheren via schermen in plaats van kale
tekst: projectinstellingen, wizardschermen aan/uit zetten, elementen per scherm bewerken, en
de installer bouwen, zonder dat je de sectienamen en property namen van Inno Setup uit je hoofd
hoeft te kennen.

## Status

Fase 1 (solution scaffolding) is gebouwd. Zie `docs/Architectuur-en-Ontwerp.md` voor het volledige
overzicht en de statuslog.

## Bouwen

```powershell
build\Build.ps1                    # hoogt het releasenummer op en bouwt (Debug)
build\Build.ps1 -Configuration Release
dotnet test tests\InnoSetupStudio.Tests\InnoSetupStudio.Tests.csproj
```

Vereist: .NET 10 SDK. De Syncfusion-licentie leest de app uit
`%LocalAppData%\InnoSetupStudio\license\syncfusionlicense.txt` (staat bewust buiten de repo).

## Projectstructuur

- `src\InnoSetupStudio.Core` — datamodel en instellingen, geen UI-afhankelijkheden.
- `src\InnoSetupStudio.App` — WPF-app: thema's, lokalisatie (NL/EN/DE), iconen, vensters.
- `tests\InnoSetupStudio.Tests` — xUnit-tests voor Core.
- `build\` — releasenummering en buildscript.

## Documentatie

Bouwdocumentatie in Markdown wordt gemirrord naar Obsidian
(`Development\HNSoftwareDevelopment\Inno Setup Studio\`) via `build\sync-obsidian.ps1`.
