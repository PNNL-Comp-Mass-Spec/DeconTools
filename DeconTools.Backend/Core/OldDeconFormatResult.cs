using System;
using System.Collections.Generic;
using System.Text;

namespace DeconTools.Backend.Core
{
    
    //this class was created for the purpose of interacting with older Decon2LS and it's clsProcRunner.cls
    //We needed a way of getting RAPID deconvoluted results into the clsProcRunner.  So this class was created
    //And the Rapiddeconvolutor has a method that returns an array of this class. 
    //This class mimics the IsofitRecord class of the DeconEngine...
    public class OldDeconFormatResult
    {
        public int PeakIndex { get; set; }

        public int ScanNum { get; set; }

        public short ChargeState { get; set; }

        public int Abundance { get; set; }

        public double Mz { get; set; }

        public double Fit { get; set; }

        public double AverageMW { get; set; }

        public double MonoMW { get; set; }

        public double MostIntenseMW { get; set; }

        public double Fwhm { get; set; }

        public double SignalToNoise { get; set; }

        public int MonoIntensity { get; set; }

        public int IplusTwoIntensity { get; set; }

        public double Delta { get; set; }

        public int NumIsotopesObserved { get; set; }
    }
}
