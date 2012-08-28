package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.SpectraData;
import uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationResult;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 16-Nov-2010
 * @since 1.0
 */
public class SpectrumIdentificationResultRefResolver extends AbstractReferenceResolver<SpectrumIdentificationResult> {

    public SpectrumIdentificationResultRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(SpectrumIdentificationResult object) {
        // add objects for the refID
        String ref = object.getSpectraDataRef();
        if (ref != null) {
            SpectraData refObject = this.unmarshal(ref, SpectraData.class);
            object.setSpectraData(refObject);
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
        if (SpectrumIdentificationResult.class.isInstance(target) && MzIdentMLElement.SpectrumIdentificationResult.isAutoRefResolving()) {
            updateObject((SpectrumIdentificationResult) target);
        } // else, not business of this resolver
    }

}
