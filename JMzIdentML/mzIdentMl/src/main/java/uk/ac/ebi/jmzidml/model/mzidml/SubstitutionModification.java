
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlType;
import uk.ac.ebi.jmzidml.model.MzIdentMLObject;


/**
 * A modification where one residue is substituted by another (amino acid change). 
 * 
 * <p>Java class for SubstitutionModificationType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="SubstitutionModificationType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;attribute name="originalResidue" use="required">
 *         &lt;simpleType>
 *           &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *             &lt;pattern value="[ABCDEFGHIJKLMNOPQRSTUVWXYZ?\-]{1}"/>
 *           &lt;/restriction>
 *         &lt;/simpleType>
 *       &lt;/attribute>
 *       &lt;attribute name="replacementResidue" use="required">
 *         &lt;simpleType>
 *           &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *             &lt;pattern value="[ABCDEFGHIJKLMNOPQRSTUVWXYZ?\-]{1}"/>
 *           &lt;/restriction>
 *         &lt;/simpleType>
 *       &lt;/attribute>
 *       &lt;attribute name="location" type="{http://www.w3.org/2001/XMLSchema}int" />
 *       &lt;attribute name="avgMassDelta" type="{http://www.w3.org/2001/XMLSchema}double" />
 *       &lt;attribute name="monoisotopicMassDelta" type="{http://www.w3.org/2001/XMLSchema}double" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "SubstitutionModificationType")
public class SubstitutionModification
    extends MzIdentMLObject
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlAttribute(required = true)
    protected String originalResidue;
    @XmlAttribute(required = true)
    protected String replacementResidue;
    @XmlAttribute
    protected Integer location;
    @XmlAttribute
    protected Double avgMassDelta;
    @XmlAttribute
    protected Double monoisotopicMassDelta;

    /**
     * Gets the value of the originalResidue property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getOriginalResidue() {
        return originalResidue;
    }

    /**
     * Sets the value of the originalResidue property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setOriginalResidue(String value) {
        this.originalResidue = value;
    }

    /**
     * Gets the value of the replacementResidue property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getReplacementResidue() {
        return replacementResidue;
    }

    /**
     * Sets the value of the replacementResidue property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setReplacementResidue(String value) {
        this.replacementResidue = value;
    }

    /**
     * Gets the value of the location property.
     * 
     * @return
     *     possible object is
     *     {@link Integer }
     *     
     */
    public Integer getLocation() {
        return location;
    }

    /**
     * Sets the value of the location property.
     * 
     * @param value
     *     allowed object is
     *     {@link Integer }
     *     
     */
    public void setLocation(Integer value) {
        this.location = value;
    }

    /**
     * Gets the value of the avgMassDelta property.
     * 
     * @return
     *     possible object is
     *     {@link Double }
     *     
     */
    public Double getAvgMassDelta() {
        return avgMassDelta;
    }

    /**
     * Sets the value of the avgMassDelta property.
     * 
     * @param value
     *     allowed object is
     *     {@link Double }
     *     
     */
    public void setAvgMassDelta(Double value) {
        this.avgMassDelta = value;
    }

    /**
     * Gets the value of the monoisotopicMassDelta property.
     * 
     * @return
     *     possible object is
     *     {@link Double }
     *     
     */
    public Double getMonoisotopicMassDelta() {
        return monoisotopicMassDelta;
    }

    /**
     * Sets the value of the monoisotopicMassDelta property.
     * 
     * @param value
     *     allowed object is
     *     {@link Double }
     *     
     */
    public void setMonoisotopicMassDelta(Double value) {
        this.monoisotopicMassDelta = value;
    }

}
