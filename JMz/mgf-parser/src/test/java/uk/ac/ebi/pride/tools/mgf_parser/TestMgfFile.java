package uk.ac.ebi.pride.tools.mgf_parser;

import java.io.File;
import java.net.URL;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

import junit.framework.TestCase;
import uk.ac.ebi.pride.tools.jmzreader.JMzReaderException;
import uk.ac.ebi.pride.tools.jmzreader.model.IndexElement;
import uk.ac.ebi.pride.tools.jmzreader.model.Spectrum;
import uk.ac.ebi.pride.tools.mgf_parser.model.Ms2Query;
import uk.ac.ebi.pride.tools.mgf_parser.model.PmfQuery;

public class TestMgfFile extends TestCase {
	private MgfFile mgfFile;
	private File sourceFile;

	protected void setUp() throws Exception {
		mgfFile = new MgfFile();
	}
	
	private void loadTestFile() {		
		URL testFile = getClass().getClassLoader().getResource("F001257.mgf");
        assertNotNull("Error loading mgf test file", testFile);
        
		try {
			sourceFile = new File(testFile.toURI());
			
			mgfFile = new MgfFile(sourceFile);
		} catch (Exception e) {
			fail("Faild to load test file");
		}
	}

	public void testGetAccessions() {
		loadTestFile();
		assertEquals(3, mgfFile.getAccessions().size());
		
		assertEquals("P12346", mgfFile.getAccessions().get(1));
		assertEquals("P12347", mgfFile.getAccessions().get(2));
	}

	public void testSetAccessions() {
		ArrayList<String> accessions = new ArrayList<String>();
		accessions.add("P12345");
		accessions.add("P12346");
		
		mgfFile.setAccessions(accessions);
		
		String mgfString = mgfFile.toString();
		
		assertEquals("ACCESSION=\"P12345\",\"P12346\"\n", mgfString);
	}

	public void testGetCharge() {
		loadTestFile();
		
		assertEquals("2+,3+,4+,5+", mgfFile.getCharge());
	}

	public void testSetCharge() {
		mgfFile.setCharge("8-,5-,4-,3-");
		
		assertEquals("CHARGE=8-,5-,4-,3-\n", mgfFile.toString());
	}

	public void testGetEnzyme() {
		loadTestFile();
		assertEquals("Trypsin", mgfFile.getEnzyme());
	}

	public void testSetEnzyme() {
		mgfFile.setEnzyme("Trypsin");
		
		assertEquals("CLE=Trypsin\n", mgfFile.toString());
	}

	public void testGetSearchTitle() {
		loadTestFile();
		assertEquals("First test experiment (values are not real)", mgfFile.getSearchTitle());
	}

	public void testSetSearchTitle() {
		mgfFile.setSearchTitle("My first test experiment");
		
		assertEquals("COM=My first test experiment\n", mgfFile.toString());
	}

	public void testGetPrecursorRemoval() {
		loadTestFile();
		assertEquals("20,120", mgfFile.getPrecursorRemoval());
	}

	public void testSetPrecursorRemoval() {
		mgfFile.setPrecursorRemoval("10,120");
		assertEquals("CUTOUT=10,120\n", mgfFile.toString());
	}

	public void testGetDatabase() {
		loadTestFile();
		assertEquals("SwissProt v57", mgfFile.getDatabase());
	}

	public void testSetDatabase() {
		mgfFile.setDatabase("UniProt 1");
		assertEquals("DB=UniProt 1\n", mgfFile.toString());
	}

	public void testGetPerformDecoySearch() {
		loadTestFile();
		assertEquals(Boolean.FALSE, mgfFile.getPerformDecoySearch());
	}

	public void testSetPerformDecoySearch() {
		mgfFile.setPerformDecoySearch(true);
		assertEquals("DECOY=1\n", mgfFile.toString());
	}

	public void testGetIsErrorTolerant() {
		loadTestFile();
		assertEquals(Boolean.TRUE, mgfFile.getIsErrorTolerant());
	}

	public void testSetIsErrorTolerant() {
		mgfFile.setIsErrorTolerant(false);
		assertEquals("ERRORTOLERANT=0\n", mgfFile.toString());
	}

	public void testGetFormat() {
		loadTestFile();
		assertEquals("Mascot generic", mgfFile.getFormat());
	}

	public void testSetFormat() {
		mgfFile.setFormat("Sequest (.DTA)");
		assertEquals("FORMAT=Sequest (.DTA)\n", mgfFile.toString());
	}

	public void testGetFrames() {
		loadTestFile();
		assertEquals(6, mgfFile.getFrames().size());
		assertEquals(new Integer(5), mgfFile.getFrames().get(4));
		assertEquals(new Integer(3), mgfFile.getFrames().get(2));
	}

	public void testSetFrames() {
		ArrayList<Integer> frames = new ArrayList<Integer>();
		frames.add(2);
		frames.add(4);
		frames.add(5);
		
		mgfFile.setFrames(frames);
		assertEquals("FRAMES=2,4,5\n", mgfFile.toString());
	}

