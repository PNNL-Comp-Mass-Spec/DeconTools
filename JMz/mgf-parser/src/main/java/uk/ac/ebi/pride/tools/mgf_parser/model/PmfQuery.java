package uk.ac.ebi.pride.tools.mgf_parser.model;

import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;

public class PmfQuery {
	/**
	 * The query's mass. This is always present.
	 */
	private Double mass;
	/**
	 * The query's intensity. Null if not set
	 */
	private Double intensity;
	
	public PmfQuery(Double mass, Double intensity) {
		this.mass = mass;
		this.intensity = intensity;
	}
	
	/**
	 * Creates a PmfQuery object based on an mgfFileLine.
	 * @param mgfFileLine The line in a mgfFile representing the pmf query
	 * @throws JMzReaderException 
	 */
	public PmfQuery(String mgfFileLine) throws JMzReaderException {
		// split the line into fields
		String[] fields = mgfFileLine.split("\\s+");
		
		if (fields.length == 0)
			throw new JMzReaderException("Malformatted line passed to generate PmfQuery object");
		
		mass = Double.parseDouble(fields[0]);
		
		// intensity is optional
		if (fields.length > 1)
			intensity = Double.parseDouble(fields[1]);
	}

	public Double getMass() {
		return mass;
	}

	public void setMass(Double mass) {
		this.mass = mass;
	}

	public Double getIntensity() {
		return intensity;
	}

	public void setIntensity(Double intensity) {
		this.intensity = intensity;
	}

	@Override
	/**
	 * Returns the PmfQuery as it is present in the
	 * mgf file.
	 */
	public String toString() {
		return mass.toString() + ((intensity != null) ? " " + intensity.toString() : "");
	}	
}
