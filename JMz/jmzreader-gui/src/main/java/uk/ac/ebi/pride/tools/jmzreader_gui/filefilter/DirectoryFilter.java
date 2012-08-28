package uk.ac.ebi.pride.tools.jmzreader_gui.filefilter;

import java.io.File;

import javax.swing.filechooser.FileFilter;

public class DirectoryFilter extends FileFilter {

	@Override
	public boolean accept(File f) {
		return f.isDirectory();
	}

	@Override
	public String getDescription() {
		return "Directory containing one spectrum per file (.pkl, .dta)";
	}

}
