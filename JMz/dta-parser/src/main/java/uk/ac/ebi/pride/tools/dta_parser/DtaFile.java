package uk.ac.ebi.pride.tools.dta_parser;

import java.io.File;
import java.io.FilenameFilter;
import java.io.IOException;
import java.io.RandomAccessFile;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

import uk.ac.ebi.pride.tools.braf.BufferedRandomAccessFile;
import uk.ac.ebi.pride.tools.dta_parser.model.DtaSpectrum;
import uk.ac.ebi.pride.tools.jmzreader.JMzReader;
import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import uk.ac.ebi.pride.tools.jmzreader.model.IndexElement;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.IndexElementImpl;

/**
 * This class represents a DTA file. Normally, a dta
 * file can hold only one spectrum per file. There are
 * tools that concatenate several dta files and seperate
 * the single spectra with one or more empty line. This type
 * of dta files is supported by this class.
 * @author jg
 *
 */
public class DtaFile implements JMzReader {
	/**
	 * The sourcef file passed when creating this object
	 */
	private File sourceFile;
	/**
	 * The list of files in the directory
	 */
	private ArrayList<String> filenames;
	/**
	 * Index of the spectra in the file
	 */
	private Map<Integer, IndexElement> fileIndex;
	
	/**
	 * Creates a DtaFile object either based on a single DTA file
	 * that may contain multiple spectra separated by at least one
	 * empty line or a pointer to a directory containing multiple
	 * dta files.
	 * @param sourceFile
	 */
	public DtaFile(File sourceFile) throws JMzReaderException {
		this.sourceFile = sourceFile;
		
		if (sourceFile.isDirectory())
			loadDirectoryIndex();
		else {
			try {
				indexFile();
			} catch (IOException e) {
				throw new JMzReaderException("Failed to read from DTA file", e);
			}
		}
	}

	/**
	 * Returns an array holding the ids of all spectra
	 * in the order as they are returned by the Iterator.
	 * If the DAO is handling a directory of dta files the
	 * filenames are returned, otherwise the 0-based index
	 * in the .dta file.
	 * @return The list of spectrum ids.
	 */
	public List<String> getSpectraIds() {
		ArrayList<String> ids = new ArrayList<String>();
		
		if (sourceFile.isDirectory()) {
			for (String filename : filenames)
				ids.add(filename);
		}
		else {
			// just return 0... n-1
			for (Integer i = 0; i < fileIndex.size(); i++)
				ids.add(i.toString());
		}
		
		return ids;
	}
	
	/**
	 * Unmarhals a spectrum who's position in the file
	 * is known.
	 * @param sourcefile The file to load the spectrum from.
	 * @param indexElement IndexElement specifying the position of the Spectrum in the file.
	 * @return A Spectrum object.
	 * @throws JMzReaderException
	 */
	public static Spectrum getIndexedSpectrum(File sourcefile, IndexElement indexElement) throws JMzReaderException {
		if (sourcefile == null)
			throw new JMzReaderException("Missing required parameter sourcefile.");
		if (indexElement == null)
			throw new JMzReaderException("Missing required parameter indexElement");
		
		String specString = readSpectrumFromFile(sourcefile, indexElement);
		
		return new DtaSpectrum(specString, 1);
	}
	
	public boolean acceptsFile() {
		return true;
	}

	public boolean acceptsDirectory() {
		return true;
	}

	public Spectrum getSpectrumById(String id) throws JMzReaderException {
		return getDtaSpectrum(id);
	}

	public Spectrum getSpectrumByIndex(int index) throws JMzReaderException {
		return getDtaSpectrum(index);
	}

	public Iterator<Spectrum> getSpectrumIterator() {
		return new SpectrumIterator();
	}

	/**
	 * Indexes the current source file. Stores the offset of
	 * every spectrum in the file.
	 * @throws IOException
	 */
	private void indexFile() throws IOException{
		// create the access object
		BufferedRandomAccessFile reader = new BufferedRandomAccessFile(sourceFile, "r", 1024 * 1000);
		
		fileIndex = new HashMap<Integer, IndexElement>();
		
		// process the file line by line
		boolean emptyLineFound = true; // indicates that an empty line was enountered before
		String line = "";
		int currentSpectrum = 1; // 1-based spectrum index
		long previousIndex = 0;
		long offset = 0;
		
		while ((line = reader.getNextLine()) != null) {
			// if there was an empty line found and the current line isn't empty, save this offset
			if (emptyLineFound && line.trim().length() > 0) {
				offset = previousIndex;
				emptyLineFound = false;
			}
			
			// check if the line is empty
			if (line.trim().length() == 0) {
				if (offset >= 0) {
					int size = (int) (reader.getFilePointer() - offset);
					fileIndex.put(currentSpectrum++, new IndexElementImpl(offset, size));
				}
				
				offset = -1;
				
				emptyLineFound = true;
			}
			
			previousIndex = reader.getFilePointer();
		}
		
		if (offset >= 0 && reader.getFilePointer() > offset) {
			int size = (int) (reader.getFilePointer() - offset);
			fileIndex.put(currentSpectrum++, new IndexElementImpl(offset, size));
		}
		
		reader.close();
	}
	
	/**
	 * Loads all files present in the directory specified
	 * by sourceFile.
	 */
	private void loadDirectoryIndex() {
		// get the names of all .dta files in the directory
		String[] dtaFiles = sourceFile.list(new DtaFileFilter());
		
		filenames = new ArrayList<String>();
		
		for (String filename : dtaFiles)
			filenames.add(filename);
	}
	
