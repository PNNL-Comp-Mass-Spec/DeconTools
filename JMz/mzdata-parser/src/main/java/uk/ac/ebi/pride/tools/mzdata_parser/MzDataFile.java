package uk.ac.ebi.pride.tools.mzdata_parser;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.RandomAccessFile;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import psidev.psi.tools.xxindex.StandardXpathAccess;
import psidev.psi.tools.xxindex.index.IndexElement;
import psidev.psi.tools.xxindex.index.XpathIndex;
import uk.ac.ebi.pride.tools.jmzreader.JMzReader;
import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.IndexElementImpl;
import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.CvLookup;
import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.MzData.Description;
import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.MzDataElement;
import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.unmarshaller.MzDataUnmarshaller;
import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.unmarshaller.MzDataUnmarshallerFactory;

public class MzDataFile implements JMzReader {
	/**
	 * The mzXML source file.
	 */
	private File sourcefile;
	/**
	 * The random access file object is
	 * used to read the file. This object
	 * is only opened once.
	 */
	private RandomAccessFile accessFile;
	/**
	 * The actual XPath index to use.
	 */
	private XpathIndex index;
	/**
	 * The XPath access to use.
	 */
	private StandardXpathAccess xpathAccess;
	/**
	 * A Map with the ms level as key and the
	 * list with associated index elements as
	 * values.
	 */
	private Map<Integer, List<IndexElement>> msNScans;
	/**
	 * The spectra ids in the order
	 * they appear in the file.
	 */
	private List<String> spectraIds;
	/**
	 * Holds the attributes of the mzData element.
	 */
	private HashMap<String, String> mzDataAttributes;
	/**
	 * Pattern used to extract xml attribute name
	 * value pairs.
	 */
	private static final Pattern xmlAttributePattern = Pattern.compile("(\\w+)=\"([^\"]*)\"");
	/**
	 * Map with the spectra's index attribute as key
	 * and the corresponding IndexElement as value.
	 */
	private Map<Integer, IndexElement> idToIndexElementMap;
	/**
	 * The unmarshaller to use.
	 */
	MzDataUnmarshaller unmarshaller;
	
	/**
	 * Creates a new MzXMLFile object based
	 * on the given mzXML file.
	 * @param sourcefile The mzXML file to parse.
	 * @throws JMzReaderException
	 */
	public MzDataFile(File sourcefile) throws JMzReaderException {
		this.sourcefile = sourcefile;
		
		// index the file
		indexFile();
		
		// create the unmarshaller
		unmarshaller = MzDataUnmarshallerFactory.getInstance().initializeUnmarshaller();
		
		// read the mzData attributes
		readMzDataAttributes();
		
		// initialize the spectra maps
		initializeSpectraMaps();
	}
	
	/**
	 * Retrieves a spectrum who's position in the file is
	 * known. The passed sourcefile is not indexed thus allowing
	 * fast and efficient access.
	 * 
	 * @param sourcefile The file to read the spectrum from.
	 * @param indexElement An IndexElement specifying the spectrum's (byte) position in the file.
	 * @return The unmarshalled Spectrum object.
	 * @throws JMzReaderException
	 */
	public static Spectrum getIndexedSpectrum(File sourcefile, uk.ac.ebi.pride.tools.jmzreader.model.IndexElement indexElement) throws JMzReaderException {
		try {
			// read the snipplet from the file 
			RandomAccessFile accessFile = new RandomAccessFile(sourcefile, "r");
			byte[] buffer = new byte[indexElement.getSize()];
			
			accessFile.seek(indexElement.getStart());
			accessFile.read(buffer);
			
			String snipplet = new String(buffer);
			
			// create the unmarshaller
			MzDataUnmarshaller unmarshaller = MzDataUnmarshallerFactory.getInstance().initializeUnmarshaller();
			
			// unmarshal the spectrum object
			uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum mzDataSpectrum = unmarshaller.unmarshal(snipplet, MzDataElement.SPECTRUM);
			
			return new MzDataSpectrum(mzDataSpectrum);
			
		} catch (IOException e) {
			throw new JMzReaderException("Failed to read from mzData file.", e);
		} catch (Exception e) {
			throw new JMzReaderException("Failed to unmarshal mzData spectrum object.", e);
		}
	}
	
