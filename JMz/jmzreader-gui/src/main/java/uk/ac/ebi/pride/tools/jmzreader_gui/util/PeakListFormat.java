package uk.ac.ebi.pride.tools.jmzreader_gui.util;

/**
 * The supported peak list formats.
 * @author jg
 *
 */
public enum PeakListFormat {
	DTA(".dta"),
	MGF(".mgf"),
	MS2(".ms2"),
	PKL(".pkl"),
	MZXML(".mzXML"),
	XML_FILE(".xml"),
	MZML(".mzML");
	
	private String extension;
	
	private PeakListFormat(String extension) {
		this.extension = extension;
	}
	
	public String getExtension() {
		return extension;
	}
	
	/**
	 * Returns the expected file format based
	 * on the filename's extension. Returns null
	 * in case the filetype is unknown.
	 * @param filename
	 * @return
	 */
	public static PeakListFormat getFormat(String filename) {
		filename = filename.toLowerCase();
		
		for (PeakListFormat f : values()) {
			if (filename.endsWith(f.getExtension().toLowerCase()))
				return f;
		}
		
		return null;
	}
}
