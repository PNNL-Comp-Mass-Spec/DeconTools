
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import javax.xml.bind.annotation.*;

import uk.ac.ebi.jmzidml.model.MzIdentMLObject;


/**
 * The role that a Contact plays in an organization or with respect to the associating class. A Contact may have several Roles within scope, and as such,
 * associations to ContactRole allow the use of a Contact in a certain manner. Examples
 * might include a provider, or a data analyst. 
 *
 * TODO marshalling/ persistor add validation to check for case where someone gets contact and changes its id without updating ref id in
 *      ContactRole and other such clases.
 *
 * NOTE: There is no setter method for the contactRef. This simplifies keeping the contact object reference and
 * contactRef synchronized.
 *
 * <p>Java class for ContactRoleType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ContactRoleType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="Role" type="{http://psidev.info/psi/pi/mzIdentML/1.1}RoleType"/>
 *       &lt;/sequence>
 *       &lt;attribute name="contact_ref" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ContactRoleType", propOrder = {
    "role"
})
public class ContactRole
    extends MzIdentMLObject
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "Role", required = true)
    protected Role role;
    @XmlAttribute(name = "contact_ref", required = true)
    protected String contactRef;

    @XmlTransient
    protected AbstractContact contact;

    public Person getPerson(){
        if(contact != null && contact instanceof Person) return (Person)contact;
        else return null;
    }

    public Organization getOrganization(){
        if(contact != null && contact instanceof Organization) return (Organization)contact;
        else return null;
    }

    /**
     * Gets the value of the role property.
     * 
     * @return
     *     possible object is
     *     {@link Role }
     *     
     */
    public Role getRole() {
        return role;
    }

    /**
     * Sets the value of the role property.
     * 
     * @param value
     *     allowed object is
     *     {@link Role }
     *     
     */
    public void setRole(Role value) {
        this.role = value;
    }

    /**
     * Gets the value of the contactRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getContactRef() {
        return contactRef;
    }

    public AbstractContact getContact() {
        return contact;
    }

    /**
     * Set contact. contactRef is also updated.
     * @param contact
     */
    public void setContact(AbstractContact contact) {
          if (contact == null) {
              this.contactRef = null;
          } else {
              String refId = contact.getId();
              if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
              this.contactRef = refId;
          }
          this.contact = contact;
    }

}
