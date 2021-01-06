@echo off

IF [%1] == [] GOTO MissingTarget

echo.
echo.
echo Copying files to %1

xcopy ..\DeconConsole\bin\x64\Release\*.exe                   %1 /D /Y /F
xcopy ..\DeconConsole\bin\x64\Release\*.dll                   %1 /D /Y /F

xcopy ..\DeconConsole\bin\x64\Release\ThermoRawFileReader.pdb %1 /D /Y /F
xcopy ..\DeconConsole\bin\x64\Release\DeconTools.Backend.pdb  %1 /D /Y /F
xcopy ..\DeconConsole\bin\x64\Release\DeconConsole.pdb        %1 /D /Y /F
xcopy ..\DeconConsole\bin\x64\Release\ThermoRawFileReader.xml %1 /D /Y /F

xcopy ..\DeconConsole\bin\x64\Release\x64\SQLite.Interop.dll  %1 /D /Y /F
xcopy ..\DeconConsole\bin\x64\Release\x86\SQLite.Interop.dll  %1 /D /Y /F

xcopy ..\Readme.md                                            %1 /D /Y /F

goto done

:MissingTarget
echo.
echo Error: You must specify a directory when calling this batch file

:Done
