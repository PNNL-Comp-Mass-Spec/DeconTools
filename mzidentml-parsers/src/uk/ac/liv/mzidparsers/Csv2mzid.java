/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package uk.ac.liv.mzidparsers;

import java.net.URL;
import uk.ac.ebi.JmzIdentMLParser;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLMarshaller;
import au.com.bytecode.opencsv.CSVReader;
import java.io.*;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;
import java.util.List;
import uk.ac.ebi.jmzidml.model.mzidml.*;




/**
 *
 * @author jonesar
 */
public class Csv2mzid {
    
    private MzIdentMLMarshaller marshaller;
    private String outFile = "temp.mzid";
    //private static String inputCsvFile = "src/resources/Toxo_1D_Slice43_omssa.csv";
    private String parser_params =  "build/classes/resources/toxo_omssa_params.csv";
    private String inputCsvFile =  "build/classes/resources/Toxo_1D_Slice43_omssa_fiddle_ranks_for_testing.csv";
    //private URL configFile =  this.getClass().getResource("/resources/csv_config_file.csv");
    private InputStream inputConfigFile =  this.getClass().getResourceAsStream("/resources/csv_config_file.csv");

    
    private HashMap<String,String> dataTypeMap = new HashMap();         //mapping for config file from internal datatypes to datatypes specified by each Software
    private String softwareName = "";
    
    //These will be set after reading the config file - 
    private String cvAccForEvalue = "";
    private String cvNameForEvalue = "";    
    private String cvAccForPvalue = "";
    private String cvNameForPvalue = "";
    
    private String cvScoreToOrderBy = "MS:1001328";     //This is a default only

    private HashMap<String,Peptide> peptideIDToPeptideMap = new HashMap();
    private HashMap<String,PeptideEvidence> idToPeptideEvidenceMap = new HashMap();
    private HashMap<String,SpectrumIdentificationResult> spectrumIDToSIRMap = new HashMap();
    
    //Param	cvTerm	Accession	Value  (structure of the parameter file containing search metadata)
    private HashMap<String,String> paramToCvParamName = new HashMap();
    private HashMap<String,String> paramToCvParamValue = new HashMap();
    private HashMap<String,String> paramToCvParamAcc = new HashMap();
    
    //Mods results name	Unimod name	Unimod ID	Residue	Fixed	Mass Delta   (structure of the parameter file containing the mods searched)
    private HashMap<String,String> modNameToUnimodName = new HashMap();
    private HashMap<String,String> modNameToUnimodID = new HashMap();
    private HashMap<String,String> modNameToResidue = new HashMap();
    private HashMap<String,Boolean> modNameToIsFixed = new HashMap();
    private HashMap<String,Float> modNameToMassDelta = new HashMap();
    
    private HashMap<String,DBSequence> accessionToDBSequenceHashMap = new HashMap();
    static String decoyRegex = null; 
    ReadUnimod unimodDoc;
    
    static String siiListID = "SII_LIST_1";
    static String spectraDataID = "SID_1";
    static String psiCvID = "PSI-MS";
    static String siProtocolID = "SearchProtocol_1";
    static String searchDBID = "SearchDB_1";
    static String pepEvidenceListID = "PepEvidList_1";
    static String analysisSoftID = "ID_software";
    static String specIdentID = "SpecIdent_1";
    static String unimodID = "UNIMOD";
    static String unitCvID = "UO";
    static String measureMzID = "Measure_MZ";
    static String measureIntID = "Measure_Int";
    static String measureErrorID = "Measure_Error";
    static String sourceFileID = "SourceFile_1";
    
        //Some objects we will need globally
    private Cv unimodCV;
    private Cv psiCV;
    private Cv unitCV;
    private SpectrumIdentificationProtocol siProtocol;
    private SearchDatabase searchDB;
    private SpectraData spectraData;
    private Person docOwner;
    private SequenceCollection sequenceCollection;
    private SpectrumIdentificationList siList;
    private CvList cvList;
    private AnalysisSoftwareList analysisSoftwareList;
    private Provider provider;
    private AuditCollection auditCollection;
    private AnalysisSampleCollection analysisSampleCollection;
    private AnalysisCollection analysisCollection;
    private AnalysisProtocolCollection analysisProtocolCollection;
    private Inputs inputs;

    private AnalysisSoftware analysisSoftware;
    
    public static void main(String[] args) {
           
        //For internal testing only
        Csv2mzid csv2mzid = new Csv2mzid();

        
    }
    
    /*
     * Constructor for testing with in-built example files.
     */
    public Csv2mzid(){
        
        try{            

            System.out.println("Running conversion for the example files...");
            this.init();

        }
        catch(Exception e){
            e.printStackTrace();
        }
    }
    
    public Csv2mzid(String inputfile, String outputfile, String paramsFile, String cvParamAccForRankingPSMs, String decoyRegularExpression){

        try{
            
            this.decoyRegex = decoyRegularExpression;
            this.inputCsvFile = inputfile;
            this.outFile = outputfile;
            this.parser_params = paramsFile;
            this.cvScoreToOrderBy = cvParamAccForRankingPSMs;
            
            this.init();
        }
        catch(Exception e){
            e.printStackTrace();
        }
    }
    
    public Csv2mzid(String inputfile, String outputfile, String paramsFile, String cvParamAccForRankingPSMs){

        try{
            
            this.inputCsvFile = inputfile;
            this.outFile = outputfile;
            this.parser_params = paramsFile;
            
            this.cvScoreToOrderBy = cvParamAccForRankingPSMs;
            
            this.init();

        }
        catch(Exception e){
            e.printStackTrace();
        }
    }
    
    /*
     * Method to set going to conversion once all the global variables have been set
     */
    private void init(){
        try{
            unimodDoc = new ReadUnimod();
            marshaller = new MzIdentMLMarshaller();

            buildParameters();
            readConfigFile();
            buildMetaDeta();

            buildPSMs();            
            handleAnalysisCollection("TIME:TODO");
            //parseFile(omxFile);
            writeMzidFile();

        }
        catch(Exception e){
            e.printStackTrace();
        }
    }
    
    
    private void readConfigFile(){
        
         try{
            //System.out.println("Config file location:" + configFile.toURI());
            //File file = new File(configFile.getPath());
            //InputStreamReader reader = new InputStreamReader(inputConfigFile);
            
            CSVReader reader = new CSVReader(new InputStreamReader(inputConfigFile));
            String [] nextLine;

            
            int lineCounter = 0;
            Boolean mappingCVTerms = false;
            
            int softwareColumn = -1;        //Needs to match header to know how data type mappings will work
            while ((nextLine = reader.readNext()) != null) {
                // nextLine[] is an array of values from the line
                
                String firstCell = nextLine[0].trim();                
                
                
                if(firstCell != null){
                    if(lineCounter==0){
                        for(int i=1; i<nextLine.length;i++){
                            if(nextLine[i].equals(softwareName)){
                                softwareColumn=i;
                                break;
                            } 
                        }
                        
                        if(softwareColumn == -1){
                            System.out.println("No valid software mapping found from params file \"Software Name\":" + softwareName + " value to config file column header, exiting...");
                            System.exit(1);
                        }
                    }
                    else if(firstCell.equals("CV mappings")){     //Reached the part of the file that deals with the mods
                        mappingCVTerms=true;
                    }
                    else{
                        if(!mappingCVTerms){
                            //Just grab the column with the correct software name
                            dataTypeMap.put(nextLine[0], nextLine[softwareColumn]);     
                            
                        }
                        else{
                            if(!firstCell.equals("Data type")){
   
                                if(nextLine[0].equals("evalue") && nextLine[3].equals(softwareName)){
                                    cvAccForEvalue = nextLine[2];
                                    cvNameForEvalue = nextLine[1];
                                }
                                else if(nextLine[0].equals("pvalue") && nextLine[3].equals(softwareName)){
                                    cvAccForPvalue = nextLine[2];
                                    cvNameForPvalue = nextLine[1];
                                }
    
                            }
                        }
                    }
                    lineCounter++;
                }
            }
            
            if(cvAccForEvalue.equals("") || cvAccForPvalue.equals("") || cvNameForPvalue.equals("") || cvNameForEvalue.equals("")){
                System.out.println("Error - not recognized e or p-value equivalent from config file");
                System.exit(1);
            }
            
         }
         catch(Exception e){
            e.printStackTrace();
        }
        
    }
    
    private void buildParameters(){
        
        try{
            CSVReader reader = new CSVReader(new FileReader(parser_params));
            String [] nextLine;

            
            int lineCounter = 0;
            Boolean parsingMods = false;
            while ((nextLine = reader.readNext()) != null) {
                // nextLine[] is an array of values from the line
                
                String firstCell = nextLine[0];                
                
                
                if(firstCell != null){
                    
                    if(lineCounter==0){
                         //ignore headers
                    }
                    else if(firstCell.equals("Mods results name")){     //Reached the part of the file that deals with the mods
                        parsingMods=true;
                    }
                    else{
                        if(!parsingMods){
                            paramToCvParamName.put(nextLine[0], nextLine[1]);
                            paramToCvParamAcc.put(nextLine[0], nextLine[2]);
                            paramToCvParamValue.put(nextLine[0], nextLine[3]);
                            //System.out.println("Read param: " + nextLine[0] + " " + nextLine[1] + " " + nextLine[2] + " " + nextLine[3]);

                        }
                        else{
                            
                            modNameToUnimodName.put(nextLine[0], nextLine[1]);
                            modNameToUnimodID.put(nextLine[0], nextLine[2]);
                            modNameToResidue.put(nextLine[0], nextLine[3]);
                            modNameToIsFixed.put(nextLine[0], Boolean.parseBoolean(nextLine[4]));
                            modNameToMassDelta.put(nextLine[0], Float.parseFloat(nextLine[5]));
                            
                        }                        
                    }
                    lineCounter++;
                }
           
            }
            
            softwareName = paramToCvParamName.get("Software name");
            if(softwareName==null){
                System.out.println("Error, params file did not contain a software name - required for configuring converter. Exiting..");
                System.exit(1);
            }
            
        }
        catch(Exception e){
            e.printStackTrace();
        }
        
    }
    

    
    private void buildMetaDeta(){
        
       handleCVs();        
       handleAnalysisSoftware();      //TODO - not clear that we can get the software version from the results file
       
       handleAuditCollection("firstname","secondName","email@place.com","address","myworkplace");
       handleProvider();                //Performed after auditcollection, since contact is needed for provider

       handleAnalysisProtocolCollection();  //This method is to fix TODO

       handleInputs();    
       //handleAnalysisCollection("TIME:TODO");  ///now called later
    }
    
    
    /**
      *
      * Aim is to write out set up the analysisSoftwareList following this structure:
      *  <AnalysisSoftware id="ID_software" name="xtandem" version="2008.12.1.1" >
      *      <SoftwareName>
      *              <cvParam accession="MS:1001476" name="xtandem" cvRef="PSI-MS" />
      *      </SoftwareName>
      *
     */
    public void handleAnalysisSoftware(){
        analysisSoftwareList = new AnalysisSoftwareList();
        List<AnalysisSoftware> analysisSoftwares = analysisSoftwareList.getAnalysisSoftware();
        analysisSoftware = new AnalysisSoftware();
        
        String softwareVersion = paramToCvParamValue.get("Software version");
        analysisSoftware.setName(softwareName);
        Param tempParam = new Param();
        //tempParam.setParamGroup(makeCvParam("MS:1001475","OMSSA",psiCV));
        tempParam.setParam(makeCvParam(paramToCvParamAcc.get("Software name"),paramToCvParamName.get("Software name"),psiCV));
        analysisSoftware.setSoftwareName(tempParam);
        analysisSoftware.setId(analysisSoftID);
        analysisSoftware.setVersion(softwareVersion);

        analysisSoftwares.add(analysisSoftware);

    }
    
