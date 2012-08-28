package uk.ac.ebi.jmzidml;

import org.apache.log4j.Logger;
import uk.ac.ebi.jmzidml.model.MzIdentMLObject;
import uk.ac.ebi.jmzidml.model.mzidml.CvParam;
import uk.ac.ebi.jmzidml.model.mzidml.UserParam;
import uk.ac.ebi.jmzidml.model.utils.MzIdentMLElementConfig;
import uk.ac.ebi.jmzidml.model.utils.MzIdentMLElementProperties;
import uk.ac.ebi.jmzidml.xml.jaxb.resolver.AbstractReferenceResolver;

import javax.xml.bind.JAXB;
import java.net.URL;
import java.util.HashMap;
import java.util.Map;

/**
 * For performance reasons (Memory Overflow), all the reference auto-resolving have been switched off.
 * Reference auto-resolving creates too many duplicated objects, one possible solution for this is to use
 * caching.
 */
@SuppressWarnings("unused")
public enum MzIdentMLElement {


    // ToDo: define and document dependencies between flags/attributes
    // ToDo (for example: a element can not be ID mapped if it is not indexed,
    // ToDo: or an element can not be cached if it is not ID mapped)?
    // ToDo: implement according consistency checks

    // ToDo: complete xpath for all elements
    // ToDo: update indexed flag for elements that should be indexed
    // ToDo: check which elements should be cached
    // ToDo: check for which elements an id map should be generated

    /**
     * Order is important - SetupMzIdentMLElement must appear first so that initialization of cfgMap happens before it is used in
     * remaining constructors.
     */
    SetupMzIdentMLElement(),
    //                               tag name                         indexed   xpath                                                                                                                                         cached idMapped class-name                           CvParam-subclass                            UserParam-subclass                        refResolving, reference-resolver
    //AbstractContact                 (null,                              false, null, /*abstract class*/                                                                                                                         false, false, AbstractContact.class,                null,                                       null,                                           false,  null),
    AbstractContact(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractContact.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractContact.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractContact.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractContact.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractContact.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractContact.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractContact.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractContact.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractContact.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractContact.class.getName()).getRefResolverClass()),

    //AbstractParam                   (null,                              false, null, /*abstract class*/                                                                                                                         false, false, AbstractParam.class,                  null,                                       null,                                           false,   AbstractParamUnitCvRefResolver.class), // this resolver might be turned off for performance
    AbstractParam(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractParam.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractParam.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractParam.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractParam.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractParam.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractParam.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractParam.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractParam.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractParam.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AbstractParam.class.getName()).getRefResolverClass()), // this resolver might be turned off for performance

    //    Affiliations                    ("Affiliations",                    true,  "/MzIdentML/AuditCollection/Person/Affiliations",                                                                                                false, false, Affiliations.class,                   null,                                       null,                                           false,  AffiliationsRefResolver.class),
    Affiliation(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Affiliation.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Affiliation.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Affiliation.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Affiliation.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Affiliation.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Affiliation.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Affiliation.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Affiliation.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Affiliation.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Affiliation.class.getName()).getRefResolverClass()),

    //    AmbiguousResidue                ("AmbiguousResidue",                false, "/MzIdentML/AnalysisProtocolCollection/SpectrumIdentificationProtocol/MassTable/AmbiguousResidue",                                               false, false, AmbiguousResidue.class,               AmbiguousResidueCvParam.class,              AmbiguousResidueUserParam.class,                false,  null),
    AmbiguousResidue(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AmbiguousResidue.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AmbiguousResidue.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AmbiguousResidue.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AmbiguousResidue.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AmbiguousResidue.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AmbiguousResidue.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AmbiguousResidue.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AmbiguousResidue.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AmbiguousResidue.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AmbiguousResidue.class.getName()).getRefResolverClass()),

    //    AnalysisCollection              ("AnalysisCollection",              true,  "/MzIdentML/AnalysisCollection",                                                                                                                 false, false, AnalysisCollection.class,             null,                                       null,                                           false,  null),
    AnalysisCollection(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisCollection.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisCollection.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisCollection.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisCollection.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisCollection.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisCollection.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisCollection.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisCollection.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisCollection.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisCollection.class.getName()).getRefResolverClass()),

    //    AnalysisData                    ("AnalysisData",                    true,  "/MzIdentML/DataCollection/AnalysisData",                                                                                                        false, false, AnalysisData.class,                   null,                                       null,                                           false,  null),
    AnalysisData(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisData.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisData.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisData.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisData.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisData.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisData.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisData.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisData.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisData.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisData.class.getName()).getRefResolverClass()),


    //    AnalysisProtocolCollection      ("AnalysisProtocolCollection",      true,  "/MzIdentML/AnalysisProtocolCollection",                                                                                                         false, false, AnalysisProtocolCollection.class,     null,                                       null,                                           false,  null),
    AnalysisProtocolCollection(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisProtocolCollection.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisProtocolCollection.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisProtocolCollection.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisProtocolCollection.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisProtocolCollection.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisProtocolCollection.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisProtocolCollection.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisProtocolCollection.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisProtocolCollection.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisProtocolCollection.class.getName()).getRefResolverClass()),

    //AnalysisSampleCollection        ("AnalysisSampleCollection",        true,  "/MzIdentML/AnalysisSampleCollection",                                                                                                           false, false, AnalysisSampleCollection.class,       null,                                       null,                                           false,  null),
    AnalysisSampleCollection(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSampleCollection.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSampleCollection.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSampleCollection.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSampleCollection.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSampleCollection.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSampleCollection.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSampleCollection.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSampleCollection.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSampleCollection.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSampleCollection.class.getName()).getRefResolverClass()),


    //   AnalysisSoftware                ("AnalysisSoftware",                true,  "/MzIdentML/AnalysisSoftwareList/AnalysisSoftware",                                                                                              false, true,  AnalysisSoftware.class,               null,                                       null,                                           false,  null),
    AnalysisSoftware(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftware.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftware.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftware.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftware.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftware.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftware.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftware.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftware.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftware.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftware.class.getName()).getRefResolverClass()),

    //    AnalysisSoftwareList            ("AnalysisSoftwareList",            true,  "/MzIdentML/AnalysisSoftwareList",                                                                                                               false, false, AnalysisSoftwareList.class,           null,                                       null,                                           false,  null),
    AnalysisSoftwareList(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftwareList.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftwareList.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftwareList.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftwareList.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftwareList.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftwareList.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftwareList.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftwareList.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftwareList.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftwareList.class.getName()).getRefResolverClass()),

    //AuditCollection                 ("AuditCollection",                 true,  "/MzIdentML/AuditCollection",                                                                                                                    false, false, AuditCollection.class,                null,                                       null,                                           false,  null),
    AuditCollection(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AuditCollection.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AuditCollection.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AuditCollection.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AuditCollection.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AuditCollection.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AuditCollection.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AuditCollection.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AuditCollection.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AuditCollection.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.AuditCollection.class.getName()).getRefResolverClass()),


    //BibliographicReference          ("BibliographicReference",          true,  "/MzIdentML/BibliographicReference",                                                                                                             false, false, BibliographicReference.class,         null,                                       null,                                           false,  null),
    BibliographicReference(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.BibliographicReference.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.BibliographicReference.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.BibliographicReference.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.BibliographicReference.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.BibliographicReference.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.BibliographicReference.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.BibliographicReference.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.BibliographicReference.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.BibliographicReference.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.BibliographicReference.class.getName()).getRefResolverClass()),

