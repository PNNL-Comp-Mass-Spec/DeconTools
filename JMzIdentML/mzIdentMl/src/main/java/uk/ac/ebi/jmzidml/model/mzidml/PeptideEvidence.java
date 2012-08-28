
package uk.ac.ebi.jmzidml.model.mzidml;

import uk.ac.ebi.jmzidml.model.ParamGroupCapable;
import uk.ac.ebi.jmzidml.model.utils.FacadeList;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.*;


/**
 * PeptideEvidence links a specific Peptide element to a specific position in a DBSequence. There must only be one PeptideEvidence item per Peptide-to-DBSequence-position. 
 * 
 * <p>Java class for PeptideEvidenceType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="PeptideEvidenceType">
 *   &lt;complexContent>
 *     &lt;extension base="{http://psidev.info/psi/pi/mzIdentML/1.1}IdentifiableType">
 *       &lt;sequence>
 *         &lt;group ref="{http://psidev.info/psi/pi/mzIdentML/1.1}ParamGroup" maxOccurs="unbounded" minOccurs="0"/>
 *       &lt;/sequence>
 *       &lt;attribute name="dBSequence_ref" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="peptide_ref" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="start" type="{http://www.w3.org/2001/XMLSchema}int" />
 *       &lt;attribute name="end" type="{http://www.w3.org/2001/XMLSchema}int" />
 *       &lt;attribute name="pre">
 *         &lt;simpleType>
 *           &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *             &lt;pattern value="[ABCDEFGHIJKLMNOPQRSTUVWXYZ?\-]{1}"/>
 *           &lt;/restriction>
 *         &lt;/simpleType>
 *       &lt;/attribute>
 *       &lt;attribute name="post">
 *         &lt;simpleType>
 *           &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *             &lt;pattern value="[ABCDEFGHIJKLMNOPQRSTUVWXYZ?\-]{1}"/>
 *           &lt;/restriction>
 *         &lt;/simpleType>
 *       &lt;/attribute>
 *       &lt;attribute name="translationTable_ref" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="frame" type="{http://psidev.info/psi/pi/mzIdentML/1.1}allowed_frames" />
 *       &lt;attribute name="isDecoy" type="{http://www.w3.org/2001/XMLSchema}boolean" default="false" />
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "PeptideEvidenceType", propOrder = {
    "paramGroup"
})
public class PeptideEvidence
    extends Identifiable
    implements Serializable, ParamGroupCapable
{

    private final static long serialVersionUID = 100L;
    @XmlElements({
        @XmlElement(name = "userParam", type = UserParam.class),
        @XmlElement(name = "cvParam", type = CvParam.class)
    })
    protected List<AbstractParam> paramGroup;
    @XmlAttribute(name = "dBSequence_ref", required = true)
    protected String dbSequenceRef;
    @XmlAttribute(name = "peptide_ref", required = true)
    protected String peptideRef;
    @XmlAttribute
    protected Integer start;
    @XmlAttribute
    protected Integer end;
    @XmlAttribute
    protected String pre;
    @XmlAttribute
    protected String post;
    @XmlAttribute(name = "translationTable_ref")
    protected String translationTableRef;
    @XmlAttribute
    protected Integer frame;
    @XmlAttribute
    protected Boolean isDecoy;



    @XmlTransient
    protected DBSequence dbSequence;
    @XmlTransient
    protected Peptide peptide;
    @XmlTransient
    protected TranslationTable translationTable;


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

    public TranslationTable getTranslationTable() {
        return translationTable;
    }

    public void setTranslationTable(TranslationTable translationTable) {
        if (translationTable == null) {
            this.translationTableRef = null;
        } else {
            String refId = translationTable.getId();
            if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
            this.translationTableRef = refId;
        }
        this.translationTable = translationTable;
    }

    /**
     * Additional parameters or descriptors for the PeptideEvidence.Gets the value of the paramGroup property.
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
     * {@link UserParam }
     * {@link CvParam }
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
     * Gets the value of the start property.
     * 
     * @return
     *     possible object is
     *     {@link Integer }
     *     
     */
    public Integer getStart() {
        return start;
    }

    /**
     * Sets the value of the start property.
     * 
     * @param value
     *     allowed object is
     *     {@link Integer }
     *     
     */
    public void setStart(Integer value) {
        this.start = value;
    }

    /**
     * Gets the value of the end property.
     * 
     * @return
     *     possible object is
     *     {@link Integer }
     *     
     */
    public Integer getEnd() {
        return end;
    }

    /**
     * Sets the value of the end property.
     * 
     * @param value
     *     allowed object is
     *     {@link Integer }
     *     
     */
    public void setEnd(Integer value) {
        this.end = value;
    }

    /**
     * Gets the value of the pre property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getPre() {
        return pre;
    }

    /**
     * Sets the value of the pre property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setPre(String value) {
        this.pre = value;
    }

    /**
     * Gets the value of the post property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getPost() {
        return post;
    }

    /**
     * Sets the value of the post property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setPost(String value) {
        this.post = value;
    }

    /**
     * Gets the value of the translationTableRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTranslationTableRef() {
        return translationTableRef;
    }


    /**
     * Gets the value of the frame property.
     * 
     * @return
     *     possible object is
     *     {@link Integer }
     *     
     */
    public Integer getFrame() {
        return frame;
    }

    /**
     * Sets the value of the frame property.
     * 
     * @param value
     *     allowed object is
     *     {@link Integer }
     *     
     */
    public void setFrame(Integer value) {
        this.frame = value;
    }

    /**
     * Gets the value of the isDecoy property.
     * 
     * @return
     *     possible object is
     *     {@link Boolean }
     *     
     */
    public boolean isIsDecoy() {
        if (isDecoy == null) {
            return false;
        } else {
            return isDecoy;
        }
    }

    /**
     * Sets the value of the isDecoy property.
     * 
     * @param value
     *     allowed object is
     *     {@link Boolean }
     *     
     */
    public void setIsDecoy(Boolean value) {
        this.isDecoy = value;
    }

    public List<CvParam> getCvParam() {
        return new FacadeList<CvParam>(this.getParamGroup(), CvParam.class);
    }

    public List<UserParam> getUserParam() {
        return new FacadeList<UserParam>(this.getParamGroup(), UserParam.class);
    }

}
