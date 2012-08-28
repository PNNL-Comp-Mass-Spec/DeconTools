package uk.ac.liv.mzidparsers;

import java.io.File;
import java.util.List;

import junit.framework.TestCase;
import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.AnalysisData;
import uk.ac.ebi.jmzidml.model.mzidml.DataCollection;
import uk.ac.ebi.jmzidml.model.mzidml.Peptide;
import uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidence;
import uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceRef;
import uk.ac.ebi.jmzidml.model.mzidml.SequenceCollection;
import uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItem;
import uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationList;
import uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationResult;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLUnmarshaller;



/**
 * In this class we use the jmzidentml parser (see http://code.google.com/p/jmzidentml/)
 * to read in the transformed mzIdentML file and automatically verify its output based
 * on a number of assertions. 
 * 
 * So: all automated regression tests can be placed here. 
 * 
 *      
 * @author plukasse
 *
 */
public class Tandem2mzidRegressionTests extends TestCase 
{
	
	/**
	 * Basic regression tests/checks 
	 *  
	 * @throws Exception 
	 */
	public void test_basic_55merge_tandem_file() throws Exception
	{
		 String xTandemFile = "test/data/55merge_tandem.xml";
		 String resultFile = "test/data/55merge_tandem.xml.mzid";
		 new Tandem2mzid(xTandemFile, resultFile, "MS:1001348","MS:1001062",false);

		 //===================================================================================
		 //========================= Checks /assertions section :=============================
		 //===================================================================================
         MzIdentMLUnmarshaller unmarshaller = new MzIdentMLUnmarshaller(new File(resultFile));

         DataCollection dc =  unmarshaller.unmarshal(DataCollection.class);
         AnalysisData ad = dc.getAnalysisData();

         // Get the list of SpectrumIdentification elements
         List<SpectrumIdentificationList> sil = ad.getSpectrumIdentificationList();
         assertEquals(1, sil.size());
         assertEquals(169, sil.get(0).getSpectrumIdentificationResult().size());

         int countSII = 0;
         int countPepEvid = 0;
         SpectrumIdentificationItem sIIWith2PepEvidence = null;
         SpectrumIdentificationItem sIIWith3PepEvidence = null;
         
         for (SpectrumIdentificationList sIdentList : sil) 
         {
              for (SpectrumIdentificationResult spectrumIdentResult
                      : sIdentList.getSpectrumIdentificationResult()) 
              {

            	  String spectrumID = spectrumIdentResult.getSpectrumID();
            	  
                  for (SpectrumIdentificationItem spectrumIdentItem
                       : spectrumIdentResult.getSpectrumIdentificationItem()) 
                  {
                	  countSII++;

                      if (spectrumID.equals("index=241"))
                    	  sIIWith2PepEvidence = spectrumIdentItem;
                      if (spectrumID.equals("index=302"))
                    	  sIIWith3PepEvidence = spectrumIdentItem;                      
                      
                      countPepEvid += spectrumIdentItem.getPeptideEvidenceRef().size();
                      
                      // If the auto-resolve mechanism is activated for SpectrumIdentificationItem
                      // then automatically resolve the Peptide Object; 
                      // (see  http://code.google.com/p/jmzidentml/wiki/AutoResolveExample)
                      if (MzIdentMLElement.SpectrumIdentificationItem.isAutoRefResolving()
                              && spectrumIdentItem.getPeptideRef() != null) {
                           Peptide peptide = spectrumIdentItem.getPeptide();
                           String peptideSequence = peptide.getPeptideSequence();
                          System.out.println("Pepetide Sequence = " + peptideSequence);
                      }

                  } // end spectrum identification item
              } // end spectrum identification results
         }
         
         SequenceCollection sc =  unmarshaller.unmarshal(SequenceCollection.class);
         
         //from the 169 identifications, we expect to find the following:
         // - one SIR that has 2 SIIs  (one mass peak matching to 2 different peptides)
         // - one SII (of another SIR) that has 2 PeptideEvidence items  (one of the peptides is found in 2 different proteins)
         //These are checked below.
         
         //Evidence for one of the SIRs having 2 SIIs
         assertEquals(170, countSII);
         
         //Evidence for one SII having 2 PeptideEvidence items, and one SII having 3 PeptideEvidence items:
         assertEquals(173, countPepEvid);
         assertEquals(2, sIIWith2PepEvidence.getPeptideEvidenceRef().size());
         assertEquals("GVGAER___", sIIWith2PepEvidence.getPeptideRef());
         assertEquals("GVGAER___", getPeptideEvidenceItem(sc, sIIWith2PepEvidence.getPeptideEvidenceRef().get(0)).getPeptideRef());
         assertEquals("GVGAER___", getPeptideEvidenceItem(sc, sIIWith2PepEvidence.getPeptideEvidenceRef().get(1)).getPeptideRef());
         
         //EEEEKGEEEK case (is a peptide matching 3 times on the same protein, at different places - is similar
         //to spectrum 6642 in test_modifications test case, so skipping refined tests for now):
         assertEquals("EEEEKGEEEK___", sIIWith3PepEvidence.getPeptideRef());
         assertEquals(3, sIIWith3PepEvidence.getPeptideEvidenceRef().size());
         
    }

		
	/**
	 * Returns the PeptideEvidence object given the PeptideEvidenceRef item.
	 * 
	 * @param sc 
	 * @param peptideEvidenceRef
	 * @return
	 */
	private PeptideEvidence getPeptideEvidenceItem(SequenceCollection sc, PeptideEvidenceRef peptideEvidenceRef) 
	{
		List<PeptideEvidence> pepEvidenceList = sc.getPeptideEvidence();
		for (PeptideEvidence pepEvidence : pepEvidenceList)
		{
			if (pepEvidence.getId().equals(peptideEvidenceRef.getPeptideEvidenceRef()))
				return pepEvidence;
		}
		return null;
	}



