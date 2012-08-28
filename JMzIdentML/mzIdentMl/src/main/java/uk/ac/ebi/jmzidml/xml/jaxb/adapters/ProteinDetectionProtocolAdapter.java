package uk.ac.ebi.jmzidml.xml.jaxb.adapters;

import uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionProtocol;
import uk.ac.ebi.jmzidml.xml.Constants;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.cache.AdapterObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 03-Sep-2010
 * @since 0.1
 */
public class ProteinDetectionProtocolAdapter extends AbstractResolvingAdapter<String, ProteinDetectionProtocol> {

    public ProteinDetectionProtocolAdapter(MzIdentMLIndexer index, AdapterObjectCache cache) {
        super(index, cache);
    }

    public ProteinDetectionProtocol unmarshal(String refId) {

        ProteinDetectionProtocol retval;
        if (cache.getCachedObject(refId, ProteinDetectionProtocol.class) != null) {
            retval = (ProteinDetectionProtocol) cache.getCachedObject(refId, ProteinDetectionProtocol.class);
            logger.debug("used cached value for ID: " + refId);
        } else {
            retval = super.unmarshal(refId, Constants.ReferencedType.ProteinDetectionProtocol);
            cache.putInCache(refId, retval);
            logger.debug("cached object at ID: " + refId);
        }
        return retval;
    }

    public String marshal(ProteinDetectionProtocol element) {
        if (element != null) {
            return element.getId();
        } else {
            return null;
        }
    }
}