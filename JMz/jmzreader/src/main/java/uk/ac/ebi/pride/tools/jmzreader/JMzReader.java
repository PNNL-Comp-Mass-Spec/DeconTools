package uk.ac.ebi.pride.tools.jmzreader;

import java.util.Iterator;
import java.util.List;
import java.util.Map;

import uk.ac.ebi.pride.tools.jmzreader.model.IndexElement;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;

/**
 * Parser for a peak list formats.
 * 
 * <b>NOTE:</b> Every class implementing the JMzReader
 * interface should contain the following static function:
 * public static Spectrum getIndexedSpectrum(File sourcefile, IndexElement indexElement)
 * 
 * @author jg
 *
 */
public interface JMzReader {
	/**
	 * Returns the number of spectra in the given
	 * file / directory.
	 * @return
	 */
	public int getSpectraCount();
	
	/**
	 * Indicates whether the given peak list
	 * parser supports the parsing of single files.
	 * @return
	 */
	public boolean acceptsFile();
	
	/**
	 * Indicates whether the given peak list
	 * parser supports the parsing of
	 * directories.
	 * @return
	 */
	public boolean acceptsDirectory();
	
	/**
	 * Returns a list of Strings that hold the 
	 * spectra's ids. The order of the returned 
	 * ids resembls the order the spectra are 
	 * returned in the spectra iterator. For 
	 * formats that do not support ids such as 
	 * dta or mgf the spectrum's 1-based index 
	 * is used as id.
	 * @return
	 */
	public List<String> getSpectraIds();
	
	/**
	 * Returns the spectrum with the given id.
	 * @param id The spectrum's id.
	 * @return A Spectrum or null in case a spectrum with the given id doesn't exist.
	 */
	public Spectrum getSpectrumById(String id) throws JMzReaderException;
	
	/**
	 * Returns the spectrum based on its 1-based index
	 * in the file. In case the parser is parsing a
	 * directory the index will be translated to the
	 * filename's position in the directory (which can
	 * be random).
	 * @param index The 1-based index of the spectrum in the file.
	 * @return A Spectrum or null in case a spectrum with the given index doesn't exist.
	 */
	public Spectrum getSpectrumByIndex(int index) throws JMzReaderException;
	
	/**
	 * Returns an Iterator over all spectra in the file.
	 * @return
	 */
	public Iterator<Spectrum> getSpectrumIterator();
	
	/**
	 * Returns a list of IndexElements for the spectra
	 * of the given MS level in the file.
	 * 
	 * @param msLevel The ms level of the spectra to get the index elements for.
	 * @return A list of IndexElementS
	 */
	public List<IndexElement> getMsNIndexes(int msLevel);
	
	/**
	 * Returns a list of ms levels (as integers) found in
	 * the parsed file.
	 * 
	 * @return List of ms levels in the file.
	 */
	public List<Integer> getMsLevels();
	
	/**
	 * Returns a Map containing the spectra ids as keys
	 * and the associated IndexElementS as values.
	 * 
	 * @return Map with spectra ids as key and their IndexElements as values.
	 */
	public Map<String, IndexElement> getIndexElementForIds();
}
