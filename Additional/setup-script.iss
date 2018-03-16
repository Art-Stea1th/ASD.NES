#define AppName "ASD.NES"
#define AppVersion "0.1"
#define AppPublisher "Art of Software Development [ex Art.Stea1th Design]"
#define AppURL "https://github.com/Art-Stea1th/ASD.NES"
#define AppExeName "ASD.NES.WPF.exe"

#define SolutionRoot "..\"
#define ReleaseDir "\Application\ASD.NES.WPF\bin\Release"
#define OutDir ".\"

[Setup]
AppId={{8FD12CCC-7AFF-4F41-AB45-375CE63D2C04}
AppName={#AppName}
AppVersion={#AppVersion}
;AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={pf}\{#AppName}
DefaultGroupName={#AppName}
OutputDir={#OutDir}
OutputBaseFilename=ASD.NES_Setup_Windows
SetupIconFile=icon.ico
Compression=lzma2/ultra64
SolidCompression=yes
DisableDirPage=auto
RestartIfNeededByRun=False
InternalCompressLevel=ultra64
DisableWelcomePage=True
UninstallDisplayIcon={#OutDir}\icon.ico

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Files]
Source: {#SolutionRoot}\{#ReleaseDir}\ASD.NES.WPF.exe;  DestDir: "{app}";        Flags: ignoreversion
Source: {#SolutionRoot}\{#ReleaseDir}\ASD.NES.Core.dll; DestDir: "{app}";        Flags: ignoreversion
Source: {#SolutionRoot}\{#ReleaseDir}\NAudio.dll;       DestDir: "{app}";        Flags: ignoreversion
Source: {#SolutionRoot}\{#ReleaseDir}\Games\*;          DestDir: "{app}\Games\"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}";                       Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:ProgramOnTheWeb,{#AppName}}";  Filename: "{#AppURL}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent