package uk.ac.ebi.jmzidml.test.xml.unmarshaller;

import org.junit.Test;
import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.*;
import uk.ac.ebi.jmzidml.model.mzidml.params.*;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLUnmarshaller;

import javax.naming.ConfigurationException;
import javax.xml.bind.JAXBException;
import java.net.URL;
import java.util.List;
import java.util.Map;

import static org.junit.Assert.*;


/**
 * @author Florian Reisinger
 *         Date: 02-Dec-2010
 * @since 1.0
 */
public class MzIdentMLUnmarshallerTest {

    private static MzIdentMLUnmarshaller unmarshaller;

    static {
        URL xmlFileURL = MzIdentMLUnmarshallerTest.class.getClassLoader().getResource("Mascot_MSMS_example.mzid");
        assertNotNull(xmlFileURL);

        unmarshaller = new MzIdentMLUnmarshaller(xmlFileURL);
        assertNotNull(unmarshaller);
    }

    @Test
    public void testAttributeRetrieval() throws Exception {
        // Number of providers
        int total = unmarshaller.getObjectCountForXpath(MzIdentMLElement.AnalysisSoftware.getXpath());
        assertEquals(2, total);

        AnalysisSoftware software = unmarshaller.unmarshal(MzIdentMLElement.AnalysisSoftware);
        assertNotNull(software);
        String id = "AS_mascot_server";
        Map<String, String> attributes = unmarshaller.getElementAttributes(id, MzIdentMLElement.AnalysisSoftware.getClazz());
        assertNotNull(attributes);
        // at the very least the id attribute has to be present
        // (otherwise we would not have found the element!)
        assertEquals(id, attributes.get("id"));

        // there should also be 4 attributes in this case
        // the 'id' plus 'name', 'version' and 'URI' attributes
        assertEquals(4, attributes.keySet().size());
        assertEquals("Mascot Server", attributes.get("name"));
        assertEquals("2.2.03", attributes.get("version"));
        assertEquals("http://www.matrixscience.com/search_form_select.html", attributes.get("uri"));

        // test with another element
        Cv cv = unmarshaller.unmarshal(MzIdentMLElement.CV);
        assertNotNull(cv);

        id = "PSI-MS";
        attributes = unmarshaller.getElementAttributes(id, MzIdentMLElement.CV.getClazz());
        assertNotNull(attributes);
        // at the very least the id attribute has to be present
        // (otherwise we would not have found the element!)
        assertEquals(id, attributes.get("id"));

        // there should also be 4 attributes in this case
        // the 'id' plus 'fullName', 'version' and 'URI' attributes
        assertEquals(4, attributes.keySet().size());
        assertEquals("Proteomics Standards Initiative Mass Spectrometry Vocabularies", attributes.get("fullName"));
        assertEquals("2.25.0", attributes.get("version"));
        assertEquals("http://psidev.cvs.sourceforge.net/viewvc/*checkout*/psidev/psi/psi-ms/mzML/controlledVocabulary/psi-ms.obo", attributes.get("uri"));
    }


    @Test
    public void testRootAttributeRetrieval() throws Exception {
        // check the root element (mzIdentML) attributes
        assertEquals("example_mzidentml_1", unmarshaller.getMzIdentMLId());
        assertEquals("1.1.0", unmarshaller.getMzIdentMLVersion());
        assertEquals("test-mzid", unmarshaller.getMzIdentMLName());

        // now check all the other attributes
        String id = "example_mzidentml_1";
        Map<String, String> attributes = unmarshaller.getElementAttributes(id, MzIdentMLElement.MzIdentML.getClazz());
        assertNotNull(attributes);
        assertEquals(7, attributes.keySet().size());
        assertEquals("2009-08-18T17:59:55", attributes.get("creationDate"));
        assertEquals("http://psidev.info/psi/pi/mzIdentML/1.1", attributes.get("xmlns"));
        assertEquals("http://psidev.info/psi/pi/mzIdentML/1.1 ../resources/mzIdentML1.1.0.xsd", attributes.get("xsi:schemaLocation"));
        assertEquals("http://www.w3.org/2001/XMLSchema-instance", attributes.get("xmlns:xsi"));
    }

