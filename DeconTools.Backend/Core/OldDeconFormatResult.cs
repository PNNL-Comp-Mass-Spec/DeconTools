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
        private int peakIndex;

        public int PeakIndex
        {
            get { return peakIndex; }
            set { peakIndex = value; }
        }
        private int scanNum;

        public int ScanNum
        {
            get { return scanNum; }
            set { scanNum = value; }
        }
        private short chargeState;

        public short ChargeState
        {
            get { return chargeState; }
            set { chargeState = value; }
        }
        private int abundance;

        public int Abundance
        {
            get { return abundance; }
            set { abundance = value; }
        }
        private double mz;

        public double Mz
        {
            get { return mz; }
            set { mz = value; }
        }
        private double fit;

        public double Fit
        {
            get { return fit; }
            set { fit = value; }
        }
        private double averageMW;

        public double AverageMW
        {
            get { return averageMW; }
            set { averageMW = value; }
        }
        private double monoMW;

        public double MonoMW
        {
            get { return monoMW; }
            set { monoMW = value; }
        }
        private double mostIntenseMW;

        public double MostIntenseMW
        {
            get { return mostIntenseMW; }
            set { mostIntenseMW = value; }
        }
        private double fwhm;

        public double Fwhm
        {
            get { return fwhm; }
            set { fwhm = value; }
        }
        private double signalToNoise;

        public double SignalToNoise
        {
            get { return signalToNoise; }
            set { signalToNoise = value; }
        }
        private int monoIntensity;

        public int MonoIntensity
        {
            get { return monoIntensity; }
            set { monoIntensity = value; }
        }
        private int iplusTwoIntensity;

        public int IplusTwoIntensity
        {
            get { return iplusTwoIntensity; }
            set { iplusTwoIntensity = value; }
        }
        private double delta;

        public double Delta
        {
            get { return delta; }
            set { delta = value; }
        }
        private int numIsotopesObserved;

        public int NumIsotopesObserved
        {
            get { return numIsotopesObserved; }
            set { numIsotopesObserved = value; }
        }


    }
}
