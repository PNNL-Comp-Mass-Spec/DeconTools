/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.filters;

import org.apache.log4j.Logger;
import org.xml.sax.Attributes;
import org.xml.sax.SAXException;
import org.xml.sax.XMLReader;
import org.xml.sax.helpers.XMLFilterImpl;
import uk.ac.ebi.jmzidml.model.utils.ModelConstants;

/**
 * @author Ritesh
 */
public class MzIdentMLNamespaceFilter extends XMLFilterImpl {


    private static final Logger logger = Logger.getLogger(MzIdentMLNamespaceFilter.class);

    public MzIdentMLNamespaceFilter() {
        logger.debug("MzMLNamespaceFilter created. Remember to call setParent(XMLReader)");
    }

    public MzIdentMLNamespaceFilter(XMLReader reader) {
        super(reader);
    }

    @Override
    public void startElement(String uri, String localName, String qName, Attributes atts) throws SAXException {
        // the elements are defined by a qualified schema, but we rip them out of context with the xxindex
        // so the namespace information is lost and we have to add it again here manually
        logger.trace("Changing namespace. uri: " + uri + " \tlocalName: " + localName + " \tqName: " + qName + " \tatts: " + atts);
        if (uri.length() == 0){
            super.startElement(ModelConstants.MZIDML_NS, localName, qName, atts);
        }
        else super.startElement(uri, localName, qName, atts);
    }

}
