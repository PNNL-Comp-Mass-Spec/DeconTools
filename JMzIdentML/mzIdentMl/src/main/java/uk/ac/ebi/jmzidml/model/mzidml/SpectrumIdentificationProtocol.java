
package uk.ac.ebi.jmzidml.model.mzidml;

import uk.ac.ebi.jmzidml.model.ParamCapable;
import uk.ac.ebi.jmzidml.model.ParamListCapable;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.*;


/**
 * The parameters and settings of a SpectrumIdentification analysis.
 * 
 * <p>Java class for SpectrumIdentificationProtocolType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="SpectrumIdentificationProtocolType">
 *   &lt;complexContent>
 *     &lt;extension base="{http://psidev.info/psi/pi/mzIdentML/1.1}IdentifiableType">
 *       &lt;sequence>
 *         &lt;element name="SearchType" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ParamType"/>
 *         &lt;element name="AdditionalSearchParams" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ParamListType" minOccurs="0"/>
 *         &lt;element name="ModificationParams" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ModificationParamsType" minOccurs="0"/>
 *         &lt;element name="Enzymes" type="{http://psidev.info/psi/pi/mzIdentML/1.1}EnzymesType" minOccurs="0"/>
 *         &lt;element name="MassTable" type="{http://psidev.info/psi/pi/mzIdentML/1.1}MassTableType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="FragmentTolerance" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ToleranceType" minOccurs="0"/>
 *         &lt;element name="ParentTolerance" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ToleranceType" minOccurs="0"/>
 *         &lt;element name="Threshold" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ParamListType"/>
 *         &lt;element name="DatabaseFilters" type="{http://psidev.info/psi/pi/mzIdentML/1.1}DatabaseFiltersType" minOccurs="0"/>
 *         &lt;element name="DatabaseTranslation" type="{http://psidev.info/psi/pi/mzIdentML/1.1}DatabaseTranslationType" minOccurs="0"/>
 *       &lt;/sequence>
 *       &lt;attribute name="analysisSoftware_ref" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "SpectrumIdentificationProtocolType", propOrder = {
    "searchType",
    "additionalSearchParams",
    "modificationParams",
    "enzymes",
    "massTable",
    "fragmentTolerance",
    "parentTolerance",
    "threshold",
    "databaseFilters",
    "databaseTranslation"
})
public class SpectrumIdentificationProtocol
    extends Identifiable
    implements Serializable, ParamListCapable, ParamCapable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(name = "SearchType", required = true)
    protected Param searchType;
    @XmlElement(name = "AdditionalSearchParams")
    protected ParamList additionalSearchParams;
    @XmlElement(name = "ModificationParams")
    protected ModificationParams modificationParams;
    @XmlElement(name = "Enzymes")
    protected Enzymes enzymes;
    @XmlElement(name = "MassTable")
    protected List<MassTable> massTable;
    @XmlElement(name = "FragmentTolerance")
    protected Tolerance fragmentTolerance;
    @XmlElement(name = "ParentTolerance")
    protected Tolerance parentTolerance;
    @XmlElement(name = "Threshold", required = true)
    protected ParamList threshold;
    @XmlElement(name = "DatabaseFilters")
    protected DatabaseFilters databaseFilters;
    @XmlElement(name = "DatabaseTranslation")
    protected DatabaseTranslation databaseTranslation;
    @XmlAttribute(name = "analysisSoftware_ref", required = true)
    protected String analysisSoftwareRef;
    @XmlTransient
    protected AnalysisSoftware analysisSoftware;

    public AnalysisSoftware getAnalysisSoftware() {
        return analysisSoftware;
    }

    public void setAnalysisSoftware(AnalysisSoftware analysisSoftware) {
        if (analysisSoftware == null) {
            this.analysisSoftwareRef = null;
        } else {
            String refId = analysisSoftware.getId();
            if (refId == null) throw new IllegalArgumentException("Referenced object does not have an identifier.");
            this.analysisSoftwareRef = refId;
        }
        this.analysisSoftware = analysisSoftware;
    }

    /**
     * Gets the value of the searchType property.
     * 
     * @return
     *     possible object is
     *     {@link Param }
     *     
     */
    public Param getSearchType() {
        return searchType;
    }

    /**
     * Sets the value of the searchType property.
     * 
     * @param value
     *     allowed object is
     *     {@link Param }
     *     
     */
    public void setSearchType(Param value) {
        this.searchType = value;
    }

    /**
     * Gets the value of the additionalSearchParams property.
     * 
     * @return
     *     possible object is
     *     {@link ParamList }
     *     
     */
    public ParamList getAdditionalSearchParams() {
        return additionalSearchParams;
    }

    /**
     * Sets the value of the additionalSearchParams property.
     * 
     * @param value
     *     allowed object is
     *     {@link ParamList }
     *     
     */
    public void setAdditionalSearchParams(ParamList value) {
        this.additionalSearchParams = value;
    }

    /**
     * Gets the value of the modificationParams property.
     * 
     * @return
     *     possible object is
     *     {@link ModificationParams }
     *     
     */
    public ModificationParams getModificationParams() {
        return modificationParams;
    }

    /**
     * Sets the value of the modificationParams property.
     * 
     * @param value
     *     allowed object is
     *     {@link ModificationParams }
     *     
     */
    public void setModificationParams(ModificationParams value) {
        this.modificationParams = value;
    }

    /**
     * Gets the value of the enzymes property.
     * 
     * @return
     *     possible object is
     *     {@link Enzymes }
     *     
     */
    public Enzymes getEnzymes() {
        return enzymes;
    }

    /**
     * Sets the value of the enzymes property.
     * 
     * @param value
     *     allowed object is
     *     {@link Enzymes }
     *     
     */
    public void setEnzymes(Enzymes value) {
        this.enzymes = value;
    }

    /**
     * Gets the value of the massTable property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the massTable property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getMassTable().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link MassTable }
     * 
     * 
     */
    public List<MassTable> getMassTable() {
        if (massTable == null) {
            massTable = new ArrayList<MassTable>();
        }
        return this.massTable;
    }

    /**
     * Gets the value of the fragmentTolerance property.
     * 
     * @return
     *     possible object is
     *     {@link Tolerance }
     *     
     */
    public Tolerance getFragmentTolerance() {
        return fragmentTolerance;
    }

    /**
     * Sets the value of the fragmentTolerance property.
     * 
     * @param value
     *     allowed object is
     *     {@link Tolerance }
     *     
     */
    public void setFragmentTolerance(Tolerance value) {
        this.fragmentTolerance = value;
    }

    /**
     * Gets the value of the parentTolerance property.
     * 
     * @return
     *     possible object is
     *     {@link Tolerance }
     *     
     */
    public Tolerance getParentTolerance() {
        return parentTolerance;
    }

    /**
     * Sets the value of the parentTolerance property.
     * 
     * @param value
     *     allowed object is
     *     {@link Tolerance }
     *     
     */
    public void setParentTolerance(Tolerance value) {
        this.parentTolerance = value;
    }

    /**
     * Gets the value of the threshold property.
     * 
     * @return
     *     possible object is
     *     {@link ParamList }
     *     
     */
    public ParamList getThreshold() {
        return threshold;
    }

    /**
     * Sets the value of the threshold property.
     * 
     * @param value
     *     allowed object is
     *     {@link ParamList }
     *     
     */
    public void setThreshold(ParamList value) {
        this.threshold = value;
    }

    /**
     * Gets the value of the databaseFilters property.
     * 
     * @return
     *     possible object is
     *     {@link DatabaseFilters }
     *     
     */
    public DatabaseFilters getDatabaseFilters() {
        return databaseFilters;
    }

    /**
     * Sets the value of the databaseFilters property.
     * 
     * @param value
     *     allowed object is
     *     {@link DatabaseFilters }
     *     
     */
    public void setDatabaseFilters(DatabaseFilters value) {
        this.databaseFilters = value;
    }

    /**
     * Gets the value of the databaseTranslation property.
     * 
     * @return
     *     possible object is
     *     {@link DatabaseTranslation }
     *     
     */
    public DatabaseTranslation getDatabaseTranslation() {
        return databaseTranslation;
    }

    /**
     * Sets the value of the databaseTranslation property.
     * 
     * @param value
     *     allowed object is
     *     {@link DatabaseTranslation }
     *     
     */
    public void setDatabaseTranslation(DatabaseTranslation value) {
        this.databaseTranslation = value;
    }

    /**
     * Gets the value of the analysisSoftwareRef property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getAnalysisSoftwareRef() {
        return analysisSoftwareRef;
    }
}
