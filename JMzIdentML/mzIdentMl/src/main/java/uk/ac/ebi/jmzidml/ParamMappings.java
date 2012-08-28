package uk.ac.ebi.jmzidml;

import uk.ac.ebi.jmzidml.model.mzidml.SearchDatabase;
import uk.ac.ebi.jmzidml.model.mzidml.AnalysisSoftware;
import uk.ac.ebi.jmzidml.model.mzidml.Filter;
import uk.ac.ebi.jmzidml.model.mzidml.SpectrumIdentificationProtocol;

/**
 * User: gokelly
 * Date: 4/14/11
 * Time: 1:56 PM
 */
public enum ParamMappings {
    AnalysisSearchDatabase                 ("DatabaseName" , SearchDatabase.class),
    AnalysisSoftware                       ("SoftwareName", AnalysisSoftware.class),
    Filter                                  ("FilterType", Filter.class),
    SpectrumIdentificationProtocol          ("SearchType", SpectrumIdentificationProtocol.class);


    private Class clazz;
    private String className;

    private ParamMappings(String className, Class clazz){
        this.className = className;
        this.clazz = clazz;
    }

    public String getClassName(){
        return className;
    }

    public Class getClazz(){
        return this.clazz;
    }

    public static ParamMappings getType(Class clazz){
        for (ParamMappings type : ParamMappings.values()) {
            if (type.getClazz() == clazz) {
                return type;
            }
        }
        return null;
    }

}