	public void testGetInstrument() {
		loadTestFile();
		assertEquals("ESI-QUAD", mgfFile.getInstrument());
	}

	public void testSetInstrument() {
		mgfFile.setInstrument("Default");
		assertEquals("INSTRUMENT=Default\n", mgfFile.toString());
	}

	public void testGetVariableModifications() {
		loadTestFile();
		assertEquals("Oxidation (M)", mgfFile.getVariableModifications());
	}

	public void testSetVariableModifications() {
		mgfFile.setVariableModifications("My mod");
		assertEquals("IT_MODS=My mod\n", mgfFile.toString());
	}

	public void testGetFragmentIonTolerance() {
		loadTestFile();
		assertEquals(0.5, mgfFile.getFragmentIonTolerance());
	}

	public void testSetFragmentIonTolerance() {
		mgfFile.setFragmentIonTolerance(0.3);
		assertEquals("ITOL=0.3\n", mgfFile.toString());
	}

	public void testGetFragmentIonToleranceUnit() {
		loadTestFile();
		assertEquals(MgfFile.FragmentToleranceUnits.DA, mgfFile.getFragmentIonToleranceUnit());
	}

	public void testSetFragmentIonToleranceUnit() {
		mgfFile.setFragmentIonToleranceUnit(MgfFile.FragmentToleranceUnits.MMU);
		assertEquals("ITOLU=mmu\n", mgfFile.toString());
	}

	public void testGetMassType() {
		loadTestFile();
		assertEquals(MgfFile.MassType.MONOISOTOPIC, mgfFile.getMassType());
	}

	public void testSetMassType() {
		mgfFile.setMassType(MgfFile.MassType.AVERAGE);
		assertEquals("MASS=Average\n", mgfFile.toString());
	}

	public void testGetFixedMofications() {
		loadTestFile();
		assertEquals("Carbamidomethylation (C)", mgfFile.getFixedMofications());
	}

	public void testSetFixedMofications() {
		mgfFile.setFixedMofications("Another mod");
		assertEquals("MODS=Another mod\n", mgfFile.toString());
	}

	public void testGetPeptideIsotopeError() {
		loadTestFile();
		assertEquals(1.3, mgfFile.getPeptideIsotopeError());
	}

	public void testSetPeptideIsotopeError() {
		mgfFile.setPeptideIsotopeError(1.9);
		assertEquals("PEP_ISOTOPE_ERROR=1.9\n", mgfFile.toString());
	}

	public void testGetPartials() {
		loadTestFile();
		assertEquals(new Integer(1), mgfFile.getPartials());
	}

	public void testSetPartials() {
		mgfFile.setPartials(2);
		assertEquals("PFA=2\n", mgfFile.toString());
	}

	public void testGetPrecursor() {
		loadTestFile();
		assertEquals(1047.0, mgfFile.getPrecursor());
	}

	public void testSetPrecursor() {
		mgfFile.setPrecursor(1011.0);
		assertEquals("PRECURSOR=1011.0\n", mgfFile.toString());
	}

	public void testGetQuantitation() {
		loadTestFile();
		assertEquals("iTRAQ 4plex", mgfFile.getQuantitation());
	}

	public void testSetQuantitation() {
		mgfFile.setQuantitation("SILAC");
		assertEquals("QUANTITATION=SILAC\n", mgfFile.toString());
	}

	public void testGetMaxHitsToReport() {
		loadTestFile();
		assertEquals("1500", mgfFile.getMaxHitsToReport());
	}

	public void testSetMaxHitsToReport() {
		mgfFile.setMaxHitsToReport("Auto");
		assertEquals("REPORT=Auto\n", mgfFile.toString());
	}

	public void testGetReportType() {
		loadTestFile();
		assertEquals(MgfFile.ReportType.PEPTIDE, mgfFile.getReportType());
	}

	public void testSetReportType() {
		mgfFile.setReportType(MgfFile.ReportType.PROTEIN);
		assertEquals("REPTYPE=protein\n", mgfFile.toString());
	}

	public void testGetSearchType() {
		loadTestFile();
		assertEquals(MgfFile.SearchType.MIS, mgfFile.getSearchType());
	}

	public void testSetSearchType() {
		mgfFile.setSearchType(MgfFile.SearchType.PMF);
		assertEquals("SEARCH=PMF\n", mgfFile.toString());
	}

	public void testGetProteinMass() {
		loadTestFile();
		assertEquals("10489", mgfFile.getProteinMass());
	}

	public void testSetProteinMass() {
		mgfFile.setProteinMass("1010");
		assertEquals("SEG=1010\n", mgfFile.toString());
	}

	public void testGetTaxonomy() {
		loadTestFile();
		assertEquals("Human 9606", mgfFile.getTaxonomy());
	}

	public void testSetTaxonomy() {
		mgfFile.setTaxonomy("My taxon");
		assertEquals("TAXONOMY=My taxon\n", mgfFile.toString());
	}

	public void testGetPeptideMassTolerance() {
		loadTestFile();
		assertEquals(0.2, mgfFile.getPeptideMassTolerance());
	}

