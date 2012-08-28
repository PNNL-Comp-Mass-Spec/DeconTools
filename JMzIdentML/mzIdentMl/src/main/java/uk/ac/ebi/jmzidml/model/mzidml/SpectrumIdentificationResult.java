
package uk.ac.ebi.jmzidml.model.mzidml;

import uk.ac.ebi.jmzidml.model.ParamGroupCapable;
import uk.ac.ebi.jmzidml.model.utils.FacadeList;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.*;


/**
 * All identifications made from searching one spectrum. For PMF data, all peptide identifications will be listed underneath as SpectrumIdentificationItems. For MS/MS data, there will be ranked SpectrumIdentificationItems corresponding to possible different peptide IDs.
 * 
 * <p>Java class for SpectrumIdentificationResultType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="SpectrumIdentificationResultType">
 *   &lt;complexContent>
 *     &lt;extension base="{http://psidev.info/psi/pi/mzIdentML/1.1}IdentifiableType">
 *       &lt;sequence>
 *         &lt;element name="SpectrumIdentificationItem" type="{http://psidev.info/psi/pi/mzIdentML/1.1}SpectrumIdentificationItemType" maxOccurs="unbounded"/>
 *         &lt;group ref="{http://psidev.info/psi/pi/mzIdentML/1.1}ParamGroup" maxOccurs="unbounded" minOccurs="0"/>
 *       &lt;/sequence>
 *       &lt;attribute name="spectrumID" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="spectraData_ref" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "SpectrumIdentificationResultType", propOrder = {
    "spectrumIdentificationItem",
    "paramGroup"
})
public class SpectrumIdentificationResult
    extends Identifiable
    implements Serializable, ParamGroupCapable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "SpectrumIdentificationItem", required = true)
    protected List<SpectrumIdentificationItem> spectrumIdentificationItem;
    @XmlElements({
        @XmlElement(name = "cvParam", type = CvParam.class),
        @XmlElement(name = "userParam", type = UserParam.class)
    })
    protected List<AbstractParam> paramGroup;
    @XmlAttribute(required = true)
    protected String spectrumID;
    @XmlAttribute(name = "spectraData_ref", required = true)
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
            this.spectraDataRef = refId;
        }
        this.spectraData = spectraData;
    }
    /**
     * Gets the value of the spectrumIdentificationItem property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the spectrumIdentificationItem property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getSpectrumIdentificationItem().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link SpectrumIdentificationItem }
     * 
     * 
     */
    public List<SpectrumIdentificationItem> getSpectrumIdentificationItem() {
        if (spectrumIdentificationItem == null) {
            spectrumIdentificationItem = new ArrayList<SpectrumIdentificationItem>();
        }
        return this.spectrumIdentificationItem;
    }

    /**
     *  Scores or parameters associated with the SpectrumIdentificationResult (i.e the set of SpectrumIdentificationItems derived from one spectrum) e.g. the number of peptide sequences within the parent tolerance for this spectrum. Gets the value of the paramGroup property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the paramGroup property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getParamGroup().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link CvParam }
     * {@link UserParam }
     * 
     * 
     */
    public List<AbstractParam> getParamGroup() {
        if (paramGroup == null) {
            paramGroup = new ArrayList<AbstractParam>();
        }
        return this.paramGroup;
    }

    /**
     * Gets the value of the spectrumID property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSpectrumID() {
        return spectrumID;
    }

    /**
     * Sets the value of the spectrumID property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSpectrumID(String value) {
        this.spectrumID = value;
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

    /**
     * Get the cvparams for spectrumidentificationresult
     * @return
     */
    public List<CvParam> getCvParam() {
        return new FacadeList<CvParam>(this.getParamGroup(), CvParam.class);
    }

    /**
     * Get the userparams for spectrumidentificationresult
     * @return
     */

    public List<UserParam> getUserParam() {
        return new FacadeList<UserParam>(this.getParamGroup(), UserParam.class);
    }
}