    @Test
    public void testEnzymes() throws JAXBException{
        SpectrumIdentificationProtocol spectrumIdentificactionProtocol = unmarshaller.unmarshal(SpectrumIdentificationProtocol.class, "SIP");
        Enzymes enzymes = spectrumIdentificactionProtocol.getEnzymes();
        assertTrue(enzymes.getEnzyme().get(0).getId().equals("ENZ_0"));
        assertTrue(enzymes.getEnzyme().size() == 2);
        Enzyme enzyme = enzymes.getEnzyme().get(0);
        assertTrue(enzyme.getId().equals("ENZ_0"));
        List<UserParam> enzymeNameUserParams = enzyme.getEnzymeName().getUserParam();
        assertTrue(enzymeNameUserParams.get(0) instanceof EnzymeNameUserParam);
        assertTrue(enzymeNameUserParams.get(0).getValue().equals("CNBr+Trypsin"));
        List<CvParam> enzymeNameCvParam = enzyme.getEnzymeName().getCvParam();
        assertTrue(enzymeNameCvParam.size() == 0);

        enzyme = enzymes.getEnzyme().get(1);
        assertTrue(enzyme.getId().equals("ENZ_1"));
        enzymeNameUserParams = enzyme.getEnzymeName().getUserParam();
        assertTrue(enzymeNameUserParams.get(0) instanceof EnzymeNameUserParam);
        assertTrue(enzymeNameUserParams.get(0).getValue().equals("CNBr+Trypsin"));
    }

    /**
     * Unmarshal Inputs and confirm that FileFormat adapter has successfully converted the
     * FileFormat  property into a CvParam in SourceFile
     */
    @Test
    public void testInputsUnmarshalWithFileFormatAdapter() {
        Inputs inputs = unmarshaller.unmarshal(Inputs.class);
        assertTrue(inputs.getSourceFile().get(0).getFileFormat().getCvParam().getAccession().equals("MS:1001199"));
    }

    /**
     * Unmarshal SpectraData and confirm that SpectrumIDFormat adapter has successfully converted the
     * SpectrumIDFormat property into a list of CvParams in SpectraData.
     */
    @Test
    public void testInputsUnmarshalWithSpectrumIDFormatAdapter() {
        SpectraData spectraData = unmarshaller.unmarshal(SpectraData.class);
        assertTrue(spectraData.getSpectrumIDFormat().getCvParam().getAccession().equals("MS:1001528"));
    }

    @Test
    public void testDataCollection() {
        DataCollection dataCollection = unmarshaller.unmarshal(DataCollection.class);
        dataCollection.getInputs();
    }

    @Test
    public void testOrganization() throws JAXBException {
        Organization organization = unmarshaller.unmarshal(Organization.class, "ORG_MSL");
        assertTrue(organization.getParamGroup().size()==4);
        assertTrue(organization.getName().equals("Matrix Science Limited"));
        // Test facadelist
        List<CvParam> orgCvParams = organization.getCvParam();
        assertTrue(orgCvParams.size() == 2);
        assertTrue(orgCvParams.get(0).getAccession().equals("MS:1000589"));
        List<UserParam> orgUserParams = organization.getUserParam();
        assertTrue(orgUserParams.size() == 2);
        assertTrue(orgUserParams.get(0).getName().equals("contact phone"));

    }

    @Test
    public void testProvider() throws JAXBException {
        Provider provider = unmarshaller.unmarshal(Provider.class, "PROVIDER");
        assertTrue(provider.getContactRole().getRole().getCvParam().getAccession().equals("MS:1001271"));
    }

    @Test
    public void testAnalysisSampleCollection() {
        AnalysisSampleCollection analysisSampleCollection = unmarshaller.unmarshal(AnalysisSampleCollection.class);
        Sample sample = analysisSampleCollection.getSample().get(0);
        // Test facade list
        List<CvParam> sampleCvParams = sample.getCvParam();
        assertTrue(sample.getParamGroup().size() == 2);
        assertTrue(sampleCvParams.size() == 1);
        assertTrue(sampleCvParams.get(0).getUnitCvRef().equals("UO"));
        List<UserParam> sampleUserParams = sample.getUserParam();
        assertTrue(sampleUserParams.size() == 1);
        assertTrue(sampleUserParams.get(0).getUnitCvRef().equals("UO"));
    }


