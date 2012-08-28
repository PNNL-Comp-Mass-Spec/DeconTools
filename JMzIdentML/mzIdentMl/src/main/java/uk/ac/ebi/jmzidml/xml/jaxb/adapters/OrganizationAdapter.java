package uk.ac.ebi.jmzidml.xml.jaxb.adapters;

import uk.ac.ebi.jmzidml.model.mzidml.Organization;
import uk.ac.ebi.jmzidml.xml.Constants;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.cache.AdapterObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 02-Sep-2010
 * @since $version
 */
public class OrganizationAdapter extends AbstractResolvingAdapter<String, Organization> {

    public OrganizationAdapter(MzIdentMLIndexer index, AdapterObjectCache cache) {
        super(index, cache);
    }

    public Organization unmarshal(String refId) {

        Organization retval;
        if (cache.getCachedObject(refId, Organization.class) != null) {
            // ToDo: unchecked cast, may produce runtime exception! check if we can get around this
            // ToDo: maybe check returned type (but then we would have to throw exception anyway)
            retval = (Organization) cache.getCachedObject(refId, Organization.class);
            logger.debug("used cached value for ID: " + refId);
        } else {
            retval = super.unmarshal(refId, Constants.ReferencedType.Organization);
            cache.putInCache(refId, retval);
            logger.debug("cached object at ID: " + refId);
        }
        return retval;
    }

    public String marshal(Organization contact) {
        if (contact != null) {
            return contact.getId();
        } else {
            return null;
        }
    }
}