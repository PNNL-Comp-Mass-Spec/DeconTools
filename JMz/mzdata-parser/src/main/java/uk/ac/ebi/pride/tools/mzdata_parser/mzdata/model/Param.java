
package uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlElements;
import javax.xml.bind.annotation.XmlSeeAlso;
import javax.xml.bind.annotation.XmlType;


/**
 * Structure allowing the use of controlled or uncontrolled vocabulary
 * 
 * <p>Java class for paramType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="paramType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;choice maxOccurs="unbounded" minOccurs="0">
 *         &lt;element name="cvParam" type="{}cvParamType"/>
 *         &lt;element name="userParam" type="{}userParamType"/>
 *       &lt;/choice>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "paramType", propOrder = {
    "cvParamOrUserParam"
})
@XmlSeeAlso({
    Description.class,
    uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.SpectrumSettings.AcqSpecification.Acquisition.class,
    uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.SpectrumSettings.SpectrumInstrument.class
})
public class Param
    implements Serializable, MzDataObject
{

    private final static long serialVersionUID = 105L;
    @XmlElements({
        @XmlElement(name = "userParam", type = UserParam.class),
        @XmlElement(name = "cvParam", type = CvParam.class)
    })
    protected List<MzDataObject> cvParamOrUserParam;

    /**
     * Gets the value of the cvParamOrUserParam property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the cvParamOrUserParam property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getCvParamOrUserParam().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link UserParam }
     * {@link CvParam }
     * 
     * 
     */
    public List<MzDataObject> getCvParamOrUserParam() {
        if (cvParamOrUserParam == null) {
            cvParamOrUserParam = new ArrayList<MzDataObject>();
        }
        return this.cvParamOrUserParam;
    }

    public List<CvParam> getCvParams() {
    	if (cvParamOrUserParam == null)
    		return Collections.emptyList();
    	
    	List<CvParam> cvParams = new ArrayList<CvParam>();
    	
    	for (MzDataObject param : cvParamOrUserParam) {
    		if (param instanceof CvParam) {
    			cvParams.add((CvParam) param);
    		}
    	}
    	
    	return cvParams;
    }
    
    public List<UserParam> getUserParams() {
    	if (cvParamOrUserParam == null)
    		return Collections.emptyList();
    	
    	List<UserParam> userParams = new ArrayList<UserParam>();
    	
    	for (MzDataObject param : cvParamOrUserParam) {
    		if (param instanceof UserParam) {
    			userParams.add((UserParam) param);
    		}
    	}
    	
    	return userParams;
    }
}