     public void handleCVs(){


        //<cv id="PSI-MS" fullName="PSI-MS" URI="http://psidev.cvs.sourceforge.net/viewvc/*checkout*/psidev/psi/psi-ms/mzML/controlledVocabulary/psi-ms.obo" version="2.25.0"/>
        //<cv id="UNIMOD" fullName="UNIMOD" URI="http://www.unimod.org/obo/unimod.obo" />
        //<cv id="UO" fullName="UNIT-ONTOLOGY" URI="http://obo.cvs.sourceforge.net/*checkout*/obo/obo/ontology/phenotype/unit.obo"></cv>

        cvList = new CvList();
        List<Cv> localCvList = cvList.getCv();
        psiCV = new Cv();
        psiCV.setUri("http://psidev.cvs.sourceforge.net/viewvc/*checkout*/psidev/psi/psi-ms/mzML/controlledVocabulary/psi-ms.obo");
        psiCV.setId(psiCvID);
        psiCV.setVersion("2.25.0");
        psiCV.setFullName("PSI-MS");

        unimodCV = new Cv();
        unimodCV.setUri("http://www.unimod.org/obo/unimod.obo");
        unimodCV.setId(unimodID);
        unimodCV.setFullName("UNIMOD");

        unitCV = new Cv();
        unitCV.setUri("http://obo.cvs.sourceforge.net/*checkout*/obo/obo/ontology/phenotype/unit.obo");
        unitCV.setId(unitCvID);
        unitCV.setFullName("UNIT-ONTOLOGY");

        localCvList.add(psiCV);
        localCvList.add(unimodCV);
        localCvList.add(unitCV);
    }
     
     
     
    /**
     *  Setup Provider element as follows
     *	<Provider id="PROVIDER">
     *      <ContactRole Contact_ref="PERSON_DOC_OWNER">
     *		<role>
     *			<cvParam accession="MS:1001271" name="researcher" cvRef="PSI-MS"/>
     *          </role>
     *		</ContactRole>
     *	</Provider>
     *
     */
    public void handleProvider(){
        provider = new Provider();
        provider.setId("PROVIDER");

        ContactRole contactRole = new ContactRole();
        contactRole.setContact(docOwner);


        Role role = new Role();
        role.setCvParam(makeCvParam("MS:1001271","researcher",psiCV));
        contactRole.setRole(role);

        provider.setContactRole(contactRole);

    }

    /**  TO DO Capture name and email of the user
     *	<AuditCollection>
     *		<Person id="PERSON_DOC_OWNER" firstName="Andy" lastName="Jones" email="someone@someuniversity.com">
     *			<affiliations Organization_ref="ORG_DOC_OWNER"/>
     *		</Person>
     *		<Organization id="ORG_DOC_OWNER" address="Some address" name="Some place" />
     *	</AuditCollection>
     *
     *
     */
    public void handleAuditCollection(String firstName, String lastName, String email, String address, String affiliationName){
        auditCollection = new AuditCollection();
        //List<Contact> contactList = auditCollection.getContactGroup();
        List<AbstractContact> contactList = auditCollection.getPersonOrOrganization();
        docOwner = new Person();
        docOwner.setId("PERSON_DOC_OWNER");
        docOwner.setFirstName(firstName);
        docOwner.setLastName(lastName);

        //docOwner.setEmail(email);

        Organization org = new Organization();
        org.setId("ORG_DOC_OWNER");
        org.setName(affiliationName);

        //org.setAddress(address);


        List<Affiliation> affList = docOwner.getAffiliation();
        Affiliation aff = new Affiliation();
        aff.setOrganization(org);
        affList.add(aff);
        contactList.add(docOwner);
        contactList.add(org);

    }


    /**
     * TODO This part is optional in the file - not yet completed
     *
     *
     *
     */
   public void handleAnalysisSampleCollection(){
        analysisSampleCollection = new AnalysisSampleCollection();

   }

   /**
    *  	<AnalysisCollection>
    *		<SpectrumIdentification id="SI_1" SpectrumIdentificationProtocol_ref="SearchProtocol" SpectrumIdentificationList_ref="siiListID" activityDate="2008-02-27T08:22:12">
    *			<InputSpectra SpectraData_ref="SD_1"/>
    *			<SearchDatabase SearchDatabase_ref="search_database"/>
    *		</SpectrumIdentification>
    *	</AnalysisCollection>
    *
    */
   public void handleAnalysisCollection(String activityDate){
        analysisCollection = new AnalysisCollection();
        List<SpectrumIdentification> specIdentList = analysisCollection.getSpectrumIdentification();
        SpectrumIdentification specIdent = new SpectrumIdentification();
        specIdent.setId(specIdentID);
        specIdent.setSpectrumIdentificationList(siList);
        specIdent.setSpectrumIdentificationProtocol(siProtocol);
        List<SearchDatabaseRef> searchDBRefList = specIdent.getSearchDatabaseRef();
        SearchDatabaseRef searchDBRef = new SearchDatabaseRef();
        searchDBRef.setSearchDatabase(searchDB);
        searchDBRefList.add(searchDBRef);


        List<InputSpectra> inputSpecList = specIdent.getInputSpectra();
        InputSpectra inputSpec = new InputSpectra();
        inputSpec.setSpectraData(spectraData);
        inputSpecList.add(inputSpec);

        specIdentList.add(specIdent);

   }


