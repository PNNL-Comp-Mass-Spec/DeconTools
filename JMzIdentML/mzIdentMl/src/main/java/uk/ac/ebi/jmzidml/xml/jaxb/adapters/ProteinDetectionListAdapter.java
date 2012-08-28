package uk.ac.ebi.jmzidml.xml.jaxb.adapters;

import uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionList;
import uk.ac.ebi.jmzidml.xml.Constants;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.cache.AdapterObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 03-Sep-2010
 * @since 0.1
 */
public class ProteinDetectionListAdapter extends AbstractResolvingAdapter<String, ProteinDetectionList> {

    public ProteinDetectionListAdapter(MzIdentMLIndexer index, AdapterObjectCache cache) {
        super(index, cache);
    }

    // ToDo: the referenced list might be too big to fit into memory!! We may not want to automatically resolve this reference!

    public ProteinDetectionList unmarshal(String refId) {

        // ToDo: maybe change to return 'hollow' ProteinDetectionList object with attributes filled in, but ProteinAmbiguityGroups missing (only accessible on manual load)

        ProteinDetectionList retval;
        if (cache.getCachedObject(refId, ProteinDetectionList.class) != null) {
            retval = (ProteinDetectionList) cache.getCachedObject(refId, ProteinDetectionList.class);
            logger.debug("used cached value for ID: " + refId);
        } else {
            retval = super.unmarshal(refId, Constants.ReferencedType.ProteinDetectionList);
            cache.putInCache(refId, retval);
            logger.debug("cached object at ID: " + refId);
        }
        return retval;
    }

    public String marshal(ProteinDetectionList element) {
        if (element != null) {
            return element.getId();
        } else {
            return null;
        }
    }
}