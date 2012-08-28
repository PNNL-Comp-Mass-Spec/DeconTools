package uk.ac.ebi.jmzidml.xml.jaxb.adapters;

import uk.ac.ebi.jmzidml.model.mzidml.Measure;
import uk.ac.ebi.jmzidml.xml.Constants;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.cache.AdapterObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 03-Sep-2010
 * @since 0.1
 */
public class MeasureAdapter extends AbstractResolvingAdapter<String, Measure> {

    public MeasureAdapter(MzIdentMLIndexer index, AdapterObjectCache cache) {
        super(index, cache);
    }

    public Measure unmarshal(String refId) {

        Measure retval;
        if (cache.getCachedObject(refId, Measure.class) != null) {
            retval = (Measure) cache.getCachedObject(refId, Measure.class);
            logger.debug("used cached value for ID: " + refId);
        } else {
            retval = super.unmarshal(refId, Constants.ReferencedType.Measure);
            cache.putInCache(refId, retval);
            logger.debug("cached object at ID: " + refId);
        }
        return retval;
    }

    public String marshal(Measure element) {
        if (element != null) {
            return element.getId();
        } else {
            return null;
        }
    }
}