
package uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
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
 *         &lt;element name="plateManufacturer" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *         &lt;element name="plateModel" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *         &lt;element name="pattern" minOccurs="0">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 &lt;sequence>
 *                   &lt;element name="spottingPattern" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *                   &lt;element name="orientation">
 *                     &lt;complexType>
 *                       &lt;complexContent>
 *                         &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                           &lt;attribute name="firstSpotID" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *                           &lt;attribute name="secondSpotID" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *                         &lt;/restriction>
 *                       &lt;/complexContent>
 *                     &lt;/complexType>
 *                   &lt;/element>
 *                 &lt;/sequence>
 *               &lt;/restriction>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element name="spot" maxOccurs="unbounded">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 &lt;sequence>
 *                   &lt;element name="maldiMatrix" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *                 &lt;/sequence>
 *                 &lt;attribute name="spotID" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *                 &lt;attribute name="spotXPosition" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *                 &lt;attribute name="spotYPosition" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *                 &lt;attribute name="spotDiameter" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *               &lt;/restriction>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *       &lt;/sequence>
 *       &lt;attribute name="plateID" use="required" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *       &lt;attribute name="spotXCount" use="required" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *       &lt;attribute name="spotYCount" use="required" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {
    "plateManufacturer",
    "plateModel",
    "pattern",
    "spot"
})
public class Plate
    implements Serializable, MzXMLObject
{

    private final static long serialVersionUID = 320L;
    @XmlElement(required = true)
    protected OntologyEntry plateManufacturer;
    @XmlElement(required = true)
    protected OntologyEntry plateModel;
    protected Pattern pattern;
    @XmlElement(required = true)
    protected List<Spot> spot;
    @XmlAttribute(required = true)
    @XmlJavaTypeAdapter(NonNegativeIntegerAdapter.class)
    @XmlSchemaType(name = "positiveInteger")
    protected Long plateID;
    @XmlAttribute(required = true)
    @XmlJavaTypeAdapter(NonNegativeIntegerAdapter.class)
    @XmlSchemaType(name = "positiveInteger")
    protected Long spotXCount;
    @XmlAttribute(required = true)
    @XmlJavaTypeAdapter(NonNegativeIntegerAdapter.class)
    @XmlSchemaType(name = "positiveInteger")
    protected Long spotYCount;

    /**
     * Gets the value of the plateManufacturer property.
     * 
     * @return
     *     possible object is
     *     {@link OntologyEntry }
     *     
     */
    public OntologyEntry getPlateManufacturer() {
        return plateManufacturer;
    }

    /**
     * Sets the value of the plateManufacturer property.
     * 
     * @param value
     *     allowed object is
     *     {@link OntologyEntry }
     *     
     */
    public void setPlateManufacturer(OntologyEntry value) {
        this.plateManufacturer = value;
    }

    /**
     * Gets the value of the plateModel property.
     * 
     * @return
     *     possible object is
     *     {@link OntologyEntry }
     *     
     */
    public OntologyEntry getPlateModel() {
        return plateModel;
    }

    /**
     * Sets the value of the plateModel property.
     * 
     * @param value
     *     allowed object is
     *     {@link OntologyEntry }
     *     
     */
    public void setPlateModel(OntologyEntry value) {
        this.plateModel = value;
    }

    /**
     * Gets the value of the pattern property.
     * 
     * @return
     *     possible object is
     *     {@link Pattern }
     *     
     */
    public Pattern getPattern() {
        return pattern;
    }

    /**
     * Sets the value of the pattern property.
     * 
     * @param value
     *     allowed object is
     *     {@link Pattern }
     *     
     */
    public void setPattern(Pattern value) {
        this.pattern = value;
    }

    /**
     * Gets the value of the spot property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the spot property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getSpot().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Spot }
     * 
     * 
     */
    public List<Spot> getSpot() {
        if (spot == null) {
            spot = new ArrayList<Spot>();
        }
        return this.spot;
    }

    /**
     * Gets the value of the plateID property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public Long getPlateID() {
        return plateID;
    }

    /**
     * Sets the value of the plateID property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setPlateID(Long value) {
        this.plateID = value;
    }

    /**
     * Gets the value of the spotXCount property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public Long getSpotXCount() {
        return spotXCount;
    }

    /**
     * Sets the value of the spotXCount property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSpotXCount(Long value) {
        this.spotXCount = value;
    }

    /**
     * Gets the value of the spotYCount property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public Long getSpotYCount() {
        return spotYCount;
    }

    /**
     * Sets the value of the spotYCount property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSpotYCount(Long value) {
        this.spotYCount = value;
    }

}