	public void test_debug_File() throws Exception
	{
		String xTandemFileFromGalaxyStep = "test/data/DEBUG_MSMS_XTANDEM.bioml";
		String outMzid = xTandemFileFromGalaxyStep + ".mzid";
		new Tandem2mzid(xTandemFileFromGalaxyStep, outMzid, "MS:1001348","MS:1000584",false);
		
		 //===================================================================================
		 //========================= Checks /assertions section :=============================
		 //===================================================================================
		
		//we expect TO find in the output the following items:
		// - 1 SIR
		// - 2 SII, each with 2 PeptideEvidence items. Sub-checks: 
		//    o the PeptideEvidence have the same sequence
		//    o the first SII is about evidence on  YIYEIAR
		//	  o the second SII is about evidence on YLYEIAR
		// - 4 dbsequences
		// - 2 peptides
		// - 4 peptideEvidence items
		
        MzIdentMLUnmarshaller unmarshaller = new MzIdentMLUnmarshaller(new File(outMzid));

        DataCollection dc =  unmarshaller.unmarshal(DataCollection.class);
        AnalysisData ad = dc.getAnalysisData();

        // Get the list of SpectrumIdentification elements
        List<SpectrumIdentificationList> sil = ad.getSpectrumIdentificationList();
        assertEquals(1, sil.size());
        // - 1 SIR
        assertEquals(1, sil.get(0).getSpectrumIdentificationResult().size());

        int countSII = 0;
        int countPepEvid = 0;
        SpectrumIdentificationItem first_sII = sil.get(0).getSpectrumIdentificationResult().get(0).getSpectrumIdentificationItem().get(0);
        SpectrumIdentificationItem second_sII = sil.get(0).getSpectrumIdentificationResult().get(0).getSpectrumIdentificationItem().get(1);
        
        for (SpectrumIdentificationList sIdentList : sil) 
        {
             for (SpectrumIdentificationResult spectrumIdentResult
                     : sIdentList.getSpectrumIdentificationResult()) 
             {
                 for (SpectrumIdentificationItem spectrumIdentItem
                      : spectrumIdentResult.getSpectrumIdentificationItem()) 
                 {
               	  	 countSII++;
                     countPepEvid += spectrumIdentItem.getPeptideEvidenceRef().size();
                     
                 } // end spectrum identification item
             } // end spectrum identification results
        }
        
        SequenceCollection sc =  unmarshaller.unmarshal(SequenceCollection.class);
        
        // - 2 SII, each with 2 PeptideEvidence items. 
        assertEquals(2, countSII);
        assertEquals(2, first_sII.getPeptideEvidenceRef().size());
        assertEquals(2, second_sII.getPeptideEvidenceRef().size());
        //Sub-checks: 
        //	o the PeptideEvidence items within a SII have the same sequence
  		//	o the first SII is about evidence on  YIYEIAR
  		//	o the second SII is about evidence on YLYEIAR

        assertEquals("YIYEIAR___", first_sII.getPeptideRef());
        assertEquals("YIYEIAR___", getPeptideEvidenceItem(sc, first_sII.getPeptideEvidenceRef().get(0)).getPeptideRef());
        assertEquals("YIYEIAR___", getPeptideEvidenceItem(sc, first_sII.getPeptideEvidenceRef().get(1)).getPeptideRef());

        assertEquals("YLYEIAR___", second_sII.getPeptideRef());
        assertEquals("YLYEIAR___", getPeptideEvidenceItem(sc, second_sII.getPeptideEvidenceRef().get(0)).getPeptideRef());
        assertEquals("YLYEIAR___", getPeptideEvidenceItem(sc, second_sII.getPeptideEvidenceRef().get(1)).getPeptideRef());
        
        // - 4 peptideEvidence items
        assertEquals(4, countPepEvid);
        
        // - 2 peptides
        assertEquals(2, sc.getPeptide().size());
        
     	// - 4 dbsequences 
        assertEquals(4, sc.getDBSequence().size());
        
	}
	
