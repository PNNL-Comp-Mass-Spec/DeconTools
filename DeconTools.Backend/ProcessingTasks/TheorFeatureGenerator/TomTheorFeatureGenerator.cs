﻿using System;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;
using DeconTools.Utilities;

namespace DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator
{
    public class TomTheorFeatureGenerator : ITheorFeatureGenerator
    {
        #region Constructors

        readonly TomIsotopicPattern _tomIsotopicPatternGenerator = new TomIsotopicPattern();

        readonly N15IsotopeProfileGenerator _N15IsotopicProfileGenerator = new N15IsotopeProfileGenerator();

        public TomTheorFeatureGenerator()
            : this(Globals.LabelingType.NONE, 0.005)
        {
        }

        public TomTheorFeatureGenerator(Globals.LabelingType labelingType, double lowPeakCutOff)
        {
            LabelingType = labelingType;
            LowPeakCutOff = lowPeakCutOff;
        }
        #endregion

        #region Properties

        public Globals.LabelingType LabelingType { get; set; }

        /// <summary>
        /// Peaks below the cutoff will be trimmed out from the theoretical profile
        /// </summary>
        public double LowPeakCutOff { get; set; }

        #endregion

        #region Public Methods
        public override void GenerateTheorFeature(TargetBase mt)
        {
            Check.Require(mt != null, "FeatureGenerator failed. MassTag not defined.");
            if (mt == null)
            {
                return;
            }

            Check.Require(mt.EmpiricalFormula != null, "Theoretical feature generator failed. Can't retrieve empirical formula from Mass Tag");

            switch (LabelingType)
            {
                case Globals.LabelingType.NONE:

                    mt.IsotopicProfile = GetUnlabeledIsotopicProfile(mt);
                    break;
                case Globals.LabelingType.O18:
                    throw new NotImplementedException();
                case Globals.LabelingType.N15:
                    mt.IsotopicProfileLabeled = _N15IsotopicProfileGenerator.GetN15IsotopicProfile(mt, LowPeakCutOff);

                    break;
                default:
                    break;
            }
        }

        private IsotopicProfile GetUnlabeledIsotopicProfile(TargetBase mt)
        {
            IsotopicProfile iso;

            try
            {
                iso = _tomIsotopicPatternGenerator.GetIsotopePattern(mt.EmpiricalFormula, _tomIsotopicPatternGenerator.aafIsos);
            }
            catch (Exception ex)
            {
                throw new Exception("Theoretical feature generator failed. Details: " + ex.Message);
            }

            PeakUtilities.TrimIsotopicProfile(iso, LowPeakCutOff);
            iso.ChargeState = mt.ChargeState;
            if (iso.ChargeState != 0)
            {
                calculateMassesForIsotopicProfile(iso, mt);
            }

            return iso;
        }

        #endregion

        #region Private Methods
        #endregion

        private void calculateMassesForIsotopicProfile(IsotopicProfile iso, TargetBase mt)
        {
            if (iso?.Peaklist == null)
            {
                return;
            }

            for (var i = 0; i < iso.Peaklist.Count; i++)
            {
                var calcMZ = mt.MonoIsotopicMass / mt.ChargeState + Globals.PROTON_MASS + i * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / mt.ChargeState;
                iso.Peaklist[i].XValue = calcMZ;
            }

            iso.MonoPeakMZ = iso.Peaklist[0].XValue;
            iso.MonoIsotopicMass = (iso.MonoPeakMZ - Globals.PROTON_MASS) * mt.ChargeState;
        }

        public override void LoadRunRelatedInfo(ResultCollection results)
        {
            // do nothing
        }
    }
}
