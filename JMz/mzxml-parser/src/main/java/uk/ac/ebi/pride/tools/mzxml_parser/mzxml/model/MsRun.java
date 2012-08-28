
package uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
import javax.xml.bind.annotation.XmlSchemaType;
import javax.xml.bind.annotation.XmlType;
import javax.xml.bind.annotation.adapters.XmlJavaTypeAdapter;
import javax.xml.datatype.Duration;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.xml.util.NonNegativeIntegerAdapter;


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
 *         &lt;element name="parentFile" maxOccurs="unbounded">
 *           &lt;complexType>
 *             &lt;simpleContent>
 *               &lt;extension base="&lt;http://www.w3.org/2001/XMLSchema>anySimpleType">
 *                 &lt;attribute name="fileName" use="required" type="{http://www.w3.org/2001/XMLSchema}anyURI" />
 *                 &lt;attribute name="fileType" use="required">
 *                   &lt;simpleType>
 *                     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *                       &lt;enumeration value="RAWData"/>
 *                       &lt;enumeration value="processedData"/>
 *                     &lt;/restriction>
 *                   &lt;/simpleType>
 *                 &lt;/attribute>
 *                 &lt;attribute name="fileSha1" use="required">
 *                   &lt;simpleType>
 *                     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *                       &lt;length value="40"/>
 *                     &lt;/restriction>
 *                   &lt;/simpleType>
 *                 &lt;/attribute>
 *               &lt;/extension>
 *             &lt;/simpleContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element name="msInstrument" maxOccurs="unbounded" minOccurs="0">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 &lt;sequence>
 *                   &lt;element name="msManufacturer">
 *                     &lt;complexType>
 *                       &lt;complexContent>
 *                         &lt;extension base="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType">
 *                         &lt;/extension>
 *                       &lt;/complexContent>
 *                     &lt;/complexType>
 *                   &lt;/element>
 *                   &lt;element name="msModel" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *                   &lt;element name="msIonisation" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *                   &lt;element name="msMassAnalyzer">
 *                     &lt;complexType>
 *                       &lt;complexContent>
 *                         &lt;extension base="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType">
 *                         &lt;/extension>
 *                       &lt;/complexContent>
 *                     &lt;/complexType>
 *                   &lt;/element>
 *                   &lt;element name="msDetector" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *                   &lt;element ref="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}software"/>
 *                   &lt;element name="msResolution" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType" minOccurs="0"/>
 *                   &lt;element ref="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}operator" minOccurs="0"/>
 *                   &lt;sequence maxOccurs="unbounded" minOccurs="0">
 *                     &lt;element name="nameValue" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}namevalueType"/>
 *                     &lt;element name="comment" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *                   &lt;/sequence>
 *                 &lt;/sequence>
 *                 &lt;attribute name="msInstrumentID" type="{http://www.w3.org/2001/XMLSchema}int" />
 *               &lt;/restriction>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element name="dataProcessing" maxOccurs="unbounded">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 &lt;sequence>
 *                   &lt;element ref="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}software"/>
 *                   &lt;sequence maxOccurs="unbounded" minOccurs="0">
 *                     &lt;element name="processingOperation" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}namevalueType"/>
 *                     &lt;element name="comment" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *                   &lt;/sequence>
 *                 &lt;/sequence>
 *                 &lt;attribute name="intensityCutoff" type="{http://www.w3.org/2001/XMLSchema}float" />
 *                 &lt;attribute name="centroided" type="{http://www.w3.org/2001/XMLSchema}boolean" />
 *                 &lt;attribute name="deisotoped" type="{http://www.w3.org/2001/XMLSchema}boolean" />
 *                 &lt;attribute name="chargeDeconvoluted" type="{http://www.w3.org/2001/XMLSchema}boolean" />
 *                 &lt;attribute name="spotIntegration" type="{http://www.w3.org/2001/XMLSchema}boolean" />
 *               &lt;/restriction>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element name="separation" minOccurs="0">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 &lt;sequence>
 *                   &lt;element ref="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}separationTechnique" maxOccurs="unbounded"/>
 *                 &lt;/sequence>
 *               &lt;/restriction>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element name="spotting" minOccurs="0">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 &lt;sequence>
 *                   &lt;element name="plate" maxOccurs="unbounded">
 *                     &lt;complexType>
 *                       &lt;complexContent>
 *                         &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                           &lt;sequence>
 *                             &lt;element name="plateManufacturer" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *                             &lt;element name="plateModel" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *                             &lt;element name="pattern" minOccurs="0">
 *                               &lt;complexType>
 *                                 &lt;complexContent>
 *                                   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                                     &lt;sequence>
 *                                       &lt;element name="spottingPattern" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *                                       &lt;element name="orientation">
 *                                         &lt;complexType>
 *                                           &lt;complexContent>
 *                                             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                                               &lt;attribute name="firstSpotID" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *                                               &lt;attribute name="secondSpotID" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *                                             &lt;/restriction>
 *                                           &lt;/complexContent>
 *                                         &lt;/complexType>
 *                                       &lt;/element>
 *                                     &lt;/sequence>
 *                                   &lt;/restriction>
 *                                 &lt;/complexContent>
 *                               &lt;/complexType>
 *                             &lt;/element>
 *                             &lt;element name="spot" maxOccurs="unbounded">
 *                               &lt;complexType>
 *                                 &lt;complexContent>
 *                                   &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                                     &lt;sequence>
 *                                       &lt;element name="maldiMatrix" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *                                     &lt;/sequence>
 *                                     &lt;attribute name="spotID" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *                                     &lt;attribute name="spotXPosition" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *                                     &lt;attribute name="spotYPosition" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *                                     &lt;attribute name="spotDiameter" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *                                   &lt;/restriction>
 *                                 &lt;/complexContent>
 *                               &lt;/complexType>
 *                             &lt;/element>
 *                           &lt;/sequence>
 *                           &lt;attribute name="plateID" use="required" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *                           &lt;attribute name="spotXCount" use="required" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *                           &lt;attribute name="spotYCount" use="required" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *                         &lt;/restriction>
 *                       &lt;/complexContent>
 *                     &lt;/complexType>
 *                   &lt;/element>
 *                   &lt;element name="robot" minOccurs="0">
 *                     &lt;complexType>
 *                       &lt;complexContent>
 *                         &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                           &lt;sequence>
 *                             &lt;element name="robotManufacturer" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *                             &lt;element name="robotModel" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}ontologyEntryType"/>
 *                           &lt;/sequence>
 *                           &lt;attribute name="timePerSpot" use="required" type="{http://www.w3.org/2001/XMLSchema}duration" />
 *                           &lt;attribute name="deadVolume" type="{http://www.w3.org/2001/XMLSchema}nonNegativeInteger" />
 *                         &lt;/restriction>
 *                       &lt;/complexContent>
 *                     &lt;/complexType>
 *                   &lt;/element>
 *                 &lt;/sequence>
 *               &lt;/restriction>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element ref="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}scan" maxOccurs="unbounded"/>
 *         &lt;element name="sha1" minOccurs="0">
 *           &lt;simpleType>
 *             &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *               &lt;length value="40"/>
 *             &lt;/restriction>
 *           &lt;/simpleType>
 *         &lt;/element>
 *       &lt;/sequence>
 *       &lt;attribute name="scanCount" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *       &lt;attribute name="startTime" type="{http://www.w3.org/2001/XMLSchema}duration" />
 *       &lt;attribute name="endTime" type="{http://www.w3.org/2001/XMLSchema}duration" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {
    "parentFile",
    "msInstrument",
    "dataProcessing",
    "separation",
    "spotting",
    "scan",
    "sha1"
})
@XmlRootElement(name = "msRun")
public class MsRun
    implements Serializable, MzXMLObject
{

    private final static long serialVersionUID = 320L;
    @XmlElement(required = true)
    protected List<ParentFile> parentFile;
    protected List<MsInstrument> msInstrument;
    @XmlElement(required = true)
    protected List<DataProcessing> dataProcessing;
    protected Separation separation;
    protected Spotting spotting;
    @XmlElement(required = true)
    protected List<Scan> scan;
    protected String sha1;
    @XmlAttribute
    @XmlJavaTypeAdapter(NonNegativeIntegerAdapter.class)
    @XmlSchemaType(name = "positiveInteger")
    protected Long scanCount;
    @XmlAttribute
    protected Duration startTime;
    @XmlAttribute
    protected Duration endTime;

    /**
     * Gets the value of the parentFile property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the parentFile property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getParentFile().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link ParentFile }
     * 
     * 
     */
    public List<ParentFile> getParentFile() {
        if (parentFile == null) {
            parentFile = new ArrayList<ParentFile>();
        }
        return this.parentFile;
    }

    /**
     * Gets the value of the msInstrument property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the msInstrument property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getMsInstrument().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link MsInstrument }
     * 
     * 
     */
    public List<MsInstrument> getMsInstrument() {
        if (msInstrument == null) {
            msInstrument = new ArrayList<MsInstrument>();
        }
        return this.msInstrument;
    }

    /**
     * Gets the value of the dataProcessing property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the dataProcessing property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getDataProcessing().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link DataProcessing }
     * 
     * 
     */
    public List<DataProcessing> getDataProcessing() {
        if (dataProcessing == null) {
            dataProcessing = new ArrayList<DataProcessing>();
        }
        return this.dataProcessing;
    }

    /**
     * Gets the value of the separation property.
     * 
     * @return
     *     possible object is
     *     {@link Separation }
     *     
     */
    public Separation getSeparation() {
        return separation;
    }

    /**
     * Sets the value of the separation property.
     * 
     * @param value
     *     allowed object is
     *     {@link Separation }
     *     
     */
    public void setSeparation(Separation value) {
        this.separation = value;
    }

    /**
     * Gets the value of the spotting property.
     * 
     * @return
     *     possible object is
     *     {@link Spotting }
     *     
     */
    public Spotting getSpotting() {
        return spotting;
    }

    /**
     * Sets the value of the spotting property.
     * 
     * @param value
     *     allowed object is
     *     {@link Spotting }
     *     
     */
    public void setSpotting(Spotting value) {
        this.spotting = value;
    }

    /**
     * Gets the value of the scan property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the scan property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getScan().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Scan }
     * 
     * 
     */
    public List<Scan> getScan() {
        if (scan == null) {
            scan = new ArrayList<Scan>();
        }
        return this.scan;
    }

    /**
     * Gets the value of the sha1 property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getSha1() {
        return sha1;
    }

    /**
     * Sets the value of the sha1 property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setSha1(String value) {
        this.sha1 = value;
    }

    /**
     * Gets the value of the scanCount property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public Long getScanCount() {
        return scanCount;
    }

    /**
     * Sets the value of the scanCount property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setScanCount(Long value) {
        this.scanCount = value;
    }

    /**
     * Gets the value of the startTime property.
     * 
     * @return
     *     possible object is
     *     {@link Duration }
     *     
     */
    public Duration getStartTime() {
        return startTime;
    }

    /**
     * Sets the value of the startTime property.
     * 
     * @param value
     *     allowed object is
     *     {@link Duration }
     *     
     */
    public void setStartTime(Duration value) {
        this.startTime = value;
    }

    /**
     * Gets the value of the endTime property.
     * 
     * @return
     *     possible object is
     *     {@link Duration }
     *     
     */
    public Duration getEndTime() {
        return endTime;
    }

    /**
     * Sets the value of the endTime property.
     * 
     * @param value
     *     allowed object is
     *     {@link Duration }
     *     
     */
    public void setEndTime(Duration value) {
        this.endTime = value;
    }

}
