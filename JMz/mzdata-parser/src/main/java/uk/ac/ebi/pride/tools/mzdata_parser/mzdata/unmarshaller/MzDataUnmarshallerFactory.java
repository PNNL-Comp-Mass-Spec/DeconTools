package uk.ac.ebi.pride.tools.mzdata_parser.mzdata.unmarshaller;

import java.io.StringReader;

import javax.xml.bind.JAXBContext;
import javax.xml.bind.JAXBElement;
import javax.xml.bind.JAXBException;
import javax.xml.bind.Unmarshaller;
import javax.xml.transform.sax.SAXSource;

import org.apache.log4j.Logger;
import org.xml.sax.InputSource;

import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.ModelConstants;
import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.MzDataElement;
import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.MzDataObject;

public class MzDataUnmarshallerFactory {
	private static final Logger logger = Logger.getLogger(MzDataUnmarshallerFactory.class);

    private static MzDataUnmarshallerFactory instance = new MzDataUnmarshallerFactory();
    private static JAXBContext jc = null;

    private MzDataUnmarshallerFactory() {
    }

    public static MzDataUnmarshallerFactory getInstance() {
        return instance;
    }

    public MzDataUnmarshaller initializeUnmarshaller() {

        try {
            // Lazy caching of the JAXB Context.
            if (jc == null) {
                jc = JAXBContext.newInstance(ModelConstants.MODEL_PKG);
            }

            //create unmarshaller
            MzDataUnmarshaller pum = new MzDataUnmarshallerImpl();
            logger.debug("Unmarshaller Initialized");

            return pum;

        } catch (JAXBException e) {
            logger.error("UnmarshallerFactory.initializeUnmarshaller", e);
            throw new IllegalStateException("Could not initialize unmarshaller", e);
        }
    }

    private class MzDataUnmarshallerImpl implements MzDataUnmarshaller {

        private Unmarshaller unmarshaller = null;

        private MzDataUnmarshallerImpl() throws JAXBException {
            unmarshaller = jc.createUnmarshaller();
        }

        /**
         * Add synchronization feature, unmarshaller is not thread safe by default.
         *
         * @param xmlSnippet raw xml string
         * @param element    The mzXML element to unmarshall
         * @param <T>        an instance of class type.
         * @return T    return an instance of class type.
         */
		@Override
		public synchronized <T extends MzDataObject> T unmarshal(String xmlSnippet, MzDataElement element)
				throws Exception {
			
			T retval;
            try {

                if (xmlSnippet == null || element == null) {
                    return null;
                }

                @SuppressWarnings("unchecked")
                JAXBElement<T> holder = unmarshaller.unmarshal(new SAXSource(new InputSource(new StringReader(xmlSnippet))), element.getClassType());
                retval = holder.getValue();

            } catch (JAXBException e) {
                throw new Exception("Error unmarshalling object: " + e.getMessage(), e);
            }

            return retval;
		}

    }
}

