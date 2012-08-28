package uk.ac.ebi.pride.tools.mzxml_parser;

import java.util.Map;

import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.CvParam;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.ParamGroup;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.UserParam;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.Scan;

/**
 * This class wraps Scan elements into PeakListParser
 * compatible Spectrum objects. It is implemented as
 * a nested class to be completely independent of the
 * JAXB generated object model.
 * @author jg
 *
 */
public class MzXMLSpectrum implements Spectrum {
	/**
	 * The run's num attribute
	 */
	private Long num;
	/**
	 * The spectrum's charge
	 */
	private Long charge;
	/**
	 * The precursor's m/z
	 */
	private Double precursorMz;
	/**
	 * The precursor's intensity
	 */
	private Double precursorIntensity;
	/**
	 * The actual peak list
	 */
	private Map<Double, Double> peakList;
	/**
	 * ParamGroup holding additional information
	 * about the spectrum.
	 */
	private ParamGroup paramGroup;
	/**
	 * The spectrum's ms level
	 */
	private Long msLevel;

	/**
	 * Create a new MzXMLSpectrum object wrapping
	 * the given Scan object.
	 * @param scan the Scan to create a MzXMLSpectra object from.
	 */
	public MzXMLSpectrum(Scan scan) throws MzXMLParsingException {
		// make sure it's a MS2 spectrum
//		if (scan.getMsLevel().intValue() != 2)
//			throw new MzXMLParsingException("Unsupported MS level encountered in MzXMLSpectrum: MzXMLSpectrum can only wrap MS2 spectra.");
		
		// only single peak lists are supported by this class
		if (scan.getPeaks().size() == 1)
			peakList = MzXMLFile.convertPeaksToMap(scan.getPeaks().get(0));
		else
			throw new MzXMLParsingException("Multiple peak lists can not be modeled in a mzXMLSpectrum.");
		
		msLevel = scan.getMsLevel();
		
		// set the num
		num = scan.getNum();
		
		// set the rest of the information
		if (scan.getPrecursorMz().size() == 1) {
			precursorMz = (double) scan.getPrecursorMz().get(0).getValue();
			precursorIntensity = (double) scan.getPrecursorMz().get(0).getPrecursorIntensity();
			charge = scan.getPrecursorMz().get(0).getPrecursorCharge();
		}
		
		// create and populate the param group
		paramGroup = new ParamGroup();
		
		if (scan.getPolarity() != null)
			paramGroup.addParam(new CvParam("scan polarity", scan.getPolarity(), "MS", "MS:1000465"));
		if (scan.getScanType() != null)
			paramGroup.addParam(new UserParam("scan type", scan.getScanType()));
		if (scan.getFilterLine() != null)
			paramGroup.addParam(new UserParam("filter line", scan.getFilterLine()));
		if (scan.isCentroided() != null && scan.isCentroided())
			paramGroup.addParam(new CvParam("centroid spectrum", "true", "MS", "MS:1000127"));
		if (scan.isDeisotoped() != null && scan.isDeisotoped())
			paramGroup.addParam(new CvParam("deisotoping", "true", "MS", "MS:1000033"));
		if (scan.isChargeDeconvoluted())
			paramGroup.addParam(new CvParam("charge deconvolution", "true", "MS", "MS:1000034"));
		if (scan.getRetentionTime() != null)
			paramGroup.addParam(new CvParam("retention time", scan.getRetentionTime().toString(), "MS", "MS:1000894"));
		if (scan.getIonisationEnergy() != null)
			paramGroup.addParam(new UserParam("ionisation energy", scan.getIonisationEnergy().toString()));
		if (scan.getCollisionEnergy() != null)
			paramGroup.addParam(new CvParam("collision energy", scan.getCollisionEnergy().toString(), "MS", "MS:1000045"));
		if (scan.getCidGasPressure() != null)
			paramGroup.addParam(new CvParam("collision gas pressure", scan.getCidGasPressure().toString(), "MS", "MS:1000045"));
		if (scan.getTotIonCurrent() != null)
			paramGroup.addParam(new CvParam("total ion current", scan.getTotIonCurrent().toString(), "MS", "MS:1000285"));
	}
	
	@Override
	public String getId() {
		return num.toString();
	}

	@Override
	public Integer getPrecursorCharge() {
		return (charge != null) ? charge.intValue() : null;
	}

	@Override
	public Double getPrecursorMZ() {
		return precursorMz;
	}

	@Override
	public Double getPrecursorIntensity() {
		return precursorIntensity;
	}

	@Override
	public Map<Double, Double> getPeakList() {
		return peakList;
	}

	@Override
	public Integer getMsLevel() {
		// these are always MS2 spectra
		return msLevel.intValue();
	}

	@Override
	public ParamGroup getAdditional() {
		return paramGroup;
	}
}
