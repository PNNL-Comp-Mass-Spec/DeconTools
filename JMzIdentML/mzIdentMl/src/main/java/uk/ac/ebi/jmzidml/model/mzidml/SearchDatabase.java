
package uk.ac.ebi.jmzidml.model.mzidml;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlSchemaType;
import javax.xml.bind.annotation.XmlType;
import javax.xml.bind.annotation.adapters.XmlJavaTypeAdapter;

import uk.ac.ebi.jmzidml.model.CvParamListCapable;
import uk.ac.ebi.jmzidml.model.ParamCapable;
import uk.ac.ebi.jmzidml.model.ParamGroupCapable;
import uk.ac.ebi.jmzidml.xml.jaxb.adapters.CalendarAdapter;


/**
 * A database for searching mass spectra. Examples include a set of amino acid sequence entries, or annotated spectra libraries. 
 * 
 * <p>Java class for SearchDatabaseType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="SearchDatabaseType">
 *   &lt;complexContent>
 *     &lt;extension base="{http://psidev.info/psi/pi/mzIdentML/1.1}ExternalDataType">
 *       &lt;sequence>
 *         &lt;element name="DatabaseName" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ParamType"/>
 *         &lt;element name="cvParam" type="{http://psidev.info/psi/pi/mzIdentML/1.1}CVParamType" maxOccurs="unbounded" minOccurs="0"/>
 *       &lt;/sequence>
 *       &lt;attribute name="version" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="releaseDate" type="{http://www.w3.org/2001/XMLSchema}dateTime" />
 *       &lt;attribute name="numDatabaseSequences" type="{http://www.w3.org/2001/XMLSchema}long" />
 *       &lt;attribute name="numResidues" type="{http://www.w3.org/2001/XMLSchema}long" />
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "SearchDatabaseType", propOrder = {
    "databaseName",
    "cvParam"
})
public class SearchDatabase
    extends ExternalData
    implements Serializable, ParamCapable, CvParamListCapable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "DatabaseName", required = true)
    protected Param databaseName;
    protected List<CvParam> cvParam;
    @XmlAttribute
    protected String version;
    @XmlAttribute
    @XmlJavaTypeAdapter(CalendarAdapter.class)
    @XmlSchemaType(name = "dateTime")
    protected Calendar releaseDate;
    @XmlAttribute
    protected Long numDatabaseSequences;
    @XmlAttribute
    protected Long numResidues;

    /**
     * Gets the value of the databaseName property.
     * 
     * @return
     *     possible object is
     *     {@link Param }
     *     
     */
    public Param getDatabaseName() {
        return databaseName;
    }

    /**
     * Sets the value of the databaseName property.
     * 
     * @param value
     *     allowed object is
     *     {@link Param }
     *     
     */
    public void setDatabaseName(Param value) {
        this.databaseName = value;
    }

    /**
     * Gets the value of the cvParam property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the cvParam property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getCvParam().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link CvParam }
     * 
     * 
     */
    public List<CvParam> getCvParam() {
        if (cvParam == null) {
            cvParam = new ArrayList<CvParam>();
        }
        return this.cvParam;
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
     * Gets the value of the releaseDate property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public Calendar getReleaseDate() {
        return releaseDate;
    }

    /**
     * Sets the value of the releaseDate property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setReleaseDate(Calendar value) {
        this.releaseDate = value;
    }

    /**
     * Gets the value of the numDatabaseSequences property.
     * 
     * @return
     *     possible object is
     *     {@link Long }
     *     
     */
    public Long getNumDatabaseSequences() {
        return numDatabaseSequences;
    }

    /**
     * Sets the value of the numDatabaseSequences property.
     * 
     * @param value
     *     allowed object is
     *     {@link Long }
     *     
     */
    public void setNumDatabaseSequences(Long value) {
        this.numDatabaseSequences = value;
    }

    /**
     * Gets the value of the numResidues property.
     * 
     * @return
     *     possible object is
     *     {@link Long }
     *     
     */
    public Long getNumResidues() {
        return numResidues;
    }

    /**
     * Sets the value of the numResidues property.
     * 
     * @param value
     *     allowed object is
     *     {@link Long }
     *     
     */
    public void setNumResidues(Long value) {
        this.numResidues = value;
    }

}
