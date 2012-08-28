package uk.ac.ebi.pride.tools.pkl_parser;

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
import uk.ac.ebi.pride.tools.jmzreader.JMzReader;
import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import uk.ac.ebi.pride.tools.jmzreader.model.IndexElement;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.jmzreader.model.impl.IndexElementImpl;
import uk.ac.ebi.pride.tools.pkl_parser.model.PklSpectrum;

/**
 * This class represents either a PKL file containing multiple spectra
 * separated at least by one empty line or a directory containing multipl
 * .pkl files with one spectrum each.
 * @author jg
 *
 */
public class PklFile implements JMzReader {
	/**
	 * The sourcefile passed when creating this object
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
	 * Creates a PklFile object either based on a single PKL file
	 * that may contain multiple spectra separated by at least one
	 * empty line or a pointer to a directory containing multiple
	 * dta files.
	 * @param sourceFile
	 * @throws JMzReaderException 
	 */
	public PklFile(File sourceFile) throws JMzReaderException {
		this.sourceFile = sourceFile;
		
		if (sourceFile.isDirectory())
			loadDirectoryIndex();
		else
			try {
				indexFile();
			} catch (IOException e) {
				throw new JMzReaderException("Failed to read from PKL file", e);
			}
	}
	
	/**
	 * Creates a PklFile object together with the pre-created index.
	 * WARNING: This constructor is only valid for pkl files!
	 * @param sourceFile The pkl file to use.
	 * @param fileIndex The file's index containing the 1-based spectrum index as key and its offset as value.
	 * @throws JMzReaderException 
	 */
	public PklFile(File sourceFile, Map<Integer, IndexElement> fileIndex) throws JMzReaderException {
		if (!sourceFile.isFile())
			throw new JMzReaderException("Illegal call to PklFile(File sourceFile, HashMap<Integer, Long> fileIndex): sourceFile must point to a file.");
		
		this.sourceFile = sourceFile;
		this.fileIndex = fileIndex;
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
				if (offset >= 0 && previousIndex > 0) {
					int size = (int) ( previousIndex - offset );
					fileIndex.put(currentSpectrum++, new IndexElementImpl(offset, size));
				}
				offset = -1;
				emptyLineFound = true;
			}
			
