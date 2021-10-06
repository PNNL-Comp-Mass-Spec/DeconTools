# __<span style="color:#D57500">DeconTools (Decon2LS)</span>__
Used to de-isotope mass spectra and to detect features from mass spectrometry data using observed isotopic signatures.

### Description
DeconTools is a software package used for detecting features in MS data by using the isotopic signatures of expected components. The tool uses different algorithms for different parts of the deconvolution process: noise reduction, peak detection, prediction of theoretical isotopic envelope and scoring functions that quantitate the quality of signature observed in the data.

DeconTools can read the following file formats. However, some of the file formats require that you install the vendor-supplied data browsing software so that Decon2LS has access to the necessary DLLs.

* mzXML files (requires ProteoWizard installation)
* mzML files (requires ProteoWizard installation)
* mz5 files (requires ProteoWizard installation)
* Thermo Finnigan .Raw files (requires Xcalibur or MSFileReader)
  * Download Thermo MSFileReader v3.x by creating an account at https://thermo.flexnetoperations.com/control/thmo/login then login and choose "Utility Software". Look for MSFileReader 3.1 SP4 or similar.
  * Run the installer and install both the 32-bit and 64-bit versions.
* Agilent .D folders (requires Chemstation)
* Bruker acqu files
* IMS .IMF and .UIMF files

DeconTools has both a GUI version (DeconToolsAutoProcessor) and a command-line version (DeconConsole). Note that DeconTools is the next generation release of Decon2LS, an application developed by Navdeep Jaitly. Furthermore, DeconTools is the .NET version of the ICR2LS application developed by Gordon Anderson.

Download ProteoWizard from http://proteowizard.sourceforge.net/. <br>
You typically will need the 64-bit version.

A PDF tutorial (see "Downloads" below) is available for DeconTools. In addition, the "[Data Extraction and Analysis for LC-MS Based Proteomics](http://panomics.pnnl.gov/training/workshops/)" sessions at the 2007 and 2008 US HUPO conferences discussed the use of Decon2LS for processing example LC-MS datasets to allow [VIPER](http://pnnl-comp-mass-spec.github.io/VIPER/) to match the data to an AMT tag database. PDF files of the slides presented are available for download as a [5 MB PDF file (2007)](http://panomics.pnnl.gov/training/workshops/2007HUPO/LCMSBasedProteomicsDataProcessing.pdf) and a [6.5 MB PDF file (2008)](http://panomics.pnnl.gov/training/workshops/2008HUPO/LCMSBasedProteomicsDataProcessing2008.pdf).

See also the [Open Source Tools for the Accurate Mass and Time (AMT) Tag Proteomics Pipeline](http://panomics.pnnl.gov/posters/ASMS2006.stm#Jaitly) poster presented by Navdeep Jaitly at ASMS 2006. This poster presents an overview of Decon2LS and its interactions with other proteomics software.

### Downloads
* [Latest version](https://github.com/PNNL-Comp-Mass-Spec/DeconTools/releases/latest)
* [Source code on GitHub](https://github.com/PNNL-Comp-Mass-Spec/DeconTools)
* [Tutorial](DeconTools_Tutorial.pdf)

#### Software Instructions
Note: As of November 13, 2015, the installer has changed and will not uninstall older (pre-November 2015) versions of DeconTools when it installs. We recommend uninstalling any older version of DeconTools. Also, due to complications with the usage of the ProteoWizard pwiz_bindings_cli dll, DeconTools now requires that 64-bit ProteoWizard be installed in the default 64-bit ProteoWizard installation directory when trying to use mzML, mzXML or mz5 files.

### Acknowledgment

All publications that utilize this software should provide appropriate acknowledgement to PNNL and the DeconTools GitHub repository. However, if the software is extended or modified, then any subsequent publications should include a more extensive statement, as shown in the Readme file for the given application or on the website that more fully describes the application.

### Disclaimer

These programs are primarily designed to run on Windows machines. Please use them at your own risk. This material was prepared as an account of work sponsored by an agency of the United States Government. Neither the United States Government nor the United States Department of Energy, nor Battelle, nor any of their employees, makes any warranty, express or implied, or assumes any legal liability or responsibility for the accuracy, completeness, or usefulness or any information, apparatus, product, or process disclosed, or represents that its use would not infringe privately owned rights.

Portions of this research were supported by the NIH National Center for Research Resources (Grant RR018522), the W.R. Wiley Environmental Molecular Science Laboratory (a national scientific user facility sponsored by the U.S. Department of Energy's Office of Biological and Environmental Research and located at PNNL), and the National Institute of Allergy and Infectious Diseases (NIH/DHHS through interagency agreement Y1-AI-4894-01). PNNL is operated by Battelle Memorial Institute for the U.S. Department of Energy under contract DE-AC05-76RL0 1830.

We would like your feedback about the usefulness of the tools and information provided by the Resource. Your suggestions on how to increase their value to you will be appreciated. Please e-mail any comments to proteomics@pnl.gov
