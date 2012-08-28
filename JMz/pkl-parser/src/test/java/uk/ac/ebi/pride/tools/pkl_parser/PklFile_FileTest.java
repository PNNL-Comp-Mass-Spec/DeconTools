package uk.ac.ebi.pride.tools.pkl_parser;

import java.io.File;
import java.net.URISyntaxException;
import java.net.URL;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

import junit.framework.TestCase;
import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import uk.ac.ebi.pride.tools.jmzreader.model.IndexElement;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.pkl_parser.model.PklSpectrum;

public class PklFile_FileTest extends TestCase {
	private PklFile pklFile;
	private File sourceFile;
	private Map<Integer, IndexElement> fileIndex;

	protected void setUp() throws Exception {
		URL testFile = getClass().getClassLoader().getResource("testfile.pkl");
        assertNotNull("Error loading test file", testFile);
        
		try {
			sourceFile = new File(testFile.toURI());
			
			pklFile = new PklFile(sourceFile);
		} catch (URISyntaxException e) {
			fail("Faild to load test file");
		}
	}

	public void testGetSpectraCount() {
		assertEquals(3, pklFile.getSpectraCount());
	}

	public void testGetSpectrum() {			
		PklSpectrum s;
		try {
			s = pklFile.getSpectrum(2);
		
			assertNotNull(s);
			assertEquals(11370039.0, s.getObservedIntensity());
			assertEquals(940.12, s.getObservedMZ());
			assertEquals(new Integer(2), s.getPrecursorCharge());
			
			assertEquals(491, s.getPeakList().size());
			
			assertEquals(533.4, s.getPeakList().get(212.87));
		
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
		
		assertEquals(3, nSpecCount);
	}
	
	public void testGetFileIndex() {
		fileIndex = pklFile.getFileIndex();
		
		assertEquals(3, fileIndex.size());
	}

	public void testPklFile() {
		fileIndex = pklFile.getFileIndex();
		
		PklFile newPklFile;
		try {
			newPklFile = new PklFile(sourceFile, fileIndex);
		
			// make sure the two files return the same results
			Iterator<PklSpectrum> it = pklFile.getPklSpectrumIterator();
			Iterator<PklSpectrum> it2 = newPklFile.getPklSpectrumIterator();
			
			while (it.hasNext() && it2.hasNext()) {
				assertEquals(it.next().toString(), it2.next().toString());
			}
		} catch (JMzReaderException e) {
			fail(e.getMessage());
		}
	}
	
	public void testGetSpectraIds() {
		List<String> ids = pklFile.getSpectraIds();
		
		assertEquals(3, ids.size());
		
		for (Integer i = 1; i <= 3; i++)
			assertEquals(i.toString(), ids.get(i - 1));
	}
	
	public void testGetIndexedSpectrum() {
		List<IndexElement> index = pklFile.getMsNIndexes(2);
		
		try {
			Spectrum s1 = pklFile.getSpectrumByIndex(2);
			Spectrum s2 = PklFile.getIndexedSpectrum(sourceFile, index.get(1));
			
			assertEquals(s1.getPeakList(), s2.getPeakList());
		} catch (JMzReaderException e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}
}
