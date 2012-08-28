package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.DBSequence;
import uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionHypothesis;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 16-Nov-2010
 * @since 1.0
 */
public class ProteinDetectionHypothesisRefResolver extends AbstractReferenceResolver<ProteinDetectionHypothesis> {

    public ProteinDetectionHypothesisRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(ProteinDetectionHypothesis object) {
        // add objects for the refID
        String ref = object.getDBSequenceRef();
        if (ref != null) {
            DBSequence refObject = this.unmarshal(ref, DBSequence.class);
            object.setDBSequence(refObject);
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
        if (ProteinDetectionHypothesis.class.isInstance(target) && MzIdentMLElement.ProteinDetectionHypothesis.isAutoRefResolving()) {
            updateObject((ProteinDetectionHypothesis) target);
        } // else, not business of this resolver
    }

}
