
package uk.ac.ebi.jmzidml.model.mzidml;

import uk.ac.ebi.jmzidml.model.ParamGroupCapable;
import uk.ac.ebi.jmzidml.model.utils.FacadeList;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.*;


/**
 * A single result of the ProteinDetection analysis (i.e. a protein).
 * 
 * <p>Java class for ProteinDetectionHypothesisType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ProteinDetectionHypothesisType">
 *   &lt;complexContent>
 *     &lt;extension base="{http://psidev.info/psi/pi/mzIdentML/1.1}IdentifiableType">
 *       &lt;sequence>
 *         &lt;element name="PeptideHypothesis" type="{http://psidev.info/psi/pi/mzIdentML/1.1}PeptideHypothesisType" maxOccurs="unbounded"/>
 *         &lt;group ref="{http://psidev.info/psi/pi/mzIdentML/1.1}ParamGroup" maxOccurs="unbounded" minOccurs="0"/>
 *       &lt;/sequence>
 *       &lt;attribute name="dBSequence_ref" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="passThreshold" use="required" type="{http://www.w3.org/2001/XMLSchema}boolean" />
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ProteinDetectionHypothesisType", propOrder = {
    "peptideHypothesis",
    "paramGroup"
})
public class ProteinDetectionHypothesis
    extends Identifiable
    implements Serializable, ParamGroupCapable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "PeptideHypothesis", required = true)
    protected List<PeptideHypothesis> peptideHypothesis;
    @XmlElements({
        @XmlElement(name = "cvParam", type = CvParam.class),
        @XmlElement(name = "userParam", type = UserParam.class)
    })
    protected List<AbstractParam> paramGroup;
    @XmlAttribute(name = "dBSequence_ref")
    protected String dbSequenceRef;
    @XmlAttribute(required = true)
    protected boolean passThreshold;
    @XmlTransient
    protected DBSequence dbSequence;

    /**
     * Gets the value of the peptideHypothesis property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the peptideHypothesis property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getPeptideHypothesis().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link PeptideHypothesis }
     * 
     * 
     */
    public List<PeptideHypothesis> getPeptideHypothesis() {
        if (peptideHypothesis == null) {
            peptideHypothesis = new ArrayList<PeptideHypothesis>();
        }
        return this.peptideHypothesis;
    }

    /**
     * Scores or parameters associated with this ProteinDetectionHypothesis e.g. p-value Gets the value of the paramGroup property.
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
     * Gets the value of the dbSequenceRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getDBSequenceRef() {
        return dbSequenceRef;
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


    public DBSequence getDBSequence() {
        return dbSequence;
    }

    public void setDBSequence(DBSequence dbSequence) {
        if (dbSequence == null) {
            this.dbSequenceRef = null;
        } else {
            String refId = dbSequence.getId();
            if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
            this.dbSequenceRef = refId;
        }
        this.dbSequence = dbSequence;
    }

    public List<CvParam> getCvParam() {
        return new FacadeList<CvParam>(this.getParamGroup(), CvParam.class);
    }

    public List<UserParam> getUserParam() {
        return new FacadeList<UserParam>(this.getParamGroup(), UserParam.class);
    }
}