//    Contact                         ("Contact",                         false, "/MzIdentML/AuditCollection/Contact",                                                                                                            false, false, Contact.class,                        null,                                       null,                                           false,  null),

    //    ContactRole                     ("ContactRole",                     true,  "/MzIdentML/AnalysisSoftwareList/AnalysisSoftware/ContactRole",                                                                                  false, false, ContactRole.class,                    null,                                       null,                                           false,   ContactRoleRefResolver.class),
    ContactRole(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ContactRole.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ContactRole.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ContactRole.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ContactRole.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ContactRole.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ContactRole.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ContactRole.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ContactRole.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ContactRole.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ContactRole.class.getName()).getRefResolverClass()),

    //CV                              ("cv",                              true,  "/MzIdentML/cvList/cv",                                                                                                                          false, true,  Cv.class,                             null,                                       null,                                           false,  null),
    CV(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Cv.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Cv.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Cv.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Cv.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Cv.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Cv.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Cv.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Cv.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Cv.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Cv.class.getName()).getRefResolverClass()),

    //    CvList                          ("cvList",                          true,  "/MzIdentML/cvList",                                                                                                                             false, false, CvList.class,                         null,                                       null,                                           false,  null),
    CvList(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvList.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvList.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvList.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvList.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvList.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvList.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvList.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvList.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvList.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvList.class.getName()).getRefResolverClass()),

    //CvParam                         ("cvParam",                         false, null, /* multiple locations */                                                                                                                   false, false, CvParam.class,                        null,                                       null,                                           false,   CvParamRefResolver.class),
    CvParam(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvParam.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvParam.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvParam.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvParam.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvParam.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvParam.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvParam.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvParam.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvParam.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.CvParam.class.getName()).getRefResolverClass()),

    //    DatabaseFilters                 ("DatabaseFilters",                 false, "/MzIdentML/AnalysisProtocolCollection/SpectrumIdentificationProtocol/DatabaseFilters",                                                          false, false, DatabaseFilters.class,                null,                                       null,                                           false,  null),
    DatabaseFilters(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseFilters.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseFilters.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseFilters.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseFilters.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseFilters.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseFilters.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseFilters.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseFilters.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseFilters.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseFilters.class.getName()).getRefResolverClass()),

    //    DatabaseTranslation             ("DatabaseTranslation",             true,  "/MzIdentML/AnalysisProtocolCollection/SpectrumIdentificationProtocol/DatabaseTranslation",                                                      false, false, DatabaseTranslation.class,            null,                                       null,                                           false,  null),
    DatabaseTranslation(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseTranslation.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseTranslation.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseTranslation.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseTranslation.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseTranslation.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseTranslation.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseTranslation.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseTranslation.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseTranslation.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DatabaseTranslation.class.getName()).getRefResolverClass()),

    //    DataCollection                  ("DataCollection",                  true,  "/MzIdentML/DataCollection",                                                                                                                     false, false, DataCollection.class,                 null,                                       null,                                           false,  null),
    DataCollection(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DataCollection.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DataCollection.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DataCollection.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DataCollection.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DataCollection.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DataCollection.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DataCollection.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DataCollection.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DataCollection.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DataCollection.class.getName()).getRefResolverClass()),

    //    DBSequence                      ("DBSequence",                      true,  "/MzIdentML/SequenceCollection/DBSequence",                                                                                                      false, true,  DBSequence.class,                     DBSequenceCvParam.class,                    DBSequenceUserParam.class,                      false,   DBSequenceRefResolver.class),
    DBSequence(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DBSequence.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DBSequence.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DBSequence.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DBSequence.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DBSequence.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DBSequence.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DBSequence.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DBSequence.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DBSequence.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.DBSequence.class.getName()).getRefResolverClass()),

    //    Enzyme                          ("Enzyme",                          false, "/MzIdentML/AnalysisProtocolCollection/SpectrumIdentificationProtocol/Enzymes/Enzyme",                                                           false, false, Enzyme.class,                         null,                                       null,                                           false,  null),
    Enzyme(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzyme.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzyme.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzyme.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzyme.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzyme.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzyme.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzyme.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzyme.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzyme.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzyme.class.getName()).getRefResolverClass()),

    //    Enzymes                         ("Enzymes",                         false, "/MzIdentML/AnalysisProtocolCollection/SpectrumIdentificationProtocol/Enzymes",                                                                  false, false, Enzymes.class,                        null,                                       null,                                           false,  null),
    Enzymes(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzymes.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzymes.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzymes.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzymes.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzymes.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzymes.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzymes.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzymes.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzymes.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Enzymes.class.getName()).getRefResolverClass()),

    //    ExternalData                    (null,                              false, null, /* base element, not directly used */                                                                                                      false, false, ExternalData.class,                   null,                                       null,                                           false,  null),
    ExternalData(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ExternalData.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ExternalData.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ExternalData.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ExternalData.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ExternalData.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ExternalData.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ExternalData.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ExternalData.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ExternalData.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ExternalData.class.getName()).getRefResolverClass()),

    //    FileFormat                      ("FileFormat",                      false, null, /* multiple locations */                                                                                                                   false, false, FileFormat.class,                     FileFormatCvParam.class,                    null,                                           false,  null),
    FileFormat(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FileFormat.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FileFormat.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FileFormat.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FileFormat.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FileFormat.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FileFormat.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FileFormat.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FileFormat.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FileFormat.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FileFormat.class.getName()).getRefResolverClass()),

    //    Filter                          ("Filter",                          false, "/MzIdentML/AnalysisProtocolCollection/SpectrumIdentificationProtocol/DatabaseFilters/Filter",                                                   false, false, Filter.class,                         null,                                       null,                                           false,  null),
    Filter(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Filter.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Filter.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Filter.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Filter.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Filter.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Filter.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Filter.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Filter.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Filter.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Filter.class.getName()).getRefResolverClass()),

    //    FragmentArray                   ("FragmentArray",                   false, "/MzIdentML/DataCollection/AnalysisData/SpectrumIdentificationList/SpectrumIdentificationResult/SpectrumIdentificationItem/Fragmentation/IonType/FragmentArray", false, false, FragmentArray.class,  null,                                       null,                                           false,  FragmentArrayRefResolver.class),
    FragmentArray(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentArray.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentArray.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentArray.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentArray.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentArray.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentArray.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentArray.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentArray.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentArray.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentArray.class.getName()).getRefResolverClass()),

    //    Fragmentation                   ("Fragmentation",                   false, "/MzIdentML/DataCollection/AnalysisData/SpectrumIdentificationList/SpectrumIdentificationResult/SpectrumIdentificationItem/Fragmentation",       false, false, Fragmentation.class,                  null,                                       null,                                           false,  null),
    Fragmentation(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Fragmentation.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Fragmentation.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Fragmentation.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Fragmentation.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Fragmentation.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Fragmentation.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Fragmentation.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Fragmentation.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Fragmentation.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Fragmentation.class.getName()).getRefResolverClass()),

    //    FragmentationTable              ("FragmentationTable",              true,  "/MzIdentML/DataCollection/AnalysisData/SpectrumIdentificationList/FragmentationTable",                                                          false, false, FragmentationTable.class,             null,                                       null,                                           false,  null),
    FragmentationTable(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentationTable.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentationTable.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentationTable.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentationTable.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentationTable.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentationTable.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentationTable.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentationTable.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentationTable.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.FragmentationTable.class.getName()).getRefResolverClass()),

    //    Identifiable                    (null,                              false, null, /* abstract class */                                                                                                                       false, false, Identifiable.class,                   null,                                       null,                                           false,  null),
    Identifiable(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Identifiable.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Identifiable.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Identifiable.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Identifiable.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Identifiable.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Identifiable.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Identifiable.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Identifiable.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Identifiable.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Identifiable.class.getName()).getRefResolverClass()),

    //    Inputs                          ("Inputs",                          true,  "/MzIdentML/DataCollection/Inputs",                                                                                                              false, false, Inputs.class,                         null,                                       null,                                           false,  null),
    Inputs(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Inputs.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Inputs.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Inputs.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Inputs.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Inputs.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Inputs.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Inputs.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Inputs.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Inputs.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Inputs.class.getName()).getRefResolverClass()),

    //    InputSpectra                    ("InputSpectra",                    true,  "/MzIdentML/AnalysisCollection/SpectrumIdentification/InputSpectra",                                                                             false, false, InputSpectra.class,                   null,                                       null,                                           false,   InputSpectraRefResolver.class),
    InputSpectra(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectra.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectra.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectra.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectra.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectra.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectra.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectra.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectra.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectra.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectra.class.getName()).getRefResolverClass()),

    //    InputSpectrumIdentifications    ("InputSpectrumIdentifications",    false, "/MzIdentML/AnalysisCollection/ProteinDetection/InputSpectrumIdentifications",                                                                   false, false, InputSpectrumIdentifications.class,   null,                                       null,                                           false,   InputSpectrumIdentificationsRefResolver.class),
    InputSpectrumIdentifications(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectrumIdentifications.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectrumIdentifications.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectrumIdentifications.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectrumIdentifications.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectrumIdentifications.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectrumIdentifications.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectrumIdentifications.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectrumIdentifications.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectrumIdentifications.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.InputSpectrumIdentifications.class.getName()).getRefResolverClass()),

    //    IonType                         ("IonType",                         false, "/MzIdentML/DataCollection/AnalysisData/SpectrumIdentificationList/SpectrumIdentificationResult/SpectrumIdentificationItem/Fragmentation/IonType",false, false, IonType.class,                       IonTypeCvParam.class,                       null,                                           false,  null),
    IonType(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.IonType.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.IonType.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.IonType.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.IonType.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.IonType.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.IonType.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.IonType.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.IonType.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.IonType.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.IonType.class.getName()).getRefResolverClass()),

    //    MassTable                       ("MassTable",                       true,  "/MzIdentML/AnalysisProtocolCollection/SpectrumIdentificationProtocol/MassTable",                                                                false, true,  MassTable.class,                      MassTableCvParam.class,                     MassTableUserParam.class,                       false,  null),
    MassTable(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MassTable.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MassTable.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MassTable.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MassTable.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MassTable.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MassTable.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MassTable.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MassTable.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MassTable.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MassTable.class.getName()).getRefResolverClass()),

    //    Measure                         ("Measure",                         true,  "/MzIdentML/DataCollection/AnalysisData/SpectrumIdentificationList/FragmentationTable/Measure",                                                  false, true,  Measure.class,                        MeasureCvParam.class,                       null,                                           false,  null),
    Measure(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Measure.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Measure.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Measure.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Measure.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Measure.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Measure.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Measure.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Measure.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Measure.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Measure.class.getName()).getRefResolverClass()),

    //    Modification                    ("Modification",                    false, "/MzIdentML/SequenceCollection/Peptide/Modification",                                                                                            false, false, Modification.class,                   ModificationCvParam.class,                  ModificationUserParam.class,                    false,  null),
    Modification(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Modification.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Modification.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Modification.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Modification.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Modification.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Modification.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Modification.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Modification.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Modification.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Modification.class.getName()).getRefResolverClass()),

    //    ModificationParams              ("ModificationParams",              false, "/MzIdentML/AnalysisProtocolCollection/SpectrumIdentificationProtocol/ModificationParams",                                                       false, false, ModificationParams.class,             null,                                       null,                                           false,  null),
    ModificationParams(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ModificationParams.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ModificationParams.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ModificationParams.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ModificationParams.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ModificationParams.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ModificationParams.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ModificationParams.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ModificationParams.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ModificationParams.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ModificationParams.class.getName()).getRefResolverClass()),

    //    MzIdentML                       ("MzIdentML",                       true,  "/MzIdentML",                                                                                                                                    false, true,  MzIdentML.class,                      null,                                       null,                                           false,  null),
    MzIdentML(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MzIdentML.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MzIdentML.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MzIdentML.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MzIdentML.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MzIdentML.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MzIdentML.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MzIdentML.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MzIdentML.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MzIdentML.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.MzIdentML.class.getName()).getRefResolverClass()),

    //    Organization                    ("Organization",                    true,  "/MzIdentML/AuditCollection/Organization",                                                                                                       false, true,  Organization.class,                   OrganizationCvParam.class,                  OrganizationUserParam.class,                    false,  null),
    Organization(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Organization.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Organization.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Organization.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Organization.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Organization.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Organization.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Organization.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Organization.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Organization.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Organization.class.getName()).getRefResolverClass()),

    //    Param                           (null,                              false, null, /* abstract class */                                                                                                                       false, false, Param.class,                          null,                                       null,                                           false,  null),
    Param(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Param.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Param.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Param.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Param.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Param.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Param.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Param.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Param.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Param.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Param.class.getName()).getRefResolverClass()),

    //    ParamList                       (null,                              false, null, /* multiple places */                                                                                                                      false, false, ParamList.class,                      null,                                       null,                                           false,  null),
    ParamList(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParamList.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParamList.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParamList.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParamList.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParamList.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParamList.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParamList.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParamList.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParamList.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParamList.class.getName()).getRefResolverClass()),

    //    ParentOrganization              ("Parent",                          false, "/MzIdentML/AuditCollection/Organization/Parent",                                                                                                false, false, ParentOrganization.class,             null,                                       null,                                           false,  ParentOrganizationRefResolver.class),
    ParentOrganization(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParentOrganization.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParentOrganization.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParentOrganization.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParentOrganization.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParentOrganization.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParentOrganization.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParentOrganization.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParentOrganization.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParentOrganization.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ParentOrganization.class.getName()).getRefResolverClass()),

    //    Peptide                         ("Peptide",                         true,  "/MzIdentML/SequenceCollection/Peptide",                                                                                                         false, true,  Peptide.class,                        PeptideCvParam.class,                       PeptideUserParam.class,                         false,  null),
    Peptide(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Peptide.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Peptide.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Peptide.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Peptide.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Peptide.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Peptide.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Peptide.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Peptide.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Peptide.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Peptide.class.getName()).getRefResolverClass()),

    //    PeptideEvidence                 ("PeptideEvidence",                 true,  "/MzIdentML/SequenceCollection/PeptideEvidenceList/PeptideEvidence",                                                                             false, true,  PeptideEvidence.class,                PeptideEvidenceCvParam.class,               PeptideEvidenceUserParam.class,                 false,   PeptideEvidenceResolver.class),
    PeptideEvidence(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidence.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidence.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidence.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidence.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidence.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidence.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidence.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidence.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidence.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidence.class.getName()).getRefResolverClass()),

