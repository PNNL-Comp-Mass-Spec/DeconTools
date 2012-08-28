package uk.ac.ebi.pride.tools.ms2_parser.model;

import java.util.HashMap;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.ParamGroup;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.UserParam;

/**
 * Represents a spectrum in a ms2 file.
 * @author jg
 *
 */
public class Ms2Spectrum implements Spectrum {
	/**
	 * The first scan used.
	 */
	private int lowScan;
	/**
	 * The last scan used (can be the same as the first
	 * scan if only one was used.
	 */
	private int highScan;
	/**
	 * The precursor's m/z
	 */
	private Double precursorMZ;
	/**
	 * Additional information (as f.e. the retention time). This
	 * corresponds to the information found in the "I..." lines.
	 * This information is charge independent.
	 */
	private HashMap<String, String> additionalInformation;
	/**
	 * Charge information. The charge is used as key and the respective
	 * mass as value.
	 */
	private HashMap<Integer, Double> charges;
	/**
	 * Charge dependent analysis data
	 */
	private HashMap<String, String> chargeDependentData;
	/**
	 * The peak list
	 */
	private Map<Double, Double> peakList;
	/**
	 * Pattern used to parse header lines
	 */
	private static Pattern headerLinePattern = Pattern.compile("([SIZD])\t(.+)");
	/**
	 * The spectrum's 1-based index int he file.
	 */
	private Integer index;
	
	/**
	 * Creates a Ms2Spectrum object by parsing
	 * the supplied ms2 spectrum information. This
	 * should be the portion of the ms2 file between
	 * the respective "S..." line and the line before
	 * the next "S..." line (= the last peak).
	 * @param ms2SpectrumInformation The ms2 spectrum information to parse.
	 * @param index The spectrum's 1-based index in the file
	 * @throws JMzReaderException 
	 */
	public Ms2Spectrum(String ms2SpectrumInformation, int index) throws JMzReaderException {
		// split the supplied info into lines
		String[] lines = ms2SpectrumInformation.split("\\n");
		
		// parse the header
		int dataOffset = parseHeader(lines);
		
		// create the peak list
		parsePeakList(lines, dataOffset);
		
		this.index = index;
	}
	
	/**
	 * Parses the header section of a ms2 spectrum. Returns the
	 * index of the first non-header line.
	 * @param lines The lines of the header
	 * @return The index of the first non-header line.
	 */
	private int parseHeader(String[] lines) throws JMzReaderException {
		int i = 0;
		
		// reset / create the HashMaps
		additionalInformation 	= new HashMap<String, String>();
		chargeDependentData 	= new HashMap<String, String>();
		charges					= new HashMap<Integer, Double>();
		
		// loop through the lines to parse the header
		for (; i < lines.length; i++) {
			// make sure it's a header line
			Matcher matcher = headerLinePattern.matcher(lines[i]);
			
			// if it's not a header line end the loop
			if (!matcher.find())
				break;
			
			// all two groups must be found
			if (matcher.groupCount() != 2)
				throw new JMzReaderException("Invalid spectrum header line encountered: '" + lines[i] + "'");
			
			// the first line must be the S line
			if (i == 0 && !matcher.group(1).equals("S"))
				throw new JMzReaderException("Spectra must start with a S line.");
			
			// S line
			if ("S".equals(matcher.group(1))) {
				String fields[] = matcher.group(2).split("\t");
				// every S line must have 3 fields
				if (fields.length != 3)
					throw new JMzReaderException("Invalid S line encountered in spectrum: '" + lines[i] + "'");
				
				// save the variables
				lowScan 	= Integer.parseInt(fields[0]);
				highScan 	= Integer.parseInt(fields[1]);
				precursorMZ = Double.parseDouble(fields[2]);
			}
			// I line
			else if ("I".equals(matcher.group(1))) {
				// always field value pairs
				String fields[] = matcher.group(2).split("\t");
				
				if (fields.length != 2)
					throw new JMzReaderException("Invalid I line encountered: " + lines[i] + "'");
				
				// save the field
				setField(additionalInformation, fields[0], fields[1]);
			}
			// Z line
			else if ("Z".equals(matcher.group(1))) {
				// always field value pairs
				String fields[] = matcher.group(2).split("\t");
				
				if (fields.length != 2)
					throw new JMzReaderException("Invalid Z line encountered: " + lines[i] + "'");
				
				// save the field
				charges.put(Integer.parseInt(fields[0]), Double.parseDouble(fields[1]));
			}
			// D line
			else if ("D".equals(matcher.group(1))) {
				// always field value pairs
				String fields[] = matcher.group(2).split("\t");
				
				if (fields.length != 2)
					throw new JMzReaderException("Invalid D line encountered: " + lines[i] + "'");
				
				// save the field
				setField(chargeDependentData, fields[0], fields[1]);
			}
		}
		
		return i;
	}
	
