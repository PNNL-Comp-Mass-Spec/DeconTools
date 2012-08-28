package uk.ac.ebi.pride.tools.mzml_wrapper;

import java.io.File;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

import uk.ac.ebi.jmzml.model.mzml.BinaryDataArray;
import uk.ac.ebi.jmzml.model.mzml.BinaryDataArrayList;
import uk.ac.ebi.jmzml.model.mzml.CVParam;
import uk.ac.ebi.jmzml.model.mzml.PrecursorList;
import uk.ac.ebi.jmzml.xml.io.MzMLUnmarshaller;
import uk.ac.ebi.jmzml.xml.io.MzMLUnmarshallerException;
import uk.ac.ebi.pride.tools.jmzreader.JMzReader;
import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;

/**
 * Wrapper class around the jmzml parser
 * library that's implementing the
 * JMzReader interface.
 * @author jg
 *
 */
public class MzMlWrapper implements JMzReader {
	/**
	 * MzML cvParams to be used to extract
	 * required parameters from the spectra.
	 * @author jg
	 *
	 */
	private enum MZML_PARAMS {
		SELECTED_MZ("MS:1000744"),
		PEAK_INTENSITY("MS:1000042"),
		CHARGE_STATE("MS:1000041"),
		MS_LEVEL("MS:1000511");
		
		private MZML_PARAMS(String accession) {
			this.accession = accession;
		}
		
		private final String accession;
		
		public String getAccess() {
			return accession;
		}
	}
	/**
	 * The unmarshaller to use to unmarshall
	 * the mzML objects.
	 */
	private final MzMLUnmarshaller unmarshaller;
    /**
     * List of spectra found in the mzML
     * file.
     */
    private final List<String> spectraIds;
    
    /**
     * Creates a new MzMlWrapper object parsing
     * the passed mzML file.
     * @param sourcefile The mzML file to parse.
     * @throws JMzReaderException Thrown in case the mzML file cannot be parsed correctly.
     */
    public MzMlWrapper(File sourcefile) throws JMzReaderException {
        // unmarshal the file
        try {
        	unmarshaller = new MzMLUnmarshaller(sourcefile);

        	// save the spectra ids
        	spectraIds = new ArrayList<String>(unmarshaller.getSpectrumIDs());
        }
        catch (RuntimeException e) {
        	throw new JMzReaderException("Failed to parse mzML file.", e);
        }
    }

	public int getSpectraCount() {
		return spectraIds.size();
	}

	public boolean acceptsFile() {
		return true;
	}

	public boolean acceptsDirectory() {
		return false;
	}

	public List<String> getSpectraIds() {
		return spectraIds;
	}

	public Spectrum getSpectrumById(String id) throws JMzReaderException {
		try {
			uk.ac.ebi.jmzml.model.mzml.Spectrum mzMlSpectrum = unmarshaller.getSpectrumById(id);
			
			return new MzMlWrapperSpectrum(mzMlSpectrum);
		} catch (MzMLUnmarshallerException e) {
			throw new JMzReaderException("Failed to load spectrum " + id + " from mzML file.", e);
		}
	}

	public Spectrum getSpectrumByIndex(int index)
			throws JMzReaderException {
		if (index < 1 || index > spectraIds.size())
			throw new JMzReaderException("Index out of range.");
		
		String id = spectraIds.get(index - 1);
		
		return getSpectrumById(id);
	}

	public Iterator<Spectrum> getSpectrumIterator() {
		return new MzMLSpectrumIterator();
	}
	
	/**
	 * Iterator over all spectra in the mzML
	 * file returning Peak List Parser 
	 * @author jg
	 *
	 */
	private class MzMLSpectrumIterator implements Iterator<Spectrum> {
		private final Iterator<String> idIterator = spectraIds.iterator();

		public boolean hasNext() {
			return idIterator.hasNext();
		}

		public Spectrum next() {
			try {
				return getSpectrumById(idIterator.next());
			} catch (JMzReaderException e) {
				throw new RuntimeException("Failed to parse mzML spectrum.", e);
			}
		}

		public void remove() {
			// not sensible			
		}
		
	}

	/**
	 * A wrapper class around the jmzml Spectrum
	 * class implementing the Spectrum interface
	 * from the peak-list-parser library.
	 * @author jg
	 *
	 */
	private class MzMlWrapperSpectrum implements Spectrum {
		
		/**
		 * The spectrum's id in the mzML file
		 */
		private final String id;
		/**
		 * The precursor's charge. Null if not
		 * available.
		 */
		private final Integer charge;
		/**
		 * The precursor's m/z value. Null if not
		 * available.
		 */
		private final Double mz;
		/**
		 * The precursor's intensity. Null if not
		 * available.
		 */
		private final Double intensity;
		/**
		 * The spectrum's MS level. Null if not
		 * available.
		 */
		private final Integer msLevel;
		/**
		 * The spectrum's peak list as a Map with
		 * the m/z values as keys and their intensities
		 * as values.
		 */
		private final Map<Double, Double> peakList;
		
