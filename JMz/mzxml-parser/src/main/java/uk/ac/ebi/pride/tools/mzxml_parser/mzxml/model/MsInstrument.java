
package uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlElements;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for anonymous complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType>
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="msManufacturer">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;extension base="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType">
 *               &lt;/extension>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element name="msModel" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *         &lt;element name="msIonisation" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *         &lt;element name="msMassAnalyzer">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;extension base="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType">
 *               &lt;/extension>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element name="msDetector" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *         &lt;element ref="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}software"/>
 *         &lt;element name="msResolution" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType" minOccurs="0"/>
 *         &lt;element ref="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}operator" minOccurs="0"/>
 *         &lt;sequence maxOccurs="unbounded" minOccurs="0">
 *           &lt;element name="nameValue" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}namevalueType"/>
 *           &lt;element name="comment" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;/sequence>
 *       &lt;/sequence>
 *       &lt;attribute name="msInstrumentID" type="{http://www.w3.org/2001/XMLSchema}int" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {
    "msManufacturer",
    "msModel",
    "msIonisation",
    "msMassAnalyzer",
    "msDetector",
    "software",
    "msResolution",
    "operator",
    "nameValueAndComment"
})
public class MsInstrument
    implements Serializable, MzXMLObject
{

    private final static long serialVersionUID = 320L;
    @XmlElement(required = true)
    protected MsManufacturer msManufacturer;
    @XmlElement(required = true)
    protected OntologyEntry msModel;
    @XmlElement(required = true)
    protected OntologyEntry msIonisation;
    @XmlElement(required = true)
    protected MsMassAnalyzer msMassAnalyzer;
    @XmlElement(required = true)
    protected OntologyEntry msDetector;
    @XmlElement(required = true)
    protected Software software;
    protected OntologyEntry msResolution;
    protected Operator operator;
    @XmlElements({
        @XmlElement(name = "nameValue", type = NameValue.class),
        @XmlElement(name = "comment", type = String.class)
    })
    protected List<Serializable> nameValueAndComment;
    @XmlAttribute
    protected Integer msInstrumentID;

    /**
     * Gets the value of the msManufacturer property.
     * 
     * @return
     *     possible object is
     *     {@link MsManufacturer }
     *     
     */
    public MsManufacturer getMsManufacturer() {
        return msManufacturer;
    }

    /**
     * Sets the value of the msManufacturer property.
     * 
     * @param value
     *     allowed object is
     *     {@link MsManufacturer }
     *     
     */
    public void setMsManufacturer(MsManufacturer value) {
        this.msManufacturer = value;
    }

    /**
     * Gets the value of the msModel property.
     * 
     * @return
     *     possible object is
     *     {@link OntologyEntry }
     *     
     */
    public OntologyEntry getMsModel() {
        return msModel;
    }

    /**
     * Sets the value of the msModel property.
     * 
     * @param value
     *     allowed object is
     *     {@link OntologyEntry }
     *     
     */
    public void setMsModel(OntologyEntry value) {
        this.msModel = value;
    }

    /**
     * Gets the value of the msIonisation property.
     * 
     * @return
     *     possible object is
     *     {@link OntologyEntry }
     *     
     */
    public OntologyEntry getMsIonisation() {
        return msIonisation;
    }

    /**
     * Sets the value of the msIonisation property.
     * 
     * @param value
     *     allowed object is
     *     {@link OntologyEntry }
     *     
     */
    public void setMsIonisation(OntologyEntry value) {
        this.msIonisation = value;
    }

    /**
     * Gets the value of the msMassAnalyzer property.
     * 
     * @return
     *     possible object is
     *     {@link MsMassAnalyzer }
     *     
     */
    public MsMassAnalyzer getMsMassAnalyzer() {
        return msMassAnalyzer;
    }

    /**
     * Sets the value of the msMassAnalyzer property.
     * 
     * @param value
     *     allowed object is
     *     {@link MsMassAnalyzer }
     *     
     */
    public void setMsMassAnalyzer(MsMassAnalyzer value) {
        this.msMassAnalyzer = value;
    }

    /**
     * Gets the value of the msDetector property.
     * 
     * @return
     *     possible object is
     *     {@link OntologyEntry }
     *     
     */
    public OntologyEntry getMsDetector() {
        return msDetector;
    }

    /**
     * Sets the value of the msDetector property.
     * 
     * @param value
     *     allowed object is
     *     {@link OntologyEntry }
     *     
     */
    public void setMsDetector(OntologyEntry value) {
        this.msDetector = value;
    }

    /**
     * Gets the value of the software property.
     * 
     * @return
     *     possible object is
     *     {@link Software }
     *     
     */
    public Software getSoftware() {
        return software;
    }

    /**
     * Sets the value of the software property.
     * 
     * @param value
     *     allowed object is
     *     {@link Software }
     *     
     */
    public void setSoftware(Software value) {
        this.software = value;
    }

    /**
     * Gets the value of the msResolution property.
     * 
     * @return
     *     possible object is
     *     {@link OntologyEntry }
     *     
     */
    public OntologyEntry getMsResolution() {
        return msResolution;
    }

    /**
     * Sets the value of the msResolution property.
     * 
     * @param value
     *     allowed object is
     *     {@link OntologyEntry }
     *     
     */
    public void setMsResolution(OntologyEntry value) {
        this.msResolution = value;
    }

    /**
     * Gets the value of the operator property.
     * 
     * @return
     *     possible object is
     *     {@link Operator }
     *     
     */
    public Operator getOperator() {
        return operator;
    }

    /**
     * Sets the value of the operator property.
     * 
     * @param value
     *     allowed object is
     *     {@link Operator }
     *     
     */
    public void setOperator(Operator value) {
        this.operator = value;
    }

    /**
     * Gets the value of the nameValueAndComment property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the nameValueAndComment property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getNameValueAndComment().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link NameValue }
     * {@link String }
     * 
     * 
     */
    public List<Serializable> getNameValueAndComment() {
        if (nameValueAndComment == null) {
            nameValueAndComment = new ArrayList<Serializable>();
        }
        return this.nameValueAndComment;
    }

    /**
     * Gets the value of the msInstrumentID property.
     * 
     * @return
     *     possible object is
     *     {@link Integer }
     *     
     */
    public Integer getMsInstrumentID() {
        return msInstrumentID;
    }

    /**
     * Sets the value of the msInstrumentID property.
     * 
     * @param value
     *     allowed object is
     *     {@link Integer }
     *     
     */
    public void setMsInstrumentID(Integer value) {
        this.msInstrumentID = value;
    }

}
