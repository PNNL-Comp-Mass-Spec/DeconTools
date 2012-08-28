
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * A data set containing spectra data (consisting of one or more spectra). 
 * 
 * <p>Java class for SpectraDataType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="SpectraDataType">
 *   &lt;complexContent>
 *     &lt;extension base="{http://psidev.info/psi/pi/mzIdentML/1.1}ExternalDataType">
 *       &lt;sequence>
 *         &lt;element name="SpectrumIDFormat" type="{http://psidev.info/psi/pi/mzIdentML/1.1}SpectrumIDFormatType"/>
 *       &lt;/sequence>
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "SpectraDataType", propOrder = {
    "spectrumIDFormat"
})
public class SpectraData
    extends ExternalData
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "SpectrumIDFormat", required = true)
    protected SpectrumIDFormat spectrumIDFormat;

    /**
     * Gets the value of the spectrumIDFormat property.
     * 
     * @return
     *     possible object is
     *     {@link SpectrumIDFormat }
     *     
     */
    public SpectrumIDFormat getSpectrumIDFormat() {
        return spectrumIDFormat;
    }

    /**
     * Sets the value of the spectrumIDFormat property.
     * 
     * @param value
     *     allowed object is
     *     {@link SpectrumIDFormat }
     *     
     */
    public void setSpectrumIDFormat(SpectrumIDFormat value) {
        this.spectrumIDFormat = value;
    }

}
