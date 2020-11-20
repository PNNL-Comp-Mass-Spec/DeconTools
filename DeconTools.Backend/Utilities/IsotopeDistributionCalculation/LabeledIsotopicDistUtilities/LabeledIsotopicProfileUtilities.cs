using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using PNNLOmics.Data.Constants;

namespace DeconTools.Backend.Utilities.IsotopeDistributionCalculation.LabeledIsotopicDistUtilities
{
    public class LabeledIsotopicProfileUtilities
    {
        private readonly PeptideUtils _peptideUtils = new PeptideUtils();

        public LabeledIsotopicProfileUtilities()
        {
            IsotopicProfiles = new List<IsotopicProfileComponent>();
        }

        #region Public Methods

        public List<IsotopicProfileComponent> IsotopicProfiles { get; set; }

        public void AddIsotopicProfile(IsotopicProfile iso, double fraction)
        {
            var component = new IsotopicProfileComponent(iso, fraction);
            IsotopicProfiles.Add(component);
        }

        public IsotopicProfile GetMixedIsotopicProfile()
        {
            IsotopicProfile mixedIso = null;

            foreach (var isotopicProfileComponent in IsotopicProfiles)
            {
                if (mixedIso == null)
                {
                    mixedIso = isotopicProfileComponent.IsotopicProfile.CloneIsotopicProfile();
                    mixedIso.Peaklist.Clear();
                }

                //we need to first make sure all the intensities of the peaks add up to 1 for each isotopic profile
                //then we need to adjust the ratios of each peak according to the fractional amount the isotopic profile contributes to the mixture
                var iso = isotopicProfileComponent.IsotopicProfile.CloneIsotopicProfile();
                var sumIntensities = iso.Peaklist.Sum(p => p.Height);

                foreach (var msPeak in iso.Peaklist)
                {
                    msPeak.Height = (float)(msPeak.Height / sumIntensities * isotopicProfileComponent.Fraction);
                }

                for (var i = 0; i < iso.Peaklist.Count; i++)
                {
                    var currentPeak = iso.Peaklist[i];
                    var indexOfPeak = GetIndexOfTargetPeak(mixedIso, currentPeak.XValue, out var mixedIsoContainsMZ);

                    if (mixedIsoContainsMZ)
                    {
                        mixedIso.Peaklist[indexOfPeak].Height += currentPeak.Height;
                    }
                    else
                    {
                        var addedPeak = new MSPeak(currentPeak.XValue, currentPeak.Height, currentPeak.Width, currentPeak.SignalToNoise)
                        {
                            MSFeatureID = currentPeak.MSFeatureID,
                            DataIndex = currentPeak.DataIndex
                        };

                        var insertionIndex = indexOfPeak - 1;    //the above method returns the m/z of the next highest peak

                        if (insertionIndex >= 0)
                        {
                            mixedIso.Peaklist.Insert(insertionIndex, addedPeak);
                        }
                        else
                        {
                            mixedIso.Peaklist.Add(addedPeak);
                        }
                    }
                }
            }

            return mixedIso;
        }

        public IsotopicProfile CreateIsotopicProfileFromSequence(string peptideSequenceOrOtherCode, string elementLabeled, int lightIsotope, int heavyIsotope, double percentHeavyLabel, int chargeState = 1)
        {
            var baseEmpiricalFormula = _peptideUtils.GetEmpiricalFormulaForPeptideSequence(peptideSequenceOrOtherCode);

            return CreateIsotopicProfileFromEmpiricalFormula(baseEmpiricalFormula, elementLabeled, lightIsotope,
                                                             heavyIsotope, percentHeavyLabel, chargeState);
        }

        public IsotopicProfile CreateIsotopicProfileFromEmpiricalFormula(string baseEmpiricalFormula, string elementLabeled, int lightIsotope, int heavyIsotope, double percentHeavyLabel, int chargeState = 1)
        {
            var isUnlabeled = elementLabeled == "" || Math.Abs(percentHeavyLabel) < float.Epsilon;

            IsotopicProfile iso;
            if (isUnlabeled)
            {
                iso = IsotopicDistributionCalculator.Instance.GetIsotopePattern(baseEmpiricalFormula);
            }
            else
            {
                var abundanceLightIsotopeLabeled1 = CalculateAbundanceLightIsotope(elementLabeled, lightIsotope, percentHeavyLabel);
                var abundanceHeavyIsotopeLabeled1 = CalculateAbundanceHeavyIsotope(elementLabeled, heavyIsotope, percentHeavyLabel);

                IsotopicDistributionCalculator.Instance.SetLabeling(elementLabeled, lightIsotope, abundanceLightIsotopeLabeled1, heavyIsotope, abundanceHeavyIsotopeLabeled1);
                iso = IsotopicDistributionCalculator.Instance.GetIsotopePattern(baseEmpiricalFormula);

                IsotopicDistributionCalculator.Instance.ResetToUnlabeled();
            }

            var monoisotopicMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(baseEmpiricalFormula);
            iso.MonoIsotopicMass = monoisotopicMass;
            CalculateMZValuesForLabeledProfile(iso, baseEmpiricalFormula, elementLabeled, chargeState,
                                               lightIsotope, heavyIsotope);

            iso.ChargeState = chargeState;

            return iso;
        }

