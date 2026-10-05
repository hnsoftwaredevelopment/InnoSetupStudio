; Gegenereerd door Inno Setup Studio. Handmatige wijzigingen in dit bestand gaan
; verloren zodra het opnieuw wordt gegenereerd.

[Setup]
; Toepassing
AppId={{8C5C7F52-0F3E-4F5B-9A59-1D2E3F4A5B6C}
AppName=Oude {{Machine} "Plus"
AppVersion=2.0

; Mappen en startmenu
DefaultDirName={autopf}\Oude {{Machine} _Plus_
DefaultGroupName=Oude {{Machine} _Plus_
DisableDirPage=yes
DisableProgramGroupPage=yes

; Wizardpagina's
DisableReadyPage=yes
DisableFinishedPage=yes

; Installatie en uitvoer
Compression=lzma2
SolidCompression=yes
UninstallDisplayName=Oude {{Machine} "Plus"
UninstallDisplayIcon={app}\MijnApp.exe
OutputBaseFilename=Oude {Machine} _Plus_-2.0-Setup

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Files]
Source: "C:\Bron\MijnApp\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Oude {{Machine} _Plus_"; Filename: "{app}\MijnApp.exe"; WorkingDir: "{app}"
