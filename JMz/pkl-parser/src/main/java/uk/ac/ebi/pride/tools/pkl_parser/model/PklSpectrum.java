package uk.ac.ebi.pride.tools.pkl_parser.model;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.ParamGroup;

/**
 * Represents a PKL spectrum. This can either be a part of
 * a concatenated PKL file or a PKL file containing only a
 * single spectrum.
 * @author jg
 *
 */
public class PklSpectrum implements Spectrum {
	/**
	 * This variable is only set in case the object represents a dta file containing
	 * only one spectrum
	 */
	private File sourceFile;
	/**
	 * The peptide's charge
	 */
	private int charge;
	/**
	 * The observed m/z
	 */
	private Double observedMZ;
	/**
	 * The observed intensity
	 */
	private Double observedIntensity;
	/**
	 * The peak list with the m/z as key and
	 * their intensities as values.
	 */
	private Map<Double, Double> peakList;
	/**
	 * The 1-based index of the spectrum in the file.
	 */
	private Integer index;
	
	/**
	 * Creates a PKL spectrum object that's based
	 * on a specific source file. This sourcefile
	 * must only contain one spectrum.
	 * @param sourceFile
	 * @throws JMzReaderException 
	 * @throws IOException 
	 */
	public PklSpectrum(File sourceFile) throws JMzReaderException {
		// open the file just read it in a buffer
		String line = "";
		ArrayList<String> lines = new ArrayList<String>();
		
		// save the sourcefile
		this.sourceFile = sourceFile;
		
		try {
			BufferedReader reader = new BufferedReader(new InputStreamReader(new FileInputStream(sourceFile)));
			
			while ((line = reader.readLine()) != null) {
				lines.add(line);
			}
			
			// use the DtaSpectrum(String dtaFileSection) constructor to create the object
			parsePklSection(lines);
		}
		catch (IOException e) {
			throw new JMzReaderException("Failed to read from file.", e);
		}
	}
	
	/**
	 * Creates a PKL spectrum object based on a section
	 * of a concatenated PKL file. This string must only
	 * contain the information for one spectrum.
	 * @param pklFileSection
	 * @param index The 1-based index of the spectrum in the file.
	 * @throws JMzReaderException 
	 */
	public PklSpectrum(String pklFileSection, int index) throws JMzReaderException {
		// parse the string line by line
		String[] lines = pklFileSection.trim().split("\n");
		ArrayList<String> lineArray = new ArrayList<String>(lines.length);
		
		for (String line : lines)
			lineArray.add(line);
		
		this.index = index;
		
		// parse the section
		parsePklSection(lineArray);
	}
	
	/**
	 * Parses a PKL section (see constructor) in form of an
	 * Array of Strings = Lines.
	 * @param lines
	 * @throws JMzReaderException 
	 */
	private void parsePklSection(List<String> lines) throws JMzReaderException {
		// create the new peak list
		peakList = new HashMap<Double, Double>();
		
		// make sure there are lines to parse
		if (lines.size() < 1)
			throw new JMzReaderException("Invalid PKL section passed to generate PKL spectrum object. PKL section contains to few lines.");
		
		// the first line should always contain m/z, intensity and charge
		String[] headerFields = lines.get(0).split("\\s+");
		
		// there must only be two files
		if (headerFields.length != 3)
			throw new JMzReaderException("Invalid header section encountered in PKL spectrum.");
		
		// set the header fields
		observedMZ 			= Double.parseDouble(headerFields[0]);
		observedIntensity 	= Double.parseDouble(headerFields[1]);
		charge 				= Integer.parseInt(headerFields[2].replace(".0", "")); // remove a possible .0 (sometimes a software artefact)
		
		// process the peak list
		for (int nLineNumber = 1; nLineNumber < lines.size(); nLineNumber++) {
			String[] fields = lines.get(nLineNumber).split("\\s+");
			
			// make sure there are only two fields
			if (fields.length != 2)
				throw new JMzReaderException("Invalid line encountered in PKL spectrum");
			
			// save the peak
			peakList.put(Double.parseDouble(fields[0]), Double.parseDouble(fields[1]));
		}
	}
	
	public Double getObservedMZ() {
		return observedMZ;
	}

	public Double getObservedIntensity() {
		return observedIntensity;
	}

	public Map<Double, Double> getPeakList() {
		return peakList;
	}

	public String getId() {
		return (sourceFile != null) ? sourceFile.getName() : index.toString();
	}

	public Integer getPrecursorCharge() {
		return charge;
	}

	public Double getPrecursorMZ() {
		return getObservedMZ();
	}

	public Double getPrecursorIntensity() {
		return getObservedIntensity();
	}
	
	public Integer getMsLevel() {
		// can only contain MS2 spectra.
		return 2;
	}

	@Override
	public String toString() {
		// intialize the string with the header
		String string = String.format("%3.2f\t%3.2f\t%d\n", observedMZ, observedIntensity, charge);
		
		List<Double> masses = new ArrayList<Double>(peakList.keySet());
		Collections.sort(masses);
		
		// add the peaks
		for (Double mz : masses)
			string += String.format("%3.2f\t%3.2f\n", mz, peakList.get(mz));
		
		return string;
	}

	@Override
	public ParamGroup getAdditional() {
		// no additional data available for PKL spectra
		return new ParamGroup();
	}
}
