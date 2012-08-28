
package uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlElements;
import javax.xml.bind.annotation.XmlSeeAlso;
import javax.xml.bind.annotation.XmlType;


/**
 * The structure tha captures the generation of a peak list (including
 *  the underlying acquisitions)
 * 
 * <p>Java class for spectrumType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="spectrumType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="spectrumDesc" type="{}spectrumDescType"/>
 *         &lt;element name="supDesc" type="{}supDescType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="mzArrayBinary" type="{}peakListBinaryType"/>
 *         &lt;element name="intenArrayBinary" type="{}peakListBinaryType"/>
 *         &lt;choice maxOccurs="unbounded" minOccurs="0">
 *           &lt;element name="supDataArrayBinary" type="{}supDataBinaryType"/>
 *           &lt;element name="supDataArray" type="{}supDataType"/>
 *         &lt;/choice>
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
@XmlType(name = "spectrumType", propOrder = {
    "spectrumDesc",
    "supDesc",
    "mzArrayBinary",
    "intenArrayBinary",
    "supDataArrayBinaryOrSupDataArray"
})
@XmlSeeAlso({
    uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.MzData.SpectrumList.Spectrum.class
})
public class Spectrum
    implements Serializable, MzDataObject
{

    private final static long serialVersionUID = 105L;
    @XmlElement(required = true)
    protected SpectrumDesc spectrumDesc;
    protected List<SupDesc> supDesc;
    @XmlElement(required = true)
    protected PeakListBinary mzArrayBinary;
    @XmlElement(required = true)
    protected PeakListBinary intenArrayBinary;
    @XmlElements({
        @XmlElement(name = "supDataArrayBinary", type = SupDataBinary.class),
        @XmlElement(name = "supDataArray", type = SupData.class)
    })
    protected List<MzDataObject> supDataArrayBinaryOrSupDataArray;
    @XmlAttribute(required = true)
    protected int id;

    /**
     * Gets the value of the spectrumDesc property.
     * 
     * @return
     *     possible object is
     *     {@link SpectrumDesc }
     *     
     */
    public SpectrumDesc getSpectrumDesc() {
        return spectrumDesc;
    }

    /**
     * Sets the value of the spectrumDesc property.
     * 
     * @param value
     *     allowed object is
     *     {@link SpectrumDesc }
     *     
     */
    public void setSpectrumDesc(SpectrumDesc value) {
        this.spectrumDesc = value;
    }

    /**
     * Gets the value of the supDesc property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the supDesc property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getSupDesc().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link SupDesc }
     * 
     * 
     */
    public List<SupDesc> getSupDesc() {
        if (supDesc == null) {
            supDesc = new ArrayList<SupDesc>();
        }
        return this.supDesc;
    }

    /**
     * Gets the value of the mzArrayBinary property.
     * 
     * @return
     *     possible object is
     *     {@link PeakListBinary }
     *     
     */
    public PeakListBinary getMzArrayBinary() {
        return mzArrayBinary;
    }

    /**
     * Sets the value of the mzArrayBinary property.
     * 
     * @param value
     *     allowed object is
     *     {@link PeakListBinary }
     *     
     */
    public void setMzArrayBinary(PeakListBinary value) {
        this.mzArrayBinary = value;
    }

    /**
     * Gets the value of the intenArrayBinary property.
     * 
     * @return
     *     possible object is
     *     {@link PeakListBinary }
     *     
     */
    public PeakListBinary getIntenArrayBinary() {
        return intenArrayBinary;
    }

    /**
     * Sets the value of the intenArrayBinary property.
     * 
     * @param value
     *     allowed object is
     *     {@link PeakListBinary }
     *     
     */
    public void setIntenArrayBinary(PeakListBinary value) {
        this.intenArrayBinary = value;
    }

    /**
     * Gets the value of the supDataArrayBinaryOrSupDataArray property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the supDataArrayBinaryOrSupDataArray property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getSupDataArrayBinaryOrSupDataArray().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link SupDataBinary }
     * {@link SupData }
     * 
     * 
     */
    public List<MzDataObject> getSupDataArrayBinaryOrSupDataArray() {
        if (supDataArrayBinaryOrSupDataArray == null) {
            supDataArrayBinaryOrSupDataArray = new ArrayList<MzDataObject>();
        }
        return this.supDataArrayBinaryOrSupDataArray;
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
