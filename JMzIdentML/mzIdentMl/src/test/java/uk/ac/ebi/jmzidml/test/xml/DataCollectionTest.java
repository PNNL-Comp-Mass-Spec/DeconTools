package uk.ac.ebi.jmzidml.test.xml;

import junit.framework.TestCase;
import org.apache.log4j.Logger;
import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.*;
import uk.ac.ebi.jmzidml.model.mzidml.params.FileFormatCvParam;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLUnmarshaller;

import java.net.URL;
import java.util.List;

/**
 * Package  : uk.ac.ebi.jmzidml.test.xml
 * Author   : riteshk
 * Date     : Sep 19, 2010
 */
public class DataCollectionTest extends TestCase {

    Logger log = Logger.getLogger(DataCollectionTest.class);


    public void testDataCollectionInformation() throws Exception {

        URL xmlFileURL = DataCollectionTest.class.getClassLoader().getResource("Mascot_MSMS_example.mzid");
        assertNotNull(xmlFileURL);

        MzIdentMLUnmarshaller unmarshaller = new MzIdentMLUnmarshaller(xmlFileURL);
        assertNotNull(unmarshaller);

        DataCollection dc =  unmarshaller.unmarshal(DataCollection.class);
        assertNotNull(dc);

        Inputs dcInputs = dc.getInputs();
        assertNotNull(dcInputs);

        assertEquals(1, dcInputs.getSearchDatabase().size());
        SearchDatabase asd = dcInputs.getSearchDatabase().get(0);

        String searchDB = asd.getId(); // store for later reference
        String searchDBName = asd.getName();
        assertEquals("SwissProt", searchDBName);
        assertEquals("SDB_SwissProt", searchDB);
        assertEquals("SwissProt_51.6.fasta", asd.getVersion());
        log.debug("Inout -> SearchDatabase Location:" + asd.getLocation() + " Id:" + asd.getId()
                + " Name:" + asd.getName() + " Version:" + asd.getVersion());

        assertNotNull(asd.getFileFormat());
        CvParam cvParam = asd.getFileFormat().getCvParam();
        assertNotNull(cvParam);
        assertEquals("MS:1001348", cvParam.getAccession());
        // test the sub-classing of a CvParamCapable (with only one CvParam)
        assertTrue(cvParam instanceof FileFormatCvParam);


        assertEquals(1, dcInputs.getSourceFile().size());
        SourceFile sourceFile = dcInputs.getSourceFile().get(0);
        assertNotNull(sourceFile);
        assertTrue(sourceFile.getLocation().endsWith(".dat"));
        log.debug("Input -> SourceFile : Location:" + sourceFile.getLocation());

        assertEquals(1, dcInputs.getSpectraData().size());
        SpectraData spectraData = dcInputs.getSpectraData().get(0);
        assertTrue(spectraData.getId().startsWith("SD"));
        assertEquals("MS:1001528", spectraData.getSpectrumIDFormat().getCvParam().getAccession());
        assertEquals("Mascot query number", spectraData.getSpectrumIDFormat().getCvParam().getName());


        //**************** Analysis Data ***************************

        AnalysisData ad = dc.getAnalysisData();
        assertNotNull(ad);

        ProteinDetectionList pdl  = ad.getProteinDetectionList();
        assertNotNull(pdl);
        assertEquals("PDL_1", pdl.getId());

        // check ProteinAmbiguityGroup
        assertEquals(5, pdl.getProteinAmbiguityGroup().size());
        for (ProteinAmbiguityGroup proteinAmbiguityGroup : pdl.getProteinAmbiguityGroup()) {
            assertTrue(proteinAmbiguityGroup.getId().startsWith("PAG_hit_"));

            assertTrue(proteinAmbiguityGroup.getProteinDetectionHypothesis().size() > 0);
            for (ProteinDetectionHypothesis proteinDetectionHypothesis : proteinAmbiguityGroup.getProteinDetectionHypothesis()) {
                assertNotNull(proteinDetectionHypothesis.getId());
                if (MzIdentMLElement.ProteinDetectionHypothesis.isAutoRefResolving() && proteinDetectionHypothesis.getDBSequenceRef() != null) {
                    DBSequence seq = proteinDetectionHypothesis.getDBSequence();
                    assertNotNull(seq);
                    if (MzIdentMLElement.DBSequence.isAutoRefResolving() && seq.getSearchDatabaseRef() != null) {
                        assertNotNull(seq.getSearchDatabase());
                        assertNotNull(seq.getSearchDatabase().getName());
                        assertEquals(searchDB, seq.getSearchDatabase().getId());
                        assertEquals(searchDBName, seq.getSearchDatabase().getName());
                    } else {
                        System.out.println("DBSequence is not auto-resolving or does not contain a SearchDatabase reference.");
                        assertNull(seq.getSearchDatabase());
                    }
                } else {
                    System.out.println("ProteinDetectionHypothesis is not auto-resolving or does not contain a DBSequence reference.");
                    assertNull(proteinDetectionHypothesis.getDBSequence());
                }

                for (PeptideHypothesis peptideHypothesis : proteinDetectionHypothesis.getPeptideHypothesis()) {
                    if (MzIdentMLElement.PeptideHypothesis.isAutoRefResolving() && peptideHypothesis.getPeptideEvidenceRef() != null) {
                        PeptideEvidence evd = peptideHypothesis.getPeptideEvidence();
                        assertNotNull(evd);
                        assertTrue(evd.getId().startsWith("PE"));
                        if (MzIdentMLElement.PeptideEvidence.isAutoRefResolving() && evd.getDBSequenceRef() != null) {
                            assertNotNull(evd.getDBSequence());
                            assertTrue(evd.getDBSequence().getSeq().length() > 5);
                        } else {
                            System.out.println("PeptideEvidence is not auto-resolving or does not contain a DBSequence reference.");
                            assertNull(evd.getDBSequence());
                        }
                    } else {
                        System.out.println("PeptideHypothesis is not auto-resolving or does not contain a PeptideEvidence reference.");
                        assertNull(peptideHypothesis.getPeptideEvidence());
                    }
                } // end for-all PeptideHypothesis
            } // end for-all ProteinDetectionHypothesis
        } // end for-all ProteinAmbiguityGroup


        List<SpectrumIdentificationList> sil = ad.getSpectrumIdentificationList();
        assertNotNull(sil);
        assertEquals(1, sil.size()); // only one spectrum identification list

        for (SpectrumIdentificationList sIdentList : sil) {
            assertTrue(sIdentList.getId().startsWith("SIL_"));

            assertEquals(3, sIdentList.getFragmentationTable().getMeasure().size());
            for (Measure measure : sIdentList.getFragmentationTable().getMeasure()) {
                assertTrue(measure.getId().startsWith("m_"));
                assertEquals(1, measure.getCvParam().size());
                assertTrue(measure.getCvParam().get(0).getName().startsWith("product ion"));
            }

            assertEquals(4, sIdentList.getSpectrumIdentificationResult().size());
            for (SpectrumIdentificationResult spectrumIdentResult : sIdentList.getSpectrumIdentificationResult()) {
                assertEquals(10, spectrumIdentResult.getSpectrumIdentificationItem().size());
                for (SpectrumIdentificationItem spectrumIdentItem : spectrumIdentResult.getSpectrumIdentificationItem()) {
                    assertTrue(spectrumIdentItem.getId().startsWith("SII_"));

                    if (MzIdentMLElement.SpectrumIdentificationItem.isAutoRefResolving() && spectrumIdentItem.getPeptideRef() != null) {
                        Peptide peptide = spectrumIdentItem.getPeptide();
                        assertNotNull(peptide);
                        int seqLength = peptide.getPeptideSequence().length();
                        assertTrue(seqLength > 5 && seqLength < 20); // peptide seq length is much shorter than the peptide evidence seq length
                    } else {
                        System.out.println("SpectrumIdentificationItem is not auto-resolving or does not contain a Peptide reference.");
                        assertNull(spectrumIdentItem.getPeptide());
                    }

                    for (PeptideEvidenceRef peptideEvidenceRef : spectrumIdentItem.getPeptideEvidenceRef()) {
                        if (MzIdentMLElement.PeptideEvidence.isAutoRefResolving() && peptideEvidenceRef.getPeptideEvidence().getDBSequenceRef() != null) {
                            assertNotNull(peptideEvidenceRef.getPeptideEvidence().getDBSequence());
                            assertTrue(peptideEvidenceRef.getPeptideEvidence().getDBSequence().getSeq().length() > 30);
                        } else {
                            System.out.println("PeptideEvidence is not auto-resolving or does not contain a DBSequence reference.");
                            /**
                             *  Now that peptideevidence is not auto resolving confirm PeptideEvidence is null
                             */
                            assertNull(peptideEvidenceRef.getPeptideEvidence());


                        }
                    }
                } // end spectrum identification items
                // check SpectrumIdentificationResult.spectraData
                if (MzIdentMLElement.SpectrumIdentificationResult.isAutoRefResolving() && spectrumIdentResult.getSpectraDataRef() != null) {
                    SpectraData specData = spectrumIdentResult.getSpectraData();
                    assertNotNull(specData);
                    assertNotNull(specData.getId());
                } else {
                    System.out.println("SpectrumIdentificationResult is not auto-resolving or does not contain a SpectraData reference.");
                    assertNull(spectrumIdentResult.getSpectraData());
                }


            } // end spectrum identification results

        } // end spectrum identifications
    } // end test method
}
