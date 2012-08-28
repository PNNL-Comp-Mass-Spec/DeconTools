
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;
import uk.ac.ebi.jmzidml.model.MzIdentMLObject;


/**
 * Specification of a search modification as parameter for a spectra search. Contains the name of the modification, the mass, the specificity and whether it is a static modification. 
 * 
 * <p>Java class for SearchModificationType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="SearchModificationType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="SpecificityRules" type="{http://psidev.info/psi/pi/mzIdentML/1.1}SpecificityRulesType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="cvParam" type="{http://psidev.info/psi/pi/mzIdentML/1.1}CVParamType" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *       &lt;attribute name="fixedMod" use="required" type="{http://www.w3.org/2001/XMLSchema}boolean" />
 *       &lt;attribute name="massDelta" use="required" type="{http://www.w3.org/2001/XMLSchema}float" />
 *       &lt;attribute name="residues" use="required" type="{http://psidev.info/psi/pi/mzIdentML/1.1}listOfCharsOrAny" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "SearchModificationType", propOrder = {
    "specificityRules",
    "cvParam"
})
public class SearchModification
    extends MzIdentMLObject
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "SpecificityRules")
    protected List<SpecificityRules> specificityRules;
    @XmlElement(required = true)
    protected List<CvParam> cvParam;
    @XmlAttribute(required = true)
    protected boolean fixedMod;
    @XmlAttribute(required = true)
    protected float massDelta;
    @XmlAttribute(required = true)
    protected List<String> residues;

    /**
     * Gets the value of the specificityRules property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the specificityRules property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getSpecificityRules().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link SpecificityRules }
     * 
     * 
     */
    public List<SpecificityRules> getSpecificityRules() {
        if (specificityRules == null) {
            specificityRules = new ArrayList<SpecificityRules>();
        }
        return this.specificityRules;
    }

    /**
     * Gets the value of the cvParam property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the cvParam property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getCvParam().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link CvParam }
     * 
     * 
     */
    public List<CvParam> getCvParam() {
        if (cvParam == null) {
            cvParam = new ArrayList<CvParam>();
        }
        return this.cvParam;
    }

    /**
     * Gets the value of the fixedMod property.
     * 
     */
    public boolean isFixedMod() {
        return fixedMod;
    }

    /**
     * Sets the value of the fixedMod property.
     * 
     */
    public void setFixedMod(boolean value) {
        this.fixedMod = value;
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