        public bool ValidateElement(string elementSymbol)
        {
            try
            {
                var unused = Constants.Elements[elementSymbol];
            }
            catch (KeyNotFoundException)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Private methods
        private void CalculateMZValuesForLabeledProfile(IsotopicProfile iso, string empiricalFormula, string elementLabeled, int chargeState, int lightIsotope, int heavyIsotope)
        {
            var elementTable = EmpiricalFormulaUtilities.ParseEmpiricalFormulaString(empiricalFormula);

            var numLabeledAtoms = elementTable[elementLabeled];

            for (var i = 0; i < iso.Peaklist.Count; i++)
            {
                var keyLightIsotope = elementLabeled + lightIsotope;
                var keyHeavyIsotope = elementLabeled + heavyIsotope;

                var lightIsotopeMass = Constants.Elements[elementLabeled].IsotopeDictionary[keyLightIsotope].Mass;
                var heavyIsotopeMass = Constants.Elements[elementLabeled].IsotopeDictionary[keyHeavyIsotope].Mass;

                var massDiff = heavyIsotopeMass - lightIsotopeMass;

                var monoMZ = iso.MonoIsotopicMass / chargeState + Globals.PROTON_MASS;

                var peakMZIfUnlabeled = monoMZ + (i * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS) / chargeState;

                var monoPeakFullyLabeled = monoMZ + massDiff * numLabeledAtoms / chargeState;

                var peakMZBasedOnLabeled = monoPeakFullyLabeled -
                                           (numLabeledAtoms - i) * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS / chargeState;

                int peaksToUse;
                if (i > numLabeledAtoms)
                {
                    peaksToUse = numLabeledAtoms;
                }
                else
                {
                    peaksToUse = i;
                }

                iso.Peaklist[i].XValue = peakMZBasedOnLabeled * peaksToUse / numLabeledAtoms +
                                         peakMZIfUnlabeled * (numLabeledAtoms - peaksToUse) / numLabeledAtoms;
            }
        }

        private double CalculateAbundanceLightIsotope(string elementSymbol, int isotopeNum, double percentAddedLabeling)
        {
            var elementIsValid = ValidateElement(elementSymbol);
            if (!elementIsValid) return 0;

            var key = elementSymbol + isotopeNum;
            var isotope = Constants.Elements[elementSymbol].IsotopeDictionary[key];

            var labelChange = percentAddedLabeling;

            return isotope.NaturalAbundance - labelChange / 100;
        }

        private double CalculateAbundanceHeavyIsotope(string elementSymbol, int isotopeNum, double percentAddedLabeling)
        {
            var elementIsValid = ValidateElement(elementSymbol);
            if (!elementIsValid) return 0;

            var key = elementSymbol + isotopeNum;
            var isotope = Constants.Elements[elementSymbol].IsotopeDictionary[key];

            var labelChange = percentAddedLabeling;

            return isotope.NaturalAbundance + labelChange / 100;
        }

        private int GetIndexOfTargetPeak(IsotopicProfile mixedIso, double xValue, out bool peakWasFound, double toleranceInPPM = 1)
        {
            var toleranceInMZ = toleranceInPPM * xValue / 1e6;

            var indexOfPeak = 0;

            peakWasFound = false;

            for (var i = 0; i < mixedIso.Peaklist.Count; i++)
            {
                var currentPeakMZ = mixedIso.Peaklist[i].XValue;

                if (Math.Abs(currentPeakMZ - xValue) <= toleranceInMZ)
                {
                    indexOfPeak = i;
                    peakWasFound = true;
                    break;
                }

                if (currentPeakMZ > xValue)      // purpose is to report index of the peak that
                {
                    indexOfPeak = i;
                    peakWasFound = false;
                    break;
                }
            }

            return indexOfPeak;
        }

        #endregion
    }
}
