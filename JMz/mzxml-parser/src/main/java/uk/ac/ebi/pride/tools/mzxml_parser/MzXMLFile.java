package uk.ac.ebi.pride.tools.mzxml_parser;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.IOException;
import java.io.RandomAccessFile;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.nio.FloatBuffer;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;
import java.util.zip.DataFormatException;
import java.util.zip.Inflater;

import psidev.psi.tools.xxindex.StandardXpathAccess;
import psidev.psi.tools.xxindex.index.IndexElement;
import psidev.psi.tools.xxindex.index.XpathIndex;
import uk.ac.ebi.pride.tools.jmzreader.JMzReader;
import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.IndexElementImpl;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.DataProcessing;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.MsInstrument;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.MzXMLObject;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.MzXmlElement;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.ParentFile;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.Peaks;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.Scan;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.Separation;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.Spotting;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.unmarshaller.MzXMLUnmarshaller;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.unmarshaller.MzXMLUnmarshallerFactory;

/**
 * The main class to parse mzXML files.
 * This objects represents a mzXML file and
 * provides all the required functions to access
 * the files elements.
 * @author jg
 *
 */
public class MzXMLFile implements JMzReader {
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
	 * The indexes of all level 1
	 * scans in the mzXML file.
	 */
	private List<IndexElement> level1ScanIndexes;
	/**
	 * The indexes of all level 2
	 * scans in the mzXML file.
	 */
	private List<IndexElement> level2ScanIndexes;
	/**
	 * A Map with the ms level as key and the
	 * list with associated index elements as
	 * values.
	 */
	private Map<Integer, List<IndexElement>> msNScans;
	/**
	 * Holds the attributes of the run attribute.
	 */
	private HashMap<String, String> runAttributes;
	/**
	 * Pattern used to extract xml attribute name
	 * value pairs.
	 */
	private static final Pattern xmlAttributePattern = Pattern.compile("(\\w+)=\"([^\"]*)\"");
	/**
	 * Map with the spectra's num attribute as key
	 * and the corresponding IndexElement as value.
	 */
	private Map<Long, IndexElement> numToIndexMap;
	/**
	 * The unmarshaller to use.
	 */
	private MzXMLUnmarshaller unmarshaller;
	/**
	 * Creates a new MzXMLFile object based
	 * on the given mzXML file.
	 * @param sourcefile The mzXML file to parse.
	 * @throws MzXMLParsingException
	 */
	public MzXMLFile(File sourcefile) throws MzXMLParsingException {
		this.sourcefile = sourcefile;
		
		// index the file
		indexFile();
		
		// create the unmarshaller
		unmarshaller = MzXMLUnmarshallerFactory.getInstance().initializeUnmarshaller();
		
		// save the scan indexes
		level1ScanIndexes = index.getElements(MzXmlElement.SCAN_LEVEL1.getXpath());
		level2ScanIndexes = index.getElements(MzXmlElement.SCAN_LEVEL2.getXpath());
		
		// read the ms run attributes
		readMsRunAttributes();
		
		// build the spectra maps
		buildSpectraMaps();
		
		// build the MS N indexes
		buildMsNIndexes();
	}
	
	/**
	 * Creates the maps between the spectra numbers
	 * and their IndexElements
	 */
	private void buildSpectraMaps() throws MzXMLParsingException {
		// make sure the spectra indexes were loaded
		if (level1ScanIndexes == null || level2ScanIndexes == null)
			return;
		
		// initialize the map
		numToIndexMap = new HashMap<Long, IndexElement>(level1ScanIndexes.size() + level2ScanIndexes.size());
		
		for (IndexElement indexElement : level1ScanIndexes) {
			// get the attributes
			Map<String, String> attributes = readElementAttributes(indexElement);
			
			// make sure there's a num attribute
			if (!attributes.containsKey("num"))
				continue;
			
			Long num = Long.parseLong(attributes.get("num"));
			
			numToIndexMap.put(num, indexElement);
		}
		
		// process the level 2 indexes		
		for (IndexElement indexElement : level2ScanIndexes) {
			// get the attributes
			Map<String, String> attributes = readElementAttributes(indexElement);
			
			// make sure there's a num attribute
			if (!attributes.containsKey("num"))
				continue;
			
			Long num = Long.parseLong(attributes.get("num"));
			
			numToIndexMap.put(num, indexElement);
		}
	}
	
