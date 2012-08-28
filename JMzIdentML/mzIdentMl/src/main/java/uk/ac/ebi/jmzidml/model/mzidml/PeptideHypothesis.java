
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.*;

import uk.ac.ebi.jmzidml.model.MzIdentMLObject;


/**
 * Peptide evidence on which this ProteinHypothesis is based by reference to a PeptideEvidence element. 
 *
 * TODO marshalling/ persistor add validation to check for case where someone gets peptideEvidence and changes its id without updating ref id in
 *      PeptideHypothesis and other such classes.
 *
 * NOTE: There is no setter method for the peptideEvidenceRef. This simplifies keeping the peptideEvidence object reference and
 * peptideEvidenceRef synchronized.
 *
 * <p>Java class for PeptideHypothesisType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="PeptideHypothesisType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="SpectrumIdentificationItemRef" type="{http://psidev.info/psi/pi/mzIdentML/1.1}SpectrumIdentificationItemRefType" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *       &lt;attribute name="peptideEvidence_ref" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "PeptideHypothesisType", propOrder = {
    "spectrumIdentificationItemRef"
})
public class PeptideHypothesis
    extends MzIdentMLObject
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "SpectrumIdentificationItemRef", required = true)
    protected List<SpectrumIdentificationItemRef> spectrumIdentificationItemRef;
    @XmlAttribute(name = "peptideEvidence_ref", required = true)
    protected String peptideEvidenceRef;
    @XmlTransient
    protected PeptideEvidence peptideEvidence;

    /**
     * Gets the value of the spectrumIdentificationItemRef property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the spectrumIdentificationItemRef property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getSpectrumIdentificationItemRef().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link SpectrumIdentificationItemRef }
     * 
     * 
     */
    public List<SpectrumIdentificationItemRef> getSpectrumIdentificationItemRef() {
        if (spectrumIdentificationItemRef == null) {
            spectrumIdentificationItemRef = new ArrayList<SpectrumIdentificationItemRef>();
        }
        return this.spectrumIdentificationItemRef;
    }

    /**
     * Gets the value of the peptideEvidenceRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getPeptideEvidenceRef() {
        return peptideEvidenceRef;
    }

    public PeptideEvidence getPeptideEvidence() {
        return peptideEvidence;
    }

    public void setPeptideEvidence(PeptideEvidence peptideEvidence) {
        if (peptideEvidence == null) {
            this.peptideEvidenceRef = null;
        } else {
            String refId = peptideEvidence.getId();
            if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
            this.peptideEvidenceRef = refId;
        }
        this.peptideEvidence = peptideEvidence;
    }
}