   /**
    *	<AnalysisProtocolCollection>
            <SpectrumIdentificationProtocol id="SearchProtocol" AnalysisSoftware_ref="ID_software">
                    <SearchType>
                            <cvParam accession="MS:1001083" name="ms-ms search" cvRef="PSI-MS"/>
                    </SearchType>
                    <AdditionalSearchParams>
                            <cvParam accession="MS:1001211" name="parent mass type mono"    cvRef="PSI-MS"/>
                            <cvParam accession="MS:1001256" name="fragment mass type mono"    cvRef="PSI-MS"/>
                    </AdditionalSearchParams>
                    <ModificationParams>
                            <SearchModification fixedMod="true">
                                    <ModParam massDelta="57.021464" residues="C">
                                            <cvParam accession="UNIMOD:4" name="Carbamidomethyl" cvRef="UNIMOD" />
                                    </ModParam>
                            </SearchModification>
                            <SearchModification fixedMod="false">
                                    <ModParam massDelta="15.994919" residues="M">
                                            <cvParam accession="UNIMOD:35" name="Oxidation" cvRef="UNIMOD" />
                                    </ModParam>
                            </SearchModification>
                    </ModificationParams>
                    <Enzymes independent="0">
                            <Enzyme id="ENZ_1" CTermGain="OH" NTermGain="H" missedCleavages="1" semiSpecific="0">
                            <EnzymeName>
                            <cvParam accession="MS:1001251" name="Trypsin" cvRef="PSI-MS" />
                            </EnzymeName>
                            </Enzyme>
                    </Enzymes>
                    <MassTable id="0" msLevel="2">
                    </MassTable>
                    <FragmentTolerance>
                            <cvParam accession="MS:1001412" name="search tolerance plus value" value="0.5" cvRef="PSI-MS" unitAccession="UO:0000221" unitName="dalton" unitCvRef="UO" />
                            <cvParam accession="MS:1001413" name="search tolerance minus value" value="0.5" cvRef="PSI-MS" unitAccession="UO:0000221" unitName="dalton" unitCvRef="UO" />
                    </FragmentTolerance>
                    <ParentTolerance>
                            <cvParam accession="MS:1001412" name="search tolerance plus value" value="0.5" cvRef="PSI-MS" unitAccession="UO:0000221" unitName="dalton" unitCvRef="UO" />
                            <cvParam accession="MS:1001413" name="search tolerance minus value" value="0.5" cvRef="PSI-MS" unitAccession="UO:0000221" unitName="dalton" unitCvRef="UO" />
                    </ParentTolerance>
                    <Threshold>
                            <cvParam accession="MS:1001494" name="no threshold" cvRef="PSI-MS" />
                    </Threshold>
            </SpectrumIdentificationProtocol>
    </AnalysisProtocolCollection>
    *
    *
    */
   public void handleAnalysisProtocolCollection(){

      

        analysisProtocolCollection = new AnalysisProtocolCollection();
        List<SpectrumIdentificationProtocol> sipList = analysisProtocolCollection.getSpectrumIdentificationProtocol();



        siProtocol = new SpectrumIdentificationProtocol();
        siProtocol.setId(siProtocolID);
        siProtocol.setAnalysisSoftware(analysisSoftware);

        //<cvParam accession="MS:1001083" name="ms-ms search" cvRef="PSI-MS"/>
        Param tempParam = new Param();
        tempParam.setParam(makeCvParam("MS:1001083","ms-ms search",psiCV));
        siProtocol.setSearchType(tempParam);


        //List<CvParam> cvParamList = siProtocol.getAdditionalSearchCvParams();
        ParamList paramList = siProtocol.getAdditionalSearchParams();
        if(paramList == null ){
            paramList = new ParamList();
            siProtocol.setAdditionalSearchParams(paramList);
        }
        List<CvParam> cvParamList = paramList.getCvParam();

       

        cvParamList.add(makeCvParam(paramToCvParamName.get("Parent mass type"),paramToCvParamAcc.get("Parent mass type"),psiCV));
        cvParamList.add(makeCvParam(paramToCvParamName.get("Fragment mass type"),paramToCvParamAcc.get("Fragment mass type"),psiCV));
        
        

        ModificationParams modParams = new ModificationParams();
        List<SearchModification> searchModList = modParams.getSearchModification();

        for(String modName : modNameToUnimodName.keySet() ){
            SearchModification searchMod = new SearchModification();
            searchMod.setFixedMod(modNameToIsFixed.get(modName));
            List<CvParam> modCvParamList = searchMod.getCvParam();
            modCvParamList.add(makeCvParam(modNameToUnimodName.get(modName),modNameToUnimodID.get(modName),unimodCV));
            searchMod.setMassDelta(modNameToMassDelta.get(modName));
            List<String> residueList = searchMod.getResidues();
            residueList.add(modNameToResidue.get(modName));
            searchModList.add(searchMod);
            //TODO - need to write code to handle N and C terminal mods
        }
        
    
        siProtocol.setModificationParams(modParams);
   
        Enzymes enzymes = siProtocol.getEnzymes();

        if(enzymes==null){
            enzymes = new Enzymes();
            siProtocol.setEnzymes(enzymes);
        }
        enzymes.setIndependent(false);

        List<Enzyme> enzymeList = enzymes.getEnzyme();

        
        Enzyme enzyme = new Enzyme();
        //[KR]|{P}

        //TODO only a few enzymes implemented
        enzyme.setId("Enz1");
        enzyme.setCTermGain("OH");
        enzyme.setNTermGain("H");
        enzyme.setMissedCleavages(Integer.parseInt(paramToCvParamValue.get("Missed cleavages")));
        enzyme.setSemiSpecific(false);
        ParamList eParamList = enzyme.getEnzymeName();
        if(eParamList == null ){
            eParamList = new ParamList();
            enzyme.setEnzymeName(eParamList);
        }
        List<CvParam> eCvParamList = eParamList.getCvParam();
        eCvParamList.add(makeCvParam(paramToCvParamName.get("Enzyme"),paramToCvParamAcc.get("Enzyme"),psiCV));
        enzymeList.add(enzyme);



        Tolerance fragTol = new Tolerance();
        Tolerance parTol = new Tolerance();

        List<CvParam> fragCvList = fragTol.getCvParam();
        CvParam fragCvPlus = getCvParamWithMassUnits(true);
        CvParam fragCvMinus = getCvParamWithMassUnits(true);
//Fragment search tolerance plus
//Fragment search tolerance minus
//Parent search tolerance plus
//Parent search tolerance minus


        fragCvPlus.setAccession("MS:1001412");
        fragCvPlus.setName("search tolerance plus value");
        fragCvMinus.setAccession("MS:1001413");
        fragCvMinus.setName("search tolerance minus value");
        fragCvPlus.setValue(paramToCvParamValue.get("Fragment search tolerance plus"));    
        fragCvMinus.setValue(paramToCvParamValue.get("Fragment search tolerance minus")); 
        fragCvList.add(fragCvPlus);
        fragCvList.add(fragCvMinus);

        List<CvParam> parCvList = parTol.getCvParam();
        CvParam parCvPlus = getCvParamWithMassUnits(true);
        CvParam parCvMinus = getCvParamWithMassUnits(true);

        parCvPlus.setAccession("MS:1001412");
        parCvPlus.setName("search tolerance plus value");
        parCvMinus.setAccession("MS:1001413");
        parCvMinus.setName("search tolerance minus value");
        parCvPlus.setValue(paramToCvParamValue.get("Parent search tolerance plus")); 
        parCvMinus.setValue(paramToCvParamValue.get("Parent search tolerance plus"));
        parCvList.add(parCvPlus);
        parCvList.add(parCvMinus);

        siProtocol.setFragmentTolerance(fragTol);
        siProtocol.setParentTolerance(parTol);

        // siProtocol.getThresholdCvParams();
        ParamList sip_paramList = siProtocol.getThreshold();
        if(sip_paramList == null ){
            sip_paramList = new ParamList();
            siProtocol.setThreshold(sip_paramList);
        }
        cvParamList =sip_paramList.getCvParam();

        cvParamList.add(makeCvParam(paramToCvParamName.get("PSM threshold"),paramToCvParamAcc.get("PSM threshold"),psiCV));
        //<cvParam accession="MS:1001494" name="no threshold" cvRef="PSI-MS" />
        
        sipList.add(siProtocol);

   }

