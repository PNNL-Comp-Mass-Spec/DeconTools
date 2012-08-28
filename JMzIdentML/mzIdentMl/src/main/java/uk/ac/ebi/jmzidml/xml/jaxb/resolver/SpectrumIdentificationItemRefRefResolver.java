package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItem;
import uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItemRef;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author rwang
 * @author florian
 */
public class SpectrumIdentificationItemRefRefResolver extends AbstractReferenceResolver<SpectrumIdentificationItemRef> {

    public SpectrumIdentificationItemRefRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(SpectrumIdentificationItemRef object) {
        // add objects for the refID
        String ref = object.getSpectrumIdentificationItemRef();
        if (ref != null) {
            SpectrumIdentificationItem refObject = this.unmarshal(ref, SpectrumIdentificationItem.class);
            object.setSpectrumIdentificationItem(refObject);
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
        if (SpectrumIdentificationItemRef.class.isInstance(target) && MzIdentMLElement.SpectrumIdentificationItemRef.isAutoRefResolving()) {
            updateObject((SpectrumIdentificationItemRef) target);
        } // else, not business of this resolver
    }
}
