
package uk.ac.ebi.jmzidml.model.mzidml;

import uk.ac.ebi.jmzidml.model.CvParamCapable;
import uk.ac.ebi.jmzidml.model.MzIdentMLObject;

import javax.xml.bind.annotation.*;
import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;


/**
 * The modification searched for, sourced from e.g. UniMod and the mass delta
 *             
 * 
 * <p>Java class for ModParamType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ModParamType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="cvParam" type="{http://psidev.info/psi/pi/mzIdentML/1.1}cvParamType"/>
 *       &lt;/sequence>
 *       &lt;attribute name="massDelta" use="required" type="{http://www.w3.org/2001/XMLSchema}float" />
 *       &lt;attribute name="residues" use="required" type="{http://psidev.info/psi/pi/mzIdentML/1.1}listOfChars" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ModParamType", propOrder = {
    "cvParam"
})
public class ModParam
    extends MzIdentMLObject
    implements Serializable, CvParamCapable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(required = true)
    protected CvParam cvParam;
    @XmlAttribute(required = true)
    protected float massDelta;
    @XmlAttribute(required = true)
    protected List<String> residues;

    /**
     * Gets the value of the cvParam property.
     * 
     * @return
     *     possible object is
     *     {@link CvParam }
     *     
     */
    public CvParam getCvParam() {
        return cvParam;
    }

    /**
     * Sets the value of the cvParam property.
     * 
     * @param value
     *     allowed object is
     *     {@link CvParam }
     *     
     */
    public void setCvParam(CvParam value) {
        this.cvParam = value;
    }

    /**
     * Gets the value of the massDelta property.
     * 
     */
    public float getMassDelta() {
        return massDelta;
    }

    /**
     * Sets the value of the massDelta property.
     * 
     */
    public void setMassDelta(float value) {
        this.massDelta = value;
    }

    /**
     * Gets the value of the residues property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the residues property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getResidues().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link String }
     * 
     * 
     */
    public List<String> getResidues() {
        if (residues == null) {
            residues = new ArrayList<String>();
        }
        return this.residues;
    }

}
