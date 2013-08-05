DeconTools uses CompassXtractMS.dll to read Bruker Maxis data.
This file resides at C:\DMS_Programs\ProteoWizard\CompassXtractMS.dll

Install CompassXtract using the following (auto reboots if you use /passive):
"\\prismdevii\past\Software\CompassXport\CompassXtract_Setup_v3.1.2\Bruker Daltonics CompassXtract.msi" /passive

Next, log back in and run this command
C:\DMS_Programs\DeconTools\DeconConsole.exe \\prismdevii\PAST\BrukerTestData\2012_05_15_MN9_A_000010.d \\prismdevii\PAST\BrukerTestData\BrukerMax

I don't know why, but if you don't manually run a DeconTools analysis after installing CompassXtract, 
then automated processing with the AnalysisManager fails. 
You can stop the DeconConsole.exe application using Ctrl+Break as soon as it starts showing progress messages.
