
package uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
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
 *     &lt;extension base="&lt;http://sashimi.sourceforge.net/schema_revision/mzXML_3.2>strictBase64Type">
 *       &lt;attribute name="precision">
 *         &lt;simpleType>
 *           &lt;restriction base="{http://www.w3.org/2001/XMLSchema}positiveInteger">
 *             &lt;enumeration value="32"/>
 *             &lt;enumeration value="64"/>
 *           &lt;/restriction>
 *         &lt;/simpleType>
 *       &lt;/attribute>
 *       &lt;attribute name="byteOrder" use="required" type="{http://www.w3.org/2001/XMLSchema}string" fixed="network" />
 *       &lt;attribute name="contentType" use="required">
 *         &lt;simpleType>
 *           &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *             &lt;enumeration value="m/z-int"/>
 *             &lt;enumeration value="m/z"/>
 *             &lt;enumeration value="m/z ruler"/>
 *             &lt;enumeration value="TOF"/>
 *             &lt;enumeration value="intensity"/>
 *             &lt;enumeration value="S/N"/>
 *             &lt;enumeration value="charge"/>
 *           &lt;/restriction>
 *         &lt;/simpleType>
 *       &lt;/attribute>
 *       &lt;attribute name="compressionType" use="required">
 *         &lt;simpleType>
 *           &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *             &lt;enumeration value="none"/>
 *             &lt;enumeration value="zlib"/>
 *           &lt;/restriction>
 *         &lt;/simpleType>
 *       &lt;/attribute>
 *       &lt;attribute name="compressedLen" use="required" type="{http://www.w3.org/2001/XMLSchema}int" />
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
public class Peaks
    implements Serializable, MzXMLObject
{

    private final static long serialVersionUID = 320L;
    @XmlValue
    protected byte[] value;
    @XmlAttribute
    @XmlJavaTypeAdapter(NonNegativeIntegerAdapter.class)
    protected Long precision;
    @XmlAttribute(required = true)
    protected String byteOrder;
    @XmlAttribute(required = true)
    protected String contentType;
    @XmlAttribute(required = true)
    protected String compressionType;
    @XmlAttribute(required = true)
    protected int compressedLen;

    /**
     * Gets the value of the value property.
     * 
     * @return
     *     possible object is
     *     byte[]
     */
    public byte[] getValue() {
        return value;
    }

    /**
     * Sets the value of the value property.
     * 
     * @param value
     *     allowed object is
     *     byte[]
     */
    public void setValue(byte[] value) {
        this.value = ((byte[]) value);
    }

    /**
     * Gets the value of the precision property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public Long getPrecision() {
        return precision;
    }

    /**
     * Sets the value of the precision property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setPrecision(Long value) {
        this.precision = value;
    }

    /**
     * Gets the value of the byteOrder property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getByteOrder() {
        if (byteOrder == null) {
            return "network";
        } else {
            return byteOrder;
        }
    }

    /**
     * Sets the value of the byteOrder property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setByteOrder(String value) {
        this.byteOrder = value;
    }

    /**
     * Gets the value of the contentType property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getContentType() {
        return contentType;
    }

    /**
     * Sets the value of the contentType property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setContentType(String value) {
        this.contentType = value;
    }

    /**
     * Gets the value of the compressionType property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getCompressionType() {
        return compressionType;
    }

    /**
     * Sets the value of the compressionType property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setCompressionType(String value) {
        this.compressionType = value;
    }

    /**
     * Gets the value of the compressedLen property.
     * 
     */
    public int getCompressedLen() {
        return compressedLen;
    }

    /**
     * Sets the value of the compressedLen property.
     * 
     */
    public void setCompressedLen(int value) {
        this.compressedLen = value;
    }

}
