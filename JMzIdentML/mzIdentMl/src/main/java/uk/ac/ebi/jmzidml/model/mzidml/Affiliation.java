
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import javax.xml.bind.annotation.*;

import uk.ac.ebi.jmzidml.model.MzIdentMLObject;


/**
 * <p>Java class for AffiliationType complex type.
 * TODO marshalling/ persistor add validation to check for case where someone gets organization and changes its id without updating ref id in
 *      affliliations and other such clases.
 *
 * NOTE: There is no setter method for the organizationRef. This simplifies keeping the organization object reference and
 * organizationRef synchronized.
 *
 *
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="AffiliationType">
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
@XmlType(name = "AffiliationType")
public class Affiliation
    extends MzIdentMLObject
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlAttribute(name = "organization_ref", required = true)
    protected String organizationRef;

    @XmlTransient
    protected Organization organization;


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



    /**
     * Get the Organization for this Affiliation
     *
     * @return organization
     */
    public Organization getOrganization() {
        return organization;
    }


    /**
     * Set the Organization for this Affiliation. Update the organizationRef property also.
     *
     * @param organization
     */
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



}
