#ifndef AppVersion
#define AppVersion "1.0.0"
#endif

#ifndef SourceDir
#define SourceDir "..\\..\\src\\DesktopMemo.App\\bin\\Release\\net48"
#endif

#ifndef OutputDir
#define OutputDir "..\\..\\artifacts\\installer"
#endif

[Setup]
AppId={{D2E4D16D-26FD-4E4D-AEFD-E67A7FC0D300}
AppName=DesktopMemo
AppVersion={#AppVersion}
AppPublisher=DesktopMemo
DefaultDirName={autopf}\DesktopMemo
DefaultGroupName=DesktopMemo
OutputDir={#OutputDir}
OutputBaseFilename=DesktopMemo-Setup-{#AppVersion}
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop icon"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\DesktopMemo"; Filename: "{app}\DesktopMemo.App.exe"
Name: "{autodesktop}\DesktopMemo"; Filename: "{app}\DesktopMemo.App.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\DesktopMemo.App.exe"; Description: "Launch DesktopMemo"; Flags: nowait postinstall skipifsilent
