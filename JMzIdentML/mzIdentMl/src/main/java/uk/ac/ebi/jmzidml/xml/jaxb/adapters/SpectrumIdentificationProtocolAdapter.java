package uk.ac.ebi.jmzidml.xml.jaxb.adapters;

import uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationProtocol;
import uk.ac.ebi.jmzidml.xml.Constants;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.cache.AdapterObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 03-Sep-2010
 * @since 0.1
 */
public class SpectrumIdentificationProtocolAdapter extends AbstractResolvingAdapter<String, SpectrumIdentificationProtocol> {

    public SpectrumIdentificationProtocolAdapter(MzIdentMLIndexer index, AdapterObjectCache cache) {
        super(index, cache);
    }

    public SpectrumIdentificationProtocol unmarshal(String refId) {

        SpectrumIdentificationProtocol retval;
        if (cache.getCachedObject(refId, SpectrumIdentificationProtocol.class) != null) {
            retval = (SpectrumIdentificationProtocol) cache.getCachedObject(refId, SpectrumIdentificationProtocol.class);
            logger.debug("used cached value for ID: " + refId);
        } else {
            retval = super.unmarshal(refId, Constants.ReferencedType.SpectrumIdentificationProtocol);
            cache.putInCache(refId, retval);
            logger.debug("cached object at ID: " + refId);
        }
        return retval;
    }

    public String marshal(SpectrumIdentificationProtocol element) {
        if (element != null) {
            return element.getId();
        } else {
            return null;
        }
    }
}