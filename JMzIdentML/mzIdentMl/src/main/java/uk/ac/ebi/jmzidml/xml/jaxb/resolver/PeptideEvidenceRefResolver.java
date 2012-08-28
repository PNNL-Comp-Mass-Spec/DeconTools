package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidence;
import uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceRef;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author rwang
 * @author florian
 */
public class PeptideEvidenceRefResolver extends AbstractReferenceResolver<PeptideEvidenceRef> {

    public PeptideEvidenceRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(PeptideEvidenceRef object) {
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
        if (PeptideEvidenceRef.class.isInstance(target) && MzIdentMLElement.PeptideEvidenceRef.isAutoRefResolving()) {
            updateObject((PeptideEvidenceRef) target);
        } // else, not business of this resolver
    }
}