		/**
		 * Creates a new MzMlWrapperSpectrum based
		 * on the passed mzML Spectrum object.
		 * @param mzMlSpectrum
		 * @throws JMzReaderException 
		 */
		public MzMlWrapperSpectrum(uk.ac.ebi.jmzml.model.mzml.Spectrum mzMlSpectrum) throws JMzReaderException {
			id = mzMlSpectrum.getId();
			
			// get the precursor information. If there are multiple precursors used
			// simply use the first one
			PrecursorList precursorList = mzMlSpectrum.getPrecursorList();
			
			if (precursorList == null || precursorList.getCount().intValue() < 1 || precursorList.getPrecursor().get(0).getSelectedIonList() == null) {
				mz = null;
				intensity = null;
				charge = null;
			}
			else {
				// check if there are selected ions
				List<CVParam> selectionParams = precursorList.getPrecursor().get(0).getSelectedIonList().getSelectedIon().get(0).getCvParam();
				
				CVParam mzParam = getParamFromGroup(selectionParams, MZML_PARAMS.SELECTED_MZ.getAccess());
				mz = mzParam != null ? Double.parseDouble(mzParam.getValue()) : null;
				
				CVParam intensParam = getParamFromGroup(selectionParams, MZML_PARAMS.PEAK_INTENSITY.getAccess());
				intensity = intensParam != null ? Double.parseDouble(intensParam.getValue()) : null;
				
				CVParam chargeParam = getParamFromGroup(selectionParams, MZML_PARAMS.CHARGE_STATE.getAccess());
				charge = chargeParam != null ? Integer.parseInt(chargeParam.getValue()) : null;
			}
			
			CVParam msLevelParam = getParamFromGroup(mzMlSpectrum.getCvParam(), MZML_PARAMS.MS_LEVEL.getAccess());
			msLevel = msLevelParam != null ? Integer.parseInt(msLevelParam.getValue()) : null;
			
			peakList = convertPeakList(mzMlSpectrum.getBinaryDataArrayList());
		}

		/**
		 * Converts the spectrum's peak list into
		 * a Map as defined by the Spectrum
		 * interface.
		 * @param binaryDataArrayList
		 * @return
		 * @throws JMzReaderException 
		 */
		private Map<Double, Double> convertPeakList(
				BinaryDataArrayList binaryDataArrayList) throws JMzReaderException {
			 // make sure the spectrum contains a m/z and an intensity array
	        BinaryDataArray mzArray = null, intenArray = null;

	        for (BinaryDataArray array : binaryDataArrayList.getBinaryDataArray()) {
	            // check the cvParams
	            for (CVParam param : array.getCvParam()) {
	                if (param.getAccession().equals("MS:1000514")) {
	                    mzArray = array;
	                    break;
	                }
	                if (param.getAccession().equals("MS:1000515")) {
	                    intenArray = array;
	                    break;
	                }
	            }

	            if (mzArray != null && intenArray != null)
	                break;
	        }

	        // if the spectrum doesn't contain a mz and binary array return an empty map
	        if (mzArray == null || intenArray == null)
	            return Collections.emptyMap();
	        
	        // get the values as numbers
	        Number mzNumbers[] = mzArray.getBinaryDataAsNumberArray();
	        ArrayList<Double> mzValues = new ArrayList<Double>(mzNumbers.length);

	        for (Number n : mzNumbers)
	            mzValues.add(n.doubleValue());
	        
	        Number intenNumbers[] = intenArray.getBinaryDataAsNumberArray();
	        ArrayList<Double> intenValues = new ArrayList<Double>(intenNumbers.length);
	        
	        for (Number n : intenNumbers)
	        	intenValues.add(n.doubleValue());
	        
	        // make sure both have the same size
	        if (intenValues.size() != mzValues.size())
	        	throw new JMzReaderException("Different sizes for m/z and intensity value arrays for spectrum " + id);
	        
	        // create the map
	        Map<Double, Double> peakList = new HashMap<Double, Double>(mzNumbers.length);
	        
	        for (int i = 0; i < mzNumbers.length; i++)
	        	peakList.put(mzValues.get(i), intenValues.get(i));
	        
	        return peakList;
		}

		public String getId() {
			return id;
		}

		public Integer getPrecursorCharge() {
			return charge;
		}

		public Double getPrecursorMZ() {
			return mz;
		}

		public Double getPrecursorIntensity() {
			return intensity;
		}

		public Map<Double, Double> getPeakList() {
			return peakList;
		}

		public Integer getMsLevel() {
			return msLevel;
		}
		
		/**
		 * Returns the (first) parameter with the given
		 * accession from the passed parameter group or
		 * null in case no parameter with the given
		 * accession exists.
		 * @param params
		 * @param accession
		 * @return The first CVParam identified by this accession or null in case no parameter with that accession exists.
		 */
		private CVParam getParamFromGroup(List<CVParam> params, String accession) {
			for (CVParam p : params) {
				if (p.getAccession().equals(accession))
					return p;
			}
			
			return null;
		}
	}
}
