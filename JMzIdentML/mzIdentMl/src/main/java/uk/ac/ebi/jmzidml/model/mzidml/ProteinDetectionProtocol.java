
package uk.ac.ebi.jmzidml.model.mzidml;

import uk.ac.ebi.jmzidml.model.ParamListCapable;

import java.io.Serializable;
import javax.xml.bind.annotation.*;


/**
 * The parameters and settings of a ProteinDetection process.
 * 
 * <p>Java class for ProteinDetectionProtocolType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ProteinDetectionProtocolType">
 *   &lt;complexContent>
 *     &lt;extension base="{http://psidev.info/psi/pi/mzIdentML/1.1}IdentifiableType">
 *       &lt;sequence>
 *         &lt;element name="AnalysisParams" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ParamListType" minOccurs="0"/>
 *         &lt;element name="Threshold" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ParamListType"/>
 *       &lt;/sequence>
 *       &lt;attribute name="analysisSoftware_ref" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ProteinDetectionProtocolType", propOrder = {
    "analysisParams",
    "threshold"
})
public class ProteinDetectionProtocol
    extends Identifiable
    implements Serializable, ParamListCapable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "AnalysisParams")
    protected ParamList analysisParams;
    @XmlElement(name = "Threshold", required = true)
    protected ParamList threshold;
    @XmlAttribute(name = "analysisSoftware_ref", required = true)
    protected String analysisSoftwareRef;

    @XmlTransient
    protected AnalysisSoftware analysisSoftware;

    /**
     * Gets the value of the analysisParams property.
     * 
     * @return
     *     possible object is
     *     {@link ParamList }
     *     
     */
    public ParamList getAnalysisParams() {
        return analysisParams;
    }

    /**
     * Sets the value of the analysisParams property.
     * 
     * @param value
     *     allowed object is
     *     {@link ParamList }
     *     
     */
    public void setAnalysisParams(ParamList value) {
        this.analysisParams = value;
    }

    /**
     * Gets the value of the threshold property.
     * 
     * @return
     *     possible object is
     *     {@link ParamList }
     *     
     */
    public ParamList getThreshold() {
        return threshold;
    }

    /**
     * Sets the value of the threshold property.
     * 
     * @param value
     *     allowed object is
     *     {@link ParamList }
     *     
     */
    public void setThreshold(ParamList value) {
        this.threshold = value;
    }

    /**
     * Gets the value of the analysisSoftwareRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getAnalysisSoftwareRef() {
        return analysisSoftwareRef;
    }


    public AnalysisSoftware getAnalysisSoftware() {
        return analysisSoftware;
    }

    public void setAnalysisSoftware(AnalysisSoftware analysisSoftware) {
        if (analysisSoftware == null) {
            this.analysisSoftwareRef = null;
        } else {
            String refId = analysisSoftware.getId();
            if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
            this.analysisSoftwareRef = refId;
        }
        this.analysisSoftware = analysisSoftware;
    }


}
