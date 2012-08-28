package uk.ac.ebi.jmzidml.xml.jaxb.adapters;

import uk.ac.ebi.jmzidml.model.mzidml.SearchDatabase;
import uk.ac.ebi.jmzidml.xml.Constants;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.cache.AdapterObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 02-Sep-2010
 * @since 0.1
 */
public class SearchDatabaseAdapter extends AbstractResolvingAdapter<String, SearchDatabase> {

    public SearchDatabaseAdapter(MzIdentMLIndexer index, AdapterObjectCache cache) {
        super(index, cache);
    }

    public SearchDatabase unmarshal(String refId) {

        SearchDatabase retval;
        if (cache.getCachedObject(refId, SearchDatabase.class) != null) {
            retval = (SearchDatabase) cache.getCachedObject(refId, SearchDatabase.class);
            logger.debug("used cached value for ID: " + refId);
        } else {
            retval = super.unmarshal(refId, Constants.ReferencedType.SearchDatabase);
            cache.putInCache(refId, retval);
            logger.debug("cached object at ID: " + refId);
        }
        return retval;
    }

    public String marshal(SearchDatabase sdb) {
        if (sdb != null) {
            return sdb.getId();
        } else {
            return null;
        }
    }
}