package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.Sample;
import uk.ac.ebi.jmzidml.model.mzidml.SubSample;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author rwang
 * @author florian
 */
public class SubSampleRefResolver extends AbstractReferenceResolver<SubSample> {

    public SubSampleRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(SubSample object) {
        // add objects for the refID
        String ref = object.getSampleRef();
        if (ref != null) {
            Sample refObject = this.unmarshal(ref, Sample.class);
            object.setSample(refObject);
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
        if (SubSample.class.isInstance(target) && MzIdentMLElement.SubSample.isAutoRefResolving()) {
            updateObject((SubSample) target);
        } // else, not business of this resolver
    }
}
