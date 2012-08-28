package uk.ac.ebi.pride.tools.dta_parser;

import java.io.File;
import java.net.URISyntaxException;
import java.net.URL;
import java.util.Iterator;
import java.util.List;

import uk.ac.ebi.pride.tools.dta_parser.model.DtaSpectrum;
import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import junit.framework.TestCase;

public class DtaFile_FileTest extends TestCase {
	DtaFile dtaFile;

	protected void setUp() throws Exception {
		URL testFile = getClass().getClassLoader().getResource("QSTAR1a_concat");
        assertNotNull("Error loading test file", testFile);
        File sourceFile;
        
		try {
			sourceFile = new File(testFile.toURI());
			
			dtaFile = new DtaFile(sourceFile);
		} catch (URISyntaxException e) {
			fail("Faild to load test file");
		}
	}

	public void testGetSpectraCount() {
		assertEquals(389, dtaFile.getSpectraCount());
	}

	public void testGetSpectrum() {
		DtaSpectrum s;
		try {
			s = dtaFile.getDtaSpectrum(3);
		
			assertEquals(2010.9163, s.getMhMass());
			assertEquals(new Integer(2), s.getPrecursorCharge());
			
			assertEquals(51, s.getPeakList().size());
			assertEquals(3.0, s.getPeakList().get(462.2336));
		} catch (JMzReaderException e) {
			e.printStackTrace();
		}
	}

	public void testGetSpectrumIterator() {
		Iterator<DtaSpectrum> it = dtaFile.getDtaSpectrumIterator();
		
		int count = 0;
		
		while (it.hasNext()) {
			assertNotNull(it.next());
			count++;
		}
		
		assertEquals(389, count);
	}
	
	public void testGetSpectraIds() {
		List<String> ids = dtaFile.getSpectraIds();
		
		assertEquals(389, ids.size());
		
		for (Integer i = 0; i < 389; i++)
			assertEquals(i.toString(), ids.get(i));
	}

}
