package uk.ac.ebi.jmzidml.xml.io;

import uk.ac.ebi.jmzidml.model.IdentifiableMzIdentMLObject;
import uk.ac.ebi.jmzidml.model.MzIdentMLObject;

import java.util.List;

/**
 * @author Florian Reisinger
 *         Date: 11-Nov-2010
 * @since 1.0
 */
public interface MzIdentMLObjectCache {

    // ToDo: change to only handle IdentifiableMzIdentMLObjects
    // ToDo: that would also mean we can not cache CvParams or UserParams, etc
    // that way we make sure that the objects have an ID which identifies them!

    public void putInCache(String id, MzIdentMLObject object);

    public void putInCache(IdentifiableMzIdentMLObject element);

    public <T extends MzIdentMLObject> T getCachedObject(String id, Class<T> cls);

    public <T extends MzIdentMLObject> boolean hasEntry(Class<T> clazz);

    public <T extends MzIdentMLObject> List<T> getEntries(Class<T> clazz);

}
