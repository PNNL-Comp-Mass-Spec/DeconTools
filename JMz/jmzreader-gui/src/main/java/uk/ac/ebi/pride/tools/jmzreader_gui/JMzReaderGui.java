package uk.ac.ebi.pride.tools.jmzreader_gui;

import java.awt.BorderLayout;
import java.awt.ComponentOrientation;
import java.awt.Cursor;
import java.awt.Dimension;
import java.awt.event.ActionEvent;
import java.awt.event.ActionListener;
import java.awt.event.ItemEvent;
import java.awt.event.ItemListener;
import java.awt.event.KeyEvent;
import java.awt.event.MouseEvent;
import java.awt.event.WindowAdapter;
import java.awt.event.WindowEvent;
import java.beans.PropertyChangeEvent;
import java.beans.PropertyChangeListener;
import java.io.File;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Iterator;
import java.util.List;
import java.util.concurrent.CancellationException;
import java.util.concurrent.ExecutionException;

import javax.swing.GroupLayout;
import javax.swing.GroupLayout.Alignment;
import javax.swing.ImageIcon;
import javax.swing.JButton;
import javax.swing.JFileChooser;
import javax.swing.JFrame;
import javax.swing.JLabel;
import javax.swing.JMenu;
import javax.swing.JMenuBar;
import javax.swing.JMenuItem;
import javax.swing.JOptionPane;
import javax.swing.JPanel;
import javax.swing.JProgressBar;
import javax.swing.JScrollPane;
import javax.swing.JSplitPane;
import javax.swing.JTable;
import javax.swing.LayoutStyle.ComponentPlacement;
import javax.swing.ListSelectionModel;
import javax.swing.ProgressMonitor;
import javax.swing.SwingWorker;
import javax.swing.UIManager;
import javax.swing.UnsupportedLookAndFeelException;
import javax.swing.border.EmptyBorder;
import javax.swing.event.ListSelectionEvent;
import javax.swing.event.ListSelectionListener;
import javax.swing.table.JTableHeader;

import uk.ac.ebi.pride.mzgraph.SpectrumBrowser;
import uk.ac.ebi.pride.tools.jmzreader.JMzReader;
import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.jmzreader_gui.filefilter.DirectoryFilter;
import uk.ac.ebi.pride.tools.jmzreader_gui.filefilter.PeakListFileFilter;
import uk.ac.ebi.pride.tools.jmzreader_gui.table_renderers.NaDefaultRenderer;
import uk.ac.ebi.pride.tools.jmzreader_gui.util.MgfExporterWorker;
import uk.ac.ebi.pride.tools.jmzreader_gui.util.PeakListParserFactory;

@SuppressWarnings("serial")
public class JMzReaderGui extends JFrame implements ActionListener, ItemListener, PropertyChangeListener, ListSelectionListener {
	private File inputfile;
	private JMenuBar menuBar;
	private JPanel contentPane;
	private JSplitPane contentSplitPane;
	private JScrollPane tableScrollPane;
	private JTable table;
	private SpectrumTableModel spectrumTableModel;
	private SpectrumBrowser spectrumBrowser;
	private JPanel statusBar;
	private JProgressBar statusProgress;
	private JButton btnCancelLoading;
	
	private JLabel filenameLabel;
	private JLabel numSpecLabel;
	
	private JMenuItem exitItem;
	private JMenuItem openPeakListItem;
	private JMenuItem openPeakListDirectory;
	
	private JMenuItem exportToMGF;
	
	private JMzReader inputParser;
	
	private TableLoaderWorker tableLoader;
	private SetPeakListParserWorker setPeakListWorker;
	
	private ProgressMonitor exportMonitor;
	private MgfExporterWorker mgfExportWorker;
	
