package uk.ac.liv.mzidparsers;

/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */


import uk.ac.liv.unimod.ModT;
import java.io.File;
import de.proteinms.xtandemparser.xtandem.*;
import java.io.*;
import java.util.*;
import de.proteinms.xtandemparser.interfaces.*;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.*;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLMarshaller;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLUnmarshaller;

import java.io.FileWriter;
import java.io.IOException;
import java.net.URL;
import java.util.Iterator;

import java.math.BigInteger;
import uk.ac.liv.unimod.*;


/**
 *
 * @author jonesar
 */
public class Tandem2mzid {


    //These are the main structures to be output by the main writing method
    SequenceCollection sequenceCollection;
    SpectrumIdentificationList siList;
    CvList cvList;
    AnalysisSoftwareList analysisSoftwareList;
    Provider provider;
    AuditCollection auditCollection;
    AnalysisSampleCollection analysisSampleCollection;
    AnalysisCollection analysisCollection;
    AnalysisProtocolCollection analysisProtocolCollection;
    Inputs inputs;


    //Some IDs to be used throughout;

    static String inputTandemFile =  "resources/55merge_tandem.xml";

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
    Cv unimodCV;
    Cv psiCV;
    Cv unitCV;
    SpectrumIdentificationProtocol siProtocol;
    SearchDatabase searchDB;
    SpectraData spectraData;
    Person docOwner;
    //PeptideEvidenceList pepEvidList;
    AnalysisSoftware analysisSoftware;

    HashMap<String, DBSequence> foundProts;
    HashMap<String, String> pepProtMap;
    //HashMap<String, uk.ac.ebi.jmzidml.model.mzidml.Peptide> peptideLookup;   //lookup to get a peptide by peptideseq_varmods_fixedmods_start_pos_end_pos
    HashMap<String, PeptideEvidence>    pepEvidLookup;                              //lookup to get a peptide evidence object by peptideID_proteinacc_start_end
    HashMap<String, uk.ac.ebi.jmzidml.model.mzidml.Peptide> uniquePeps;
    //int pepCounter = 0;
    int pepEvidCounter = 0;

    //List<SpectrumIdentificationResult> specIdentResults;


    int sirCounter = 1; //Counter used to create unique ID for SpectrumIdentificationResult
    ReadUnimod unimodDoc;
    Double unimodMassError = 0.001;

    static Boolean outputFragmentation = true;


    public Tandem2mzid(String fileName) throws Exception{

        try{
            //File tandemFile = new File (fileName);
            //XTandemParser parser = new XTandemParser(tandemFile);
            XTandemFile xfile = new XTandemFile (fileName);

            /*
            XTandemFile paramsFile = null;

            if(paramsFileName != null && !paramsFileName.equals("")){
                  paramsFile = new XTandemFile(paramsFileName);
            }
*/
            unimodDoc = new ReadUnimod();
            parseFile(xfile);
            writeMzidFile("");

        }
        catch(Exception e){

        	 throw e;
        }
    }

    // by F. Ghali
    public Tandem2mzid(String inputfile, String outputfile) throws Exception{

        try{
            //File tandemFile = new File (fileName);
            //XTandemParser parser = new XTandemParser(tandemFile);
            XTandemFile xfile = new XTandemFile (inputfile);

            /*
            XTandemFile paramsFile = null;

            if(paramsFileName != null && !paramsFileName.equals("")){
                  paramsFile = new XTandemFile(paramsFileName);
            }
*/
            unimodDoc = new ReadUnimod();
            parseFile(xfile);
            writeMzidFile(outputfile);


        }
        catch(Exception e){

            throw e;
        }
    }

    public static void main(String[] args) {

        try {
			new Tandem2mzid(inputTandemFile);
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}

    }


