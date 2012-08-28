
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * Organizations are entities like companies, universities, government agencies. Any additional information such as the address, email etc. should be supplied either as CV parameters or as user parameters. 
 * 
 * <p>Java class for OrganizationType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="OrganizationType">
 *   &lt;complexContent>
 *     &lt;extension base="{http://psidev.info/psi/pi/mzIdentML/1.1}AbstractContactType">
 *       &lt;sequence>
 *         &lt;element name="Parent" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ParentOrganizationType" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "OrganizationType", propOrder = {
    "parent"
})
public class Organization
    extends AbstractContact
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "Parent")
    protected ParentOrganization parent;

    /**
     * Gets the value of the parent property.
     * 
     * @return
     *     possible object is
     *     {@link ParentOrganization }
     *     
     */
    public ParentOrganization getParent() {
        return parent;
    }

    /**
     * Sets the value of the parent property.
     * 
     * @param value
     *     allowed object is
     *     {@link ParentOrganization }
     *     
     */
    public void setParent(ParentOrganization value) {
        this.parent = value;
    }

}
