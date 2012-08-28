/**
 *
 * Adopted by Ritesh
 * Date - May 27, 2010
 */
package uk.ac.ebi.jmzidml.xml.io;

import org.apache.log4j.Logger;
import org.xml.sax.InputSource;
import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.MzIdentMLObject;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.UnmarshallerFactory;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.filters.MzIdentMLNamespaceFilter;
import uk.ac.ebi.jmzidml.xml.xxindex.FileUtils;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexerFactory;

import javax.naming.ConfigurationException;
import javax.xml.bind.JAXBElement;
import javax.xml.bind.JAXBException;
import javax.xml.bind.Unmarshaller;
import javax.xml.transform.sax.SAXSource;
import java.io.File;
import java.io.StringReader;
import java.net.URL;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;
import java.util.Set;
import java.util.regex.Matcher;
import java.util.regex.Pattern;


public class MzIdentMLUnmarshaller {
    private static final Logger logger = Logger.getLogger(MzIdentMLUnmarshaller.class);

    private final MzIdentMLIndexer index;
    private final MzIdentMLObjectCache cache;

    private String mzidName = null;
    private String mzidID = null;
    private String mzidVersion = null;

    // maybe those pattern are a bit too generic, but it works fine for now
    private static final Pattern ID_PATTERN = Pattern.compile("id\\s*=\\s*[\"']([^\"'>]*)?[\"']", Pattern.CASE_INSENSITIVE);
    private static final Pattern VERSION_PATTERN = Pattern.compile("version\\s*=\\s*[\"']([^\"'>]*)?[\"']", Pattern.CASE_INSENSITIVE);
    private static final Pattern NAME_PATTERN = Pattern.compile("name\\s*=\\s*[\"']([^\"'>]*)?[\"']", Pattern.CASE_INSENSITIVE);
    private static final Pattern XML_ATT_PATTERN = Pattern.compile("\\s+([A-Za-z:]+)\\s*=\\s*[\"']([^\"'>]+?)[\"']", Pattern.DOTALL);

    ///// ///// ///// ///// ///// ///// ///// ///// ///// ///// ///// ///// ///// ///// /////
    // Constructor

    public MzIdentMLUnmarshaller(URL mzIdentMLFileURL) {
        this(FileUtils.getFileFromURL(mzIdentMLFileURL));
    }

    public MzIdentMLUnmarshaller(File mzIdentMLFile) {
        this(MzIdentMLIndexerFactory.getInstance().buildIndex(mzIdentMLFile));
    }

    public MzIdentMLUnmarshaller(MzIdentMLIndexer indexer) {
        this.index = indexer;
        this.cache = null;
    }

    @Deprecated
    public MzIdentMLUnmarshaller(URL mzIdentMLFileURL, MzIdentMLObjectCache cache) {
        this(FileUtils.getFileFromURL(mzIdentMLFileURL), cache);
    }

    @Deprecated
    public MzIdentMLUnmarshaller(File mzIdentMLFile, MzIdentMLObjectCache cache) {
        this.index = MzIdentMLIndexerFactory.getInstance().buildIndex(mzIdentMLFile);
        this.cache = cache;
    }

    ///// ///// ///// ///// ///// ///// ///// ///// ///// ///// ///// ///// ///// ///// /////
    // public Methods

    public String getMzIdentMLVersion() {
        if (mzidVersion == null) {
            Matcher match = VERSION_PATTERN.matcher(index.getMzIdentMLAttributeXMLString());
            if (match.find()) {
                mzidVersion = match.group(1);
            }
        }
        return mzidVersion;
    }

    public String getMzIdentMLId() {
        if (mzidID == null) {
            Matcher match = ID_PATTERN.matcher(index.getMzIdentMLAttributeXMLString());
            if (match.find()) {
                mzidID = match.group(1);
            }
        }
        return mzidID;
    }