   public void handleInputs(){

       inputs = new Inputs();
       List<SearchDatabase> searchDBList = inputs.getSearchDatabase();

       searchDB = new SearchDatabase();
       searchDB.setId(searchDBID);
       //searchDB.setNumDatabaseSequences();
       //<cvParam accession="MS:1001401" name="xtandem xml file" cvRef="PSI-MS"/>

       UserParam param = new UserParam();
       //param.setName(settings.MSSearchSettings_db);
       Param tempParam = new Param();
       param.setName(paramToCvParamValue.get("Database name"));
       tempParam.setParam(param);
       searchDB.setDatabaseName(tempParam);
       searchDB.setLocation(paramToCvParamValue.get("Local database path"));

       List<CvParam> searchDBCvParamList = searchDB.getCvParam();    

       if(paramToCvParamName.get("Decoy database composition")!=null){
           searchDBCvParamList.add(makeCvParam(paramToCvParamName.get("Decoy database composition"),paramToCvParamAcc.get("Decoy database composition"),psiCV));
       }
       if(paramToCvParamName.get("Decoy database regex")!=null){
           searchDBCvParamList.add(makeCvParam(paramToCvParamName.get("Decoy database regex"),paramToCvParamAcc.get("Decoy database regex"),psiCV,paramToCvParamValue.get("Decoy database regex")));
       }
              if(paramToCvParamName.get("Decoy database type")!=null){
           searchDBCvParamList.add(makeCvParam(paramToCvParamName.get("Decoy database type"),paramToCvParamAcc.get("Decoy database type"),psiCV));
       }       
               
       FileFormat ff = new FileFormat();
       ff.setCvParam(makeCvParam(paramToCvParamName.get("Database file format"),paramToCvParamAcc.get("Database file format"),psiCV));
       searchDB.setFileFormat(ff);   //TODO - this should not be hard coded <cvParam accession="MS:1001348" name="FASTA format" cvRef="PSI-MS"/>
       searchDBList.add(searchDB);

       List<SourceFile> sourceFileList = inputs.getSourceFile();
       SourceFile sourceFile = new SourceFile();
       sourceFile.setLocation(inputCsvFile);
       sourceFile.setId(sourceFileID);
       ff = new FileFormat();
       ff.setCvParam(makeCvParam(paramToCvParamName.get("Input file format"),paramToCvParamAcc.get("Input file format"),psiCV));

       sourceFile.setFileFormat(ff);
       sourceFileList.add(sourceFile);

       List<SpectraData> spectraDataList = inputs.getSpectraData();
       spectraData = new SpectraData();
       SpectrumIDFormat sif = new SpectrumIDFormat();
       sif.setCvParam(makeCvParam("MS:1000774","multiple peak list nativeID format",psiCV));
       spectraData.setSpectrumIDFormat(sif);

       ff = new FileFormat();
       ff.setCvParam(makeCvParam(paramToCvParamName.get("Spectra data file format"),paramToCvParamAcc.get("Spectra data file format"),psiCV));
       spectraData.setFileFormat(ff);        

       spectraData.setId(spectraDataID);
       spectraData.setLocation(paramToCvParamValue.get("Searched spectrum"));
       spectraDataList.add(spectraData);


   }

     
    
