package uk.ac.ebi.pride.tools.mzdata_parser;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.ParamGroup;
import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.CvParam;
import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.PeakListBinary;
import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Precursor;

/**
 * A wrapper class around the mzData Spectrum
 * object implementing the PeakListParser
 * Spectrum interface.
 * @author jg
 *
 */
public class MzDataSpectrum implements Spectrum {
	/**
	 * The extracted precursor charge. NULL
	 * in case non was found.
	 */
	private Integer precursorCharge = null;
	/**
	 * The extracted precursor m/z. NULL
	 * in case non was found.
	 */
	private Double precursorMz = null;
	/**
	 * The extracted precursor intensity. NULL
	 * in case non was found.
	 */
	private Double precursorIntensity = null;
	/**
	 * The peak list as a Map of DoubeS with
	 * the m/z as key and its intensity as value.
	 */
	private Map<Double, Double> peakList;
	/**
	 * The spectrum's ms level.
	 */
	private int msLevel;
	/**
	 * The spectrum's id
	 */
	private Integer id;
	/**
	 * Container holding additional information about
	 * the spectrum.
	 */
	private ParamGroup paramGroup;
	
	public MzDataSpectrum(uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum mzDataSpectrum) {
		// save the id
		this.id = mzDataSpectrum.getId();
		
		// set the ms level
		msLevel = mzDataSpectrum.getSpectrumDesc().getSpectrumSettings().getSpectrumInstrument().getMsLevel();
		
		// initialize the param group
		paramGroup = new ParamGroup();
		
		// extract the precursor information
		extractPrecursorInformation(mzDataSpectrum);
		
		// extract the peak list
		extractPeakList(mzDataSpectrum);
	}
	
	/**
	 * Extracts the precursor's m/z, intensity,
	 * and charge form the passed SpectrumType object.
	 * @param mzDataSpectrum
	 */
	private void extractPrecursorInformation(uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum mzDataSpectrum) {
		// if there's no precursor list, return
		if (mzDataSpectrum.getSpectrumDesc().getPrecursorList() == null)
			return;
		
		// only process the first precursor
		Precursor precursor = mzDataSpectrum.getSpectrumDesc().getPrecursorList().getPrecursor().get(0);
		
		// parse the cvParams
		for (CvParam cvParam : precursor.getIonSelection().getCvParams()) {
			String accession = cvParam.getAccession();
			String value = cvParam.getValue();
			
			if ("PSI:1000040".equals(accession) || "MS:1000744".equals(accession))
				precursorMz = Double.parseDouble(value);
			else if ("PSI:1000041".equals(accession) || "MS:1000041".equals(accession))
				precursorCharge = Integer.parseInt(value);
			else if ("PSI:1000042".equals(accession) || "MS:1000042".equals(accession))
				precursorIntensity = Double.parseDouble(value);
			else
				paramGroup.addParam(new uk.ac.ebi.pride.tools.jmzreader.model.impl.CvParam(
						cvParam.getName(), cvParam.getValue(), 
						cvParam.getCvLabel(), cvParam.getAccession()));
		}
		
		// parse the activation parameters
		for (CvParam cvParam : precursor.getActivation().getCvParams()) {
			paramGroup.addParam(new uk.ac.ebi.pride.tools.jmzreader.model.impl.CvParam(
					cvParam.getName(), cvParam.getValue(), 
					cvParam.getCvLabel(), cvParam.getAccession()));
		}
	}
	
	/**
	 * Extracts the peak list from the passed
	 * mzData spectrum and saves it in the
	 * peakList Map.
	 * @param mzDataSpectrum
	 */
	private void extractPeakList(
			uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum mzDataSpectrum) {
		// get the data
		PeakListBinary.Data mzData 		= mzDataSpectrum.getMzArrayBinary().getData();
		PeakListBinary.Data intenData 	= mzDataSpectrum.getIntenArrayBinary().getData();
		
		ByteBuffer mzBuffer 	= ByteBuffer.wrap(mzData.getValue());
		ByteBuffer intenBuffer 	= ByteBuffer.wrap(intenData.getValue());
		
		// set the endianess and precision
		if (mzData.getEndian().equalsIgnoreCase("little"))
			mzBuffer.order(ByteOrder.LITTLE_ENDIAN);
		else
			mzBuffer.order(ByteOrder.BIG_ENDIAN);
		
		if (intenData.getEndian().equalsIgnoreCase("little"))
			intenBuffer.order(ByteOrder.LITTLE_ENDIAN);
		else
			intenBuffer.order(ByteOrder.BIG_ENDIAN);
		
		// convert the buffers into lists
		List<Double> mz = new ArrayList<Double>();
		List<Double> inten = new ArrayList<Double>();
		
		int size = mzData.getPrecision().equals("32") ? 4 : 8;
		for (int i = 0; i < mzBuffer.limit(); i += size)
			mz.add(size == 4 ? ((Float) mzBuffer.getFloat(i)).doubleValue() : mzBuffer.getDouble(i));
		
		size = intenData.getPrecision().equals("32") ? 4 : 8;
		for (int i = 0; i < intenBuffer.limit(); i += size)
			inten.add(size == 4 ? ((Float) intenBuffer.getFloat(i)).doubleValue() : intenBuffer.getDouble(i));
		
		if (inten.size() != mz.size())
			throw new IllegalStateException("Different sizes encountered for intensity and m/z array (spectrum id = " + id + ")");
		
		// create and fill the peak list
		peakList = new HashMap<Double, Double>();
		
		for (int i = 0; i < mz.size(); i++)
			peakList.put(mz.get(i), inten.get(i));
	}
	
	@Override
	public String getId() {
		return id.toString();
	}

	@Override
	public Integer getPrecursorCharge() {
		return precursorCharge;
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
		return msLevel;
	}

	@Override
	public ParamGroup getAdditional() {
		return paramGroup;
	}
}
