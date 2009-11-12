using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.TomIsotopicDistribution;

namespace DeconTools.Backend.ProcessingTasks.N14N15Analyzers.TomN14N15Analyzer
{
    public class TomN14N15Analyzer : I_N14N15Analyzer
    {

        private readonly bool b_enforce_N14_peak_has_no_iso_preceding = true;
        private readonly bool b_enforce_no_diff_charge_iso_peaks = true;
        // check if peaks have contradictory charge states

        //private readonly bool b_all_templates_must_have_nearby_iso = false;
        private readonly bool b_N14_template_must_have_nearby_iso = false;
        private readonly bool b_N15_template_must_have_nearby_iso = false;
        private readonly bool b_N14orN15_template_must_have_nearby_iso = true;
        private readonly bool b_prevent_overlapping_templates = true;     //Gord changed this to false!
        private readonly double dInstrumentTolerancePPM = 10.0; // in PPM
        private double dtplthashstartmass = 400.0;
        private double dtplthashendmass = 6000.0;
        private double dtplthashbucket = 10.0; // per m/z unit we bin
        private int itplthashbuckets;


        private List<IsotopicProfile> atplthashList = new List<IsotopicProfile>();
        static List<IsotopicProfile> atplthashListN15 = new List<IsotopicProfile>();

        public TomN14N15Analyzer()
        {

            itplthashbuckets = (int)Math.Ceiling((dtplthashendmass - dtplthashstartmass) / dtplthashbucket);

        }

        public override void ExtractN14N15Values(DeconTools.Backend.Core.ResultCollection resultList)
        {
            bool bLabel = true; // for N15
            double matchTol = 0.03f; // match tolerance
            double fCutOff = 0.01f; // template minimum peak intensity

            List<MSPeak> peakList = resultList.Run.MSPeakList;
            InitTemplateHash();

            RemovePeaksWithinTol(peakList, matchTol);

            List<TemplateMatch> templateList = Make14N15NTemplateList(peakList, fCutOff);
            List<TemplateMatch> scoredTemplates = GetScoreList(peakList, templateList, bLabel);

            TemplateMatch tmBestPeak = (TemplateMatch)scoredTemplates[0];
            //atmProcessAssignedTemplates.Add(tmBestPeak);

            List<TemplateMatch> atmProcessAssignedTemplates = new List<TemplateMatch>();

            //for (int ii = 0; templateList.Count > 0 && tmBestPeak.fScore > 0; ii++)
            //{
            //    // now you've got the top score, 
            //    // set all the peaks it accounts for to 0 intensity and re-score...
            //    SetTemplateMatchesToZero(tmBestPeak, peakList);
            //    atmProcessAssignedTemplates.Add(tmBestPeak);
            //    templateList =
            //        Make14N15NTemplateList(peakList, fCutOff);
                
            //    scoredTemplates = GetScoreList(peakList, templateList, bLabel);
            //    if (scoredTemplates.Count > 0)
            //        tmBestPeak = (TemplateMatch)scoredTemplates[0];
            //}

            TemplateMatch.SetSortBy(TemplateMatch.SortBy.MZ);
            atmProcessAssignedTemplates.Sort();
            TemplateMatch.SetSortBy(TemplateMatch.SortBy.SCORE);
            // 
            //return atmProcessAssignedTemplates;*/

            DisplayTemplates(scoredTemplates);
        }

        private void SetTemplateMatchesToZero(TemplateMatch tmBestPeak, List<MSPeak> peakList)
        {
            return;
        }

        private void DisplayTemplates(List<TemplateMatch> scoredTemplates)
        {
            StringBuilder sb = new StringBuilder();
            char delimiter = '\t';

            string header = "Z\tLightMZ\tLightIntens\tHeavyMZ\tHeavyIntens\tTotalPeakRatio\tFit\tScore\n";
            sb.Append(header);

            foreach (TemplateMatch tm in scoredTemplates)
            {
                sb.Append(tm.iCharge);
                sb.Append(delimiter);
                sb.Append(tm.fMZ);
                sb.Append(delimiter);
                sb.Append(tm.fLightIntensity);
                sb.Append(delimiter);
                sb.Append(tm.fHeavyMZ);
                sb.Append(delimiter);
                sb.Append(tm.fHeavyIntensity);
                sb.Append(delimiter);
                sb.Append(tm.fTotalPeakRatio);
                sb.Append(delimiter);
                sb.Append(tm.fFit);
                sb.Append(delimiter);
                sb.Append(tm.fScore);
                sb.Append(Environment.NewLine);                
                
            }

            Console.Write(sb.ToString());
        }

