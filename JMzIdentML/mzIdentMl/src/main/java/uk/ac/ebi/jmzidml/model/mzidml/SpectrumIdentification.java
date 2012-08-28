
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.*;


/**
 * An Analysis which tries to identify peptides in input spectra, referencing the database searched, the input spectra, the output results and the protocol that is run. 
 *
 * TODO marshalling/ persistor add validation to check for case where someone gets spectrumIdentificationList/spectrumIdentificationProtocol and changes its id without updating ref id in
 * SpectrumIdentification and other such clases.
 * <p/>
 * NOTE: There is no setter method for the spectrumIdentificationListRef/spectrumIdentificationProtocolRef. This simplifies keeping the spectrumIdentificationList/spectrumIdentificationProtocol object reference and
 * spectrumIdentificationListRef/spectrumIdentificationProtocolRef synchronized.
 * <p/>
 *
 * <p>Java class for SpectrumIdentificationType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="SpectrumIdentificationType">
 *   &lt;complexContent>
 *     &lt;extension base="{http://psidev.info/psi/pi/mzIdentML/1.1}ProtocolApplicationType">
 *       &lt;sequence>
 *         &lt;element name="InputSpectra" type="{http://psidev.info/psi/pi/mzIdentML/1.1}InputSpectraType" maxOccurs="unbounded"/>
 *         &lt;element name="SearchDatabaseRef" type="{http://psidev.info/psi/pi/mzIdentML/1.1}SearchDatabaseRefType" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *       &lt;attribute name="spectrumIdentificationProtocol_ref" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="spectrumIdentificationList_ref" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "SpectrumIdentificationType", propOrder = {
    "inputSpectra",
    "searchDatabaseRef"
})
public class SpectrumIdentification
    extends ProtocolApplication
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "InputSpectra", required = true)
    protected List<InputSpectra> inputSpectra;
    @XmlElement(name = "SearchDatabaseRef", required = true)
    protected List<SearchDatabaseRef> searchDatabaseRef;
    @XmlAttribute(name = "spectrumIdentificationProtocol_ref", required = true)
    protected String spectrumIdentificationProtocolRef;
    @XmlAttribute(name = "spectrumIdentificationList_ref", required = true)
    protected String spectrumIdentificationListRef;

    @XmlTransient
    protected SpectrumIdentificationList spectrumIdentificationList;
    @XmlTransient
    protected SpectrumIdentificationProtocol spectrumIdentificationProtocol;


    public SpectrumIdentificationProtocol getSpectrumIdentificationProtocol() {
        return spectrumIdentificationProtocol;
    }

    public void setSpectrumIdentificationProtocol(SpectrumIdentificationProtocol spectrumIdentificationProtocol) {
        if (spectrumIdentificationProtocol == null) {
            this.spectrumIdentificationProtocolRef = null;
        } else {
            String refId = spectrumIdentificationProtocol.getId();
            if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
            this.spectrumIdentificationProtocolRef = refId;
        }
        this.spectrumIdentificationProtocol = spectrumIdentificationProtocol;
    }

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
     * Gets the value of the inputSpectra property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the inputSpectra property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getInputSpectra().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link InputSpectra }
     * 
     * 
     */
    public List<InputSpectra> getInputSpectra() {
        if (inputSpectra == null) {
            inputSpectra = new ArrayList<InputSpectra>();
        }
        return this.inputSpectra;
    }

    /**
     * Gets the value of the searchDatabaseRef property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the searchDatabaseRef property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getSearchDatabaseRef().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link SearchDatabaseRef }
     * 
     * 
     */
    public List<SearchDatabaseRef> getSearchDatabaseRef() {
        if (searchDatabaseRef == null) {
            searchDatabaseRef = new ArrayList<SearchDatabaseRef>();
        }
        return this.searchDatabaseRef;
    }

    /**
     * Gets the value of the spectrumIdentificationProtocolRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSpectrumIdentificationProtocolRef() {
        return spectrumIdentificationProtocolRef;
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
