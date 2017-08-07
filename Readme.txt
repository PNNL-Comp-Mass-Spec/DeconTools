== Overview ==

DeconTools is a software package used for detecting features in MS data by using the isotopic signatures of
expected components. The tool uses different algorithms for different parts of the deconvolution process: noise
reduction, peak detection, prediction of theoretical isotopic envelope and scoring functions that quantitate
the quality of signature observed in the data.

== Running DeconTools ==

DeconTools can read the following file formats.  However, some of the file formats require that you 
install the vendor-supplied data browsing software so that DeconTools has access to the necessary DLLs.
* mzXML files   (Requires ProteoWizard installation)
* mzML files    (Requires ProteoWizard installation)
* mz5 files     (Requires ProteoWizard installation)
* Thermo Finnigan .Raw files (requires Xcalibur or MSFileReader)
  * Download MSFileReader v3.x by creating an account at https://thermo.flexnetoperations.com/control/thmo/login 
    then login and choose "Utility Software". Look for MSFileReader 3.1 SP4 or similar. 
  * Run the installer and install both the 32-bit and 64-bit versions.
* Agilent .Wiff files (requires Analyst)
* Agilent .D folders (requires Chemstation)
* Micromass files (requires MassLynx)
* Bruker acqu files
* IMS .IMF and .UIMF files

DeconTools has both a GUI version (DeconToolsAutoProcessor) and a command-line version (DeconConsole).  

Download ProteoWizard from http://proteowizard.sourceforge.net/
You typically will need the 64-bit version.

== Results files ==

For each dataset processed, DeconTools creates a pair of .CSV (comma-separated value) files 
containing information on the spectra in the input files and the deisotoped data found.

The _scans.csv file contains information about each mass spectrum (aka scan); columns are:
	scan_num         The scan number, aka spectrum number
	scan_time        The number of minutes from the start of the analysis
	type             1 for MS spectra, 2 for MS/MS spectra
	bpi              The Base Peak Intensity value
	bpi_mz           The m/z of the Base Peak ion
	tic              The Total Ion Current value
	num_peaks        The number of data points in the spectrum above the background noise level
	num_deisotoped   The number of peaks that were successfully deisotoped
	info             For Thermo Raw files, the scan header

The _isos.csv file contains the deisotoped data; columns are:
	scan_num         The spectrum number containing the data point
	charge           The charge determined via the deisotoping process
	abundance        The data point's abundance
	mz               The Mass-to-Charge Ratio for the data point
	fit              The Isotopic fit value: the least square error between the theoretical data and the experimental data. 
	                 Values closer to 0 are better.  Values larger than ~0.15 are typically low quality results.
	average_mw       The Average Mass determined
	monoisotopic_mw  The Monoisotopic Mass determined
	mostabundant_mw  The mass of the most abundant ion in the isotopic distribution
	fwhm             Full width at half maximum
	signal_noise     Signal to noise ratio
	mono_abundance   For 16O/18O processing, the abundance of the 16O peak
	mono_plus2_abundance   For 16O/18O processing, the abundance of the peak 2 Da away from the 16O peak
	flag                   Specialized column for ion mobility (IMS) data
	interference_score     Measures the likelihood that another isotopic distribution is overlapping with the given distribution (0 means no interference)
	

-------------------------------------------------------------------------------
Written by Gordon Slysz for the Department of Energy (PNNL, Richland, WA)

E-mail: matthew.monroe@pnnl.gov or samuel.payne@pnnl.gov
Website: http://panomics.pnnl.gov/ or http://omics.pnl.gov
-------------------------------------------------------------------------------

Licensed under the Apache License, Version 2.0; you may not use this file except 
in compliance with the License.  You may obtain a copy of the License at 
http://www.apache.org/licenses/LICENSE-2.0

All publications that result from the use of this software should include 
the following acknowledgment statement:
 Portions of this research were supported by the W.R. Wiley Environmental 
 Molecular Science Laboratory, a national scientific user facility sponsored 
 by the U.S. Department of Energy's Office of Biological and Environmental 
 Research and located at PNNL.  PNNL is operated by Battelle Memorial Institute 
 for the U.S. Department of Energy under contract DE-AC05-76RL0 1830.

Notice: This computer software was prepared by Battelle Memorial Institute, 
hereinafter the Contractor, under Contract No. DE-AC05-76RL0 1830 with the 
Department of Energy (DOE).  All rights in the computer software are reserved 
by DOE on behalf of the United States Government and the Contractor as 
provided in the Contract.  NEITHER THE GOVERNMENT NOR THE CONTRACTOR MAKES ANY 
WARRANTY, EXPRESS OR IMPLIED, OR ASSUMES ANY LIABILITY FOR THE USE OF THIS 
SOFTWARE.  This notice including this sentence must appear on any copies of 
this computer software.