			previousIndex = reader.getFilePointer();
		}
		
		// add the last spectrum
		if (offset >= 0 && reader.getFilePointer() > offset) {
			int size = (int) ( previousIndex - offset );
			fileIndex.put(currentSpectrum++, new IndexElementImpl(offset, size));
		}
		
		reader.close();
	}
	
	/**
	 * Loads all files present in the directory specified
	 * by sourceFile.
	 */
	private void loadDirectoryIndex() {
		// get the names of all .pkl files in the directory
		String[] pklFiles = sourceFile.list(new PklFileFilter());
		
		filenames = new ArrayList<String>();
		
		for (String filename : pklFiles)
			filenames.add(filename);
	}
	
	/**
	 * Reads a spectrum who's position in the file
	 * is known from the PKL file.
	 * 
	 * @param sourcefile The file to read the spectrum from.
	 * @param indexElement The IndexElement specifying the spectrum's position.
	 * @return The unmarshalled Scan object.
	 * @throws JMzReaderException
	 */
	public static Spectrum getIndexedSpectrum(File sourcefile, IndexElement indexElement) throws JMzReaderException {
		if (sourcefile == null)
			throw new JMzReaderException("Parameter sourcefile must not be null.");
		if (indexElement == null)
			throw new JMzReaderException("Parameter indexElement must not be null.");
		
		String snipplet = readSpectrumFromFile(indexElement, sourcefile);
		
		return new PklSpectrum(snipplet, 1);
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
	 * @throws JMzReaderException 
	 */
	public PklSpectrum getSpectrum(Object index) throws JMzReaderException {
		// get the type
		if (sourceFile.isDirectory()) {
			if (!(index instanceof String))
				throw new JMzReaderException("For PKL file objects representing directories the spectrum index must be a filename");
			
			File specFile = new File(sourceFile.getAbsoluteFile() + File.separator + (String) index);
			
			// create and return the spectrum
			return new PklSpectrum(specFile);
		}
		else  {
			// make sure the index is a integer
			if (!(index instanceof Integer))
				index = Integer.parseInt(index.toString());
			
			// get the offset
			IndexElement indexElement = fileIndex.get(index);
			
			if (indexElement == null)
				throw new JMzReaderException("Spectrum with given index " + index + " does not exist");
			
			// read the spectrum from the file
			String spec = readSpectrumFromFile(indexElement, sourceFile);
			
			return new PklSpectrum(spec,(Integer) index);
		}		
	}
	
	/**
	 * Reads the spectrum starting at offset and returns it
	 * as a string.
	 * @param indexElement
	 * @param sourceFile The file to read the spectrum from.
	 * @return The spectrum as a string.
	 * @throws JMzReaderException 
	 */
	private static String readSpectrumFromFile(IndexElement indexElement, File sourceFile) throws JMzReaderException {
		try {
			// create the random access file
			RandomAccessFile file = new RandomAccessFile(sourceFile, "r");
			
			// move to the respective position
			file.seek(indexElement.getStart());
			
			byte[] buffer = new byte[indexElement.getSize()];
			
			file.read(buffer);			
			file.close();
			
			String spectrum = new String(buffer);
			
			return spectrum;
		}
		catch(Exception e) {
			throw new JMzReaderException("Failed to read from file", e);
		}
	}
	
	public Iterator<PklSpectrum> getPklSpectrumIterator() {
		return new PklFileSpectrumIterator();
	}
	
	public boolean acceptsFile() {
		return true;
	}

	public boolean acceptsDirectory() {
		return true;
	}

	public Spectrum getSpectrumById(String id) throws JMzReaderException {
		return getSpectrum(id);
	}

	public Spectrum getSpectrumByIndex(int index) throws JMzReaderException {
		return getSpectrum(index);
	}

	public Iterator<Spectrum> getSpectrumIterator() {
		return new SpectrumIterator();
	}
	
	private class SpectrumIterator implements Iterator<Spectrum> {
		private PklFileSpectrumIterator it = new PklFileSpectrumIterator();

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
	
	private class PklFileSpectrumIterator implements Iterator<PklSpectrum>, Iterable<PklSpectrum> {
		/**
		 * The current index, either in the filename array or in the hash (1-based)
		 */
		int currentIndex = 0;
		
		public Iterator<PklSpectrum> iterator() {
			return this;
		}

		public boolean hasNext() {
			if (sourceFile.isDirectory())
				return currentIndex < filenames.size();
			else
				return fileIndex.containsKey(currentIndex + 1); // fileindex is 1-based
		}

		public PklSpectrum next() {
			// if the DtaFile represents a directory, get the filename and generate the DTA file from there.
			if (sourceFile.isDirectory()) {
				String filename = filenames.get(currentIndex++);
				
				File specFile = new File(sourceFile.getAbsolutePath() + File.separator + filename);
				
				try {
					return new PklSpectrum(specFile);
				} catch (JMzReaderException e) {
					throw new IllegalStateException("Failed to parse pkl spectrum", e);
				}
			}
			// if the dta file is a concatenatd dta file, read the spectrum from the index
			else {
				IndexElement indexElement = fileIndex.get(currentIndex++ + 1);
				
				String spec;
				try {
					spec = readSpectrumFromFile(indexElement, sourceFile);
					
					return new PklSpectrum(spec, currentIndex);
				} catch (JMzReaderException e) {
					throw new IllegalStateException("Failed to parse pkl spectrum", e);
				}
			}
		}

		public void remove() {
			// not supported
			throw new IllegalStateException("Objects cannot be removed from PklFileSpectrumIterator.");
		}
	}
	
	public class PklFileFilter implements FilenameFilter {

		public boolean accept(File dir, String name) {
			return name.endsWith(".pkl");
		}
	}

	/**
	 * Returns the fileIndex for the given file. The
	 * index contains the spectrum's 1-based index in the
	 * file as key and its position in the file as value.
	 * This functions returns null in case the PklFile object
	 * represents a directory.
	 * @return HashMap Contains the spectrums' 1-based index as key and its offset as value. Is null in case the PklFile object represents a directory.
	 */
	public Map<Integer, IndexElement> getFileIndex() {
		return (sourceFile.isFile()) ? fileIndex : null;
	}
	
	/**
	 * Returns an array holding the ids of all spectra
	 * in the order as they are returned by the Iterator.
	 * If the DAO is handling a directory of pkl files the
	 * filenames are returned, otherwise the 1-based index
	 * in the .pkl file.
	 * @return The list of spectrum ids.
	 */
	public List<String> getSpectraIds() {
		ArrayList<String> ids = new ArrayList<String>();
		
		if (sourceFile.isDirectory()) {
			for (String filename : filenames)
				ids.add(filename);
		}
		else {
			// just return 1... n
			for (Integer i = 1; i <= fileIndex.size(); i++)
				ids.add(i.toString());
		}
		
		return ids;
	}

	@Override
	public List<IndexElement> getMsNIndexes(int msLevel) {
		if (msLevel != 2)
			return Collections.emptyList();
		
		List<IndexElement> index = new ArrayList<IndexElement>(fileIndex.size());
		
		for (Integer i = 1; i <= fileIndex.size(); i++) {
			if (fileIndex.containsKey(i))
				index.add(fileIndex.get(i));
		}
		
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
		Map<String, IndexElement> idToIndexMap = new HashMap<String, IndexElement>(fileIndex.size());
		
		for (Integer i = 1; i <= fileIndex.size(); i++) {
			if (fileIndex.containsKey(i))
				idToIndexMap.put(i.toString(), fileIndex.get(i));
		}
		
		return idToIndexMap;
	}
}