	public void testSetPeptideMassTolerance() {
		mgfFile.setPeptideMassTolerance(0.3);
		assertEquals("TOL=0.3\n", mgfFile.toString());
	}

	public void testGetPeptideMassToleranceUnit() {
		loadTestFile();
		assertEquals(MgfFile.PeptideToleranceUnit.PPM, mgfFile.getPeptideMassToleranceUnit());
	}

	public void testSetPeptideMassToleranceUnit() {
		mgfFile.setPeptideMassToleranceUnit(MgfFile.PeptideToleranceUnit.PERCENT);
		assertEquals("TOLU=%\n", mgfFile.toString());
	}

	public void testGetUserParameter() {
		loadTestFile();
		assertEquals(3, mgfFile.getUserParameter().size());
		assertEquals("2nd user param", mgfFile.getUserParameter().get(1));
	}

	public void testSetUserParameter() {
		ArrayList<String> params = new ArrayList<String>();
		params.add("My param");
		params.add("Another param");
		
		mgfFile.setUserParameter(params);
		
		assertEquals("USER00=My param\nUSER01=Another param\n", mgfFile.toString());
	}

	public void testGetUserMail() {
		loadTestFile();
		assertEquals("jgriss@ebi.ac.uk", mgfFile.getUserMail());
	}

	public void testSetUserMail() {
		mgfFile.setUserMail("another@mail");
		assertEquals("USEREMAIL=another@mail\n", mgfFile.toString());
	}

	public void testGetUserName() {
		loadTestFile();
		assertEquals("Johannes Griss", mgfFile.getUserName());
	}

	public void testSetUserName() {
		mgfFile.setUserName("Another name");
		assertEquals("USERNAME=Another name\n", mgfFile.toString());
	}

	public void testGetPmfQueries() {
		loadTestFile();
		assertEquals(6, mgfFile.getPmfQueries().size());
		assertEquals(1223.145, mgfFile.getPmfQueries().get(1).getMass());
		assertEquals(3092.0, mgfFile.getPmfQueries().get(3).getIntensity());
	}

	public void testSetPmfQueries() {
		ArrayList<PmfQuery> queries = new ArrayList<PmfQuery>();
		
		queries.add(new PmfQuery(10.0, 10.0));
		queries.add(new PmfQuery(20.0, 20.0));
		queries.add(new PmfQuery(30.0, null));
		
		mgfFile.setPmfQueries(queries);
		
		assertEquals("10.0 10.0\n20.0 20.0\n30.0\n\n", mgfFile.toString());
	}

	public void testSetMs2Queries() {
		Ms2Query query;
		try {
			query = new Ms2Query("BEGIN IONS\nPEPMASS=406.283\n145.119100 8\n217.142900 75\n409.221455 11\n438.314735 46\n567.400183 24\nEND IONS\n", 1);
			
			ArrayList<Ms2Query> queries = new ArrayList<Ms2Query>();
			queries.add(query);
			
			mgfFile.setMs2Queries(queries);
			
			assertEquals("BEGIN IONS\nPEPMASS=406.283\n145.1191 8.0\n217.1429 75.0\n409.221455 11.0\n438.314735 46.0\n567.400183 24.0\nEND IONS\n\n", mgfFile.toString());
		} catch (JMzReaderException e) {
			e.printStackTrace();
		}
	}

	public void testGetMs2QueryCount() {
		loadTestFile();
		assertEquals(10, mgfFile.getMs2QueryCount());
	}

	public void testGetMs2Query() {
		loadTestFile();
		
		try {
			assertNotNull(mgfFile.getMs2Query(7));
		} catch (JMzReaderException e) {
			fail(e.getMessage());
		}
	}

	public void testGetMs2QueryIterator() {
		loadTestFile();
		
		int queryCount = 0;
		
		try {
			for (Ms2Query q : mgfFile.getMs2QueryIterator()) {
				assertNotNull(q);
				queryCount++;
			}
			assertEquals(10, queryCount);
		} catch (JMzReaderException e) {
			fail(e.getMessage());
		}
	}
	
	public void testMgfFile() {
		loadTestFile();
		
		// get the index
		List<IndexElement> index = mgfFile.getIndex();
		
		// create the new file
		MgfFile newFile;
		try {
			newFile = new MgfFile(sourceFile, index);
			
			Iterator<Ms2Query> it1 = mgfFile.getMs2QueryIterator();
			Iterator<Ms2Query> it2 = newFile.getMs2QueryIterator();
			
			while (it1.hasNext() && it2.hasNext()) {
				assertEquals(it1.next().toString(), it2.next().toString());
			}
		} catch (JMzReaderException e) {
			fail(e.getMessage());
		}
	}
	
	public void testGetIndex() {
		try {
			loadTestFile();
			List<IndexElement> index = mgfFile.getMsNIndexes(2);
			
			Spectrum s = mgfFile.getSpectrumByIndex(3);
			
			Spectrum s1 = MgfFile.getIndexedSpectrum(sourceFile, index.get(2));
			
			assertEquals(s.toString(), s1.toString());
		}
		catch (Exception e) {
			e.printStackTrace();
			fail(e.getMessage());
		}
	}
}
