
package uk.ac.ebi.jmzidml.model.mzidml;

import uk.ac.ebi.jmzidml.model.ParamGroupCapable;
import uk.ac.ebi.jmzidml.model.utils.FacadeList;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.*;


/**
 * An identification of a single (poly)peptide, resulting from querying an input spectra, along with the set of confidence values for that identification.
 * PeptideEvidence elements should be given for all mappings of the corresponding Peptide sequence within protein sequences. 
 *
 * TODO marshalling/ persistor add validation to check for case where someone gets peptide/massTable/sample and changes its id without updating ref id in
 *      SpectrumIdentificationItem and other such clases.
 *
 * NOTE: There is no setter method for the peptideRef/massTableRef/sampleRef. This simplifies keeping the peptide/massTable/sample object reference and
 * peptideRef/massTableRef/sampleRef synchronized.
 *
 * TODO: write an adaptor for changing List<PeptideEvidenceRef> to List<String>
 *
 * <p>Java class for SpectrumIdentificationItemType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="SpectrumIdentificationItemType">
 *   &lt;complexContent>
 *     &lt;extension base="{http://psidev.info/psi/pi/mzIdentML/1.1}IdentifiableType">
 *       &lt;sequence>
 *         &lt;element name="PeptideEvidenceRef" type="{http://psidev.info/psi/pi/mzIdentML/1.1}PeptideEvidenceRefType" maxOccurs="unbounded"/>
 *         &lt;element name="Fragmentation" type="{http://psidev.info/psi/pi/mzIdentML/1.1}FragmentationType" minOccurs="0"/>
 *         &lt;group ref="{http://psidev.info/psi/pi/mzIdentML/1.1}ParamGroup" maxOccurs="unbounded" minOccurs="0"/>
 *       &lt;/sequence>
 *       &lt;attribute name="chargeState" use="required" type="{http://www.w3.org/2001/XMLSchema}int" />
 *       &lt;attribute name="experimentalMassToCharge" use="required" type="{http://www.w3.org/2001/XMLSchema}double" />
 *       &lt;attribute name="calculatedMassToCharge" type="{http://www.w3.org/2001/XMLSchema}double" />
 *       &lt;attribute name="calculatedPI" type="{http://www.w3.org/2001/XMLSchema}float" />
 *       &lt;attribute name="peptide_ref" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="rank" use="required" type="{http://www.w3.org/2001/XMLSchema}int" />
 *       &lt;attribute name="passThreshold" use="required" type="{http://www.w3.org/2001/XMLSchema}boolean" />
 *       &lt;attribute name="massTable_ref" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="sample_ref" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "SpectrumIdentificationItemType", propOrder = {
    "peptideEvidenceRef",
    "fragmentation",
    "paramGroup"
})
public class SpectrumIdentificationItem
    extends Identifiable
    implements Serializable, ParamGroupCapable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "PeptideEvidenceRef", required = true)
    protected List<PeptideEvidenceRef> peptideEvidenceRef;
    @XmlElement(name = "Fragmentation")
    protected Fragmentation fragmentation;
    @XmlElements({
        @XmlElement(name = "cvParam", type = CvParam.class),
        @XmlElement(name = "userParam", type = UserParam.class)
    })
    protected List<AbstractParam> paramGroup;
    @XmlAttribute(required = true)
    protected int chargeState;
    @XmlAttribute(required = true)
    protected double experimentalMassToCharge;
    @XmlAttribute
    protected Double calculatedMassToCharge;
    @XmlAttribute
    protected Float calculatedPI;
    @XmlAttribute(name = "peptide_ref")
    protected String peptideRef;
    @XmlAttribute(required = true)
    protected int rank;
    @XmlAttribute(required = true)
    protected boolean passThreshold;
    @XmlAttribute(name = "massTable_ref")
    protected String massTableRef;
    @XmlAttribute(name = "sample_ref")
    protected String sampleRef;
    @XmlTransient
     protected Peptide peptide;
     @XmlTransient
     protected MassTable massTable;
     @XmlTransient
     protected Sample sample;

     public Peptide getPeptide() {
         return peptide;
     }

     public void setPeptide(Peptide peptide) {
         if (peptide == null) {
             this.peptideRef = null;
         } else {
             String refId = peptide.getId();
             if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
             this.peptideRef = refId;
         }
         this.peptide = peptide;
     }

     public MassTable getMassTable() {
         return massTable;
     }

     public void setMassTable(MassTable massTable) {
         if (massTable == null) {
             this.massTableRef = null;
         } else {
             String refId = massTable.getId();
             if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
             this.massTableRef = refId;
         }
         this.massTable = massTable;
     }

     public Sample getSample() {
         return sample;
     }

     public void setSample(Sample sample) {
         if (sample == null) {
             this.sampleRef = null;
         } else {
             String refId = sample.getId();
             if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
             this.sampleRef = refId;
         }
         this.sample = sample;
     }

    /**
     * Gets the value of the peptideEvidenceRef property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the peptideEvidenceRef property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getPeptideEvidenceRef().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link PeptideEvidenceRef }
     * 
     * 
     */
    public List<PeptideEvidenceRef> getPeptideEvidenceRef() {
        if (peptideEvidenceRef == null) {
            peptideEvidenceRef = new ArrayList<PeptideEvidenceRef>();
        }
        return this.peptideEvidenceRef;
    }

    /**
     * Gets the value of the fragmentation property.
     * 
     * @return
     *     possible object is
     *     {@link Fragmentation }
     *     
     */
    public Fragmentation getFragmentation() {
        if(fragmentation == null){
            this.fragmentation = new Fragmentation();
        }
        return fragmentation;
    }

    /**
     * Sets the value of the fragmentation property.
     * 
     * @param value
     *     allowed object is
     *     {@link Fragmentation }
     *     
     */
    public void setFragmentation(Fragmentation value) {
        this.fragmentation = value;
    }

    /**
     * Scores or attributes associated with the SpectrumIdentificationItem e.g. e-value, p-value, score.Gets the value of the paramGroup property.
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
     * Gets the value of the chargeState property.
     * 
     */
    public int getChargeState() {
        return chargeState;
    }

    /**
     * Sets the value of the chargeState property.
     * 
     */
    public void setChargeState(int value) {
        this.chargeState = value;
    }

    /**
     * Gets the value of the experimentalMassToCharge property.
     * 
     */
    public double getExperimentalMassToCharge() {
        return experimentalMassToCharge;
    }

    /**
     * Sets the value of the experimentalMassToCharge property.
     * 
     */
    public void setExperimentalMassToCharge(double value) {
        this.experimentalMassToCharge = value;
    }

    /**
     * Gets the value of the calculatedMassToCharge property.
     * 
     * @return
     *     possible object is
     *     {@link Double }
     *     
     */
    public Double getCalculatedMassToCharge() {
        return calculatedMassToCharge;
    }

    /**
     * Sets the value of the calculatedMassToCharge property.
     * 
     * @param value
     *     allowed object is
     *     {@link Double }
     *     
     */
    public void setCalculatedMassToCharge(Double value) {
        this.calculatedMassToCharge = value;
    }

    /**
     * Gets the value of the calculatedPI property.
     * 
     * @return
     *     possible object is
     *     {@link Float }
     *     
     */
    public Float getCalculatedPI() {
        return calculatedPI;
    }

    /**
     * Sets the value of the calculatedPI property.
     * 
     * @param value
     *     allowed object is
     *     {@link Float }
     *     
     */
    public void setCalculatedPI(Float value) {
        this.calculatedPI = value;
    }

    /**
     * Gets the value of the peptideRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getPeptideRef() {
        return peptideRef;
    }


    /**
     * Gets the value of the rank property.
     * 
     */
    public int getRank() {
        return rank;
    }

    /**
     * Sets the value of the rank property.
     * 
     */
    public void setRank(int value) {
        this.rank = value;
    }

    /**
     * Gets the value of the passThreshold property.
     * 
     */
    public boolean isPassThreshold() {
        return passThreshold;
    }

    /**
     * Sets the value of the passThreshold property.
     * 
     */
    public void setPassThreshold(boolean value) {
        this.passThreshold = value;
    }

    /**
     * Gets the value of the massTableRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getMassTableRef() {
        return massTableRef;
    }


    /**
     * Gets the value of the sampleRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSampleRef() {
        return sampleRef;
    }


    /**
     * Get the cv params for SpectrumIdentification
     * @return
     *    List<CvParam> A FacadeList providing a CvParam view of the underlying param list.
     */
    public List<CvParam> getCvParam() {
        return new FacadeList<CvParam>(this.getParamGroup(), CvParam.class);
    }

    /**
     * Get the user params for SpectrumIdentification
     * @return
     *    List<UserParam> A FacadeList providing a UserParam view of the underlying param list.
     */
    public List<UserParam> getUserParam() {
        return new FacadeList<UserParam>(this.getParamGroup(), UserParam.class);
    }
}
