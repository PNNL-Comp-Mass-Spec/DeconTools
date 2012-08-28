
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;
import uk.ac.ebi.jmzidml.model.MzIdentMLObject;


/**
 * The analyses performed to get the results, which map the input and output data sets. Analyses are for example: SpectrumIdentification (resulting in peptides) or ProteinDetection (assemble proteins from peptides).
 * 
 * <p>Java class for AnalysisCollectionType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="AnalysisCollectionType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="SpectrumIdentification" type="{http://psidev.info/psi/pi/mzIdentML/1.1}SpectrumIdentificationType" maxOccurs="unbounded"/>
 *         &lt;element name="ProteinDetection" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ProteinDetectionType" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "AnalysisCollectionType", propOrder = {
    "spectrumIdentification",
    "proteinDetection"
})
public class AnalysisCollection
    extends MzIdentMLObject
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "SpectrumIdentification", required = true)
    protected List<SpectrumIdentification> spectrumIdentification;
    @XmlElement(name = "ProteinDetection")
    protected ProteinDetection proteinDetection;

    /**
     * Gets the value of the spectrumIdentification property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the spectrumIdentification property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getSpectrumIdentification().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link SpectrumIdentification }
     * 
     * @return spectrumIdentification
     */
    public List<SpectrumIdentification> getSpectrumIdentification() {
        if (spectrumIdentification == null) {
            spectrumIdentification = new ArrayList<SpectrumIdentification>();
        }
        return this.spectrumIdentification;
    }

    /**
     * Gets the value of the proteinDetection property.
     * 
     * @return
     *     possible object is
     *     {@link ProteinDetection }
     *     
     */
    public ProteinDetection getProteinDetection() {
        return proteinDetection;
    }

    /**
     * Sets the value of the proteinDetection property.
     * 
     * @param value
     *     allowed object is
     *     {@link ProteinDetection }
     *     
     */
    public void setProteinDetection(ProteinDetection value) {
        this.proteinDetection = value;
    }

}