	/**
	 * Initializes the idToIndexElement
	 * map.
	 * @throws JMzReaderException 
	 */
	private void initializeSpectraMaps() throws JMzReaderException {
		List<IndexElement> spectra = index.getElements(MzDataElement.SPECTRUM.getXpath());
		
		idToIndexElementMap = new HashMap<Integer, IndexElement>(spectra.size());
		spectraIds = new ArrayList<String>(spectra.size());
		msNScans = new HashMap<Integer, List<IndexElement>>(spectra.size());
		
		for (IndexElement spectrum : spectra) {
			// read the attributes
			Map<String, String> attributes = readElementAttributes(spectrum);
			int msLevel = readSpectrumMsLevel(spectrum);
			
			if (!attributes.containsKey("id"))
				throw new JMzReaderException("Spectrum element with missing id attribute at line " + spectrum.getLineNumber());
			
			idToIndexElementMap.put(Integer.parseInt( attributes.get("id") ), spectrum);
			spectraIds.add(attributes.get("id"));
			
			if (!msNScans.containsKey(msLevel))
				msNScans.put(msLevel, new ArrayList<IndexElement>());
			
			msNScans.get(msLevel).add(spectrum);
		}		
	}

	/**
	 * Reads the attributes of the mzData
	 * element.
	 * @throws JMzReaderException
	 */
	private void readMzDataAttributes() throws JMzReaderException {
		RandomAccessFile access = getRandomAccess();
		
		// process the file line by line
		try {
			// initialize the run attributes
			mzDataAttributes = new HashMap<String, String>();
			
			// go to the beginning of the file
			access.seek(0);
			
			String line;
			
			while ((line = access.readLine()) != null) {
				// check if it's the "msRun" line
				if (line.contains("<mzData")) {
					break;
				}
			}
			
			// make sure the run line was found
			if (line == null) 
				return;
			
			// parse the line
			Matcher matcher = xmlAttributePattern.matcher(line);
			
			while (matcher.find()) {
				String name = matcher.group(1);
				String value = matcher.group(2);
				
				if (name != null && value != null)
					mzDataAttributes.put(name, value);
			}
		} catch (IOException e) {
			throw new JMzReaderException("Failed to read mzData file.", e);
		}
	}

	/**
	 * Indexes the current sourcefile and creates the
	 * index and xpathAccess objects.
	 * @throws JMzReaderException Thrown when the sourcefile cannot be accessed.
	 */
	private void indexFile() throws JMzReaderException {
		try {
			// build the xpath
			xpathAccess = new StandardXpathAccess(sourcefile, MzDataElement.getXpaths());
			
			// save the index
			index = xpathAccess.getIndex();
		} catch (IOException e) {
			throw new JMzReaderException("Failed to index mzData file.", e);
		}		
	}
	
	/**
	 * Extracts the spectrum's msLevel from the file without
	 * unmarshalling the whole mzData spectrum.
	 * @param indexElement IndexElement specifying the spectrum's position.
	 * @return The spectrum's msLevel,
	 * @throws JMzReaderException
	 */
	private Integer readSpectrumMsLevel(IndexElement indexElement) throws JMzReaderException {
RandomAccessFile access = getRandomAccess();
		
		// process the file line by line
		try {			
			// go to the beginning of element
			access.seek(indexElement.getStart());
			
			// just read the beginning of the element
			byte[] headerBuffer = new byte[500];
			
			access.read(headerBuffer);
			
			String headerString = new String(headerBuffer);
			
			// make sure the msLevel was found
			int msLevelIndex = -1;
			
			do {
				msLevelIndex = headerString.indexOf("msLevel=\"");
				
				// if the msLevel was found make sure enough data is there
				if (msLevelIndex > -1) {
					int msLevelEnd = headerString.indexOf('"', msLevelIndex + 10);
					
					if (msLevelEnd > -1) {
						return Integer.parseInt(headerString.substring(msLevelIndex + 9, msLevelEnd));
					}
				}
			} while (headerString.length() < indexElement.getStop() - indexElement.getStart());
			
			return -1;
		} catch (IOException e) {
			throw new JMzReaderException("Failed to read mzData file.", e);
		}
	}
	
