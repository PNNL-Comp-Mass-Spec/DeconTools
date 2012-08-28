
package uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * Description of the software, and the way in which it was used to generate the peak list.
 * 
 * <p>Java class for dataProcessingType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="dataProcessingType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="software">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;extension base="{}softwareType">
 *               &lt;/extension>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element name="processingMethod" type="{}paramType" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "dataProcessingType", propOrder = {
    "software",
    "processingMethod"
})
public class DataProcessing
    implements Serializable, MzDataObject
{

    private final static long serialVersionUID = 105L;
    @XmlElement(required = true)
    protected DataProcessing.Software software;
    protected Param processingMethod;

    /**
     * Gets the value of the software property.
     * 
     * @return
     *     possible object is
     *     {@link DataProcessing.Software }
     *     
     */
    public DataProcessing.Software getSoftware() {
        return software;
    }

    /**
     * Sets the value of the software property.
     * 
     * @param value
     *     allowed object is
     *     {@link DataProcessing.Software }
     *     
     */
    public void setSoftware(DataProcessing.Software value) {
        this.software = value;
    }

    /**
     * Gets the value of the processingMethod property.
     * 
     * @return
     *     possible object is
     *     {@link Param }
     *     
     */
    public Param getProcessingMethod() {
        return processingMethod;
    }

    /**
     * Sets the value of the processingMethod property.
     * 
     * @param value
     *     allowed object is
     *     {@link Param }
     *     
     */
    public void setProcessingMethod(Param value) {
        this.processingMethod = value;
    }


    /**
     * <p>Java class for anonymous complex type.
     * 
     * <p>The following schema fragment specifies the expected content contained within this class.
     * 
     * <pre>
     * &lt;complexType>
     *   &lt;complexContent>
     *     &lt;extension base="{}softwareType">
     *     &lt;/extension>
     *   &lt;/complexContent>
     * &lt;/complexType>
     * </pre>
     * 
     * 
     */
    @XmlAccessorType(XmlAccessType.FIELD)
    @XmlType(name = "")
    public static class Software
        extends uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Software
        implements Serializable
    {

        private final static long serialVersionUID = 105L;

    }

}
