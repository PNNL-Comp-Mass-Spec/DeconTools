; This is an Inno Setup configuration file
; http://www.jrsoftware.org/isinfo.php

#define ApplicationVersion GetFileVersion('..\DeconConsole\bin\x64\Release\DeconConsole.exe')

[CustomMessages]
AppName=DeconTools

[Messages]
WelcomeLabel2=This will install [name/ver] on your computer.%n%n%n%nNOTICE:%nSome source files require access to a 64-bit ProteoWizard installation. Please install 64-bit ProteoWizard before using the program to avoid errors.%n%n
; Example with multiple lines:
; WelcomeLabel2=Welcome message%n%nAdditional sentence

[Files]
Source: DeconConsole\bin\x64\Release\DeconConsole.exe                                     ; DestDir: {app}
Source: DeconConsole\bin\x64\Release\DeconConsole.exe.config                              ; DestDir: {app}

Source: DeconConsole\bin\x64\Release\BaseCommon.dll                                       ; DestDir: {app}
Source: DeconConsole\bin\x64\Release\BaseDataAccess.dll                                   ; DestDir: {app}
Source: DeconConsole\bin\x64\Release\BaseError.dll                                        ; DestDir: {app}
Source: DeconConsole\bin\x64\Release\MassSpecDataReader.dll                               ; DestDir: {app}

Source: DeconTools.Backend\bin\x64\Release\DeconTools.Backend.dll                         ; DestDir: {app}
Source: DeconTools.Backend\bin\x64\Release\MathNet.Numerics.dll                           ; DestDir: {app}

Source: DeconToolsAutoProcessV1\bin\x64\Release\DeconToolsAutoProcessV1.exe               ; DestDir: {app}
Source: DeconToolsAutoProcessV1\bin\x64\Release\DeconToolsAutoProcessV1.exe.config        ; DestDir: {app}

Source: DeconTools.Backend\bin\x64\Release\x64\SQLite.Interop.dll                         ; DestDir: {app}\x64
Source: DeconTools.Backend\bin\x64\Release\x86\SQLite.Interop.dll                         ; DestDir: {app}\x86
Source: DeconTools.Backend\bin\x64\Release\System.Data.SQLite.dll                         ; DestDir: {app}
Source: DeconTools.Backend\bin\x64\Release\UIMFLibrary.dll                                ; DestDir: {app}
Source: DeconTools.Backend\bin\x64\Release\PRISM.dll                                      ; DestDir: {app}

Source: Library\alglibnet2.dll                                                            ; DestDir: {app}
Source: Library\BrukerDataReader.dll                                                      ; DestDir: {app}
Source: Library\GWSFileUtilities.dll                                                      ; DestDir: {app}
Source: Library\DLLsToBeCopied_x64\Interop.EDAL.SxS.manifest                              ; DestDir: {app}
Source: Library\DLLsToBeCopied\Interop.HSREADWRITELib.SxS.manifest                        ; DestDir: {app}
Source: Library\Mapack.dll                                                                ; DestDir: {app}
Source: Library\DLLsToBeCopied_x64\MassLynxRaw.dll                                        ; DestDir: {app}
Source: Library\MSDBLibrary.dll                                                           ; DestDir: {app}
Source: Library\x64\DeconEngineV2.dll                                                     ; DestDir: {app}
Source: Library\x64\MultiAlignEngine.dll                                                  ; DestDir: {app}
Source: Library\PNNLOmics.dll                                                             ; DestDir: {app}
Source: Library\PNNLOmicsElementData.xml                                                  ; DestDir: {app}
Source: Library\ProteowizardWrapper.dll                                                   ; DestDir: {app}
Source: DeconConsole\bin\x64\Release\ThermoRawFileReader.dll                              ; DestDir: {app}
Source: DeconConsole\bin\x64\Release\ThermoFisher.CommonCore.MassPrecisionEstimator.dll   ; DestDir: {app}
Source: DeconConsole\bin\x64\Release\ThermoFisher.CommonCore.RawFileReader.dll            ; DestDir: {app}
Source: DeconConsole\bin\x64\Release\ThermoFisher.CommonCore.BackgroundSubtraction.dll    ; DestDir: {app}
Source: DeconConsole\bin\x64\Release\ThermoFisher.CommonCore.Data.dll                     ; DestDir: {app}
Source: Library\RawFileReaderLicense.doc                                                  ; DestDir: {app}

