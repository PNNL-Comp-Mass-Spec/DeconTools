/**
 *
 * @author Ritesh
 */

package uk.ac.ebi;

import org.apache.commons.collections.keyvalue.TiedMapEntry;
import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.MzIdentML;
import uk.ac.ebi.jmzidml.model.mzidml.ProteinAmbiguityGroup;
import uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItem;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLUnmarshaller;

import java.net.URL;
import java.util.Iterator;
import java.util.List;

public class JmzIdentMLParser {

    public static void main(String[] args) {
        List<SpectrumIdentificationItem> SpectrumIdentificationItem;

        try {

//            URL xmlFileURL = JmzIdentMLParser.class.getClassLoader().getResource("Mascot_MSMS_example.mzid");
            URL xmlFileURL = JmzIdentMLParser.class.getClassLoader().getResource("SA_silac_a24.mzid");
            System.out.println("mzIdentML file: " + xmlFileURL);

            if (xmlFileURL != null) {


                boolean aUseSpectrumCache = true;

                long start = System.currentTimeMillis();
                MzIdentMLUnmarshaller unmarshaller = new MzIdentMLUnmarshaller(xmlFileURL);
                long end = System.currentTimeMillis();
                System.out.println("Time to initialise unmarshaller: " + (end - start) + "ms");

                System.out.println("mzIdentML ID: " + unmarshaller.getMzIdentMLId());
                System.out.println("mzIdentML name: " + unmarshaller.getMzIdentMLName());
                System.out.println("mzIdentML version: " + unmarshaller.getMzIdentMLVersion());

                System.out.println("Peptides: " + unmarshaller.getObjectCountForXpath(MzIdentMLElement.Peptide.getXpath()));
                System.out.println("PeptideEvidences: " + unmarshaller.getObjectCountForXpath(MzIdentMLElement.PeptideEvidence.getXpath()));

                System.out.println("ProteinAmbiguityGroups: " + unmarshaller.getObjectCountForXpath(MzIdentMLElement.ProteinAmbiguityGroup.getXpath()));

                Iterator<ProteinAmbiguityGroup> iter = unmarshaller.unmarshalCollectionFromXpath(MzIdentMLElement.ProteinAmbiguityGroup);
                while (iter.hasNext()) {
                    ProteinAmbiguityGroup group = iter.next();
                    System.out.println("ProteinAmbiguityGroup ID" + group.getId());
                }



//                MzIdentML mzIdentML_whole = (MzIdentML)unmarshaller.unmarshal(MzIdentML.class);
//                System.out.println("mzIdentML id: " + mzIdentML_whole.getId());
//                System.out.println("mzIdentML creation time: " + mzIdentML_whole.getCreationDate().getTime().toString());
//                System.out.println("mzIdentML provider role: " + mzIdentML_whole.getProvider().getContactRole().getRole().getCvParam().getName());
//                System.out.println("mzIdentML protein ambiguity groups: " + mzIdentML_whole.getDataCollection().getAnalysisData().getProteinDetectionList().getProteinAmbiguityGroup().size());


//                System.out.println("attempt to read contacts");
//                ContactRole role = unmarshaller.unmarshalFromXpath("/mzIdentML/AnalysisSoftwareList/AnalysisSoftware/ContactRole", ContactRole.class);
//                System.out.println("contactRole: " + role.getContact().getId());
//                System.out.println("contactRole: " + role.getContact().getAddress());


//                System.out.println("attempt to read Person");
//                Iterator<Person> person1Iter = unmarshaller.unmarshalCollectionFromXpath("/mzIdentML/AuditCollection/Person", Person.class);
//                while (person1Iter.hasNext()) {
//                    Person person = person1Iter.next();
//                    System.out.println("person affiliation id: " + person.getAffiliations().get(0).getOrganization().getId());
//                    System.out.println("person affiliation name: " + person.getAffiliations().get(0).getOrganization().getName());
//                }


//                System.out.println("attempt to read Person");
//                Iterator<Person> personIter = unmarshaller.unmarshalCollectionFromXpath("/mzIdentML/AuditCollection/Person", Person.class);
//                while (personIter.hasNext()) {
//                    Person person = personIter.next();
//                    System.out.println("person affiliation id: " + person.getAffiliations().get(0).getOrganization().getId());
//                    System.out.println("person affiliation name: " + person.getAffiliations().get(0).getOrganization().getName());
//                }

//                System.out.println("attempt to read DBSequence");
//                Iterator<DBSequence> seq1Iter = unmarshaller.unmarshalCollectionFromXpath("/mzIdentML/SequenceCollection/DBSequence", DBSequence.class);
//                while (seq1Iter.hasNext()) {
//                    DBSequence dbseq = seq1Iter.next();
//                    System.out.println("dbseq -> search db id: " + dbseq.getAnalysisSearchDatabase().getId());
//                    System.out.println("dbseq -> search db name: " + dbseq.getAnalysisSearchDatabase().getName());
//                }

//                System.out.println("attempt to read SpectrumIdentification");
//                Iterator<SpectrumIdentification> seq2Iter = unmarshaller.unmarshalCollectionFromXpath("/mzIdentML/AnalysisCollection/SpectrumIdentification", SpectrumIdentification.class);
//                while (seq2Iter.hasNext()) {
//                    SpectrumIdentification ident = seq2Iter.next();
//                    System.out.println("spectrum ident -> search db (1) id: " + ident.getSearchDatabase().get(0).getAnalysisSearchDatabase().getId());
//                }

//                System.out.println("attempt to read SpectrumIdentificationResult");
//                Iterator<SpectrumIdentificationResult> seq3Iter = unmarshaller.unmarshalCollectionFromXpath("/mzIdentML/DataCollection/AnalysisData/SpectrumIdentificationList/SpectrumIdentificationResult", SpectrumIdentificationResult.class);
//                while (seq3Iter.hasNext()) {
//                    SpectrumIdentificationResult element = seq3Iter.next();
//                    System.out.println("SpectrumIdentificationResult -> spectraData id: " + element.getSpectraData().getId());
//                }

//                System.out.println("attempt to read SpectrumIdentification");
//                Iterator<SpectrumIdentification> seq4Iter = unmarshaller.unmarshalCollectionFromXpath("/mzIdentML/AnalysisCollection/SpectrumIdentification", SpectrumIdentification.class);
//                while (seq4Iter.hasNext()) {
//                    SpectrumIdentification element = seq4Iter.next();
//                    System.out.println("SpectrumIdentification -> spectraData id: " + element.getInputSpectra().get(0).getSpectraData().getId());
//                }

//                System.out.println("attempt to read ProteinDetection");
//                Iterator<ProteinDetection> seq5Iter = unmarshaller.unmarshalCollectionFromXpath("/mzIdentML/AnalysisCollection/ProteinDetection", ProteinDetection.class);
//                while (seq5Iter.hasNext()) {
//                    ProteinDetection element = seq5Iter.next();
//                    System.out.println("ProteinDetection -> protocol name: " + element.getProteinDetectionProtocol().getName());
//                    System.out.println("ProteinDetection -> list -> ambibuity group (0) id: " + element.getProteinDetectionList().getProteinAmbiguityGroup().get(0).getId());
//                }

//                System.out.println("attempt to read PeptideEvidence");
//                Iterator<PeptideEvidence> seq6Iter = unmarshaller.unmarshalCollectionFromXpath("/mzIdentML/DataCollection/AnalysisData/SpectrumIdentificationList/SpectrumIdentificationResult/SpectrumIdentificationItem/PeptideEvidence", PeptideEvidence.class);
//                while (seq6Iter.hasNext()) {
//                    PeptideEvidence element = seq6Iter.next();
//                    System.out.println("PeptideEvidence id: " + element.getId());
//                    if (element.getId().equalsIgnoreCase("PE_1_1_HSP7D_MANSE_0")) {
//                        System.out.println("PeptideEvidence -> translation table: " + element.getTranslationTable());
//                    }
//                }

//                System.out.println("attempt to read SpectrumIdentificationItem");
//                Iterator<SpectrumIdentificationItem> seq7Iter = unmarshaller.unmarshalCollectionFromXpath("/mzIdentML/DataCollection/AnalysisData/SpectrumIdentificationList/SpectrumIdentificationResult/SpectrumIdentificationItem", SpectrumIdentificationItem.class);
//                while (seq7Iter.hasNext()) {
//                    SpectrumIdentificationItem element = seq7Iter.next();
//                    System.out.println("SpectrumIdentificationResult -> spectraData id: " + element.getId());
//                    if (element.getId().equalsIgnoreCase("SII_1_1")) {
//                        System.out.println("mass table id: " + element.getMassTable().getId());
//                    }
//                }



   /*
                CvList list = unmarshaller.unmarshalFromXpath("/mzIdentML/cvList", CvList.class);
                System.out.println("list = " + list);

                MzIdentMLObjectIterator iter = unmarshaller.unmarshalCollectionFromXpath("/mzIdentML/DataCollection/AnalysisData/SpectrumIdentificationList/SpectrumIdentificationResult", SpectrumIdentificationResult.class);
                while (iter.hasNext()) {
                    SpectrumIdentificationResult res = (SpectrumIdentificationResult) iter.next();
                    System.out.println("Peptide info = " + res.getSpectrumIdentificationItem().get(0).getPeptide().getId());

                     List<SpectrumIdentificationItem> SI = res.getSpectrumIdentificationItem();
                        for(int i = 0; i < SI.size(); i++){
                            if(!SI.get(i).getPeptideEvidence().isEmpty()){
                                List<PeptideEvidence> PE = SI.get(i).getPeptideEvidence();

                                for(int k = 0; k < PE.size(); k++){
                                    System.out.println("Peptide Evidence :: ID = " + PE.get(k).getId());
                                    System.out.println("Peptide Evidence :: DBSequence ID = " + PE.get(k).getDBSequence().getAccession());
                                    System.out.println("Peptide Evidence :: DBSequence Sequence = " + PE.get(k).getDBSequence().getSeq());

                                }
                            }
                        }
                }
               
                System.out.println("\n\n Testing Adapters in ProteinDetectionHypothesis : \n\n");

                MzIdentMLObjectIterator iterPD = unmarshaller.unmarshalCollectionFromXpath("/mzIdentML/DataCollection/AnalysisData/ProteinDetectionList/ProteinAmbiguityGroup/ProteinDetectionHypothesis", ProteinDetectionHypothesis.class);
                while(iterPD.hasNext()){
                    ProteinDetectionHypothesis ph = (ProteinDetectionHypothesis)iterPD.next();
                    System.out.println("ProteinDetectionHypothesis ID = " + ph.getId());
                    System.out.println("ProteinDetectionHypothesis : Resolved -- DB_Ref = " + ph.getDBSequenceProteinDetection().getId() + "\t" + ph.getDBSequenceProteinDetection().getAccession());

                    List <PeptideHypothesis> pepHyp = ph.getPeptideHypothesis();
                    System.out.println("Total Peptide Hypothesis :: " + pepHyp.size());
                    for(int s = 0; s < pepHyp.size() ; s++){
                    		PeptideEvidence phEvd = pepHyp.get(s).getPeptideEvidence();
                    		// To do :: This part resulting into null 
                    		if(phEvd != null)
                    			System.out.println("---- Resolved PeptideHypothesis :: PeptideEvidence = " + phEvd.getId());
                    }

                }



    */

            } else {
                System.out.println("FILE NOT FOUND");
            }

        } catch (Exception e) {
            e.printStackTrace();
        }

    }
}
