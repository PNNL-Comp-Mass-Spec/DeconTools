package uk.ac.ebi.jmzidml.model;

import uk.ac.ebi.jmzidml.model.mzidml.CvParam;
import uk.ac.ebi.jmzidml.model.mzidml.Identifiable;
import uk.ac.ebi.jmzidml.model.mzidml.UserParam;

import javax.xml.bind.annotation.XmlTransient;
import java.util.ArrayList;
import java.util.List;

/**
 * @author Florian Reisinger
 *         Date: 09-Nov-2010
 * @since 1.0
 */
public abstract class AbstractIdentifiableParamGroup extends Identifiable implements ParamGroupCapable {

    @XmlTransient
    private List<CvParam> cvParams;
    @XmlTransient
    private List<UserParam> userParams;



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


}
