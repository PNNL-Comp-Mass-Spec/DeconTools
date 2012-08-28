////////////////////////////////////////////////////////////////////////////////////
// This is a library of functions to write and extract data from UIMF files
// Authors: Yan Shi, William Danielson III, and Anuj Shah
// Pacific Northwest National Laboratory
// December 2008
////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Data.SQLite;

namespace UIMFLibrary
{

    public class GlobalParameters
    {
		//public DateTime DateStarted;         // 1, Date Experiment was acquired 
        public string DateStarted;
        public int NumFrames;                  // 2, Number of frames in dataset
        public int TimeOffset;                 // 3, Offset from 0. All bin numbers must be offset by this amount
        public double BinWidth;                // 4, Width of TOF bins (in ns)
        public int Bins;                       // 5, Total number of TOF bins in frame
		public float TOFCorrectionTime;
        public float FrameDataBlobVersion;     // 6, Version of FrameDataBlob in T_Frame
        public float ScanDataBlobVersion;      // 7, Version of ScanInfoBlob in T_Frame
        public string TOFIntensityType;        // 8, Data type of intensity in each TOF record (ADC is int/TDC is short/FOLDED is float)
        public string DatasetType;             // 9, Type of dataset (HMS/HMSMS/HMS-MSn)
		public int Prescan_TOFPulses;		   // 10 - 14, Prescan parameter
		public int Prescan_Accumulations;
		public int Prescan_TICThreshold;
		public bool Prescan_Continuous;		   // True or False
		public string Prescan_Profile;	       // If continuous is true, set this to NULL;
        public string InstrumentName;          // Name of the system on which data was acquired
        
	}
    public class FrameParameters
    {
        public int FrameNum;                           // 0, Frame number (primary key)     
        public double StartTime;                       // 1, Start time of frame, in minutes
        public double Duration;                        // 2, Duration of frame, in seconds 
        public int Accumulations;                      // 3, Number of collected and summed acquisitions in a frame 
        public DataReader.FrameType FrameType;         // 4, Bitmap: 0=MS (Legacy); 1=MS (Regular); 2=MS/MS (Frag); 3=Calibration; 4=Prescan
        public int Scans;                              // 5, Number of TOF scans  
        public string IMFProfile;			           // 6, IMFProfile Name; this stores the name of the sequence used to encode the data when acquiring data multiplexed
        public double TOFLosses;			           // 7, Number of TOF Losses
        public double AverageTOFLength;                // 8, Average time between TOF trigger pulses
        public double CalibrationSlope;                // 9, Value of k0  
        public double CalibrationIntercept;            // 10, Value of t0  
        public double a2;	                           // 11, These six parameters below are coefficients for residual mass error correction
        public double b2;	                           // 12, ResidualMassError=a2t+b2t^3+c2t^5+d2t^7+e2t^9+f2t^11
        public double c2;                              // 13
        public double d2;                              // 14
        public double e2;                              // 15
        public double f2;                              // 16
        public double Temperature;                     // 17, Ambient temperature
        public double voltHVRack1;                     // 18, Voltage setting in the IMS system
        public double voltHVRack2;                     // 19, Voltage setting in the IMS system
        public double voltHVRack3;                     // 20, Voltage setting in the IMS system
        public double voltHVRack4;                     // 21, Voltage setting in the IMS system
        public double voltCapInlet;                    // 22, Capillary Inlet Voltage
        public double voltEntranceHPFIn;               // 23, HPF In Voltage  (renamed from voltEntranceIFTIn  to voltEntranceHPFIn  in July 2011)
        public double voltEntranceHPFOut;              // 24, HPF Out Voltage (renamed from voltEntranceIFTOut to voltEntranceHPFOut in July 2011)
        public double voltEntranceCondLmt;             // 25, Cond Limit Voltage
        public double voltTrapOut;                     // 26, Trap Out Voltage
        public double voltTrapIn;                      // 27, Trap In Voltage
        public double voltJetDist;                     // 28, Jet Disruptor Voltage
        public double voltQuad1;                       // 29, Fragmentation Quadrupole Voltage
        public double voltCond1;                       // 30, Fragmentation Conductance Voltage
        public double voltQuad2;                       // 31, Fragmentation Quadrupole Voltage
        public double voltCond2;                       // 32, Fragmentation Conductance Voltage
        public double voltIMSOut;                      // 33, IMS Out Voltage
        public double voltExitHPFIn;                   // 34, HPF In Voltage  (renamed from voltExitIFTIn  to voltExitHPFIn  in July 2011)
        public double voltExitHPFOut;                  // 35, HPF Out Voltage (renamed from voltExitIFTOut to voltExitHPFOut in July 2011)
        public double voltExitCondLmt;                 // 36, Cond Limit Voltage
        public double PressureFront;                   // 37, Pressure at front of Drift Tube 
        public double PressureBack;                    // 38, Pressure at back of Drift Tube 
        public short MPBitOrder;                       // 39, Determines original size of bit sequence 
        public double[] FragmentationProfile;          // 40, Voltage profile used in fragmentation
        public double HighPressureFunnelPressure;      // 41
        public double IonFunnelTrapPressure;           // 42
        public double RearIonFunnelPressure;           // 43
        public double QuadrupolePressure;              // 44
        public double ESIVoltage;                      // 45
        public double FloatVoltage;                    // 46
        public int CalibrationDone = -1;               // 47, Set to 1 after a frame has been calibrated
        public int Decoded = 0;                        // 48, Set to 1 after a frame has been decoded (added June 27, 2011)

