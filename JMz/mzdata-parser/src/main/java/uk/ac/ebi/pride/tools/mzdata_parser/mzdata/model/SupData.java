
package uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.JAXBElement;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlElementRef;
import javax.xml.bind.annotation.XmlElementRefs;
import javax.xml.bind.annotation.XmlType;


/**
 * Data type for additional data vectors (beyond m/z and intensity).
 * 
 * <p>Java class for supDataType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="supDataType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="arrayName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;choice>
 *           &lt;element name="float" type="{http://www.w3.org/2001/XMLSchema}float" maxOccurs="unbounded"/>
 *           &lt;element name="double" type="{http://www.w3.org/2001/XMLSchema}double" maxOccurs="unbounded"/>
 *           &lt;element name="int" type="{http://www.w3.org/2001/XMLSchema}int" maxOccurs="unbounded"/>
 *           &lt;element name="boolean" type="{http://www.w3.org/2001/XMLSchema}boolean" maxOccurs="unbounded"/>
 *           &lt;element name="string" type="{http://www.w3.org/2001/XMLSchema}string" maxOccurs="unbounded"/>
 *           &lt;element name="time" type="{http://www.w3.org/2001/XMLSchema}float" maxOccurs="unbounded"/>
 *           &lt;element name="URI" type="{http://www.w3.org/2001/XMLSchema}anyURI" maxOccurs="unbounded"/>
 *         &lt;/choice>
 *       &lt;/sequence>
 *       &lt;attribute name="id" use="required" type="{http://www.w3.org/2001/XMLSchema}int" />
 *       &lt;attribute name="length" use="required" type="{http://www.w3.org/2001/XMLSchema}int" />
 *       &lt;attribute name="indexed" use="required" type="{http://www.w3.org/2001/XMLSchema}boolean" />
 *       &lt;attribute name="offset" type="{http://www.w3.org/2001/XMLSchema}int" default="0" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "supDataType", propOrder = {
    "arrayName",
    "floatOrDoubleOrInt"
})
public class SupData implements Serializable, MzDataObject
{

    private final static long serialVersionUID = 105L;
    @XmlElement(required = true)
    protected String arrayName;
    @XmlElementRefs({
        @XmlElementRef(name = "float", type = JAXBElement.class),
        @XmlElementRef(name = "boolean", type = JAXBElement.class),
        @XmlElementRef(name = "double", type = JAXBElement.class),
        @XmlElementRef(name = "int", type = JAXBElement.class),
        @XmlElementRef(name = "string", type = JAXBElement.class),
        @XmlElementRef(name = "time", type = JAXBElement.class),
        @XmlElementRef(name = "URI", type = JAXBElement.class)
    })
    protected List<JAXBElement<? extends Serializable>> floatOrDoubleOrInt;
    @XmlAttribute(required = true)
    protected int id;
    @XmlAttribute(required = true)
    protected int length;
    @XmlAttribute(required = true)
    protected boolean indexed;
    @XmlAttribute
    protected Integer offset;

    /**
     * Gets the value of the arrayName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getArrayName() {
        return arrayName;
    }

    /**
     * Sets the value of the arrayName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setArrayName(String value) {
        this.arrayName = value;
    }

    /**
     * Gets the value of the floatOrDoubleOrInt property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the floatOrDoubleOrInt property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getFloatOrDoubleOrInt().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link JAXBElement }{@code <}{@link Float }{@code >}
     * {@link JAXBElement }{@code <}{@link Boolean }{@code >}
     * {@link JAXBElement }{@code <}{@link Double }{@code >}
     * {@link JAXBElement }{@code <}{@link Integer }{@code >}
     * {@link JAXBElement }{@code <}{@link String }{@code >}
     * {@link JAXBElement }{@code <}{@link Float }{@code >}
     * {@link JAXBElement }{@code <}{@link String }{@code >}
     * 
     * 
     */
    public List<JAXBElement<? extends Serializable>> getFloatOrDoubleOrInt() {
        if (floatOrDoubleOrInt == null) {
            floatOrDoubleOrInt = new ArrayList<JAXBElement<? extends Serializable>>();
        }
        return this.floatOrDoubleOrInt;
    }

    /**
     * Gets the value of the id property.
     * 
     */
    public int getId() {
        return id;
    }

    /**
     * Sets the value of the id property.
     * 
     */
    public void setId(int value) {
        this.id = value;
    }

    /**
     * Gets the value of the length property.
     * 
     */
    public int getLength() {
        return length;
    }

    /**
     * Sets the value of the length property.
     * 
     */
    public void setLength(int value) {
        this.length = value;
    }

    /**
     * Gets the value of the indexed property.
     * 
     */
    public boolean isIndexed() {
        return indexed;
    }

    /**
     * Sets the value of the indexed property.
     * 
     */
    public void setIndexed(boolean value) {
        this.indexed = value;
    }

    /**
     * Gets the value of the offset property.
     * 
     * @return
     *     possible object is
     *     {@link Integer }
     *     
     */
    public int getOffset() {
        if (offset == null) {
            return  0;
        } else {
            return offset;
        }
    }

    /**
     * Sets the value of the offset property.
     * 
     * @param value
     *     allowed object is
     *     {@link Integer }
     *     
     */
    public void setOffset(Integer value) {
        this.offset = value;
    }

}