        private List<TemplateMatch> GetScoreList(List<MSPeak> peakList, List<TemplateMatch> templateList, bool bLabel)
        {
            List<TemplateMatch> scoreList = new List<TemplateMatch>();
            for (int ii = 0; ii < templateList.Count; ii++)
            {
                TemplateMatch tm = (TemplateMatch)templateList[ii];
                ScoreAndMatchTemplate(tm, peakList); // changes TM

                scoreList.Add(tm);
            }
            scoreList.Sort();
            scoreList.Reverse();
            return scoreList;
        }

        private void ScoreAndMatchTemplate(TemplateMatch TM, List<MSPeak> peakList)
        {
            // iOffset is where to start in afPeaklist, corresponding to template start m/z
            // (we'll have to shift the template up and down as well!)
            {

                int iOffset = TM.iPos;
                IsotopicProfile afTemplate = TM.isoPattern;
                double fScore = 0.0f, fScoreIncr;
                MSPeak afCurrPeak, myTemplatePeak, myMZPeak;
                // we set the maximum peak of the template to this intensity

                myMZPeak = peakList[iOffset];
                myTemplatePeak = afTemplate.Peaklist[0];

                double fBasePeakInten = myMZPeak.Intensity; // all peaks are relative to this

                // go through both lists side by side; return SD for each peak

                double fRxSquared = 0.0f, fTxSquared = 0.0f; // dot product normalization
                fRxSquared = 0.0f; fTxSquared = 0.0f; // dot product normalization

                int iLastPeakMatch = 0;
                int[] aiCorrespondingPeaks = new int[afTemplate.Peaklist.Count];
                for (int ii = 0; ii < aiCorrespondingPeaks.Length; ii++)
                    aiCorrespondingPeaks[ii] = -1; // not there

                bool b_forced_zero_score = false;
                for (int jj = 0; jj < afTemplate.Peaklist.Count; jj++)
                {
                    myTemplatePeak = afTemplate.Peaklist[jj];
                    bool bPeakFound = false;

                    afCurrPeak = peakList[iLastPeakMatch];
                    //afCurrPeak = (MZPeak)peakList[0];

                    for (int kk = iLastPeakMatch; kk < peakList.Count; kk++)
                    {
                        if (!IsWithinTolPPM(afCurrPeak.MZ, myTemplatePeak.MZ)
                            && myTemplatePeak.MZ < afCurrPeak.MZ) break;
                        afCurrPeak = peakList[kk];
                        // ; kk++)

                        // Now this is a subtle bug: what happens if more than 1 peak matches within tolerance?
                        // We want to pick the biggest one only of course...
                        if (IsWithinTolPPM(myTemplatePeak.MZ, afCurrPeak.MZ))
                        {
                            bPeakFound = true; // a hit

                            //if (myTemplatePeak.Intensity*fBasePeakInten 
                            //	> afCurrPeak.Intensity*afCurrPeak.fInUse)
                            //	fScoreIncr = afCurrPeak.Intensity*afCurrPeak.fInUse;
                            //else
                            //	fScoreIncr = myTemplatePeak.Intensity*fBasePeakInten;
                            fScoreIncr = myTemplatePeak.Intensity * afCurrPeak.Intensity; //* afCurrPeak.fInUse; // like xcorr
                            if (b_prevent_overlapping_templates)// && afCurrPeak.fInUse < 0.5)
                                b_forced_zero_score = true;
                            //if (myTemplatePeak.aiMatchesPeaks.Contains(kk) == false)
                            //	myTemplatePeak.aiMatchesPeaks.Add(kk); // add matching peak
                            if (aiCorrespondingPeaks[jj] != kk && aiCorrespondingPeaks[jj] != -1)
                            {
                                System.Console.WriteLine("Warning! More than one peak matching template.");
                                TM.Print();
                            }
                            aiCorrespondingPeaks[jj] = kk; // not sure if this'll work, 5/9/08
                            fScore += fScoreIncr;
                            fTxSquared += myTemplatePeak.Intensity * myTemplatePeak.Intensity;
                            fRxSquared += afCurrPeak.Intensity * afCurrPeak.Intensity;

                        }
                        iLastPeakMatch++;
                    }
                    if (iLastPeakMatch > 1) iLastPeakMatch--;
                    if (bPeakFound == false)
                    {
                        //fScoreIncr = myTemplatePeak.Intensity*fBasePeakInten;
                        //fScore -= fScoreIncr;
                    }
                    if (iLastPeakMatch == peakList.Count) break;
                }

                // dot product normalization


                double fNormalize = (double)Math.Sqrt((double)fTxSquared * fRxSquared);
                //fScore /= fNormalize;

                TM.SetCorrespondingPeaks(aiCorrespondingPeaks);
                TM.fBasePeakInten = fBasePeakInten;
                TM.fFit = fScore / fNormalize;

                TM.fScore = fScore;
                if (b_forced_zero_score) TM.fScore = 0.0;
            }
        }