Source: Library\Agilent_D\agtsampleinforw.dll                                             ; DestDir: {app}
Source: Library\Agilent_D\BaseCommon.dll                                                  ; DestDir: {app}
Source: Library\Agilent_D\BaseDataAccess.dll                                              ; DestDir: {app}
Source: Library\Agilent_D\BaseError.dll                                                   ; DestDir: {app}
Source: Library\Agilent_D\BaseTof.dll                                                     ; DestDir: {app}
Source: Library\Agilent_D\MassSpecDataReader.dll                                          ; DestDir: {app}
Source: Library\Agilent_D\MIDAC.dll                                                       ; DestDir: {app}
Source: Library\Agilent_D\BaseDataAccess.dll.config                                       ; DestDir: {app}

Source: Readme.md                                                                         ; DestDir: {app}
Source: RevisionHistory.txt                                                               ; DestDir: {app}
Source: Parameter_Files\SampleParameterFile.xml                                           ; DestDir: {app}
Source: Parameter_Files\SampleParameterFile_SmallMolecule.xml                             ; DestDir: {app}
Source: Parameter_Files\SampleParameterFileIMS.xml                                        ; DestDir: {app}

[Dirs]
Name: {commonappdata}\DeconTools; Flags: uninsalwaysuninstall

[Tasks]
Name: desktopicon; Description: {cm:CreateDesktopIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked
; Name: quicklaunchicon; Description: {cm:CreateQuickLaunchIcon}; GroupDescription: {cm:AdditionalIcons}; Flags: unchecked

[Icons]
Name: {group}\DeconToolsAutoProcessor; Filename: {app}\DeconToolsAutoProcessV1.exe; Comment: Decon Tools Auto Processor
Name: {group}\ReadMe; Filename: {app}\Readme.md; Comment: DeconTools ReadMe
Name: {group}\SampleParameterFile; Filename: {app}\SampleParameterFile.xml; Comment: Sample Parameter File
Name: {group}\SampleParameterFileIMS; Filename: {app}\SampleParameterFileIMS.xml; Comment: Sample IMS Parameter File
Name: {group}\Uninstall DeconTools; Filename: {uninstallexe}

Name: {commondesktop}\DeconToolsAutoProcessor; Filename: {app}\DeconToolsAutoProcessV1.exe; Tasks: desktopicon; Comment: Decon Tools Auto Processor

[Setup]
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64
AppName=DeconTools_64bit
AppVersion={#ApplicationVersion}
;AppVerName=DeconTools
AppID=DeconToolsId
AppPublisher=Pacific Northwest National Laboratory
AppPublisherURL=http://omics.pnl.gov/software
AppSupportURL=http://omics.pnl.gov/software
AppUpdatesURL=http://omics.pnl.gov/software
DefaultDirName={autopf}\DeconTools
DefaultGroupName=DeconTools
AppCopyright=© PNNL
;LicenseFile=.\License.rtf
PrivilegesRequired=poweruser
OutputBaseFilename=DeconTools_Installer_x64
;VersionInfoVersion=1.57
VersionInfoVersion={#ApplicationVersion}
VersionInfoCompany=PNNL
VersionInfoDescription=DeconTools
VersionInfoCopyright=PNNL
DisableFinishedPage=true
ShowLanguageDialog=no
;SetupIconFile=..\MageFileProcessor\wand.ico
;InfoBeforeFile=.\readme.rtf
ChangesAssociations=false
;WizardImageFile=..\Deploy\Images\MageSetupSideImage.bmp
;WizardSmallImageFile=..\Deploy\Images\MageSetupSmallImage.bmp
;InfoAfterFile=.\postinstall.rtf
EnableDirDoesntExistWarning=false
AlwaysShowDirOnReadyPage=true
UninstallDisplayIcon={app}\delete_16x.ico
ShowTasksTreeLines=true
OutputDir=Installer\Output
SourceDir=..\
Compression=lzma
SolidCompression=yes
[Registry]
;Root: HKCR; Subkey: MageFile; ValueType: string; ValueName: ; ValueData:Mage File; Flags: uninsdeletekey
;Root: HKCR; Subkey: MageSetting\DefaultIcon; ValueType: string; ValueData: {app}\wand.ico,0; Flags: uninsdeletevalue
[UninstallDelete]
Name: {app}; Type: filesandordirs

[Code]
function InitializeSetup(): Boolean;
var
    NetFrameWorkInstalled : Boolean;
begin
	NetFrameWorkInstalled := RegKeyExists(HKLM,'SOFTWARE\Microsoft\Net Framework Setup\NDP\v4.0');
	if NetFrameWorkInstalled =true then
	begin
		Result := true;
	end;

	if NetFrameWorkInstalled =false then
	begin
		MsgBox('This setup requires the .NET Framework 4.0. Please install the .NET Framework and run this setup again.',
			mbInformation, MB_OK);
		Result:=false;
	end;
end;
