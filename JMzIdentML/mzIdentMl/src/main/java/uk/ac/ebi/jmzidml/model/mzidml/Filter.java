
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;
import uk.ac.ebi.jmzidml.model.MzIdentMLObject;
import uk.ac.ebi.jmzidml.model.ParamCapable;
import uk.ac.ebi.jmzidml.model.ParamListCapable;


/**
 * Filters applied to the search database. The filter must include at least one of Include and Exclude. If both are used, it is assumed that inclusion is performed first. 
 * 
 * <p>Java class for FilterType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="FilterType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="FilterType" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ParamType"/>
 *         &lt;element name="Include" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ParamListType" minOccurs="0"/>
 *         &lt;element name="Exclude" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ParamListType" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "FilterType", propOrder = {
    "filterType",
    "include",
    "exclude"
})
public class Filter
    extends MzIdentMLObject
    implements Serializable, ParamListCapable, ParamCapable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "FilterType", required = true)
    protected Param filterType;
    @XmlElement(name = "Include")
    protected ParamList include;
    @XmlElement(name = "Exclude")
    protected ParamList exclude;

    /**
     * Gets the value of the filterType property.
     * 
     * @return
     *     possible object is
     *     {@link Param }
     *     
     */
    public Param getFilterType() {
        return filterType;
    }

    /**
     * Sets the value of the filterType property.
     * 
     * @param value
     *     allowed object is
     *     {@link Param }
     *     
     */
    public void setFilterType(Param value) {
        this.filterType = value;
    }

    /**
     * Gets the value of the include property.
     * 
     * @return
     *     possible object is
     *     {@link ParamList }
     *     
     */
    public ParamList getInclude() {
        return include;
    }

    /**
     * Sets the value of the include property.
     * 
     * @param value
     *     allowed object is
     *     {@link ParamList }
     *     
     */
    public void setInclude(ParamList value) {
        this.include = value;
    }

    /**
     * Gets the value of the exclude property.
     * 
     * @return
     *     possible object is
     *     {@link ParamList }
     *     
     */
    public ParamList getExclude() {
        return exclude;
    }

    /**
     * Sets the value of the exclude property.
     * 
     * @param value
     *     allowed object is
     *     {@link ParamList }
     *     
     */
    public void setExclude(ParamList value) {
        this.exclude = value;
    }

}