	/**
	 * Parses the peak list of the ms2 spectrum.
	 * @param lines The lines containing the ms2 spectrum's information.
	 * @param nOffset The index of the first line containing peak information.
	 * @throws JMzReaderException 
	 */
	private void parsePeakList(String[] lines, int nOffset) throws JMzReaderException {
		// reset / create the peak list
		peakList = new HashMap<Double, Double>();
		
		// parse the lines
		for (int i = nOffset; i < lines.length; i++) {
			String[] fields = lines[i].split("\\s+");
			
			// every line must contain two fields
			if (fields.length != 2)
				throw new JMzReaderException("Invalid peak line found: '" + lines[i] + "'");
			
			// save the peak
			peakList.put(Double.parseDouble(fields[0]), Double.parseDouble(fields[1]));
		}
	}
	
	/**
	 * Sets the given field in the respective hash map. If the field is already
	 * set the new value is stored under the key "[fieldName]_[1-n]". F.e. if 
	 * "Comments" already exists another "Comments" field is stored under
	 * "Comments_1".
	 * @param hashMap The HashMap to store the field in.
	 * @param fielName The field's name.
	 * @param fieldValue The field's value.
	 */
	private void setField(HashMap<String, String> hashMap, String fieldName, String fieldValue) {
		// check whether the field already exists
		if (hashMap.containsKey(fieldName)) {
			int fieldNumber = 1;
			
			while (hashMap.containsKey(fieldName + "_" + fieldNumber))
				fieldNumber++;
			
			// set the new unique fieldName
			fieldName = fieldName + "_" + fieldNumber;
		}
		
		// store the field
		hashMap.put(fieldName, fieldValue);
	}
	
	/**
	 * The number of the first scan used to generate this
	 * peak list.
	 * @return Number of the first scan.
	 */
	public int getLowScan() {
		return lowScan;
	}

	/**
	 * Number of the last scan used to generate this
	 * peak list.
	 * @return Number of the last scan.
	 */
	public int getHighScan() {
		return highScan;
	}

	/**
	 * The precursor's m/z.
	 * @return The precursor's m/z.
	 */
	public Double getPrecursorMZ() {
		return precursorMZ;
	}
	
	public String getId() {
		return index.toString();
	}

	public Integer getPrecursorCharge() {
		if (charges.size() == 1)
			return charges.keySet().iterator().next();
		else
			return null;
	}

	public Double getPrecursorIntensity() {
		// only available if there's only one charge
		if (charges.size() == 1)
			return charges.values().iterator().next();
		else
			return null;
	}

	/**
	 * A HashMap containing the fields from the additional
	 * charge state independent information (I lines). If
	 * certain fields are reported multiple times the
	 * additional values are stored under the key
	 * [fieldname]_[1-n].
	 * @return A HashMap containing the additional charge independent information.
	 */
	public HashMap<String, String> getAdditionalInformation() {
		return additionalInformation;
	}

	/**
	 * The different charge states associated with this spectrum
	 * in a HashMap with the charge as key and the resulting
	 * mass as value.
	 * @return A HashMap containing the associated charge states.
	 */
	public HashMap<Integer, Double> getCharges() {
		return charges;
	}

	/**
	 * A HashMap containing charge dependent additional data.
	 * If certain fields are reported multiple times the
	 * additional values are stored under the key
	 * [fieldname]_[1-n].
	 * @return The charge dependent additional data.
	 */
	public HashMap<String, String> getChargeDependentData() {
		return chargeDependentData;
	}

	/**
	 * Returns the spectrum's peak list as a map with
	 * the m/z values as key and their intensities
	 * as values.
	 * @return The peak list as a Map
	 */
	public Map<Double, Double> getPeakList() {
		return peakList;
	}

	public Integer getMsLevel() {
		// always MS2 spectra
		return 2;
	}

	@Override
	public ParamGroup getAdditional() {
		ParamGroup paramGroup = new ParamGroup();
		
		paramGroup.addParam(new UserParam("high scan", String.format("%d", highScan)));
		paramGroup.addParam(new UserParam("low scan", String.format("%d", lowScan)));
		
		for (String name : chargeDependentData.keySet())
			paramGroup.addParam(new UserParam(name, chargeDependentData.get(name)));
		
		for (String name : additionalInformation.keySet())
			paramGroup.addParam(new UserParam(name, additionalInformation.get(name)));
		
		return paramGroup;
	}
}
