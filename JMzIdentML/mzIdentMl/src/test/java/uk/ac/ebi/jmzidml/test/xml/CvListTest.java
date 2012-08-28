package uk.ac.ebi.jmzidml.test.xml;

import junit.framework.TestCase;
import org.apache.log4j.Logger;
import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.Cv;
import uk.ac.ebi.jmzidml.model.mzidml.CvList;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLUnmarshaller;

import java.net.URL;
import java.util.List;

/**
 * Package  : uk.ac.ebi.jmzidml.test.xml
 * Author   : riteshk
 * Date     : Sep 18, 2010
 */

public class CvListTest extends TestCase {

    Logger log = Logger.getLogger(CvListTest.class);


    public void testCvListInformation() throws Exception {

        URL xmlFileURL = CvListTest.class.getClassLoader().getResource("Mascot_MSMS_example.mzid");
        assertNotNull(xmlFileURL);

        MzIdentMLUnmarshaller unmarshaller = new MzIdentMLUnmarshaller(xmlFileURL);
        assertNotNull(unmarshaller);

        log.debug("Checking CvList element.");
        // Number of providers
        int totalCv = unmarshaller.getObjectCountForXpath(MzIdentMLElement.CvList.getXpath());
        assertEquals(1,totalCv);

        CvList cvList = unmarshaller.unmarshal(CvList.class);
        assertNotNull(cvList);
        List<Cv> cvs = cvList.getCv();
        assertNotNull(cvs);
        assertEquals(3, cvs.size());

        for (Cv cv : cvs) {
            assertTrue(cv.getUri().endsWith(".obo"));
        }
    }

}
