==============
UIMF Library
==============

This software package includes a library of C# functions to create, modify and extract data from uimf files.

How to use UIMFLibrary in C#:
------------------------------   
    1) Copy UIMFLibrary.dll & System.Data.SQLite.DLL from UIMFLibrary\bin\Debug\x86 folder to 
       your project's bin\debug or bin\release folder
    2) Copy IMSCOMP.dll from the lib\x86 folder to your project's bin\debug or bin\release folder
    3) Open the Solution Explorer, add UIMFLibrary to your Project/References
    4) Add this using statement at the top of your code: using UIMFLibrary;
    5) Declare the DataReader and DataWriter classes in your main code:

	UIMFReader DataReader = new UIMFReader();
	UIMFWriter DataWriter = new UIMFWriter();

    5) Now the functions can be called as such:

	DataReader.set_FrameType(iFrameType.MS);
	DataReader.GetSpectrum(frame_index, scanNum, mzArray, intensityArray);
		......

	DataWriter.OpenUIMF(FileName);
	DataWriter.CreateTables();


How to use UIMFLibrary in C++:
-------------------------------   
    1) Copy UIMFLibrary.dll & System.Data.SQLite.DLL from UIMFLibrary\bin\Debug\x86 folder to 
       your project's bin\debug or bin\release folder
    2) Copy IMSCOMP.dll from the lib\x86 folder to your project's bin\debug or bin\release folder
    3) Open the Solution Explorer, add UIMFLibrary to your Project/References
    4) Add this using statement at the top of your code: using namespace UIMFLibrary;
    5) In your .h file, declare the DataReader and DataWriter classes: 

	gcroot<UIMFReader*> ReadUIMF;
	gcroot<UIMFWriter*> WriteUIMF;

    6) In you .cpp file, instantantiate the reader and the writer:
	ReadUIMF = new UIMFReader();
	WriteUIMF = new UIMFWriter();

    7) Now the functions can be called as such:

	bool status;
	status = DataWriter->OpenUIMF(FileName);
	status = ReadUIMF->GetSpectrum(frame_index, scanNum, mzArray, intensityArray);
		......

	WriteUIMF->OpenUIMF(FileName);
	WriteUIMF->CreateTables();


============================================================
As of April 2011, the following information is out-of-date
============================================================

Functions in UIMFReader Class:
-------------------------------
	bool OpenUIMF(string FileName)
	bool CloseUIMF(string FileName)

	object GetGlobalParameters()
	object GetGlobalParameters(string ParameterName)
	object GetFrameParameters(int FrameNum)
	object GetFrameParameters(int FrameNum, string ParameterName)

	int GetCountPerSpectrum(int frame_num, int scan_num)

	int SumScans(double[] mzs, double[] intensities, int frameType, int startFrame, int endFrame, int startScan, int endScan)
	int SumScans(double[] mzs, double[] intensities, int frameType, int startFrame, int endFrame, int scanNum)
	int SumScans(double[] mzs, double[] intensities, int frameType, int startFrame, int endFrame)
	int SumScans(double[] mzs, double[] intensities, int frameType, int frameNum)

	int SumScans(double[] mzs, int[] intensities, int frameType, int startFrame, int endFrame, int startScan, int endScan)
	int SumScans(double[] mzs, int[] intensities, int frameType, int startFrame, int endFrame, int scanNum)
	int SumScans(double[] mzs, int[] intensities, int frameType, int startFrame, int endFrame)
	int SumScans(double[] mzs, int[] intensities, int frameType, int frameNum)

	int SumScans(double[] mzs, float[] intensities, int frameType, int startFrame, int endFrame, int startScan, int endScan)
	int SumScans(double[] mzs, float[] intensities, int frameType, int startFrame, int endFrame, int scanNum)
	int SumScans(double[] mzs, float[] intensities, int frameType, int startFrame, int endFrame)
	int SumScans(double[] mzs, float[] intensities, int frameType, int frameNum)

	int SumScans(double[] mzs, short[] intensities, int frameType, int startFrame, int endFrame, int startScan, int endScan)
	int SumScans(double[] mzs, short[] intensities, int frameType, int startFrame, int endFrame, int scanNum)
	int SumScans(double[] mzs, short[] intensities, int frameType, int startFrame, int endFrame)
	int SumScans(double[] mzs, short[] intensities, int frameType, int frameNum)
	
	void GetTIC(double[] TIC, int frameType, int startFrame, int endFrame, int startScan, int endScan)
	void GetTIC(float[] TIC, int frameType, int startFrame, int endFrame, int startScan, int endScan)
	void GetTIC(int[] TIC, int frameType, int startFrame, int endFrame, int	startScan, int endScan)
	void GetTIC(short[] TIC, int frameType, int startFrame, int endFrame, int startScan, int endScan)

	void GetTIC(ref double TIC, int frameNum, int scanNum)
	void GetTIC(ref float TIC, int frameNum, int scanNum)
	void GetTIC(ref int TIC, int frameNum, int scanNum)
	void GetTIC(ref short TIC, int frameNum, int scanNum)
	
	int GetSpectrum(int frameNum, int scanNum, double []spectrum, double []mzs)
	int GetSpectrum(int frameNum, int scanNum, float []spectrum, double []mzs)
	int GetSpectrum(int frameNum, int scanNum, int []spectrum, double []mzs)
	int GetSpectrum(int frameNum, int scanNum, short []spectrum, double []mzs)

