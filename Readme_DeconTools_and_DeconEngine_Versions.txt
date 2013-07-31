.NET 1.1 version of DeconEngineV2
	http://prismsvn.pnl.gov/svn/User_Applications/Jaitly_Utilities/Decon2LS/branches/Decon2LS_Beta_activeBranch
	Used by DeconConsole.exe (11/30/2012) to deisotope spectra
	Compile with Visual Studio 2003, but only works on XP; does not compile with .NET 2003 on Windows 7 (according to Gordon Slysz)

.NET 4.0 version of DeconEngineV2
	http://prismsvn.pnl.gov/svn/User_Applications/Slysz_Utilities/DeconEngineV2/trunk
	Port of DeconEngineV2 to VS 2010
	Charge determination is 2x slower compared with the .NET 1.1 version

C# DeconTools (with ThrashV2)
	http://prismsvn.pnl.gov/svn/User_Applications/Slysz_Utilities/DeconTools/trunk
	Port of DeconEngine to C#
	See file DeconTools.Backend\ProcessingTasks\Deconvoluters\HornDeconvolutor\ThrashDeconvolutorV2.cs
	Used by DeconConsole if parameter file has:
		<UseThrashV1>true</UseThrashV1>