    private void buildPSMs(){
        
        try{
            CSVReader reader = new CSVReader(new FileReader(inputCsvFile));
            System.out.println("Processing..." + inputCsvFile);
            String [] nextLine;
            
            //Setup all the objects we will need to populate
            //MzIdentML mzid = new MzIdentML();
            sequenceCollection = new SequenceCollection();
            //mzid.setSequenceCollection(sequenceCollection);
            List<Peptide> peptideList = sequenceCollection.getPeptide();
            
            siList = new SpectrumIdentificationList();
            siList.setId(siiListID);
            //DataCollection dataCollection = new DataCollection();
            List<DBSequence> dbSequenceList = sequenceCollection.getDBSequence();
            List<PeptideEvidence> peptideEvidenceList = sequenceCollection.getPeptideEvidence();
            AnalysisData analysisData = new AnalysisData();
            //List<SpectrumIdentificationList> siListList = analysisData.getSpectrumIdentificationList();
            //siListList.add(siList);
            List<SpectrumIdentificationResult> sirList = siList.getSpectrumIdentificationResult();
            //dataCollection.setAnalysisData(analysisData);
            //mzid.setDataCollection(dataCollection);
            
            HashMap<String,Integer> headerToColumnMap = new HashMap();
            HashMap<Integer,String> columnToHeaderMap = new HashMap();
            int lineCounter = 0;
            
            int sirCounter=1;
            
            
            while ((nextLine = reader.readNext()) != null) {
                // nextLine[] is an array of values from the line
                
                if(lineCounter==0){
                    
                    for(int i=0; i< nextLine.length; i++){
                        
                        headerToColumnMap.put(nextLine[i].trim(), i);
                        columnToHeaderMap.put(i,nextLine[i]);
                        //System.out.println(nextLine[i] + " " + i);
                    }
                }
                else{

                    String spectrumID = "index="+nextLine[headerToColumnMap.get("Spectrum number")];
                    SpectrumIdentificationResult sir;
                    
                    if(spectrumIDToSIRMap.containsKey(spectrumID)){
                        sir = spectrumIDToSIRMap.get(spectrumID);
                    }
                    else{                        
                        sir = new SpectrumIdentificationResult();
                        sirList.add(sir);
                        sir.setId("SIR_" + sirCounter);
                        sir.setSpectrumID(spectrumID);
                        sirCounter++;
                        sir.setSpectraData(spectraData);
                        spectrumIDToSIRMap.put(spectrumID,sir);
                        
                        //TODO - need to insert correct spectrum ranks
                    }
                    
                    String defline = nextLine[headerToColumnMap.get("Defline")];
                    
                    if(defline == null){
                        System.out.println("Error - Unable to extract protein accessions from the Defline - quitting");
                        System.exit(0);
                    }
                    String protAcc = null;
                    
                    if(defline.indexOf(" ") != -1){ //TODO -set this is a regex to grab the correct accession. Omssa often fails to grab the accession but we can usually get it from the defline
                        protAcc = defline.substring(0,defline.indexOf(" "));
                    }
                    else{
                        protAcc = defline;      //Try grabbing the whole defline
                    }
                    
                    if(protAcc == null){
                        System.out.println("Error - Unable to extract protein accessions from the Defline - quitting");
                        System.exit(0);
                    }
                    
                    
                    DBSequence dbSequence = null;
                    if(accessionToDBSequenceHashMap.containsKey(protAcc)){                        
                        dbSequence = accessionToDBSequenceHashMap.get(protAcc);
                    }
                    else{                        
                         dbSequence = new DBSequence();
                         dbSequence.setId("dbseq_" + protAcc);
                         dbSequence.setAccession(protAcc);
                         dbSequenceList.add(dbSequence);
                         dbSequence.getCvParam().add(makeCvParam("MS:1001088","protein description",psiCV,defline));
                         dbSequence.setSearchDatabase(searchDB);
                         accessionToDBSequenceHashMap.put(protAcc,dbSequence);
                    }

                    
                    String pepSeq = nextLine[headerToColumnMap.get("Peptide")].toUpperCase();
                    String modString = nextLine[headerToColumnMap.get("Mods")];
                    
                    Peptide pep;
                    String pepID = pepSeq + "_" + modString;
                    
                    if(peptideIDToPeptideMap.containsKey(pepID)){
                        pep = peptideIDToPeptideMap.get(pepID);
                    }
                    else{
                        pep = new Peptide();
                        peptideList.add(pep);
                        pep.setPeptideSequence(pepSeq);
                        pep.setId(pepID);
                        peptideIDToPeptideMap.put(pepID,pep);
                        List<Modification> modList = pep.getModification();
                        //System.out.println("New pep:" + pepSeq + modString);
                        
                        String[] mods = modString.split(",");
                        for(String mod:mods){
                            Modification mzidMod = new Modification();
                            mod = mod.trim();
                            
                            String[] temp = mod.split(":");
                            String oneModString = temp[0].trim();
                            if(!oneModString.equals("")){
                                if(temp.length==2){

                                    //System.out.println("Mod2 " + oneModString);

                                    int location = Integer.parseInt(temp[1]);

                                    Boolean foundOkay=true;
                                    if(modNameToMassDelta.get(oneModString)!=null){
                                        mzidMod.setMonoisotopicMassDelta((double)modNameToMassDelta.get(oneModString));
                                    }
                                    else{
                                        System.out.println("Unable to insert correct mass delta:" + oneModString);
                                        foundOkay=false;
                                    }

                                    mzidMod.setLocation(location);
                                    String residue = modNameToResidue.get(oneModString);
                                    if(residue!= null){
                                        
                                        if(residue.equals("N-term")){
                                            residue = ".";
                                            mzidMod.getCvParam().add(makeCvParam("MS:1001189","modification specificity N-term",psiCV));
                                        }
                                        else if(residue.equals("C-term")){
                                            residue = ".";
                                            mzidMod.getCvParam().add(makeCvParam("MS:1001190","modification specificity C-term",psiCV));
                                        }
                                        mzidMod.getResidues().add(modNameToResidue.get(oneModString));
                                    }
                                    else{
                                        System.out.println("Unable to insert correct residue for:" + oneModString);
                                        foundOkay=false;
                                    }

                                    if(foundOkay){
                                        CvParam modParam = new CvParam();
                                        modParam.setAccession(modNameToUnimodID.get(oneModString));
                                        modParam.setCv(unimodCV);
                                        modParam.setName(modNameToUnimodName.get(oneModString));

                                        mzidMod.getCvParam().add(modParam);
                                        modList.add(mzidMod);
                                    }
                                    else{                                   
                                        mzidMod.getCvParam().add(makeCvParam("MS:1001460","unknown modification",psiCV,mod));
                                    }
                                }
                                else{
                                    System.out.println("Incorrectly formatted mod:" + mod);
                                }
                            }
                            
                        }
                        

                    }
                    
                    int start = Integer.parseInt(nextLine[headerToColumnMap.get("Start")]);
                    int end = Integer.parseInt(nextLine[headerToColumnMap.get("Stop")]);
                    String pepEvidID = pepSeq + "_" + protAcc + "_" + start + "_" + end;
                    
                    PeptideEvidence peptideEvidence = null;
                    if(idToPeptideEvidenceMap.containsKey(pepEvidID)){
                        peptideEvidence = idToPeptideEvidenceMap.get(pepEvidID);

                    }
                    else{
                        peptideEvidence = new PeptideEvidence();
                        peptideEvidence.setId(pepEvidID);
                        peptideEvidence.setDBSequence(dbSequence);
                        peptideEvidence.setPeptide(pep);

                        peptideEvidence.setStart(start);
                        peptideEvidence.setEnd(end);

                        peptideEvidence.setIsDecoy(Boolean.FALSE);     
                        if(decoyRegex!=null){
                            if(protAcc.contains(decoyRegex)){
                                peptideEvidence.setIsDecoy(Boolean.TRUE);
                            }
                        }
                        peptideEvidenceList.add(peptideEvidence);
                        
                        idToPeptideEvidenceMap.put(pepEvidID,peptideEvidence);
                        
                    }
                    

                    
                    List<SpectrumIdentificationItem> siiList = sir.getSpectrumIdentificationItem();
                    
                    /*
                     * Cases:
                     * If PepID is the same, then this is another PeptideEvidence, use same sii
                     * If PepID is different
                     */
                    
                    SpectrumIdentificationItem sii = null;
                    
                    for(SpectrumIdentificationItem currentSii : siiList){
                        String currentPepID = currentSii.getPeptideRef();
                        if(currentPepID.equals(pepID)){
                            sii = currentSii;
                            //TODO - how to exit foreach loop in Java?
                        }
                    }
                    
                    if(sii == null){
                        sii = new SpectrumIdentificationItem();
                        
                        sii.setChargeState(Integer.parseInt(nextLine[headerToColumnMap.get("Charge")]));
                        sii.setExperimentalMassToCharge(Double.parseDouble(nextLine[headerToColumnMap.get("Mass")]));
                        sii.setCalculatedMassToCharge(Double.parseDouble(nextLine[headerToColumnMap.get("Theo Mass")]));
                        
                        List<CvParam> cvParamList = sii.getCvParam();
                        cvParamList.add(makeCvParam(cvAccForEvalue,cvNameForEvalue,psiCV,nextLine[headerToColumnMap.get(dataTypeMap.get("evalue"))]));
                        cvParamList.add(makeCvParam(cvAccForPvalue,cvNameForPvalue,psiCV,nextLine[headerToColumnMap.get(dataTypeMap.get("pvalue"))]));
                        //cvParamList.add(makeCvParam("MS:1001328","OMSSA:evalue",psiCV,nextLine[headerToColumnMap.get("E-value")])); //TODO - add dynamic mapping to other CV params
                        //cvParamList.add(makeCvParam("MS:1001874","FDRScore",psiCV,nextLine[headerToColumnMap.get("E-value")])); //TODO - add dynamic mapping to other CV params
                        //cvParamList.add(makeCvParam("MS:1999999","Simple FDRScore",psiCV,nextLine[headerToColumnMap.get("P-value")]));
                        //cvParamList.add(makeCvParam("MS:1001329","OMSSA:pvalue",psiCV,nextLine[headerToColumnMap.get("P-value")]));
                        sii.setPeptide(pep);
                        
                        
                        Boolean orderLowToHigh = true;
                        
                        addSIIToListAndSetRank(siiList,sii,cvScoreToOrderBy,orderLowToHigh,sir.getId());
                        
                        //TODO Need to set headers from config file
                                            
                        
                    }
                    PeptideEvidenceRef pepEvidRef = new PeptideEvidenceRef();
                    pepEvidRef.setPeptideEvidence(peptideEvidence);
                    sii.getPeptideEvidenceRef().add(pepEvidRef);
                }
                //System.out.println(nextLine[0] + nextLine[1] + "etc...");
                lineCounter++;
            }
            //marshaller.marshal(mzid, new FileOutputStream(outFile));

        }
        catch(Exception e){
            e.printStackTrace();
        }
    }
    
