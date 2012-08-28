package uk.ac.ebi.pride.tools.mgf_parser.model;

import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.CvParam;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.ParamGroup;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.UserParam;
import uk.ac.ebi.pride.tools.mgf_parser.MgfFile;

public class Ms2Query implements Spectrum {
	
	
	private static Pattern peakPattern = Pattern.compile("\\s*([0-9.]+)\\s+([0-9.]+)\\s*");
	
	/**
	 * Optional title of the spectrum identification
	 */
	private String title;
	/**
	 * Optional charge state of the precursor peptide.
	 */
	private String chargeState;
	/**
	 * Optional petpide tolerance
	 */
	private Double tolerance;
	/**
	 * Optional peptide tolerance unit
	 */
	private MgfFile.PeptideToleranceUnit toleranceUnit;
	/**
	 * Optional sequence qualifiers
	 * See the Mascot documentation for detailed information (http://www.matrixscience.com/help/sq_help.html#SEQ)
	 */
	private List<String> sequenceQualifiers;
	/**
	 * Optional amino acid composition
	 */
	private String composition;
	/**
	 * Sequence tags
	 * See the Mascot documentation for detailed information (http://www.matrixscience.com/help/sq_help.html#TAG)
	 */
	private List<String> tags;
	/**
	 * Error tolerant sequence tags
	 */
	private List<String> errorTolerantTags;
	/**
	 * Optional scan number or range
	 */
	private String scan;
	/**
	 * Optional retention time or range in seconds
	 */
	private String retentionTime;
	/**
	 * Optional MS/MS ion series to be used based on the instrument 
	 * as defined in fragmentation_rules
	 */
	private String instrument;
	/**
	 * Optional variable modifications as defined in unimod.xml
	 */
	private String variableModifications;
	/**
	 * The spectrum's peaks
	 */
	private Map<Double, Double> peakList;
	/**
	 * Optional peptide mass
	 */
	private Double peptideMass;
    /**
     * Optional peptide intensity
     */
    private Double peptideIntensity;
    /**
     * 1-based index of the spectrum in the file.
     */
    private Integer index;
	
	/**
	 * Default constructor generating an empty Ms2Query
	 */
	public Ms2Query() {
		
	}
	
	/**
	 * Generates a Ms2Query from a mgf part representing
	 * a Ms2Query (including "BEGIN IONS" and "END IONS")
	 * @param mgfQuery
	 */
	public Ms2Query(String mgfQuery, int index) throws JMzReaderException  {
		this.index = index;
		
		// process the mgf section line by line
		String[] lines = mgfQuery.trim().split("\n");
		boolean inAttributeSection = true;
		
		for (int nLineNumber = 0; nLineNumber < lines.length; nLineNumber++) {
			String line = lines[nLineNumber].trim();
			
			// remove comments from the line
			line = line.replaceAll(MgfFile.mgfCommentRegex, line);
			
			// ignore empty lines
			if (line.length() < 1)
				continue;
			
			// first line must be "BEGIN IONS" and last line must be "END IONS"
			if (nLineNumber == 0 && !"BEGIN IONS".equals(line))
				throw new JMzReaderException("MS2 query must start with 'BEGIN IONS'");
			
			if (nLineNumber == 0) continue;
			
			if (nLineNumber == lines.length - 1 && !"END IONS".equals(line))
				throw new JMzReaderException("MS2 query must end with 'END IONS'");
			
			if (nLineNumber == lines.length -1) continue;
			
			// check if it's a property
			Matcher attributeMatcher = MgfFile.attributePattern.matcher(line);
			
			if (inAttributeSection && attributeMatcher.find()) {
				if (attributeMatcher.groupCount() != 2)
					throw new JMzReaderException("Invalid attribute line encountered in MS2 query.");
				
				String name 	= attributeMatcher.group(1);
				String value	= attributeMatcher.group(2);
				
				// save the attribute
				saveAttribute(name, value);
			}
			else {
				Matcher peakMatcher = peakPattern.matcher(line);
				
				if (!peakMatcher.find() || peakMatcher.groupCount() != 2)
					throw new JMzReaderException("Invalid line encountered in MS2 query: " + line);
				
				// add the peak
				addPeak(Double.parseDouble(peakMatcher.group(1)), Double.parseDouble(peakMatcher.group(2)));
				
				inAttributeSection = false;
			}
		}
	}
	
