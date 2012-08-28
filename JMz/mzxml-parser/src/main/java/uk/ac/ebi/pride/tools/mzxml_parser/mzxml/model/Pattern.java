
package uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
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
 *         &lt;element name="spottingPattern" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *         &lt;element name="orientation">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 &lt;attribute name="firstSpotID" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *                 &lt;attribute name="secondSpotID" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *               &lt;/restriction>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {
    "spottingPattern",
    "orientation"
})
public class Pattern
    implements Serializable, MzXMLObject
{

    private final static long serialVersionUID = 320L;
    @XmlElement(required = true)
    protected OntologyEntry spottingPattern;
    @XmlElement(required = true)
    protected Orientation orientation;

    /**
     * Gets the value of the spottingPattern property.
     * 
     * @return
     *     possible object is
     *     {@link OntologyEntry }
     *     
     */
    public OntologyEntry getSpottingPattern() {
        return spottingPattern;
    }

    /**
     * Sets the value of the spottingPattern property.
     * 
     * @param value
     *     allowed object is
     *     {@link OntologyEntry }
     *     
     */
    public void setSpottingPattern(OntologyEntry value) {
        this.spottingPattern = value;
    }

    /**
     * Gets the value of the orientation property.
     * 
     * @return
     *     possible object is
     *     {@link Orientation }
     *     
     */
    public Orientation getOrientation() {
        return orientation;
    }

    /**
     * Sets the value of the orientation property.
     * 
     * @param value
     *     allowed object is
     *     {@link Orientation }
     *     
     */
    public void setOrientation(Orientation value) {
        this.orientation = value;
    }

}
