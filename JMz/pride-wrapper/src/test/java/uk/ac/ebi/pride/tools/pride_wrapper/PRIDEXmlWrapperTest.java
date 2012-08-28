package uk.ac.ebi.pride.tools.pride_wrapper;

import junit.framework.TestCase;
import uk.ac.ebi.pride.tools.jmzreader.model.IndexElement;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.CvParam;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.ParamGroup;

import java.io.File;
import java.net.URL;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

/**
 * @author Rui Wang
 * @version $Id$
 */
public class PRIDEXmlWrapperTest extends TestCase {

    private PRIDEXmlWrapper wrapper;

    public void setUp() throws Exception {
        URL url = PRIDEXmlWrapperTest.class.getClassLoader().getResource("test-pride.xml");
        if (url == null) {
            throw new IllegalStateException("no file for input found!");
        }
        File inputFile = new File(url.toURI());
        wrapper = new PRIDEXmlWrapper(inputFile);
    }

    public void tearDown() throws Exception {
        wrapper = null;
    }

    public void testGetSpectraCount() throws Exception {
        assertTrue("The number of spectrum should be 100", wrapper.getSpectraCount() == 100);
    }

    public void testAcceptsFile() throws Exception {
        assertTrue("This wrapper should allow file", wrapper.acceptsFile());
    }

    public void testAcceptsDirectory() throws Exception {
        assertFalse("This wrapper should not allow folder", wrapper.acceptsDirectory());
    }

    public void testGetSpectraIds() throws Exception {
        List<String> ids = wrapper.getSpectraIds();
        // check size
        assertEquals(100, ids.size());
        // check existence
        assertTrue("This wrapper should contain spectrum", ids.contains("2"));
    }

    public void testGetSpectrumById() throws Exception {
        Spectrum spectrum = wrapper.getSpectrumById("2");
        // id
        assertEquals("2", spectrum.getId());
        // ms level
        assertEquals(0, spectrum.getMsLevel().intValue());
        // precursor charge
        assertEquals(2, spectrum.getPrecursorCharge().intValue());
        // precursor m/z
        assertEquals(898, spectrum.getPrecursorMZ().intValue());
        // precursor intensity
        assertNull(spectrum.getPrecursorIntensity());
        // param group
        ParamGroup paramGroup = spectrum.getAdditional();
        assertNotNull(paramGroup);
        // cv params
        List<CvParam> cvParams = paramGroup.getCvParams();
    }

    public void testGetSpectrumByIndex() throws Exception {
        Spectrum spectrum = wrapper.getSpectrumByIndex(1);

        // id
        assertEquals("2", spectrum.getId());
        // ms level
        assertEquals(0, spectrum.getMsLevel().intValue());
        // precursor charge
        assertEquals(2, spectrum.getPrecursorCharge().intValue());
        // precursor m/z
        assertEquals(898, spectrum.getPrecursorMZ().intValue());
        // precursor intensity
        assertNull(spectrum.getPrecursorIntensity());
        // param group
        ParamGroup paramGroup = spectrum.getAdditional();
        assertNotNull(paramGroup);
        // cv params
        List<CvParam> cvParams = paramGroup.getCvParams();
    }

    public void testGetSpectrumIterator() throws Exception {
        Iterator<Spectrum> iter = wrapper.getSpectrumIterator();

        assertNotNull(iter);

        iter.next();

        Spectrum spectrum = iter.next();

        // id
        assertEquals("2", spectrum.getId());
        // ms level
        assertEquals(0, spectrum.getMsLevel().intValue());
        // precursor charge
        assertEquals(2, spectrum.getPrecursorCharge().intValue());
        // precursor m/z
        assertEquals(898, spectrum.getPrecursorMZ().intValue());
        // precursor intensity
        assertNull(spectrum.getPrecursorIntensity());
        // param group
        ParamGroup paramGroup = spectrum.getAdditional();
        assertNotNull(paramGroup);
        // cv params
        List<CvParam> cvParams = paramGroup.getCvParams();
    }

    public void testGetMsNIndexes() throws Exception {
        List<IndexElement> indexElements = wrapper.getMsNIndexes(0);
        assertEquals(1, indexElements.size());
    }

    public void testGetMsLevels() throws Exception {
        List<Integer> msLevels = wrapper.getMsLevels();

        assertEquals(2, msLevels.size());

        assertTrue(msLevels.contains(2));

        assertTrue(msLevels.contains(0));
    }

    public void testGetIndexElementForIds() throws Exception {
        Map<String, IndexElement> indexElements = wrapper.getIndexElementForIds();
        assertNotNull(indexElements);
        assertNotNull(indexElements.get("2"));
    }
}