//    PeptideEvidenceList             ("PeptideEvidenceList",             true,  "/MzIdentML/SequenceCollection/PeptideEvidenceList",                                                                                             false, true,  PeptideEvidenceList.class,            PeptideEvidenceListCvParam.class,           PeptideEvidenceListUserParam.class,                                           false,  null),
/*
    PeptideEvidenceList             (getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceList.class.getName()).getTagName(),
                                     getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceList.class.getName()).isIndexed(),
                                     getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceList.class.getName()).getXpath(),
                                     getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceList.class.getName()).isCached(),
                                     getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceList.class.getName()).isIdMapped(),
                                     getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceList.class.getName()).getClazz(),
                                     getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceList.class.getName()).getCvParamClass(),
                                     getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceList.class.getName()).getUserParamClass(),
                                     getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceList.class.getName()).isAutoRefResolving(),
                                     getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceList.class.getName()).getRefResolverClass()),
*/

    //    PeptideEvidenceRef              ("PeptideEvidenceRef",              false, "/MzIdentML/DataCollection/AnalysisData/SpectrumIdentificationList/SpectrumIdentificationResult/SpectrumIdentificationItem/PeptideEvidenceRef",  false, false, PeptideEvidenceRef.class,             null,                                       null,                                           false,   PeptideEvidenceRefResolver.class),
    PeptideEvidenceRef(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceRef.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceRef.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceRef.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceRef.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceRef.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceRef.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceRef.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceRef.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceRef.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideEvidenceRef.class.getName()).getRefResolverClass()),

    //    PeptideHypothesis               ("PeptideHypothesis",               false, "/MzIdentML/DataCollection/AnalysisData/ProteinDetectionList/ProteinAmbiguityGroup/ProteinDetectionHypothesis/PeptideHypothesis",                false, false, PeptideHypothesis.class,              null,                                       null,                                           false,   PeptideHypothesisRefResolver.class),
    PeptideHypothesis(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideHypothesis.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideHypothesis.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideHypothesis.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideHypothesis.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideHypothesis.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideHypothesis.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideHypothesis.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideHypothesis.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideHypothesis.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.PeptideHypothesis.class.getName()).getRefResolverClass()),

    //    Person                          ("Person",                          true,  "/MzIdentML/AuditCollection/Person",                                                                                                             false, true,  Person.class,                         PersonCvParam.class,                        PersonUserParam.class,                          false,  null),
    Person(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Person.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Person.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Person.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Person.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Person.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Person.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Person.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Person.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Person.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Person.class.getName()).getRefResolverClass()),

    //    ProteinAmbiguityGroup           ("ProteinAmbiguityGroup",           true,  "/MzIdentML/DataCollection/AnalysisData/ProteinDetectionList/ProteinAmbiguityGroup",                                                             false, false, ProteinAmbiguityGroup.class,          ProteinAmbiguityGroupCvParam.class,         ProteinAmbiguityGroupUserParam.class,           false,  null),
    ProteinAmbiguityGroup(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinAmbiguityGroup.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinAmbiguityGroup.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinAmbiguityGroup.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinAmbiguityGroup.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinAmbiguityGroup.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinAmbiguityGroup.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinAmbiguityGroup.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinAmbiguityGroup.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinAmbiguityGroup.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinAmbiguityGroup.class.getName()).getRefResolverClass()),

    //    ProteinDetection                ("ProteinDetection",                true,  "/MzIdentML/AnalysisCollection/ProteinDetection",                                                                                                false, false, ProteinDetection.class,               null,                                       null,                                           false,   ProteinDetectionRefResolver.class),
    ProteinDetection(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetection.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetection.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetection.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetection.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetection.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetection.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetection.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetection.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetection.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetection.class.getName()).getRefResolverClass()),

    //    ProteinDetectionHypothesis      ("ProteinDetectionHypothesis",      true,  "/MzIdentML/DataCollection/AnalysisData/ProteinDetectionList/ProteinAmbiguityGroup/ProteinDetectionHypothesis",                                  false, false, ProteinDetectionHypothesis.class,     ProteinDetectionHypothesisCvParam.class,    ProteinDetectionHypothesisUserParam.class,      false,   ProteinDetectionHypothesisRefResolver.class),
    ProteinDetectionHypothesis(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionHypothesis.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionHypothesis.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionHypothesis.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionHypothesis.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionHypothesis.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionHypothesis.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionHypothesis.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionHypothesis.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionHypothesis.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionHypothesis.class.getName()).getRefResolverClass()),

    //    ProteinDetectionList            ("ProteinDetectionList",            true,  "/MzIdentML/DataCollection/AnalysisData/ProteinDetectionList",                                                                                   false, true,  ProteinDetectionList.class,           ProteinDetectionListCvParam.class,          ProteinDetectionListUserParam.class,            false,  null),
    ProteinDetectionList(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionList.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionList.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionList.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionList.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionList.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionList.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionList.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionList.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionList.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionList.class.getName()).getRefResolverClass()),

    //    ProteinDetectionProtocol        ("ProteinDetectionProtocol",        true,  "/MzIdentML/AnalysisProtocolCollection/ProteinDetectionProtocol",                                                                                false, true,  ProteinDetectionProtocol.class,       null,                                       null,                                           false,   ProteinDetectionProtocolRefResolver.class),
    ProteinDetectionProtocol(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionProtocol.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionProtocol.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionProtocol.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionProtocol.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionProtocol.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionProtocol.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionProtocol.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionProtocol.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionProtocol.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProteinDetectionProtocol.class.getName()).getRefResolverClass()),

    //    ProtocolApplication             (null,                              false, null, /* abstract class */                                                                                                                       false, false, ProtocolApplication.class,            null,                                       null,                                           false,  null),
    ProtocolApplication(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProtocolApplication.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProtocolApplication.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProtocolApplication.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProtocolApplication.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProtocolApplication.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProtocolApplication.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProtocolApplication.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProtocolApplication.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProtocolApplication.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.ProtocolApplication.class.getName()).getRefResolverClass()),

    //    Provider                        ("Provider",                        true,  "/MzIdentML/Provider",                                                                                                                           false, true,  Provider.class,                       null,                                       null,                                           false,   ProviderRefResolver.class),
    Provider(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Provider.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Provider.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Provider.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Provider.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Provider.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Provider.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Provider.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Provider.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Provider.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Provider.class.getName()).getRefResolverClass()),

    //    Residue                         ("Residue",                         false, "/MzIdentML/AnalysisProtocolCollection/SpectrumIdentificationProtocol/MassTable/Residue",                                                        false, false, Residue.class,                        null,                                       null,                                           false,  null),
    Residue(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Residue.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Residue.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Residue.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Residue.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Residue.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Residue.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Residue.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Residue.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Residue.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Residue.class.getName()).getRefResolverClass()),

    //    Role                            ("Role",                            false, "/MzIdentML/AnalysisSoftwareList/AnalysisSoftware/ContactRole/Role",                                                                             false, false, Role.class,                           RoleCvParam.class,                          null,                                           false,  null),
    Role(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Role.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Role.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Role.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Role.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Role.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Role.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Role.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Role.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Role.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Role.class.getName()).getRefResolverClass()),

    //    Sample                          ("Sample",                          true,  "/MzIdentML/AnalysisSampleCollection/Sample",                                                                                                    false, true,  Sample.class,                         SampleCvParam.class,                        SampleUserParam.class,                          false,  null),
    Sample(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Sample.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Sample.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Sample.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Sample.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Sample.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Sample.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Sample.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Sample.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Sample.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Sample.class.getName()).getRefResolverClass()),


    //AnalysisSearchDatabase          ("SearchDatabase",                  true,  "/MzIdentML/DataCollection/Inputs/SearchDatabase",                                                                                               false, true,  AnalysisSearchDatabase.class,         AnalysisSearchDatabaseCvParam.class,        null,                                           false,  null),
    SearchDatabase(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabase.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabase.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabase.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabase.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabase.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabase.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabase.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabase.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabase.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabase.class.getName()).getRefResolverClass()),

    //    SearchDatabaseRef               ("SearchDatabaseRef",                  false, "/MzIdentML/AnalysisCollection/SpectrumIdentification/SearchDatabaseRef",                                                                        false, false, SearchDatabaseRef.class,              null,                                       null,                                           false,   SearchDatabaseRefResolver.class),
    SearchDatabaseRef(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabaseRef.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabaseRef.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabaseRef.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabaseRef.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabaseRef.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabaseRef.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabaseRef.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabaseRef.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabaseRef.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchDatabaseRef.class.getName()).getRefResolverClass()),

    //SearchModification              ("SearchModification",              false, "/MzIdentML/AnalysisProtocolCollection/SpectrumIdentificationProtocol/ModificationParams/SearchModification",                                    false, false, SearchModification.class,             null,                                       null,                                           false,  null),
    SearchModification(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchModification.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchModification.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchModification.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchModification.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchModification.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchModification.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchModification.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchModification.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchModification.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SearchModification.class.getName()).getRefResolverClass()),

    //    SequenceCollection              ("SequenceCollection",              true,  "/MzIdentML/SequenceCollection",                                                                                                                 false, false, SequenceCollection.class,             null,                                       null,                                           false,  null),
    SequenceCollection(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SequenceCollection.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SequenceCollection.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SequenceCollection.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SequenceCollection.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SequenceCollection.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SequenceCollection.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SequenceCollection.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SequenceCollection.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SequenceCollection.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SequenceCollection.class.getName()).getRefResolverClass()),

    //    SourceFile                      ("SourceFile",                      false, "/MzIdentML/DataCollection/Inputs/SourceFile",                                                                                                   false, false, SourceFile.class,                     SourceFileCvParam.class,                    SourceFileUserParam.class,                      false,  null),
    SourceFile(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SourceFile.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SourceFile.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SourceFile.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SourceFile.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SourceFile.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SourceFile.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SourceFile.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SourceFile.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SourceFile.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SourceFile.class.getName()).getRefResolverClass()),

    //    SpecificityRules                ("SpecificityRules",                false, "/MzIdentML/AnalysisProtocolCollection/SpectrumIdentificationProtocol/ModificationParams/SearchModification/SpecificityRules",                   false, false, SpecificityRules.class,               SpecificityRulesCvParam.class,              null,                                           false,  null),
    SpecificityRules(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpecificityRules.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpecificityRules.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpecificityRules.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpecificityRules.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpecificityRules.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpecificityRules.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpecificityRules.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpecificityRules.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpecificityRules.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpecificityRules.class.getName()).getRefResolverClass()),

    //    SpectraData                     ("SpectraData",                     true,  "/MzIdentML/DataCollection/Inputs/SpectraData",                                                                                                  false, true,  SpectraData.class,                    null,                                       null,                                           false,  null),
    SpectraData(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectraData.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectraData.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectraData.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectraData.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectraData.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectraData.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectraData.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectraData.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectraData.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectraData.class.getName()).getRefResolverClass()),

    //    SpectrumIdentification          ("SpectrumIdentification",          true,  "/MzIdentML/AnalysisCollection/SpectrumIdentification",                                                                                          false, false, SpectrumIdentification.class,         null,                                       null,                                           false,   SpectrumIdentificationRefResolver.class),
    SpectrumIdentification(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentification.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentification.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentification.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentification.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentification.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentification.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentification.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentification.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentification.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentification.class.getName()).getRefResolverClass()),

    //    SpectrumIdentificationItem      ("SpectrumIdentificationItem",      true,  "/MzIdentML/DataCollection/AnalysisData/SpectrumIdentificationList/SpectrumIdentificationResult/SpectrumIdentificationItem",                     false, false, SpectrumIdentificationItem.class,     SpectrumIdentificationItemCvParam.class,    SpectrumIdentificationItemUserParam.class,      false,   SpectrumIdentificationItemRefResolver.class),
    SpectrumIdentificationItem(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItem.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItem.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItem.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItem.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItem.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItem.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItem.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItem.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItem.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItem.class.getName()).getRefResolverClass()),

    //    SpectrumIdentificationItemRef   ("SpectrumIdentificationItemRef",   false, "/MzIdentML/DataCollection/AnalysisData/ProteinDetectionList/ProteinAmbiguityGroup/ProteinDetectionHypothesis/PeptideHypothesis/SpectrumIdentificationItemRef", false, false,  SpectrumIdentificationItemRef.class,  null,                        null,                                           false,  SpectrumIdentificationItemRefRefResolver.class),
    SpectrumIdentificationItemRef(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItemRef.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItemRef.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItemRef.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItemRef.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItemRef.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItemRef.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItemRef.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItemRef.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItemRef.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationItemRef.class.getName()).getRefResolverClass()),


    //    SpectrumIdentificationList      ("SpectrumIdentificationList",      true,  "/MzIdentML/DataCollection/AnalysisData/SpectrumIdentificationList",                                                                             false, true,  SpectrumIdentificationList.class,     SpectrumIdentificationListCvParam.class,    SpectrumIdentificationListUserParam.class,      false,  null),
    SpectrumIdentificationList(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationList.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationList.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationList.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationList.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationList.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationList.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationList.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationList.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationList.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationList.class.getName()).getRefResolverClass()),

    //    SpectrumIdentificationProtocol  ("SpectrumIdentificationProtocol",  true,  "/MzIdentML/AnalysisProtocolCollection/SpectrumIdentificationProtocol",                                                                          false, true,  SpectrumIdentificationProtocol.class, null,                                       null,                                           false ,  SpectrumIdentificationProtocolRefResolver.class),
    SpectrumIdentificationProtocol(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationProtocol.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationProtocol.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationProtocol.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationProtocol.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationProtocol.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationProtocol.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationProtocol.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationProtocol.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationProtocol.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationProtocol.class.getName()).getRefResolverClass()),

    //    SpectrumIdentificationResult    ("SpectrumIdentificationResult",    true,  "/MzIdentML/DataCollection/AnalysisData/SpectrumIdentificationList/SpectrumIdentificationResult",                                                false, false, SpectrumIdentificationResult.class,   SpectrumIdentificationResultCvParam.class,  SpectrumIdentificationResultUserParam.class,    false,   SpectrumIdentificationResultRefResolver.class),
    SpectrumIdentificationResult(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationResult.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationResult.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationResult.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationResult.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationResult.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationResult.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationResult.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationResult.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationResult.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationResult.class.getName()).getRefResolverClass()),

    //    SpectrumIDFormat                ("SpectrumIDFormat",                false, "/MzIdentML/DataCollection/Inputs/SpectraData/SpectrumIDFormat",                                                                                 false, false, SpectrumIDFormat.class,               SpectrumIDFormatCvParam.class,              null,                                           false,  null),
    SpectrumIDFormat(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIDFormat.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIDFormat.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIDFormat.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIDFormat.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIDFormat.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIDFormat.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIDFormat.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIDFormat.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIDFormat.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SpectrumIDFormat.class.getName()).getRefResolverClass()),

    //    SubSample                       ("SubSample",                       false, "/MzIdentML/AnalysisSampleCollection/Sample/SubSample",                                                                                          false, false, SubSample.class,                      null,                                       null,                                           false,  SubSampleRefResolver.class),
    SubSample(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubSample.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubSample.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubSample.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubSample.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubSample.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubSample.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubSample.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubSample.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubSample.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubSample.class.getName()).getRefResolverClass()),

    //    SubstitutionModification        ("SubstitutionModification",        false, "/MzIdentML/SequenceCollection/Peptide/SubstitutionModification",                                                                                false, false, SubstitutionModification.class,       null,                                       null,                                           false,  null),
    SubstitutionModification(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubstitutionModification.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubstitutionModification.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubstitutionModification.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubstitutionModification.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubstitutionModification.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubstitutionModification.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubstitutionModification.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubstitutionModification.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubstitutionModification.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.SubstitutionModification.class.getName()).getRefResolverClass()),

    //    Tolerance                       (null,                              false, null, /* multiple tag names */                                                                                                                   false, false, Tolerance.class,                      ToleranceCvParam.class,                     null,                                           false,  null),
    Tolerance(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Tolerance.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Tolerance.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Tolerance.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Tolerance.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Tolerance.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Tolerance.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Tolerance.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Tolerance.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Tolerance.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.Tolerance.class.getName()).getRefResolverClass()),

    //    TranslationTable                ("TranslationTable",                true,  "/MzIdentML/AnalysisProtocolCollection/SpectrumIdentificationProtocol/DatabaseTranslation/TranslationTable",                                     false, true,  TranslationTable.class,               TranslationTableCvParam.class,              null,                                           false,  null),
    TranslationTable(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.TranslationTable.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.TranslationTable.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.TranslationTable.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.TranslationTable.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.TranslationTable.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.TranslationTable.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.TranslationTable.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.TranslationTable.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.TranslationTable.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.TranslationTable.class.getName()).getRefResolverClass()),

    //    UserParam                       ("userParam",                       false, null, /* multiple locations */                                                                                                                   false, false, UserParam.class,                      null,                                       null,                                           false,  null);
    UserParam(getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.UserParam.class.getName()).getTagName(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.UserParam.class.getName()).isIndexed(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.UserParam.class.getName()).getXpath(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.UserParam.class.getName()).isCached(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.UserParam.class.getName()).isIdMapped(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.UserParam.class.getName()).getClazz(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.UserParam.class.getName()).getCvParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.UserParam.class.getName()).getUserParamClass(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.UserParam.class.getName()).isAutoRefResolving(),
            getCfg().get(uk.ac.ebi.jmzidml.model.mzidml.UserParam.class.getName()).getRefResolverClass());

    private String tagName;
    private boolean indexed;
    private String xpath;
    private boolean cached;
    private boolean idMapped;
    private Class clazz;
    private Class cvParamClass;
    private Class userParamClass;
    private boolean autoRefResolving;
    private Class refResolverClass;

    /**
     * This should be called first in order to retrieve configuration from a file and populate cfgMap.
     */
    private <T extends MzIdentMLObject> MzIdentMLElement() {
        loadProperties();
    }

    private <T extends MzIdentMLObject> MzIdentMLElement(String tagName,
                                                         boolean indexed,
                                                         String xpath,
                                                         boolean cached,
                                                         boolean idMapped,
                                                         Class<T> clazz,
                                                         Class cvParamClass,
                                                         Class userParamClass,
                                                         boolean autoRefResolving,
                                                         Class refResolverClass) {

        this.tagName = tagName;
        this.indexed = indexed;
        this.cached = cached; // currently not used!
        this.xpath = xpath;
        this.idMapped = idMapped;
        this.clazz = clazz;
        this.cvParamClass = cvParamClass;
        this.userParamClass = userParamClass;
        this.autoRefResolving = autoRefResolving;
        this.refResolverClass = refResolverClass;
    }

    public String getTagName() {
        return tagName;
    }

    public boolean isIndexed() {
        return indexed;
    }

    public boolean isCached() {
        return cached;
    }

    public boolean isIdMapped() {
        return idMapped;
    }

    public boolean isAutoRefResolving() {
        return autoRefResolving;
    }

    public String getXpath() {
        return xpath;
    }

    @SuppressWarnings("unchecked")
    public <T extends MzIdentMLObject> Class<T> getClazz() {
        return clazz;
    }

    @SuppressWarnings("unchecked")
    public <C extends CvParam> Class<C> getCvParamClass() {
        return cvParamClass;
    }

    @SuppressWarnings("unchecked")
    public <U extends UserParam> Class<U> getUserParamClass() {
        return userParamClass;
    }

    @SuppressWarnings("unchecked")
    public <R extends AbstractReferenceResolver> Class<R> getRefResolverClass() {
        return refResolverClass;
    }

    public static MzIdentMLElement getType(Class clazz) {
        for (MzIdentMLElement type : MzIdentMLElement.values()) {
            if (type.getClazz() == clazz) {
                return type;
            }
        }
        return null;
    }

    public static MzIdentMLElement getType(String xpath) {
        for (MzIdentMLElement type : MzIdentMLElement.values()) {
            if (type.getXpath() != null && type.getXpath().equals(xpath)) {
                return type;
            }
        }
        return null;
    }

    private static Map<String, MzIdentMLElementConfig> cfgMap;

    private static Map<String, MzIdentMLElementConfig> getCfg() {
        if (cfgMap == null) {
            cfgMap = new HashMap<String, MzIdentMLElementConfig>();
        }
        return cfgMap;
    }

    /**
     * Read the configuration info from the properties file. Note: this simply loads the information into a hashmap.
     * Actual setting of values is done through the constructors.
     */
    public static void loadProperties() {

        Logger logger = Logger.getLogger(MzIdentMLElement.class);

        //check to see if we have a project-specific configuration file
        URL xmlFileURL = MzIdentMLElement.class.getClassLoader().getResource("MzIdentMLElement.cfg.xml");
        //if not, use default config
        if (xmlFileURL == null) {
            xmlFileURL = MzIdentMLElement.class.getClassLoader().getResource("defaultMzIdentMLElement.cfg.xml");
        }
        logger.warn("MzIdentML Configuration file: " + xmlFileURL.toString());

        MzIdentMLElementProperties props = JAXB.unmarshal(xmlFileURL, MzIdentMLElementProperties.class);
        Map<String, MzIdentMLElementConfig> localCfg = getCfg();
        for (MzIdentMLElementConfig cfg : props.getConfigurations()) {
            Class clazz = cfg.getClazz();
            if (clazz != null) {
                localCfg.put(clazz.getName(), cfg);
            }
        }
    }

    @Override
    public String toString() {
        return "MzIdentMLElement{" +
                "indexed=" + indexed +
                ", xpath='" + xpath + '\'' +
                ", cached=" + cached +
                ", clazz=" + clazz +
                '}';
    }
}