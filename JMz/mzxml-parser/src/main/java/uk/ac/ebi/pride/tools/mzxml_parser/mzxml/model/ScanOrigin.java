
package uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
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
 *       &lt;attribute name="parentFileID" use="required">
 *         &lt;simpleType>
 *           &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *             &lt;length value="40"/>
 *           &lt;/restriction>
 *         &lt;/simpleType>
 *       &lt;/attribute>
 *       &lt;attribute name="num" use="required" type="{http://www.w3.org/2001/XMLSchema}nonNegativeInteger" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "")
public class ScanOrigin
    implements Serializable, MzXMLObject
{

    private final static long serialVersionUID = 320L;
    @XmlAttribute(required = true)
    protected String parentFileID;
    @XmlAttribute(required = true)
    @XmlJavaTypeAdapter(NonNegativeIntegerAdapter.class)
    @XmlSchemaType(name = "nonNegativeInteger")
    protected Long num;

    /**
     * Gets the value of the parentFileID property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getParentFileID() {
        return parentFileID;
    }

    /**
     * Sets the value of the parentFileID property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setParentFileID(String value) {
        this.parentFileID = value;
    }

    /**
     * Gets the value of the num property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public Long getNum() {
        return num;
    }

    /**
     * Sets the value of the num property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setNum(Long value) {
        this.num = value;
    }

}
