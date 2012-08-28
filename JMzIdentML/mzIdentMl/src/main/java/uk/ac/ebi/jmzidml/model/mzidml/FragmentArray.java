
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.*;

import uk.ac.ebi.jmzidml.model.MzIdentMLObject;


/**
 * An array of values for a given type of measure and for a particular ion type, in parallel to the index of ions identified. 
 *
 * TODO marshalling/ persistor add validation to check for case where someone gets measure and changes its id without updating ref id in
 *      FragmentArray and other such clases.
 *
 * NOTE: There is no setter method for the measureRef. This simplifies keeping the measure object reference and
 * measureRef synchronized.
 *
 * <p>Java class for FragmentArrayType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="FragmentArrayType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;attribute name="values" use="required" type="{http://psidev.info/psi/pi/mzIdentML/1.1}listOfFloats" />
 *       &lt;attribute name="measure_ref" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "FragmentArrayType")
public class FragmentArray
    extends MzIdentMLObject
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlAttribute(required = true)
    protected List<Float> values;
    @XmlAttribute(name = "measure_ref", required = true)
    protected String measureRef;
    @XmlTransient
    protected Measure measure;

    /**
     * Gets the value of the values property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the values property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getValues().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Float }
     * 
     * 
     */
    public List<Float> getValues() {
        if (values == null) {
            values = new ArrayList<Float>();
        }
        return this.values;
    }

    /**
     * Gets the value of the measureRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getMeasureRef() {
        return measureRef;
    }

    public Measure getMeasure() {
         return measure;
     }

     public void setMeasure(Measure measure) {
         if (measure == null) {
             this.measureRef = null;
         } else {
             String refId = measure.getId();
             if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
             this.measureRef = refId;
         }
         this.measure = measure;
     }


}
