package uk.ac.ebi.pride.tools.ms2_parser;

import java.io.File;
import java.net.URISyntaxException;
import java.net.URL;
import java.util.Iterator;

import junit.framework.TestCase;
import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import uk.ac.ebi.pride.tools.ms2_parser.model.Ms2Spectrum;

public class Ms2FileTest extends TestCase {
	Ms2File ms2File;

	protected void setUp() throws Exception {
		URL testFile = getClass().getClassLoader().getResource("test.ms2");
        assertNotNull("Error loading test directory", testFile);
        File sourceFile;
        
		try {
			sourceFile = new File(testFile.toURI());
			
			ms2File = new Ms2File(sourceFile);
		} catch (URISyntaxException e) {
			fail("Faild to load test file");
		}
	}

	public void testMs2File() {
		// check that the header was parsed correctly
		assertEquals(14, ms2File.getHeader().size());
		assertEquals("700", ms2File.getHeader().get("MinimumMass"));
		assertEquals("RawXtract written by John Venable, 2003", ms2File.getHeader().get("Comments"));
		
		assertNotNull(ms2File.getCreationDate());
		assertNotNull(ms2File.getExtractor());
		assertNotNull(ms2File.getExtractorVersion());
		assertNotNull(ms2File.getExtractorOptions());
		
		assertEquals(16421, ms2File.getSpectraCount());
	}
	
	public void testGetSpectrum() {
		Ms2Spectrum s;
		try {
			s = ms2File.getSpectrum(5);
			
			assertEquals(492.41000, s.getPrecursorMZ());
			assertEquals("0.33", s.getAdditionalInformation().get("RetTime"));
			assertEquals("hupo_06_itms.ms1", s.getAdditionalInformation().get("PrecursorFile"));
			
			assertEquals(3, s.getCharges().size());
			assertEquals(1966.61652, s.getCharges().get(4));
			
			assertEquals(0, s.getChargeDependentData().size());
			
			assertEquals(35, s.getPeakList().size());
			assertEquals(2.2, s.getPeakList().get(336.9019));
		} catch (JMzReaderException e) {
			fail(e.getMessage());
		}
	}
	
	public void testGetSpectrumIterator() {
		Iterator<Ms2Spectrum> it;
		try {
			it = ms2File.getMs2SpectrumIterator();
			
			int nSpecCount = 0;
			
			while (it.hasNext()) {
				assertNotNull(it.next());
				
				nSpecCount++;
			}
			
			assertEquals(16421, nSpecCount);
		} catch (JMzReaderException e) {
			fail(e.getMessage());
		}
	}
}
