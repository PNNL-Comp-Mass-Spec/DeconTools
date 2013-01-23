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

        public void AddIsotopicProfile(IsotopicProfile iso, double fraction, string description = "")
        {
            var component = new IsotopicProfileComponent(iso, fraction, description);
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

                for (int i = 0; i < iso.Peaklist.Count; i++)
                {
                    MSPeak currentPeak = iso.Peaklist[i];
                    bool mixedIsoContainsMZ;
                    int indexOfPeak = GetIndexOfTargetPeak(mixedIso, currentPeak.XValue, out mixedIsoContainsMZ);

                    if (mixedIsoContainsMZ)
                    {
                        mixedIso.Peaklist[indexOfPeak].Height += (float)(currentPeak.Height);
                    }
                    else
                    {
                        MSPeak addedPeak = new MSPeak
                        {
                            Height = currentPeak.Height,
                            XValue = currentPeak.XValue,
                            Width = currentPeak.Width,
                            MSFeatureID = currentPeak.MSFeatureID,
                            DataIndex = currentPeak.DataIndex,
                            SignalToNoise = currentPeak.SignalToNoise
                        };

                        addedPeak.Height = (float)(currentPeak.Height);


                        int insertionIndex = indexOfPeak - 1;    //the above method returns the m/z of the next highest peak

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





        public IsotopicProfile CreateIsotopicProfileFromSequence(string peptideSequenceOrOtherCode, string elementLabelled, int lightIsotope, int heavyIsotope, double percentHeavyLabel, int chargeState = 1)
        {

            string baseEmpiricalFormula = _peptideUtils.GetEmpiricalFormulaForPeptideSequence(peptideSequenceOrOtherCode);

            return CreateIsotopicProfileFromEmpiricalFormula(baseEmpiricalFormula, elementLabelled, lightIsotope,
                                                             heavyIsotope, percentHeavyLabel, chargeState);

        }


        public IsotopicProfile CreateIsotopicProfileFromEmpiricalFormula(string baseEmpiricalFormula, string elementLabelled, int lightIsotope, int heavyIsotope, double percentHeavyLabel, int chargeState = 1)
        {
            double monoisotopicMass =
            EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(baseEmpiricalFormula);

            bool isUnlabelled = elementLabelled == "" || percentHeavyLabel == 0;

            IsotopicProfile iso;
            if (isUnlabelled)
            {

                iso = IsotopicDistributionCalculator.Instance.GetIsotopePattern(baseEmpiricalFormula);
            }
            else
            {
                double abundanceLightIsotopeLabeled1 = CalculateAbundanceLightIsotope(elementLabelled, lightIsotope, percentHeavyLabel);
                double abundanceHeavyIsotopeLabeled1 = CalculateAbundanceHeavyIsotope(elementLabelled, heavyIsotope, percentHeavyLabel);

                IsotopicDistributionCalculator.Instance.SetLabeling(elementLabelled, lightIsotope, abundanceLightIsotopeLabeled1, heavyIsotope, abundanceHeavyIsotopeLabeled1);
                iso = IsotopicDistributionCalculator.Instance.GetIsotopePattern(baseEmpiricalFormula);

                IsotopicDistributionCalculator.Instance.ResetToUnlabeled();

            }

            iso.MonoIsotopicMass = monoisotopicMass;
            CalculateMZValuesForLabeledProfile(iso, baseEmpiricalFormula, elementLabelled, chargeState,
                                               lightIsotope, heavyIsotope);

            iso.ChargeState = chargeState;
            

            return iso;
        }


        public bool ValidateElement(string elementSymbol)
        {
            try
            {
                var element = Constants.Elements[elementSymbol];
            }
            catch (KeyNotFoundException ex)
            {
                return false;

            }

            return true;

        }

        #endregion


        #region Private methods
        private void CalculateMZValuesForLabeledProfile(IsotopicProfile iso, string empiricalFormula, string elementLabelled, int chargeState, int lightIsotope, int heavyIsotope)
        {

            var elementTable = EmpiricalFormulaUtilities.ParseEmpiricalFormulaString(empiricalFormula);

            var numLabelledAtoms = elementTable[elementLabelled];


            for (int i = 0; i < iso.Peaklist.Count; i++)
            {
                string keyLightIsotope = elementLabelled + lightIsotope;
                string keyHeavyIsotope = elementLabelled + heavyIsotope;

                double lightIsotopeMass = Constants.Elements[elementLabelled].IsotopeDictionary[keyLightIsotope].Mass;
                double heavyIsotopeMass = Constants.Elements[elementLabelled].IsotopeDictionary[keyHeavyIsotope].Mass;

                double massDiff = heavyIsotopeMass - lightIsotopeMass;


                double monoMZ = iso.MonoIsotopicMass / chargeState + Globals.PROTON_MASS;

                double peakMZIfUnlabelled = monoMZ + (i * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS) / chargeState;

                double monoPeakFullyLabelled = monoMZ + massDiff * numLabelledAtoms / chargeState;

                double peakMZBasedOnLabeled = monoPeakFullyLabelled -
                                              (numLabelledAtoms - i) * Globals.MASS_DIFF_BETWEEN_ISOTOPICPEAKS /
                                              chargeState;


                int peaksToUse;
                if (i > numLabelledAtoms)
                {
                    peaksToUse = numLabelledAtoms;
                }
                else
                {
                    peaksToUse = i;
                }


                iso.Peaklist[i].XValue = peakMZBasedOnLabeled * peaksToUse / numLabelledAtoms +
                                                                        peakMZIfUnlabelled * (numLabelledAtoms - peaksToUse) / numLabelledAtoms;


            }






        }

        private double CalculateAbundanceLightIsotope(string elementSymbol, int isotopeNum, double percentAddedLabeling)
        {
            bool elementIsValid = ValidateElement(elementSymbol);
            if (!elementIsValid) return 0;

            string key = elementSymbol + isotopeNum;
            var isotope = Constants.Elements[elementSymbol].IsotopeDictionary[key];

            double labelChange = percentAddedLabeling;

            return isotope.NaturalAbundance - labelChange / 100;


        }

        private double CalculateAbundanceHeavyIsotope(string elementSymbol, int isotopeNum, double percentAddedLabeling)
        {
            bool elementIsValid = ValidateElement(elementSymbol);
            if (!elementIsValid) return 0;

            string key = elementSymbol + isotopeNum;
            var isotope = Constants.Elements[elementSymbol].IsotopeDictionary[key];

            double labelChange = percentAddedLabeling;

            return isotope.NaturalAbundance + labelChange / 100;


        }


        private int GetIndexOfTargetPeak(IsotopicProfile mixedIso, double xValue, out bool peakWasFound, double toleranceInPPM = 1)
        {
            double toleranceInMZ = toleranceInPPM * xValue / 1e6;

            int indexOfPeak = 0;

            peakWasFound = false;

            for (int i = 0; i < mixedIso.Peaklist.Count; i++)
            {
                double currentPeakMZ = mixedIso.Peaklist[i].XValue;

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
