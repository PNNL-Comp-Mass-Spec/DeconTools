@echo off
echo.
echo Be sure to compile DeconTools_x64 in Release mode before continuing
echo.

pause

@echo on

call Distribute_Files_Work.bat C:\DMS_Programs\DeconTools
call Distribute_Files_Work.bat \\pnl\projects\OmicsSW\DMS_Programs\AnalysisToolManagerDistribution\DeconTools
call Distribute_Files_Work.bat \\floyd\software\DeconTools\CurrentVersion\Exe

@echo off
pause