	protected String[] columnToolTips = {
		    "The spectrum's position in the file.",
		    "The spectrum's id in the file. This field depends on the underlying file format.",
		    "The spectrum's precursor's m/z.",
		    "The spectrum's precursor's intensity.",
		    "The spectrum's precursor's charge.",
		    "Total number of peaks in the given spectrum."};
	
	/**
	 * The last directory chosen by the user.
	 */
	private File lastChosenDirectory;
	/**
	 * Error encountered when parsing the peak list file.
	 */
	private String lastParsingError;
	
	public JMzReaderGui() {
		createGUI();
		
		setIconImage(createImage("program_icon.png", "jmzReader GUI").getImage());
		setTitle("jmzReader GUI");
	}

	private void createGUI() {
		// set the menu
		menuBar = new JMenuBar();
		
		// file menu
		JMenu fileMenu = new JMenu("File");
		fileMenu.setMnemonic(KeyEvent.VK_F);
		menuBar.add(fileMenu);
		
		// open peak list file
		openPeakListItem = new JMenuItem("Open file");
		openPeakListItem.setMnemonic(KeyEvent.VK_O);
		openPeakListItem.addActionListener(this);
		openPeakListItem.setToolTipText("Opens a single peak list file.");
		openPeakListItem.setActionCommand("OpenPeakList");
		fileMenu.add(openPeakListItem);
		
		// open peak list directory
		openPeakListDirectory = new JMenuItem("Open directory");
		openPeakListDirectory.setMnemonic(KeyEvent.VK_D);
		openPeakListDirectory.addActionListener(this);
		openPeakListDirectory.setToolTipText("Opens a directory of peak list files each containing a single spectrum. This option is available for .dta and .pkl.");
		openPeakListDirectory.setActionCommand("OpenPeakListDirectory");
		fileMenu.add(openPeakListDirectory);
		
		exitItem = new JMenuItem("Exit");
		exitItem.setMnemonic(KeyEvent.VK_E);
		exitItem.setActionCommand("Exit");
		exitItem.addActionListener(this);
		fileMenu.add(exitItem);
		
		// export menu
		JMenu exportMenu = new JMenu("Export");
		exportMenu.setMnemonic(KeyEvent.VK_E);
		menuBar.add(exportMenu);
		
		exportToMGF = new JMenuItem("Export to MGF");
		exportToMGF.setMnemonic(KeyEvent.VK_M);
		exportToMGF.addActionListener(this);
		exportToMGF.setActionCommand("exportToMGF");
		exportToMGF.setToolTipText("Exports the current peak list to MGF");
		exportMenu.add(exportToMGF);
		
		setJMenuBar(menuBar);
		
		// create the spectrum table
		spectrumTableModel = new SpectrumTableModel();
		table = createSpectrumTable();
		
		tableScrollPane = new JScrollPane(table);
		
		// create the spectrum panel
		spectrumBrowser = new SpectrumBrowser();
		
		// add them to the content pane
		contentSplitPane = new JSplitPane(JSplitPane.VERTICAL_SPLIT, spectrumBrowser, tableScrollPane);
		
		contentSplitPane.setOneTouchExpandable(true);
		contentSplitPane.setDividerLocation(350);
		
		// build the top panel
		filenameLabel = new JLabel(" No file loaded");
		filenameLabel.setBorder(null);
		
		numSpecLabel = new JLabel("Number of Spectra: 0 ");
		numSpecLabel.setBorder(null);
		
		statusProgress = new JProgressBar(0, 100);
		statusProgress.setPreferredSize(new Dimension(50, 14));
		statusProgress.setBorder(null);
		statusProgress.setValue(0);
		
		btnCancelLoading = new JButton("Cancel");
		btnCancelLoading.setEnabled(false);
		btnCancelLoading.addActionListener(new ActionListener() {
			public void actionPerformed(ActionEvent arg0) {
				if (tableLoader != null && !tableLoader.isCancelled() && !tableLoader.isDone())
					tableLoader.cancel(false);
				else if (setPeakListWorker != null && !setPeakListWorker.isCancelled() && !setPeakListWorker.isDone())
					setPeakListWorker.cancel(true);
			}
		});
		
		statusBar = new JPanel();
		statusBar.setPreferredSize(new Dimension(20, 20));
		statusBar.setMinimumSize(new Dimension(20, 20));
		statusBar.setMaximumSize(new Dimension(32767, 20));
		statusBar.setComponentOrientation(ComponentOrientation.LEFT_TO_RIGHT);
		
		contentPane = new JPanel();
		contentPane.setLayout(new BorderLayout(0, 0));
		contentPane.add(contentSplitPane, BorderLayout.CENTER);
		contentPane.add(statusBar, BorderLayout.PAGE_END);
		
		
		GroupLayout gl_statusBar = new GroupLayout(statusBar);
		gl_statusBar.setHorizontalGroup(
			gl_statusBar.createParallelGroup(Alignment.LEADING)
				.addGroup(gl_statusBar.createSequentialGroup()
					.addContainerGap()
					.addComponent(filenameLabel, GroupLayout.PREFERRED_SIZE, 200, Short.MAX_VALUE)
					.addGap(12)
					.addComponent(numSpecLabel, GroupLayout.DEFAULT_SIZE, 214, Short.MAX_VALUE)
					.addPreferredGap(ComponentPlacement.RELATED)
					.addComponent(statusProgress, 40, 273, Short.MAX_VALUE)
					.addGap(12)
					.addComponent(btnCancelLoading, GroupLayout.PREFERRED_SIZE, 81, GroupLayout.PREFERRED_SIZE)
					.addContainerGap())
		);
		gl_statusBar.setVerticalGroup(
			gl_statusBar.createParallelGroup(Alignment.TRAILING)
				.addGroup(gl_statusBar.createSequentialGroup()
					.addGroup(gl_statusBar.createParallelGroup(Alignment.LEADING)
						.addGroup(Alignment.TRAILING, gl_statusBar.createParallelGroup(Alignment.BASELINE)
							.addComponent(numSpecLabel, GroupLayout.DEFAULT_SIZE, GroupLayout.PREFERRED_SIZE, GroupLayout.PREFERRED_SIZE)
							.addComponent(filenameLabel, GroupLayout.PREFERRED_SIZE, 18, GroupLayout.PREFERRED_SIZE))
						.addComponent(btnCancelLoading, GroupLayout.PREFERRED_SIZE, 20, Short.MAX_VALUE)
						.addComponent(statusProgress, GroupLayout.DEFAULT_SIZE, 20, Short.MAX_VALUE))
					.addContainerGap())
		);
		statusBar.setLayout(gl_statusBar);
		
		setContentPane(contentPane);
	}

