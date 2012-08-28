package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftware;
import uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionProtocol;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 16-Nov-2010
 * @since 1.0
 */
public class ProteinDetectionProtocolRefResolver extends AbstractReferenceResolver<ProteinDetectionProtocol> {

    public ProteinDetectionProtocolRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(ProteinDetectionProtocol object) {
        // add objects for the refID
        String ref = object.getAnalysisSoftwareRef();
        if (ref != null) {
            AnalysisSoftware refObject = this.unmarshal(ref, AnalysisSoftware.class);
            object.setAnalysisSoftware(refObject);
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
        if (ProteinDetectionProtocol.class.isInstance(target) && MzIdentMLElement.ProteinDetectionProtocol.isAutoRefResolving()) {
            updateObject((ProteinDetectionProtocol) target);
        } // else, not business of this resolver
    }

}
