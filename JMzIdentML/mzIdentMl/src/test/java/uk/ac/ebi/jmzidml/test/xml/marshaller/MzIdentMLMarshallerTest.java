package uk.ac.ebi.jmzidml.test.xml.marshaller;

import org.junit.Test;
import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.*;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLMarshaller;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLUnmarshaller;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.net.URL;
import java.util.Iterator;

import static junit.framework.Assert.assertNotNull;
import static junit.framework.Assert.assertTrue;

/**
 * This test is not run automatically, since it only creates a mzIdentML output file.
 * No real additional testing is performed. It is just for manual checking.
 * <p/>
 * TODO: automate comparing before and after values of marshalling.
 *
 * @author Florian Reisinger
 *         Date: 03-Dec-2010
 * @since 1.0
 */
@SuppressWarnings("unused")
public class MzIdentMLMarshallerTest {


    @Test
    public void testIncrementalMarshalling() throws IOException {
        int cvCount = -1;
        int analysisSoftwareCount = -1;
        int auditCount = -1;
        int dbSequenceCount = -1;
        int peptideEvidencePeptideCount = -1;
        URL xmlFileURL = MzIdentMLMarshallerTest.class.getClassLoader().getResource("Mascot_MSMS_example.mzid");
        assertNotNull(xmlFileURL);
        MzIdentMLUnmarshaller unmarshaller = new MzIdentMLUnmarshaller(xmlFileURL);
        assertNotNull(unmarshaller);

        MzIdentMLMarshaller m = new MzIdentMLMarshaller();
        assertNotNull(m);

        FileWriter writer = null;
        try {
            writer = new FileWriter("output.xml");

            // mzIdentML
            //     cvList
            //     AnalysisSoftwareList
            //     Provider
            //     AuditCollection
            //     AnalysisSampleCollection
            //     SequenceCollection
            //     AnalysisCollection
            //     AnalysisProtocolCollection
            //     DataCollection
            //         Inputs
            //         AnalysisData
            //             SpectrumIdentificationList
            //             ProteinDetectionList
            //         /AnalysisData
            //     /DataCollection
            //     BibliographicReference
            // /mzIdentML


            // Note: writing of '\n' characters is optional and only for readability of the produced XML document
            // Also note: since the XML is produced in individual parts, the overall formatting of the document
            //            is not as nice as it would be when marshalling the whole structure at once.

            // XML header
            writer.write(m.createXmlHeader() + "\n");
            // mzIdentML start tag
            writer.write(m.createMzIdentMLStartTag("12345") + "\n");

            CvList cvList = unmarshaller.unmarshal(MzIdentMLElement.CvList.getXpath());
            cvCount = cvList.getCv().size();
            m.marshal(cvList, writer);
            writer.write("\n");

            AnalysisSoftwareList analysisSoftwareList = unmarshaller.unmarshal(MzIdentMLElement.AnalysisSoftwareList.getXpath());
            analysisSoftwareCount = analysisSoftwareList.getAnalysisSoftware().size();
            m.marshal(analysisSoftwareList, writer);
            writer.write("\n");

            Provider provider = unmarshaller.unmarshal(MzIdentMLElement.Provider.getXpath());
            m.marshal(provider, writer);
            writer.write("\n");

            AuditCollection auditCollection = unmarshaller.unmarshal(MzIdentMLElement.AuditCollection.getXpath());
            auditCount = auditCollection.getPersonOrOrganization().size();
            m.marshal(auditCollection, writer);
            writer.write("\n");

            AnalysisSampleCollection analysisSampleCollection = unmarshaller.unmarshal(MzIdentMLElement.AnalysisSampleCollection.getXpath());
            m.marshal(analysisSampleCollection, writer);
            writer.write("\n");

            SequenceCollection sequenceCollection = unmarshaller.unmarshal(MzIdentMLElement.SequenceCollection.getXpath());
            dbSequenceCount = sequenceCollection.getDBSequence().size();
            peptideEvidencePeptideCount = sequenceCollection.getPeptideEvidence().size();
            m.marshal(sequenceCollection, writer);
            writer.write("\n");

            AnalysisCollection analysisCollection = unmarshaller.unmarshal(MzIdentMLElement.AnalysisCollection.getXpath());
            m.marshal(analysisCollection, writer);
            writer.write("\n");

            AnalysisProtocolCollection analysisProtocolCollection = unmarshaller.unmarshal(MzIdentMLElement.AnalysisProtocolCollection.getXpath());
            analysisProtocolCollection.setProteinDetectionProtocol(analysisProtocolCollection.getProteinDetectionProtocol());
            m.marshal(analysisProtocolCollection, writer);
            writer.write("\n");

            writer.write(m.createDataCollectionStartTag() + "\n");

            Inputs inputs = unmarshaller.unmarshal(MzIdentMLElement.Inputs.getXpath());
            m.marshal(inputs, writer);
            writer.write("\n");

            writer.write(m.createAnalysisDataStartTag() + "\n");

            writer.write(m.createSpectrumIdentificationListStartTag("SIL_1", null, 71412L) + "\n");

            FragmentationTable table = unmarshaller.unmarshal(MzIdentMLElement.FragmentationTable.getXpath());
            m.marshal(table, writer);
            writer.write("\n");

            Iterator<SpectrumIdentificationResult> specResIter = unmarshaller.unmarshalCollectionFromXpath(MzIdentMLElement.SpectrumIdentificationResult);
            while (specResIter.hasNext()) {
                SpectrumIdentificationResult specIdentRes = specResIter.next();
                m.marshal(specIdentRes, writer);
                writer.write("\n");
            }

            writer.write(m.createSpectrumIdentificationListClosingTag() + "\n");
            writer.write(m.createProteinDetectionListStartTag("PDL_1", null) + "\n");
            Iterator<ProteinAmbiguityGroup> protAmbGroupIter = unmarshaller.unmarshalCollectionFromXpath(MzIdentMLElement.ProteinAmbiguityGroup);
            while (protAmbGroupIter.hasNext()) {
                ProteinAmbiguityGroup protAmbGroup = protAmbGroupIter.next();
                m.marshal(protAmbGroup, writer);
                writer.write("\n");
            }

            writer.write(m.createProteinDetectionListClosingTag() + "\n");

            writer.write(m.createAnalysisDataClosingTag() + "\n");

            writer.write(m.createDataCollectionClosingTag() + "\n");

            BibliographicReference ref = unmarshaller.unmarshal(MzIdentMLElement.BibliographicReference.getXpath());
            m.marshal(ref, writer);
            writer.write("\n");

            writer.write(m.createMzIdentMLClosingTag());

        } finally {
            if (writer != null) writer.close();
        }
        File outputFile = new File("output.xml");
        unmarshaller = new MzIdentMLUnmarshaller(outputFile);
        MzIdentML mzIdentMl = unmarshaller.unmarshal(MzIdentMLElement.MzIdentML);
        /**
         * Do some shallow testing. Confirm correct number of child elements returned for each tested element.
         */
        assertTrue(mzIdentMl.getId().equals("12345"));
        assertTrue(cvCount >= 0);
        assertTrue(mzIdentMl.getCvList().getCv().size() == cvCount);
        assertTrue(analysisSoftwareCount >= 0);
        assertTrue(mzIdentMl.getAnalysisSoftwareList().getAnalysisSoftware().size() == analysisSoftwareCount);
        assertTrue(mzIdentMl.getProvider().getId().equals("PROVIDER"));
        assertTrue(auditCount >= 0);
        assertTrue(mzIdentMl.getAuditCollection().getPersonOrOrganization().size() == auditCount);
        assertTrue(dbSequenceCount >= 0);
        assertTrue(mzIdentMl.getSequenceCollection().getDBSequence().size() == dbSequenceCount);
        assertTrue(peptideEvidencePeptideCount >= 0);
        assertTrue(mzIdentMl.getSequenceCollection().getPeptideEvidence().size()
                == peptideEvidencePeptideCount);
    }

