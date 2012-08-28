package uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.cache;


import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.IdentifiableMzIdentMLObject;
import uk.ac.ebi.jmzidml.model.MzIdentMLObject;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * Java in memory HashMap based Object cache implementation of the MzIdentMLObjectCache.
 * Note that this implementation makes use of the MzIdentMLElement types to control which
 * Objects are cached and which are ignored.
 */
public class AdapterObjectCache implements MzIdentMLObjectCache {

    private HashMap<Class, HashMap<String, MzIdentMLObject>> cache = new HashMap<Class, HashMap<String, MzIdentMLObject>>();

    /**
     * Stores the Object in a in-memory Map.
     * Note: if a Object is really stored or not is controlled by the settings
     * of its corresponding MzIdentMLElement.
     *
     * @see MzIdentMLElement
     * @param id the ID under which to store the Object.
     * @param object the MzIdentMLObject to store.
     */
    public void putInCache(String id, MzIdentMLObject object) {
        Class cls = object.getClass();
        MzIdentMLElement element = MzIdentMLElement.getType(cls);
        if ( element.isCached() ) {
            HashMap<String, MzIdentMLObject> classCache = cache.get(cls);
            if (classCache == null) {
                classCache = new HashMap<String, MzIdentMLObject>();
                cache.put(cls, classCache);
            }
            System.out.println("Element put in cache: " + object);
            classCache.put(id, object);
        } else {
            // this element is not meant to be cached!
        }
    }

    /**
     * Convenience method to store Objects that implement the Identifiable interface.
     *
     * @param element the Identifiable to store in the cache.
     */
    public void putInCache(IdentifiableMzIdentMLObject element) {
//        putInCache(element.getId(), element);
    }

    /**
     * Retrieve a MzIdentMLObject from the object cache.
     *
     * @param id the ID of the object to retrieve.
     * @param cls the class of the object to retrieve.
     * @return the retrieved object or null if no entry was found in the cache.
     */
    @SuppressWarnings("unchecked")
    public <T extends MzIdentMLObject> T getCachedObject(String id, Class<T> cls) {

        Map<String, MzIdentMLObject> classCache = cache.get(cls);
        if (classCache == null) {
            return null;
        }
        MzIdentMLObject o = classCache.get(id);
        return (T) o;
    }

    public <T extends MzIdentMLObject> boolean hasEntry(Class<T> clazz) {
        HashMap<String, MzIdentMLObject> map = cache.get(clazz);
        return map != null && map.size() > 0;
    }

    @SuppressWarnings("unchecked")
    public <T extends MzIdentMLObject> List<T> getEntries(Class<T> clazz) {
        HashMap<String, MzIdentMLObject> map = cache.get(clazz);
        if (map == null) {
            return null;
        }
        List<T> retVal = new ArrayList<T>();
        for (MzIdentMLObject value : map.values()) {
            retVal.add((T)value);
        }
        return retVal;
    }
}
