package uk.ac.ebi.jmzidml.xml.jaxb.marshaller.listeners;

import org.apache.log4j.Logger;
import uk.ac.ebi.jmzidml.model.mzidml.*;

import javax.xml.bind.Marshaller;
import java.util.List;

/**
 * Listener to handle class specific pre/post processing steps during marshalling.
 * @author Florian Reisinger
 *         Date: 21-Sep-2010
 * @since 0.1
 */
public class ObjectClassListener extends Marshaller.Listener {

    private static final Logger log = Logger.getLogger(ObjectClassListener.class);

    public void beforeMarshal(Object source){
        log.debug("Handling " + source.getClass() + " in beforeMarshal.");
        if(source instanceof ParamList){
            List<AbstractParam> paramList = ((ParamList)source).getParamGroup();
            if(paramList.size() == 0){
                throw new IllegalArgumentException("ParamList contains an empty list of AbstractParam.");
            }
        }else if(source instanceof Fragmentation){
            List<IonType> ionTypeList = ((Fragmentation)source).getIonType();
            if(ionTypeList.size() == 0){
                throw new IllegalArgumentException("Fragmentation contains an empty list of IonType.");
            }
        }else if(source instanceof Enzymes){
            List<Enzyme> enzymeList = ((Enzymes)source).getEnzyme();
            if(enzymeList.size() == 0){
                throw new IllegalArgumentException("Enzymes contains an empty list of Enzyme.");
            }
        }else if(source instanceof CvList){
            List<Cv> cvList = ((CvList)source).getCv();
            if(cvList.size() == 0){
                throw new IllegalArgumentException("CvList contains an empty list of Cv.");
            }
        }else if(source instanceof AuditCollection){
            List<AbstractContact> contactList = ((AuditCollection)source).getPersonOrOrganization();
            if(contactList.size() == 0){
                throw new IllegalArgumentException("AuditCollection contains an empty list of AbstractContact.");
            }
        }else if(source instanceof AnalysisSampleCollection){
            List<Sample> sampleList = ((AnalysisSampleCollection)source).getSample();
            if(sampleList.size() == 0){
                throw new IllegalArgumentException("AnalysisSampleCollection contains an empty list of Sample.");
            }
        }


        // Handle the cases were we have ParamGroups, CvParams, UserParams, etc...
//        if (source instanceof ParamAlternative) {
//            ((ParamAlternative)source).beforeMarshalOperation();
//        } else if (source instanceof ParamGroupCapable) {
//            ParamGroupCapable apg = (ParamGroupCapable) source;
//            // we have to re-unite the CvParam and UserParam we split in the unmarshall process
//            apg.updateParamList();
//        }

        // Since the ID of a referenced object is updated when the referenced object is updated/added
        // and the object is not taken into account for the marshalling process, we don't really need
        // to do anything else here (regarding the automatic reference resolving).
    }

}