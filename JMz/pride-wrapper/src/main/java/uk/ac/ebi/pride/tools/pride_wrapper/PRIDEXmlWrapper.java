package uk.ac.ebi.pride.tools.pride_wrapper;

import uk.ac.ebi.pride.jaxb.model.*;
import uk.ac.ebi.pride.jaxb.xml.PrideXmlReader;
import uk.ac.ebi.pride.tools.jmzreader.JMzReader;
import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import uk.ac.ebi.pride.tools.jmzreader.model.IndexElement;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.*;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.CvParam;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.UserParam;

import java.io.File;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.*;

/**
 * wrapper class on PRIDE JAXB library that implements
 * jmzReader interface
 *
 * @author Rui Wang
 * @version $Id$
 */
public class PRIDEXmlWrapper implements JMzReader {

    /**
     * PRIDE JAXB reader for reading pride xml file
     */
    private PrideXmlReader prideXmlReader;

    /**
     * A list of pre-loading spectrum ids
     */
    private List<String> spectrumIds;

    /**
     * Spectrum id to index map
     */
    private Map<String, IndexElement> spectrumIndices;

    /**
     * Spectrum index sorted according to their ms level
     */
    private Map<Integer, List<IndexElement>> msLevelIndices;


    public PRIDEXmlWrapper(File file) {
        this.prideXmlReader = new PrideXmlReader(file);
        this.spectrumIds = new ArrayList<String>(prideXmlReader.getSpectrumIds());
        convertIndices(prideXmlReader.getSpectrumIndices());
    }

    /**
     * Convert XXIndex index element to jmzReaer index element
     *
     * @param rawIndices XXIndex index element
     * @return Map<String, IndexElement>   jmzReader index element map
     */
    private void convertIndices(Map<String, psidev.psi.tools.xxindex.index.IndexElement> rawIndices) {
        spectrumIndices = new HashMap<String, IndexElement>();
        msLevelIndices = new HashMap<Integer, List<IndexElement>>();

        for (Map.Entry<String, psidev.psi.tools.xxindex.index.IndexElement> rawIndex : rawIndices.entrySet()) {
            psidev.psi.tools.xxindex.index.IndexElement rawIndexElement = rawIndex.getValue();
            long start = rawIndexElement.getStart();
            int size = (int) (rawIndexElement.getStop() - rawIndexElement.getStart());
            // new index element
            IndexElement indexElement = new IndexElementImpl(start, size);
            spectrumIndices.put(rawIndex.getKey(), indexElement);

            // ms level
            int msLevel = prideXmlReader.getSpectrumMsLevel(rawIndex.getKey());
            List<IndexElement> msLevelIndexElements = msLevelIndices.get(msLevel);
            if (msLevelIndexElements == null) {
                msLevelIndexElements = new ArrayList<IndexElement>();
                msLevelIndices.put(msLevel, msLevelIndexElements);
            }
            msLevelIndexElements.add(indexElement);
        }

    }

    /**
     * Get the number of spectra
     *
     * @return spectra number
     */
    @Override
    public int getSpectraCount() {
        return prideXmlReader.getSpectrumIds().size();
    }

    /**
     * Whether this wrapper accepts file as input
     *
     * @return true means files are accepted
     */
    @Override
    public boolean acceptsFile() {
        return true;
    }

    /**
     * Whether this wrapper accepts folder as input
     *
     * @return true means folders are accepted
     */
    @Override
    public boolean acceptsDirectory() {
        return false;
    }

    /**
     * Get a list of spectrum ids
     *
     * @return a list of spectrum ids
     */
    @Override
    public List<String> getSpectraIds() {
        return new ArrayList<String>(spectrumIds);
    }

    /**
     * Get spectrum using a given spectrum id
     *
     * @param id spectrum id
     * @return Spectrum object
     * @throws JMzReaderException exception while reading spectrum
     */
    @Override
    public Spectrum getSpectrumById(String id) throws JMzReaderException {
        uk.ac.ebi.pride.jaxb.model.Spectrum prideSpectrum = prideXmlReader.getSpectrumById(id);
        return new PRIDEXmlSpectrumWrapper(prideSpectrum);
    }

    /**
     * Get spectrum using its index in the file
     *
     * @param index spectrum index
     * @return Spectrum object
     * @throws JMzReaderException exception while reading spectrum
     */
    @Override
    public Spectrum getSpectrumByIndex(int index) throws JMzReaderException {
        if (index < 1 || index > prideXmlReader.getSpectrumIds().size())
            throw new JMzReaderException("Index out of range.");
        String spectrumId = spectrumIds.get(index);
        return getSpectrumById(spectrumId);
    }

    /**
     * Get a spectrum iterator
     *
     * @return an iterator on spectrum
     */
    @Override
    public Iterator<Spectrum> getSpectrumIterator() {
        return new PRIDEXmlSpectrumIterator();
    }

    /**
     * Get all the file binary positions on spectra of certain ms level
     *
     * @param msLevel ms level
     * @return list of index element
     */
    @Override
    public List<IndexElement> getMsNIndexes(int msLevel) {
        return new ArrayList<IndexElement>(msLevelIndices.get(msLevel));
    }

