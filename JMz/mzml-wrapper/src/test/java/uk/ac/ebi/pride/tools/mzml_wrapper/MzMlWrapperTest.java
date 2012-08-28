package uk.ac.ebi.pride.tools.mzml_wrapper;

import java.io.File;
import java.net.URL;
import java.util.Iterator;

import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;



import junit.framework.TestCase;

public class MzMlWrapperTest extends TestCase {
	private static MzMlWrapper wrapper;
	private static File sourcefile;

	protected void setUp() throws Exception {
		super.setUp();
		
		if (sourcefile != null)
			return;
		
		URL testFile = getClass().getClassLoader().getResource("dta_example.mzML");
        assertNotNull("Error loading mzData test file", testFile);
        
		try {
			sourcefile = new File(testFile.toURI());
			
			assertNotNull(sourcefile);
			
			wrapper = new MzMlWrapper(sourcefile);
			
			assertNotNull(wrapper);
		} catch (Exception e) {
			fail("Faild to load test file");
		}
	}
	
	public void testSpectraLoading() {
		assertEquals(10, wrapper.getSpectraCount());
		assertTrue(wrapper.acceptsFile());
		assertFalse(wrapper.acceptsDirectory());
		
		// iterate over all 10 spectra
		Iterator<Spectrum> it = wrapper.getSpectrumIterator();
		
		try {
			int count = 0;
			
			while (it.hasNext()) {
				Spectrum s = it.next();
				
				assertNotNull(s);
				
				count++;
			}
			
			assertEquals(10, count);
		} catch(Exception e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}

	public void testGetSpecByIndex() {
		try {
			Spectrum s = wrapper.getSpectrumByIndex(1);
			assertEquals("scan=3", s.getId());
			assertEquals(1, s.getPrecursorCharge().intValue());
			assertEquals(419.115, s.getPrecursorMZ());
			
			assertEquals(92, s.getPeakList().size());
			
			assertEquals(5876118.0, s.getPeakList().get(419.0830078125));
			
		} catch(Exception e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}
	
	public void testGetSpecById() {
		try {
			Spectrum s = wrapper.getSpectrumById("scan=3");
			assertEquals("scan=3", s.getId());
			assertEquals(1, s.getPrecursorCharge().intValue());
			assertEquals(419.115, s.getPrecursorMZ());
			
			assertEquals(92, s.getPeakList().size());
			
			assertEquals(5876118.0, s.getPeakList().get(419.0830078125));
			
		} catch(Exception e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}
}
