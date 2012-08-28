
package uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * Extension of binary data group for supplemental data
 * 
 * <p>Java class for supDataBinaryType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="supDataBinaryType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="arrayName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;group ref="{}binaryDataGroup"/>
 *       &lt;/sequence>
 *       &lt;attribute name="id" use="required" type="{http://www.w3.org/2001/XMLSchema}int" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "supDataBinaryType", propOrder = {
    "arrayName",
    "data"
})
public class SupDataBinary implements Serializable, MzDataObject
{

    private final static long serialVersionUID = 105L;
    @XmlElement(required = true)
    protected String arrayName;
    @XmlElement(required = true)
    protected uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.PeakListBinary.Data data;
    @XmlAttribute(required = true)
    protected int id;

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
     * Gets the value of the data property.
     * 
     * @return
     *     possible object is
     *     {@link uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.PeakListBinary.Data }
     *     
     */
    public uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.PeakListBinary.Data getData() {
        return data;
    }

    /**
     * Sets the value of the data property.
     * 
     * @param value
     *     allowed object is
     *     {@link uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.PeakListBinary.Data }
     *     
     */
    public void setData(uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.PeakListBinary.Data value) {
        this.data = value;
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

}
