package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.DBSequence;
import uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidence;
import uk.ac.ebi.jmzidml.model.mzidml.TranslationTable;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 16-Nov-2010
 * @since $version
 */
public class PeptideEvidenceResolver extends AbstractReferenceResolver<PeptideEvidence> {

    public PeptideEvidenceResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(PeptideEvidence object) {
        // add objects for the refID
        String ref1 = object.getDBSequenceRef();
        if (ref1 != null) {
            DBSequence refObject1 = this.unmarshal(ref1, DBSequence.class);
            object.setDBSequence(refObject1);
        }

        String ref2 = object.getTranslationTableRef();
        if (ref2 != null) {
            TranslationTable refObject2 = this.unmarshal(ref2, TranslationTable.class);
            object.setTranslationTable(refObject2);
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
        if (PeptideEvidence.class.isInstance(target) && MzIdentMLElement.PeptideEvidence.isAutoRefResolving()) {
            updateObject((PeptideEvidence) target);
        } // else, not business of this resolver
    }
}
