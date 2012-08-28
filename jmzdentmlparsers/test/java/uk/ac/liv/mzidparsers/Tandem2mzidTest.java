package uk.ac.liv.mzidparsers;

import java.io.File;

import javax.xml.bind.Unmarshaller;

import junit.framework.TestCase;
import uk.ac.ebi.jmzidml.model.mzidml.MzIdentML;
import uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItem;



/**
 * This class can be used to test the results of Tandem2mzid transformation tool.
 * 
 *      
 * @author plukasse
 *
 */
public class Tandem2mzidTest extends TestCase 
{
	
	/**
	 * Testing the basic functionality
	 * @throws Exception 
	 */
	public void test_basic() throws Exception
	{
		String xTandemFile = "test/data/55merge_tandem.xml";
		String resultFile = "test/data/55merge_tandem.xml.mzid";
		new Tandem2mzid(xTandemFile, resultFile);
		
		
	}
	

	public void test_galaxyFile() throws Exception
	{
		String xTandemFileFromGalaxyStep = "test/data/Galaxy-[X_Tandem_on_data_N].bioml";
		new Tandem2mzid(xTandemFileFromGalaxyStep, xTandemFileFromGalaxyStep + ".mzid");
	}
		
	public void test_debug_File() throws Exception
	{
		String xTandemFileFromGalaxyStep = "test/data/DEBUG_MSMS_XTANDEM.bioml";
		String outMzid = xTandemFileFromGalaxyStep + ".mzid";
		new Tandem2mzid(xTandemFileFromGalaxyStep, outMzid);
		
		//Possible check if code below would work...:
		/*
		uk.ac.ebi.jmzidml.xml.io.MzIdentMLUnmarshaller unmarshaller = new uk.ac.ebi.jmzidml.xml.io.MzIdentMLUnmarshaller(new File(outMzid));
		SpectrumIdentificationItem firstIdentification = (SpectrumIdentificationItem)unmarshaller.unmarshal(".//SpectrumIdentificationItem");
		//Get 1st identification:
		assertTrue(firstIdentification.getExperimentalMassToCharge() == 464.285614);
		*/
	}
	
	public void test_multiple_sii_case() throws Exception
	{
		String xTandemFileFromGalaxyStep = "test/data/20091015_spiking_0_05fmol_CytC_MSMS_XTANDEM.out";
		String outMzid = xTandemFileFromGalaxyStep + ".mzid";
		new Tandem2mzid(xTandemFileFromGalaxyStep, outMzid);
	}
			
}
