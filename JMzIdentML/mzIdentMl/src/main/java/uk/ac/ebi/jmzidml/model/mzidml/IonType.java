
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import java.math.BigInteger;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;

import uk.ac.ebi.jmzidml.model.CvParamCapable;
import uk.ac.ebi.jmzidml.model.MzIdentMLObject;


/**
 * IonType defines the index of fragmentation ions being reported, importing a CV term for the type of ion e.g. b ion. Example: if b3 b7 b8 and b10 have been identified, the index attribute will contain 3 7 8 10, and the corresponding values will be reported in parallel arrays below 
 * 
 * <p>Java class for IonTypeType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="IonTypeType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="FragmentArray" type="{http://psidev.info/psi/pi/mzIdentML/1.1}FragmentArrayType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="cvParam" type="{http://psidev.info/psi/pi/mzIdentML/1.1}CVParamType"/>
 *       &lt;/sequence>
 *       &lt;attribute name="index" type="{http://psidev.info/psi/pi/mzIdentML/1.1}listOfIntegers" />
 *       &lt;attribute name="charge" use="required" type="{http://www.w3.org/2001/XMLSchema}int" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "IonTypeType", propOrder = {
    "fragmentArray",
    "cvParam"
})
public class IonType
    extends MzIdentMLObject
    implements Serializable, CvParamCapable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "FragmentArray")
    protected List<FragmentArray> fragmentArray;
    @XmlElement(required = true)
    protected CvParam cvParam;
    @XmlAttribute
    protected List<Integer> index;
    @XmlAttribute(required = true)
    protected int charge;

    /**
     * Gets the value of the fragmentArray property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the fragmentArray property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getFragmentArray().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link FragmentArray }
     * 
     * 
     */
    public List<FragmentArray> getFragmentArray() {
        if (fragmentArray == null) {
            fragmentArray = new ArrayList<FragmentArray>();
        }
        return this.fragmentArray;
    }

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
     * Gets the value of the index property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the index property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getIndex().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Integer }
     * 
     * 
     */
    public List<Integer> getIndex() {
        if (index == null) {
            index = new ArrayList<Integer>();
        }
        return this.index;
    }

    /**
     * Gets the value of the charge property.
     * 
     */
    public int getCharge() {
        return charge;
    }

    /**
     * Sets the value of the charge property.
     * 
     */
    public void setCharge(int value) {
        this.charge = value;
    }

}