	/**
	 * Test case with special scenario that includes a peptide with modifications found in 2 different proteins.
	 * 
	 * 
	 * TODO - run this test case again after fix in xtandem parser
	 *        Currently it is failing at a number of places
	 *        due to bug reported at http://code.google.com/p/xtandem-parser/issues/detail?id=9
	 * 
	 * @throws Exception
	 */
	public void test_modifications() throws Exception
	{
		String xTandemFile = "test/data/S2_depl_spiked_6.mzML_xtandemOut.bioml";
		String resultFile = "test/data/S2_depl_spiked_6.mzML_xtandemOut.bioml.mzid";
		boolean isMs2SpectrumIdStartingAtZero = true;
		new Tandem2mzid(xTandemFile, resultFile, "MS:1001348","MS:1000584",isMs2SpectrumIdStartingAtZero);
		
		//===================================================================================
		//========================= Checks /assertions section :=============================
		//===================================================================================
		
		//we expect TO find in the output the following items:
		// - 685 SIR
		// - when zooming in on spectrum 6642, we expect: 
		//		o 1 SII, with 2 PeptideEvidence items. Sub-checks: 
		//			> both the PeptideEvidence have the same sequence YGAEDMSGMEGSSGLGDRFGAK
		//			> the first PeptideEvidence matches region start="1531" end="1552"
		//			> the second PeptideEvidence matches region start="2035" end="2056"
		
		MzIdentMLUnmarshaller unmarshaller = new MzIdentMLUnmarshaller(new File(resultFile));

        DataCollection dc =  unmarshaller.unmarshal(DataCollection.class);
        AnalysisData ad = dc.getAnalysisData();

        // Get the list of SpectrumIdentification elements
        List<SpectrumIdentificationList> sil = ad.getSpectrumIdentificationList();
        assertEquals(1, sil.size());
        // - 685 SIR
        assertEquals(685, sil.get(0).getSpectrumIdentificationResult().size());
        
        SpectrumIdentificationResult sIR6642 = null;
        SpectrumIdentificationResult sIR3099 = null;        
        
        for (SpectrumIdentificationList sIdentList : sil) 
        {
             for (SpectrumIdentificationResult spectrumIdentResult
                     : sIdentList.getSpectrumIdentificationResult()) 
             {
            	 String spectrumID = spectrumIdentResult.getSpectrumID();
            	 if (spectrumID.equals("index=6642"))
            		 sIR6642 = spectrumIdentResult;
            	 if (spectrumID.equals("index=3099"))
            		 sIR3099 = spectrumIdentResult;
            	 
             } // end spectrum identification results
        }
        
        // - when zooming in on spectrum 6642, we expect: 
 		//		o 1 SII, with 2 PeptideEvidence items. Sub-checks: 
 		//			> both the PeptideEvidence have the same sequence YGAEDMSGMEGSSGLGDRFGAK
 		//			> the first PeptideEvidence matches region start="1531" end="1552"
 		//			> the second PeptideEvidence matches region start="2035" end="2056"
    
        assertEquals(1, sIR6642.getSpectrumIdentificationItem().size());
        SpectrumIdentificationItem first_sII = sIR6642.getSpectrumIdentificationItem().get(0);
        assertEquals(2, first_sII.getPeptideEvidenceRef().size());

        //Sub-checks: 
 		//			> both the PeptideEvidence have the same sequence YGAEDMSGMEGSSGLGDRFGAK, 
        //			  with the same modifications annotations at positions 6, 9, 12, 13, 1
        //			  that is: 15.9949@M$6;15.9949@M$9;79.9663@S$12;79.9663@S$13;79.9663@S$1;
        SequenceCollection sc =  unmarshaller.unmarshal(SequenceCollection.class);
        String peptideSignature = "YGAEDMSGMEGSSGLGDRFGAK_15.9949@M$6;15.9949@M$9;79.9663@S$12;79.9663@S$13;79.9663@Y$1;__";
        assertEquals(peptideSignature, first_sII.getPeptideRef());
        assertEquals(peptideSignature, getPeptideEvidenceItem(sc, first_sII.getPeptideEvidenceRef().get(0)).getPeptideRef());
        assertEquals(peptideSignature, getPeptideEvidenceItem(sc, first_sII.getPeptideEvidenceRef().get(1)).getPeptideRef());
 		//			> the first PeptideEvidence matches region start="1531" end="1552"
        assertEquals(1531, getPeptideEvidenceItem(sc, first_sII.getPeptideEvidenceRef().get(0)).getStart().intValue());
        assertEquals(1552, getPeptideEvidenceItem(sc, first_sII.getPeptideEvidenceRef().get(0)).getEnd().intValue());
 		//			> the second PeptideEvidence matches region start="2035" end="2056"
        assertEquals(2035, getPeptideEvidenceItem(sc, first_sII.getPeptideEvidenceRef().get(1)).getStart().intValue());
        assertEquals(2056, getPeptideEvidenceItem(sc, first_sII.getPeptideEvidenceRef().get(1)).getEnd().intValue());
	
        
        //Here the case of AVPSVSSNVTAMSLQWNLIR in spectrum 3099, where it is matched 
        //with a modification on M and on its second <OR> third S !
        //What we expect is:
        // - there are 2 SII, one for each set of modifications (@M and @2ndS, @M and @3rdS)
        assertEquals(2, sIR3099.getSpectrumIdentificationItem().size());
        first_sII = sIR3099.getSpectrumIdentificationItem().get(0);
        // - each SII has 8 PeptideEvidence items (on 8 different proteins)
        assertEquals(8, first_sII.getPeptideEvidenceRef().size());
        SpectrumIdentificationItem second_sII = sIR3099.getSpectrumIdentificationItem().get(1);
        assertEquals(8, second_sII.getPeptideEvidenceRef().size());
        
	}
	
	
/*	
	public void test_galaxyFile() throws Exception
	{
		String xTandemFileFromGalaxyStep = "test/data/Galaxy-[X_Tandem_on_data_N].bioml";
		new Tandem2mzid(xTandemFileFromGalaxyStep, xTandemFileFromGalaxyStep + ".mzid", true);
	}

	
	public void test_multiple_sii_case() throws Exception
	{
		String xTandemFileFromGalaxyStep = "test/data/20091015_spiking_0_05fmol_CytC_MSMS_XTANDEM.out";
		String outMzid = xTandemFileFromGalaxyStep + ".mzid";
		new Tandem2mzid(xTandemFileFromGalaxyStep, outMzid, true);
	}
	
	
	
	*/		
}
