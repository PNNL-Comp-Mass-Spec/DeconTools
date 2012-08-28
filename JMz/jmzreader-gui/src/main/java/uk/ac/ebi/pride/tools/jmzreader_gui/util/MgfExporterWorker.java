package uk.ac.ebi.pride.tools.jmzreader_gui.util;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.util.Iterator;
import java.util.concurrent.CancellationException;
import java.util.concurrent.ExecutionException;

import javax.swing.JFrame;
import javax.swing.JOptionPane;
import javax.swing.SwingWorker;

import uk.ac.ebi.pride.tools.jmzreader.JMzReader;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.mgf_parser.model.Ms2Query;

/**
 * Exports the content of a given
 * JMzParser to an mgf file
 * @author jg
 *
 */
public class MgfExporterWorker extends SwingWorker<Void, Void> {
	private File outputfile;
	private JMzReader parser;
	private FileWriter writer;
	private final String EOL = System.getProperty("line.separator");
	public final String PROPERTY_NAME = "exporter-progress";
	private JFrame parentFrame;
	
	public MgfExporterWorker(File outputfile, JMzReader parser, JFrame parentFrame) {
		this.outputfile = outputfile;
		this.parser = parser;
		this.parentFrame = parentFrame;
	}

	@Override
	protected Void doInBackground() throws Exception {
		// check the passed variables
		if (parser == null)
			throw new Exception("Invalid JMzParser object passed to 'exportToMgf'");
		if (outputfile == null)
			throw new Exception("Invalid output File object passed to 'exportToMgf'");
		
		// make sure the file can be written
		if (outputfile.exists() && !outputfile.canWrite())
			throw new Exception("Missing permission to write outputfile '" + outputfile.getName() + "'");
		
		if (isCancelled())
			return null;
		
		// create the writer object
		writer = new FileWriter(outputfile);
		
		firePropertyChange(PROPERTY_NAME, 0, 0);
		
		// as there is no meta-data to set, just parse the spectra
		processSpectra(parser);
		
		if (writer != null) {
			writer.close();
			writer = null;
		}
		
		firePropertyChange(PROPERTY_NAME, 0, 100);
		
		return null;
	}

	/**
	 * Process the spectra of the input parser
	 * and add them to the mgfOutput file.
	 * @param parser The JMzParser to get the spectra from.
	 * @return A list of Ms2QueryS.
	 */
	private void processSpectra(JMzReader parser) throws Exception {
		int nCurrentSpec = 1;
		int specCount = parser.getSpectraCount();
		
		// process the spectra
		Iterator<Spectrum> it = parser.getSpectrumIterator();
		
		if (isCancelled())
			return;

		while (it.hasNext()) {
			Spectrum spectrum = it.next();
			
			// set the progress
			int progress = nCurrentSpec++ * 100 / specCount;
			firePropertyChange(PROPERTY_NAME, 0, progress);
			
			Ms2Query query = new Ms2Query();
			
			if (spectrum.getId() != null)
				query.setTitle(spectrum.getId());
			if (spectrum.getPrecursorCharge() != null)
				query.setChargeState(spectrum.getPrecursorCharge().toString());
			if (spectrum.getPrecursorMZ() != null)
				query.setPeptideMass(spectrum.getPrecursorMZ());
			if (spectrum.getPrecursorIntensity() != null)
				query.setPeptideIntensity(spectrum.getPrecursorIntensity());
			
			query.setPeakList(spectrum.getPeakList());
			
			// write the spectrum
			writer.write(query.toString() + EOL);
			
			if (isCancelled())
				return;
		}
	}

	@Override
	protected void done() {
		// close the writer and just use to delete the output file 
		try {
			if (writer != null)
				writer.close();
		}
		catch (IOException e) {
			// ignore any problems
		}
		
		try {
			get();
			
			JOptionPane.showMessageDialog(parentFrame, 
					"Data successfully written to " + outputfile.getName(),
					"MGF export complete",
					JOptionPane.INFORMATION_MESSAGE);
		} catch (CancellationException e) { 
			// delete the file
			if (outputfile.exists())
				outputfile.delete();
		}
		catch (InterruptedException e) {
			// delete the file
			if (outputfile.exists())
				outputfile.delete();
			
			JOptionPane.showMessageDialog(parentFrame, 
					"Failed to write MGF file. " + e.getMessage(),
					"Failed to write MGF file",
					JOptionPane.ERROR_MESSAGE);
		} catch (ExecutionException e) {
			// delete the file
			if (outputfile.exists())
				outputfile.delete();
			
			JOptionPane.showMessageDialog(parentFrame, 
					"Failed to write MGF file. " + e.getMessage(),
					"Failed to write MGF file",
					JOptionPane.ERROR_MESSAGE);
		}
		
		super.done();
	}
}
