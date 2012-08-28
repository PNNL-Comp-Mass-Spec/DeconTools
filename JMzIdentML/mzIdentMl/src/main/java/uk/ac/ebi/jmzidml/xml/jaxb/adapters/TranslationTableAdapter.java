package uk.ac.ebi.jmzidml.xml.jaxb.adapters;

import uk.ac.ebi.jmzidml.model.mzidml.TranslationTable;
import uk.ac.ebi.jmzidml.xml.Constants;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.cache.AdapterObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 03-Sep-2010
 * @since $version
 */
public class TranslationTableAdapter extends AbstractResolvingAdapter<String, TranslationTable> {

    public TranslationTableAdapter(MzIdentMLIndexer index, AdapterObjectCache cache) {
        super(index, cache);
    }

    public TranslationTable unmarshal(String refId) {

        TranslationTable retval;
        if (cache.getCachedObject(refId, TranslationTable.class) != null) {
            retval = (TranslationTable) cache.getCachedObject(refId, TranslationTable.class);
            logger.debug("used cached value for ID: " + refId);
        } else {
            retval = super.unmarshal(refId, Constants.ReferencedType.TranslationTable);
            cache.putInCache(refId, retval);
            logger.debug("cached object at ID: " + refId);
        }
        return retval;
    }

    public String marshal(TranslationTable element) {
        if (element != null) {
            return element.getId();
        } else {
            return null;
        }
    }
}