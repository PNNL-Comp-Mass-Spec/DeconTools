package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.FragmentArray;
import uk.ac.ebi.jmzidml.model.mzidml.Measure;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 12-Nov-2010
 * @since 0.1
 */
public class FragmentArrayRefResolver extends AbstractReferenceResolver<FragmentArray> {

    public FragmentArrayRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(FragmentArray object) {
        // add objects for the refID
        String ref = object.getMeasureRef();
        if (ref != null) {
            Measure refObject = this.unmarshal(ref, Measure.class);
            object.setMeasure(refObject);
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
        if (FragmentArray.class.isInstance(target) && MzIdentMLElement.FragmentArray.isAutoRefResolving()) {
            updateObject((FragmentArray) target);
        } // else, not business of this resolver
    }

}
