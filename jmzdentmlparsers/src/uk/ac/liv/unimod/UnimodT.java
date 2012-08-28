//
// This file was generated by the JavaTM Architecture for XML Binding(JAXB) Reference Implementation, vJAXB 2.1.3 in JDK 1.6 
// See <a href="http://java.sun.com/xml/jaxb">http://java.sun.com/xml/jaxb</a> 
// Any modifications to this file will be lost upon recompilation of the source schema. 
// Generated on: 2011.03.15 at 10:02:01 AM GMT 
//


package uk.ac.liv.unimod;

import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlSchemaType;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for unimod_t complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="unimod_t">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="elements" type="{http://www.unimod.org/xmlns/schema/unimod_2}elements_t" minOccurs="0"/>
 *         &lt;element name="modifications" type="{http://www.unimod.org/xmlns/schema/unimod_2}modifications_t" minOccurs="0"/>
 *         &lt;element name="amino_acids" type="{http://www.unimod.org/xmlns/schema/unimod_2}amino_acids_t" minOccurs="0"/>
 *         &lt;element name="mod_bricks" type="{http://www.unimod.org/xmlns/schema/unimod_2}mod_bricks_t" minOccurs="0"/>
 *       &lt;/sequence>
 *       &lt;attribute name="majorVersion" use="required" type="{http://www.w3.org/2001/XMLSchema}unsignedShort" fixed="2" />
 *       &lt;attribute name="minorVersion" use="required" type="{http://www.unimod.org/xmlns/schema/unimod_2}minorVersion_t" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "unimod_t", propOrder = {
    "elements",
    "modifications",
    "aminoAcids",
    "modBricks"
})
public class UnimodT {

    protected ElementsT elements;
    protected ModificationsT modifications;
    @XmlElement(name = "amino_acids")
    protected AminoAcidsT aminoAcids;
    @XmlElement(name = "mod_bricks")
    protected ModBricksT modBricks;
    @XmlAttribute(required = true)
    @XmlSchemaType(name = "unsignedShort")
    protected int majorVersion;
    @XmlAttribute(required = true)
    protected int minorVersion;

    /**
     * Gets the value of the elements property.
     * 
     * @return
     *     possible object is
     *     {@link ElementsT }
     *     
     */
    public ElementsT getElements() {
        return elements;
    }

    /**
     * Sets the value of the elements property.
     * 
     * @param value
     *     allowed object is
     *     {@link ElementsT }
     *     
     */
    public void setElements(ElementsT value) {
        this.elements = value;
    }

    /**
     * Gets the value of the modifications property.
     * 
     * @return
     *     possible object is
     *     {@link ModificationsT }
     *     
     */
    public ModificationsT getModifications() {
        return modifications;
    }

    /**
     * Sets the value of the modifications property.
     * 
     * @param value
     *     allowed object is
     *     {@link ModificationsT }
     *     
     */
    public void setModifications(ModificationsT value) {
        this.modifications = value;
    }

    /**
     * Gets the value of the aminoAcids property.
     * 
     * @return
     *     possible object is
     *     {@link AminoAcidsT }
     *     
     */
    public AminoAcidsT getAminoAcids() {
        return aminoAcids;
    }

    /**
     * Sets the value of the aminoAcids property.
     * 
     * @param value
     *     allowed object is
     *     {@link AminoAcidsT }
     *     
     */
    public void setAminoAcids(AminoAcidsT value) {
        this.aminoAcids = value;
    }

    /**
     * Gets the value of the modBricks property.
     * 
     * @return
     *     possible object is
     *     {@link ModBricksT }
     *     
     */
    public ModBricksT getModBricks() {
        return modBricks;
    }

    /**
     * Sets the value of the modBricks property.
     * 
     * @param value
     *     allowed object is
     *     {@link ModBricksT }
     *     
     */
    public void setModBricks(ModBricksT value) {
        this.modBricks = value;
    }

    /**
     * Gets the value of the majorVersion property.
     * 
     */
    public int getMajorVersion() {
        return majorVersion;
    }

    /**
     * Sets the value of the majorVersion property.
     * 
     */
    public void setMajorVersion(int value) {
        this.majorVersion = value;
    }

    /**
     * Gets the value of the minorVersion property.
     * 
     */
    public int getMinorVersion() {
        return minorVersion;
    }

    /**
     * Sets the value of the minorVersion property.
     * 
     */
    public void setMinorVersion(int value) {
        this.minorVersion = value;
    }

}