	private JTable createSpectrumTable() {
		JTable table = new JTable(spectrumTableModel) {
			protected JTableHeader createDefaultTableHeader() {				
				return new JTableHeader(columnModel) {
		            public String getToolTipText(MouseEvent e) {
		                java.awt.Point p = e.getPoint();
		                int index = columnModel.getColumnIndexAtX(p.x);
		                int realIndex = 
		                        columnModel.getColumn(index).getModelIndex();
		                return columnToolTips[realIndex];
		            }
		        };

			}
		};
		table.setBorder(new EmptyBorder(0, 0, 0, 0));
		table.setFillsViewportHeight(true);
		table.setAutoCreateRowSorter(true);
		table.setSelectionMode(ListSelectionModel.SINGLE_SELECTION);
		table.getSelectionModel().addListSelectionListener(this);
		table.setName("SpectrumTable");
		table.setDefaultRenderer(Object.class, new NaDefaultRenderer());
		
		// set the column width of the first (spectrum #) column
		table.getColumnModel().getColumn(0).setPreferredWidth(30);
		
		return table;
	}

	public static void main(String[] args) {
		// set the look and feel
		// Set System L&F
        try {
			UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
		} catch (ClassNotFoundException e1) {
			e1.printStackTrace();
		} catch (InstantiationException e1) {
			e1.printStackTrace();
		} catch (IllegalAccessException e1) {
			e1.printStackTrace();
		} catch (UnsupportedLookAndFeelException e1) {
			e1.printStackTrace();
		}
		
		JMzReaderGui gui = new JMzReaderGui();

        gui.setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
        gui.addWindowListener(new WindowAdapter() {
            /**
             * Invoked when a window is in the process of being closed.
             * The close operation can be overridden at this point.
             */
            public void windowClosing(WindowEvent e) {
                e.getWindow().setVisible(false);
                e.getWindow().dispose();
                System.exit(0);
            }
        });
        gui.setLocation(100, 100);
        gui.setPreferredSize(new Dimension(830, 600));
        gui.pack();
        gui.setVisible(true);
    }

