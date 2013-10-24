DeconTools is a software package used for detecting features in MS data by using the isotopic signatures of
expected components. The tool uses different algorithms for different parts of the deconvolution process: noise
reduction, peak detection, prediction of theoretical isotopic envelope and scoring functions that quantitate
the quality of signature observed in the data.

DeconTools can read the following file formats.  However, some of the file formats require that you 
install the vendor-supplied data browsing software so that DeconTools has access to the necessary DLLs.
* mzXML files
* mzML files
* mz5 files
* Thermo Finnigan .Raw files (requires Xcalibur or MSFileReader)
  * Download MSFileReader from http://sjsupport.thermofinnigan.com/public/detail.asp?id=703
  * When the installer offers you the option of the version to install, be sure to install the 32-bit version.
* Agilent .Wiff files (requires Analyst)
* Agilent .D folders (requires Chemstation)
* Micromass files (requires MassLynx)
* Bruker acqu files
* IMS .IMF and .UIMF files

DeconTools has both a GUI version (DeconToolsAutoProcessor) and a command-line version (DeconConsole).  

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
