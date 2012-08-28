//
// This file was generated by the JavaTM Architecture for XML Binding(JAXB) Reference Implementation, vJAXB 2.1.3 in JDK 1.6 
// See <a href="http://java.sun.com/xml/jaxb">http://java.sun.com/xml/jaxb</a> 
// Any modifications to this file will be lost upon recompilation of the source schema. 
// Generated on: 2011.03.15 at 10:02:01 AM GMT 
//


package uk.ac.liv.unimod;

import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for aa_t complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="aa_t">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="element" type="{http://www.unimod.org/xmlns/schema/unimod_2}elem_ref_t" maxOccurs="unbounded" minOccurs="0"/>
 *       &lt;/sequence>
 *       &lt;attribute name="title" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="three_letter" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="full_name" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="mono_mass" type="{http://www.w3.org/2001/XMLSchema}double" />
 *       &lt;attribute name="avge_mass" type="{http://www.w3.org/2001/XMLSchema}double" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "aa_t", propOrder = {
    "element"
})
public class AaT {

    protected List<ElemRefT> element;
    @XmlAttribute
    protected String title;
    @XmlAttribute(name = "three_letter")
    protected String threeLetter;
    @XmlAttribute(name = "full_name")
    protected String fullName;
    @XmlAttribute(name = "mono_mass")
    protected Double monoMass;
    @XmlAttribute(name = "avge_mass")
    protected Double avgeMass;

    /**
     * Gets the value of the element property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the element property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getElement().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link ElemRefT }
     * 
     * 
     */
    public List<ElemRefT> getElement() {
        if (element == null) {
            element = new ArrayList<ElemRefT>();
        }
        return this.element;
    }

    /**
     * Gets the value of the title property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTitle() {
        return title;
    }

    /**
     * Sets the value of the title property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setTitle(String value) {
        this.title = value;
    }

    /**
     * Gets the value of the threeLetter property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getThreeLetter() {
        return threeLetter;
    }

    /**
     * Sets the value of the threeLetter property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setThreeLetter(String value) {
        this.threeLetter = value;
    }

    /**
     * Gets the value of the fullName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getFullName() {
        return fullName;
    }

    /**
     * Sets the value of the fullName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setFullName(String value) {
        this.fullName = value;
    }

    /**
     * Gets the value of the monoMass property.
     * 
     * @return
     *     possible object is
     *     {@link Double }
     *     
     */
    public Double getMonoMass() {
        return monoMass;
    }

    /**
     * Sets the value of the monoMass property.
     * 
     * @param value
     *     allowed object is
     *     {@link Double }
     *     
     */
    public void setMonoMass(Double value) {
        this.monoMass = value;
    }

    /**
     * Gets the value of the avgeMass property.
     * 
     * @return
     *     possible object is
     *     {@link Double }
     *     
     */
    public Double getAvgeMass() {
        return avgeMass;
    }

    /**
     * Sets the value of the avgeMass property.
     * 
     * @param value
     *     allowed object is
     *     {@link Double }
     *     
     */
    public void setAvgeMass(Double value) {
        this.avgeMass = value;
    }

}