    /*
     * Accepts the SIIs associated with any SIR and inserts the correct rank values
     * 
     */
    private void addSIIToListAndSetRank(List<SpectrumIdentificationItem> siiList, SpectrumIdentificationItem sii,  String scoreCvParamNameToOrderBy, Boolean orderLowToHigh, String sirID){
        
        Double scoreOfNewSII = getScoreFromSII(sii,scoreCvParamNameToOrderBy);
       
        Boolean inserted = false;
        Boolean insertedAsEqual = false;        //Special case for if the scores are equal, don't need to change ranks
        
        int foundRank = 0;
                      
        
        for(SpectrumIdentificationItem storedSii : siiList){            

            Double storedScore = getScoreFromSII(storedSii,scoreCvParamNameToOrderBy);
            foundRank = storedSii.getRank();
            
            if(!inserted){
                
                //If ordering low to high by score, if score of new SII is less than the current one in the ranked list, give it the current rank
                if(Double.compare(storedScore,scoreOfNewSII) == 0){
                    sii.setRank(foundRank);
                    inserted = true;
                    insertedAsEqual=true;

                }                                
                else if((Double.compare(storedScore,scoreOfNewSII) > 0 && orderLowToHigh) || (Double.compare(storedScore,scoreOfNewSII) < 0 && !orderLowToHigh)){
                    sii.setRank(foundRank);
                    inserted = true;
                }

            }
            
            //The new SII has been inserted earlier therefore increase the rank of all other SIIs
            if(inserted && !insertedAsEqual){
                storedSii.setRank(storedSii.getRank()+1);                
                storedSii.setId(sirID + "_SII_" + (storedSii.getRank()+1));       //And reset ID to match the new rank
            }           
        }
        
        if(sii.getRank()==0){
            sii.setRank(foundRank+1);                 
        }
        
        if(!insertedAsEqual){
            sii.setId(sirID + "_SII_" + sii.getRank());
        }
        else{
            sii.setId(sirID + "SII_" + sii.getRank() + "_rand" + Math.random());                
        }  
        
        siiList.add(sii);
        
    }
    
    public Double getScoreFromSII(SpectrumIdentificationItem sii, String cvParamAccForScore){
        
        Double score = 0.0;
        
        for(CvParam cvParam : sii.getCvParam()){
            if(cvParam.getAccession().equals(cvParamAccForScore)){
                score = Double.parseDouble(cvParam.getValue());
            }            
        }                
        
        return score;
        
    }
    
     /**
     * Helper method to create and return a CvParam from accession, name and CV
     *
     * @return CvParam
     */
    public CvParam makeCvParam(String accession, String name, Cv cv){
        CvParam cvParam = new CvParam();
        cvParam.setAccession(accession);
        cvParam.setName(name);
        cvParam.setCv(cv);
        return cvParam;
    }

        /**
     * Helper method to create and return a CvParam from accession, name and CV
     *
     * @return CvParam
     */
    public CvParam makeCvParam(String accession, String name, Cv cv, String value){
        CvParam cvParam = new CvParam();
        cvParam.setAccession(accession);
        cvParam.setName(name);
        cvParam.setCv(cv);
        cvParam.setValue(value);
        return cvParam;
    }


