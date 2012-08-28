
package uk.ac.ebi.jmzidml.model.mzidml;

import uk.ac.ebi.jmzidml.model.ParamCapable;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlSchemaType;
import javax.xml.bind.annotation.XmlType;


/**
 * The software used for performing the analyses.
 * 
 * <p>Java class for AnalysisSoftwareType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="AnalysisSoftwareType">
 *   &lt;complexContent>
 *     &lt;extension base="{http://psidev.info/psi/pi/mzIdentML/1.1}IdentifiableType">
 *       &lt;sequence>
 *         &lt;element name="ContactRole" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ContactRoleType" minOccurs="0"/>
 *         &lt;element name="SoftwareName" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ParamType"/>
 *         &lt;element name="Customizations" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *       &lt;/sequence>
 *       &lt;attribute name="version" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="uri" type="{http://www.w3.org/2001/XMLSchema}anyURI" />
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "AnalysisSoftwareType", propOrder = {
    "contactRole",
    "softwareName",
    "customizations"
})
public class AnalysisSoftware
    extends Identifiable
    implements Serializable, ParamCapable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "ContactRole")
    protected ContactRole contactRole;
    @XmlElement(name = "SoftwareName", required = true)
    protected Param softwareName;
    @XmlElement(name = "Customizations")
    protected String customizations;
    @XmlAttribute
    protected String version;
    @XmlAttribute
    @XmlSchemaType(name = "anyURI")
    protected String uri;

    /**
     * Gets the value of the contactRole property.
     * 
     * @return
     *     possible object is
     *     {@link ContactRole }
     *     
     */
    public ContactRole getContactRole() {
        return contactRole;
    }

    /**
     * Sets the value of the contactRole property.
     * 
     * @param value
     *     allowed object is
     *     {@link ContactRole }
     *     
     */
    public void setContactRole(ContactRole value) {
        this.contactRole = value;
    }

    /**
     * Gets the value of the softwareName property.
     * 
     * @return
     *     possible object is
     *     {@link Param }
     *     
     */
    public Param getSoftwareName() {
        return softwareName;
    }

    /**
     * Sets the value of the softwareName property.
     * 
     * @param value
     *     allowed object is
     *     {@link Param }
     *     
     */
    public void setSoftwareName(Param value) {
        this.softwareName = value;
    }

    /**
     * Gets the value of the customizations property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getCustomizations() {
        return customizations;
    }

    /**
     * Sets the value of the customizations property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setCustomizations(String value) {
        this.customizations = value;
    }

    /**
     * Gets the value of the version property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getVersion() {
        return version;
    }

    /**
     * Sets the value of the version property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setVersion(String value) {
        this.version = value;
    }

    /**
     * Gets the value of the uri property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getUri() {
        return uri;
    }

    /**
     * Sets the value of the uri property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setUri(String value) {
        this.uri = value;
    }

}
