package uk.ac.ebi.pride.tools.pkl_parser;

import java.io.File;
import java.net.URISyntaxException;
import java.net.URL;
import java.util.Iterator;
import java.util.List;

import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import uk.ac.ebi.pride.tools.pkl_parser.model.PklSpectrum;

import junit.framework.TestCase;

public class PklFile_DirTest extends TestCase {
	private PklFile pklFile;

	protected void setUp() throws Exception {
		URL testFile = getClass().getClassLoader().getResource("pkl_dir");
        assertNotNull("Error loading test file", testFile);
        File sourceFile;
        
		try {
			sourceFile = new File(testFile.toURI());
			
			pklFile = new PklFile(sourceFile);
		} catch (URISyntaxException e) {
			fail("Faild to load test file");
		}
	}

	public void testGetSpectraCount() {
		assertEquals(16, pklFile.getSpectraCount());
	}

	public void testGetSpectrum() {
		PklSpectrum s;
		try {
			s = pklFile.getSpectrum("110208A.1708.1708.0.pkl");
			
			assertNotNull(s);
			assertEquals(648.20, s.getObservedMZ());
			assertEquals(171079939.0, s.getObservedIntensity());
			
			assertEquals(399, s.getPeakList().size());
			assertEquals(19208.9, s.getPeakList().get(235.03));
		} catch (JMzReaderException e) {
			fail(e.getMessage());
		}
	}

	public void testGetSpectrumIterator() {
		Iterator<PklSpectrum> it = pklFile.getPklSpectrumIterator();
		
		int nSpecCount = 0;
		
		while (it.hasNext()) {
			assertNotNull(it.next());
			
			nSpecCount++;
		}
		
		assertEquals(16, nSpecCount);
	}
	
	public void testGetSpectraIds() {
		List<String> ids = pklFile.getSpectraIds();
		
		assertEquals(16, ids.size());
		assertTrue(ids.contains("110208A.1715.1715.2.pkl"));
	}

}
