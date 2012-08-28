package uk.ac.ebi.pride.tools.braf;

import java.io.File;
import java.net.URL;

import junit.framework.TestCase;

public class BrafTest extends TestCase {
	private File sourcefile;
	private File smallFile;
	private final int BUF_SIZE = 1024 * 1000;
	
	protected void setUp() throws Exception {
		URL testFile = getClass().getClassLoader().getResource("41390_MERGED.mgf");
		URL smallFileUrl = getClass().getClassLoader().getResource("testFile");
        assertNotNull("Error loading mgf test file", testFile);
        
		try {
			sourcefile = new File(testFile.toURI());	
			smallFile = new File(smallFileUrl.toURI());
			
			assertTrue(sourcefile.exists());
			assertTrue(smallFile.exists());
		} catch (Exception e) {
			fail("Faild to load test file");
		}
	}

	public void testFirstSteps() {
		try {
			BufferedRandomAccessFile reader = new BufferedRandomAccessFile(sourcefile, "r", BUF_SIZE);
			
			String line;
			int lineCount = 0;
			
			while ((line = reader.readLine()) != null) {
//				// do something
//				System.out.println(reader.getFilePointer() + ": " + line.trim());
//				
//				if (lineCount > 10)
//					break;
				
				if (lineCount %1000000 == 0) {
					System.out.println(((Runtime.getRuntime().totalMemory() - Runtime.getRuntime().freeMemory()) / (1024*1024)) + " MB used");
				}
				
				lineCount++;
			}
		} catch (Exception e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}
	
	public void testSmallFile() {
		int lineNumber = 0;
		
		try {
			BufferedRandomAccessFile reader = new BufferedRandomAccessFile(smallFile, "r", BUF_SIZE);
			
			String line;
			long position = 0L;
			
			assertEquals(position, reader.getFilePointer());
			
			while ((line = reader.readLine()) != null) {
				if (lineNumber == 0)
					assertEquals("Line 1", line);
				if (lineNumber == 1)
					assertEquals("Line 2", line);
				if (lineNumber == 2)
					assertEquals("This is before the last line", line);
				if (lineNumber == 3)
					assertEquals("The last line doesn't end with \"\\n\"", line);
				
				lineNumber++;
				
				position += line.length() + 1; // add 1 for the \n character
				
				// the last line doesn't have an "\n"
				if (position == 79)
					position--;
				assertEquals(position, reader.getFilePointer());
			}
		} catch (Exception e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}
	
	public void testSeek() {		
		try {
			BufferedRandomAccessFile reader = new BufferedRandomAccessFile(smallFile, "r", BUF_SIZE);
			
			reader.seek(9);
			
			String line = reader.readLine();
			
			assertEquals("ne 2", line);
			
			reader.seek(8);
			
			line = reader.readLine();
			
			assertEquals("ine 2", line);
		} catch (Exception e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}
}