Functions in UIMFWriter Class:
-------------------------------
	void OpenUIMF(string FileName)
	bool CloseUIMF(string FileName)

	void CreateTables(string DataType)

	void InsertGlobal(GlobalParameters header)
	void InsertFrame(FrameParameters fp)

	int InsertScan(int frameNum, int scanNum, double[] intensities, double bin_width, double calibration_slope, double calibration_intercept)
	int InsertScan(int frameNum, int scanNum, float[] intensities, double bin_width, double calibration_slope, double calibration_intercept)
	int InsertScan(int frameNum, int scanNum, int[] intensities, double bin_width, double calibration_slope, double calibration_intercept)
	int InsertScan(int frameNum, int scanNum, short[] intensities, double bin_width, double calibration_slope, double calibration_intercept)

	void UpdateCalibrationCoefficients(int frameNum, float slope, float intercept)
	void AddGlobalParameter(string ParameterName, string ParameterType, string ParameterValue)
	void AddFrameParameter(string ParameterName, string ParameterType)
	void UpdateGlobalParameter(string ParameterName, string Value)
	void UpdateFrameParameter(int frameNum, string ParameterName, string Value)

UIMF database structure:
------------------------
	Three tables are included in the UIMF database file: Global_Parameters, Frame_Parameters, and Frame_Scans.

	Global_Parameters table:
	-----------------------
	Param_ID 	Param_Name 		Param_Type 	Comment
	--------	----------		----------	-------
	1		DateStarted 		date 		Date Experiment was acquired
	2		NumFrames 		long 		Number of frames in dataset
	3		TimeOffset 		long 		Offset from 0. All bin numbers must be offset by this amount
	4		BinWidth 		double 		Width of TOF bins (in ns)
	5		Bins 			long 		Total TOF bins in a frame
	6 		TOFCorrectionTime	float		Time Delay correction
	7		FrameDataBlobVersion 	float 		Version of FrameDataBlob in table Frame_Parameters
	8		ScanDataBlobVersion 	float 		Version of ScanInfoBlob in table Frame_Parameters
	9		TOFIntensityType 	string 		Data type of intensity in each TOF record (ADC is int/TDC is short/FOLDED is float)
	10		DatasetType 		string 		Type of dataset (HMS/HMSMS/HMS-MSn)
	11		Prescan_TOFPulses 	int 		Prescan TOP Pulses
	12		Prescan_Accumulations 	int 		Prescan Accumulations
	13		Prescan_TICThreshold 	int 		Prescan TIC Threshold
	14		Prescan_Continuous 	bool 		Prescan continous mode (True or False)
	15		Prescan_Profile 	string 		Prescan Profile File Name

	Frame_Parameters table:
	----------------------
	Column Name 	 Column Index 	Data Type 	Comment
	-----------	 ------------	---------	-------
	FrameNum 		0 	Integer		(Primary Key) Contains the frame number
	StartTime 		1 	DateTime 	Start time of frame
	Duration 		2 	double 		Duration of frame
	Accumulations 		3 	int 		Number of collected and summed acquisitions in a frame
	FrameType 		4 	short 		Bitmap: 0=MS (Regular); 1=MS/MS (Frag); 2=Prescan; 4=Multiplex
	Scans 			5 	long 		Number of TOF scans
	IMFProfile 		6 	string 		File name for IMF Profile
	TOFLOsses 		7 	double 		TOF Losses
	AverageTOFLength 	8 	double 		Average time between TOF trigger pulses
	CalibrationSlope 	9 	double 		Value of k0
	CalibrationIntercept 	10 	double 		Value of t0
	a2			11	double		secondary coefficients for residual mass error correction
	b2			12	double		secondary coefficients for residual mass error correction
	c2			13	double		secondary coefficients for residual mass error correction
	d2			14	double		secondary coefficients for residual mass error correction
	e2			15	double		secondary coefficients for residual mass error correction
	f2			16	double		secondary coefficients for residual mass error correction	
	Temperature 		17 	double 		Ambient temperature
	voltHVRack1 		18 	double 		HVRack Voltage
	voltHVRack2 		19 	double 		HVRack Voltage
	voltHVRack3 		20 	double 		HVRack Voltage
	voltHVRack4 		21 	double 		HVRack Voltage
	voltCapInlet 		22 	double 		Capilary Inlet Voltage
	voltEntranceIFTIn 	23 	double 		IFT In Voltage
	voltEntranceIFTOut 	24 	double 		IFT Out Voltage
	voltEntranceCondLmt 	25 	double 		Cond Limit Voltage
	voltTrapOut 		26 	double 		Trap Out Voltage
	voltTrapIn 		27 	double 		Trap In Voltage
	voltJetDist 		28 	double 		Jet Disruptor Voltage
	voltQuad1 		29 	double 		Fragmentation Quadrupole Voltage
	voltCond1 		30 	double 		Fragmentation Conductance Voltage
	voltQuad2 		31 	double 		Fragmentation Quadrupole Voltage
	voltCond2 		32 	double 		Fragmentation Conductance Voltage
	voltIMSOut 		33 	double 		IMS Out Voltage
	voltExitIFTIn 		34 	double 		IFT In Voltage
	voltExitIFTOut 		35 	double 		IFT Out Voltage
	voltExitCondLimit 	36 	double 		Cond Limit Voltage
	PressureFront 		37 	double 		Pressure at front of Drift Tube
	PressureBack 		38 	double 		Pressure at back of Drift Tube
	MPBitOrder 		39 	smallint 	Determines original size of bit sequence
	FragmentationProfile 	40 	binary (BLOB) 	Voltage profile used in fragmentation, Length number of Scans
	
	Frame_Scans table:
	-----------------
	Column Name 	Column Index 	Data Type 	 	Comment
	-----------	------------	---------		-------
	FrameNum 	0		Long Int 		Contains the frame number
	ScanNum 	1 		Int 			Contains the TOF pulse number the spectra are located in
	NonZeroCount 	2 		Int 			Nonzero intensities
	BPI 		3 		Double/Float/Int* 	Base Peak Intensity per Scan
	BPI_MZ 		4 		Double/Float/Int 	m/z associated with BPI
	TIC 		5 		Double/Float/Int 	Total Ion Chromatogram per Scan
	Intensities 	6 		binary (BLOB) 	  	Intensities in compressed binary format 


Examples:
---------
To see examples of how to create, modify, and, extract data out of the database, refer to testUIMFLibrary.cs file included in this software package.

---------------------------------------------------------------------------------------------------------------
Written by Yan Shi for the Department of Energy (PNNL, Richland, WA)
Copyright 2009, Battelle Memorial Institute.  All Rights Reserved.

E-mail: yan.shi@pnl.gov or proteomics@pnl.gov
Website: http://ncrr.pnl.gov/software/
----------------------------------------------------------------------------------------------------------------