    /**
     * @param path the path where to load the image from.
     * @param description a description to add to the image.
     * @return an ImageIcon of the image found on the specified path, or null if the path was invalid.
     **/
	private ImageIcon createImage(String path, String description) {
	    java.net.URL imgURL = getClass().getClassLoader().getResource(path);
	    if (imgURL != null) {
	        return new ImageIcon(imgURL, description);
	    } else {
	        System.err.println("Couldn't find file: " + path);
	        return null;
	    }
	}

	public void actionPerformed(ActionEvent e) {
		if (openPeakListItem.getActionCommand().equals(e.getActionCommand()))
			onOpenPeakListFile();
		if (openPeakListDirectory.getActionCommand().equals(e.getActionCommand()))
			onOpenPeakListDirectory();
		if (exportToMGF.getActionCommand().equals(e.getActionCommand()))
			exportToMGF();
		if (exitItem.getActionCommand().equals(e.getActionCommand()))
			onExit();
	}
	
	private void onExit() {
		setVisible(false);
        dispose();
        System.exit(0);
	}

	private void exportToMGF() {
		if (inputParser == null) {
			JOptionPane.showMessageDialog(this, 
				"Please open a file to export its content to MGF.",
				"No File Loaded",
				JOptionPane.ERROR_MESSAGE);
			
			return;
		}
		
		// create a file chooser
		JFileChooser fc = new JFileChooser();
		fc.setFileSelectionMode(JFileChooser.FILES_ONLY);
		fc.setMultiSelectionEnabled(false);
		fc.setDialogTitle("Export to MGF");
		if (lastChosenDirectory != null)
			fc.setCurrentDirectory(lastChosenDirectory);
		
		// show the dialog
		int returnVal = fc.showSaveDialog(this);
		
		// make sure the user clicked OK
		if (returnVal != JFileChooser.APPROVE_OPTION)
			return;
		
		// if the file exists, ask whether to overwrite
		File outputFile = fc.getSelectedFile();
				
		// make sure the outputfile ends with ".mgf"
		if (!outputFile.getName().endsWith(".mgf"))
			outputFile = new File(outputFile.getAbsolutePath() + ".mgf");
		
		if (outputFile.exists()) {
			Object[] options = {"Overwrite",
			                    "Cancel"};
			int n = JOptionPane.showOptionDialog(this,
			    outputFile.getName() + " exists. Are you sure you want to overwrite?",
			    "Overwrite file",
			    JOptionPane.OK_CANCEL_OPTION,
			    JOptionPane.WARNING_MESSAGE,
			    null,
			    options,
			    options[1]);
			
			// exist if the user doesn't want to overwrite
			if (n != 0)
				return;
		}
		
		// create a new task
		exportMonitor = new ProgressMonitor(this, "Writing data to '" + outputFile.getName() + "'...", "", 0, 100);
		exportMonitor.setProgress(0);
		exportMonitor.setMillisToDecideToPopup(1);
		
		mgfExportWorker = new MgfExporterWorker(outputFile, inputParser, this);
		mgfExportWorker.addPropertyChangeListener(this);
		mgfExportWorker.execute();
	}