	/**
	 * Stores the attribute in the respective member variable.
	 * @param name The attribute's name
	 * @param value The attribute's value
	 * @throws JMzReaderException 
	 */
	private void saveAttribute(String name, String value) throws JMzReaderException {
		if ("TITLE".equals(name))
			title = value;
		else if ("CHARGE".equals(name))
			chargeState = value;
		else if ("TOL".equals(name))
			tolerance = Double.parseDouble(value);
		else if ("TOLU".equals(value)) {
			if ("%".equals(value))		toleranceUnit = MgfFile.PeptideToleranceUnit.PERCENT;
			if ("ppm".equals(value))	toleranceUnit = MgfFile.PeptideToleranceUnit.PPM;
			if ("mmu".equals(value))	toleranceUnit = MgfFile.PeptideToleranceUnit.MMU;
			if ("Da".equals(value))		toleranceUnit = MgfFile.PeptideToleranceUnit.DA;
			
			// make sure the tolerance unit
			if (toleranceUnit == null)
				throw new IllegalStateException("Invalid tolerance unit set.");
		}
		else if ("SEQ".equals(name))
			addSequenceQualifier(value);
		else if ("COMP".equals(name))
			composition = value;
		else if ("TAG".equals(name))
			addTag(value);
		else if ("ETAG".equals(name))
			addETag(value);
		else if ("SCANS".equals(name))
			scan = value;
		else if ("RTINSECONDS".equals(name))
			retentionTime = value;
		else if ("INSTRUMENT".equals(name))
			instrument = value;
		// TODO: make sure IT_MODS are handeled correctly
		else if ("IT_MODS".equals(name))
			variableModifications = value;
		else if ("PEPMASS".equals(name)) {
            // This section has been changed by Rui Wang, to support optional intensity values
            if (value.trim().contains(" ")) {
                String[] parts = value.split(" ");
                peptideMass = Double.parseDouble(parts[0]);
                peptideIntensity = Double.parseDouble(parts[1]);
            } else {
                peptideMass = Double.parseDouble(value);
            }
        }
		else
			throw new JMzReaderException("Unknown peptide property '" + name + "' encountered");
	}
	
	/**
	 * Adds a peak to the spectrum.
	 * @param peak
	 */
	public void addPeak(Double mz, Double intensity) {
		if (peakList == null)
			peakList = new HashMap<Double, Double>(1);
		
		peakList.put(mz, intensity);
	}
	
	public void addSequenceQualifier(String sequenceQualifier) {
		if (sequenceQualifiers == null)
			sequenceQualifiers = new ArrayList<String>();
		
		sequenceQualifiers.add(sequenceQualifier);
	}
	
	public void addTag(String tag) {
		if (tags == null)
			tags = new ArrayList<String>();
		
		tags.add(tag);
	}
	
	public void addETag(String eTag) {
		if (errorTolerantTags == null)
			errorTolerantTags = new ArrayList<String>();
		
		errorTolerantTags.add(eTag);
	}

	public String getTitle() {
		return title;
	}

	public void setTitle(String title) {
		this.title = title;
	}

	public String getChargeState() {
		return chargeState;
	}

	public void setChargeState(String chargeState) {
		this.chargeState = chargeState;
	}

	public Double getTolerance() {
		return tolerance;
	}

	public void setTolerance(Double tolerance) {
		this.tolerance = tolerance;
	}

	public MgfFile.PeptideToleranceUnit getToleranceUnit() {
		return toleranceUnit;
	}

	public void setToleranceUnit(MgfFile.PeptideToleranceUnit toleranceUnit) {
		this.toleranceUnit = toleranceUnit;
	}

	public String getComposition() {
		return composition;
	}

	public void setComposition(String composition) {
		this.composition = composition;
	}

	public List<String> getTags() {
		return tags;
	}

	public void setTags(List<String> tags) {
		this.tags = tags;
	}

	public String getRetentionTime() {
		return retentionTime;
	}

	public void setRetentionTime(String retentionTime) {
		this.retentionTime = retentionTime;
	}

	public String getInstrument() {
		return instrument;
	}

	public void setInstrument(String instrument) {
		this.instrument = instrument;
	}

	public List<String> getSequenceQualifiers() {
		return sequenceQualifiers;
	}

	public void setSequenceQualifiers(List<String> sequenceQualifiers) {
		this.sequenceQualifiers = sequenceQualifiers;
	}

