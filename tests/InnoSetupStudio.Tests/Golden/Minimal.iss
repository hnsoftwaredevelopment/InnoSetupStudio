; Gegenereerd door Inno Setup Studio. Handmatige wijzigingen in dit bestand gaan
; verloren zodra het opnieuw wordt gegenereerd.

[Setup]
; Toepassing
AppId={{8C5C7F52-0F3E-4F5B-9A59-1D2E3F4A5B6C}
AppName=MijnApp
AppVersion=1.2.3

; Mappen en startmenu
DefaultDirName={autopf}\MijnApp
DefaultGroupName=MijnApp
DisableDirPage=no

; Wizardpagina's
DisableWelcomePage=no

; Uiterlijk
WizardStyle=modern

; Installatie en uitvoer
ArchitecturesInstallIn64BitMode=x64compatible
Compression=lzma2
SolidCompression=yes
UninstallDisplayName=MijnApp
UninstallDisplayIcon={app}\MijnApp.exe
OutputBaseFilename=MijnApp-1.2.3-Setup

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "C:\Bron\MijnApp\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