    public String getMzIdentMLName() {
        if (mzidName == null) {
            Matcher match = NAME_PATTERN.matcher(index.getMzIdentMLAttributeXMLString());
            if (match.find()) {
                return match.group(1);
            }
        }
        return mzidName;
    }

    /**
     * Method to retrieve attribute name-value pairs for a XML element
     * defined by it's id and Class.
     *
     * @param id the value of the 'id' attribute of the XML element.
     * @param clazz the Class representing the XML element.
     * @return A map of all the found name-value attribute pairs or
     *         null if no element with the specified id was found.
     */
    public Map<String, String> getElementAttributes(String id, Class clazz) {
        Map<String, String> attributes = new HashMap<String, String>();
        // retrieve the start tag of the corresponding XML element
        String tag = index.getStartTag(id, clazz);
        if (tag == null) {
            return null;
        }

        // parse the tag for attributes
        Matcher match = XML_ATT_PATTERN.matcher(tag);
        while (match.find()) {
            if (match.groupCount() == 2) {
                // found name - value pair
                String name = match.group(1);
                String value = match.group(2);
                // stick the found attributes in the map
                attributes.put(name, value);
            } else {
                // not a name - value pair, something is wrong!
                System.out.println("Unexpected number of groups for XML attribute: " + match.groupCount() + " in tag: " + tag);
            }

        }
        return attributes;
    }

    /**
     * @see uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer#getCount(String)  
     * @param xpath the xpath defining the XML element.
     * @return the number of XML elements matching the xpath or -1
     *         if no elements were found for the specified xpath.
     */
    public int getObjectCountForXpath(String xpath) {
        if (xpath != null) {
            return index.getCount(xpath);
        } else {
            return -1;
        }
    }

    /**
     * Unmarshal one object for the specified class.
     * Note: The class has to refer to MzIdentMLObject elements.
     *
     * @see #unmarshal(uk.ac.ebi.jmzidml.MzIdentMLElement)
     * @param clazz the type of Object to sub-class. It has to be a sub-class of MzIdentMLObject.
     * @return a object of the specified class
     */
    public <T extends MzIdentMLObject> T unmarshal(Class<T> clazz) {
        String xpath = MzIdentMLElement.getType(clazz).getXpath();
        return doUnmarshal(clazz, xpath);
    }

    public <T extends MzIdentMLObject> T unmarshal(String xpath) {
        Class<T> clazz = MzIdentMLElement.getType(xpath).getClazz();
        return doUnmarshal(clazz, xpath);
    }

    /**
     * Unmarshals one element of the type defined by the MzIdentMLElement.
     * Note: In case there are more than one element for the specified MzIdentMLElement,
     *       only one found will be returned. This is usually the first such element, but
     *       no order is guaranteed! Use appropriate methods to check that there is only
     *       one such element or to deal with a collection of elements.
     *
     * @see #unmarshalCollectionFromXpath(uk.ac.ebi.jmzidml.MzIdentMLElement)
     * @param element The MzIdentMLElement defining the type of element to unmarshal.
     * @return A MzIdentMLObject according to the type defined by the MzIdentMLElement.
     */
    public <T extends MzIdentMLObject> T unmarshal(MzIdentMLElement element) {
        // Class and xpath of the element to unmarshal
        Class<T> clazz = element.getClazz();
        String xpath = element.getXpath();

        // first check if we have an element(s) for this Class in the cache
        return doUnmarshal(clazz, xpath);
    }

    public <T extends MzIdentMLObject> Iterator<T> unmarshalCollectionFromXpath(MzIdentMLElement element) {
        // caching deactivated
//        int indexCnt = getObjectCount(element);
//
//        if (cache != null) {
//            List<MzIdentMLObject> list = cache.getEntries(element.getClazz());
//            if (list != null) {
//                int cacheCnt = list.size();
//                if (indexCnt == cacheCnt) {
//                    // all elements are already cached
//                    return cache.getEntries(element.<T>getClazz()).iterator();
//                }
//            }
//        }

        // we have to iterate over the XML elements
        return new MzIdentMLObjectIterator<T>(element, index, cache);
    }