    @Test(expected= IllegalArgumentException.class)
    public void testMarshallEmptyAuditCollection() throws Exception{
        URL xmlFileURL = MzIdentMLMarshallerTest.class.getClassLoader().getResource("Mascot_MSMS_example.mzid");
        assertNotNull(xmlFileURL);
        MzIdentMLUnmarshaller unmarshaller = new MzIdentMLUnmarshaller(xmlFileURL);
        assertNotNull(unmarshaller);
        MzIdentMLMarshaller m = new MzIdentMLMarshaller();
        AuditCollection auditCollection = unmarshaller.unmarshal(MzIdentMLElement.AuditCollection.getXpath());
        auditCollection.getPersonOrOrganization().clear();
        m.marshal(auditCollection);
    }


    @Test(expected= IllegalArgumentException.class)
    public void testMarshallEmptyCvList() throws Exception{
        URL xmlFileURL = MzIdentMLMarshallerTest.class.getClassLoader().getResource("Mascot_MSMS_example.mzid");
        assertNotNull(xmlFileURL);
        MzIdentMLUnmarshaller unmarshaller = new MzIdentMLUnmarshaller(xmlFileURL);
        assertNotNull(unmarshaller);
        MzIdentMLMarshaller m = new MzIdentMLMarshaller();
        CvList cvList = unmarshaller.unmarshal(MzIdentMLElement.CvList.getXpath());
        cvList.getCv().clear();
        m.marshal(cvList);
    }


}
