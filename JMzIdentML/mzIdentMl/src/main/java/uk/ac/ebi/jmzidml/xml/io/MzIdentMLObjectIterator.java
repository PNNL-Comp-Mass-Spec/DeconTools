/*
 * Date: 22/7/2008
 * Author: rcote
 * File: uk.ac.ebi.jmzml.xml.io.MzMLObjectIterator
 *
 * jmzml is Copyright 2008 The European Bioinformatics Institute
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 *
 *
 */

package uk.ac.ebi.jmzidml.xml.io;

import org.apache.log4j.Logger;
import org.xml.sax.InputSource;
import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.MzIdentMLObject;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.UnmarshallerFactory;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.filters.MzIdentMLNamespaceFilter;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

import javax.xml.bind.JAXBElement;
import javax.xml.bind.JAXBException;
import javax.xml.bind.Unmarshaller;
import javax.xml.transform.sax.SAXSource;
import java.io.StringReader;
import java.util.Iterator;

public class MzIdentMLObjectIterator<T extends MzIdentMLObject> implements Iterator<T> {

    private static Logger logger = Logger.getLogger(MzIdentMLObjectIterator.class);

    private Iterator<String> innerXpathIterator;
    private String xpath;
    private Class<T> cls;
    private MzIdentMLIndexer index;
    private MzIdentMLObjectCache cache;


    MzIdentMLObjectIterator(MzIdentMLElement element, MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        innerXpathIterator = index.getXmlStringIterator(element.getXpath());
        this.xpath = element.getXpath();
        this.cls = element.getClazz();
        this.index = index;
        this.cache = cache;
    }


    public boolean hasNext() {
        return innerXpathIterator != null && innerXpathIterator.hasNext();
    }

    @SuppressWarnings("unchecked")
    public T next() {
        T retval;
        // ToDo: take cache into account: for that we need ot look up the index and retrieve all the ID for the elements
        // ToDo: if that is not possible (because caching or the ID map is not enabled) we have to revert to using the XML

        // ToDo: cache or pull into Unmarshaller to re-use method! 

        try {
            String xmlSt = innerXpathIterator.next();

            if (logger.isDebugEnabled()) {
                logger.trace("XML to unmarshal: " + xmlSt);
            }

            //required for the addition of namespaces to top-level objects
            MzIdentMLNamespaceFilter xmlFilter = new MzIdentMLNamespaceFilter();
            //initializeUnmarshaller will assign the proper reader to the xmlFilter
            Unmarshaller unmarshaller = UnmarshallerFactory.getInstance().initializeUnmarshaller(index, cache, xmlFilter);
            //unmarshall the desired object
            JAXBElement<T> holder = unmarshaller.unmarshal(new SAXSource(xmlFilter, new InputSource(new StringReader(xmlSt))), cls);

            retval = holder.getValue();

            if (logger.isDebugEnabled()) {
                logger.debug("unmarshalled object = " + retval);
            }

        } catch (JAXBException e) {
            logger.error("MzMLObjectIterator.next", e);
            throw new IllegalStateException("Could not unmarshal object at xpath:" + xpath);
        }


        // ToDo: check this with Richard!
        // Not all the elements of this type are cached (otherwise we would
        // iterate over the cache instead of unmarshalling the elements here).
        // Now some of the Objects could be cached, in which case we rather
        // want to return those and discard the generated ones (to save memory).
        // We also don't want to compromise the cache by storing the same Object
        // twice or replacing the old entry with a new one.
        // So here we check if the umarshalled Object has a representative in
        // the cache (e.g. an Object with the same ID), if so we return that
        // if not, we cache the new Object and return that.
//        if (cache != null && retval instanceof IdentifiableMzIdentMLObject) {
//            IdentifiableMzIdentMLObject object = (IdentifiableMzIdentMLObject) retval;
//            T cachedObject = (T) cache.getCachedObject(object.getId(), object.getClass());
//            if (cachedObject != null) {
//                // discard the unmarshalled object and return the cached version
//                retval = cachedObject;
//            } else {
//                // not in cache yet, so put it in
//                cache.putInCache(object);
//            }
//        }

        return retval;
    }

    public void remove() {
        throw new UnsupportedOperationException(MzIdentMLObjectIterator.class.getName() + " can't be used to remove objects while iterating");
    }

}
