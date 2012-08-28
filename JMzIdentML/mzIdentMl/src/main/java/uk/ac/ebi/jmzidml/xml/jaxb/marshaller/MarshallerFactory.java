package uk.ac.ebi.jmzidml.xml.jaxb.marshaller;

import org.apache.log4j.Logger;
import uk.ac.ebi.jmzidml.model.utils.ModelConstants;
import uk.ac.ebi.jmzidml.xml.Constants;
import uk.ac.ebi.jmzidml.xml.jaxb.marshaller.listeners.ObjectClassListener;

import javax.xml.bind.JAXBContext;
import javax.xml.bind.JAXBException;
import javax.xml.bind.Marshaller;

/**
 * @author Florian Reisinger
 *         Date: 21-Sep-2010
 * @since $version
 */
public class MarshallerFactory {

    private static final Logger logger = Logger.getLogger(MarshallerFactory.class);
    private static MarshallerFactory instance = new MarshallerFactory();
    private static JAXBContext jc = null;

    public static MarshallerFactory getInstance() {
        return instance;
    }

    private MarshallerFactory() {
    }

    public Marshaller initializeMarshaller() {
        logger.debug("Initializing Marshaller for mzML.");
        try {
            // Lazy caching of JAXB context.
            if(jc == null) {
                jc = JAXBContext.newInstance(ModelConstants.PACKAGE);
            }
            //create marshaller and set basic properties
            Marshaller marshaller = jc.createMarshaller();
            marshaller.setProperty(Constants.JAXB_ENCODING_PROPERTY, "UTF-8");
            marshaller.setProperty(Constants.JAXB_FORMATTING_PROPERTY, true);

            // Register a listener that calls before/afterMarshalOperation on ParamAlternative/-List objects.
            // See: ParamAlternative.beforeMarshalOperation and ParamAlternativeList.beforeMarshalOperation
            marshaller.setListener(new ObjectClassListener());


            logger.info("Marshaller initialized");

            return marshaller;

        } catch (JAXBException e) {
            logger.error("MarshallerFactory.initializeMarshaller", e);
            throw new IllegalStateException("Can't initialize marshaller: " + e.getMessage());
        }

    }

}
