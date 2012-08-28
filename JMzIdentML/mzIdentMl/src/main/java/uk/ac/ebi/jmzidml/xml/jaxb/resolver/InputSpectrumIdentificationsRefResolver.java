package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.InputSpectrumIdentifications;
import uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationList;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 16-Nov-2010
 * @since 1.0
 */
public class InputSpectrumIdentificationsRefResolver extends AbstractReferenceResolver<InputSpectrumIdentifications> {

    public InputSpectrumIdentificationsRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(InputSpectrumIdentifications object) {
        // add objects for the refID
        String ref = object.getSpectrumIdentificationListRef();
        if (ref != null) {
            SpectrumIdentificationList refObject = this.unmarshal(ref, SpectrumIdentificationList.class);
            object.setSpectrumIdentificationList(refObject);
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
        if (InputSpectrumIdentifications.class.isInstance(target) && MzIdentMLElement.InputSpectrumIdentifications.isAutoRefResolving()) {
            updateObject((InputSpectrumIdentifications) target);
        } // else, not business of this resolver
    }
}
