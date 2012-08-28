package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.ProteinDetection;
import uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionList;
import uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionProtocol;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 16-Nov-2010
 * @since 1.0
 */
public class ProteinDetectionRefResolver extends AbstractReferenceResolver<ProteinDetection> {

    public ProteinDetectionRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(ProteinDetection object) {
        // add objects for the refID
        String ref1 = object.getProteinDetectionListRef();
        if (ref1 != null) {
            ProteinDetectionList refObject1 = this.unmarshal(ref1, ProteinDetectionList.class);
            object.setProteinDetectionList(refObject1);
        }

        String ref2 = object.getProteinDetectionProtocolRef();
        if (ref2 != null) {
            ProteinDetectionProtocol refObject2 = this.unmarshal(ref2, ProteinDetectionProtocol.class);
            object.setProteinDetectionProtocol(refObject2);
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
        if (ProteinDetection.class.isInstance(target) && MzIdentMLElement.ProteinDetection.isAutoRefResolving()) {
            updateObject((ProteinDetection) target);
        } // else, not business of this resolver
    }

}
