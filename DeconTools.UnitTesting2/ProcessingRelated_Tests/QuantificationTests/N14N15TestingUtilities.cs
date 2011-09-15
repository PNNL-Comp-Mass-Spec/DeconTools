using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;

namespace DeconTools.UnitTesting2.QuantificationTests
{
    public class N14N15TestingUtilities
    {

        public static string MS_AMTTag23140708_z2_sum1 = "..\\..\\..\\TestFiles\\Quantification\\N14N15_AMTTag23140708_mz1034_z2_sum1Scan.txt";    // scan 1428 from RSPH_AOnly_run3_16Dec07_raptor_07-11-11
        public static string MS_AMTTag23140708_z2_sum3 = "..\\..\\..\\TestFiles\\Quantification\\N14N15_AMTTag23140708_mz1034_z2_sum3Scans.txt";

        public static string MS_AMTTag23140708_z3_sum1 = "..\\..\\..\\TestFiles\\Quantification\\N14N15_AMTTag23140708_mz0689_z3_sum1Scan.txt";    // scan 1428 from RSPH_AOnly_run3_16Dec07_raptor_07-11-11
        public static string MS_AMTTag23140708_z3_sum3 = "..\\..\\..\\TestFiles\\Quantification\\N14N15_AMTTag23140708_mz0689_z3_sum3Scans.txt";

        public static string MS_AMTTag23085904_z2_sum1_lowN15 = "..\\..\\..\\TestFiles\\Quantification\\N14N15_AMTTag23085904_mz0878_z2_sum1Scan.txt";  //scan 1659 from :  RSPH_POnly_24_run1_30Jan08_Raptor_07-11-11   (15 min growth)



        #region Constructors
        #endregion

        #region Properties
        #endregion

        #region Public Methods

        public XYData GetSpectrumAMTTag23140708_Z2_Sum1()
        {
            Run run = new MSScanFromTextFileRun(N14N15TestingUtilities.MS_AMTTag23140708_z2_sum1);
            run.GetMassSpectrum(new ScanSet(1), 0, 50000);
            return run.XYData;
        }


        public XYData GetSpectrumAMTTag23140708_Z2_Sum3()
        {
            Run run = new MSScanFromTextFileRun(N14N15TestingUtilities.MS_AMTTag23140708_z2_sum3);
            run.GetMassSpectrum(new ScanSet(1), 0, 50000);
            return run.XYData;

        }

        public XYData GetSpectrumAMTTag23140708_Z3_Sum1()
        {
            Run run = new MSScanFromTextFileRun(N14N15TestingUtilities.MS_AMTTag23140708_z3_sum1);
            run.GetMassSpectrum(new ScanSet(1), 0, 50000);
            return run.XYData;
        }


        public XYData GetSpectrumAMTTag23140708_Z3_Sum3()
        {
            Run run = new MSScanFromTextFileRun(N14N15TestingUtilities.MS_AMTTag23140708_z3_sum3);
            run.GetMassSpectrum(new ScanSet(1), 0, 50000);
            return run.XYData;

        }


        public static XYData GetTestSpectrum(string xydataFileName)
        {
            Run run = new MSScanFromTextFileRun(xydataFileName);
            run.GetMassSpectrum(new ScanSet(1), 0, 50000);
            return run.XYData;
        }



        #endregion

        #region Private Methods
        #endregion

        public MassTag CreateMT23085904_Z2()
        {
            MassTag mt = new MassTag();
            mt.ChargeState = 2;
            mt.Code = "AMPIDLSNLALLDANGK";
            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
            mt.MonoIsotopicMass = 1754.923582420;
            mt.MZ = 878.4690677;
            mt.NormalizedElutionTime = 0.4509717f;
        
            return mt;
        }
        
        
        public MassTag CreateMT23140708_Z2()
        {
            MassTag mt = new MassTag();
            mt.ChargeState = 2;
            mt.Code = "IVKVNVDENPESPAMLGVR";
            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
            mt.MonoIsotopicMass = 2066.08292;
            mt.MZ = 1034.048737;
            mt.NormalizedElutionTime = 0.329644f;
     
            return mt;
        }

        public MassTag CreateMT23140708_Z3()
        {
            MassTag mt = new MassTag();
            mt.ChargeState = 3;
            mt.Code = "IVKVNVDENPESPAMLGVR";
            mt.EmpiricalFormula = mt.GetEmpiricalFormulaFromTargetCode();
            mt.MonoIsotopicMass = 2066.08292;
            mt.MZ = 689.7015832;
            mt.NormalizedElutionTime = 0.329644f;
           
            return mt;
        }

    }
}
