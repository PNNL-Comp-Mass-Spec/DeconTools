using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Utilities;
using NUnit.Framework;

namespace DeconTools.UnitTesting2.Algorithm_tests
{
    [TestFixture]
    public class LabelingDistributionCalculatorTests
    {

        [Test]
        public void CalculateLabelingDistributionTest1()
        {
            // LabelDistribution calculation is based on John Chik's work
            // He provided a standard set of data here:  http://people.ucalgary.ca/~dschriem/CalcDeut/CalcDeut/
            // We will use his test values and pass it through our implementation
            /*
             *#<<<Isotope Calculation Result >>>
                #Formula: ISO:|VDWCPTGF| H2O + (H)1
                #Resolution: 10000 resolving power
                #Threshold (%): 1.0000
                #Isotope List - 5 Mass(es)
                # 
                #Isotope Mass   Relative Intensity
                   924.39202        100.0000
                   925.39495         53.0190
                   926.39553         20.6161
                   927.39655          5.9286
                   928.39781          1.3694
                             */
            //observed: (from J. Chik's data)          118.8932 73.8074 34.413 14.4563 6.3169

            /*
            *  Output Data from 'CalcDeut' testing from Dave Schriemer's data (see link above)
            *  File: MS924.3_Z1_10.0-10.2_A344-351_dist.xml
            * 
            *    <DeutDist startdeut='-0.0' chisq='5.80293903626e-05' content_type='numlist' distsum='1.00331024913' entries='5'>
                     0.832945525577
                     0.0747772118716
                     0.0305125766799
                     0.0704222225599
                     -0.00534728755986
                 </DeutDist>
            * 
            * 
            */
            var theorIntensities = new List<double>(new[] {1, 0.530190, 0.206161, 0.059286, 0.013694});
            var obsIntensities = new List<double>(new[] { 118.8932, 73.8074, 34.413, 14.4563, 6.3169 });

            var labelDistCalc = new LabelingDistributionCalculator();

            var intensityThreshold = 0.01;
            labelDistCalc.CalculateLabelingDistribution(theorIntensities, obsIntensities, intensityThreshold, intensityThreshold, out var solvedXVals, out var solvedYvals, 
                                                        truncateTheorBasedOnRelIntensity: false, 
                                                        truncateObservedBasedOnRelIntensity: false, 
                                                        leftPadding: 0, 
                                                        rightPadding: 2, 
                                                        numPeaksForAbsoluteTheorList: 3, 
                                                        numPeaksForAbsoluteObsList: 5);

            Assert.AreEqual(0.8329455, Math.Round(solvedYvals[0]), 7);
            Assert.AreEqual(0.0747772, Math.Round(solvedYvals[1]), 7);
            Assert.AreEqual(0.0305126, Math.Round(solvedYvals[2]), 7);
            Assert.AreEqual(0.0704222, Math.Round(solvedYvals[3]), 7);
            Assert.AreEqual(-0.0053473, Math.Round(solvedYvals[4]), 7);
         
            var xydata = new XYData();
            xydata.Xvalues = solvedXVals;
            xydata.Yvalues = solvedYvals;

            Console.WriteLine();
            xydata.Display();
            labelDistCalc.OutputLabelingInfo(solvedYvals.ToList(), out var fractionUnlabelled, out var fractionLabelled,
                                             out var averageLabelsIncorporated);

            Console.WriteLine();
            Console.WriteLine("fractionUnlabelled= " + fractionUnlabelled);
            Console.WriteLine("fractionLabelled= " + fractionLabelled);
            Console.WriteLine("averageAmountLabelIncorp= " + averageLabelsIncorporated);


        }


