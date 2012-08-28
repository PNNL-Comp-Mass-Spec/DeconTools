package uk.ac.ebi.pride.tools.mzdata_parser;

import java.io.File;
import java.net.URL;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

import junit.framework.TestCase;
import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import uk.ac.ebi.pride.tools.jmzreader.model.IndexElement;
import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.CvLookup;
import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.MzData.Description;
import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum;

public class MzDataFileTest extends TestCase {
	private static File sourcefile;
	private static MzDataFile mzDataFile;

	protected void setUp() throws Exception {
		super.setUp();
		
		if (mzDataFile != null)
			return;
		
		URL testFile = getClass().getClassLoader().getResource("PRIDE_Exp_mzData_Ac_8869.xml");
        assertNotNull("Error loading mzData test file", testFile);
        
		try {
			sourcefile = new File(testFile.toURI());
			
			assertNotNull(sourcefile);
			
			mzDataFile = new MzDataFile(sourcefile);
			
			assertNotNull(mzDataFile);
		} catch (Exception e) {
			fail("Faild to load test file");
		}
	}

	public void testGetMzDataAttributes() {
		Map<String, String> attr = mzDataFile.getMzDataAttributes();
		
		assertNotNull(attr);
		assertEquals("1.05", attr.get("version"));
		assertEquals("8869", attr.get("accessionNumber"));
	}

	public void testGetCvLookups() {
		List<CvLookup> cvs;
		try {
			cvs = mzDataFile.getCvLookups();
			
			assertNotNull(cvs);
			assertEquals(6, cvs.size());
			assertEquals("BRENDA tissue / enzyme source", cvs.get(0).getFullName());
			assertEquals("http://psidev.sourceforge.net/ontology/", cvs.get(5).getAddress());
		} catch (JMzReaderException e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}

	public void testGetDescription() {
		try {
			Description description = mzDataFile.getDescription();
			
			assertNotNull(description);
			assertEquals("MS-060322ng_DC-LPS-CYT", description.getAdmin().getSampleName());
			assertEquals(4, description.getAdmin().getSampleDescription().getCvParams().size());
			assertEquals("GO:0005737", description.getAdmin().getSampleDescription().getCvParams().get(2).getAccession());
			
			assertEquals(1, description.getAdmin().getContact().size());
			assertEquals("Christopher Gerner", description.getAdmin().getContact().get(0).getName());
			assertEquals("XCT Ultra, Agilent", description.getInstrument().getInstrumentName());
			assertEquals(3, description.getInstrument().getSource().getCvParams().size());
			assertEquals(1, description.getInstrument().getAnalyzerList().getAnalyzer().size());
			
			assertEquals("A.03.03", description.getDataProcessing().getSoftware().getVersion());
		} catch (JMzReaderException e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}

	public void testGetSpectraCount() {
		assertEquals(2139, mzDataFile.getSpectraCount());
	}

	public void testGetSpectraIds() {
		List<String> ids = mzDataFile.getSpectraIds();
		
		assertEquals(2139, ids.size());
		
		for (Integer i = 1; i <= 2139; i++)
			assertEquals(i.toString(), ids.get(i - 1));
	}

	public void testGetMzDataSpectrumById() {
		try {
			Spectrum s = mzDataFile.getMzDataSpectrumById(3);
			
			assertNotNull(s);
			assertEquals(336 * 4, s.getIntenArrayBinary().getData().getValue().length);
			assertEquals(new Float(1800.0), s.getSpectrumDesc().getSpectrumSettings().getSpectrumInstrument().getMzRangeStop());
		} catch (JMzReaderException e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}

	public void testGetSpectrumById() {
		try {
			uk.ac.ebi.pride.tools.jmzreader.model.Spectrum s = mzDataFile.getSpectrumById("3");			
			
			assertNotNull(s);
			assertEquals(336, s.getPeakList().size());
			assertNull(s.getPrecursorCharge());
			assertNull(s.getPrecursorIntensity());
			assertNull(s.getPrecursorMZ());
		} catch (JMzReaderException e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}

	public void testGetMzDataSpectrumByIndex() {
		try {
			Spectrum s = mzDataFile.getMzDataSpectrumByIndex(3);
			
			assertNotNull(s);
			assertEquals(336 * 4, s.getIntenArrayBinary().getData().getValue().length);
			assertEquals(new Float(1800.0), s.getSpectrumDesc().getSpectrumSettings().getSpectrumInstrument().getMzRangeStop());
		} catch (JMzReaderException e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}

	public void testGetSpectrumByIndex() {
		try {
			uk.ac.ebi.pride.tools.jmzreader.model.Spectrum s = mzDataFile.getSpectrumByIndex(3);			
			
			assertNotNull(s);
			assertEquals(336, s.getPeakList().size());
			assertNull(s.getPrecursorCharge());
			assertNull(s.getPrecursorIntensity());
			assertNull(s.getPrecursorMZ());
		} catch (JMzReaderException e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}

	public void testGetSpectrumIterator() {
		Iterator<uk.ac.ebi.pride.tools.jmzreader.model.Spectrum> it = mzDataFile.getSpectrumIterator();
		
		int count = 0;
		
		while (it.hasNext()) {
			uk.ac.ebi.pride.tools.jmzreader.model.Spectrum s = it.next();
			
			assertNotNull(s);
			
			assertEquals("" + (count + 1), s.getId());
			count++;
		}
		
		assertEquals(2139, count);
	}

	public void testGetMzDataSpectrumIterator() {
		Iterator<Spectrum> it = mzDataFile.getMzDataSpectrumIterator();
		
		int count = 0;
		
		while (it.hasNext()) {
			Spectrum s = it.next();
			
			assertNotNull(s);
			
			assertEquals(count + 1, s.getId());
			
			count++;
		}
		
		Iterator<uk.ac.ebi.pride.tools.jmzreader.model.Spectrum> it2 = mzDataFile.getSpectrumIterator();
		
		while (it.hasNext()) {
			uk.ac.ebi.pride.tools.jmzreader.model.Spectrum s = it2.next();
			
			assertNotNull(s);
			
			assertEquals(2, s.getMsLevel().intValue());
		}
		
		assertEquals(2139, count);
	}

	public void testGetIndexedSpectrum() {
		try {
			List<IndexElement> index = mzDataFile.getMsNIndexes(2);
			
			assertNotNull(index);
			
			uk.ac.ebi.pride.tools.jmzreader.model.Spectrum s1 = mzDataFile.getSpectrumByIndex(3);
			uk.ac.ebi.pride.tools.jmzreader.model.Spectrum s2 = MzDataFile.getIndexedSpectrum(sourcefile, index.get(2));
			
			assertEquals(s1.getPeakList(), s2.getPeakList());
			assertEquals(s1.getPrecursorMZ(), s2.getPrecursorMZ());
		}
		catch (Exception e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}
}