        private void RemovePeaksWithinTol(List<MSPeak> peakList, double tolerance)
        {
            if (peakList == null || peakList.Count < 2) return;

            for (int i = peakList.Count - 2; i >= 0; i--)     //counting backwards avoids indexing exceptions when peaks are removed
            {
                if (peakList[i + 1].MZ - peakList[i].MZ < tolerance)
                {

                    if (peakList[i].Intensity < peakList[i + 1].Intensity)
                    {
                        peakList.RemoveAt(i);
                    }
                    else
                    {
                        peakList.RemoveAt(i + 1);
                    }
                }
            }

        }


        private void InitTemplateHash()
        {
            if (atplthashList.Count > 0) return;
            atplthashListN15 = new List<IsotopicProfile>();
            atplthashList = new List<IsotopicProfile>();
            for (int ii = 0; ii < itplthashbuckets; ii++)
            {
                double dmonomass = (double)ii * dtplthashbucket + dtplthashstartmass;
                int[] afAvnBasedFormula = TomIsotopicPattern.GetClosestAvnFormula(dmonomass, false);
                IsotopicProfile mzIsoPattern = new IsotopicProfile();
                mzIsoPattern = TomIsotopicPattern.GetIsotopePattern(afAvnBasedFormula, TomIsotopicPattern.aafIsos);
                atplthashList.Add(mzIsoPattern);
                afAvnBasedFormula = TomIsotopicPattern.GetClosestAvnFormula(dmonomass * 0.987957526f, true);
                atplthashListN15.Add(mzIsoPattern);

            }
        }


