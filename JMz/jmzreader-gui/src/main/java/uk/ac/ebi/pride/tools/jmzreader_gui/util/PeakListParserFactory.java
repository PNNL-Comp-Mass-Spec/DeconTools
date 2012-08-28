package uk.ac.ebi.pride.tools.jmzreader_gui.util;

import java.io.File;
import java.io.FilenameFilter;
import java.io.IOException;
import java.io.RandomAccessFile;

import uk.ac.ebi.pride.tools.dta_parser.DtaFile;
import uk.ac.ebi.pride.tools.jmzreader.JMzReader;
import uk.ac.ebi.pride.tools.mgf_parser.MgfFile;
import uk.ac.ebi.pride.tools.ms2_parser.Ms2File;
import uk.ac.ebi.pride.tools.mzdata_parser.MzDataFile;
import uk.ac.ebi.pride.tools.mzml_wrapper.MzMlWrapper;
import uk.ac.ebi.pride.tools.mzxml_parser.MzXMLFile;
import uk.ac.ebi.pride.tools.pkl_parser.PklFile;
import uk.ac.ebi.pride.tools.pride_wrapper.PRIDEXmlWrapper;

public class PeakListParserFactory {
	private static PeakListParserFactory instance;
	
	/**
	 * Can only be created once.
	 */
	private PeakListParserFactory() {
		
	}
	
	public static PeakListParserFactory getInstance() {
		if (instance == null)
			instance = new PeakListParserFactory();
		
		return instance;
	}
	
	/**
	 * Returns a peak list parser for the passed
	 * File. In case the parser cannot be determined
	 * null is returend.
	 * @param inputfile
	 * @return A JMzParser or null in case the filetype cannot be determined.
	 */
	public JMzReader getParser(File inputfile) throws Exception {
		PeakListFormat format = null;
		
		// if it's a file get the format using the PeakListFormat enum
		if (inputfile.isFile()) {
			format = PeakListFormat.getFormat(inputfile.getName());
			}
		else {
			// check what kind of files are in the directory
			String[] files = inputfile.list(new FilenameFilter() {
				
				public boolean accept(File dir, String name) {
					PeakListFormat format = PeakListFormat.getFormat(name);
					
					return (format == PeakListFormat.DTA || format == PeakListFormat.PKL);
				}
			});
			
			// make sure there were files found
			if (files == null || files.length == 0)
				return null;
			
			// check which type it is
			format = PeakListFormat.getFormat(files[0]);
		}
		
		// if the format is null, return null
		if (format == null)
			return null;
		
		try {
			switch (format) {
				case DTA:
					return new DtaFile(inputfile);
				case MGF:
					return new MgfFile(inputfile);
				case MS2:
					return new Ms2File(inputfile);
				case MZXML:
					return new MzXMLFile(inputfile);
				case PKL:
					return new PklFile(inputfile);
				case XML_FILE:
					if (isPrideXML(inputfile))
						return new PRIDEXmlWrapper(inputfile);
					if (isMzData(inputfile))
						return new MzDataFile(inputfile);
					if (isMzXml(inputfile))
						return new MzXMLFile(inputfile);
					
					throw new Exception("Unsupported XML file format. Supported formats are PRIDE XML, mzXML and mzData.");
				case MZML:
					return new MzMlWrapper(inputfile);
				default:
					return null;
			}
		}
		catch (Exception e) {
			throw e;
		}
	}
	
	private static boolean isPrideXML(File file) throws Exception {
		RandomAccessFile access = new RandomAccessFile(file, "r");
		
		// read the first 500 bytes
		byte[] buffer = new byte[500];
		
		access.read(buffer);
		
		access.close();
		
		String header = new String(buffer);
		
		if (header.contains("<ExperimentCollection version=\""))
			return true;
		else
			return false;
	}
	
	private static boolean isMzData(File file) throws Exception {
		RandomAccessFile access = new RandomAccessFile(file, "r");
		
		// read the first 500 bytes
		byte[] buffer = new byte[500];
		
		access.read(buffer);
		
		access.close();
		
		String header = new String(buffer);
		
		if (header.contains("<mzData version=\"1.05\""))
			return true;
		else
			return false;
	}
	
	private static boolean isMzXml(File file) throws Exception {
		RandomAccessFile access = new RandomAccessFile(file, "r");
		
		// read the first 500 bytes
		byte[] buffer = new byte[500];
		
		access.read(buffer);
		
		access.close();
		
		String header = new String(buffer);
		
		if (header.contains("<mzXML"))
			return true;
		else
			return false;
	}
}
