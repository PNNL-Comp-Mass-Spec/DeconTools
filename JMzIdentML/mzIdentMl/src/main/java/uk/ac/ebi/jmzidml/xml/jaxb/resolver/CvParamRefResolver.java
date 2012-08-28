package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.Cv;
import uk.ac.ebi.jmzidml.model.mzidml.CvParam;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 16-Nov-2010
 * @since 1.0
 */
public class CvParamRefResolver extends AbstractReferenceResolver<CvParam> {

    public CvParamRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(CvParam object) {
        // add objects for the refID
        String ref = object.getCvRef();
        if (ref != null) {
            Cv refObject = this.unmarshal(ref, Cv.class);
            object.setCv(refObject);
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
        if (CvParam.class.isInstance(target) && MzIdentMLElement.CvParam.isAutoRefResolving()) {
            updateObject((CvParam) target);
        } // else, not business of this resolver
    }

}
