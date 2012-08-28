package uk.ac.ebi.jmzidml.xml.jaxb.adapters;

import uk.ac.ebi.jmzidml.model.mzidml.Sample;
import uk.ac.ebi.jmzidml.xml.Constants;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.cache.AdapterObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 03-Sep-2010
 * @since 0.1
 */
public class SampleAdapter extends AbstractResolvingAdapter<String, Sample> {

    public SampleAdapter(MzIdentMLIndexer index, AdapterObjectCache cache) {
        super(index, cache);
    }

    public Sample unmarshal(String refId) {

        Sample retval;
        if (cache.getCachedObject(refId, Sample.class) != null) {
            retval = (Sample) cache.getCachedObject(refId, Sample.class);
            logger.debug("used cached value for ID: " + refId);
        } else {
            retval = super.unmarshal(refId, Constants.ReferencedType.Sample);
            cache.putInCache(refId, retval);
            logger.debug("cached object at ID: " + refId);
        }
        return retval;
    }

    public String marshal(Sample element) {
        if (element != null) {
            return element.getId();
        } else {
            return null;
        }
    }
}