/*
 * Date: 22/7/2008
 * Author: rcote
 * File: uk.ac.ebi.jmzml.xml.jaxb.adapters.AbstractResolvingAdapter
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

package uk.ac.ebi.jmzidml.xml.jaxb.adapters;

import org.apache.log4j.Logger;
import org.xml.sax.InputSource;
import uk.ac.ebi.jmzidml.model.mzidml.*;
import uk.ac.ebi.jmzidml.xml.Constants;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.UnmarshallerFactory;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.cache.AdapterObjectCache;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.filters.MzIdentMLNamespaceFilter;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

import javax.xml.bind.JAXBElement;
import javax.xml.bind.JAXBException;
import javax.xml.bind.Unmarshaller;
import javax.xml.bind.annotation.adapters.XmlAdapter;
import javax.xml.transform.sax.SAXSource;
import java.io.StringReader;


public abstract class AbstractResolvingAdapter<ValueType, BoundType> extends XmlAdapter<ValueType, BoundType> {

    protected static final Logger logger = Logger.getLogger(AbstractResolvingAdapter.class);

    protected MzIdentMLIndexer index = null;
    protected AdapterObjectCache cache = null;
    protected boolean useSpectrumCache = true;

    /**
     * the presence of a constructor forces all subclasses to provide a valid indexer
     * for reference resolving. This will also prevent jaxb from using the no-argument
     * constructor versions of these Adapters.
     *
     * @param index
     */
    protected AbstractResolvingAdapter(MzIdentMLIndexer index, AdapterObjectCache cache) {
        this(index, cache, true);
    }

    protected AbstractResolvingAdapter(MzIdentMLIndexer index, AdapterObjectCache cache, boolean aUseSpectrumCache) {
        this.index = index;
        this.cache = cache;
        this.useSpectrumCache = aUseSpectrumCache;
    }

    public <BoundType> BoundType unmarshal(String refId, Constants.ReferencedType refType) {

        logger.debug("AbstractResolvingAdapter.unmarshal for id: " + refId);

        String xml;
        Class cls;
        // special case for ContactRole.class as we can either have a Person.class or a Organisation.class
        if (refType == Constants.ReferencedType.ContactRole) {
            // see if the ID fits a Person
            String personXML = index.getXmlString(refId, Person.class);
            // see if the ID fits an Organisation
            String organisationXML = index.getXmlString(refId, Organization.class);
            //toDo: check if not found xml is null or throws exception
            if (personXML != null && organisationXML == null) {
                xml = personXML;
                cls = Person.class;
            } else if (personXML == null && organisationXML != null) {
                xml = organisationXML;
                cls = Organization.class;
            } else {
                throw new IllegalStateException("Could not uniquely resolve ContactRole reference " + refId);
            }
        } else {
            switch (refType) {

                case CV:
                    cls = Cv.class;
                    break;
                case Peptide:
                    cls = Peptide.class;
                    break;
                case DBSequence:
                     cls = DBSequence.class;
                     break;
                case PeptideEvidence:
                     cls = PeptideEvidence.class;
                     break;
                case Organization:
                     cls = Organization.class;
                     break;
                case Person:
                     cls = Person.class;
                     break;
                case SearchDatabase:
                     cls = SearchDatabase.class;
                     break;
                case SpectraData:
                     cls = SpectraData.class;
                     break;
                case SpectrumIdentificationList:
                     cls = SpectrumIdentificationList.class;
                     break;
                case SpectrumIdentificationProtocol:
                     cls = SpectrumIdentificationProtocol.class;
                     break;
                case AnalysisSoftware:
                     cls = AnalysisSoftware.class;
                     break;
                case ProteinDetectionList:
                     cls = ProteinDetectionList.class;
                     break;
                case ProteinDetectionProtocol:
                     cls = ProteinDetectionProtocol.class;
                     break;
                case TranslationTable:
                     cls = TranslationTable.class;
                     break;
                case MassTable:
                     cls = MassTable.class;
                     break;
                case Sample:
                     cls = Sample.class;
                     break;
                case Measure:
                     cls = Measure.class;
                     break;
                default:
                    throw new IllegalStateException("Unkonwn cache type: " + refType);
            }
            xml = index.getXmlString(refId, cls);

        }

        if (logger.isDebugEnabled()) {
            logger.trace("read xml is = " + xml);
        }

        try {

            // ToDo: why not use the MzIdentMLUnmarshaller ?? -> internal state conflicts if trying to use one MzIdentMLUnmarshaller

            //required for the addition of namespaces to top-level objects
            MzIdentMLNamespaceFilter xmlFilter = new MzIdentMLNamespaceFilter();
            //initializeUnmarshaller will assign the proper reader to the xmlFilter
            //this also propagates the cache so that any associated IDREF calls
            //are handled efficiently if possible
            Unmarshaller unmarshaller = UnmarshallerFactory.getInstance().initializeUnmarshaller(index, cache, xmlFilter);

            //need to do it this way because snippet does not have a XmlRootElement annotation
            JAXBElement<BoundType> holder = unmarshaller.unmarshal(new SAXSource(xmlFilter, new InputSource(new StringReader(xml))), cls);
            return holder.getValue();

        } catch (JAXBException e) {
            logger.error("AbstractResolvingAdapter.unmarshal", e);
            throw new IllegalStateException("Could not unmarshall refId: " + refId + " of type: " + refType);
        }

    }

}
