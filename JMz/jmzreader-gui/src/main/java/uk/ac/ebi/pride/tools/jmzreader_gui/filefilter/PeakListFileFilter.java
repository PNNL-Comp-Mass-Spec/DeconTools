package uk.ac.ebi.pride.tools.jmzreader_gui.filefilter;

import java.io.File;

import javax.swing.filechooser.FileFilter;

import uk.ac.ebi.pride.tools.jmzreader_gui.util.PeakListFormat;

public class PeakListFileFilter extends FileFilter {
	
	@Override
	public String getDescription() {
		return "Peak list file (*.dta, *.ms2, *.mgf, *.pkl, *.mzXML, *.xml (mzData, PRIDE XML), *.mzML)";
	}

	public boolean accept(File arg0) {
		if (arg0.isDirectory())
			return true;
		
		PeakListFormat format = PeakListFormat.getFormat(arg0.getName());
		
		if (format != null)
			return true;
		else
			return false;
	}

}