	/**
	 * Reads the given element's attributes and returns
	 * them as a Map with the attribute's name as key
	 * and its value as value.
	 * @param indexElement
	 * @return
	 * @throws JMzReaderException
	 */
	private Map<String, String> readElementAttributes(IndexElement indexElement) throws JMzReaderException {
		RandomAccessFile access = getRandomAccess();
		
		// process the file line by line
		try {
			// initialize the run attributes
			HashMap<String, String> foundAttributes = new HashMap<String, String>();
			
			// go to the beginning of element
			access.seek(indexElement.getStart());
			
			// just read the beginning of the element (250 elements should be sufficient)
			byte[] headerBuffer = new byte[250];
			
			access.read(headerBuffer);
			
			String headerString = new String(headerBuffer);
			
			// make sure the whole header was retrieved
			while (!headerString.contains(">")) {
				// read another header string
				access.seek(indexElement.getStart() + headerString.length());
				
				access.read(headerBuffer);
				
				headerString += new String(headerBuffer);
			}
			
			// remove all new line characters
			headerString = headerString.replace("\n", "");
			
			// remove everything after the first ">"
			headerString = headerString.substring(0, headerString.indexOf('>') + 1);
			
			// parse the line
			Matcher matcher = xmlAttributePattern.matcher(headerString);
			
			while (matcher.find()) {
				String name = matcher.group(1);
				String value = matcher.group(2);
				
				if (name != null && value != null)
					foundAttributes.put(name, value);
			}
			
			return foundAttributes;
		} catch (IOException e) {
			throw new JMzReaderException("Failed to read mzData file.", e);
		}
	}
	
	/**
	 * Returns the random access file object to access
	 * the source file.
	 * @return
	 * @throws JMzReaderException
	 */
	private RandomAccessFile getRandomAccess() throws JMzReaderException {
		if (accessFile != null)
			return accessFile;
		
		try {
			accessFile = new RandomAccessFile(sourcefile, "r");
		} catch (FileNotFoundException e) {
			throw new JMzReaderException("Could not find mzData file '" + sourcefile.getPath() + "'", e);
		}
		
		return accessFile;
	}
	
	/**
	 * Reads a given XML Snipplet from the file and returns
	 * it as a String.
	 * @param indexElement An IndexElement specifying the position to read.
	 * @return
	 * @throws JMzReaderException
	 */
	private String readSnipplet(IndexElement indexElement) throws JMzReaderException {
		// read the XML from the file
		RandomAccessFile access = getRandomAccess();
		
		// calculate the snipplets length
		int length = (int) (indexElement.getStop() - indexElement.getStart());
		
		// create the byte buffer
		byte[] bytes = new byte[length];
		
		try {
			// move to the position in the file
			access.seek(indexElement.getStart());
			
			// read the snipplet
			access.read(bytes);
			
			// create and return the string
			String snipplet = new String(bytes);
			return snipplet;
			
		} catch (IOException e) {
			throw new JMzReaderException("Failed to read from mzData file.", e);
		}
	}
	
	/**
	 * Returns the mzData object's attributes
	 * as a Map with the attribute's name as
	 * key and its value as value.
	 * @return
	 */
	public Map<String, String> getMzDataAttributes() {
		return mzDataAttributes;
	}
	