    /**
     * Get all existing ms levels form the source file
     *
     * @return a list of ms levels
     */
    @Override
    public List<Integer> getMsLevels() {
        return new ArrayList<Integer>(msLevelIndices.keySet());
    }

    /**
     * Get a map of spectrum ids to their file binary position
     *
     * @return map of id to binary position
     */
    @Override
    public Map<String, IndexElement> getIndexElementForIds() {
        return new HashMap<String, IndexElement>(spectrumIndices);
    }

    /**
     * Iterator over all spectra in the PRIDE XML
     * file returning Peak List Parser
     */
    private class PRIDEXmlSpectrumIterator implements Iterator<Spectrum> {
        private final Iterator<String> idIterator = spectrumIds.iterator();

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
     * Wrapper class
     */
    private static class PRIDEXmlSpectrumWrapper implements Spectrum {

        /**
         * Original PRIDE spectrum
         */
        private uk.ac.ebi.pride.jaxb.model.Spectrum prideSpectrum;

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
         * Additional parameters
         */
        private ParamGroup paramGroup;

        private PRIDEXmlSpectrumWrapper(uk.ac.ebi.pride.jaxb.model.Spectrum prideSpectrum) {
            this.prideSpectrum = prideSpectrum;

            // save the id
            this.id = prideSpectrum.getId();

            // set the ms level
            msLevel = prideSpectrum.getSpectrumDesc().getSpectrumSettings().getSpectrumInstrument().getMsLevel();

            // extract the precursor information
            extractPrecursorInformation();

            // extract the peak list
            extractPeakList();

            // extract additional
            extractAdditional();
        }

        private void extractPrecursorInformation() {
            // if there's no precursor list, return
            if (prideSpectrum.getSpectrumDesc().getPrecursorList() == null)
                return;

            // only process the first precursor
            Precursor precursor = prideSpectrum.getSpectrumDesc().getPrecursorList().getPrecursor().get(0);

            // parse the cvParams
            for (uk.ac.ebi.pride.jaxb.model.CvParam cvParam : precursor.getIonSelection().getCvParam()) {
                String accession = cvParam.getAccession();
                String value = cvParam.getValue();

                if ("PSI:1000040".equals(accession) || "MS:1000744".equals(accession))
                    precursorMz = Double.parseDouble(value);
                if ("PSI:1000041".equals(accession) || "MS:1000041".equals(accession))
                    precursorCharge = Integer.parseInt(value);
                if ("PSI:1000042".equals(accession) || "MS:1000042".equals(accession))
                    precursorIntensity = Double.parseDouble(value);
            }
        }

        private void extractPeakList() {
            // get the data
            Data mzData = prideSpectrum.getMzArrayBinary().getData();
            Data intenData = prideSpectrum.getIntenArrayBinary().getData();

            ByteBuffer mzBuffer = ByteBuffer.wrap(mzData.getValue());
            ByteBuffer intenBuffer = ByteBuffer.wrap(intenData.getValue());

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

        private void extractAdditional() {
            paramGroup = new ParamGroup();

            // get spectrum settings
            SpectrumDesc rawSpecDesc = prideSpectrum.getSpectrumDesc();
            uk.ac.ebi.pride.jaxb.model.SpectrumSettings rawSpecSettings = prideSpectrum.getSpectrumDesc().getSpectrumSettings();
            uk.ac.ebi.pride.jaxb.model.AcqSpecification rawActSpec = rawSpecSettings.getAcqSpecification();
            uk.ac.ebi.pride.jaxb.model.SpectrumInstrument rawSpecInstrument = rawSpecSettings.getSpectrumInstrument();

            // add ms level
            paramGroup.addParam(new CvParam("ms level", msLevel + "", "MS", "MS:1000449"));

            // add spectrum representation
            if (rawActSpec != null) {
                String rawSpecType = rawActSpec.getSpectrumType();
                if ("discrete".equals(rawSpecType)) {
                    paramGroup.addParam(new CvParam("centroid spectrum", rawSpecType, "MS", "MS:1000127"));
                } else if ("continuous".equals(rawSpecType)) {
                    paramGroup.addParam(new CvParam("profile spectrum", rawSpecType, "MS", "MS:1000128"));
                }
            }

            // add spectrum instrument
            List<uk.ac.ebi.pride.jaxb.model.CvParam> rawCvParams = rawSpecInstrument.getCvParam();
            if (rawCvParams != null) {
                for (uk.ac.ebi.pride.jaxb.model.CvParam rawCvParam : rawCvParams) {
                    paramGroup.addParam(new CvParam(rawCvParam.getName(), rawCvParam.getValue(), rawCvParam.getCvLabel(), rawCvParam.getAccession()));
                }
            }

            List<uk.ac.ebi.pride.jaxb.model.UserParam> rawUserParams = rawSpecInstrument.getUserParam();
            if (rawUserParams != null) {
                for (uk.ac.ebi.pride.jaxb.model.UserParam rawUserParam : rawUserParams) {
                    paramGroup.addParam(new UserParam(rawUserParam.getName(), rawUserParam.getValue()));
                }
            }

            // add comments
            List<String> comments = rawSpecDesc.getComments();
            for (String comment : comments) {
                paramGroup.addParam(new UserParam("comment", comment));
            }
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
}