		/// <summary>
		/// This constructor assumes the developer will manually store a value in StartTime
		/// </summary>
		public FrameParameters() {
		}

		/// <summary>
		/// This constructor auto-populates StartTime using Now minutes dtRunStartTime using the correct format
		/// </summary>
		/// <param name="dtRunStartTime"></param>
		public FrameParameters(System.DateTime dtRunStartTime) {
			StartTime = System.DateTime.UtcNow.Subtract(dtRunStartTime).TotalMinutes;
		}

        /// <summary>
        /// Included for backwards compatibility
        /// </summary>
        public double voltEntranceIFTOut
        {
            get { return this.voltEntranceHPFOut; }
            set { this.voltEntranceHPFOut = value; }
        }

        /// <summary>
        /// Included for backwards compatibility
        /// </summary>
        public double voltEntranceIFTIn
        {
            get { return this.voltEntranceHPFIn; }
            set { this.voltEntranceHPFIn = value; }
        }

        /// <summary>
        /// Included for backwards compatibility
        /// </summary>
        public double voltExitIFTOut
        {
            get { return this.voltExitHPFOut; }
            set { this.voltExitHPFOut = value; }
        }

        /// <summary>
        /// Included for backwards compatibility
        /// </summary>
        public double voltExitIFTIn
        {
            get { return this.voltExitHPFIn; }
            set { this.voltExitHPFIn = value; }
        }


        public void CopyTo(out FrameParameters Target)
        {
            Target = new FrameParameters();

            Target.FrameNum = this.FrameNum;
            Target.StartTime = this.StartTime;
            Target.Duration = this.Duration;
            Target.Accumulations = this.Accumulations;
            Target.FrameType = this.FrameType;
            Target.Scans = this.Scans;
            Target.IMFProfile = this.IMFProfile;
            Target.TOFLosses = this.TOFLosses;
            Target.AverageTOFLength = this.AverageTOFLength;
            Target.CalibrationSlope = this.CalibrationSlope;
            Target.CalibrationIntercept = this.CalibrationIntercept;
            Target.a2 = this.a2;
            Target.b2 = this.b2;
            Target.c2 = this.c2;
            Target.d2 = this.d2;
            Target.e2 = this.e2;
            Target.f2 = this.f2;
            Target.Temperature = this.Temperature;
            Target.voltHVRack1 = this.voltHVRack1;
            Target.voltHVRack2 = this.voltHVRack2;
            Target.voltHVRack3 = this.voltHVRack3;
            Target.voltHVRack4 = this.voltHVRack4;
            Target.voltCapInlet = this.voltCapInlet;
            Target.voltEntranceHPFIn = this.voltEntranceHPFIn;
            Target.voltEntranceHPFOut = this.voltEntranceHPFOut;
            Target.voltEntranceCondLmt = this.voltEntranceCondLmt;
            Target.voltTrapOut = this.voltTrapOut;
            Target.voltTrapIn  = this.voltTrapIn ;
            Target.voltJetDist = this.voltJetDist;
            Target.voltQuad1 = this.voltQuad1;
            Target.voltCond1 = this.voltCond1;
            Target.voltQuad2 = this.voltQuad2;
            Target.voltCond2 = this.voltCond2;
            Target.voltIMSOut = this.voltIMSOut;
            Target.voltExitHPFIn = this.voltExitHPFIn;
            Target.voltExitHPFOut = this.voltExitHPFOut;
            Target.voltExitCondLmt = this.voltExitCondLmt;
            Target.PressureFront = this.PressureFront;
            Target.PressureBack = this.PressureBack;
            Target.MPBitOrder = this.MPBitOrder;

            if (FragmentationProfile != null)
            {
                Target.FragmentationProfile = new double[this.FragmentationProfile.Length];
                Array.Copy(this.FragmentationProfile, Target.FragmentationProfile, this.FragmentationProfile.Length);
            }
            
            Target.HighPressureFunnelPressure = this.HighPressureFunnelPressure;
            Target.IonFunnelTrapPressure = this.IonFunnelTrapPressure;
            Target.RearIonFunnelPressure = this.RearIonFunnelPressure;
            Target.QuadrupolePressure = this.QuadrupolePressure;
            Target.ESIVoltage = this.ESIVoltage;
            Target.FloatVoltage = this.FloatVoltage;
            Target.CalibrationDone = this.CalibrationDone;
            Target.Decoded = this.Decoded;
        }
    }


    // /////////////////////////////////////////////////////////////////////
    // Calibrate TOF to m/z according to formula mass = (k * (t-t0))^2
    //
    public class MZ_Calibrator
    {
        private double K;
        private double T0;

        public MZ_Calibrator(double k, double t0)
        {
            this.K = k;
            this.T0 = t0;
        }

        public double TOFtoMZ(double TOFValue)
        {
            double r = this.K * (TOFValue - this.T0);
            return r * r;
        }

        public int MZtoTOF(double mz)
        {
            double r = (Math.Sqrt(mz));
            return (int)(((r / this.K) + this.T0) + .5); // .5 for rounding
        }

        public string Description
        {
            get
            {
                return "mz = (k*(t-t0))^2";
            }
        }

        public double k
        {
            get { return this.K; }
            set { this.K = value; }
        }

        public double t0
        {
            get { return this.T0; }
            set { this.T0 = value; }
        }
    }
}
