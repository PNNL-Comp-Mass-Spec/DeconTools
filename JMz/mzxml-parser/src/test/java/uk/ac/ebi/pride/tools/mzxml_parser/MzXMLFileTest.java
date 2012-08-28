package uk.ac.ebi.pride.tools.mzxml_parser;

import java.io.File;
import java.net.URL;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

import uk.ac.ebi.pride.tools.jmzreader.model.IndexElement;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.DataProcessing;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.MsInstrument;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.ParentFile;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.Scan;

import junit.framework.TestCase;

public class MzXMLFileTest extends TestCase {
	private static MzXMLFile mzxmlFile;
	private File sourcefile;

	protected void setUp() throws Exception {
		super.setUp();
		
		 // create the mzxml dao
        try {
            URL testFile = getClass().getClassLoader().getResource("testfile.mzXML");
            assertNotNull("Error loading mzXML test file", testFile);
            sourcefile = new File(testFile.toURI());
            
            if (mzxmlFile == null)
            	mzxmlFile = new MzXMLFile(sourcefile);
        } catch (Exception ex) {
            fail(ex.getMessage());
        }
	}

	public void testGetParentFile() {
		try {
			List<ParentFile> parentFiles = mzxmlFile.getParentFile();
			
			assertEquals(1, parentFiles.size());
			
			ParentFile file = parentFiles.get(0);
			assertNotNull(file);
			
			assertEquals("R1_RG59_B4_1.RAW", file.getFileName());
			assertEquals("RAWData", file.getFileType());
		}
		catch (Exception e) {
			e.printStackTrace();
			fail("Failed to get parent file.");
		}
	}

	public void testGetMsInstrument() {
		try {
			List<MsInstrument> instruments = mzxmlFile.getMsInstrument();
			
			assertNotNull(instruments);
			assertEquals(1, instruments.size());
			
			MsInstrument instrument = instruments.get(0);
			assertNotNull(instrument);
			assertNotNull(instrument.getMsDetector());
			assertEquals("unknown", instrument.getMsDetector().getTheValue());
			assertEquals("FTMS", instrument.getMsMassAnalyzer().getTheValue());
			
			assertNull(instrument.getMsInstrumentID());
		}
		catch (Exception e) {
			e.printStackTrace();
			fail("Failed to get parent file.");
		}
	}

	public void testGetDataProcessing() {
		try {
			List<DataProcessing> processings = mzxmlFile.getDataProcessing();
			
			assertEquals(1, processings.size());
			
			DataProcessing p = processings.get(0);
			assertNotNull(p);
			assertEquals("ReAdW", p.getSoftware().getName());
			assertNull(p.getIntensityCutoff());
			assertEquals(0, p.getProcessingOperationAndComment().size());
		}
		catch (Exception e) {
			e.printStackTrace();
			fail("Failed to get parent file.");
		}
	}

	public void testGetSpearation() {
		try {
			assertNull(mzxmlFile.getSpearation());
		}
		catch (Exception e) {
			e.printStackTrace();
			fail("Failed to get parent file.");
		}
	}

	public void testGetSpotting() {
		try {
			assertNull(mzxmlFile.getSpotting());
		}
		catch (Exception e) {
			e.printStackTrace();
			fail("Failed to get parent file.");
		}
	}

	public void testGetRunAttributes() {
		assertEquals("{startTime=PT480.065S, endTime=PT6598.78S, scanCount=9181}", mzxmlFile.getRunAttributes().toString());
	}

	public void testGetScanCount() {
		assertEquals(9181, mzxmlFile.getMS1ScanCount() + mzxmlFile.getMS2ScanCount());
	}

	public void testGetScanIterator() {
		Iterable<Scan> scans = mzxmlFile.geMS1ScanIterator();
		
		int scanCount = 0;
		
		for (Scan s : scans) {
			scanCount++;
			assertNotNull(s);
			
			assertEquals(new Long(1), s.getMsLevel());
		}
		
		scans = mzxmlFile.getMS2ScanIterator();
		
		for (Scan s : scans) {
			scanCount++;
			assertNotNull(s);
			
			assertEquals(new Long(2), s.getMsLevel());
		}
		
		assertEquals(9181, scanCount);
	}
	
	public void testGetSpectrumIterator() {
		Iterator<Spectrum> it = mzxmlFile.getSpectrumIterator();
		
		int count = 0;
		
		while (it.hasNext()) {
			Spectrum s = it.next();
			
			assertNotNull(s);
			count++;
		}
		
		assertEquals(9181, count);
	}

	public void testConvertPeaksToMap() {
		Iterator<Scan> scans = mzxmlFile.getScanIterator();
		
		// just test the first scan
		Scan scan = scans.next();
		
		try {
			assertEquals(922.5859985351562, MzXMLFile.convertPeaksToMap(scan.getPeaks().get(0)).get(1186.860595703125));
		} catch (MzXMLParsingException e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}
	
	public void testGetScanNumbers() {
		List<Long> scanNumbers = mzxmlFile.getScanNumbers();
		
		assertNotNull(scanNumbers);
		assertEquals(9181, scanNumbers.size());
	}
	
	public void testGetScanByNum() {
		try {
			Scan scan = mzxmlFile.getScanByNum((long) 2011);
			
			assertNotNull(scan);
			assertEquals(new Long(2), scan.getMsLevel());
			assertEquals(new Long(210), scan.getPeaksCount());
			assertEquals("PT2160.4S", scan.getRetentionTime().toString());
			assertEquals(new Float(6151.1), scan.getTotIonCurrent());
			
			assertEquals(1, scan.getPeaks().size());
			Map<Double, Double> peakList = MzXMLFile.convertPeaksToMap(scan.getPeaks().get(0));
			
			assertEquals(210, peakList.size());
		} catch (MzXMLParsingException e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}
	
	public void testGetIndexedSpectrum() {
		List<IndexElement> index = mzxmlFile.getMsNIndexes(2);
		try {
			//Spectrum s1 = mzxmlFile.getSpectrumByIndex(14);
			Spectrum s2 = MzXMLFile.getIndexedSpectrum(sourcefile, index.get(9));
			
			assertNotNull(s2);
		}
		catch (Exception e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}
}
