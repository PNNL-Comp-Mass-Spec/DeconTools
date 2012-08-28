package uk.ac.ebi.jmzidml.test.xml;

import org.apache.log4j.Logger;
import org.junit.Test;
import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftware;
import uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftwareList;
import uk.ac.ebi.jmzidml.model.mzidml.CvParam;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLUnmarshaller;

import java.net.URL;
import java.util.Iterator;

import static junit.framework.Assert.*;

/**
 * Package  : uk.ac.ebi.jmzidml.test.xml
 * User: riteshk
 * Date: Sep 18, 2010
 */
public class AnalysisSoftwareTest {

    private static final Logger log = Logger.getLogger(AnalysisSoftwareTest.class);

    @Test
    public void testAnalysisSoftwareInformation() throws Exception {

        URL xmlFileURL = AnalysisSoftwareTest.class.getClassLoader().getResource("Mascot_MSMS_example.mzid");
        assertNotNull(xmlFileURL);

//        MzIdentMLObjectCache cache = new AdapterObjectCache();
//        MzIdentMLUnmarshaller unmarshaller = new MzIdentMLUnmarshaller(xmlFileURL, cache);
        MzIdentMLUnmarshaller unmarshaller = new MzIdentMLUnmarshaller(xmlFileURL);
        assertNotNull(unmarshaller);

        // Number of Analysis Software
        int totalAnalysisSoftware = unmarshaller.getObjectCountForXpath(MzIdentMLElement.AnalysisSoftwareList.getXpath());
        assertEquals(1,totalAnalysisSoftware);
        AnalysisSoftwareList list = unmarshaller.unmarshal(MzIdentMLElement.AnalysisSoftwareList);
        assertNotNull(list);

        Iterator<AnalysisSoftware> asl = unmarshaller.unmarshalCollectionFromXpath(MzIdentMLElement.AnalysisSoftware);
        assertNotNull(asl);
        assertTrue(asl.hasNext());

        while(asl.hasNext()){
            AnalysisSoftware as = asl.next();
            assertNotNull(as);
            log.debug("\n Analysis Software -> Name : " + as.getName() + " \t Software : "
                    + as.getCustomizations() + "\t URI" + as.getUri());
            assertTrue("Analysis software is from Mascot.", as.getName().contains("Mascot"));
            if (MzIdentMLElement.ContactRole.isAutoRefResolving() && as.getContactRole().getContactRef() != null) {
                assertNotNull(as.getContactRole().getContact());
                log.debug("\n Analysis Software -> ContactRole -> Name  :" + as.getContactRole().getContact().getName());
                /**
                 * Testing that only organization is returned (in this test file analysis software instances only
                 * have Organization ContactRoles.
                 */
                assertNotNull(as.getContactRole().getOrganization());
                assertNull(as.getContactRole().getPerson());
            } else {
                System.out.println("ContactRole is not auto-resolving or does not contain a ContactRole reference.");
                assertNull(as.getContactRole().getContact());
            }

            assertTrue("Analysis software name contains Mascot.", as.getSoftwareName().getCvParam().getName().contains("Mascot"));

//            assertNull("We don't expect a UserParam in the SoftwareName tag!", as.getSoftwareName().getParamGroup().getUserParam());
            CvParam cvParam = (CvParam)as.getSoftwareName().getCvParam();
            assertNotNull(cvParam.getAccession());

        }

        // check the same in another way
        AnalysisSoftwareList asList = unmarshaller.unmarshal(AnalysisSoftwareList.class);
        assertNotNull(asList);
        for (AnalysisSoftware as : asList.getAnalysisSoftware()) {
            // same tests as above have to be true
            assertNotNull(as);
            assertTrue("Analysis software is from Mascot.", as.getName().contains("Mascot"));
            assertTrue("Analysis software name contains Mascot.", as.getSoftwareName().getCvParam().getName().contains("Mascot"));
//            assertNull("We don't expect a UserParam in the SoftwareName tag!", as.getSoftwareName().getUserParam());
            CvParam cvParam = (CvParam)as.getSoftwareName().getCvParam();
            assertNotNull(cvParam.getAccession());
        }

    }

}
