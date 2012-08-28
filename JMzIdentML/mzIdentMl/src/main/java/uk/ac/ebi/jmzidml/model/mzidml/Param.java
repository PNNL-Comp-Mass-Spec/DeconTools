
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlElements;
import javax.xml.bind.annotation.XmlType;
import uk.ac.ebi.jmzidml.model.MzIdentMLObject;


/**
 * Helper type to allow either a cvParam or a userParam to be provided for an element.
 * 
 * <p>Java class for ParamType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ParamType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;group ref="{http://psidev.info/psi/pi/mzIdentML/1.1}ParamGroup"/>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ParamType", propOrder = {
    "paramGroup"
})
public class Param
    extends MzIdentMLObject
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlElements({
        @XmlElement(name = "userParam", type = UserParam.class),
        @XmlElement(name = "cvParam", type = CvParam.class)
    })
    protected AbstractParam paramGroup;


    /**
     * Sets the value of the paramGroup property.
     * 
     * @param value
     *     allowed object is
     *     {@link UserParam }
     *     {@link CvParam }
     *     
     */
    public void setParam(AbstractParam value) {
        this.paramGroup = value;
    }

    public CvParam getCvParam() {
        if (paramGroup instanceof CvParam) {
            return (CvParam) paramGroup;
        } else {
            return null;
        }
    }

    public UserParam getUserParam() {
        if (paramGroup instanceof UserParam) {
            return (UserParam) paramGroup;
        } else {
            return null;
        }
    }
}
