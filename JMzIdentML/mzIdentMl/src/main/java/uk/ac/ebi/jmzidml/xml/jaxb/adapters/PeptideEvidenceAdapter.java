

package uk.ac.ebi.jmzidml.xml.jaxb.adapters;

/**
 *
 * @author riteshk
 */

import uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidence;
import uk.ac.ebi.jmzidml.xml.Constants;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.cache.AdapterObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

public class PeptideEvidenceAdapter extends AbstractResolvingAdapter<String, PeptideEvidence> {

    public PeptideEvidenceAdapter(MzIdentMLIndexer index, AdapterObjectCache cache) {
        super(index, cache);
    }

    public PeptideEvidence unmarshal(String refId) {

        PeptideEvidence retval;
        if (cache.getCachedObject(refId, PeptideEvidence.class) != null) {
            // ToDo: unchecked cast, may produce runtime exception! check if we can get around this
            // ToDo: maybe check returned type (but then we would have to throw exception anyway)
            retval = (PeptideEvidence) cache.getCachedObject(refId, PeptideEvidence.class);
            logger.debug("used cached value for ID: " + refId);
        } else {
            retval = super.unmarshal(refId, Constants.ReferencedType.PeptideEvidence);
            cache.putInCache(refId, retval);
            logger.debug("cached object at ID: " + refId);
        }
        return retval;
    }

    public String marshal(PeptideEvidence pep) {
        if (pep != null) {
            return pep.getId();
        } else {
            return null;
        }
    }

}
