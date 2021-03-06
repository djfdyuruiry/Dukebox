; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Dukebox"
#define MyAppVersion "0.9"
#define MyAppPublisher "djfdyuruiry"
#define MyAppURL "http://sourceforge.net/projects/dukebox/"
#define MyAppExeName "Dukebox.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{660CBCEA-E343-4F47-8E5B-1F834120EA36}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName=Dukebox {#MyAppVersion}
AllowNoIcons=yes
LicenseFile=.\license.txt
OutputDir=.\
OutputBaseFilename=Dukebox_v{#MyAppVersion}_64bit
Compression=lzma
SolidCompression=yes
ArchitecturesAllowed=x64          
UninstallDisplayIcon=..\..\Dukebox.Desktop\app.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\..\Dukebox.Desktop\bin\x64\Release\Dukebox.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\Dukebox.Desktop\bin\x64\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
;Source: "NDP452-KB2901907-x86-x64-AllOS-ENU.exe"; DestDir: "{app}"; AfterInstall: RunOtherInstaller
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
         
;[Code]
;procedure RunOtherInstaller;
;var
;  ResultCode: Integer;
;begin
;  if not Exec(ExpandConstant('{app}\NDP452-KB2901907-x86-x64-AllOS-ENU.exe /q /norestart /passive'), '', '', SW_SHOWNORMAL,
;    ewWaitUntilTerminated, ResultCode)
;  then
;    MsgBox('Other installer failed to run!' + #13#10 +
;      SysErrorMessage(ResultCode), mbError, MB_OK);
;end;

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

