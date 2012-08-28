package uk.ac.ebi.pride.tools.mgf_parser;

import java.io.File;
import java.net.URL;

import junit.framework.TestCase;

public class StrangeEolTest extends TestCase {
	private MgfFile mgfFile;
	private File sourceFile;

	protected void setUp() throws Exception {
		mgfFile = new MgfFile();
	}
	
	private void loadTestFile() {		
		URL testFile = getClass().getClassLoader().getResource("strange_eol.mgf");
        assertNotNull("Error loading mgf test file", testFile);
        
		try {
			sourceFile = new File(testFile.toURI());
			
			mgfFile = new MgfFile(sourceFile);
		} catch (Exception e) {
			e.printStackTrace();
			fail("Faild to load test file");
		}
	}

	public void testLoadFile() {
		loadTestFile();
		
		assertEquals(5, mgfFile.getMs2QueryCount());
	}
}
