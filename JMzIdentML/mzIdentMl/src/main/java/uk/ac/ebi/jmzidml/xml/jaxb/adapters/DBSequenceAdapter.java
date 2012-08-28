/*
 * @author Ritesh
 */

package uk.ac.ebi.jmzidml.xml.jaxb.adapters;

import uk.ac.ebi.jmzidml.model.mzidml.DBSequence;
import uk.ac.ebi.jmzidml.xml.Constants;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.cache.AdapterObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

public class DBSequenceAdapter extends AbstractResolvingAdapter<String, DBSequence> {

    public DBSequenceAdapter(MzIdentMLIndexer index, AdapterObjectCache cache) {
        super(index, cache);
    }

    public DBSequence unmarshal(String refId) {

        DBSequence retval;
        if (cache.getCachedObject(refId, DBSequence.class) != null) {
            // ToDo: unchecked cast, may produce runtime exception! check if we can get around this
            // ToDo: maybe check returned type (but then we would have to throw exception anyway)
            retval = (DBSequence) cache.getCachedObject(refId, DBSequence.class);
            logger.debug("used cached value for ID: " + refId);
        } else {
            retval = super.unmarshal(refId, Constants.ReferencedType.DBSequence);
            cache.putInCache(refId, retval);
            logger.debug("cached object at ID: " + refId);
        }
        return retval;
    }

    public String marshal(DBSequence db) {
        if (db != null) {
            return db.getId();
        } else {
            return null;
        }
    }

}
