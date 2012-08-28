
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import javax.xml.bind.annotation.*;

import uk.ac.ebi.jmzidml.model.MzIdentMLObject;


/**
 * References to the individual component samples within a mixed parent sample. 
 *
 * TODO marshalling/ persistor add validation to check for case where someone gets sample and changes its id without updating ref id in
 *      SubSample and other such clases.
 *
 * NOTE: There is no setter method for the sampleRef. This simplifies keeping the sample object reference and
 * sampleRef synchronized.
 * <p>Java class for SubSampleType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="SubSampleType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;attribute name="sample_ref" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "SubSampleType")
public class SubSample
    extends MzIdentMLObject
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlAttribute(name = "sample_ref", required = true)
    protected String sampleRef;
    @XmlTransient
    protected Sample sample;

    public Sample getSample() {
        return sample;
    }

    public void setSample(Sample sample) {
        if (sample == null) {
            this.sampleRef = null;
        } else {
            String refId = sample.getId();
            if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
            this.sampleRef = refId;
        }
        this.sample = sample;
    }
    /**
     * Gets the value of the sampleRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSampleRef() {
        return sampleRef;
    }

}