	private void onOpenPeakListFile() {
		// create a file chooser
		JFileChooser fc = new JFileChooser();
		fc.setFileFilter(new PeakListFileFilter());
		fc.setFileSelectionMode(JFileChooser.FILES_ONLY);
		fc.setMultiSelectionEnabled(false);
		fc.setDialogTitle("Open Peak List File");
		if (lastChosenDirectory != null)
			fc.setCurrentDirectory(lastChosenDirectory);
		
		// show the dialog
		int returnVal = fc.showOpenDialog(this);
		
		// make sure the user clicked OK
		if (returnVal != JFileChooser.APPROVE_OPTION)
			return;
		
		// save the last chosen directory
		lastChosenDirectory = fc.getCurrentDirectory();
		
		// load the spectra
		inputfile = fc.getSelectedFile();
		
		if (!inputfile.exists())
			return;
		
		loadSpectra();
	}
	
	private void onOpenPeakListDirectory() {
		// create a file chooser
		JFileChooser fc = new JFileChooser();
		fc.setFileFilter(new DirectoryFilter());
		fc.setFileSelectionMode(JFileChooser.DIRECTORIES_ONLY);
		fc.setMultiSelectionEnabled(false);
		fc.setDialogTitle("Open Peak List Directory");
		if (lastChosenDirectory != null)
			fc.setCurrentDirectory(lastChosenDirectory);
		
		// show the dialog
		int returnVal = fc.showOpenDialog(this);
		
		// make sure the user clicked OK
		if (returnVal != JFileChooser.APPROVE_OPTION)
			return;
		
		// save the last chosen directory
		lastChosenDirectory = fc.getCurrentDirectory();
		
		// load the spectra
		inputfile = fc.getSelectedFile();
		
		if (!inputfile.exists())
			return;
		
		loadSpectra();
	}
	
	/**
	 * Load the spectra from the given File.
	 */
	private void loadSpectra() {
		setCursor(Cursor.getPredefinedCursor(Cursor.WAIT_CURSOR));
		
		// reset the GUI
		resetGUI();
		
		// show that we don't know how long it takes to parse the file
		statusProgress.setIndeterminate(true);
		statusProgress.setString("Parsing peak list file...");
		statusProgress.setStringPainted(true);
		
		// get the peak list parser
		setPeakListWorker = new SetPeakListParserWorker();
		setPeakListWorker.execute();
	}
	
	@SuppressWarnings("unchecked")
	private void resetGUI() {
		// clear the table spectrum table and reset all elements		
		spectrumTableModel.setData(Collections.EMPTY_LIST);
		table.repaint();
		tableScrollPane.revalidate();
		spectrumBrowser.setPeaks(new double[0], new double[0]);
		
		// reset the status bar
		filenameLabel.setText(" No file loaded");
		numSpecLabel.setText("Number of Spectra: 0 ");
	}

	private void onPeakListFileOpened() {
		if (inputParser == null) {
			setCursor(Cursor.getPredefinedCursor(Cursor.DEFAULT_CURSOR));
			
			if (inputfile.isFile())
				JOptionPane.showMessageDialog(this, 
						(lastParsingError != null ? lastParsingError : "Failed to parse selected file."),
						"Failed to open peak list",
						JOptionPane.ERROR_MESSAGE);
			else
				JOptionPane.showMessageDialog(this, 
						"No supported peak list files found in selected directory.\nDirectories must only contain pkl or dta files with\none spectrum per file.",
						"Failed to open peak list directory",
						JOptionPane.ERROR_MESSAGE);
			
			resetGUI();
			
			statusProgress.setIndeterminate(false);
			statusProgress.setValue(0);
			statusProgress.setString("");
			
			btnCancelLoading.setEnabled(false);
			
			return;
		}
		
		filenameLabel.setText(" " + inputfile.getName() + " loaded.");
		numSpecLabel.setText("Number of Spectra: " + inputParser.getSpectraCount() + " ");
		
		// clear the table
		spectrumTableModel.clearTable();
		
		// set the progress bar
		statusProgress.setIndeterminate(false);
		statusProgress.setMaximum(100);
		statusProgress.setValue(0);
		statusProgress.setString("0% loaded");
		
		tableLoader = new TableLoaderWorker();
		tableLoader.addPropertyChangeListener(this);
		tableLoader.execute();
	}

