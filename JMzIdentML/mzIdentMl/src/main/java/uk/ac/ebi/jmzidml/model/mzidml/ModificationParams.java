
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;
import uk.ac.ebi.jmzidml.model.MzIdentMLObject;


/**
 * The specification of static/variable modifications (e.g. Oxidation of Methionine) that are to be considered in the spectra search. 
 * 
 * <p>Java class for ModificationParamsType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="ModificationParamsType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="SearchModification" type="{http://psidev.info/psi/pi/mzIdentML/1.1}SearchModificationType" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "ModificationParamsType", propOrder = {
    "searchModification"
})
public class ModificationParams
    extends MzIdentMLObject
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "SearchModification", required = true)
    protected List<SearchModification> searchModification;

    /**
     * Gets the value of the searchModification property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the searchModification property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getSearchModification().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link SearchModification }
     * 
     * 
     */
    public List<SearchModification> getSearchModification() {
        if (searchModification == null) {
            searchModification = new ArrayList<SearchModification>();
        }
        return this.searchModification;
    }

}
