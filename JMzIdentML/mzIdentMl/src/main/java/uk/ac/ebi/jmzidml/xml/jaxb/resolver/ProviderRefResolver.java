package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftware;
import uk.ac.ebi.jmzidml.model.mzidml.Provider;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 15-Nov-2010
 * @since 1.0
 */
public class ProviderRefResolver extends AbstractReferenceResolver<Provider> {

    public ProviderRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(Provider object) {
        // add objects for the refID
        String ref = object.getSoftwareRef();
        if (ref != null) {
            AnalysisSoftware refObject = this.unmarshal(ref, AnalysisSoftware.class);
            object.setSoftware(refObject);
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
        if (Provider.class.isInstance(target) && MzIdentMLElement.Provider.isAutoRefResolving()) {
            updateObject((Provider) target);
        } // else, not business of this resolver
    }
}
