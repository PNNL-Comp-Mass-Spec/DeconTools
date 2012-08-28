
package uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlSchemaType;
import javax.xml.bind.annotation.XmlType;
import javax.xml.bind.annotation.adapters.XmlJavaTypeAdapter;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.xml.util.NonNegativeIntegerAdapter;


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
 *         &lt;element name="maldiMatrix" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *       &lt;/sequence>
 *       &lt;attribute name="spotID" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="spotXPosition" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="spotYPosition" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="spotDiameter" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {
    "maldiMatrix"
})
public class Spot
    implements Serializable, MzXMLObject
{

    private final static long serialVersionUID = 320L;
    @XmlElement(required = true)
    protected OntologyEntry maldiMatrix;
    @XmlAttribute(required = true)
    protected String spotID;
    @XmlAttribute(required = true)
    protected String spotXPosition;
    @XmlAttribute(required = true)
    protected String spotYPosition;
    @XmlAttribute
    @XmlJavaTypeAdapter(NonNegativeIntegerAdapter.class)
    @XmlSchemaType(name = "positiveInteger")
    protected Long spotDiameter;

    /**
     * Gets the value of the maldiMatrix property.
     * 
     * @return
     *     possible object is
     *     {@link OntologyEntry }
     *     
     */
    public OntologyEntry getMaldiMatrix() {
        return maldiMatrix;
    }

    /**
     * Sets the value of the maldiMatrix property.
     * 
     * @param value
     *     allowed object is
     *     {@link OntologyEntry }
     *     
     */
    public void setMaldiMatrix(OntologyEntry value) {
        this.maldiMatrix = value;
    }

    /**
     * Gets the value of the spotID property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSpotID() {
        return spotID;
    }

    /**
     * Sets the value of the spotID property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSpotID(String value) {
        this.spotID = value;
    }

    /**
     * Gets the value of the spotXPosition property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSpotXPosition() {
        return spotXPosition;
    }

    /**
     * Sets the value of the spotXPosition property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSpotXPosition(String value) {
        this.spotXPosition = value;
    }

    /**
     * Gets the value of the spotYPosition property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSpotYPosition() {
        return spotYPosition;
    }

    /**
     * Sets the value of the spotYPosition property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSpotYPosition(String value) {
        this.spotYPosition = value;
    }

    /**
     * Gets the value of the spotDiameter property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public Long getSpotDiameter() {
        return spotDiameter;
    }

    /**
     * Sets the value of the spotDiameter property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSpotDiameter(Long value) {
        this.spotDiameter = value;
    }

}
