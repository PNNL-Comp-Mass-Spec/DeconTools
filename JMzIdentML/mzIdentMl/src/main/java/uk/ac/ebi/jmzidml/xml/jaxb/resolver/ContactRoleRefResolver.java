package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.AbstractContact;
import uk.ac.ebi.jmzidml.model.mzidml.ContactRole;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 15-Nov-2010
 * @since 1.0
 */
public class ContactRoleRefResolver extends AbstractReferenceResolver<ContactRole> {

    public ContactRoleRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(ContactRole object) {
        // add objects for the refID
        String ref = object.getContactRef();
        if (ref != null) {
            // AbstractContact in contactRole is a special case!
            // It can either represent a 'Person' or an 'Organisation'
            // This will be handled by the unmarshaller accordingly!
            AbstractContact refObject = this.unmarshal(ref, AbstractContact.class);
            object.setContact(refObject);
        }
    }

    /**
     * Method to perform the afterUnmarshal operation if the resolver
     * applies to the specified object.
     *
     * @param target the object to modify after unmarshalling.
     * @param parent object referencing the target. Null if target is root element.
     */
    @Override
    public void afterUnmarshal(Object target, Object parent) {
        if (ContactRole.class.isInstance(target) && MzIdentMLElement.ContactRole.isAutoRefResolving()) {
            updateObject((ContactRole) target);
        } // else, not business of this resolver
    }

}
