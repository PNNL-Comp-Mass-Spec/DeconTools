
package uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlSeeAlso;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for ontologyEntryType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ontologyEntryType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;attribute name="category" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="value" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ontologyEntryType")
@XmlSeeAlso({
    MsManufacturer.class,
    MsMassAnalyzer.class
})
public class OntologyEntry
    implements Serializable, MzXMLObject
{

    private final static long serialVersionUID = 320L;
    @XmlAttribute(required = true)
    protected String category;
    @XmlAttribute(name = "value", required = true)
    protected String theValue;

    /**
     * Gets the value of the category property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getCategory() {
        return category;
    }

    /**
     * Sets the value of the category property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setCategory(String value) {
        this.category = value;
    }

    /**
     * Gets the value of the theValue property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getTheValue() {
        return theValue;
    }

    /**
     * Sets the value of the theValue property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setTheValue(String value) {
        this.theValue = value;
    }

}
