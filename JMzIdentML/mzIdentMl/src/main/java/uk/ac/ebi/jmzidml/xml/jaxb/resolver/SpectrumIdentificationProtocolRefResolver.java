package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftware;
import uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationProtocol;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 16-Nov-2010
 * @since 1.0
 */
public class SpectrumIdentificationProtocolRefResolver extends AbstractReferenceResolver<SpectrumIdentificationProtocol> {

    public SpectrumIdentificationProtocolRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(SpectrumIdentificationProtocol object) {
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
        if (SpectrumIdentificationProtocol.class.isInstance(target) && MzIdentMLElement.SpectrumIdentificationProtocol.isAutoRefResolving()) {
            updateObject((SpectrumIdentificationProtocol) target);
        } // else, not business of this resolver
    }

}