        private List<TemplateMatch> Make14N15NTemplateList(List<MSPeak> afPeakList, double fCutOff)
        {
            // this function will take in a top-scoring template then try to match a peak
            // in mass away from it with a 15N peak
            // loop over the template list
            // for each template, get the mass
            // loop over the statistically expected number of nitrogens per mass (1 to 1.5 per 100Da)
            // see if you see a peak in the spectrum the expected distance away
            // if you do, then generate a 15N template with the maximum mass at this point
            //		and with the intensity set to the peak intensity

            //int iCharge = 1, iMaxCharge = 3;//

            int iStartCharge = 1, iMaxCharge = 3;//
            List<TemplateMatch> atmTemplateList = new List<TemplateMatch>();
            double dMassNitrogen = (TomIsotopicPattern.fN15m - TomIsotopicPattern.fN14m);
            // get scorelist
            for (int ii = 0; ii < afPeakList.Count; ii++)// ii < 2; ii++)//
            {
                MSPeak myLightMZPeak = afPeakList[ii];
                double iMinMZDiff = myLightMZPeak.MZ * TomIsotopicPattern.fNPerThLow;
                double iMaxMZDiff = myLightMZPeak.MZ * TomIsotopicPattern.fNPerThHigh;
                for (int chargeIterator = iStartCharge; chargeIterator <= iMaxCharge; chargeIterator++)
                // loop over peaks and charge states...
                {
                    // Check that there aren't any previous N14 isotope peaks if this flag is set
                    if (b_enforce_N14_peak_has_no_iso_preceding && peakListContainsNearbyIsotopePeak(
                        myLightMZPeak.MZ, chargeIterator, ii, afPeakList, true, false))
                        continue; // go to next peak, this one's bad

                    if (b_enforce_no_diff_charge_iso_peaks
                        && PeakListContainsNearbyDiffChargeIsotopePeak(
                        myLightMZPeak.MZ, chargeIterator, iMaxCharge, ii, afPeakList))
                    {
                        //Console.WriteLine("b_enforce_no_diff_charge_iso_peaks triggered 1");
                        //myLightTomMZPeak.Print();
                        continue; // go to next peak, this one's bad
                    }

                    for (int jj = ii + 1; jj < afPeakList.Count; jj++)
                    // this is the delta-N loop
                    {
                        MSPeak myHeavyMZPeak = afPeakList[jj];
                        double fDiffHere = myHeavyMZPeak.MZ - myLightMZPeak.MZ;

                        if (iMaxMZDiff < fDiffHere) break;
                        if (fDiffHere < iMinMZDiff) continue;

                        if (b_enforce_no_diff_charge_iso_peaks) // check this flag
                        {
                            if (PeakListContainsNearbyDiffChargeIsotopePeak(
                                myHeavyMZPeak.MZ, chargeIterator, iMaxCharge, jj, afPeakList))
                            {
                                //Console.WriteLine("b_enforce_no_diff_charge_iso_peaks triggered 2");
                                //myHeavyTomMZPeak.Print();
                                continue; // go to next peak, this one's bad
                            }
                        }

                        if (b_N14_template_must_have_nearby_iso)
                        {
                            if (!peakListContainsNearbyIsotopePeak
                                (myLightMZPeak.MZ, chargeIterator, ii, afPeakList, true, true))
                                continue;
                        }
                        if (b_N15_template_must_have_nearby_iso)
                        {
                            if (!peakListContainsNearbyIsotopePeak
                                (myHeavyMZPeak.MZ, chargeIterator, jj, afPeakList, true, true))
                                continue;
                        }
                        if (b_N14orN15_template_must_have_nearby_iso)
                        {
                            if (!peakListContainsNearbyIsotopePeak(
                                myLightMZPeak.MZ, chargeIterator, ii, afPeakList, true, true)
                                && !peakListContainsNearbyIsotopePeak(
                                myHeavyMZPeak.MZ, chargeIterator, jj, afPeakList, true, true))
                                continue;
                        }

                        int deltaN = (int)Math.Round(fDiffHere * chargeIterator / dMassNitrogen);
                        if (deltaN < 1) Console.WriteLine("15N prediction catastrophe");

                        double predictedN15MZ = deltaN * dMassNitrogen / chargeIterator + myLightMZPeak.MZ;
                        if (IsWithinTolPPM(predictedN15MZ, myHeavyMZPeak.MZ))
                        // we found a next peak
                        {


                            double fIntensityRatio = myHeavyMZPeak.Intensity / myLightMZPeak.Intensity;

                            TemplateMatch TM = Make14N15NTemplate(
                                myLightMZPeak.MZ, myHeavyMZPeak.MZ, chargeIterator, ii, jj,
                                deltaN, fIntensityRatio, fCutOff);
                            atmTemplateList.Add(TM);
                        }
                    }

                }
                // Now let's see if there's a peak over any of this range...
                // Assumption, possibly bad: we will see the monoisotopic peak. 
                // Maybe we won't. That would be bad.
                // 

                //if (myLightTomMZPeak.fInUse < 0.5f) continue; // peak set to 0 (seen)

            }
            return atmTemplateList;




        }

        private bool IsWithinTolPPM(double m1, double m2)
        {

            double tol = dInstrumentTolerancePPM / 1000000.0;
            if (m1 == m2) return true;
            if ((m1 < m2) && ((m2 - m1) < tol * m2)) return true;
            if ((m2 < m1) && ((m1 - m2) < tol * m1)) return true;
            return false;

        }


