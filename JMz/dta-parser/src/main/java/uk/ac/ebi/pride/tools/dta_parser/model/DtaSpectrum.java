package uk.ac.ebi.pride.tools.dta_parser.model;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.ParamGroup;

/**
 * This class can either represent a spectrum
 * in a concatenated dta file or one file
 * in a directory holding several dta spectra.
 * @author jg
 *
 */
public class DtaSpectrum implements Spectrum {
	/**
	 * This variable is only set in case the object represents a dta file containing
	 * only one spectrum
	 */
	private File sourceFile;
	/**
	 * The peptide's charge
	 */
	private Integer charge;
	/**
	 * The 1-based index of the spectrum
	 * in the file.
	 */
	private Integer index;
	/**
	 * The peptides MH+ mass. DTA files only store this mass and not
	 * the observed m/z.
	 */
	private Double mhMass;
	/**
	 * The peak list
	 */
	private HashMap<Double, Double> peakList;
	
	/**
	 * Creates a DTA spectrum object that's based
	 * on a specific source file. This sourcefile
	 * must only contain one spectrum.
	 * @param sourceFile
	 * @throws IOException 
	 */
	public DtaSpectrum(File sourceFile) throws JMzReaderException {
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
			parseDtaSection(lines);
		}
		catch (IOException e) {
			throw new JMzReaderException("Failed to read from file.", e);
		}
	}
	
	/**
	 * Creates a DTA spectrum object based on a section
	 * of a concatenated DTA file. This string must only
	 * contain the information for one spectrum.
	 * @param dtaFileSection
	 * @param index The 1-based index of the spectrum in the file.
	 */
	public DtaSpectrum(String dtaFileSection, int index) throws JMzReaderException {
		// parse the string line by line
		String[] lines = dtaFileSection.trim().split("\n");
		ArrayList<String> lineArray = new ArrayList<String>(lines.length);
		
		for (String line : lines)
			lineArray.add(line);
		
		// save the index
		this.index = index;
		
		// parse the section
		parseDtaSection(lineArray);
	}
	
	/**
	 * Parses a DTA section (see constructor) in form of an
	 * Array of Strings = Lines.
	 * @param lines
	 */
	private void parseDtaSection(List<String> lines) throws JMzReaderException {
		// create the new peak list
		peakList = new HashMap<Double, Double>();
		
		// make sure there are lines to parse
		if (lines.size() < 2)
			throw new JMzReaderException("Invalid DTA section passed to generate DTA spectrum object. DTA section contains to few lines.");
		
		// the first line should always contain the MH+ mass plus the charge state
		String[] headerFields = lines.get(0).split("\\s+");
		
		// there must only be two files
		if (headerFields.length != 2)
			throw new JMzReaderException("Invalid header section encountered in DTA spectrum.");
		
		// set the header fields
		mhMass = Double.parseDouble(headerFields[0]);
		try {
			charge = Integer.parseInt(headerFields[1]);
		}
		catch (NumberFormatException e) {
			throw new JMzReaderException("Invalid spectrum header line encountered. Charge state is not an integer: " + lines.get(0), e);
		}
		
		// process the peak list
		for (int nLineNumber = 1; nLineNumber < lines.size(); nLineNumber++) {
			String[] fields = lines.get(nLineNumber).split("\\s+");
			
			// make sure there are only two fields
			if (fields.length != 2)
				throw new JMzReaderException("Invalid line encountered in DTA spectrum");
			
			// save the peak
			peakList.put(Double.parseDouble(fields[0]), Double.parseDouble(fields[1]));
		}
	}
	
	/**
	 * Returns the precursor m/z. In DTA files only
	 * the peptide MH+ mass including the charge state
	 * is saved. In this helper function the proton mass
	 * is taken as 1.008.
	 * @return The precursor's m/z value.
	 */
	public Double getPrecursorMZ() {
		return (double) (mhMass + (1.008 * (charge - 1))) / (double) charge;
	}

	public String getId() {
		return (sourceFile != null) ? sourceFile.getName() : index.toString();
	}

	public Integer getPrecursorCharge() {
		return charge;
	}
	
	public Double getPrecursorIntensity() {
		// not available in dta files
		return null;
	}

	/**
	 * Returns the MH+ mass of the peptide. This is NOT the
	 * precursor M/Z.
	 * @return The MH+ mass of the peptide.
	 */
	public Double getMhMass() {
		return mhMass;
	}

	public Map<Double, Double> getPeakList() {
		return peakList;
	}

	public Integer getMsLevel() {
		// DTA files only contain ms2 spectra.
		return 2;
	}

	@Override
	public ParamGroup getAdditional() {
		return new ParamGroup();
	}
}