	/**
	 * Returns a list of all CvLookup objects in
	 * the mzData file.
	 * @return
	 * @throws JMzReaderException
	 */
	public List<CvLookup> getCvLookups() throws JMzReaderException {
		try {
			List<CvLookup> cvLookups = new ArrayList<CvLookup>();
			
			List<IndexElement> indexElements = index.getElements(MzDataElement.CV_LOOKUP.getXpath());
			
			for (IndexElement e : indexElements) {
				String xml = readSnipplet(e);
				CvLookup cvLookup = unmarshaller.unmarshal(xml, MzDataElement.CV_LOOKUP);
				cvLookups.add(cvLookup);
			}
			
			return cvLookups;
		}
		catch (Exception e) {
			throw new JMzReaderException("Failed to unmarshall cvLookup object.", e);
		}
	}
	
	/**
	 * Returns the Description object of the
	 * mzData file. This object contains all meta-data
	 * found in the mzData file.
	 * @return
	 * @throws JMzReaderException 
	 */
	public Description getDescription() throws JMzReaderException {
		try {
			IndexElement indexElement = index.getElements(MzDataElement.DESCRIPTION.getXpath()).get(0);
			String xml = readSnipplet(indexElement);
			uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.MzData.Description description = unmarshaller.unmarshal(xml, MzDataElement.DESCRIPTION);
			
			return description;
		}
		catch (Exception e) {
			throw new JMzReaderException("Failed to unmarshall Desription object.", e);
		}
	}
	
	@Override
	public int getSpectraCount() {
		return spectraIds.size();
	}

	@Override
	public boolean acceptsFile() {
		return true;
	}

	@Override
	public boolean acceptsDirectory() {
		return false;
	}

	@Override
	public List<String> getSpectraIds() {
		return new ArrayList<String>(spectraIds);
	}
	
	/**
	 * Reads the given spectrum from the file and
	 * returns the respective mzData Spectrum object.
	 * @param id The spectrum's id
	 * @return The spectrum as an mzData Spectrum object.
	 * @throws JMzReaderException
	 */
	public uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum getMzDataSpectrumById(int id) throws JMzReaderException {
		// make sure the spectrum exists
		if (!idToIndexElementMap.containsKey(id))
			throw new JMzReaderException("Spectrum with id '" + id + "' does not exist.");
		
		// get the index element
		IndexElement indexElement = idToIndexElementMap.get(id);
		
		if (indexElement == null)
			throw new JMzReaderException("Failed to resolved id.");
		
		// get the snipplet
		String xml = readSnipplet(indexElement);
		
		// unmarshall the object
		try {
			uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum mzDataSpectrum = unmarshaller.unmarshal(xml, MzDataElement.SPECTRUM);
			
			return mzDataSpectrum;
		} catch (Exception e) {
			throw new JMzReaderException("Failed to unmarshal spectrum", e);
		}
	}

	@Override
	public Spectrum getSpectrumById(String id) throws JMzReaderException {
		uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum mzDataSpectrum = getMzDataSpectrumById(Integer.parseInt(id));
		
		return new MzDataSpectrum(mzDataSpectrum);
	}
	
	/**
	 * Retrieves the spectrum based on its position
	 * in the file and returns it as an mzData Spectrum
	 * object.
	 * @param index The spectrum's 1-based index in the file.
	 * @return The spectrum as a mzData Spectrum object.
	 * @throws JMzReaderException
	 */
	public uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum getMzDataSpectrumByIndex(int index)
		throws JMzReaderException {		
		if (index < 1 || index > spectraIds.size())
			throw new JMzReaderException("Spectrum index " + index + " out of range. " +  spectraIds.size() + " spectra loaded.");
		
		// get the index element
		String id 					= spectraIds.get(index - 1);
		IndexElement indexElement 	=  idToIndexElementMap.get(Integer.parseInt(id));
		
		// get the snipplet
		String xml = readSnipplet(indexElement);
		
		// unmarshall the object
		try {
			uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum mzDataSpectrum = unmarshaller.unmarshal(xml, MzDataElement.SPECTRUM);
			
			return mzDataSpectrum;
		} catch (Exception e) {
			throw new JMzReaderException("Failed to unmarshal spectrum", e);
		}
	}

	@Override
	public Spectrum getSpectrumByIndex(int index)
			throws JMzReaderException {
		uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum mzDataSpectrum = getMzDataSpectrumByIndex(index);
		
		return new MzDataSpectrum(mzDataSpectrum);
	}

