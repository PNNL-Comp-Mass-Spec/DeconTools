
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.*;


/**
 * An Analysis which assembles a set of peptides (e.g. from a spectra search analysis) to proteins. 
 *
 * TODO marshalling/ persistor add validation to check for case where someone gets proteinDetectionList/proteinDetectionProtocol and changes its id without updating ref id in
 *      ProteinDetection and other such clases.
 *
 * NOTE: There is no setter method for the proteinDetectionListRef/proteinDetectionProtocolRef. This simplifies keeping the proteinDetectionList/proteinDetectionProtocol object reference and
 * proteinDetectionListRef/proteinDetectionProtocolRef synchronized.
 *
 * <p>Java class for ProteinDetectionType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ProteinDetectionType">
 *   &lt;complexContent>
 *     &lt;extension base="{http://psidev.info/psi/pi/mzIdentML/1.1}ProtocolApplicationType">
 *       &lt;sequence>
 *         &lt;element name="InputSpectrumIdentifications" type="{http://psidev.info/psi/pi/mzIdentML/1.1}InputSpectrumIdentificationsType" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *       &lt;attribute name="proteinDetectionList_ref" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="proteinDetectionProtocol_ref" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ProteinDetectionType", propOrder = {
    "inputSpectrumIdentifications"
})
public class ProteinDetection
    extends ProtocolApplication
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "InputSpectrumIdentifications", required = true)
    protected List<InputSpectrumIdentifications> inputSpectrumIdentifications;
    @XmlAttribute(name = "proteinDetectionList_ref", required = true)
    protected String proteinDetectionListRef;
    @XmlAttribute(name = "proteinDetectionProtocol_ref", required = true)
    protected String proteinDetectionProtocolRef;
    @XmlTransient
    protected ProteinDetectionList proteinDetectionList;
    @XmlTransient
    protected ProteinDetectionProtocol proteinDetectionProtocol;


    public ProteinDetectionList getProteinDetectionList() {
        return proteinDetectionList;
    }

    public void setProteinDetectionList(ProteinDetectionList proteinDetectionList) {
        if (proteinDetectionList == null) {
            this.proteinDetectionListRef = null;
        } else {
            String refId = proteinDetectionList.getId();
            if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
            this.proteinDetectionListRef = refId;
        }
        this.proteinDetectionList = proteinDetectionList;
    }

    public ProteinDetectionProtocol getProteinDetectionProtocol() {
        return proteinDetectionProtocol;
    }

    public void setProteinDetectionProtocol(ProteinDetectionProtocol proteinDetectionProtocol) {
        if (proteinDetectionProtocol == null) {
            this.proteinDetectionProtocolRef = null;
        } else {
            String refId = proteinDetectionProtocol.getId();
            if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
            this.proteinDetectionProtocolRef = refId;
        }
        this.proteinDetectionProtocol = proteinDetectionProtocol;
    }


    /**
     * Gets the value of the inputSpectrumIdentifications property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the inputSpectrumIdentifications property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getInputSpectrumIdentifications().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link InputSpectrumIdentifications }
     * 
     * 
     */
    public List<InputSpectrumIdentifications> getInputSpectrumIdentifications() {
        if (inputSpectrumIdentifications == null) {
            inputSpectrumIdentifications = new ArrayList<InputSpectrumIdentifications>();
        }
        return this.inputSpectrumIdentifications;
    }

    /**
     * Gets the value of the proteinDetectionListRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getProteinDetectionListRef() {
        return proteinDetectionListRef;
    }

    /**
     * Gets the value of the proteinDetectionProtocolRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getProteinDetectionProtocolRef() {
        return proteinDetectionProtocolRef;
    }
}
