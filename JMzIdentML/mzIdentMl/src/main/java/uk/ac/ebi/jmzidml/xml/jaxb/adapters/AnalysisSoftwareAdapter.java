/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

package uk.ac.ebi.jmzidml.xml.jaxb.adapters;

import uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftware;
import uk.ac.ebi.jmzidml.xml.Constants;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.cache.AdapterObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 *
 * @author Ritesh
 */
public class AnalysisSoftwareAdapter extends AbstractResolvingAdapter<String, AnalysisSoftware> {

    public AnalysisSoftwareAdapter(MzIdentMLIndexer index, AdapterObjectCache cache) {
        super(index, cache);
    }

    public AnalysisSoftware unmarshal(String refId) {

        AnalysisSoftware retval;
        if (cache.getCachedObject(refId, AnalysisSoftware.class) != null) {
            retval = (AnalysisSoftware) cache.getCachedObject(refId, AnalysisSoftware.class);
            logger.debug("used cached value for ID: " + refId);
        } else {
            retval = super.unmarshal(refId, Constants.ReferencedType.AnalysisSoftware);
            cache.putInCache(refId, retval);
            logger.debug("cached object at ID: " + refId);
        }
        return retval;
    }

    public String marshal(AnalysisSoftware element) {
        if (element != null) {
            return element.getId();
        } else {
            return null;
        }
    }
}