	/**
	 * Returns the number of spectra
	 */
	public int getSpectraCount() {
		return (sourceFile.isDirectory()) ? filenames.size() : fileIndex.size();
	}
	
	/**
	 * Returns the spectrum identified by the index. The index can either
	 * be the source filename in case this DtaFile object represents a directory,
	 * otherwise the 1-based index of the spectrum in the file.
	 * @param index
	 * @return
	 */
	public DtaSpectrum getDtaSpectrum(Object index) throws JMzReaderException {
		// get the type
		if (sourceFile.isDirectory()) {
			if (!(index instanceof String))
				throw new JMzReaderException("Non-filename passed to DTA file object representing a directory. The spectrum index must be a filename.");
			
			File specFile = new File(sourceFile.getAbsoluteFile() + File.separator + (String) index);
			
			// create and return the spectrum
			return new DtaSpectrum(specFile);
		}
		else  {
			// make sure the index is a integer
			if (!(index instanceof Integer))
				index = Integer.parseInt(index.toString());
			
			// get the offset
			IndexElement indexElement = fileIndex.get(index);
			
			if (indexElement == null)
				throw new JMzReaderException("Spectrum with given index " + index + " does not exist in the parsed dta file.");
			
			// read the spectrum from the file
			String spec = readSpectrumFromFile(sourceFile, indexElement);
			
			return new DtaSpectrum(spec, (Integer) index);
		}		
	}
	
	/**
	 * Reads the spectrum starting at offset and returns it
	 * as a string.
	 * @param indexElement
	 * @return The spectrum as a string.
	 */
	private static String readSpectrumFromFile(File sourcefile, IndexElement indexElement) {
		try {
			// create the random access file
			RandomAccessFile accessFile = new RandomAccessFile(sourcefile, "r");
			
			// move to the respective position
			accessFile.seek(indexElement.getStart());
			int size = indexElement.getSize();
			
			byte[] buffer = new byte[size];
			accessFile.read(buffer);
			accessFile.close();
			
			// initialize the spectrum string
			String spectrum = new String(buffer);
			
			return spectrum;
		}
		catch(Exception e) {
			throw new IllegalStateException("Failed to read from file", e);
		}
	}
	
	public Iterator<DtaSpectrum> getDtaSpectrumIterator() {
		return new DtaFileSpectrumIterator();
	}
	
	@Override
	public List<IndexElement> getMsNIndexes(int msLevel) {
		// DtaFiles can only contain msLevel 2 spectra
		if (msLevel != 2)
			return Collections.emptyList();
		
		// if it's a directory based structure, return an empty list as well
		if (sourceFile.isDirectory())
			return Collections.emptyList();
		
		// create the list of index elements
		List<IndexElement> index = new ArrayList<IndexElement>(fileIndex.size());
		
		for (Integer i = 1; i <= fileIndex.size(); i++)
			index.add(fileIndex.get(i));
		
		return index;
	}

	@Override
	public List<Integer> getMsLevels() {
		List<Integer> msLevels = new ArrayList<Integer>(1);
		msLevels.add(2);
		return msLevels;
	}

	@Override
	public Map<String, IndexElement> getIndexElementForIds() {
		// if it's a directory based structure, return an empty map
		if (sourceFile.isDirectory())
			return Collections.emptyMap();
		
		// create the map of index elements
		Map<String, IndexElement> index = new HashMap<String, IndexElement>(fileIndex.size());
		
		for (Integer i = 1; i <= fileIndex.size(); i++)
			index.put(i.toString(), fileIndex.get(i));
		
		return index;
	}



	/**
	 * Pipes everything through to the dta spectrum
	 * iterator.
	 * @author jg
	 *
	 */
	private class SpectrumIterator implements Iterator<Spectrum> {
		private DtaFileSpectrumIterator it = new DtaFileSpectrumIterator();
		
		public boolean hasNext() {
			return it.hasNext();
		}

		public Spectrum next() {
			return it.next();
		}

		public void remove() {
			it.remove();
		}
		
	}
	
	private class DtaFileSpectrumIterator implements Iterator<DtaSpectrum>, Iterable<DtaSpectrum> {
		/**
		 * The current index, either in the filename array or in the hash (1-based)
		 */
		int currentIndex = 0;
		
		public Iterator<DtaSpectrum> iterator() {
			return this;
		}

		public boolean hasNext() {
			if (sourceFile.isDirectory())
				return currentIndex < filenames.size();
			else
				return fileIndex.containsKey(currentIndex + 1); // fileindex is 1-based
		}

		public DtaSpectrum next() {
			// if the DtaFile represents a directory, get the filename and generate the DTA file from there.
			if (sourceFile.isDirectory()) {
				String filename = filenames.get(currentIndex++);
				
				File specFile = new File(sourceFile.getAbsolutePath() + File.separator + filename);
				
				try {
					return new DtaSpectrum(specFile);
				} catch (JMzReaderException e) {
					throw new RuntimeException("Failed to parse dta spectrum", e);
				}
			}
			// if the dta file is a concatenatd dta file, read the spectrum from the index
			else {
				IndexElement indexElement = fileIndex.get(currentIndex++ + 1);
				
				String spec = readSpectrumFromFile(sourceFile, indexElement);
				
				try {
					return new DtaSpectrum(spec, currentIndex);
				} catch (JMzReaderException e) {
					throw new RuntimeException("Failed to parse dta spectrum", e);
				}
			}
		}

		public void remove() {
			// not supported
			throw new IllegalStateException("Objects cannot be removed from DtaFileSpectrumIterator.");
		}
	}
	
	public class DtaFileFilter implements FilenameFilter {

		public boolean accept(File dir, String name) {
			return name.endsWith(".dta");
		}
	}
}
