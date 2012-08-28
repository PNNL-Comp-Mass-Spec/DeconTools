/*
 * Adoped by Ritesh
 * Date - 26/5/2010
 *
 * The ReferenceType represent the main elements in the XML document.
 *
 * The XPath references have been created according to the main block in the mzIdentML file.
 * 
 */

package uk.ac.ebi.jmzidml.xml;

import uk.ac.ebi.jmzidml.MzIdentMLElement;

import java.util.Collections;
import java.util.HashSet;
import java.util.Set;

/**
 * ToDo: Should XML_INDEXED_XPATHS be moved to MzIdentMLIndexerFactory?
 */
public class Constants {

    public static final String JAXB_ENCODING_PROPERTY = "jaxb.encoding";
    public static final String JAXB_FORMATTING_PROPERTY = "jaxb.formatted.output";
    public static final String JAXB_SCHEMALOCATION_PROPERTY = "jaxb.schemaLocation";
    public static final String JAXB_FRAGMENT_PROPERTY = "jaxb.fragment";


    //This should contain all the schema objects that use key/keyref
    public static enum ReferencedType {

        CV,
        AnalysisSoftware,
        Provider,
        AuditCollection,
        AnalysisSampleCollection,
        SequenceCollection,
        AnalysisCollection,
        AnalysisProtocolCollection,
        DataCollection,
        BibliographicReference,
        Peptide,
        DBSequence,
        PeptideEvidence,
        ContactRole,
        Person,
        Organization,
        SearchDatabase,
        SpectraData,
        SpectrumIdentificationList,
        SpectrumIdentificationProtocol,
        ProteinDetectionList,
        ProteinDetectionProtocol,
        TranslationTable,
        MassTable,
        Sample,
        Measure
    }

    private static Set<String> xpathsToIndex = new HashSet<String>();

    static {
        for (MzIdentMLElement element : MzIdentMLElement.values()) {
            if (element.isIndexed()) {
                xpathsToIndex.add(element.getXpath());
            }
        }
        // finally make the set unmodifiable
        xpathsToIndex = Collections.unmodifiableSet(xpathsToIndex);
    }

    public static final Set<String> XML_INDEXED_XPATHS = xpathsToIndex;

}
