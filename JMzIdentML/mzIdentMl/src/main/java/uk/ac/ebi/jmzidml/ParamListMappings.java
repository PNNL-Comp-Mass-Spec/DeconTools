package uk.ac.ebi.jmzidml;

import uk.ac.ebi.jmzidml.model.mzidml.*;

/**
 * Some classes contain one or more properties of type ParamList. This enum maps these classes to a list of class names corresponding
 * to the property names in these classes. For example, ProteinDetectionProtocol contains the ParamList properties 'threshold'
 * and 'analysisParams'. The mapping for this class is Threshold and AnalysisParams. These will be used with Reflection to call the
 * appropriate get methods on ProteinDetectionProtocol and to create the correct subclasses of CvParam and UserParam (see RawXMLListener.paramHandling).
 *
 * User: gokelly
 * Date: 11/04/11
 * Time: 10:35
 *
 */
public enum ParamListMappings{
    ProteinDetectionProtocol                 (new String[]{"Threshold", "AnalysisParams"}, ProteinDetectionProtocol.class),
    Filter                                   (new String[]{"Include", "Exclude"}, Filter.class),
    SpectrumIdentificationProtocol            (new String[]{"AdditionalSearchParams", "Threshold"}, SpectrumIdentificationProtocol.class),
    Enzyme                                   (new String[]{"EnzymeName"}, Enzyme.class);

    private Class clazz;
    private String[] classNames;

    private ParamListMappings(String[] classNames, Class clazz){
        this.classNames = classNames;
        this.clazz = clazz;
    }

    public String[] getClassNames(){
        return classNames;
    }

    public Class getClazz(){
        return this.clazz;
    }

    public static ParamListMappings getType(Class clazz){
        for (ParamListMappings type : ParamListMappings.values()) {
            if (type.getClazz() == clazz) {
                return type;
            }
        }
        return null;
    }
}
