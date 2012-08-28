
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import javax.xml.bind.annotation.*;

import uk.ac.ebi.jmzidml.model.MzIdentMLObject;


/**
 * The containing organization (the university or business which a lab belongs to, etc.) 
 * 
 * <p>Java class for ParentOrganizationType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ParentOrganizationType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;attribute name="organization_ref" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ParentOrganizationType")
public class ParentOrganization
    extends MzIdentMLObject
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlAttribute(name = "organization_ref", required = true)
    protected String organizationRef;
    @XmlTransient
    protected Organization organization;

    public Organization getOrganization() {
        return organization;
    }

    public void setOrganization(Organization organization) {
        if (organization == null) {
            this.organizationRef = null;
        } else {
            String refId = organization.getId();
            if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
            this.organizationRef = refId;
        }
        this.organization = organization;
    }
    /**
     * Gets the value of the organizationRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getOrganizationRef() {
        return organizationRef;
    }
}