        private bool PeakListContainsNearbyDiffChargeIsotopePeak(double fMonoMZ,
            int iClaimedCharge, int iMaxCharge, int iPos, List<MSPeak> peaklist)
        //returns true if the peaklist contains the C13 peak 
        // at any different charge states, so you don't pick up a 3+ as a 1+ or a 2+ as a 1+
        // tricky thing: if it's a 2+ ion, you'd pick up a "1+" ion as well
        {
            bool bClaimedChargeStateDetected = false; // set this to true if you actually see the charge
            bool bRet = false;

            bClaimedChargeStateDetected
                = peakListContainsNearbyIsotopePeak(fMonoMZ, iClaimedCharge, iPos, peaklist, true, true);

            for (int iC = 1; iC <= iMaxCharge; iC++)
            {
                if (iC == iClaimedCharge) continue;
                bool bSeenDiffChargePeak
                    = peakListContainsNearbyIsotopePeak(fMonoMZ, iC, iPos, peaklist, true, true);
                // it's okay if you detect a 1+ charge state for a 2+ or 3+ or 4+
                // and it's okay to see a 2+ charge state for a 4+
                // only if you've seen the 2+ or 3+ or 4+ already
                // it's never okay to see a 2+ state when you claim it's a 3+, or a 3+ for a 4+
                // it's never okay to see a next 2+ ion when you claim it's a 1+
                // case 1: iClaimedCharge = 2, iC = 1 so iClaimedCharge%iC == 0
                // case 1a: bClaimedChargeStateDetected is true, so bRet is false
                // case 1b: bClaimedChargeStateDetected is false, so bRet is true
                // case 2: iClaimedCharge = 3, iC = 2 so iClaimedCharge%iC != 0
                // bRet is true always
                if (bSeenDiffChargePeak)
                {
                    if (iClaimedCharge % iC == 0) // claim it's 2+, see a 1+
                    {
                        if (bClaimedChargeStateDetected) // seen a 2+ already
                        {
                            if (bRet == false) bRet = false;
                        }
                        else
                        {
                            bRet = true;
                        }
                    }
                    else
                    {
                        bRet = true;
                        break;
                    }

                }
            }
            return bRet;
        }



        private bool peakListContainsNearbyIsotopePeak(double fMonoMZ,
    int iCharge, int iPos, List<MSPeak> peakList, bool bLookDown, bool bLookUp)
        // this returns true if the peaklist contains the peak you'd expect at the charge state as C13
        // bBothDirections set to true means you look up and down the mz, false means up only
        {
            bool bRet = false;
            double fNextHighestMZ = fMonoMZ + TomIsotopicPattern.fAveMassDefect / iCharge;
            double fNextLowestMZ = fMonoMZ - TomIsotopicPattern.fAveMassDefect / iCharge;
            for (int jj = iPos + 1; bLookUp && jj < peakList.Count; jj++)
            {
                MSPeak myNextMZ = peakList[jj];
                //if (myNextMZ.fInUse < 0.5f) continue; // peak shouldn't count! // yes it should :P
                if (IsWithinTolPPM(fNextHighestMZ, myNextMZ.MZ))
                {
                    bRet = true;
                    break;
                }
                else if (fNextHighestMZ < myNextMZ.MZ) break;
            }
            for (int jj = iPos - 1; bLookDown && jj >= 0; jj--)
            {
                MSPeak myNextMZ = peakList[jj];
                //if (myNextMZ.fInUse < 0.5f) continue; // peak shouldn't count!
                if (IsWithinTolPPM(fNextLowestMZ, myNextMZ.MZ))
                {
                    bRet = true;
                    break;
                }
                else if (myNextMZ.MZ < fNextLowestMZ) break;
            }
            return bRet;
        }