    @Test
    public void testAuditCollection() throws JAXBException {
        Organization organization = unmarshaller.unmarshal(Organization.class, "ORG_MSL");
        List<CvParam> cvParams = organization.getCvParam();
        List<UserParam> userParams = organization.getUserParam();
        assertTrue(cvParams.size() == 2);
        assertTrue(userParams.size() == 2);
        CvParam cvParam = cvParams.get(0);
        assertTrue(cvParam.getAccession().equals("MS:1000589"));
        UserParam userParam = userParams.get(0);
        assertTrue(userParam.getValue().equals("+44 (0)20 7486 1050"));
    }

    @Test
    public void testProteinDetectionProtocol() throws JAXBException{
        ProteinDetectionProtocol proteinDetectionProtocol = unmarshaller.unmarshal(ProteinDetectionProtocol.class, "PDP_MascotParser_1");
        ThresholdCvParam thresholdCvParam = (ThresholdCvParam)proteinDetectionProtocol.getThreshold().getCvParam().get(0);
        assertTrue(thresholdCvParam.getAccession().equals("MS:1001494"));
        assertTrue(thresholdCvParam.getCvRef().equals("PSI-MS"));
        assertTrue(thresholdCvParam.getName().equals("no threshold"));

        List<CvParam> analysisParams = proteinDetectionProtocol.getAnalysisParams().getCvParam();
        assertTrue(analysisParams.size() == 10);
        AnalysisParamsCvParam analysisParamsCvParam = (AnalysisParamsCvParam)analysisParams.get(0);
        assertTrue(analysisParamsCvParam.getAccession().equals("MS:1001316"));
        assertTrue(analysisParamsCvParam.getName().equals("mascot:SigThreshold"));
        assertTrue(analysisParamsCvParam.getValue().equals("0.05"));

        analysisParamsCvParam = (AnalysisParamsCvParam)analysisParams.get(4);
        assertTrue(analysisParamsCvParam.getAccession().equals("MS:1001320"));
        assertTrue(analysisParamsCvParam.getName().equals("mascot:ShowHomologousProteinsWithSamePeptides"));
        assertTrue(analysisParamsCvParam.getValue().equals("1"));

        analysisParamsCvParam = (AnalysisParamsCvParam)analysisParams.get(9);
        assertTrue(analysisParamsCvParam.getAccession().equals("MS:1001325"));
        assertTrue(analysisParamsCvParam.getName().equals("mascot:ShowDecoyMatches"));
        assertTrue(analysisParamsCvParam.getValue().equals("0"));

    }

    @Test
    public void testSpectrumIdentificationProtocol() throws JAXBException{
        SpectrumIdentificationProtocol spectrumIdentificationProtocol = unmarshaller.unmarshal(SpectrumIdentificationProtocol.class, "SIP");
        List<CvParam> additionalSearchCvParams = spectrumIdentificationProtocol.getAdditionalSearchParams().getCvParam();
        assertTrue(additionalSearchCvParams.size() == 10);
        AdditionalSearchParamsCvParam additionalSearchParamsCvParam = (AdditionalSearchParamsCvParam)additionalSearchCvParams.get(0);
        assertTrue(additionalSearchParamsCvParam.getAccession().equals("MS:1001211"));
        assertTrue(additionalSearchParamsCvParam.getName().equals("parent mass type mono"));

        List<UserParam> additionalSearchUserParams = spectrumIdentificationProtocol.getAdditionalSearchParams().getUserParam();
        assertTrue(additionalSearchUserParams.size() == 2);
        AdditionalSearchParamsUserParam additionalSearchParamsUserParam = (AdditionalSearchParamsUserParam) additionalSearchUserParams.get(0);
        assertTrue(additionalSearchParamsUserParam.getName().equals("Mascot User Comment"));
        assertTrue(additionalSearchParamsUserParam.getValue().equals("Example Mascot MS-MS search for PSI mzIdentML"));

        Param searchType = spectrumIdentificationProtocol.getSearchType();
        assertTrue(searchType.getCvParam() instanceof SearchTypeCvParam);

    }

