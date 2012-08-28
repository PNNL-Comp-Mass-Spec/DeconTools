package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidence;
import uk.ac.ebi.jmzidml.model.mzidml.PeptideHypothesis;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 16-Nov-2010
 * @since 1.0
 */
public class PeptideHypothesisRefResolver extends AbstractReferenceResolver<PeptideHypothesis> {

    public PeptideHypothesisRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(PeptideHypothesis object) {
        // add objects for the refID
        String ref = object.getPeptideEvidenceRef();
        if (ref != null) {
            PeptideEvidence refObject = this.unmarshal(ref, PeptideEvidence.class);
            object.setPeptideEvidence(refObject);
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
        if (PeptideHypothesis.class.isInstance(target) && MzIdentMLElement.PeptideHypothesis.isAutoRefResolving()) {
            updateObject((PeptideHypothesis) target);
        } // else, not business of this resolver
    }
}
