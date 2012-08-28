
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
import uk.ac.ebi.jmzidml.xml.jaxb.adapters.CalendarAdapter;


/**
 * The upper-most hierarchy level of mzIdentML with sub-containers for example describing software, protocols and search results (spectrum identifications or protein detection results). 
 * 
 * <p>Java class for MzIdentMLType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="MzIdentMLType">
 *   &lt;complexContent>
 *     &lt;extension base="{http://psidev.info/psi/pi/mzIdentML/1.1}IdentifiableType">
 *       &lt;sequence>
 *         &lt;element name="cvList" type="{http://psidev.info/psi/pi/mzIdentML/1.1}CVListType"/>
 *         &lt;element name="AnalysisSoftwareList" type="{http://psidev.info/psi/pi/mzIdentML/1.1}AnalysisSoftwareListType" minOccurs="0"/>
 *         &lt;element name="Provider" type="{http://psidev.info/psi/pi/mzIdentML/1.1}ProviderType" minOccurs="0"/>
 *         &lt;element name="AuditCollection" type="{http://psidev.info/psi/pi/mzIdentML/1.1}AuditCollectionType" minOccurs="0"/>
 *         &lt;element name="AnalysisSampleCollection" type="{http://psidev.info/psi/pi/mzIdentML/1.1}AnalysisSampleCollectionType" minOccurs="0"/>
 *         &lt;element name="SequenceCollection" type="{http://psidev.info/psi/pi/mzIdentML/1.1}SequenceCollectionType" minOccurs="0"/>
 *         &lt;element name="AnalysisCollection" type="{http://psidev.info/psi/pi/mzIdentML/1.1}AnalysisCollectionType"/>
 *         &lt;element name="AnalysisProtocolCollection" type="{http://psidev.info/psi/pi/mzIdentML/1.1}AnalysisProtocolCollectionType"/>
 *         &lt;element name="DataCollection" type="{http://psidev.info/psi/pi/mzIdentML/1.1}DataCollectionType"/>
 *         &lt;element name="BibliographicReference" type="{http://psidev.info/psi/pi/mzIdentML/1.1}BibliographicReferenceType" maxOccurs="unbounded" minOccurs="0"/>
 *       &lt;/sequence>
 *       &lt;attribute name="creationDate" type="{http://www.w3.org/2001/XMLSchema}dateTime" />
 *       &lt;attribute name="version" use="required" type="{http://psidev.info/psi/pi/mzIdentML/1.1}versionRegex" />
 *     &lt;/extension>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "MzIdentMLType", propOrder = {
    "cvList",
    "analysisSoftwareList",
    "provider",
    "auditCollection",
    "analysisSampleCollection",
    "sequenceCollection",
    "analysisCollection",
    "analysisProtocolCollection",
    "dataCollection",
    "bibliographicReference"
})
public class MzIdentML
    extends Identifiable
    implements Serializable
{

    private final static long serialVersionUID = 100L;
    @XmlElement(required = true)
    protected CvList cvList;
    @XmlElement(name = "AnalysisSoftwareList")
    protected AnalysisSoftwareList analysisSoftwareList;
    @XmlElement(name = "Provider")
    protected Provider provider;
    @XmlElement(name = "AuditCollection")
    protected AuditCollection auditCollection;
    @XmlElement(name = "AnalysisSampleCollection")
    protected AnalysisSampleCollection analysisSampleCollection;
    @XmlElement(name = "SequenceCollection")
    protected SequenceCollection sequenceCollection;
    @XmlElement(name = "AnalysisCollection", required = true)
    protected AnalysisCollection analysisCollection;
    @XmlElement(name = "AnalysisProtocolCollection", required = true)
    protected AnalysisProtocolCollection analysisProtocolCollection;
    @XmlElement(name = "DataCollection", required = true)
    protected DataCollection dataCollection;
    @XmlElement(name = "BibliographicReference")
    protected List<BibliographicReference> bibliographicReference;
    @XmlAttribute
    @XmlJavaTypeAdapter(CalendarAdapter.class)
    @XmlSchemaType(name = "dateTime")
    protected Calendar creationDate;
    @XmlAttribute(required = true)
    protected String version;

    /**
     * Gets the value of the cvList property.
     * 
     * @return
     *     possible object is
     *     {@link CvList }
     *     
     */
    public CvList getCvList() {
        return cvList;
    }

    /**
     * Sets the value of the cvList property.
     * 
     * @param value
     *     allowed object is
     *     {@link CvList }
     *     
     */
    public void setCvList(CvList value) {
        this.cvList = value;
    }

    /**
     * Gets the value of the analysisSoftwareList property.
     * 
     * @return
     *     possible object is
     *     {@link AnalysisSoftwareList }
     *     
     */
    public AnalysisSoftwareList getAnalysisSoftwareList() {
        return analysisSoftwareList;
    }

    /**
     * Sets the value of the analysisSoftwareList property.
     * 
     * @param value
     *     allowed object is
     *     {@link AnalysisSoftwareList }
     *     
     */
    public void setAnalysisSoftwareList(AnalysisSoftwareList value) {
        this.analysisSoftwareList = value;
    }

    /**
     * Gets the value of the provider property.
     * 
     * @return
     *     possible object is
     *     {@link Provider }
     *     
     */
    public Provider getProvider() {
        return provider;
    }

    /**
     * Sets the value of the provider property.
     * 
     * @param value
     *     allowed object is
     *     {@link Provider }
     *     
     */
    public void setProvider(Provider value) {
        this.provider = value;
    }

    /**
     * Gets the value of the auditCollection property.
     * 
     * @return
     *     possible object is
     *     {@link AuditCollection }
     *     
     */
    public AuditCollection getAuditCollection() {
        return auditCollection;
    }

    /**
     * Sets the value of the auditCollection property.
     * 
     * @param value
     *     allowed object is
     *     {@link AuditCollection }
     *     
     */
    public void setAuditCollection(AuditCollection value) {
        this.auditCollection = value;
    }

    /**
     * Gets the value of the analysisSampleCollection property.
     * 
     * @return
     *     possible object is
     *     {@link AnalysisSampleCollection }
     *     
     */
    public AnalysisSampleCollection getAnalysisSampleCollection() {
        return analysisSampleCollection;
    }

    /**
     * Sets the value of the analysisSampleCollection property.
     * 
     * @param value
     *     allowed object is
     *     {@link AnalysisSampleCollection }
     *     
     */
    public void setAnalysisSampleCollection(AnalysisSampleCollection value) {
        this.analysisSampleCollection = value;
    }

    /**
     * Gets the value of the sequenceCollection property.
     * 
     * @return
     *     possible object is
     *     {@link SequenceCollection }
     *     
     */
    public SequenceCollection getSequenceCollection() {
        return sequenceCollection;
    }

    /**
     * Sets the value of the sequenceCollection property.
     * 
     * @param value
     *     allowed object is
     *     {@link SequenceCollection }
     *     
     */
    public void setSequenceCollection(SequenceCollection value) {
        this.sequenceCollection = value;
    }

    /**
     * Gets the value of the analysisCollection property.
     * 
     * @return
     *     possible object is
     *     {@link AnalysisCollection }
     *     
     */
    public AnalysisCollection getAnalysisCollection() {
        return analysisCollection;
    }

    /**
     * Sets the value of the analysisCollection property.
     * 
     * @param value
     *     allowed object is
     *     {@link AnalysisCollection }
     *     
     */
    public void setAnalysisCollection(AnalysisCollection value) {
        this.analysisCollection = value;
    }

    /**
     * Gets the value of the analysisProtocolCollection property.
     * 
     * @return
     *     possible object is
     *     {@link AnalysisProtocolCollection }
     *     
     */
    public AnalysisProtocolCollection getAnalysisProtocolCollection() {
        return analysisProtocolCollection;
    }

    /**
     * Sets the value of the analysisProtocolCollection property.
     * 
     * @param value
     *     allowed object is
     *     {@link AnalysisProtocolCollection }
     *     
     */
    public void setAnalysisProtocolCollection(AnalysisProtocolCollection value) {
        this.analysisProtocolCollection = value;
    }

    /**
     * Gets the value of the dataCollection property.
     * 
     * @return
     *     possible object is
     *     {@link DataCollection }
     *     
     */
    public DataCollection getDataCollection() {
        return dataCollection;
    }

    /**
     * Sets the value of the dataCollection property.
     * 
     * @param value
     *     allowed object is
     *     {@link DataCollection }
     *     
     */
    public void setDataCollection(DataCollection value) {
        this.dataCollection = value;
    }

    /**
     * Gets the value of the bibliographicReference property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the bibliographicReference property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getBibliographicReference().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link BibliographicReference }
     * 
     * 
     */
    public List<BibliographicReference> getBibliographicReference() {
        if (bibliographicReference == null) {
            bibliographicReference = new ArrayList<BibliographicReference>();
        }
        return this.bibliographicReference;
    }

    /**
     * Gets the value of the creationDate property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public Calendar getCreationDate() {
        return creationDate;
    }

    /**
     * Sets the value of the creationDate property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setCreationDate(Calendar value) {
        this.creationDate = value;
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

}
