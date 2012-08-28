package uk.ac.ebi.pride.tools.dta_parser;

import java.io.File;
import java.net.URISyntaxException;
import java.net.URL;
import java.util.Iterator;
import java.util.List;

import uk.ac.ebi.pride.tools.dta_parser.model.DtaSpectrum;
import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;

import junit.framework.TestCase;

public class DtaFile_DirectoryTest extends TestCase {
	private DtaFile dtaFile;

	protected void setUp() throws Exception {
		URL testFile = getClass().getClassLoader().getResource("QSTAR1a");
        assertNotNull("Error loading test directory", testFile);
        File sourceFile;
        
		try {
			sourceFile = new File(testFile.toURI());
			
			dtaFile = new DtaFile(sourceFile);
		} catch (URISyntaxException e) {
			fail("Faild to load test file");
		}
	}

	public void testGetSpectraCount() {
		assertEquals(10, dtaFile.getSpectraCount());
	}

	public void testGetSpectrum() {
		DtaSpectrum s;
		try {
			s = dtaFile.getDtaSpectrum("1a.280.2.0792.0792.2.dta");

			assertNotNull(s);
			assertEquals(559.3281, s.getMhMass());
			assertEquals(new Integer(2), s.getPrecursorCharge());
			assertEquals(26, s.getPeakList().size());
			assertEquals(2.0, s.getPeakList().get(276.1458));		
		} catch (JMzReaderException e) {
			e.printStackTrace();
		}
	}

	public void testGetSpectrumIterator() {
		Iterator<DtaSpectrum> it = dtaFile.getDtaSpectrumIterator();
		
		int nSpecCount = 0;
		
		while (it.hasNext()) {
			assertNotNull(it.next());
			
			nSpecCount++;
		}
		
		assertEquals(10, nSpecCount);
	}
	
	public void testGetSpectraIds() {
		List<String> ids = dtaFile.getSpectraIds();
		
		assertEquals(10, ids.size());
		assertTrue(ids.contains("1a.278.2.1902.1902.2.dta"));
	}

}