	public void itemStateChanged(ItemEvent e) {
		System.out.println(e.paramString());
	}
	
	public void propertyChange(PropertyChangeEvent evt) {		
		// check if there's a progress update
		if ("progress".equals(evt.getPropertyName())) {
			statusProgress.setIndeterminate(false);
			statusProgress.setMaximum(100);
			statusProgress.setValue((Integer) evt.getNewValue());
			statusProgress.setString(evt.getNewValue().toString() + "% loaded");
		}
		else if ("exporter-progress".equals(evt.getPropertyName())) {
			if (exportMonitor == null)
				return;
			
			int progress = (Integer) evt.getNewValue();
			exportMonitor.setProgress(progress);
			exportMonitor.setNote(String.format("%d%% spectra exported\n", progress));
			
			if (exportMonitor.isCanceled()) {
				if (mgfExportWorker != null)
					mgfExportWorker.cancel(true);
			}
		}
	}
	
	/**
	 * This is only used to listen to the user
	 * selecting rows in the spectra table
	 */
	public void valueChanged(ListSelectionEvent e) {
		// ignore any intermediate calls
		if (e.getValueIsAdjusting())
			return;
		
		// get the current selection - there's only one selection possible
		int itemIndex = table.getSelectionModel().getLeadSelectionIndex();
		itemIndex = table.convertRowIndexToModel(itemIndex);
		
		// get the item's index
		String specId = (String) spectrumTableModel.getValueAt(itemIndex, 1);
		
		// get the peak list
		Spectrum spec;
		try {
			spec = inputParser.getSpectrumById(specId);
			
			if (spec == null) {
				System.out.println("Failed to load spectrum " + specId);
			}
			
			displaySpectrum(spec);
		} catch (JMzReaderException e1) {
			e1.printStackTrace();
			
			JOptionPane.showMessageDialog(this, 
					"Failed to open selected spectrum: " + e1.getMessage(),
					"Failed to open spectrum",
					JOptionPane.ERROR_MESSAGE);
		}
	}
	
	
	/**
	 * Display the peak list in the peak list
	 * component
	 * @param spec the spectrum to display.
	 */
	private void displaySpectrum(Spectrum spec) {
		// clear the peak list
		spectrumBrowser.setPeaks(new double[0], new double[0]);
		
		// make sure there's a spectrum passed
		if (spec == null)
			return;
		
		// transform the peak list
		List<Double> mzValues;
		if (spec.getPeakList() != null)
			mzValues = new ArrayList<Double>(spec.getPeakList().keySet());
		else
			mzValues = Collections.emptyList();
		
		double[] mz = new double[mzValues.size()];
		double[] intensities = new double[mzValues.size()];
		
		int index = 0;
		
		for (Double mzValue : mzValues) {
			mz[index] = mzValue;
			intensities[index] = spec.getPeakList().get(mzValue);
			
			index++;
		}
		
		spectrumBrowser.setPeaks(mz, intensities);
	}

	private class SetPeakListParserWorker extends SwingWorker<JMzReader, Void> {
		@Override
		public JMzReader doInBackground() throws Exception {
			lastParsingError = null;
			btnCancelLoading.setEnabled(true);
			return PeakListParserFactory.getInstance().getParser(inputfile);
		}

