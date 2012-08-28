package uk.ac.ebi.jmzidml.test.xml;

import junit.framework.TestCase;
import org.apache.log4j.Logger;
import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.*;
import uk.ac.ebi.jmzidml.model.mzidml.params.SearchDatabaseCvParam;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLUnmarshaller;

import java.net.URL;
import java.util.Iterator;
import java.util.List;

/**
 * Package  : uk.ac.ebi.jmzidml.test.xml
 * Author   : riteshk
 * Date     : Sep 18, 2010
 */
public class AnalysisCollectionTest extends TestCase {

    private static final Logger log = Logger.getLogger(AnalysisCollectionTest.class);

    public void testAnalysisCollectionInformation() throws Exception {

        URL xmlFileURL = AnalysisCollectionTest.class.getClassLoader().getResource("Mascot_MSMS_example.mzid");
        assertNotNull(xmlFileURL);

//        MzIdentMLObjectCache cache = new AdapterObjectCache();
//        MzIdentMLUnmarshaller unmarshaller = new MzIdentMLUnmarshaller(xmlFileURL, cache);
        MzIdentMLUnmarshaller unmarshaller = new MzIdentMLUnmarshaller(xmlFileURL);
        assertNotNull(unmarshaller);

        AnalysisCollection asc =  unmarshaller.unmarshal(AnalysisCollection.class);
        assertNotNull(asc);

        ProteinDetection pd = asc.getProteinDetection();
        Iterator<SpectrumIdentification> si = asc.getSpectrumIdentification().iterator();
        assertNotNull(pd);
        assertNotNull(si);

        log.debug("Protein Detection: " + pd.getId() + "\t" + pd.getName());
        if (MzIdentMLElement.ProteinDetection.isAutoRefResolving() && pd.getProteinDetectionListRef() != null) {
            ProteinDetectionList pdl = pd.getProteinDetectionList();
            assertNotNull(pdl);
            log.debug("Protein Detection List: Name: " + pdl.getName() + "\t Id : " + pdl.getId());

            int pagCnt = pdl.getProteinAmbiguityGroup().size();
            assertTrue(pagCnt == 5);
            for (ProteinAmbiguityGroup proteinAmbiguityGroup : pdl.getProteinAmbiguityGroup()) {
                log.debug("Protein Ambiguity Group: Id: " + proteinAmbiguityGroup.getId()
                        + "\t Name: " + proteinAmbiguityGroup.getName());


                int pdhCnt = proteinAmbiguityGroup.getProteinDetectionHypothesis().size();
                assertTrue(pdhCnt > 0);
                for (ProteinDetectionHypothesis protHypo : proteinAmbiguityGroup.getProteinDetectionHypothesis()) {
                    if (MzIdentMLElement.ProteinDetectionHypothesis.isAutoRefResolving() && protHypo.getDBSequenceRef() != null) {
                        DBSequence seq = protHypo.getDBSequence();
                        assertNotNull(seq);
                        log.debug(" Protein Detection Hypothesis: DBSequence Accn: " + protHypo.getDBSequence().getAccession()
                                + " seq length:" + protHypo.getDBSequence().getLength());
                        assertTrue(seq.getLength() > 0);
                        assertNotNull(seq.getAccession());
                    } else {
                        System.out.println("ProteinDetectionHypothesis is not auto-resolving or does not contain a DBSequence reference.");
                        assertNull(protHypo.getDBSequence());
                    }

                    int phCnt = protHypo.getPeptideHypothesis().size();
                    assertTrue(phCnt > 0);
                    for (PeptideHypothesis pepHypo : protHypo.getPeptideHypothesis()) {
                        assertNotNull(pepHypo);
                        if (MzIdentMLElement.PeptideHypothesis.isAutoRefResolving() && pepHypo.getPeptideEvidenceRef() != null) {
                            PeptideEvidence pepEvd = pepHypo.getPeptideEvidence();
                            assertNotNull(pepEvd);
                            log.debug("Peptide Evidence Name: " + pepEvd.getName());
                            if (MzIdentMLElement.PeptideEvidence.isAutoRefResolving() && pepEvd.getDBSequenceRef() != null) {
                                assertNotNull(pepEvd.getDBSequence());
                            } else {
                                System.out.println("PeptideEvidence is not auto-resolving or does not contain a DBSequence reference.");
                                assertNull(pepEvd.getDBSequence());
                            }
                        } else {
                            System.out.println("PeptideHypothesis is not auto-resolving or does not contain a PeptideEvidence reference.");
                            assertNull(pepHypo.getPeptideEvidence());
                        }
                    } // end for-all PeptideHypothesis
                } // end for-all ProteinDetectionHypothesis
            } // end for-all ProteinAmbiguityGroups
        } else {
            System.out.println("ProteinDetection is not auto-resolving.");
            assertNull(pd.getProteinDetectionList());
        }

        // check InputSpectrumIdentifications
        List<InputSpectrumIdentifications> specIdents = pd.getInputSpectrumIdentifications();
        if (specIdents != null && specIdents.size() > 0) {
            for (InputSpectrumIdentifications ident : specIdents) {
                if (MzIdentMLElement.InputSpectrumIdentifications.isAutoRefResolving() && ident.getSpectrumIdentificationListRef() != null) {
                    assertNotNull(ident.getSpectrumIdentificationList());
                    assertNotNull(ident.getSpectrumIdentificationList().getId());
                } else {
                    System.out.println("InputSpectrumIdentifications is not auto-resolving or does not contain a SpectrumIdentificationList reference.");
                    assertNull(ident.getSpectrumIdentificationList());
                }
            }
        }



        // *************** Spectrum Info ***************

        while(si.hasNext()){

            SpectrumIdentification sid = si.next();
            log.debug("SpectrumIdentification Id :" + sid.getId());
            assertNotNull(sid.getId());


            if (MzIdentMLElement.SpectrumIdentification.isAutoRefResolving() && sid.getSpectrumIdentificationProtocolRef() != null) {
                SpectrumIdentificationProtocol sip = sid.getSpectrumIdentificationProtocol();
                assertNotNull(sip);
                if (MzIdentMLElement.SpectrumIdentificationProtocol.isAutoRefResolving() && sip.getAnalysisSoftwareRef() != null) {
/*
                    contact class has been removed. this information is now stored in a list of params. A utility
                    class will provide convenience methods wuch as getAddress and getEmail which will parse
                    log.debug("Software Name :" + sip.getAnalysisSoftware().getSoftwareName().getCvParam().getName());
                    assertEquals("Mascot", sip.getAnalysisSoftware().getSoftwareName().getCvParam().getName());
*/
                } else {
                    System.out.println("SpectrumIdentificationProtocol is not auto-resolving or does not contain a Software reference.");
                    assertNull(sip.getAnalysisSoftware());
                }
            } else {
                System.out.println("SpectrumIdentification is not auto-resolving or does not contain a SpectrumIdentificationProtocol reference.");
                assertNull(sid.getSpectrumIdentificationProtocol());
            }

            List<InputSpectra> is = sid.getInputSpectra();
            assertTrue(is.size() > 0);
            InputSpectra iSpectra = is.get(0);
            if (MzIdentMLElement.InputSpectra.isAutoRefResolving() && iSpectra.getSpectraDataRef() != null) {
                String spectraID = iSpectra.getSpectraData().getId();
                assertNotNull(spectraID);
                log.debug("Input Spectra : " + spectraID);
            } else {
                System.out.println("InputSpectra is not auto-resolving or does not contain a SpectraData reference.");
                assertNull(iSpectra.getSpectraData());
            }

            List<SearchDatabaseRef> sdbl  = sid.getSearchDatabaseRef();
            assertTrue(sdbl.size() > 0);
            for (SearchDatabaseRef db : sdbl) {
                if (MzIdentMLElement.SearchDatabaseRef.isAutoRefResolving() && db.getSearchDatabaseRef() != null) {
                    SearchDatabase searchDB = db.getSearchDatabase();
                    assertNotNull(searchDB);
                    assertEquals("SwissProt", searchDB.getName());

                    // quickly check the sub-classing of the CvParams
                    assertTrue(searchDB.getCvParam().size() > 0);
                    for (CvParam cvParam : searchDB.getCvParam()) {
                        assertTrue("Cv params have to be sub-classed.", (cvParam instanceof SearchDatabaseCvParam) );
                        if (MzIdentMLElement.CvParam.isAutoRefResolving() && cvParam.getCvRef() != null) {
                            assertNotNull(cvParam.getCv());
                        } else {
                            System.out.println("CvParam is not auto-resolving or does not contain a Cv reference.");
                            assertNull(cvParam.getCv());
                        }
                    }
                } else {
                    System.out.println("SearchDatabase is not auto-resolving or does not contain a AnalysisSearchDatabase reference.");
                    assertNull(db.getSearchDatabase());
                }
            }


            if (MzIdentMLElement.SpectrumIdentification.isAutoRefResolving() && sid.getSpectrumIdentificationListRef() != null) {
                SpectrumIdentificationList spl = sid.getSpectrumIdentificationList();
                assertNotNull(spl);
                Iterator<SpectrumIdentificationResult> sr  = spl.getSpectrumIdentificationResult().iterator();
                assertTrue(sr.hasNext());
                String identResultSpectraID = sr.next().getSpectrumID();
                log.debug("SpectrumIdentificationResult Id : " + identResultSpectraID);
                assertNotNull(identResultSpectraID);
            } else {
                System.out.println("SpectrumIdentification is not auto-resolving or does not contain a SpectrumIdentificationList reference.");
                assertNull(sid.getSpectrumIdentificationList());
            }

        }
        
    }

}
