package uk.ac.ebi.pride.tools.jmzreader.model.impl;

import java.io.Serializable;
import java.util.Map;

import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;

public class SpectrumImplementation implements Spectrum, Serializable {
	private static final long serialVersionUID = 1L;
	private final String id;
	private final Integer precursorCharge;
	private final Double precursorMz;
	private final Double precursorIntensity;
	private final Map<Double, Double> peakList;
	private final Integer msLevel;

	public SpectrumImplementation(String id, Integer precursorCharge,
			Double precursorMz, Double precursorIntensity,
			Map<Double, Double> peakList, Integer msLevel) {
		this.id = id;
		this.precursorCharge = precursorCharge;
		this.precursorMz = precursorMz;
		this.precursorIntensity = precursorIntensity;
		this.peakList = peakList;
		this.msLevel = msLevel;
	}

	public String getId() {
		return id;
	}

	public Integer getPrecursorCharge() {
		return precursorCharge;
	}

	public Double getPrecursorMZ() {
		return precursorMz;
	}

	public Double getPrecursorIntensity() {
		return precursorIntensity;
	}

	public Map<Double, Double> getPeakList() {
		return peakList;
	}
	
	public Integer getMsLevel() {
		return msLevel;
	}

	@Override
	public int hashCode() {
		final int prime = 31;
		int result = 1;
		result = prime * result + ((id == null) ? 0 : id.hashCode());
		result = prime * result
				+ ((peakList == null) ? 0 : peakList.hashCode());
		result = prime * result
				+ ((precursorCharge == null) ? 0 : precursorCharge.hashCode());
		result = prime
				* result
				+ ((precursorIntensity == null) ? 0 : precursorIntensity
						.hashCode());
		result = prime * result
				+ ((precursorMz == null) ? 0 : precursorMz.hashCode());
		result = prime * result
				+ ((msLevel == null) ? 0 : msLevel.hashCode());
		return result;
	}

	@Override
	public boolean equals(Object obj) {
		if (this == obj)
			return true;
		if (obj == null)
			return false;
		if (getClass() != obj.getClass())
			return false;
		SpectrumImplementation other = (SpectrumImplementation) obj;
		if (id == null) {
			if (other.id != null)
				return false;
		} else if (!id.equals(other.id))
			return false;
		if (peakList == null) {
			if (other.peakList != null)
				return false;
		} else if (!peakList.equals(other.peakList))
			return false;
		if (precursorCharge == null) {
			if (other.precursorCharge != null)
				return false;
		} else if (!precursorCharge.equals(other.precursorCharge))
			return false;
		if (precursorIntensity == null) {
			if (other.precursorIntensity != null)
				return false;
		} else if (!precursorIntensity.equals(other.precursorIntensity))
			return false;
		if (precursorMz == null) {
			if (other.precursorMz != null)
				return false;
		} else if (!precursorMz.equals(other.precursorMz))
			return false;
		if (msLevel == null) {
			if (other.msLevel != null)
				return false;
		} else if (!msLevel.equals(other.msLevel))
			return false;
		return true;
	}

	@Override
	public ParamGroup getAdditional() {
		// TODO: fix
		return null;
	}
}