	/**
	 * Reads the given element's attributes and returns
	 * them as a Map with the attribute's name as key
	 * and its value as value.
	 * @param indexElement
	 * @return
	 * @throws MzXMLParsingException
	 */
	private Map<String, String> readElementAttributes(IndexElement indexElement) throws MzXMLParsingException {
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
			throw new MzXMLParsingException("Failed to read mzXML file.", e);
		}
	}

	/**
	 * Reads the msRun attributes in the
	 * runAttributes HashMap.
	 */
	private void readMsRunAttributes() throws MzXMLParsingException {
		RandomAccessFile access = getRandomAccess();
		
		// process the file line by line
		try {
			// initialize the run attributes
			runAttributes = new HashMap<String, String>();
			
			// go to the beginning of the file
			access.seek(0);
			
			String line;
			
			while ((line = access.readLine()) != null) {
				// check if it's the "msRun" line
				if (line.contains("<msRun")) {
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
					runAttributes.put(name, value);
			}
		} catch (IOException e) {
			throw new MzXMLParsingException("Failed to read mzXML file.", e);
		}
	}

	/**
	 * Indexes the current sourcefile and creates the
	 * index and xpathAccess objects.
	 * @throws MzXMLParsingException Thrown when the sourcefile cannot be accessed.
	 */
	private void indexFile() throws MzXMLParsingException {
		try {
			// build the xpath
			xpathAccess = new StandardXpathAccess(sourcefile, MzXmlElement.getXpaths());
			
			// save the index
			index = xpathAccess.getIndex();
		} catch (IOException e) {
			throw new MzXMLParsingException("Failed to index mzXML file.", e);
		}		
	}
	
	/**
	 * Builds the msNIndex.
	 * @throws MzXMLParsingException
	 */
	private void buildMsNIndexes() throws MzXMLParsingException {
		// initialize the index
		msNScans = new HashMap<Integer, List<IndexElement>>();
		
		for (IndexElement element : level1ScanIndexes) {
			// get the attributes
			Map<String, String> attributes = readElementAttributes(element);
			
			// make sure there's a ms level set
			if (!attributes.containsKey("msLevel"))
				continue;
			
			Integer msLevel = Integer.parseInt(attributes.get("msLevel"));
			
			if (!msNScans.containsKey(msLevel))
				msNScans.put(msLevel, new ArrayList<IndexElement>(1));
			
			msNScans.get(msLevel).add(element);
		}
		
		for (IndexElement element : level2ScanIndexes) {
			// get the attributes
			Map<String, String> attributes = readElementAttributes(element);
			
			// make sure there's a ms level set
			if (!attributes.containsKey("msLevel"))
				continue;
			
			Integer msLevel = Integer.parseInt(attributes.get("msLevel"));
			
			if (!msNScans.containsKey(msLevel))
				msNScans.put(msLevel, new ArrayList<IndexElement>(1));
			
			msNScans.get(msLevel).add(element);
		}
	}

	@Override
	protected void finalize() throws Throwable {
		// close the file access if there is one
		if (accessFile != null)
			accessFile.close();
		
		super.finalize();
	}
	
	public static Spectrum getIndexedSpectrum(File sourcefile, uk.ac.ebi.pride.tools.jmzreader.model.IndexElement indexElement) throws JMzReaderException {
		try {
			// read the XML from the file
			RandomAccessFile access = new RandomAccessFile(sourcefile, "r");
			
			// create the byte buffer
			byte[] bytes = new byte[indexElement.getSize()];
			
			// move to the position in the file
			access.seek(indexElement.getStart());
			
			// read the snipplet
			access.read(bytes);
			
			// create and return the string
			String snipplet = new String(bytes);
			
			MzXMLUnmarshaller localUnmarshaller = MzXMLUnmarshallerFactory.getInstance().initializeUnmarshaller();
			
			Scan scan = localUnmarshaller.unmarshal(snipplet, MzXmlElement.SCAN_LEVEL1);
			
			return new MzXMLSpectrum(scan);
		} catch (Exception e) {
			throw new JMzReaderException("Failed to read from mzXML file.", e);
		}
	}

	/**
	 * Returns the random access file object to access
	 * the source file.
	 * @return
	 * @throws MzXMLParsingException
	 */
	private RandomAccessFile getRandomAccess() throws MzXMLParsingException {
		if (accessFile != null)
			return accessFile;
		
		try {
			accessFile = new RandomAccessFile(sourcefile, "r");
		} catch (FileNotFoundException e) {
			throw new MzXMLParsingException("Could not find mzXML file '" + sourcefile.getPath() + "'", e);
		}
		
		return accessFile;
	}
	
	/**
	 * Retrieves the parent files specified in the mzXML file.
	 * @return List of ParentFiles
	 * @throws MzXMLParsingException
	 */
	public List<ParentFile> getParentFile() throws MzXMLParsingException {
		List<ParentFile> parentFiles = unmarshalList(MzXmlElement.PARENT_FILE);
		
		return parentFiles;
	}
	
	/**
	 * Retrieves the msInstruments from the file.
	 * @return
	 * @throws MzXMLParsingException
	 */
	public List<MsInstrument> getMsInstrument() throws MzXMLParsingException {
		List<MsInstrument> instruments = unmarshalList(MzXmlElement.MS_INSTRUMENT);
		
		return instruments;
	}
	
	/**
	 * Retrieves the data processings from the file.
	 * @return List of DataProcessing.
	 * @throws MzXMLParsingException
	 */
	public List<DataProcessing> getDataProcessing() throws MzXMLParsingException {
		List<DataProcessing> dataProcessings = unmarshalList(MzXmlElement.DATA_PROCESSING);
		
		return dataProcessings;
	}
	
	/**
	 * Retrieves the separation object.
	 * @return A Separation object or NULL in case it does not exist.
	 * @throws MzXMLParsingException
	 */
	public Separation getSpearation() throws MzXMLParsingException {
		return unmarshalFirstElement(MzXmlElement.SEPARATION);
	}
	
	/**
	 * Retrieves the spotting object.
	 * @return A Spotting object of NULL in case it does not exist.
	 * @throws MzXMLParsingException
	 */
	public Spotting getSpotting() throws MzXMLParsingException {
		return unmarshalFirstElement(MzXmlElement.SPOTTING);
	}
	
	/**
	 * Extracts the peak list from the given
	 * Peaks object and returns it as a Map with
	 * the m/z as key and the intensity as value.
	 * @param peaks A peaks object.
	 * @return Map containing the m/z as key and the intensity as value.
	 */
	public static Map<Double, Double> convertPeaksToMap(Peaks peaks) throws MzXMLParsingException {
		// make sure the scan is not null
		if (peaks == null || peaks.getValue() == null)
			return Collections.emptyMap();
		
		// wrap the data with a ByteBuffer
		ByteBuffer byteBuffer = ByteBuffer.wrap(peaks.getValue());
		
		// check if the string is compressed
		boolean zlibCompression = (peaks.getCompressionType() != null && "zlib".equalsIgnoreCase(peaks.getCompressionType()));
		
		// handle compressed peak lists
		if (zlibCompression) {
			Inflater decompresser = new Inflater();
			
			// TODO: make sure the call to .array() is working
			decompresser.setInput(byteBuffer.array());
			
			// allocate 10x the memory of the original one
			byte[] decompressedData = new byte[byteBuffer.capacity() * 10];
			
			try {
				int usedLength = decompresser.inflate(decompressedData);
				
				// save it as the new byte buffer
				byteBuffer = ByteBuffer.wrap(decompressedData, 0, usedLength);
			} catch (DataFormatException e) {
				throw new MzXMLParsingException("Failed to decompress spectra data.", e);
			}
		}
		
		if ("network".equalsIgnoreCase(peaks.getByteOrder()))
			byteBuffer.order(ByteOrder.BIG_ENDIAN);
		else
			throw new MzXMLParsingException("Peak lists must be encoded using network (big-endian) byte order");
		
		double[] values;
		
		// get the precision
		if (peaks.getPrecision() != null && peaks.getPrecision() == 64) {
			values = new double[byteBuffer.asDoubleBuffer().capacity()];
			byteBuffer.asDoubleBuffer().get(values);
		}
		// if no precision is set, expect 32bit
		else {
			// need to convert the floats to doubles
			FloatBuffer floats = byteBuffer.asFloatBuffer();
			values = new double[floats.capacity()];
			
			for (int index = 0; index < floats.capacity(); index++)
				values[index] = new Double(floats.get(index));
		} 
		
		// make sure there's an even number of values (2 for every peak)
		if (values.length % 2 > 0)
			throw new MzXMLParsingException("Different number of m/z and intensity values encountered in peak list.");
		
		// create the Map
		HashMap<Double, Double> peakList = new HashMap<Double, Double>(values.length / 2);
		
		for (int peakIndex = 0; peakIndex < values.length - 1; peakIndex += 2) {
			// get the two value
			Double mz = values[peakIndex];
			Double intensity = values[peakIndex + 1];
			
			peakList.put(mz, intensity);
		}
		
		return peakList;
	}
	
	/**
	 * Unmarshals a given scan based on its num attribute.
	 * This function can only retrieve up to level 2 scans.
	 * Scans that are contained within a scan that's 
	 * contained within a scan are not accessible via this
	 * function. In case the scan is not available (either
	 * > level 2 or the number does not exist) null is returned.
	 * @param scanNum The scan's number.
	 * @return A Scan object or null in case the scan is not available.
	 * @throws MzXMLParsingException
	 */
	public Scan getScanByNum(Long scanNum) throws MzXMLParsingException {
		// get the index element
		IndexElement indexElement = null;
		
		// get the index element
		if (numToIndexMap.containsKey(scanNum))
			indexElement = numToIndexMap.get(scanNum);
		
		// in case the spectrum isn't indexed, return null
		if (indexElement == null)
			throw new MzXMLParsingException("Element with num=\"" + scanNum + "\" could not be found.");
		
		// read the snipplet
		String snipplet = readSnipplet(indexElement);
		
		// unmarshal the scan object
		try {
			Scan scan = unmarshaller.unmarshal(snipplet, MzXmlElement.SCAN_LEVEL1);
			
			return scan;
		} catch (Exception e) {
			throw new MzXMLParsingException("Failed to unmarshl Scan object.", e);
		}
	}
	
	/**
	 * Unmarshals a given scan based on its num attribute.
	 * This function can only retrieve up to level 2 scans.
	 * Scans that are contained within a scan that's 
	 * contained within a scan are not accessible via this
	 * function. In case the scan is not available (either
	 * > level 2 or the number does not exist) null is returned.
	 * @param scanNum The scan's number as a string.
	 * @return A Scan object or null in case the scan is not available.
	 * @throws MzXMLParsingException
	 */
	public Scan getScanByStringNum(String scanNum) throws MzXMLParsingException {
		// convert the number to a long
		try {
			Long num = Long.parseLong(scanNum);
			
			return getScanByNum(num);
		}
		catch (NumberFormatException e) {
			throw new MzXMLParsingException("Invalid spectra number passed.", e);
		}
	}
	
	/**
	 * Returns a list of all scan numbers found
	 * in the file.
	 * @return A List of Longs representing the scan numbers.
	 */
	public List<Long> getScanNumbers() {
		// initialize the return variable
		ArrayList<Long> scanNumbers = new ArrayList<Long>(numToIndexMap.keySet());
		Collections.sort(scanNumbers);
		
		return scanNumbers;
	}
	
	/**
	 * Unmarshals an MzXMLElement as a list.
	 * @param <T>
	 * @param element The MzXMLElement to unmarshal.
	 * @return A List holding the unmarshaled objects.
	 * @throws MzXMLParsingException
	 */
	private <T extends MzXMLObject> List<T> unmarshalList(MzXmlElement element) throws MzXMLParsingException{
		try {
			// read the parent file from the index
			List<IndexElement> parentFileIndex = index.getElements(element.getXpath());
			
			// initialize the list of parent files
			ArrayList<T> objects = new ArrayList<T>(parentFileIndex.size());
			
			for (IndexElement indexElement : parentFileIndex) {
				// read the xml snipplet
				String xmlSnipplet = readSnipplet(indexElement);
				
				// unmarshal the object
				T object = unmarshaller.unmarshal(xmlSnipplet, element);
				
				objects.add(object);
			}
			
			return objects;
		}
		catch (Exception e) {
			throw new MzXMLParsingException("Failed to unmarshall mzXML object.", e);
		}
	}
	
	/**
	 * Unmarshalls the first object of the passed type. In
	 * case the object does not exist NULL is returned.
	 * @param <T>
	 * @param element
	 * @return An object of type T or NULL in case no such object exists.
	 * @throws MzXMLParsingException
	 */
	private <T extends MzXMLObject> T unmarshalFirstElement(MzXmlElement element) throws MzXMLParsingException{
		try {
			// read the parent file from the index
			List<IndexElement> parentFileIndex = index.getElements(element.getXpath());
			
			// make sure at least one index was found
			if (parentFileIndex.size() < 1)
				return null;
			
			// get the first element
			String xmlSnipplet = readSnipplet(parentFileIndex.get(0));
				
			// unmarshal the object
			T object = unmarshaller.unmarshal(xmlSnipplet, element);
				
			return object;
		}
		catch (Exception e) {
			throw new MzXMLParsingException("Failed to unmarshall mzXML object.", e);
		}
	}

	/**
	 * Reads a given XML Snipplet from the file and returns
	 * it as a String.
	 * @param indexElement An IndexElement specifying the position to read.
	 * @return
	 * @throws MzXMLParsingException
	 */
	private String readSnipplet(IndexElement indexElement) throws MzXMLParsingException {
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
			throw new MzXMLParsingException("Failed to read from mzXML file.", e);
		}
	}
	
	/**
	 * Returns the run's attributes.
	 * @return
	 */
	public Map<String, String> getRunAttributes() {
		return runAttributes;
	}
	
	/**
	 * Returns the number of MS1 scans in the file
	 * @return
	 */
	public int getMS1ScanCount() {
		if (msNScans.containsKey(1))
			return msNScans.get(1).size();
		else
			return 0;
	}
	
	/**
	 * Returns the number of MS2 scans
	 * in the file.
	 * @return
	 */
	public int getMS2ScanCount() {
		if (msNScans.containsKey(2))
			return msNScans.get(2).size();
		else
			return 0;
	}
	
	/**
	 * Returns an iterator over all the MS 1 scan
	 * objects.
	 * @return
	 */
	public MzXMLScanIterator geMS1ScanIterator() {
		return new MzXMLScanIterator(1);
	}
	
	/**
	 * Returns an iterator over all the MS 2 scan
	 * objects.
	 * @return
	 */
	public MzXMLScanIterator getMS2ScanIterator() {
		return new MzXMLScanIterator(2);
	}
	
	/**
	 * Returns and iterator over all level 1 scans
	 * in the mzXML file.
	 * @return
	 */
	public MzXMLScanIterator getScanIterator() {
		return new MzXMLScanIterator(0);
	}
	
	/**
	 * An iterator over all the scans in the given
	 * mzXML file.
	 * @author jg
	 *
	 */
	public class MzXMLScanIterator implements Iterable<Scan>, Iterator<Scan> {
		/**
		 * The current position in the indexes array.
		 */
		private int currentIndex = 0;
		
		private List<IndexElement> indexes;
		
		/**
		 * This iterator must only be created from
		 * inside a MzXMLFile.
		 */
		@SuppressWarnings("unchecked")
		private MzXMLScanIterator(int msLevel) {			
			// get the indexes
			if (msLevel == 0)
				indexes = index.getElements(MzXmlElement.SCAN_LEVEL1.getXpath());
			else if (msNScans.containsKey(msLevel))
				indexes = msNScans.get(msLevel);
			else
				indexes = Collections.EMPTY_LIST;
		}
		
		@Override
		public boolean hasNext() {
			return currentIndex < indexes.size();
		}

		@Override
		public Scan next() {
			// get the IndexElement object and move to the next object
			IndexElement indexElement = indexes.get(currentIndex++);
			
			try {
				// read the snipplet
				String snipplet = readSnipplet(indexElement);
				
				// unmarshal the scan object from the snipplet
				Scan scan = unmarshaller.unmarshal(snipplet, MzXmlElement.SCAN_LEVEL1); // doesn't really matter which scan we use
				
				return scan;
			}
			catch (Exception e) {
				throw new RuntimeException("Failed to parse spectrum: " + e.getMessage(), e);
			}
		}

		@Override
		public void remove() {
			// not supported			
		}

		@Override
		public Iterator<Scan> iterator() {
			return this;
		}
	}

	@Override
	public int getSpectraCount() {
		// the peak list parser interface only accesses ms2 spectra
		return numToIndexMap.size();
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
		// just convert the stored "nums" from Long to String
		List<Long> nums = new ArrayList<Long>(getScanNumbers());
		List<String> ids = new ArrayList<String>(nums.size());
		
		for (Long num : nums)
			ids.add(num.toString());
		
		return ids;
	}

	@Override
	public Spectrum getSpectrumById(String id) throws JMzReaderException {
		// get the scan object
		try {
			Scan scan = getScanByStringNum(id);
			
			return new MzXMLSpectrum(scan);
		} catch (MzXMLParsingException e) {
			throw new JMzReaderException("Failed to parse spectrum", e);
		}
	}

	/**
	 * Retruns the spectrum based on its 1-based index
	 * in the mzXML file. This index only takes MS2
	 * spectra into consideration.
	 * @param index
	 * @return
	 */
	@Override
	public Spectrum getSpectrumByIndex(int index) throws JMzReaderException {
		if (index < 1 || index > numToIndexMap.size())
			throw new JMzReaderException("Spectrum index out of range.");
		
		Long num = getScanNumbers().get(index - 1);
		IndexElement indexElement = numToIndexMap.get(num);
		
		// make sure a spectrum with that index exists
		if (indexElement == null)
			throw new JMzReaderException("Spectrum with index " + index + " could not be found.");
		
		try {
			String snipplet = readSnipplet(indexElement);
			
			Scan scan = unmarshaller.unmarshal(snipplet, MzXmlElement.SCAN_LEVEL2);
			
			return new MzXMLSpectrum(scan);
		} catch (Exception e) {
			throw new JMzReaderException("Failed to parse spectrum", e);
		}
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
		Map<String, uk.ac.ebi.pride.tools.jmzreader.model.IndexElement> idToIndexMap = 
			new HashMap<String, uk.ac.ebi.pride.tools.jmzreader.model.IndexElement>(numToIndexMap.size());
		
		for (Long num : numToIndexMap.keySet()) {
			IndexElement e = numToIndexMap.get(num);
			idToIndexMap.put(num.toString(), new IndexElementImpl(e.getStart(), (int) (e.getStop() - e.getStart())));
		}
		
		return idToIndexMap;
	}
	
	/**
	 * Converts a list of xxindex IndexElementS
	 * to a list of JMzReader IndexElementS.
	 * @param elements
	 * @return
	 */
	private List<uk.ac.ebi.pride.tools.jmzreader.model.IndexElement> convertIndexElements(List<IndexElement> elements) {
		List<uk.ac.ebi.pride.tools.jmzreader.model.IndexElement> convertedElements = new ArrayList<uk.ac.ebi.pride.tools.jmzreader.model.IndexElement>(elements.size());
		
		for (IndexElement e : elements) {
			int size = (int) ( e.getStop() - e.getStart() );
			convertedElements.add(new IndexElementImpl(e.getStart(), size));
		}
		
		return convertedElements;
	}

	/**
	 * A Wrapper around the MzXMLScanIterator.
	 * @author jg
	 *
	 */
	public class SpectrumIterator implements Iterator<Spectrum> {
		// just iterate over ms2 scans
		Iterator<Long> numIterator = getScanNumbers().iterator();

		@Override
		public boolean hasNext() {
			return numIterator.hasNext();
		}

		@Override
		public Spectrum next() {
			Long num = numIterator.next();
			
			try {
				Scan scan = getScanByNum(num);
				MzXMLSpectrum spec = new MzXMLSpectrum(scan);
				
				return spec;
			} catch (MzXMLParsingException e) {
				throw new RuntimeException("Failed to parse spectrum " + num + ": " + e.getMessage(), e);
			}
		}

		@Override
		public void remove() {
			// not supported
		}
	}
}