	public List<String> getErrorTolerantTags() {
		return errorTolerantTags;
	}

	public void setErrorTolerantTags(List<String> errorTolerantTags) {
		this.errorTolerantTags = errorTolerantTags;
	}

	public String getScan() {
		return scan;
	}

	public void setScan(String scan) {
		this.scan = scan;
	}

	public String getVariableModifications() {
		return variableModifications;
	}

	public void setVariableModifications(String variableModifications) {
		this.variableModifications = variableModifications;
	}

	public Map<Double, Double> getPeakList() {
		return peakList;
	}

	public void setPeakList(Map<Double, Double> peakList) {
		this.peakList = peakList;
	}

	public Double getPeptideMass() {
		return peptideMass;
	}

	public void setPeptideMass(Double peptideMass) {
		this.peptideMass = peptideMass;
	}

    public Double getPeptideIntensity() {
        return peptideIntensity;
    }

    public void setPeptideIntensity(Double peptideIntensity) {
        this.peptideIntensity = peptideIntensity;
    }

    @Override
	public String toString() {
		String query = "BEGIN IONS\n";
		
		// process the optional attribtues
		if (chargeState != null)
			query += "CHARGE=" + chargeState + "\n";
		if (composition != null)
			query += "COMP=" + composition + "\n";
		if (errorTolerantTags != null && errorTolerantTags.size() > 0) {
			for (String tag : errorTolerantTags)
				query += "ETAG=" + tag + "\n";
		}
		if (instrument != null)
			query += "INSTRUMENT=" + instrument + "\n";
		if (variableModifications != null)
			query += "IT_MODS=" + variableModifications + "\n";
		if (peptideMass != null)
			query += "PEPMASS=" + peptideMass + "\n";
		if (retentionTime != null)
			query += "RTINSECONDS=" + retentionTime + "\n";
		if (scan != null)
			query += "SCANS=" + scan + "\n";
		if (sequenceQualifiers != null) {
			for (String qual : sequenceQualifiers)
				query += "SEQ=" + qual + "\n";
		}
		if (tags != null) {
			for (String tag : tags)
				query += "TAG=" + tag + "\n";
		}
		if (title != null)
			query += "TITLE=" + title + "\n";
		if (tolerance != null)
			query += "TOL=" + tolerance + "\n";
		if (toleranceUnit != null)
			query += "TOLU=" + toleranceUnit + "\n";
		
		List<Double> masses = new ArrayList<Double>(peakList.keySet());
		Collections.sort(masses);
		
		// process the peak list
		for (Double mz : masses)
			query += mz + " " + peakList.get(mz) + "\n";
		
		query += "END IONS\n";
		
		return query;
	}

	public String getId() {
		if (index == null)
			return null;
		else
			return index.toString();
	}

	public Integer getPrecursorCharge() {
		try {
			// if there are multiple charge states, give up
			if (chargeState.contains(","))
				return null;
			
			if (chargeState.contains("-")) {
				Integer charge = Integer.parseInt(chargeState);
				return charge;
			}
			else {
				Integer charge = Integer.parseInt(chargeState.replace("+", ""));
				return charge;
			}
		}
		catch (Exception e) {
			return null;
		}
	}

	public Double getPrecursorMZ() {
		return peptideMass;
	}

	public Double getPrecursorIntensity() {
		return peptideIntensity;
	}

	@Override
	public Integer getMsLevel() {
		// can only be a MS2 level spectrum
		return 2;
	}

	@Override
	public ParamGroup getAdditional() {
		// create a new param group
		ParamGroup paramGroup = new ParamGroup();
		
		if (retentionTime != null)
			paramGroup.addParam(new CvParam("retention time", retentionTime, "MS", "MS:1000894"));
		if (scan != null)
			paramGroup.addParam(new CvParam("peak list scans", scan, "MS", "MS:1000797"));
		if (title != null)
			paramGroup.addParam(new CvParam("spectrum title", title, "MS", "MS:1000796"));
		if (tolerance != null)
			paramGroup.addParam(new CvParam("Fragment mass tolerance setting", tolerance.toString(), "PRIDE", "PRIDE:0000161"));
		
		if (toleranceUnit != null)
			paramGroup.addParam(new UserParam("Fragment mass tolerance unit", toleranceUnit.toString()));
		if (instrument != null)
			paramGroup.addParam(new UserParam("Instrument", instrument));
		
		return paramGroup;
	}
}
