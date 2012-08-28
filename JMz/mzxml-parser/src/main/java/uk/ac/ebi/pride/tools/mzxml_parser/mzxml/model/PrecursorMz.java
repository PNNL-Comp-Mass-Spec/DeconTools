
package uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlSchemaType;
import javax.xml.bind.annotation.XmlType;
import javax.xml.bind.annotation.XmlValue;
import javax.xml.bind.annotation.adapters.XmlJavaTypeAdapter;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.xml.util.NonNegativeIntegerAdapter;


/**
 * <p>Java class for anonymous complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType>
 *   &lt;simpleContent>
 *     &lt;extension base="&lt;http://www.w3.org/2001/XMLSchema>float">
 *       &lt;attribute name="precursorScanNum" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *       &lt;attribute name="precursorIntensity" use="required" type="{http://www.w3.org/2001/XMLSchema}float" />
 *       &lt;attribute name="precursorCharge" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *       &lt;attribute name="possibleCharges" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="windowWideness" type="{http://www.w3.org/2001/XMLSchema}float" />
 *       &lt;attribute name="activationMethod">
 *         &lt;simpleType>
 *           &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *             &lt;enumeration value="ETD"/>
 *             &lt;enumeration value="ECD"/>
 *             &lt;enumeration value="CID"/>
 *             &lt;enumeration value="HCD"/>
 *             &lt;enumeration value="ETD+SA"/>
 *           &lt;/restriction>
 *         &lt;/simpleType>
 *       &lt;/attribute>
 *     &lt;/extension>
 *   &lt;/simpleContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {
    "value"
})
public class PrecursorMz
    implements Serializable, MzXMLObject
{

    private final static long serialVersionUID = 320L;
    @XmlValue
    protected float value;
    @XmlAttribute
    @XmlJavaTypeAdapter(NonNegativeIntegerAdapter.class)
    @XmlSchemaType(name = "positiveInteger")
    protected Long precursorScanNum;
    @XmlAttribute(required = true)
    protected float precursorIntensity;
    @XmlAttribute
    @XmlJavaTypeAdapter(NonNegativeIntegerAdapter.class)
    @XmlSchemaType(name = "positiveInteger")
    protected Long precursorCharge;
    @XmlAttribute
    protected String possibleCharges;
    @XmlAttribute
    protected Float windowWideness;
    @XmlAttribute
    protected String activationMethod;

    /**
     * Gets the value of the value property.
     * 
     */
    public float getValue() {
        return value;
    }

    /**
     * Sets the value of the value property.
     * 
     */
    public void setValue(float value) {
        this.value = value;
    }

    /**
     * Gets the value of the precursorScanNum property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public Long getPrecursorScanNum() {
        return precursorScanNum;
    }

    /**
     * Sets the value of the precursorScanNum property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setPrecursorScanNum(Long value) {
        this.precursorScanNum = value;
    }

    /**
     * Gets the value of the precursorIntensity property.
     * 
     */
    public float getPrecursorIntensity() {
        return precursorIntensity;
    }

    /**
     * Sets the value of the precursorIntensity property.
     * 
     */
    public void setPrecursorIntensity(float value) {
        this.precursorIntensity = value;
    }

    /**
     * Gets the value of the precursorCharge property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public Long getPrecursorCharge() {
        return precursorCharge;
    }

    /**
     * Sets the value of the precursorCharge property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setPrecursorCharge(Long value) {
        this.precursorCharge = value;
    }

    /**
     * Gets the value of the possibleCharges property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getPossibleCharges() {
        return possibleCharges;
    }

    /**
     * Sets the value of the possibleCharges property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setPossibleCharges(String value) {
        this.possibleCharges = value;
    }

    /**
     * Gets the value of the windowWideness property.
     * 
     * @return
     *     possible object is
     *     {@link Float }
     *     
     */
    public Float getWindowWideness() {
        return windowWideness;
    }

    /**
     * Sets the value of the windowWideness property.
     * 
     * @param value
     *     allowed object is
     *     {@link Float }
     *     
     */
    public void setWindowWideness(Float value) {
        this.windowWideness = value;
    }

    /**
     * Gets the value of the activationMethod property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getActivationMethod() {
        return activationMethod;
    }

    /**
     * Sets the value of the activationMethod property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setActivationMethod(String value) {
        this.activationMethod = value;
    }

}