        /**
     * Helper method to create and return a CvParam from accession, name, CV, unitAccession and unitName (unitCV is automatically provided)
     *
     * @return CvParam
     */
    public CvParam makeCvParam(String accession, String name, Cv cv, String unitAccession, String unitName){
        CvParam cvParam = new CvParam();
        cvParam.setAccession(accession);
        cvParam.setName(name);
        cvParam.setCv(cv);
        cvParam.setUnitAccession(unitAccession);
        cvParam.setUnitCv(unitCV);
        cvParam.setUnitName(unitName);
        return cvParam;
    }

            /**
     * Helper method to create and return a CvParam from accession, name, CV, unitAccession, unitName and unitCV 
     *
     * @return CvParam
     */
    public CvParam makeCvParam(String accession, String name, Cv cv, String unitAccession, String unitName, Cv alternateUnitCV){
        CvParam cvParam = new CvParam();
        cvParam.setAccession(accession);
        cvParam.setName(name);
        cvParam.setCv(cv);
        cvParam.setUnitAccession(unitAccession);
        cvParam.setUnitCv(alternateUnitCV);
        cvParam.setUnitName(unitName);
        return cvParam;
    }

    
     public void writeMzidFile(){

        try {
            FileWriter writer = new FileWriter(outFile);
            

            // mzIdentML
            //     cvList
            //     AnalysisSoftwareList
            //     Provider
            //     AuditCollection
            //     AnalysisSampleCollection
            //     SequenceCollection
            //     AnalysisCollection
            //     AnalysisProtocolCollection
            //     DataCollection
            //         Inputs
            //         AnalysisData
            //             SpectrumIdentificationList
            //             ProteinDetectionList
            //         /AnalysisData
            //     /DataCollection
            //     BibliographicReference
            // /mzIdentML


            // Note: writing of '\n' characters is optional and only for readability of the produced XML document
            // Also note: since the XML is produced in individual parts, the overall formatting of the document
            //            is not as nice as it would be when marshalling the whole structure at once.

            // XML header
            writer.write(marshaller.createXmlHeader() + "\n");


            // mzIdentML start tag

            writer.write(marshaller.createMzIdentMLStartTag("12345") + "\n");


            
            marshaller.marshall(cvList, writer);
            writer.write("\n");

            marshaller.marshall(analysisSoftwareList, writer);
            writer.write("\n");


            marshaller.marshall(provider, writer);
            writer.write("\n");


            marshaller.marshall(auditCollection, writer);
            writer.write("\n");


            //m.marshall(analysisSampleCollection, writer);     //TODO - complete this part
            //writer.write("\n");


            marshaller.marshall(sequenceCollection, writer);
            writer.write("\n");



            marshaller.marshall(analysisCollection, writer);
            writer.write("\n");


            marshaller.marshall(analysisProtocolCollection, writer);
            writer.write("\n");


            writer.write(marshaller.createDataCollectionStartTag() + "\n");
            marshaller.marshall(inputs, writer);
            writer.write("\n");


            //Inputs inputs = unmarshaller.unmarshal(MzIdentMLElement.Inputs.getXpath());
            //m.marshall(inputs, writer);
            //writer.write("\n");

            writer.write(marshaller.createAnalysisDataStartTag() + "\n");



           // writer.write(m.createSpectrumIdentificationListStartTag("SIL_1", null, 71412L) + "\n");

            //FragmentationTable table = unmarshaller.unmarshal(MzIdentMLElement.FragmentationTable.getXpath());
            //m.marshall(table, writer);
            //writer.write("\n");


            //Iterator<SpectrumIdentificationResult> specResIter = unmarshaller.unmarshalCollectionFromXpath(MzIdentMLElement.SpectrumIdentificationResult);

            /*
            Iterator<SpectrumIdentificationResult> specResIter = specIdentResults.iterator();
            while (specResIter.hasNext()) {
                SpectrumIdentificationResult specIdentRes = specResIter.next();
                m.marshall(specIdentRes, writer);
                writer.write("\n");
            }
            */

            marshaller.marshall(siList, writer);
            writer.write("\n");


           // writer.write(m.createSpectrumIdentificationListClosingTag() + "\n");

           writer.write(marshaller.createProteinDetectionListStartTag("PDL_1", null) + "\n");

            /*
            Iterator<ProteinAmbiguityGroup> protAmbGroupIter = unmarshaller.unmarshalCollectionFromXpath(MzIdentMLElement.ProteinAmbiguityGroup);
            while (protAmbGroupIter.hasNext()) {
                ProteinAmbiguityGroup protAmbGroup = protAmbGroupIter.next();
                m.marshall(protAmbGroup, writer);
                writer.write("\n");
            }

             */

            writer.write(marshaller.createProteinDetectionListClosingTag() + "\n");
            writer.write(marshaller.createAnalysisDataClosingTag() + "\n");
            writer.write(marshaller.createDataCollectionClosingTag() + "\n");

            //BibliographicReference ref = unmarshaller.unmarshal(MzIdentMLElement.BibliographicReference.getXpath());
           // m.marshall(ref, writer);
           // writer.write("\n");

            writer.write(marshaller.createMzIdentMLClosingTag());

            writer.close();
            
            System.out.println("Output written to " + outFile);

        } catch(IOException e){
            e.printStackTrace();
        }
     }
     
    /**
     * Helper method to setup a CvParam with CVRef, with either Daltons or ppm as units
     *
     */

    public CvParam getCvParamWithMassUnits(Boolean isDaltonUnit){
        CvParam cvParam = new CvParam();

         //<cvParam accession="MS:1001413" name="search tolerance minus value" value="0.5" cvRef="PSI-MS" unitAccession="UO:0000221" unitName="dalton" unitCvRef="UO" />
        cvParam.setCv(psiCV);
        cvParam.setUnitCv(unitCV);

        if(isDaltonUnit){
            cvParam.setUnitAccession("UO:0000221");
            cvParam.setUnitName("dalton");
        }
        else{
            cvParam.setUnitAccession("UO:0000169");
            cvParam.setUnitName("parts per million");
        }
        return cvParam;
    }

    
}
