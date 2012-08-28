
package uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;
import javax.xml.bind.annotation.XmlValue;


/**
 * Extension of binary data group for m/z and intensity values
 * 
 * <p>Java class for peakListBinaryType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="peakListBinaryType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;group ref="{}binaryDataGroup"/>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "peakListBinaryType", propOrder = {
    "data"
})
public class PeakListBinary
    implements Serializable, MzDataObject
{

    private final static long serialVersionUID = 105L;
    @XmlElement(required = true)
    protected PeakListBinary.Data data;

    /**
     * Gets the value of the data property.
     * 
     * @return
     *     possible object is
     *     {@link PeakListBinary.Data }
     *     
     */
    public PeakListBinary.Data getData() {
        return data;
    }

    /**
     * Sets the value of the data property.
     * 
     * @param value
     *     allowed object is
     *     {@link PeakListBinary.Data }
     *     
     */
    public void setData(PeakListBinary.Data value) {
        this.data = value;
    }


    /**
     * <p>Java class for anonymous complex type.
     * 
     * <p>The following schema fragment specifies the expected content contained within this class.
     * 
     * <pre>
     * &lt;complexType>
     *   &lt;simpleContent>
     *     &lt;extension base="&lt;http://www.w3.org/2001/XMLSchema>base64Binary">
     *       &lt;attribute name="precision" use="required">
     *         &lt;simpleType>
     *           &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
     *             &lt;enumeration value="32"/>
     *             &lt;enumeration value="64"/>
     *           &lt;/restriction>
     *         &lt;/simpleType>
     *       &lt;/attribute>
     *       &lt;attribute name="endian" use="required">
     *         &lt;simpleType>
     *           &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
     *             &lt;enumeration value="big"/>
     *             &lt;enumeration value="little"/>
     *           &lt;/restriction>
     *         &lt;/simpleType>
     *       &lt;/attribute>
     *       &lt;attribute name="length" use="required" type="{http://www.w3.org/2001/XMLSchema}int" />
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
    public static class Data
        implements Serializable, MzDataObject
    {

        private final static long serialVersionUID = 105L;
        @XmlValue
        protected byte[] value;
        @XmlAttribute(required = true)
        protected String precision;
        @XmlAttribute(required = true)
        protected String endian;
        @XmlAttribute(required = true)
        protected int length;

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
        public String getPrecision() {
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
        public void setPrecision(String value) {
            this.precision = value;
        }

        /**
         * Gets the value of the endian property.
         * 
         * @return
         *     possible object is
         *     {@link String }
         *     
         */
        public String getEndian() {
            return endian;
        }

        /**
         * Sets the value of the endian property.
         * 
         * @param value
         *     allowed object is
         *     {@link String }
         *     
         */
        public void setEndian(String value) {
            this.endian = value;
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

    }

}
