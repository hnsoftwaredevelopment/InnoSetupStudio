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
LicenseFile=C:\Docs\licentie.txt
InfoBeforeFile=C:\Docs\voor.rtf
InfoAfterFile=C:\Docs\na.txt
UserInfoPage=yes

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
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"

[CustomMessages]
BtnWelcomeBackCaption=Back
BtnWelcomeNextCaption=Start
dutch.BtnWelcomeNextCaption=Begin
german.BtnWelcomeNextCaption=Los
BtnWelcomeNextTooltip=Let's go
BtnWelcomeCancelCaption=Stop
BtnLicenseBackCaption=Back
BtnLicenseNextCaption=I agree
dutch.BtnLicenseNextCaption=Akkoord
BtnLicenseNextTooltip=Go on
BtnLicenseCancelCaption=Stop
BtnInfoBeforeBackCaption=Back
german.BtnInfoBeforeBackCaption=Zurück
BtnInfoBeforeNextCaption=Continue
BtnInfoBeforeNextTooltip=Go on
dutch.BtnInfoBeforeNextTooltip=Lees de informatie
BtnInfoBeforeCancelCaption=Stop
BtnUserInfoBackCaption=Back
BtnUserInfoNextCaption=Continue
BtnUserInfoNextTooltip=Enter your details
dutch.BtnUserInfoNextTooltip=Vul je gegevens in
BtnUserInfoCancelCaption=Stop
BtnSelectDirBackCaption=Back
BtnSelectDirNextCaption=Continue
BtnSelectDirNextTooltip=Go on
BtnSelectDirCancelCaption=Stop
BtnSelectDirBrowseCaption=Find...
dutch.BtnSelectDirBrowseCaption=Zoeken...
BtnSelectDirBrowseTooltip=Pick a folder
BtnSelectGroupBackCaption=Back
BtnSelectGroupNextCaption=Continue
BtnSelectGroupNextTooltip=Go on
BtnSelectGroupCancelCaption=Stop
BtnSelectGroupBrowseCaption=
dutch.BtnSelectGroupBrowseCaption=Zoeken...
BtnReadyBackCaption=Back
BtnReadyNextCaption=Install now
dutch.BtnReadyNextCaption=Nu installeren
BtnReadyNextTooltip=Go on
BtnReadyCancelCaption=Stop
BtnInfoAfterBackCaption=Back
BtnInfoAfterNextCaption=Continue
BtnInfoAfterNextTooltip=Go on
BtnInfoAfterCancelCaption=Stop

[Files]
Source: "C:\Bron\MijnApp\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Code]
var
  InitNextFontName: String;
  InitNextFontSize: Integer;
  InitNextFontStyle: TFontStyles;
  InitCancelFontSize: Integer;

