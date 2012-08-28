
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
 * The software packages used to perform the analyses.
 * 			
 * 
 * <p>Java class for AnalysisSoftwareListType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="AnalysisSoftwareListType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="AnalysisSoftware" type="{http://psidev.info/psi/pi/mzIdentML/1.1}AnalysisSoftwareType" maxOccurs="unbounded"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "AnalysisSoftwareListType", propOrder = {
    "analysisSoftware"
})
public class AnalysisSoftwareList
    extends MzIdentMLObject
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "AnalysisSoftware", required = true)
    protected List<AnalysisSoftware> analysisSoftware;

    /**
     * Gets the value of the analysisSoftware property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the analysisSoftware property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getAnalysisSoftware().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link AnalysisSoftware }
     * 
     * 
     */
    public List<AnalysisSoftware> getAnalysisSoftware() {
        if (analysisSoftware == null) {
            analysisSoftware = new ArrayList<AnalysisSoftware>();
        }
        return this.analysisSoftware;
    }

}
