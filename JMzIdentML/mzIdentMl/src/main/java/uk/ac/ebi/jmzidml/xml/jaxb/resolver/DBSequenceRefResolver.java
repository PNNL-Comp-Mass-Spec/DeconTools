package uk.ac.ebi.jmzidml.xml.jaxb.resolver;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.SearchDatabase;
import uk.ac.ebi.jmzidml.model.mzidml.DBSequence;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

/**
 * @author Florian Reisinger
 *         Date: 16-Nov-2010
 * @since $version
 */
public class DBSequenceRefResolver extends AbstractReferenceResolver<DBSequence> {

    public DBSequenceRefResolver(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        super(index, cache);
    }

    @Override
    public void updateObject(DBSequence object) {
        // add objects for the refID
        String ref = object.getSearchDatabaseRef();
        if (ref != null) {
            SearchDatabase refObject = this.unmarshal(ref, SearchDatabase.class);
            object.setSearchDatabase(refObject);
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
        if (DBSequence.class.isInstance(target) && MzIdentMLElement.DBSequence.isAutoRefResolving()) {
            updateObject((DBSequence) target);
        } // else, not business of this resolver
    }

}