        private TemplateMatch Make14N15NTemplate(double fMonoMZLight, double fMonoMZHeavy, int iCharge,
        int iPosLight, int iPosHeavy, int DeltaN, double fIntRatio, double fCutoff)
        {
            // ensure that maximum peak height is 1.0
            double fInvIntRatio = 1.0f;
            if (fIntRatio > 1.0f)
            {
                fInvIntRatio = 1.0f / fIntRatio;
                fIntRatio = 1.0f;

            }
            TemplateMatch tmLight
                = MakeTemplate(fMonoMZLight, iCharge, iPosLight, fInvIntRatio, fCutoff, false);
            TemplateMatch tmHeavy
                = MakeTemplate(fMonoMZHeavy, iCharge, iPosHeavy, fIntRatio, fCutoff, true);
            tmLight.fLightIntensity = TotalPeakIntensity(tmLight.isoPattern.Peaklist); // Really?
            tmLight.fHeavyIntensity = TotalPeakIntensity(tmHeavy.isoPattern.Peaklist);
            tmLight.fTotalPeakRatio = TotalPeakIntensity(tmHeavy.isoPattern.Peaklist)
                / TotalPeakIntensity(tmLight.isoPattern.Peaklist);

            tmLight.isoPattern.Peaklist.AddRange(tmHeavy.isoPattern.Peaklist);

            // Need to normalize to 1...
            //double fTotalIntensity = TotalPeakIntensity(tmLight.amzIsoPattern);
            //foreach (MZPeak mzPeak in tmLight.amzIsoPattern)
            //{
            //	mzPeak.fInt /= fTotalIntensity;
            //}
            //
            //tmLight.amzIsoPattern.Sort();
            tmLight.fScore += tmHeavy.fScore;
            tmLight.iDeltaN = DeltaN;
            tmLight.bIs14N15N = true;
            tmLight.fHeavyMonoMass = tmHeavy.fMonoMass;
            tmLight.fHeavyMZ = tmHeavy.fMZ;

            return tmLight;

        }

