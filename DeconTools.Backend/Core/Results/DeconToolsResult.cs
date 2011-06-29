using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeconTools.Backend.Core.Results
{
    
    //this is a result class that is free of specialized objects (as opposed to IsosResult or MassTagResultBase)
    public abstract class DeconToolsResult
    {

        #region Constructors
        #endregion

        #region Properties
        
        /// <summary>
        /// Scan (LC)
        /// </summary>

        public string DatasetName { get; set; }
        public int ScanLC { get; set; }
  
        public int ChargeState { get; set; }
        public double MonoMass { get; set; }
        public double MonoMZ { get; set; }
        public float FitScore { get; set; }
        public float IScore { get; set; }
        
        //Representative intensity for the isotopic profile.
        public float Intensity { get; set; }
        
        /// <summary>
        ///Intensity of the monoisotopic peak
        /// </summary>
        public float IntensityI0 { get; set; }
        
        /// <summary>
        /// Intensity of the most abundant peak of the isotopic profile
        /// </summary>
        public float IntensityMostAbundantPeak { get; set; }
        
        /// <summary>
        /// Index of the most abundant peak of the isotopic profile
        /// </summary>
        public short IndexOfMostAbundantPeak { get; set; }

        #endregion


    }
}
