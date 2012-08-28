
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * A person's name and contact details. Any additional information such as the address, contact email etc. should be supplied using CV parameters or user parameters.
 * 
 * <p>Java class for PersonType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="PersonType">
 *   &lt;complexContent>
 *     &lt;extension base="{http://psidev.info/psi/pi/mzIdentML/1.1}AbstractContactType">
 *       &lt;sequence>
 *         &lt;element name="Affiliation" type="{http://psidev.info/psi/pi/mzIdentML/1.1}AffiliationType" maxOccurs="unbounded" minOccurs="0"/>
 *       &lt;/sequence>
 *       &lt;attribute name="lastName" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="firstName" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="midInitials" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "PersonType", propOrder = {
    "affiliation"
})
public class Person
    extends AbstractContact
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "Affiliation")
    protected List<Affiliation> affiliation;
    @XmlAttribute
    protected String lastName;
    @XmlAttribute
    protected String firstName;
    @XmlAttribute
    protected String midInitials;

    /**
     * Gets the value of the affiliation property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the affiliation property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getAffiliation().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Affiliation }
     * 
     * 
     */
    public List<Affiliation> getAffiliation() {
        if (affiliation == null) {
            affiliation = new ArrayList<Affiliation>();
        }
        return this.affiliation;
    }

    /**
     * Gets the value of the lastName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getLastName() {
        return lastName;
    }

    /**
     * Sets the value of the lastName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setLastName(String value) {
        this.lastName = value;
    }

    /**
     * Gets the value of the firstName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getFirstName() {
        return firstName;
    }

    /**
     * Sets the value of the firstName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setFirstName(String value) {
        this.firstName = value;
    }

    /**
     * Gets the value of the midInitials property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getMidInitials() {
        return midInitials;
    }

    /**
     * Sets the value of the midInitials property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setMidInitials(String value) {
        this.midInitials = value;
    }

}
