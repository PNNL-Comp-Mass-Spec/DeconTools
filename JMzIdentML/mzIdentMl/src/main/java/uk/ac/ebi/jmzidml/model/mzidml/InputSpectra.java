
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import javax.xml.bind.annotation.*;

import uk.ac.ebi.jmzidml.model.MzIdentMLObject;


/**
 * The attribute referencing an identifier within the SpectraData section. 
 *
 * TODO marshalling/ persistor add validation to check for case where someone gets spectraData and changes its id without updating ref id in
 *      InputSpectra and other such classes.
 *
 * NOTE: There is no setter method for the spectraDataRef. This simplifies keeping the measure object reference and
 * spectraDataRef synchronized.
 *
 * <p>Java class for InputSpectraType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="InputSpectraType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;attribute name="spectraData_ref" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "InputSpectraType")
public class InputSpectra
    extends MzIdentMLObject
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlAttribute(name = "spectraData_ref")
    protected String spectraDataRef;
    @XmlTransient
    protected SpectraData spectraData;

    public SpectraData getSpectraData() {
        return spectraData;
    }

    public void setSpectraData(SpectraData spectraData) {
        if (spectraData == null) {
            this.spectraDataRef = null;
        } else {
            String refId = spectraData.getId();
            if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
            this.spectraDataRef= refId;
        }
        this.spectraData = spectraData;
    }
    /**
     * Gets the value of the spectraDataRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSpectraDataRef() {
        return spectraDataRef;
    }
}