    /**
     * Depends on the element being indexed and ID mapped.
     * See configuration of elements via MzIdentMLElement.
     *
     * @see MzIdentMLElement
     * @param element the element for which to get the IDs.
     * @return  a Set of all IDs of the specified element.
     * @throws ConfigurationException
     */
    public Set<String> getIDsForElement(MzIdentMLElement element) throws ConfigurationException {
        return index.getIDsForElement(element);
    }

    @Deprecated
    public <T extends MzIdentMLObject> T unmarshall(Class<T> clazz, String id) throws JAXBException {
        return this.unmarshal(clazz, id);
    }
    public <T extends MzIdentMLObject> T unmarshal(Class<T> clazz, String id) throws JAXBException {
        if (!index.isIDmapped(id, clazz)) {
            throw new IllegalArgumentException("No entry found for ID: " + id + " and Class: " + clazz
                    + ". Make sure the element you are looking for has an ID attribute and is id-mapped!");
        }
        String xmlSt = index.getXmlString(id, clazz);
        return generateObject(clazz, xmlSt);
    }

    ///// ///// ///// ///// ///// ///// ///// ///// ///// ///// ///// ///// ///// ///// /////
    // private Methods

    private <T extends MzIdentMLObject> T doUnmarshal(Class<T> clazz, String xpath) {
        T retval = null;
//        if (cache != null && cache.hasEntry(clazz)) {
//            retval = cache.getEntries(clazz).get(0);
//        } else {
            if (xpath != null) {
                retval = retrieveFromXML(clazz, xpath);
            } else {
                logger.info("No xpath or index entry for class " + clazz + "! Can not unmarshal!");
            }

//            // we retrieved the element from the XML (since it was not in the cache or no cache exists)
//            // now we hand it to the cache (that knows what to do with it)
//            // for now we only consider to cache elements that are IdentifiableMzIdentMLObjects (e.g. have an ID)
//            if (cache != null && retval != null && retval instanceof IdentifiableMzIdentMLObject) {
//                cache.putInCache( (IdentifiableMzIdentMLObject) retval );
//            }
//        }
        return retval;
    }

    private <T extends MzIdentMLObject> T retrieveFromXML(Class<T> cls, String xpath) {
        T retval = null;
        try {
            Iterator<String> xpathIter = index.getXmlStringIterator(xpath);
            /**
             * xpath may not be indexed or the requested cls might be optional and not present in the input xml file.
             */
            if(xpathIter == null) return null;
            if (xpathIter.hasNext()) {

                String xmlSt = xpathIter.next();

                retval = generateObject(cls, xmlSt);

            }

        } catch (JAXBException e) {
            logger.error("MzMLUnmarshaller unmarshal error: ", e);
            throw new IllegalStateException("Could not unmarshal object at xpath:" + xpath);
        }
        return retval;
    }

    private <T extends MzIdentMLObject> T generateObject(Class<T> cls, String xmlSt) throws JAXBException {
        T retval;
        if (logger.isDebugEnabled()) {
            logger.trace("XML to unmarshal: " + xmlSt);
        }

        // Create a filter to intercept events -- and patch the missing namespace
        MzIdentMLNamespaceFilter xmlFilter = new MzIdentMLNamespaceFilter();

        //required for the addition of namespaces to top-level objects
        //MzMLNamespaceFilter xmlFilter = new MzMLNamespaceFilter();
        //initializeUnmarshaller will assign the proper reader to the xmlFilter
        Unmarshaller unmarshaller = UnmarshallerFactory.getInstance().initializeUnmarshaller(index, cache, xmlFilter);
        //unmarshall the desired object
        JAXBElement<T> holder = unmarshaller.unmarshal(new SAXSource(xmlFilter, new InputSource(new StringReader(xmlSt))), cls);
        retval = holder.getValue();

        if (logger.isDebugEnabled()) {
            logger.debug("unmarshalled object = " + retval);
        }
        return retval;
    }


}