        private TemplateMatch MakeTemplate(double fStartMZ, int iCharge, int iPos,
        double fIntMax, double fCutoff, bool bLabel)
        // given an m/z and charge, make an averagine template as {mz, rel. intensity)
        // fIntMax increases max peak to this value; assume it's already normalized
        // the template has the maximum peak set to fMass
        // generates a template with fStartMZ equal to the monoisotopic mass of the template
        // Otherwise it's just too hard to understand! :)
        {
            int[] afAvnBasedFormula;
            double fMonoMass = (fStartMZ - TomIsotopicPattern.fH1m) * iCharge; // m/z 1000@2+ is neutral mass 1998

            IsotopicProfile mzIsoPattern = new IsotopicProfile();
            int iMonoPosition = 0;
            /*
            if (bLabel == false)
            {
                afAvnBasedFormula = TomIsotopicPattern.GetClosestAvnFormula(fMonoMass, bLabel);

                mzIsoPattern = TomIsotopicPattern.GetTomIsotopicPattern(afAvnBasedFormula, TomIsotopicPattern.aafIsos);
            }
            else  // this is very specific for 15N labeling
            {
                // we need to pre-correct the mass to shoot for to get a N15 labeled mass 
                // around the right mass... 15N-Avn is 1.012189262 times heavier than 14N
                afAvnBasedFormula = TomIsotopicPattern.GetClosestAvnFormula(fMonoMass*0.987957526f, bLabel);
                iMonoPosition += afAvnBasedFormula[2]; // this is where the monomass comes from, add nitrogens
                mzIsoPattern = TomIsotopicPattern.GetTomIsotopicPattern(afAvnBasedFormula, TomIsotopicPattern.aafN15Isos);
            }*/

            afAvnBasedFormula = TomIsotopicPattern.GetClosestAvnFormula(fMonoMass * 0.987957526f, bLabel);
//            iMonoPosition += afAvnBasedFormula[2]; // this is where the monomass comes from, add nitrogens     //[gord changed this!!]

            mzIsoPattern = GetTemplate(fMonoMass, bLabel);

            double[] afTmp = new double[2];
            double fTmpMZ;

            // in this bloc we fill in the m/z of the isotope template pattern
            for (int ii = 0; ii < mzIsoPattern.Peaklist.Count; ii++)
            {
                MSPeak myTemplatePeak = mzIsoPattern.Peaklist[ii];
                int iTemplatePos = ii - iMonoPosition; // here's our offset
                fTmpMZ = fStartMZ + (TomIsotopicPattern.fAveMassDefect * iTemplatePos) / iCharge;
                myTemplatePeak.MZ = fTmpMZ;
            }

            //[gord] I think I might have messed this up...  
            //NOTE:  [gord changed this]   I simplified the following code.  See 
            
            //// in this bloc we get the positions of the minimum and maximum cutoffs of the isotope pattern
            //// making sure we include the monoisotopic peak as well - this may induce an artificial
            //// low cutoff so be careful!
            //int iStartTemplateCutoff = 0;
            //for (int ii = 0; ii < iMonoPosition; ii++)
            //{
            //    MSPeak myTemplatePeak;

            //    if (mzIsoPattern.Peaklist.Count == ii)
            //    {
            //        break;   //[gord added this] if we reach the end of the peakList, break out of loop and move on
            //    }
            //    myTemplatePeak = mzIsoPattern.Peaklist[ii];    // TODO: crashes here if the peakList.Count < iMonoPosition



            //    double fPeakInt = myTemplatePeak.Intensity;
            //    // the template is normalized to 1, so 0.9 had better be above the cutoff
            //    if (fPeakInt < fCutoff && ii < iMonoPosition) iStartTemplateCutoff = ii;
            //}
            //int iEndTemplateCutoff = mzIsoPattern.Peaklist.Count - 1;
            //for (int ii = mzIsoPattern.Peaklist.Count - 1; ii > iMonoPosition; ii--) // and counting down...
            //{
            //    MSPeak myTemplatePeak = mzIsoPattern.Peaklist[ii];
            //    double fPeakInt = myTemplatePeak.Intensity;
            //    if (fPeakInt < fCutoff && iMonoPosition < ii) iEndTemplateCutoff = ii;
            //}
            ///////////////////////////////////////////////////////////////////////////
            //iMonoPosition = iMonoPosition - iStartTemplateCutoff;

            //for (int ii = iStartTemplateCutoff; ii < iEndTemplateCutoff && ii < mzIsoPattern.Peaklist.Count; ii++)
            //{
            //    MSPeak mzCurrPeak = mzIsoPattern.Peaklist[ii];
            //    //MSPeak mzNewPeak = new MZPeak(mzCurrPeak.MZ, mzCurrPeak.fInt * fIntMax, mzCurrPeak.fInUse);
            //    MSPeak mzNewPeak = new MSPeak(mzCurrPeak.MZ, (float)(mzCurrPeak.Intensity * fIntMax), 0.0f, 0.0f);

            //    amzNewTemplate.Peaklist.Add(mzNewPeak);
            //}


            
            IsotopicProfile amzNewTemplate = new IsotopicProfile();
            MSPeak mostIntensePeak = mzIsoPattern.getMostIntensePeak();
            bool foundMostIntensePeak = false;

            for (int i = 0; i < mzIsoPattern.Peaklist.Count; i++)          //this replaces the above commented-out code
            {
                if (foundMostIntensePeak)
                {
                    if (mzIsoPattern.Peaklist[i].Intensity > fCutoff)
                    {
                        amzNewTemplate.Peaklist.Add(mzIsoPattern.Peaklist[i]);
                    }
                    else
                    {
                        break;
                    }

                }
                else
                {
                    amzNewTemplate.Peaklist.Add(mzIsoPattern.Peaklist[i]);
                }
                if (mzIsoPattern.Peaklist[i].Intensity == mostIntensePeak.Intensity) foundMostIntensePeak = true;
               
            }
            
           

            TemplateMatch TM = new TemplateMatch(0.0f, fStartMZ, iCharge, iPos); // change this!!
            //TM.iPeakPosition = iMaxPosition;
            TM.iMonoPosition = iMonoPosition;
            TM.fMonoMass = fMonoMass;
            TM.SetTemplateIsoPattern(amzNewTemplate);
            return TM;
        }

        private double TotalPeakIntensity(List<MSPeak> peakList)
        {
            double totalIntensity = 0.0f;
            foreach (MSPeak peak in peakList)
            {
                totalIntensity += peak.Intensity;
            }
            return totalIntensity;
        }


        private IsotopicProfile GetTemplate(double fMonoMass, bool bLabel)
        {
            int itpltPos = (int)Math.Ceiling((fMonoMass - dtplthashstartmass) / dtplthashbucket);
            IsotopicProfile iso;
            //if (bLabel) iso = atplthashListN15[itpltPos];    
            
            //int b;

            //iso = atplthashList[itpltPos];
            //if (iso.Peaklist.Count == 0)
            //    b = 2;
            //return iso;

            if (bLabel)
            {
                iso = atplthashListN15[itpltPos];
            }
            else
            {
                iso = atplthashList[itpltPos];
            }
            return iso;

        }



    }
}
