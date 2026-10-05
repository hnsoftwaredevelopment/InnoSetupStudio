; Gegenereerd door Inno Setup Studio. Handmatige wijzigingen in dit bestand gaan
; verloren zodra het opnieuw wordt gegenereerd.

[Setup]
; Toepassing
AppId={{8C5C7F52-0F3E-4F5B-9A59-1D2E3F4A5B6C}
AppName=MijnApp
AppVersion=1.2.3
AppPublisher=Voortman Steel Machinery
AppPublisherURL=https://www.voortman.net
AppSupportURL=https://www.voortman.net
AppUpdatesURL=https://www.voortman.net
AppContact=info@voortman.net

; Mappen en startmenu
DefaultDirName={autopf}\Voortman\MijnApp
DefaultGroupName=Voortman\MijnApp
DisableProgramGroupPage=no
AppendDefaultGroupName=no
UsePreviousTasks=no

; Wizardpagina's
DisableWelcomePage=no
LicenseFile=C:\Docs\licentie.txt
InfoBeforeFile=C:\Docs\voor.rtf
InfoAfterFile=C:\Docs\na.txt
UserInfoPage=yes
DefaultUserInfoName=Herbert
DefaultUserInfoOrg=Voortman
DisableReadyMemo=yes

; Uiterlijk
SetupIconFile=C:\Beeld\app.ico
WizardImageFile=C:\Beeld\groot.bmp
WizardSmallImageFile=C:\Beeld\klein.bmp
WizardStyle=modern

; Installatie en uitvoer
ArchitecturesInstallIn64BitMode=x64compatible
Compression=lzma2
SolidCompression=yes
UninstallDisplayName=MijnApp
UninstallDisplayIcon={app}\MijnApp.exe
OutputDir=C:\Uitvoer
OutputBaseFilename=MijnApp-Setup

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "C:\Bron\MijnApp\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\MijnApp"; Filename: "{app}\MijnApp.exe"; WorkingDir: "{app}"
Name: "{autodesktop}\MijnApp"; Filename: "{app}\MijnApp.exe"; WorkingDir: "{app}"; Tasks: desktopicon
