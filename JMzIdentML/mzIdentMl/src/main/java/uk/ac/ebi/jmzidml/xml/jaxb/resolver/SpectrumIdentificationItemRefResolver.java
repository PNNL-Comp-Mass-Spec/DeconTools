package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.MassTable;
import uk.ac.ebi.jmzidml.model.mzidml.Peptide;
import uk.ac.ebi.jmzidml.model.mzidml.Sample;
import uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItem;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 16-Nov-2010
 * @since 1.0
 */
public class SpectrumIdentificationItemRefResolver extends AbstractReferenceResolver<SpectrumIdentificationItem> {

    public SpectrumIdentificationItemRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(SpectrumIdentificationItem object) {
        // add objects for the refID
        String ref1 = object.getPeptideRef();
        if (ref1 != null) {
            Peptide refObject1 = this.unmarshal(ref1, Peptide.class);
            object.setPeptide(refObject1);
        }

        String ref2 = object.getMassTableRef();
        if (ref2 != null) {
            MassTable refObject2 = this.unmarshal(ref2, MassTable.class);
            object.setMassTable(refObject2);
        }

        String ref3 = object.getSampleRef();
        if (ref3 != null) {
            Sample refObject3 = this.unmarshal(ref3, Sample.class);
            object.setSample(refObject3);
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
        if (SpectrumIdentificationItem.class.isInstance(target) && MzIdentMLElement.SpectrumIdentificationItem.isAutoRefResolving()) {
            updateObject((SpectrumIdentificationItem) target);
        } // else, not business of this resolver
    }

}
