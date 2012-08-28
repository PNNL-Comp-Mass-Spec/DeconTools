package uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.listeners;

import org.apache.log4j.Logger;
import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.ParamListMappings;
import uk.ac.ebi.jmzidml.ParamMappings;
import uk.ac.ebi.jmzidml.model.*;
import uk.ac.ebi.jmzidml.model.mzidml.CvParam;
import uk.ac.ebi.jmzidml.model.mzidml.Param;
import uk.ac.ebi.jmzidml.model.mzidml.ParamList;
import uk.ac.ebi.jmzidml.model.mzidml.UserParam;
import uk.ac.ebi.jmzidml.model.utils.ParamUpdater;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLObjectCache;
import uk.ac.ebi.jmzidml.xml.jaxb.resolver.AbstractReferenceResolver;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

import javax.xml.bind.Unmarshaller;
import java.lang.reflect.Constructor;
import java.lang.reflect.Method;

/**
 * Listener to handle class specific post processing steps during unmarshalling.
 * Dependent on the type of object we are dealing with (e.g. if it has only one or more CvParam,
 * if it has one or more Param, which in turn can be either CvParam or UserParam) it will
 * split the Param into CvParam and UserParam and subclass those.
 * This is partial for convenience on the API level, but mainly for convenience on the data
 * persistence level (since objects are persisted to tables according to there classes).
 *
 * @author Florian Reisinger
 *         Date: 21-Sep-2010
 * @since 0.1
 */
public class RawXMLListener extends Unmarshaller.Listener {

    private static final Logger log = Logger.getLogger(RawXMLListener.class);
    private final MzIdentMLIndexer index;
    private final MzIdentMLObjectCache cache;

    public RawXMLListener(MzIdentMLIndexer index, MzIdentMLObjectCache cache) {
        this.index = index;
        this.cache = cache;
    }

    @Override
    public void afterUnmarshal(Object target, Object parent) {

        log.debug("Handling " + target.getClass() + " in afterUnmarshal.");
        // retrieve the enum type for this class (for the meta data about this class/element)
        MzIdentMLElement ele = MzIdentMLElement.getType(target.getClass());

        // splitting of Param into CvParam/UserParam AND sub-classing
        paramHandling(target, ele);

        // now perform the automatic reference resolving, if configured to do so
        referenceResolving(target, parent, ele);

        // caching
        // ToDo: best place for caching is here, since all elements are handled in the afterUnmarshal method
        // ToDo: however it is VERY tricky to replace the 'target' object with the version from the cache should there be one
//        if (ele.isCached() && target instanceof IdentifiableMzIdentMLObject) {
//            IdentifiableMzIdentMLObject object = (IdentifiableMzIdentMLObject) target;
//            IdentifiableMzIdentMLObject cachedObject = cache.getCachedObject(object.getId(), object.getClass());
//            if (cachedObject == null) {
//                cache.putInCache(object);
//            }
//        }


    }