	@Override
	public Iterator<Spectrum> getSpectrumIterator() {
		return new SpectrumIterator();
	}
	
	@Override
	public List<uk.ac.ebi.pride.tools.jmzreader.model.IndexElement> getMsNIndexes(
			int msLevel) {
		if (!msNScans.containsKey(msLevel))
			return Collections.emptyList();
		
		return convertIndexElements(msNScans.get(msLevel));
	}

	@Override
	public List<Integer> getMsLevels() {
		return new ArrayList<Integer>(msNScans.keySet());
	}

	@Override
	public Map<String, uk.ac.ebi.pride.tools.jmzreader.model.IndexElement> getIndexElementForIds() {
		Map<String, uk.ac.ebi.pride.tools.jmzreader.model.IndexElement> idToIndex = 
			new HashMap<String, uk.ac.ebi.pride.tools.jmzreader.model.IndexElement>(idToIndexElementMap.size());
		
		for (Integer id : idToIndexElementMap.keySet()) {
			IndexElement e = idToIndexElementMap.get(id);
			int size = (int) (e.getStop() - e.getStart());
			idToIndex.put(id.toString(), new IndexElementImpl(e.getStart(), size));
		}	
		
		return idToIndex;
	}
	
	/**
	 * Converts a list of xxindex IndexElementS to a
	 * list of JMzReader IndexElementS.
	 * @param index
	 * @return
	 */
	private List<uk.ac.ebi.pride.tools.jmzreader.model.IndexElement> convertIndexElements(List<IndexElement> index) {
		List<uk.ac.ebi.pride.tools.jmzreader.model.IndexElement> convertedIndex = 
			new ArrayList<uk.ac.ebi.pride.tools.jmzreader.model.IndexElement>(index.size());
		
		for (IndexElement e : index) {
			int size = (int) (e.getStop() - e.getStart());
			convertedIndex.add(new IndexElementImpl(e.getStart(), size));
		}
		
		return convertedIndex;
	}

	/**
	 * Returns an iterator over all spectra
	 * in the mzData file that returns mzData
	 * spectrum objects.
	 * @return
	 */
	public Iterator<uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum> getMzDataSpectrumIterator() {
		return new MzDataSpectrumIterator();
	}
	
	/**
	 * This is basically only a simple wrapper
	 * around the MzDataSpectrumIterator that returns
	 * PeakListParser Spectrum Objects instead of
	 * mzData Spectrum objects. 
	 * @author jg
	 *
	 */
	private class SpectrumIterator implements Iterator<Spectrum> {
		private final MzDataSpectrumIterator iterator = new MzDataSpectrumIterator();
		
		@Override
		public boolean hasNext() {
			return iterator.hasNext();
		}

		@Override
		public Spectrum next() {
			uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum spectrum = iterator.next();
			
			return new MzDataSpectrum(spectrum);
		}

		@Override
		public void remove() {
			// not supported		
		}
	}

	/**
	 * Iterator over all spectra in the mzData file
	 * and returns mzData Spectrum objects.
	 * @author jg
	 *
	 */
	private class MzDataSpectrumIterator implements Iterator<uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum> {
		/**
		 * Iterator over all spectrum IndexElements.
		 * Most functions are simply passed on to this
		 * iterator.
		 */
		private final Iterator<String> idIterator = spectraIds.iterator();
		
		@Override
		public boolean hasNext() {
			return idIterator.hasNext();
		}

		@Override
		public uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum next() {
			String id = idIterator.next();
			IndexElement indexElement = idToIndexElementMap.get(Integer.parseInt(id));
			
			try {
				String xml = readSnipplet(indexElement);
				
				uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum spectrum = unmarshaller.unmarshal(xml, MzDataElement.SPECTRUM);
				
				return spectrum;
			} catch (Exception e) {
				throw new RuntimeException("Failed to load spectrum from mzData file.", e);
			}
		}

		@Override
		public void remove() {
			// not supported			
		}
	}
}
