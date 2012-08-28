package uk.ac.ebi.pride.tools.mzxml_parser.mzxml.unmarshaller;

import java.io.StringReader;

import javax.xml.bind.JAXBContext;
import javax.xml.bind.JAXBElement;
import javax.xml.bind.JAXBException;
import javax.xml.bind.Unmarshaller;
import javax.xml.transform.sax.SAXSource;

import org.apache.log4j.Logger;
import org.xml.sax.InputSource;

import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.ModelConstants;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.MzXMLObject;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.MzXmlElement;


public class MzXMLUnmarshallerFactory {
    private static final Logger logger = Logger.getLogger(MzXMLUnmarshallerFactory.class);

    private static MzXMLUnmarshallerFactory instance = new MzXMLUnmarshallerFactory();
    private static JAXBContext jc = null;

    private MzXMLUnmarshallerFactory() {
    }

    public static MzXMLUnmarshallerFactory getInstance() {
        return instance;
    }

    public MzXMLUnmarshaller initializeUnmarshaller() {

        try {
            // Lazy caching of the JAXB Context.
            if (jc == null) {
                jc = JAXBContext.newInstance(ModelConstants.MODEL_PKG);
            }

            //create unmarshaller
            MzXMLUnmarshaller pum = new MzXMLUnmarshallerImpl();
            logger.debug("Unmarshaller Initialized");

            return pum;

        } catch (JAXBException e) {
            logger.error("UnmarshallerFactory.initializeUnmarshaller", e);
            throw new IllegalStateException("Could not initialize unmarshaller", e);
        }
    }

    private class MzXMLUnmarshallerImpl implements MzXMLUnmarshaller {

        private Unmarshaller unmarshaller = null;

        private MzXMLUnmarshallerImpl() throws JAXBException {
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
		public synchronized <T extends MzXMLObject> T unmarshal(String xmlSnippet, MzXmlElement element)
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