procedure InitializeWizard;
begin
  InitNextFontName := WizardForm.NextButton.Font.Name;
  InitNextFontSize := WizardForm.NextButton.Font.Size;
  InitNextFontStyle := WizardForm.NextButton.Font.Style;
  InitCancelFontSize := WizardForm.CancelButton.Font.Size;
  WizardForm.DirBrowseButton.Caption := CustomMessage('BtnSelectDirBrowseCaption');
  WizardForm.DirBrowseButton.Font.Style := [fsBold];
  WizardForm.DirBrowseButton.Hint := CustomMessage('BtnSelectDirBrowseTooltip');
  WizardForm.DirBrowseButton.ShowHint := True;
  if CustomMessage('BtnSelectGroupBrowseCaption') <> '' then
    WizardForm.GroupBrowseButton.Caption := CustomMessage('BtnSelectGroupBrowseCaption');
  WizardForm.GroupBrowseButton.Enabled := False;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  { Setup zet Font en Hint niet terug bij een paginawissel: eerst naar de beginwaarde. }
  WizardForm.NextButton.Font.Name := InitNextFontName;
  WizardForm.NextButton.Font.Size := InitNextFontSize;
  WizardForm.NextButton.Font.Style := InitNextFontStyle;
  WizardForm.NextButton.Hint := '';
  WizardForm.NextButton.ShowHint := False;
  WizardForm.CancelButton.Font.Size := InitCancelFontSize;
  case CurPageID of
    wpWelcome:
      begin
        WizardForm.BackButton.Caption := CustomMessage('BtnWelcomeBackCaption');
        WizardForm.NextButton.Caption := CustomMessage('BtnWelcomeNextCaption');
        WizardForm.NextButton.Font.Style := [fsBold];
        WizardForm.NextButton.Hint := CustomMessage('BtnWelcomeNextTooltip');
        WizardForm.NextButton.ShowHint := True;
        WizardForm.CancelButton.Caption := CustomMessage('BtnWelcomeCancelCaption');
        WizardForm.CancelButton.Font.Size := 9;
        WizardForm.CancelButton.Enabled := False;
      end;
    wpLicense:
      begin
        WizardForm.BackButton.Caption := CustomMessage('BtnLicenseBackCaption');
        WizardForm.NextButton.Caption := CustomMessage('BtnLicenseNextCaption');
        WizardForm.NextButton.Font.Name := 'Consolas';
        WizardForm.NextButton.Hint := CustomMessage('BtnLicenseNextTooltip');
        WizardForm.NextButton.ShowHint := True;
        WizardForm.CancelButton.Caption := CustomMessage('BtnLicenseCancelCaption');
        WizardForm.CancelButton.Font.Size := 9;
      end;
    wpInfoBefore:
      begin
        WizardForm.BackButton.Caption := CustomMessage('BtnInfoBeforeBackCaption');
        WizardForm.NextButton.Caption := CustomMessage('BtnInfoBeforeNextCaption');
        WizardForm.NextButton.Font.Style := [fsBold];
        WizardForm.NextButton.Hint := CustomMessage('BtnInfoBeforeNextTooltip');
        WizardForm.NextButton.ShowHint := True;
        WizardForm.CancelButton.Caption := CustomMessage('BtnInfoBeforeCancelCaption');
        WizardForm.CancelButton.Font.Size := 9;
      end;
    wpUserInfo:
      begin
        WizardForm.BackButton.Caption := CustomMessage('BtnUserInfoBackCaption');
        WizardForm.NextButton.Caption := CustomMessage('BtnUserInfoNextCaption');
        WizardForm.NextButton.Font.Style := [fsBold];
        WizardForm.NextButton.Hint := CustomMessage('BtnUserInfoNextTooltip');
        WizardForm.NextButton.ShowHint := True;
        WizardForm.CancelButton.Caption := CustomMessage('BtnUserInfoCancelCaption');
        WizardForm.CancelButton.Font.Size := 9;
      end;
    wpSelectDir:
      begin
        WizardForm.BackButton.Caption := CustomMessage('BtnSelectDirBackCaption');
        WizardForm.NextButton.Caption := CustomMessage('BtnSelectDirNextCaption');
        WizardForm.NextButton.Font.Size := 10;
        WizardForm.NextButton.Font.Style := [fsBold];
        WizardForm.NextButton.Hint := CustomMessage('BtnSelectDirNextTooltip');
        WizardForm.NextButton.ShowHint := True;
        WizardForm.CancelButton.Caption := CustomMessage('BtnSelectDirCancelCaption');
        WizardForm.CancelButton.Font.Size := 9;
      end;
    wpSelectProgramGroup:
      begin
        WizardForm.BackButton.Caption := CustomMessage('BtnSelectGroupBackCaption');
        WizardForm.NextButton.Caption := CustomMessage('BtnSelectGroupNextCaption');
        WizardForm.NextButton.Font.Style := [fsBold];
        WizardForm.NextButton.Hint := CustomMessage('BtnSelectGroupNextTooltip');
        WizardForm.NextButton.ShowHint := True;
        WizardForm.CancelButton.Caption := CustomMessage('BtnSelectGroupCancelCaption');
        WizardForm.CancelButton.Font.Size := 9;
      end;
    wpReady:
      begin
        WizardForm.BackButton.Caption := CustomMessage('BtnReadyBackCaption');
        WizardForm.NextButton.Caption := CustomMessage('BtnReadyNextCaption');
        WizardForm.NextButton.Font.Size := 12;
        WizardForm.NextButton.Font.Style := [fsBold];
        WizardForm.NextButton.Hint := CustomMessage('BtnReadyNextTooltip');
        WizardForm.NextButton.ShowHint := True;
        WizardForm.CancelButton.Caption := CustomMessage('BtnReadyCancelCaption');
        WizardForm.CancelButton.Font.Size := 9;
      end;
    wpInfoAfter:
      begin
        WizardForm.BackButton.Caption := CustomMessage('BtnInfoAfterBackCaption');
        WizardForm.BackButton.Enabled := False;
        WizardForm.NextButton.Caption := CustomMessage('BtnInfoAfterNextCaption');
        WizardForm.NextButton.Font.Style := [fsBold];
        WizardForm.NextButton.Hint := CustomMessage('BtnInfoAfterNextTooltip');
        WizardForm.NextButton.ShowHint := True;
        WizardForm.CancelButton.Caption := CustomMessage('BtnInfoAfterCancelCaption');
        WizardForm.CancelButton.Font.Size := 9;
        WizardForm.CancelButton.Visible := False;
      end;
  end;
end;
