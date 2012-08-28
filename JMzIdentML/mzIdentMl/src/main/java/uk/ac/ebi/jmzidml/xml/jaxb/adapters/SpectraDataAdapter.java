/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package uk.ac.ebi.jmzidml.xml.jaxb.adapters;
import uk.ac.ebi.jmzidml.model.mzidml.SpectraData;
import uk.ac.ebi.jmzidml.xml.Constants;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.cache.AdapterObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 *
 * @author Ritesh
 */
public class SpectraDataAdapter extends AbstractResolvingAdapter<String, SpectraData> {

    public SpectraDataAdapter(MzIdentMLIndexer index, AdapterObjectCache cache) {
        super(index, cache);
    }

    public SpectraData unmarshal(String refId) {

        SpectraData retval;
        if (cache.getCachedObject(refId, SpectraData.class) != null) {
            retval = (SpectraData) cache.getCachedObject(refId, SpectraData.class);
            logger.debug("used cached value for ID: " + refId);
        } else {
            retval = super.unmarshal(refId, Constants.ReferencedType.SpectraData);
            cache.putInCache(refId, retval);
            logger.debug("cached object at ID: " + refId);
        }
        return retval;
    }

    public String marshal(SpectraData element) {
        if (element != null) {
            return element.getId();
        } else {
            return null;
        }
    }
}