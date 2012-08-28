
package uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlType;


/**
 * Description of a supplemental data array
 * 
 * <p>Java class for supDescType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="supDescType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="supDataDesc" type="{}descriptionType" minOccurs="0"/>
 *         &lt;element name="supSourceFile" type="{}sourceFileType" maxOccurs="unbounded" minOccurs="0"/>
 *       &lt;/sequence>
 *       &lt;attribute name="supDataArrayRef" use="required" type="{http://www.w3.org/2001/XMLSchema}int" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "supDescType", propOrder = {
    "supDataDesc",
    "supSourceFile"
})
public class SupDesc
    implements Serializable, MzDataObject
{

    private final static long serialVersionUID = 105L;
    protected Description supDataDesc;
    protected List<SourceFile> supSourceFile;
    @XmlAttribute(required = true)
    protected int supDataArrayRef;

    /**
     * Gets the value of the supDataDesc property.
     * 
     * @return
     *     possible object is
     *     {@link Description }
     *     
     */
    public Description getSupDataDesc() {
        return supDataDesc;
    }

    /**
     * Sets the value of the supDataDesc property.
     * 
     * @param value
     *     allowed object is
     *     {@link Description }
     *     
     */
    public void setSupDataDesc(Description value) {
        this.supDataDesc = value;
    }

    /**
     * Gets the value of the supSourceFile property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the supSourceFile property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getSupSourceFile().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link SourceFile }
     * 
     * 
     */
    public List<SourceFile> getSupSourceFile() {
        if (supSourceFile == null) {
            supSourceFile = new ArrayList<SourceFile>();
        }
        return this.supSourceFile;
    }

    /**
     * Gets the value of the supDataArrayRef property.
     * 
     */
    public int getSupDataArrayRef() {
        return supDataArrayRef;
    }

    /**
     * Sets the value of the supDataArrayRef property.
     * 
     */
    public void setSupDataArrayRef(int value) {
        this.supDataArrayRef = value;
    }

}