    @Test
    public void testAnalysisSearchDatabase() throws JAXBException{
        SearchDatabase searchDb = unmarshaller.unmarshal(SearchDatabase.class, "SDB_SwissProt");
        Param dbName = searchDb.getDatabaseName();
        assertTrue(dbName.getUserParam() instanceof DatabaseNameUserParam);
    }

    @Test
    public void testFilter() throws JAXBException{
        SpectrumIdentificationProtocol spectrumIdentificationProtocol = unmarshaller.unmarshal(SpectrumIdentificationProtocol.class, "SIP");
        DatabaseFilters dbFilters = spectrumIdentificationProtocol.getDatabaseFilters();
        List<Filter> filter = dbFilters.getFilter();
        assertTrue(filter.size() == 1);
        IncludeCvParam includeCvParam = (IncludeCvParam)filter.get(0).getInclude().getCvParam().get(0);
        assertTrue(includeCvParam.getAccession().equals("MS:1001467"));
        assertTrue(includeCvParam.getValue().equals("33208"));
        assertTrue(filter.get(0).getFilterType().getCvParam() instanceof FilterTypeCvParam);
        assertTrue(filter.get(0).getFilterType().getCvParam().getAccession().equals("MS:1001020"));
    }

    @Test
    public void testAnalysisSoftware() throws JAXBException{
        AnalysisSoftware analysisSoftware = unmarshaller.unmarshal(AnalysisSoftware.class, "AS_mascot_parser");
        Param param = analysisSoftware.getSoftwareName();
        assertTrue(param.getCvParam() instanceof SoftwareNameCvParam);
    }

    @Test
    public void testIonType() throws JAXBException {
        SpectrumIdentificationList list =  unmarshaller.unmarshal(SpectrumIdentificationList.class, "SIL_1");
        List<IonType> ionTypes = list.getSpectrumIdentificationResult().get(0).getSpectrumIdentificationItem().get(0).getFragmentation().getIonType();
        assertTrue(ionTypes.size() > 0);
        IonType ionType = ionTypes.get(0);
        List<Integer> index = ionType.getIndex();
        assertTrue(index.size() == 5);
        assertTrue(index.get(0) == 1);
        assertTrue(index.get(1) == 2);
        assertTrue(index.get(4) == 8);
        /**
         * Check if instances of Integer are returned. By default jaxb converts int in xml to BigInts in Java.
         */
        assertTrue(index.get(0) instanceof Integer);
    }

    @Test
    public void testMassTable() throws JAXBException {
        SpectrumIdentificationProtocol spectrumIdentificationProtocol = unmarshaller.unmarshal(SpectrumIdentificationProtocol.class, "SIP");
        List<MassTable> massTables = spectrumIdentificationProtocol.getMassTable();
        assertTrue(massTables.size() > 0);
        MassTable massTable = massTables.get(0);
        assertTrue(massTable.getId().equals("MT"));
        List<Integer> msLevels = massTable.getMsLevel();
        assertTrue(msLevels.size() == 2);
        /**
         * Check if instances of Integer are returned. By default jaxb converts int in xml to BigInts in Java.
         */
        assertTrue(msLevels.get(0).getClass() == Integer.class);
    }

    @Test
    public void testPeptideEvidence(){
        MzIdentML mzIdentML = unmarshaller.unmarshal(MzIdentML.class);
        List<PeptideEvidence> peptideEvidences =
                mzIdentML.getSequenceCollection().getPeptideEvidence();
        assertTrue(peptideEvidences.size() == 56);
        PeptideEvidence pe = peptideEvidences.get(0);
        assertTrue(pe.getId().equals("PE_1_1_HSP70_ECHGR_0"));

        if (MzIdentMLElement.PeptideEvidence.isAutoRefResolving() && pe.getDBSequenceRef() != null) {
            DBSequence dbSeq = pe.getDBSequence();
            assertTrue(dbSeq.getAccession().equals("HSP70_ECHGR"));
        }else{
             System.out.println("PeptideEvidence is not auto-resolving.");
        }
    }


    @Test
    public void testIDForElement() throws ConfigurationException {
        int numOfPE = unmarshaller.getIDsForElement(MzIdentMLElement.PeptideEvidence).size();
        assertEquals("number of PeptideEvidence not as expected.", 56, numOfPE);
    }
}