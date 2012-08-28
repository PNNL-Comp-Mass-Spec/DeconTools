
package uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * <p>Java class for anonymous complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType>
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element ref="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}separationTechnique" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {
    "separationTechnique"
})
public class Separation
    implements Serializable, MzXMLObject
{

    private final static long serialVersionUID = 320L;
    @XmlElement(required = true)
    protected List<SeparationTechnique> separationTechnique;

    /**
     * Gets the value of the separationTechnique property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the separationTechnique property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getSeparationTechnique().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link SeparationTechnique }
     * 
     * 
     */
    public List<SeparationTechnique> getSeparationTechnique() {
        if (separationTechnique == null) {
            separationTechnique = new ArrayList<SeparationTechnique>();
        }
        return this.separationTechnique;
    }

}
