package uk.ac.ebi.jmzidml.model;

import uk.ac.ebi.jmzidml.model.mzidml.CvParam;
import uk.ac.ebi.jmzidml.model.mzidml.UserParam;

import javax.xml.bind.annotation.XmlTransient;
import java.util.ArrayList;
import java.util.List;

/**
 *
 * @author Florian Reisinger
 *         Date: 08-Nov-2010
 * @since 1.0
 */
public abstract class AbstractParamGroup implements ParamGroupCapable {

    // ToDo: ?? perhaps solve in getter methods (without storing in separate lists) ??
    @XmlTransient
    private List<CvParam> cvParams;
    @XmlTransient
    private List<UserParam> userParams;

    @XmlTransient
    private Long hid;


    public List<CvParam> getCvParam() {
        if (cvParams == null) {
            cvParams = new ArrayList<CvParam>();
        }
        return cvParams;
    }

    public List<UserParam> getUserParam() {
        if (userParams == null) {
            userParams = new ArrayList<UserParam>();
        }
        return userParams;
    }

    public void splitParamList() {
        if (getCvParam() == null || getCvParam().size() != 0) {
            throw new IllegalStateException("Error in initialisation. List of CvParam objects should be not null and empty in afterUnmarshal operation!");
        }
        if (getUserParam() == null || getUserParam().size() != 0) {
            throw new IllegalStateException("Error in initialisation. List of UserParam objects should be not null and empty in afterUnmarshal operation!");
        }
/*
        for (Param param : getParamGroup()) {
            if (param.getCvParamOrUserParam() instanceof CvParam) {
                getCvParam().add((CvParam) param.getCvParamOrUserParam());
            }
            if (param.getCvParamOrUserParam() instanceof UserParam) {
                getUserParam().add((UserParam) param.getCvParamOrUserParam());
            }
        }
*/
    }

    // ToDo: document that the List<Param> is cleared on marshalling!
    public void updateParamList() {
//        // whatever we had in the List of Params, we only
//        // consider what is in the CvParam/UserParam lists now.
//        getParamGroup().clear();
//        // combine the List<CvParam> and List<UserParam> in the one List<Param> that will be marshalled.
//        for (CvParam cvParam : getCvParam()) {
//            getParamGroup().add(cvParam);
//        }
//        for (UserParam userParam : getUserParam()) {
//            getParamGroup().add(userParam);
//        }
    }

}
