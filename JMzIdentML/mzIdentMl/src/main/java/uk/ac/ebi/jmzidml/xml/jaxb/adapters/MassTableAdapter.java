package uk.ac.ebi.jmzidml.xml.jaxb.adapters;

import uk.ac.ebi.jmzidml.model.mzidml.MassTable;
import uk.ac.ebi.jmzidml.xml.Constants;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.cache.AdapterObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 03-Sep-2010
 * @since 0.1
 */
public class MassTableAdapter extends AbstractResolvingAdapter<String, MassTable> {

    public MassTableAdapter(MzIdentMLIndexer index, AdapterObjectCache cache) {
        super(index, cache);
    }

    public MassTable unmarshal(String refId) {

        MassTable retval;
        if (cache.getCachedObject(refId, MassTable.class) != null) {
            retval = (MassTable) cache.getCachedObject(refId, MassTable.class);
            logger.debug("used cached value for ID: " + refId);
        } else {
            retval = super.unmarshal(refId, Constants.ReferencedType.MassTable);
            cache.putInCache(refId, retval);
            logger.debug("cached object at ID: " + refId);
        }
        return retval;
    }

    public String marshal(MassTable element) {
        if (element != null) {
            return element.getId();
        } else {
            return null;
        }
    }
}