        [Test]
        public void CalculateLabelingDistributionTest2()
        {
            //mass_tag_id	monoisotopic_mass	NET	obs	mod_count	mod_description	pmt	peptide	peptideex
            //355176429	1538.8166126	0.42422	1	0		2.00000	LFLASACLYGAALAGV	R.LFLASACLYGAALAGV.C


            //------------------------load theor data ------------------------------------

            var peptideUtils = new PeptideUtils();

            var peptide = new PeptideTarget();
            peptide.Code = "LFLASACLYGAALAGV";
            peptide.EmpiricalFormula = new PeptideUtils().GetEmpiricalFormulaForPeptideSequence(peptide.Code);

            var theorFeatureGenerator = new JoshTheorFeatureGenerator();
            theorFeatureGenerator.GenerateTheorFeature(peptide);
            var theorProfile = peptide.IsotopicProfile;
            var theorIntensities = theorProfile.Peaklist.Select(p => (double)p.Height).ToList();

            Console.WriteLine("Total carbons = " + peptideUtils.GetNumAtomsForElement("C", peptide.EmpiricalFormula));


            var obsIntensities = new List<double>(new[]
                                     {
                                         1, 0.8335001, 0.4029815, 0.1846439, 0.1116047, 0.09458135, 0.07157851, 0.04972008,
                                         0.03036686, 0.01545749, 0.008299164, 0.004003931, 0.001711554, 0.000766305,
                                         0.000484514
                     });


            var relexCorrectedIntensities = new List<double>(new[]
                                     {
                                         1, 0.84416, 0.4174013, 0.1843375, 0.1073706, 0.09557419, 0.07190877, 0.04823519,
                                         0.02991197, 0.01456759, 0.008299164, 0.004003931, 0.001711554, 0.000766305,
                                         0.000484514
                                     });


            /*
             * 
             * Data for observed profile:  
             * LCMSFeature 8517 from Yellow_C13_070_23Mar10_Griffin_10-01-28
             *              * 
             * peak	mz	relIntens	width	sn
                0	770.418569278349	1	0.008119391	285.256
                1	770.919748892834	0.8335001	0.008161238	884.9909
                2	771.421020812818	0.4029815	0.008248929	566.6187
                3	771.922575912402	0.1846439	0.008058954	703.6938
                4	772.424544684666	0.1116047	0.007997629	358.0133
                5	772.926348484216	0.09458135	0.008128829	677.3434
                6	773.428080290323	0.07157851	0.008151106	100
                7	773.929769365137	0.04972008	0.008104968	100
                8	774.431542463583	0.03036686	0.007982519	100
                9	774.933237566856	0.01545749	0.008024476	100
                10	775.434950373045	0.008299164	0.008166174	740.9502
                11	775.936532837365	0.004003931	0.007899459	100
                12	776.438361528879	0.001711554	0.00877683	44.79652
                13	776.940072709136	0.0007663054	0.007601836	17.12788
                14	777.441663634739	0.0004845143	0.007320933	100
             * 
             */


            var labelDistCalc = new LabelingDistributionCalculator();


            var d = 0.1;
            labelDistCalc.CalculateLabelingDistribution(theorIntensities, obsIntensities, d, d, out var solvedXVals, out var solvedYvals);


            var xydata = new XYData();
            xydata.Xvalues = solvedXVals;
            xydata.Yvalues = solvedYvals;

            Console.WriteLine();
            Console.WriteLine("threshold= " + d);
            xydata.Display();



            labelDistCalc.CalculateLabelingDistribution(theorIntensities, relexCorrectedIntensities, d, d, out solvedXVals, out solvedYvals);

            xydata.Xvalues = solvedXVals;
            xydata.Yvalues = solvedYvals;

            Console.WriteLine();
            Console.WriteLine("Relex-corrected isotopic profile= " + d);
            xydata.Display();
            labelDistCalc.OutputLabelingInfo(solvedYvals.ToList(), out var fractionUnlabelled, out var fractionLabelled,
                                             out var averageLabelsIncorporated);

            Console.WriteLine();
            Console.WriteLine("fractionUnlabelled= " + fractionUnlabelled);
            Console.WriteLine("fractionLabelled= " + fractionLabelled);
            Console.WriteLine("averageAmountLabelIncorp= " + averageLabelsIncorporated);



            //for (double d = 0; d < 2.1; d+=0.1)
            //{
            //    labelDistCalc.CalculateLabelingDistribution(theorIntensities, obsIntensities, d, d, out solvedXVals, out solvedYvals);


            //    XYData xydata = new XYData();
            //    xydata.Xvalues = solvedXVals;
            //    xydata.Yvalues = solvedYvals;

            //    Console.WriteLine();
            //    Console.WriteLine("threshold= " + d);
            //    xydata.Display();
            //}



        }


    }
}
