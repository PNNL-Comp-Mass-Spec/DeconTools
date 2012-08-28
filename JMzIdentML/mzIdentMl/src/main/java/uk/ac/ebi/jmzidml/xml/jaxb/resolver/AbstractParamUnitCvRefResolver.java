package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.AbstractParam;
import uk.ac.ebi.jmzidml.model.mzidml.Cv;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * This resolver is to map unitCvRef to an Cv object.
 * <p/>
 * User: rwang
 * Date: 24/02/11
 * Time: 11:42
 * To change this template use File | Settings | File Templates.
 */
public class AbstractParamUnitCvRefResolver extends AbstractReferenceResolver<AbstractParam> {

    protected AbstractParamUnitCvRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(AbstractParam object) {
        // add objects for the refID
        String ref = object.getUnitCvRef();
        if (ref != null) {
            Cv refObject = this.unmarshal(ref, Cv.class);
            object.setUnitCv(refObject);
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
        if (AbstractParam.class.isInstance(target) && MzIdentMLElement.AbstractParam.isAutoRefResolving()) {
            updateObject((AbstractParam) target);
        } // else, not business of this resolver
    }
}
