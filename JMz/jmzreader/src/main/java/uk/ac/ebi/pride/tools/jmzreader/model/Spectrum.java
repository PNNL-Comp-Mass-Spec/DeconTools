package uk.ac.ebi.pride.tools.jmzreader.model;

import java.util.Map;

import uk.ac.ebi.pride.tools.jmzreader.model.impl.ParamGroup;

/**
 * Represents a spectrum in the
 * parsed peak list file.
 * @author jg
 *
 */
public interface Spectrum {
	/**
	 * Retrieves the spectrum's id.
	 * @return
	 */
	public String getId();
	
	/**
	 * Returns the spectrum's charge or
	 * null in case the charge is not
	 * available.
	 * @return
	 */
	public Integer getPrecursorCharge();
	
	/**
	 * Returns the precursor's m/z or
	 * null in case the precursor's m/z
	 * is not available.
	 * @return
	 */
	public Double getPrecursorMZ();
	
	/**
	 * Returns the precursor's intensity
	 * or null in case it it not available.
	 * @return
	 */
	public Double getPrecursorIntensity();
	
	/**
	 * Returns the spectrum's peak list as
	 * a HashMap with the m/z values as keys
	 * and the corresponding intensities as
	 * values.
	 * @return
	 */
	public Map<Double, Double> getPeakList();
	
	/**
	 * Returns the msLevel of the spectrum. NULL
	 * in case the MS level is not available or
	 * unknown.
	 * @return
	 */
	public Integer getMsLevel();
	
	/**
	 * Retrieves file format specific variables
	 * as parameters. Whenever possible cvParams
	 * from the MS ontology will be used to report
	 * these additional fields.
	 * @return A ParamGroup containing the additional fields as parameters.
	 */
	public ParamGroup getAdditional();
}
