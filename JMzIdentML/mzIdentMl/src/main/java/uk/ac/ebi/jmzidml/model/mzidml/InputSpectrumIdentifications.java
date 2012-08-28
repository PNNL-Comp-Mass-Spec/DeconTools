
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import javax.xml.bind.annotation.*;

import uk.ac.ebi.jmzidml.model.MzIdentMLObject;


/**
 * The lists of spectrum identifications that are input to the protein detection process. 
 *
 * TODO marshalling/ persistor add validation to check for case where someone gets SpectrumIdentificationList and changes its id without updating ref id in
 * InputSpectrumIdentifications and other such classes.
 * <p/>
 * NOTE: There is no setter method for the spectrumIdentificationListRef. This simplifies keeping the measure object reference and
 * spectrumIdentificationListRef synchronized.
 *
 * <p>Java class for InputSpectrumIdentificationsType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="InputSpectrumIdentificationsType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;attribute name="spectrumIdentificationList_ref" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "InputSpectrumIdentificationsType")
public class InputSpectrumIdentifications
    extends MzIdentMLObject
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlAttribute(name = "spectrumIdentificationList_ref", required = true)
    protected String spectrumIdentificationListRef;
    @XmlTransient
    protected SpectrumIdentificationList spectrumIdentificationList;

    public SpectrumIdentificationList getSpectrumIdentificationList() {
        return spectrumIdentificationList;
    }

    public void setSpectrumIdentificationList(SpectrumIdentificationList spectrumIdentificationList) {
        if (spectrumIdentificationList == null) {
            this.spectrumIdentificationListRef = null;
        } else {
            String refId = spectrumIdentificationList.getId();
            if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
            this.spectrumIdentificationListRef = refId;
        }
        this.spectrumIdentificationList = spectrumIdentificationList;
    }

    /**
     * Gets the value of the spectrumIdentificationListRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSpectrumIdentificationListRef() {
        return spectrumIdentificationListRef;
    }
}