    public void parseFile(XTandemFile iXTandemFile){



        // Iterate over all the spectra
        Iterator<Spectrum> iter = iXTandemFile.getSpectraIterator();
        //ArrayList<Spectrum> specList = iXTandemFile.getSpectraList();

        // Prepare everything for the peptides.
        PeptideMap pepMap = iXTandemFile.getPeptideMap();

        // Prepare everything for the proteins.
        ProteinMap protMap = iXTandemFile.getProteinMap();

        // Setup the mzid objects
       handleCVs();

       PerformParams tandemParams = iXTandemFile.getPerformParameters();
       String version = tandemParams.getProcVersion();
       String dbLocation = tandemParams.getSequenceSource_1();
       String dbName = tandemParams.getSequenceSourceDescription_1();
       long totalProts = (long)tandemParams.getTotalProteinsUsed();


       InputParams inputParams = iXTandemFile.getInputParameters();



       //TODO - This only works if the user specified to output the input params - need to document this clearly
       double massError = inputParams.getSpectrumParentMonoIsoMassErrorMinus();

       foundProts = new HashMap<String, DBSequence>();
       pepProtMap = new HashMap<String, String>();
       //peptideLookup= new HashMap<String, uk.ac.ebi.jmzidml.model.mzidml.Peptide>();   //lookup to get a peptide by peptideseq_varmods_fixedmods_start_stop
       pepEvidLookup = new HashMap<String, PeptideEvidence>();
       uniquePeps = new HashMap<String, uk.ac.ebi.jmzidml.model.mzidml.Peptide>();      //lookup to get a peptide by peptideseq_varmods_fixedmods (i.e. uniqueness check)
       sequenceCollection = new SequenceCollection();

       //List<PeptideEvidenceList> pepEvidListList = sequenceCollection.getPeptideEvidenceList();
       //pepEvidList = new PeptideEvidenceList();    //TODO - only creating one peptide evidence list, much more complicated if multiple enzymes
      // pepEvidList.setId(pepEvidenceListID);
       
       
       List<PeptideEvidence> peptideEvidenceList = sequenceCollection.getPeptideEvidence();

       siList = new SpectrumIdentificationList();
       siList.setId(siiListID);

       handleAnalysisSoftware(version);
       handleAuditCollection("firstname","secondName","email@place.com","address","myworkplace");
       handleProvider();                //Performed after auditcollection, since contact is needed for provider
       
       Boolean fragmentIsMono = handleAnalysisProtocolCollection(inputParams);
       handleInputs(dbName, dbLocation,totalProts,inputParams.getSpectrumPath());
       handleAnalysisCollection(tandemParams.getProcStartTime());


        // Create fragmentation table
        /*
          <Measure id="m_mz">
            <cvParam cvRef="PSI-MS" accession="MS:1001225" name="product ion m/z"/>
          </Measure>
          <Measure id="m_intensity">
            <cvParam cvRef="PSI-MS" accession="MS:1001226" name="product ion intensity"/>
          </Measure>
          <Measure id="m_error">
            <cvParam cvRef="PSI-MS" accession="MS:1001227" name="product ion m/z error" unitAccession="MS:1000040" unitName="m/z" unitCvRef="PSI-MS"/>
          </Measure>
         */

        FragmentationTable fragTable = new FragmentationTable();
        List<Measure> measureList = fragTable.getMeasure();
        Measure mzMeasure = new Measure();
        mzMeasure.setId(measureMzID);
        List<CvParam> cvParamList = mzMeasure.getCvParam();
        cvParamList.add(makeCvParam("MS:1001225","product ion m/z",psiCV,"MS:1000040","m/z", psiCV));
        Measure intMeasure = new Measure();
        intMeasure.setId(measureIntID);
        cvParamList = intMeasure.getCvParam();
        cvParamList.add(makeCvParam("MS:1001226","product ion intensity",psiCV,"MS:1000131","number of counts",psiCV));
        Measure errorMeasure = new Measure();
        errorMeasure.setId(measureErrorID);
        cvParamList = errorMeasure.getCvParam();
        cvParamList.add(makeCvParam("MS:1001227","product ion m/z error",psiCV,"MS:1000040","m/z", psiCV));
        measureList.add(mzMeasure);
        measureList.add(intMeasure);
        measureList.add(errorMeasure);

        

        siList.setFragmentationTable(fragTable);

        List<SpectrumIdentificationResult> specIdentResults = siList.getSpectrumIdentificationResult();
        List<DBSequence> dbSeqList = sequenceCollection.getDBSequence();
        List<uk.ac.ebi.jmzidml.model.mzidml.Peptide> peptideList = sequenceCollection.getPeptide();
        spectraData = new SpectraData();
        spectraData.setId(spectraDataID);


         while (iter.hasNext()) {

            // Get the next spectrum.
            Spectrum spectrum = iter.next();
            int spectrumNumber = spectrum.getSpectrumNumber();


             /*************************************************
             *  *** Setup SpectrumIdentificationResult  ****
             * *********************************************
             */
            SpectrumIdentificationResult specIdentRes = new SpectrumIdentificationResult();
            List<SpectrumIdentificationItem> siiList = specIdentRes.getSpectrumIdentificationItem();
            

            // Get the peptide hits.
            ArrayList<de.proteinms.xtandemparser.xtandem.Peptide> pepList = pepMap.getAllPeptides(spectrumNumber);


            //int pepEvidCounter = 1;
            int siiCounter = 1; //Counter used to create unique ID for SpectrumIdentificationItem

            Double evalue = null;
            Double hyperscore = null;

            SpectrumIdentificationItem sii = null;
            uk.ac.ebi.jmzidml.model.mzidml.Peptide mzidPep = null;
            boolean newSII = true;

            String varMods = "";
            String fixMods = "";
            List<IonType> ionTypeList=null;

            HashMap<String,SpectrumIdentificationItem> siiPeptideSeq = new HashMap<String,SpectrumIdentificationItem>();

            for (de.proteinms.xtandemparser.xtandem.Peptide peptide : pepList) {    //This list can contain both different pep2Protein maps and 

                for(Domain domain : peptide.getDomains()){

                     
                    String newPepSeq = domain.getDomainSequence();
                    if(!siiPeptideSeq.containsKey(newPepSeq)){  //new SII
                        sii = new SpectrumIdentificationItem();
                        siiPeptideSeq.put(newPepSeq, sii);

                        

                        if(outputFragmentation){
                            //ionTypeList = sii.getFragmentation();
                           Fragmentation frag = new Fragmentation();
                           sii.setFragmentation(frag);
                           ionTypeList = frag.getIonType();
                        }

                        //System.out.println("New SII");
                        newSII = true;
                    }
                    else{
                        newSII = false;
                        sii = siiPeptideSeq.get(newPepSeq);
                        //System.out.println("Retrieved SII");

                    }

                    List<PeptideEvidenceRef> peptideEvidenceRefList = sii.getPeptideEvidenceRef();

                    Protein protein = protMap.getProteinWithPeptideID(domain.getDomainID());

                    String protAccession="";
                    String protSeq="";
                    int protLength;

                    if (protein != null){
                        protAccession = protein.getLabel();
                        //System.out.println("prot: " + protAccession);
                        protSeq = peptide.getSequence();        //getSequence returns the protein sequence
                        protSeq= protSeq.replaceAll("\\s+","");
                    }

                    //Use Hash map to test if Protein sequence has been added to DBSeq before
                    DBSequence dbSeq = null;
                    if(!foundProts.containsKey(protAccession)){
                        dbSeq = new DBSequence();
                        foundProts.put(protAccession, dbSeq);
                        dbSeq.setAccession(protAccession);
                        dbSeq.setSeq(protSeq);
                        dbSeq.setLength(protSeq.length());
                        dbSeq.setId("dbseq_" + protAccession);
                        dbSeq.setSearchDatabase(searchDB);
                        //dbSeq.setSearchDatabase(searchDB);
                        dbSeqList.add(dbSeq);
                    }
                    else{
                        dbSeq = foundProts.get(protAccession);
                    }

                    /*
                     ****************************************************
                     ****** Create new SpectrumIdentificationItem *******
                     ****************************************************
                     */


                    if(newSII){
                        // Do the modifications
                        ArrayList<de.proteinms.xtandemparser.interfaces.Modification> fixModList = iXTandemFile.getModificationMap().getFixedModifications(domain.getDomainID());
                        ArrayList<de.proteinms.xtandemparser.interfaces.Modification> varModList = iXTandemFile.getModificationMap().getVariableModifications(domain.getDomainID());

                        fixMods = "";

                        ArrayList<uk.ac.ebi.jmzidml.model.mzidml.Modification> mzidFixMods = new ArrayList();
                        ArrayList<uk.ac.ebi.jmzidml.model.mzidml.Modification> mzidVarMods = new ArrayList();

                        Boolean foundFixMods = false;
                        Boolean foundVarMods = false;
                        
                        for (de.proteinms.xtandemparser.interfaces.Modification fixMod : fixModList) {

                            uk.ac.ebi.jmzidml.model.mzidml.Modification mzidFixMod = new uk.ac.ebi.jmzidml.model.mzidml.Modification();
                            foundFixMods = true;
                            Double mass = fixMod.getMass();
                            String name = fixMod.getName();
                            int loc = Integer.parseInt(fixMod.getLocation());
                            fixMods +=  name + "$" + loc + ";";

                            
                            List<CvParam> paramList = mzidFixMod.getCvParam();
                            paramList.add(getModCV(mass));

                            if(fragmentIsMono){
                                mzidFixMod.setMonoisotopicMassDelta(mass);
                            }
                            else{
                                mzidFixMod.setAvgMassDelta(mass);
                            }

                            int pepLoc = loc - domain.getDomainStart(); //location in Tandem is given as location within the whole protein
                            mzidFixMod.setLocation(pepLoc + 1);        //mzid starts counting from 1, except for NTerm mods which are 0 TODO - Almost impossible to work out what are Nterm mods in Tandem, these have the same location as mods on the first aa in the peptide
                            List<String> residueList = mzidFixMod.getResidues();
                            residueList.add(""+ domain.getDomainSequence().charAt(pepLoc));
                            mzidFixMods.add(mzidFixMod);
                            
                        }

                        varMods = "";

                        for (de.proteinms.xtandemparser.interfaces.Modification varMod : varModList) {

                            uk.ac.ebi.jmzidml.model.mzidml.Modification mzidVarMod = new uk.ac.ebi.jmzidml.model.mzidml.Modification();
                            foundVarMods = true;
                            Double mass = varMod.getMass();
                            String name = varMod.getName();
                            int loc = Integer.parseInt(varMod.getLocation());
                            varMods +=  name + "$" + loc + ";";

                            //uk.ac.ebi.jmzidml.model.mzidml.Modification mod = new uk.ac.ebi.jmzidml.model.mzidml.Modification();
                            List<CvParam> paramList = mzidVarMod.getCvParam();

                            CvParam modParam = new CvParam();

                            //Boolean isMono = true;  //TODO - need to check if we are looking for mono or average mass mods

                            if(fragmentIsMono){
                                mzidVarMod.setMonoisotopicMassDelta(mass);
                            }
                            else{
                                mzidVarMod.setAvgMassDelta(mass);
                            }
                            int pepLoc = loc - domain.getDomainStart(); //location in Tandem is given as location within the whole protein
                            mzidVarMod.setLocation(pepLoc + 1);        //mzid starts counting from 1, except for NTerm mods which are 0 TODO - Almost impossible to work out what are Nterm mods in Tandem, these have the same location as mods on the first aa in the peptide
                            List<String> residueList = mzidVarMod.getResidues();
                            residueList.add(""+ domain.getDomainSequence().charAt(pepLoc));


                            ModT unimod = unimodDoc.getModByMass(mass, unimodMassError, fragmentIsMono, domain.getDomainSequence().charAt(pepLoc));

                            if(unimod != null){
                                modParam.setAccession("UNIMOD:" + unimod.getRecordId());
                                modParam.setCv(unimodCV);
                                modParam.setName(unimod.getTitle());
                            }
                            else{
                                System.out.println("Error: modification with mass not recognized");
                                modParam.setName("" + mass);
                                modParam.setCv(unimodCV);
                                modParam.setAccession("UNIMOD:Error");
                            }

                            //paramList.add(getModCV(mass));
                            paramList.add(modParam);
                            //allMods.add(mod);
                            mzidVarMods.add(mzidVarMod);
                        }
                        
                        //Moved the creation of the unique peptide here, since we can't do this until we've seen the mods
                        String uniquePep = domain.getDomainSequence() + "_" + varMods + "_" +fixMods+"_";
                        if(!uniquePeps.containsKey(uniquePep)){
                            mzidPep = new uk.ac.ebi.jmzidml.model.mzidml.Peptide();
                            mzidPep.setPeptideSequence(domain.getDomainSequence());                 
                        
                            mzidPep.setId(uniquePep);
                            List<uk.ac.ebi.jmzidml.model.mzidml.Modification> allMods = mzidPep.getModification();
                            
                            //if(foundFixMods == true){
                            for(uk.ac.ebi.jmzidml.model.mzidml.Modification mzidFixMod : mzidFixMods){
                                allMods.add(mzidFixMod);
                            }
                            for(uk.ac.ebi.jmzidml.model.mzidml.Modification mzidVarMod : mzidVarMods){
                                allMods.add(mzidVarMod);
                            }
                            
                            if(!uniquePeps.containsKey(uniquePep)){
                                peptideList.add(mzidPep);
                                uniquePeps.put(uniquePep,mzidPep);
                            }
                        }
                        else{
                            
                            mzidPep = uniquePeps.get(uniquePep);
                        }

                        evalue = domain.getDomainExpect();
                        hyperscore = domain.getDomainHyperScore();

                        // Get the fragment ions
                        if(outputFragmentation){
                           
                            Vector IonVector = iXTandemFile.getFragmentIonsForPeptide(peptide,domain);

                            /*
                           <IonType index="2 4 4 9 7 10 8 11 8 13" charge="1">
                            <cvParam cvRef="PSI-MS" accession="MS:1001366" name="frag: internal ya ion"/>
                            <FragmentArray values="286 644.2 329.9 329.9 514.2 " Measure_ref="m_mz"/>
                            <FragmentArray values="32 194 2053 2053 125" Measure_ref="m_intensity"/>
                            <FragmentArray values="-0.2125 -0.1151 -0.2772 -0.2772 -0.0620" Measure_ref="m_error"/>
                          </IonType>

                             */

                            // Get all the ion types from the vector
                            for (int i = 0; i < IonVector.size(); i++) {
                                FragmentIon[] ions = (FragmentIon[]) IonVector.get(i);

                                IonType ion = new IonType();
                                List<Integer> ionIndexList = ion.getIndex();
                                if(ions.length > 0){
                                    List<FragmentArray> fragmentList = ion.getFragmentArray();
                                    FragmentArray mzArray = new FragmentArray();
                                    FragmentArray intArray = new FragmentArray();
                                    FragmentArray errorArray = new FragmentArray();
                                    mzArray.setMeasure(mzMeasure);
                                    intArray.setMeasure(intMeasure);
                                    errorArray.setMeasure(errorMeasure);

                                    List<Float> mzValues = mzArray.getValues();
                                    List<Float> intValues = intArray.getValues();
                                    List<Float> errorValues = errorArray.getValues();

                                    for(int j=0; j < ions.length; j++){
                                        FragmentIon fragIon = ions[j];

                                        if(j==0){
                                            int charge = (int)fragIon.getCharge();
                                            ion.setCharge(charge);
                                            CvParam cvParam = getFragmentCVParam(fragIon.getType());
                                            ion.setCvParam(cvParam);
                                        }

                                        //Reported MZ is the theoretical value
                                        mzValues.add((float)(fragIon.getMZ() + fragIon.getTheoreticalExperimentalMassError()));   //TODO - check if these peaks are in MGF file, these may be theoretical ion MZ, since they don't tally with Omssa results
                                        intValues.add((float)fragIon.getIntensity());       //Note intensity values in Tandem do not match the source spectrum, appears that some processing happens
                                        errorValues.add((float)fragIon.getTheoreticalExperimentalMassError());   //TODO test if this is the correct error value, more than one error value
                                        ionIndexList.add(fragIon.getNumber());  //index position
                                    }

                                    fragmentList.add(mzArray);
                                    fragmentList.add(intArray);
                                    fragmentList.add(errorArray);

                                    ionTypeList.add(ion);
                                }
                            }

                        }
                        int precursorCharge = spectrum.getPrecursorCharge();
                        String accession = spectrum.getLabel();
                        boolean identified = false;
                        if(pepList.isEmpty()) {
                            identified = false;
                        } else {
                            identified = true;
                        }
                        
                        //Get precursorMh reported by X!Tandem and convert it back
                        //to m/z using:
                        //     m/z=(mh + z*1.007276 - 1*1.007276)/z
                        //where mh is M+H    and H=1.007276466812 (proton mass rounded from 1.007276466812)
                        //  X!Tandem is using H=1.007276 (proton mass rounded from 1.007276466812) so we will use that
                        double protonMass = 1.007276;
                        double expMZ = (spectrum.getPrecursorMh() + precursorCharge*protonMass - protonMass)/precursorCharge;
                        //round at 6 decimals:
                        expMZ = Utils.round(expMZ, 6);
                        sii.setExperimentalMassToCharge(expMZ);

                        double calcMZ = (domain.getDomainMh() + precursorCharge*protonMass - protonMass)/precursorCharge;
                        calcMZ = Utils.round(calcMZ, 6);
                        sii.setCalculatedMassToCharge(calcMZ);
                        sii.setChargeState(precursorCharge);
                        sii.setRank(1);
                        sii.setId("SII_"+ sirCounter + "_" + siiCounter);
                        siiCounter++;


                        cvParamList = sii.getCvParam();
                        //<cvParam accession="MS:1001330" name="xtandem:expect" cvRef="PSI-MS"  value="1.1e-003" />
                        //<cvParam accession="MS:1001331" name="xtandem:hyperscore" cvRef="PSI-MS"  value="60.4" />
                        cvParamList.add(makeCvParam("MS:1001330","X\\!Tandem:expect",psiCV,"" + evalue));
                        cvParamList.add(makeCvParam("MS:1001331","X\\!Tandem:hyperscore",psiCV,"" + hyperscore));

                        siiList.add(sii);                        
                    }

                    int end = domain.getDomainEnd();
                    int start = domain.getDomainStart();

                    String testPepMods = domain.getDomainSequence() + "_" + varMods + "_" +fixMods+"_" + start + "_" + end;
                    //String uniquePep = domain.getDomainSequence() + "_" + varMods + "_" +fixMods+"_";

                    String testProt = protAccession + "_" + start + "_" + end;

                    //Check this is a unique peptide
                    //TODO - Wasteful of memory, since mzidPep doesn't need to be created if this is not a unique peptide
                    Boolean newPepEvid = false;

                    if(!pepProtMap.containsKey(testPepMods)){

                         //Add peptide sequence to mzid Peptide object - this is now done above ARJ 4/4/2012
                       // mzidPep.setPeptideSequence(domain.getDomainSequence());
                        //mzidPep.setId(uniquePep);

                        //if(!uniquePeps.containsKey(uniquePep)){
                        //    peptideList.add(mzidPep);
                        //    uniquePeps.put(uniquePep,mzidPep);
                        //}
                        

                        //peptideLookup.put(testPepMods, mzidPep);
                        pepProtMap.put(testPepMods, testProt);
                        newPepEvid = true;

                        //TODO - The logic within X!Tandem is slightly complicated - need to test if the sequence is the same (new PepEvid) or different (new SII) - only for I or L within pep sequences
                    }
                    else{
                        String mappedProts = pepProtMap.get(testPepMods);
                        if(mappedProts.indexOf(testProt)==-1){  //test if testProt is within mapped prots - if yes, do nothing, if not, create a new peptideevidence
                            pepProtMap.put(testPepMods,mappedProts + ";" + testProt);
                            newPepEvid = true;
                        }
                        //mzidPep = peptideLookup.get(testPepMods);
                    }

                    PeptideEvidence pepEvid = null;
                    if(newPepEvid){
                        pepEvid = new PeptideEvidence();
                        pepEvidLookup.put(mzidPep.getId() + "_" + testProt, pepEvid);
                        pepEvid.setEnd(domain.getDomainEnd());
                        pepEvid.setStart(domain.getDomainStart());
                        pepEvid.setPeptide(mzidPep);

 
                        //pepEvid.setMissedCleavages(domain.getMissedCleavages());
                        pepEvid.setDBSequence(foundProts.get(protAccession));
                        char post = domain.getDownFlankSequence().charAt(0);
                        if(post == ']'){
                            post = '-';

                        }
                        pepEvid.setPost(""+post); //Reports 4 chars, we need the first only


                        char pre = domain.getUpFlankSequence().charAt(domain.getUpFlankSequence().length()-1);
                        if(pre == '['){
                            pre = '-';
                        }
                        pepEvid.setPre(""+pre);    //Reports 4 chars, we need last only
                        pepEvid.setId("PE" + sirCounter + "_" + siiCounter + "_" + pepEvidCounter);
                        pepEvidCounter++;
                        pepEvid.setIsDecoy(Boolean.FALSE);      //TODO - insert some code for testing a REGEX on protein accession
                        peptideEvidenceList.add(pepEvid);
                    }
                    else{
                        pepEvid = pepEvidLookup.get(mzidPep.getId() + "_" + testProt);

                    }

                    PeptideEvidenceRef peptideEvidenceRef = new PeptideEvidenceRef();
                    peptideEvidenceRef.setPeptideEvidence(pepEvid);
                    peptideEvidenceRefList.add(peptideEvidenceRef);

                    if(newSII){

                        sii.setPeptide(mzidPep);
                    }

                }
                
            }

            // Get the support data for each spectrum.
            SupportData supportData = iXTandemFile.getSupportData(spectrumNumber);

            // Fill the peptide map: for each spectrum get the corressponding peptide list.
            //peptideMap.put(spectrumNumber, pepList);


            /*************************************************
             *  *** Complete SpectrumIdentificationResult  ****
             * *********************************************
             */
            int spectrumID = (spectrum.getSpectrumId()-1);  //Index starts from 1 rather than zero (see docs for MS:1000774)
            String label = supportData.getFragIonSpectrumDescription();
            specIdentRes.setSpectrumID("index="+spectrumID);
            specIdentRes.setSpectraData(spectraData);
            specIdentRes.setId("SIR_"+ sirCounter);
            if(label!=null && !label.equals("")){
                List<CvParam> sir_cvParamList = specIdentRes.getCvParam();
                CvParam cvp = makeCvParam("MS:1000796","spectrum title",psiCV,label);
                sir_cvParamList.add(cvp);

            }
            

            specIdentResults.add(specIdentRes);
            sirCounter++;

                                     //TO - currently only implements the case where the same peptide matches to different proteins


            // Initialize the array lists
            //ArrayList<Double> mzValues;
            //ArrayList<Double> intensityValues;

            // Get the spectrum fragment mz and intensity values
            //mzValues = supportData.getXValuesFragIonMass2Charge();

            //intensityValues = supportData.getYValuesFragIonMass2Charge();

            // Fill the maps
            //allMzValues.put(new Integer(spectrumNumber), mzValues);
           // allIntensityValues.put(new Integer(spectrumNumber), intensityValues);


         }

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


     /**
      *
      * Aim is to write out set up the analysisSoftwareList following this structure:
      *  <AnalysisSoftware id="ID_software" name="xtandem" version="2008.12.1.1" >
      *      <SoftwareName>
      *              <cvParam accession="MS:1001476" name="xtandem" cvRef="PSI-MS" />
      *      </SoftwareName>
      *
     */
    public void handleAnalysisSoftware(String version){
        analysisSoftwareList = new AnalysisSoftwareList();
        List<AnalysisSoftware> analysisSoftwares = analysisSoftwareList.getAnalysisSoftware();
        analysisSoftware = new AnalysisSoftware();
        analysisSoftware.setName("xtandem");
       // analysisSoftware.setSoftwareName(makeCvParam("MS:1001476","xtandem",psiCV));

        Param tempParam = new Param();
        tempParam.setParam(makeCvParam("MS:1001476","X\\!Tandem",psiCV));
        analysisSoftware.setSoftwareName(tempParam);
        
        analysisSoftware.setId(analysisSoftID);
        analysisSoftware.setVersion(version);

        /* TO DO - need to work out how to use Param
        CvParam cvParam = new CvParam();
        cvParam.setName("xtandem");
        cvParam.setCvRef(psiCvID);
        cvParam.setAccession("MS:1001476");
        ParamAlternative paramAlt = new ParamAlternative();
        paramAlt.setCvParam(cvParam);

        analysisSoftware.setSoftwareName(makeCvParam("MS:1001476","xtandem",psiCV));
        analysisSoftware.setSoftwareName(paramAlt);
        */
        analysisSoftwares.add(analysisSoftware);

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
   public Boolean handleAnalysisProtocolCollection(InputParams inputParams){

        //Boolean (parentIsMono, Boolean fragmentIsMono, SearchModification[] searchMods, String enzymeName, Double parTolPlus, Double parTolMinus, Double fragTolPlus, Double fragTolMinus);

        analysisProtocolCollection = new AnalysisProtocolCollection();
        List<SpectrumIdentificationProtocol> sipList = analysisProtocolCollection.getSpectrumIdentificationProtocol();

        siProtocol = new SpectrumIdentificationProtocol();
        siProtocol.setId(siProtocolID);
        siProtocol.setAnalysisSoftware(analysisSoftware);

        //<cvParam accession="MS:1001083" name="ms-ms search" cvRef="PSI-MS"/>
        //siProtocol.setSearchType(makeCvParam("MS:1001083","ms-ms search",psiCV));
        Param tempParam = new Param();
        tempParam.setParam(makeCvParam("MS:1001083","ms-ms search",psiCV));
        siProtocol.setSearchType(tempParam);

        ParamList paramList = siProtocol.getAdditionalSearchParams();
        if(paramList == null){
            paramList = new ParamList();
            siProtocol.setAdditionalSearchParams(paramList);
        }
        List<CvParam> cvParamList = paramList.getCvParam();

        Boolean parentIsMono=true;  //does not appear to be a way in Tandem of specifying parent mass is average
        if(parentIsMono){
            cvParamList.add(makeCvParam("MS:1001211","parent mass type mono",psiCV));
        }
        else{
            cvParamList.add(makeCvParam("MS:1001212","parent mass type average",psiCV));
        }


        Boolean fragmentIsMono = true;
        if(inputParams.getSpectrumFragMassType().equals("average")){
            fragmentIsMono = false;
            cvParamList.add(makeCvParam("MS:1001255","fragment mass type average",psiCV));
        }
        else{
            cvParamList.add(makeCvParam("MS:1001256","fragment mass type mono",psiCV));
        }



        //TODO Modification Params - need to get these from input.xml and default_input.xml

        ModificationParams modParams = new ModificationParams();
        List<SearchModification> searchModList = modParams.getSearchModification();

        //residue, potential modification mass
        String varMods = inputParams.getResiduePotModMass();
        String fixedMods = inputParams.getResidueModMass();

        if (varMods != null)
        {
        	String[] allVarMods = varMods.split(",");

	        //	<note type="input" label="residue, modification mass">57.021469@C</note>
		//      <note type="input" label="residue, potential modification mass">15.99492@M</note>
	        for(String varMod : allVarMods){
	            //If there are no modification parameters, this is identified with the value 'None'. 
	            //In this case, skip the block below:
	            if (!varMod.equalsIgnoreCase("None"))
	            {
	                Vector<String> residues = new Vector();
	                //Boolean isMono = true;
		            String[] temp = varMod.split("@");
		            Double monoMass = Double.parseDouble(temp[0]);
		            residues.add(temp[1]);
		            ModT unimod = unimodDoc.getModByMass(monoMass, unimodMassError, fragmentIsMono, residues);
		            
		            SearchModification searchMod = new SearchModification();
		            searchMod.setFixedMod(false);
		            List<CvParam> modCvParamList = searchMod.getCvParam();
		            modCvParamList.add(makeCvParam("UNIMOD:" + unimod.getRecordId(),unimod.getTitle(),unimodCV));
		            searchMod.setMassDelta(new Float(monoMass));
		            
		            List<String> searchModResidueList = searchMod.getResidues();
		
		            for(String residue : residues){
		                searchModResidueList.add(residue);
		            }
		
		            searchModList.add(searchMod);
	            }
	        }
        }
        if (fixedMods != null)
        {
            String[] allFixedMods = fixedMods.split(",");

        	for(String fixedMod : allFixedMods){
	            //If there are no modification parameters, this is identified with the value 'None'. 
	            //In this case, skip the block below:
	        	if (!fixedMod.equalsIgnoreCase("None"))
	            {
		            Vector<String> residues = new Vector();
		            String[] temp = fixedMod.split("@");
		            Double monoMass = Double.parseDouble(temp[0]);
		            residues.add(temp[1]);
		            ModT unimod = unimodDoc.getModByMass(monoMass, unimodMassError, fragmentIsMono, residues);
		
		            SearchModification searchMod = new SearchModification();
		            searchMod.setFixedMod(true);
		            List<CvParam> modCvParamList = searchMod.getCvParam();
		            modCvParamList.add(makeCvParam("UNIMOD:" + unimod.getRecordId(),unimod.getTitle(),unimodCV));
		            searchMod.setMassDelta(new Float(monoMass));
		
		            List<String> searchModResidueList = searchMod.getResidues();
		
		            for(String residue : residues){
		                searchModResidueList.add(residue);
		            }
		
		            searchModList.add(searchMod);
	            }
	        }
        }

                /*
            <ModificationParams>
                <SearchModification fixedMod="false">
                          <ModParam massDelta="15.994919" residues="M">
                            <cvParam accession="UNIMOD:35" name="Oxidation" cvRef="UNIMOD" />
                         </ModParam>
                </SearchModification>
                */

        /*
            <Enzymes independent="0">
                <Enzyme id="ENZ_1" CTermGain="OH" NTermGain="H" missedCleavages="1" semiSpecific="0">
                <EnzymeName>
                <cvParam accession="MS:1001251" name="Trypsin" cvRef="PSI-MS" />
                </EnzymeName>
                </Enzyme>
             </Enzymes>
*/	
        //Only add this group if there are any modifications in the list:
        if (searchModList.size() > 0)
        {
        	siProtocol.setModificationParams(modParams);
        }
        Enzymes enzymes = siProtocol.getEnzymes();

        if(enzymes ==null){
            enzymes = new Enzymes();
            siProtocol.setEnzymes(enzymes);
        }
 
        enzymes.setIndependent(false);

        List<Enzyme> enzymeList = enzymes.getEnzyme();

        Enzyme enzyme = getEnzyme(inputParams.getProteinCleavageSite(),inputParams.getScoringMissCleavageSites());        

        enzymeList.add(enzyme);

        Tolerance fragTol = new Tolerance();
        Tolerance parTol = new Tolerance();

        List<CvParam> fragCvList = fragTol.getCvParam();
        CvParam fragCvPlus = getCvParamWithMassUnits(true);
        CvParam fragCvMinus = getCvParamWithMassUnits(true);


        /*
         <FragmentTolerance>
                            <cvParam accession="MS:1001412" name="search tolerance plus value" value="0.5" cvRef="PSI-MS" unitAccession="UO:0000221" unitName="dalton" unitCvRef="UO" />
                            <cvParam accession="MS:1001413" name="search tolerance minus value" value="0.5" cvRef="PSI-MS" unitAccession="UO:0000221" unitName="dalton" unitCvRef="UO" />
                    </FragmentTolerance>
                    <ParentTolerance>
                            <cvParam accession="MS:1001412" name="search tolerance plus value" value="0.5" cvRef="PSI-MS" unitAccession="UO:0000221" unitName="dalton" unitCvRef="UO" />
                            <cvParam accession="MS:1001413" name="search tolerance minus value" value="0.5" cvRef="PSI-MS" unitAccession="UO:0000221" unitName="dalton" unitCvRef="UO" />
                    </ParentTolerance>
          */

        fragCvPlus.setAccession("MS:1001412");
        fragCvPlus.setName("search tolerance plus value");
        fragCvMinus.setAccession("MS:1001413");
        fragCvMinus.setName("search tolerance minus value");
        fragCvPlus.setValue(""+inputParams.getSpectrumMonoIsoMassError());
        fragCvMinus.setValue(""+inputParams.getSpectrumMonoIsoMassError());
        fragCvList.add(fragCvPlus);
        fragCvList.add(fragCvMinus);

        List<CvParam> parCvList = parTol.getCvParam();
        CvParam parCvPlus = getCvParamWithMassUnits(true);
        CvParam parCvMinus = getCvParamWithMassUnits(true);

        parCvPlus.setAccession("MS:1001412");
        parCvPlus.setName("search tolerance plus value");
        parCvMinus.setAccession("MS:1001413");
        parCvMinus.setName("search tolerance minus value");
        parCvPlus.setValue(""+inputParams.getSpectrumParentMonoIsoMassErrorPlus());
        parCvMinus.setValue(""+inputParams.getSpectrumParentMonoIsoMassErrorMinus());
        parCvList.add(parCvPlus);
        parCvList.add(parCvMinus);

        siProtocol.setFragmentTolerance(fragTol);
        siProtocol.setParentTolerance(parTol);

        ParamList sip_paramList = siProtocol.getThreshold();
        if(sip_paramList == null){
            sip_paramList = new ParamList();
            siProtocol.setThreshold(sip_paramList);
        }
        cvParamList =sip_paramList.getCvParam();
        cvParamList.add(makeCvParam("MS:1001494","no threshold",psiCV));
        //<cvParam accession="MS:1001494" name="no threshold" cvRef="PSI-MS" />
        sipList.add(siProtocol);

        return fragmentIsMono;
   }

   public void handleInputs(String databaseName, String databaselocation, long numProts, String inputSpectraLocation){

       inputs = new Inputs();
       List<SearchDatabase> searchDBList = inputs.getSearchDatabase();

       searchDB = new SearchDatabase();
       searchDB.setId(searchDBID);
       searchDB.setNumDatabaseSequences(numProts);
       //<cvParam accession="MS:1001401" name="xtandem xml file" cvRef="PSI-MS"/>

       UserParam param = new UserParam();
       param.setName(databaseName);
       Param tempParam = new Param();
       tempParam.setParam(param);
       searchDB.setDatabaseName(tempParam);

       //searchDB.setDatabaseName(param);
       searchDB.setLocation(databaselocation);


       FileFormat ff = new FileFormat();
       ff.setCvParam(makeCvParam("MS:1001348","FASTA format",psiCV));
       searchDB.setFileFormat(ff);          //TODO - File format is hard coded
       searchDBList.add(searchDB);

       List<SourceFile> sourceFileList = inputs.getSourceFile();
       SourceFile sourceFile = new SourceFile();
       sourceFile.setLocation(inputTandemFile);
       sourceFile.setId(sourceFileID);
       ff = new FileFormat();
       ff.setCvParam(makeCvParam("MS:1001401","X\\!Tandem xml file",psiCV));
       sourceFile.setFileFormat(ff);
       sourceFileList.add(sourceFile);

       List<SpectraData> spectraDataList = inputs.getSpectraData();
       spectraData = new SpectraData();

       SpectrumIDFormat sif = new SpectrumIDFormat();
       sif.setCvParam(makeCvParam("MS:1000774","multiple peak list nativeID format",psiCV));
       spectraData.setSpectrumIDFormat(sif);
       //spectraData.setSpectrumIDFormat(makeCvParam("MS:1000774","multiple peak list nativeID format",psiCV));

       ff = new FileFormat();
       ff.setCvParam(makeCvParam("MS:1001062","Mascot MGF file",psiCV));
       spectraData.setFileFormat(ff);        //TODO - remove hard code <cvParam accession="MS:1001062" name="Mascot MGF file" cvRef="PSI-MS" />
       spectraData.setId(spectraDataID);
       spectraData.setLocation(inputSpectraLocation);
       spectraDataList.add(spectraData);

       /*
<fileFormat>
            <cvParam accession="MS:1001062" name="Mascot MGF file" cvRef="PSI-MS" />
    </fileFormat>
    <spectrumIDFormat>
            <cvParam accession="MS:1000774" name="multiple peak list nativeID format" cvRef="PSI-MS" />
    </spectrumIDFormat>
       */


       //From tandem: <note label="modelling, total proteins used">22348</note>
       /*
        <SearchDatabase location="file:///C:/inetpub/mascot/sequence/SwissProt/current/SwissProt_51.6.fasta" id="SDB_SwissProt" name="SwissProt" numDatabaseSequences="257964" numResidues="93947433" releaseDate="SwissProt_51.6.fasta" version="SwissProt_51.6.fasta">
        <fileFormat>
          <cvParam accession="MS:1001348" name="FASTA format" cvRef="PSI-MS" />
        </fileFormat>
        <DatabaseName>
          <userParam name="SwissProt_51.6.fasta" />
        </DatabaseName>
        <cvParam accession="MS:1001073" name="database type amino acid" cvRef="PSI-MS" />
      </SearchDatabase>
        */

   }


   public void writeMzidFile(String outputfile){

        try {
            FileWriter writer=null;
            if(outputfile.equals(""))
                writer = new FileWriter("55merge_tandem.mzid");
            else
                writer = new FileWriter(outputfile);
            
            MzIdentMLMarshaller m = new MzIdentMLMarshaller();

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
            writer.write(m.createXmlHeader() + "\n");


            // mzIdentML start tag

            writer.write(m.createMzIdentMLStartTag("12345") + "\n");



            m.marshall(cvList, writer);
            writer.write("\n");

            m.marshall(analysisSoftwareList, writer);
            writer.write("\n");


            m.marshall(provider, writer);
            writer.write("\n");


            m.marshall(auditCollection, writer);
            writer.write("\n");


            //m.marshall(analysisSampleCollection, writer);     //TODO - complete this part
            //writer.write("\n");


            m.marshall(sequenceCollection, writer);
            writer.write("\n");



            m.marshall(analysisCollection, writer);
            writer.write("\n");


            m.marshall(analysisProtocolCollection, writer);
            writer.write("\n");


            writer.write(m.createDataCollectionStartTag() + "\n");
            m.marshall(inputs, writer);
            writer.write("\n");


            //Inputs inputs = unmarshaller.unmarshal(MzIdentMLElement.Inputs.getXpath());
            //m.marshall(inputs, writer);
            //writer.write("\n");

            writer.write(m.createAnalysisDataStartTag() + "\n");



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

            m.marshall(siList, writer);
            writer.write("\n");


           // writer.write(m.createSpectrumIdentificationListClosingTag() + "\n");

           writer.write(m.createProteinDetectionListStartTag("PDL_1", null) + "\n");

            /*
            Iterator<ProteinAmbiguityGroup> protAmbGroupIter = unmarshaller.unmarshalCollectionFromXpath(MzIdentMLElement.ProteinAmbiguityGroup);
            while (protAmbGroupIter.hasNext()) {
                ProteinAmbiguityGroup protAmbGroup = protAmbGroupIter.next();
                m.marshall(protAmbGroup, writer);
                writer.write("\n");
            }

             */

            writer.write(m.createProteinDetectionListClosingTag() + "\n");

            writer.write(m.createAnalysisDataClosingTag() + "\n");

            writer.write(m.createDataCollectionClosingTag() + "\n");

            //BibliographicReference ref = unmarshaller.unmarshal(MzIdentMLElement.BibliographicReference.getXpath());
           // m.marshall(ref, writer);
           // writer.write("\n");



            writer.write(m.createMzIdentMLClosingTag());

            writer.close();

        } catch(IOException e){
            e.printStackTrace();
        }

    }

    /**
     * Method to guess the Unimod entry from a given mass
     * TODO - complete for all mods or import code to do this properly
     *
     */
    public CvParam getModCV(Double mass){

        CvParam modParam = new CvParam();

        /*
            <Modification location="11" residues="M" monoisotopicMassDelta="15.994919">
                <cvParam accession="UNIMOD:35" name="Oxidation" cvRef="UNIMOD"/>
            </Modification>
            <Modification location="13" residues="C" monoisotopicMassDelta="57.021464">
                    <cvParam accession="UNIMOD:4" name="Carbamidomethyl" cvRef="UNIMOD"/>
            </Modification>
        */

        if(mass <16 && mass >15.9 ){
            modParam.setAccession("UNIMOD:35");
            modParam.setCv(unimodCV);
            modParam.setName("Oxidation");
        }
        else if(mass <57.1 && mass > 57.0){
            modParam.setAccession("UNIMOD:4");
            modParam.setCv(unimodCV);
            modParam.setName("Carbamidomethyl");
        }
        else{

            System.out.println("Error: modification with mass not recognized");
            modParam.setName("ERROR");
        }

        return modParam;
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

    public CvParam getFragmentCVParam(int iontype){

        CvParam cvParam = new CvParam();
        cvParam.setCv(psiCV);

       
        switch (iontype) {
            case FragmentIon.A_ION:
                cvParam.setAccession("MS:1001229");
                cvParam.setName("frag: a ion");
                break;
            case FragmentIon.AH2O_ION:
                cvParam.setAccession("MS:1001234");
                cvParam.setName("frag: a ion - H2O");
                break;
            case FragmentIon.ANH3_ION:
                cvParam.setAccession("MS:1001235");
                cvParam.setName("frag: a ion - NH3");
                break;
            case FragmentIon.B_ION:
                cvParam.setAccession("MS:1001224");
                cvParam.setName("frag: b ion");
                break;
            case FragmentIon.BH2O_ION:
                cvParam.setAccession("MS:1001222");
                cvParam.setName("frag: b ion - H2O");
                break;
            case FragmentIon.BNH3_ION:
                cvParam.setAccession("MS:1001232");
                cvParam.setName("frag: b ion - NH3");
                break;
            case FragmentIon.C_ION:
                cvParam.setAccession("MS:1001231");
                cvParam.setName("frag: c ion");
                break;
            case FragmentIon.X_ION:
                cvParam.setAccession("MS:1001228");
                cvParam.setName("frag: x ion");
                break;
            case FragmentIon.Y_ION:
                cvParam.setAccession("MS:1001220");
                cvParam.setName("frag: y ion");
                break;
            case FragmentIon.YH2O_ION:
                cvParam.setAccession("MS:1001223");
                cvParam.setName("frag: y ion - H2O");
            case FragmentIon.YNH3_ION:
                cvParam.setAccession("MS:1001233");
                cvParam.setName("frag: y ion - NH3");
                break;
            case FragmentIon.Z_ION:
                cvParam.setAccession("MS:1001230");
                cvParam.setName("frag: z ion");
                break;
            case FragmentIon.MH_ION:
                cvParam.setAccession("MS:1001523");
                cvParam.setName("frag: precursor ion");
                break;
            case FragmentIon.MHNH3_ION:
                cvParam.setAccession("MS:1001522");
                cvParam.setName("frag: precursor ion - NH3");
                break;
            case FragmentIon.MHH2O_ION:
                cvParam.setAccession("MS:1001521");
                cvParam.setName("frag: precursor ion - H2O");
                break;
        }

        return cvParam;

    }


    public Enzyme getEnzyme(String tandemRegex, int missedCleavage){

        Enzyme enzyme = new Enzyme();
        //[KR]|{P}

        //TODO only trypsin implemented - this is difficult to convert from Regex used in X!Tandem
        enzyme.setId("Enz1");
        enzyme.setCTermGain("OH");
        enzyme.setNTermGain("H");
        enzyme.setMissedCleavages(missedCleavage);
        enzyme.setSemiSpecific(false);

        if(tandemRegex.equalsIgnoreCase("[KR]|{P}")){

            ParamList paramList = enzyme.getEnzymeName();
            if(paramList==null){
                paramList = new ParamList();
                enzyme.setEnzymeName(paramList);
            }
            List<CvParam> cvParamList = paramList.getCvParam();
            cvParamList.add(makeCvParam("MS:1001251","Trypsin",psiCV));
        }
        else{
            //TODO
            /*
             *
             [Term]
id: MS:1001303
name: Arg-C
is_a: MS:1001045 ! cleavage agent name
relationship: has_regexp MS:1001272 ! (?<=R)(?!P)

[Term]
id: MS:1001304
name: Asp-N
is_a: MS:1001045 ! cleavage agent name
relationship: has_regexp MS:1001273 ! (?=[BD])

[Term]
id: MS:1001305
name: Asp-N_ambic
is_a: MS:1001045 ! cleavage agent name
relationship: has_regexp MS:1001274 ! (?=[DE])

[Term]
id: MS:1001306
name: Chymotrypsin
is_a: MS:1001045 ! cleavage agent name
relationship: has_regexp MS:1001332 ! (?<=[FYWL])(?!P)

[Term]
id: MS:1001307
name: CNBr
is_a: MS:1001045 ! cleavage agent name
relationship: has_regexp MS:1001333 ! (?<=M)

[Term]
id: MS:1001308
name: Formic_acid
is_a: MS:1001045 ! cleavage agent name
relationship: has_regexp MS:1001334 ! ((?<=D))|((?=D))

[Term]
id: MS:1001309
name: Lys-C
is_a: MS:1001045 ! cleavage agent name
relationship: has_regexp MS:1001335 ! (?<=K)(?!P)

[Term]
id: MS:1001310
name: Lys-C/P
is_a: MS:1001045 ! cleavage agent name
relationship: has_regexp MS:1001336 ! (?<=K)

[Term]
id: MS:1001311
name: PepsinA
is_a: MS:1001045 ! cleavage agent name
relationship: has_regexp MS:1001337 ! (?<=[FL])

[Term]
id: MS:1001312
name: TrypChymo
is_a: MS:1001045 ! cleavage agent name
relationship: has_regexp MS:1001338 ! (?<=[FYWLKR])(?!P)

[Term]
id: MS:1001313
name: Trypsin/P
is_a: MS:1001045 ! cleavage agent name
relationship: has_regexp MS:1001339 ! (?<=[KR])

[Term]
id: MS:1001314
name: V8-DE
is_a: MS:1001045 ! cleavage agent name
relationship: has_regexp MS:1001340 ! (?<=[BDEZ])(?!P)

[Term]
id: MS:1001315
name: V8-E
is_a: MS:1001045 ! cleavage agent name
relationship: has_regexp MS:1001341 ! (?<=[EZ])(?!P)
*/
        }

        return enzyme;

    }

}


