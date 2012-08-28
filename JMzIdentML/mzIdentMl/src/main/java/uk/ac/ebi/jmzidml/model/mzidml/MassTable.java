
package uk.ac.ebi.jmzidml.model.mzidml;

import uk.ac.ebi.jmzidml.model.ParamGroupCapable;
import uk.ac.ebi.jmzidml.model.utils.FacadeList;

import java.io.Serializable;
import java.math.BigInteger;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlElements;
import javax.xml.bind.annotation.XmlType;


/**
 * The masses of residues used in the search.
 * 
 * <p>Java class for MassTableType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="MassTableType">
 *   &lt;complexContent>
 *     &lt;extension base="{http://psidev.info/psi/pi/mzIdentML/1.1}IdentifiableType">
 *       &lt;sequence>
 *         &lt;element name="Residue" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ResidueType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="AmbiguousResidue" type="{http://psidev.info/psi/pi/mzIdentML/1.1}AmbiguousResidueType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;group ref="{http://psidev.info/psi/pi/mzIdentML/1.1}ParamGroup" maxOccurs="unbounded" minOccurs="0"/>
 *       &lt;/sequence>
 *       &lt;attribute name="msLevel" use="required" type="{http://psidev.info/psi/pi/mzIdentML/1.1}listOfIntegers" />
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "MassTableType", propOrder = {
    "residue",
    "ambiguousResidue",
    "paramGroup"
})
public class MassTable
    extends Identifiable
    implements Serializable, ParamGroupCapable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "Residue")
    protected List<Residue> residue;
    @XmlElement(name = "AmbiguousResidue")
    protected List<AmbiguousResidue> ambiguousResidue;
    @XmlElements({
        @XmlElement(name = "cvParam", type = CvParam.class),
        @XmlElement(name = "userParam", type = UserParam.class)
    })
    protected List<AbstractParam> paramGroup;
    @XmlAttribute(required = true)
    protected List<Integer> msLevel;

    /**
     * Gets the value of the residue property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the residue property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getResidue().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Residue }
     * 
     * 
     */
    public List<Residue> getResidue() {
        if (residue == null) {
            residue = new ArrayList<Residue>();
        }
        return this.residue;
    }

    /**
     * Gets the value of the ambiguousResidue property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the ambiguousResidue property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getAmbiguousResidue().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link AmbiguousResidue }
     * 
     * 
     */
    public List<AmbiguousResidue> getAmbiguousResidue() {
        if (ambiguousResidue == null) {
            ambiguousResidue = new ArrayList<AmbiguousResidue>();
        }
        return this.ambiguousResidue;
    }

    /**
     * Additional parameters or descriptors for the MassTable.Gets the value of the paramGroup property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the paramGroup property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getParamGroup().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link CvParam }
     * {@link UserParam }
     * 
     * 
     */
    public List<AbstractParam> getParamGroup() {
        if (paramGroup == null) {
            paramGroup = new ArrayList<AbstractParam>();
        }
        return this.paramGroup;
    }

    /**
     * Gets the value of the msLevel property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the msLevel property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getMsLevel().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Integer }
     * 
     * 
     */
    public List<Integer> getMsLevel() {
        if (msLevel == null) {
            msLevel = new ArrayList<Integer>();
        }
        return this.msLevel;
    }

    public List<CvParam> getCvParam() {
        return new FacadeList<CvParam>(this.getParamGroup(), CvParam.class);
    }

    public List<UserParam> getUserParam() {
        return new FacadeList<UserParam>(this.getParamGroup(), UserParam.class);
    }
}