    @SuppressWarnings("unchecked")
    private void paramHandling(Object target, MzIdentMLElement ele) {
        // (due to possible exceptions while sub-classing in try/catch)
        try {

            if(target instanceof ParamCapable){
                ParamMappings mapping = ParamMappings.getType(target.getClass());
                String className = mapping.getClassName();
                Method method = target.getClass().getMethod("get" + className);
                Param param  = (Param)method.invoke(target);
                if(param != null){

                    /**
                     * Use the retrieved class name to determine the correct subclasses of CvParam and UserParam to use.
                     */
                    if(param.getCvParam() != null){
                        Class clazz = Class.forName("uk.ac.ebi.jmzidml.model.mzidml.params." + className + "CvParam");
                        CvParam cvParam = ParamUpdater.updateCvParamSubclass(param.getCvParam(), clazz);
                        param.setParam(cvParam);
                    }else if(param.getUserParam() != null){
                        Class clazz = Class.forName("uk.ac.ebi.jmzidml.model.mzidml.params." + className + "UserParam");
                        UserParam userParam = ParamUpdater.updateUserParamSubclass(param.getUserParam(), clazz);
                        param.setParam(userParam);
                    }

                }

            }

             // now we check what kind of object we are dealing with
            // NOTE: the order of the if statements is IMPORTANT!
            // (every AbstractParamGroup is a CvParamCapable, but not vice versa)
            if(target instanceof ParamListCapable){
                ParamListMappings mappings = ParamListMappings.getType(target.getClass());
                String[] classNames = mappings.getClassNames();
                for(String className: classNames){
                    /**
                     * Use the retrieved class name to dynamically call the appropriate get method in the class implementing
                     * ParamListCapable
                     */
                    Method method = target.getClass().getMethod("get" + className);
                    ParamList paramList = (ParamList)method.invoke(target);
                    if(paramList != null){
                        /**
                         * Use the retrieved class name to determine the correct subclasses of CvParam and UserParam to use.
                         */
                        Class clazz = Class.forName("uk.ac.ebi.jmzidml.model.mzidml.params." + className + "CvParam");
                        ParamUpdater.updateCvParamSubclassesList(paramList.getCvParam(), clazz);
                        clazz = Class.forName("uk.ac.ebi.jmzidml.model.mzidml.params." + className + "UserParam");
                        ParamUpdater.updateUserParamSubclassesList(paramList.getUserParam(), clazz);
                    }
                }
            }else if (target instanceof ParamGroupCapable) {
                // in this case we not only have to subclass the params, but also to split them up
                ParamGroupCapable apg = (ParamGroupCapable) target;
                // first we are going to split the List<Param> in a List<CvParam> and a List<UserParam>
            //    apg.splitParamList();
                // then we are going to subclass the params

                if (ele.getCvParamClass() == null) {
                    throw new IllegalStateException("Subclass of AbstractParamGroup does not have CvParam subclass! " + target.getClass());
                }
                ParamUpdater.updateCvParamSubclassesList(apg.getCvParam(), ele.getCvParamClass());
                if (ele.getUserParamClass() == null) {
                    throw new IllegalStateException("Subclass of AbstractParamGroup does not have UserParam subclass! " + target.getClass());
                }
                ParamUpdater.updateUserParamSubclassesList(apg.getUserParam(), ele.getUserParamClass());
            } else if (target instanceof CvParamCapable) {
                // no need to split up params, but we need to subclass them
                CvParamCapable cpc = (CvParamCapable) target;
                if (ele.getCvParamClass() == null) {
                    throw new IllegalStateException("Subclass of AbstractParamGroup does not have CvParam subclass! " + target.getClass());
                }
                CvParam param = cpc.getCvParam();
                cpc.setCvParam(ParamUpdater.updateCvParamSubclass(param, ele.<CvParam>getCvParamClass()));
            } else if (target instanceof CvParamListCapable) {
                CvParamListCapable cpc = (CvParamListCapable) target;
                if (ele.getCvParamClass() == null) {
                    throw new IllegalStateException("Subclass of AbstractParamGroup does not have CvParam subclass! " + target.getClass());
                }
                ParamUpdater.updateCvParamSubclassesList(cpc.getCvParam(), ele.getCvParamClass());
            } else {
                // no need to split or subclass params
                if (ele.getCvParamClass() != null || ele.getUserParamClass() != null) {
                    throw new IllegalStateException("Element with param subclasses has not been handled! " + target.getClass());
                }
            }

            // !! ISSUES !! Exceptions !!
            // classes that can not extend AbstractParamGroup (because they already extend some other class)
            // DBSequence                      ConceptualMolecule -> Identifiable       solved in ConceptualMolecule extending AbstractIdentifiableParamGroup
            // Peptide                         ConceptualMolecule -> Identifiable       solved in ConceptualMolecule extending AbstractIdentifiableParamGroup
            // MassTable                       Identifiable                             solved extending AbstractIdentifiableParamGroup
            // PeptideEvidence                 Identifiable                             solved extending AbstractIdentifiableParamGroup
            // ProteinAmbiguityGroup           Identifiable                             solved extending AbstractIdentifiableParamGroup
            // ProteinDetectionHypothesis      Identifiable                             solved extending AbstractIdentifiableParamGroup
            // SpectrumIdentificationItem      Identifiable                             solved extending AbstractIdentifiableParamGroup
            // SpectrumIdentificationResult    Identifiable                             solved extending AbstractIdentifiableParamGroup
            // Sample                          Material -> Identifiable                 solved in Material extending AbstractIdentifiableParamGroup
            // SourceFile                      ExternalData -> Data -> Identifiable     solved in SourceFile (code duplication)
            // SpectrumIdentificationList      InternalData -> Data -> Identifiable     solved in InternalData (code duplication)
            // ProteinDetectionList            InternalData -> Data -> Identifiable     solved in InternalData (code duplication)

        } catch (Exception e) {
            log.error("Exception during post unmarshall processing! ", e);
            throw new IllegalStateException("Error during post unmarshall processing!", e);
        }
    }

    private void referenceResolving(Object target, Object parent, MzIdentMLElement ele) {
        if (ele.isAutoRefResolving()) {
            Class cls = ele.getRefResolverClass();
            if (cls == null) {
                throw new IllegalStateException("Can not auto-resolve references if no reference resolver was defined for class: " + ele.getClazz());
            }
            try {
                Constructor con = cls.getDeclaredConstructor(MzIdentMLIndexer.class, MzIdentMLObjectCache.class);
                AbstractReferenceResolver resolver = (AbstractReferenceResolver) con.newInstance(index, cache);
                resolver.afterUnmarshal(target, parent);
            } catch (Exception e) {
                log.error("Error trying to instantiate reference resolver: " + cls.getName(), e);
                throw new IllegalStateException("Could not instantiate reference resolver: " + cls.getName());
            }
        }
    }

}