		@Override
		protected void done() {
			try {
				inputParser = get();
			} catch (CancellationException e) {
				inputParser = null;
				resetGUI();

				setCursor(Cursor.getPredefinedCursor(Cursor.DEFAULT_CURSOR));
				statusProgress.setIndeterminate(false);
				statusProgress.setString("");
				
				btnCancelLoading.setEnabled(false);
				
				return;
			}
			catch (InterruptedException e) {
				inputParser = null;
				e.printStackTrace();
			} catch (ExecutionException e) {
				inputParser = null;
				if (e.getCause() != null)
					lastParsingError = e.getCause().getMessage();
				e.printStackTrace();
			}
			
			btnCancelLoading.setEnabled(false);
			onPeakListFileOpened();
			
			super.done();
		}
	}
	
	private class TableLoaderWorker extends SwingWorker<List<List<Object>>, List<Object>> {
		private boolean firstChunk = true;
		
		@SuppressWarnings("unchecked")
		@Override
		protected List<List<Object>> doInBackground() throws Exception {
			btnCancelLoading.setEnabled(true);
			List<List<Object>> data = new ArrayList<List<Object>>();
			
			Iterator<Spectrum> scanIterator = inputParser.getSpectrumIterator();
			
			Integer nCurrentSpec = 1;
			
			// initialize the progress
			setProgress(0);
			
			while (scanIterator.hasNext()) {				
				// set the progress
				int progress = nCurrentSpec * 100 / inputParser.getSpectraCount();
				setProgress(progress);
				
				Spectrum s = scanIterator.next();	
				
				// make sure the spectrum isn't empty
				if (s == null)
					continue;
				
				ArrayList<Object> scanSummary = new ArrayList<Object>(7);
				
				scanSummary.add(nCurrentSpec++);
				scanSummary.add(s.getId());
				scanSummary.add(s.getMsLevel());
				scanSummary.add(s.getPrecursorMZ());
				scanSummary.add(s.getPrecursorIntensity());
				scanSummary.add(s.getPrecursorCharge());
				scanSummary.add(s.getPeakList() != null ? s.getPeakList().size() : 0);
				
				data.add(scanSummary);
				publish(scanSummary);
				
				if (isCancelled())
					return null;
			}
			
			return data;
		}
		
		@Override
		protected void process(List<List<Object>> chunks) {
			spectrumTableModel.addData(chunks);
			table.revalidate();
			//tableScrollPane.revalidate();
			
			if (firstChunk) {
				table.getSelectionModel().setSelectionInterval(0, 0);
				setCursor(Cursor.getPredefinedCursor(Cursor.DEFAULT_CURSOR));
				firstChunk = false;
			}
			
			super.process(chunks);
		}
		
		@Override
		protected void done() {
			try {
				get();
			} catch (CancellationException e) {
				// loading the table was canceled
				statusProgress.setString(statusProgress.getString() + " - cancelled");
			}
			catch (InterruptedException e) {
				//e.printStackTrace();
				JOptionPane.showMessageDialog(null, 
						"Loading spectra data into table interrupted.",
						"Loading data interrupted",
						JOptionPane.ERROR_MESSAGE);
			} catch (ExecutionException e) {
				e.printStackTrace();
				
				String why = e.getMessage();
				
				// get the final cause
				Throwable cause = e.getCause();
				Throwable previousCause = null;
				
				while (cause != null) {
					previousCause = cause;
					cause = cause.getCause();
				} 
				
//				if (previousCause != null)
//					why = previousCause.getMessage();
				
				JOptionPane.showMessageDialog(null, 
						"Failed to parse spectrum. " + why,
						"Failed to parse spectrum",
						JOptionPane.ERROR_MESSAGE);
			}
			
			setCursor(Cursor.getPredefinedCursor(Cursor.DEFAULT_CURSOR));
			
			table.repaint();
			tableScrollPane.revalidate();
			btnCancelLoading.setEnabled(false);
			
			super.done();
		}
		
	}
}
