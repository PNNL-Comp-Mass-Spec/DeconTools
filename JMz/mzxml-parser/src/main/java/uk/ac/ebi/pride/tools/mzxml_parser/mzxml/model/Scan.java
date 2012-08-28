
package uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlElements;
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
 *         &lt;element name="scanOrigin" maxOccurs="unbounded" minOccurs="0">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 &lt;attribute name="parentFileID" use="required">
 *                   &lt;simpleType>
 *                     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *                       &lt;length value="40"/>
 *                     &lt;/restriction>
 *                   &lt;/simpleType>
 *                 &lt;/attribute>
 *                 &lt;attribute name="num" use="required" type="{http://www.w3.org/2001/XMLSchema}nonNegativeInteger" />
 *               &lt;/restriction>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element name="precursorMz" maxOccurs="unbounded" minOccurs="0">
 *           &lt;complexType>
 *             &lt;simpleContent>
 *               &lt;extension base="&lt;http://www.w3.org/2001/XMLSchema>float">
 *                 &lt;attribute name="precursorScanNum" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *                 &lt;attribute name="precursorIntensity" use="required" type="{http://www.w3.org/2001/XMLSchema}float" />
 *                 &lt;attribute name="precursorCharge" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *                 &lt;attribute name="possibleCharges" type="{http://www.w3.org/2001/XMLSchema}string" />
 *                 &lt;attribute name="windowWideness" type="{http://www.w3.org/2001/XMLSchema}float" />
 *                 &lt;attribute name="activationMethod">
 *                   &lt;simpleType>
 *                     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *                       &lt;enumeration value="ETD"/>
 *                       &lt;enumeration value="ECD"/>
 *                       &lt;enumeration value="CID"/>
 *                       &lt;enumeration value="HCD"/>
 *                       &lt;enumeration value="ETD+SA"/>
 *                     &lt;/restriction>
 *                   &lt;/simpleType>
 *                 &lt;/attribute>
 *               &lt;/extension>
 *             &lt;/simpleContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element name="maldi" minOccurs="0">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 &lt;attribute name="plateID" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *                 &lt;attribute name="spotID" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *                 &lt;attribute name="laserShootCount" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *                 &lt;attribute name="laserFrequency" type="{http://www.w3.org/2001/XMLSchema}duration" />
 *                 &lt;attribute name="laserIntensity" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *                 &lt;attribute name="collisionGas" type="{http://www.w3.org/2001/XMLSchema}boolean" />
 *               &lt;/restriction>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element name="peaks" maxOccurs="unbounded">
 *           &lt;complexType>
 *             &lt;simpleContent>
 *               &lt;extension base="&lt;http://sashimi.sourceforge.net/schema_revision/mzXML_3.2>strictBase64Type">
 *                 &lt;attribute name="precision">
 *                   &lt;simpleType>
 *                     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}positiveInteger">
 *                       &lt;enumeration value="32"/>
 *                       &lt;enumeration value="64"/>
 *                     &lt;/restriction>
 *                   &lt;/simpleType>
 *                 &lt;/attribute>
 *                 &lt;attribute name="byteOrder" use="required" type="{http://www.w3.org/2001/XMLSchema}string" fixed="network" />
 *                 &lt;attribute name="contentType" use="required">
 *                   &lt;simpleType>
 *                     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *                       &lt;enumeration value="m/z-int"/>
 *                       &lt;enumeration value="m/z"/>
 *                       &lt;enumeration value="m/z ruler"/>
 *                       &lt;enumeration value="TOF"/>
 *                       &lt;enumeration value="intensity"/>
 *                       &lt;enumeration value="S/N"/>
 *                       &lt;enumeration value="charge"/>
 *                     &lt;/restriction>
 *                   &lt;/simpleType>
 *                 &lt;/attribute>
 *                 &lt;attribute name="compressionType" use="required">
 *                   &lt;simpleType>
 *                     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *                       &lt;enumeration value="none"/>
 *                       &lt;enumeration value="zlib"/>
 *                     &lt;/restriction>
 *                   &lt;/simpleType>
 *                 &lt;/attribute>
 *                 &lt;attribute name="compressedLen" use="required" type="{http://www.w3.org/2001/XMLSchema}int" />
 *               &lt;/extension>
 *             &lt;/simpleContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;sequence maxOccurs="unbounded" minOccurs="0">
 *           &lt;element name="nameValue" type="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}namevalueType"/>
 *           &lt;element name="comment" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *         &lt;/sequence>
 *         &lt;element ref="{http://sashimi.sourceforge.net/schema_revision/mzXML_3.2}scan" maxOccurs="unbounded" minOccurs="0"/>
 *       &lt;/sequence>
 *       &lt;attribute name="num" use="required" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *       &lt;attribute name="msLevel" use="required" type="{http://www.w3.org/2001/XMLSchema}positiveInteger" />
 *       &lt;attribute name="peaksCount" use="required" type="{http://www.w3.org/2001/XMLSchema}nonNegativeInteger" />
 *       &lt;attribute name="polarity">
 *         &lt;simpleType>
 *           &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *             &lt;enumeration value="+"/>
 *             &lt;enumeration value="-"/>
 *             &lt;enumeration value="any"/>
 *           &lt;/restriction>
 *         &lt;/simpleType>
 *       &lt;/attribute>
 *       &lt;attribute name="scanType">
 *         &lt;simpleType>
 *           &lt;restriction base="{http://www.w3.org/2001/XMLSchema}string">
 *             &lt;enumeration value="Full"/>
 *             &lt;enumeration value="zoom"/>
 *             &lt;enumeration value="SIM"/>
 *             &lt;enumeration value="SRM"/>
 *             &lt;enumeration value="CRM"/>
 *             &lt;enumeration value="Q1"/>
 *             &lt;enumeration value="Q3"/>
 *           &lt;/restriction>
 *         &lt;/simpleType>
 *       &lt;/attribute>
 *       &lt;attribute name="filterLine" type="{http://www.w3.org/2001/XMLSchema}string" />
 *       &lt;attribute name="centroided" type="{http://www.w3.org/2001/XMLSchema}boolean" />
 *       &lt;attribute name="deisotoped" type="{http://www.w3.org/2001/XMLSchema}boolean" />
 *       &lt;attribute name="chargeDeconvoluted" type="{http://www.w3.org/2001/XMLSchema}boolean" default="0" />
 *       &lt;attribute name="retentionTime" type="{http://www.w3.org/2001/XMLSchema}duration" />
 *       &lt;attribute name="ionisationEnergy" type="{http://www.w3.org/2001/XMLSchema}float" />
 *       &lt;attribute name="collisionEnergy" type="{http://www.w3.org/2001/XMLSchema}float" />
 *       &lt;attribute name="cidGasPressure" type="{http://www.w3.org/2001/XMLSchema}float" />
 *       &lt;attribute name="startMz" type="{http://www.w3.org/2001/XMLSchema}float" />
 *       &lt;attribute name="endMz" type="{http://www.w3.org/2001/XMLSchema}float" />
 *       &lt;attribute name="lowMz" type="{http://www.w3.org/2001/XMLSchema}float" />
 *       &lt;attribute name="highMz" type="{http://www.w3.org/2001/XMLSchema}float" />
 *       &lt;attribute name="basePeakMz" type="{http://www.w3.org/2001/XMLSchema}float" />
 *       &lt;attribute name="basePeakIntensity" type="{http://www.w3.org/2001/XMLSchema}float" />
 *       &lt;attribute name="totIonCurrent" type="{http://www.w3.org/2001/XMLSchema}float" />
 *       &lt;attribute name="msInstrumentID" type="{http://www.w3.org/2001/XMLSchema}int" />
 *       &lt;attribute name="compensationVoltage" type="{http://www.w3.org/2001/XMLSchema}float" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {
    "scanOrigin",
    "precursorMz",
    "maldi",
    "peaks",
    "nameValueAndComment",
    "scan"
})
@XmlRootElement(name = "scan")
public class Scan
    implements Serializable, MzXMLObject
{

    private final static long serialVersionUID = 320L;
    protected List<ScanOrigin> scanOrigin;
    protected List<PrecursorMz> precursorMz;
    protected Maldi maldi;
    @XmlElement(required = true, nillable = true)
    protected List<Peaks> peaks;
    @XmlElements({
        @XmlElement(name = "comment", type = String.class),
        @XmlElement(name = "nameValue", type = NameValue.class)
    })
    protected List<Object> nameValueAndComment;
    protected List<Scan> scan;
    @XmlAttribute(required = true)
    @XmlJavaTypeAdapter(NonNegativeIntegerAdapter.class)
    @XmlSchemaType(name = "positiveInteger")
    protected Long num;
    @XmlAttribute(required = true)
    @XmlJavaTypeAdapter(NonNegativeIntegerAdapter.class)
    @XmlSchemaType(name = "positiveInteger")
    protected Long msLevel;
    @XmlAttribute(required = true)
    @XmlJavaTypeAdapter(NonNegativeIntegerAdapter.class)
    @XmlSchemaType(name = "nonNegativeInteger")
    protected Long peaksCount;
    @XmlAttribute
    protected String polarity;
    @XmlAttribute
    protected String scanType;
    @XmlAttribute
    protected String filterLine;
    @XmlAttribute
    protected Boolean centroided;
    @XmlAttribute
    protected Boolean deisotoped;
    @XmlAttribute
    protected Boolean chargeDeconvoluted;
    @XmlAttribute
    protected Duration retentionTime;
    @XmlAttribute
    protected Float ionisationEnergy;
    @XmlAttribute
    protected Float collisionEnergy;
    @XmlAttribute
    protected Float cidGasPressure;
    @XmlAttribute
    protected Float startMz;
    @XmlAttribute
    protected Float endMz;
    @XmlAttribute
    protected Float lowMz;
    @XmlAttribute
    protected Float highMz;
    @XmlAttribute
    protected Float basePeakMz;
    @XmlAttribute
    protected Float basePeakIntensity;
    @XmlAttribute
    protected Float totIonCurrent;
    @XmlAttribute
    protected Integer msInstrumentID;
    @XmlAttribute
    protected Float compensationVoltage;

    /**
     * Gets the value of the scanOrigin property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the scanOrigin property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getScanOrigin().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link ScanOrigin }
     * 
     * 
     */
    public List<ScanOrigin> getScanOrigin() {
        if (scanOrigin == null) {
            scanOrigin = new ArrayList<ScanOrigin>();
        }
        return this.scanOrigin;
    }

    /**
     * Gets the value of the precursorMz property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the precursorMz property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getPrecursorMz().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link PrecursorMz }
     * 
     * 
     */
    public List<PrecursorMz> getPrecursorMz() {
        if (precursorMz == null) {
            precursorMz = new ArrayList<PrecursorMz>();
        }
        return this.precursorMz;
    }

    /**
     * Gets the value of the maldi property.
     * 
     * @return
     *     possible object is
     *     {@link Maldi }
     *     
     */
    public Maldi getMaldi() {
        return maldi;
    }

    /**
     * Sets the value of the maldi property.
     * 
     * @param value
     *     allowed object is
     *     {@link Maldi }
     *     
     */
    public void setMaldi(Maldi value) {
        this.maldi = value;
    }

    /**
     * Gets the value of the peaks property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the peaks property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getPeaks().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link Peaks }
     * 
     * 
     */
    public List<Peaks> getPeaks() {
        if (peaks == null) {
            peaks = new ArrayList<Peaks>();
        }
        return this.peaks;
    }

    /**
     * Gets the value of the nameValueAndComment property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the nameValueAndComment property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getNameValueAndComment().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link String }
     * {@link NameValue }
     * 
     * 
     */
    public List<Object> getNameValueAndComment() {
        if (nameValueAndComment == null) {
            nameValueAndComment = new ArrayList<Object>();
        }
        return this.nameValueAndComment;
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
     * Gets the value of the num property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public Long getNum() {
        return num;
    }

    /**
     * Sets the value of the num property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setNum(Long value) {
        this.num = value;
    }

    /**
     * Gets the value of the msLevel property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public Long getMsLevel() {
        return msLevel;
    }

    /**
     * Sets the value of the msLevel property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setMsLevel(Long value) {
        this.msLevel = value;
    }

    /**
     * Gets the value of the peaksCount property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public Long getPeaksCount() {
        return peaksCount;
    }

    /**
     * Sets the value of the peaksCount property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setPeaksCount(Long value) {
        this.peaksCount = value;
    }

    /**
     * Gets the value of the polarity property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getPolarity() {
        return polarity;
    }

    /**
     * Sets the value of the polarity property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setPolarity(String value) {
        this.polarity = value;
    }

    /**
     * Gets the value of the scanType property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getScanType() {
        return scanType;
    }

    /**
     * Sets the value of the scanType property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setScanType(String value) {
        this.scanType = value;
    }

    /**
     * Gets the value of the filterLine property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getFilterLine() {
        return filterLine;
    }

    /**
     * Sets the value of the filterLine property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setFilterLine(String value) {
        this.filterLine = value;
    }

    /**
     * Gets the value of the centroided property.
     * 
     * @return
     *     possible object is
     *     {@link Boolean }
     *     
     */
    public Boolean isCentroided() {
        return centroided;
    }

    /**
     * Sets the value of the centroided property.
     * 
     * @param value
     *     allowed object is
     *     {@link Boolean }
     *     
     */
    public void setCentroided(Boolean value) {
        this.centroided = value;
    }

    /**
     * Gets the value of the deisotoped property.
     * 
     * @return
     *     possible object is
     *     {@link Boolean }
     *     
     */
    public Boolean isDeisotoped() {
        return deisotoped;
    }

    /**
     * Sets the value of the deisotoped property.
     * 
     * @param value
     *     allowed object is
     *     {@link Boolean }
     *     
     */
    public void setDeisotoped(Boolean value) {
        this.deisotoped = value;
    }

    /**
     * Gets the value of the chargeDeconvoluted property.
     * 
     * @return
     *     possible object is
     *     {@link Boolean }
     *     
     */
    public boolean isChargeDeconvoluted() {
        if (chargeDeconvoluted == null) {
            return false;
        } else {
            return chargeDeconvoluted;
        }
    }

    /**
     * Sets the value of the chargeDeconvoluted property.
     * 
     * @param value
     *     allowed object is
     *     {@link Boolean }
     *     
     */
    public void setChargeDeconvoluted(Boolean value) {
        this.chargeDeconvoluted = value;
    }

    /**
     * Gets the value of the retentionTime property.
     * 
     * @return
     *     possible object is
     *     {@link Duration }
     *     
     */
    public Duration getRetentionTime() {
        return retentionTime;
    }

    /**
     * Sets the value of the retentionTime property.
     * 
     * @param value
     *     allowed object is
     *     {@link Duration }
     *     
     */
    public void setRetentionTime(Duration value) {
        this.retentionTime = value;
    }

    /**
     * Gets the value of the ionisationEnergy property.
     * 
     * @return
     *     possible object is
     *     {@link Float }
     *     
     */
    public Float getIonisationEnergy() {
        return ionisationEnergy;
    }

    /**
     * Sets the value of the ionisationEnergy property.
     * 
     * @param value
     *     allowed object is
     *     {@link Float }
     *     
     */
    public void setIonisationEnergy(Float value) {
        this.ionisationEnergy = value;
    }

    /**
     * Gets the value of the collisionEnergy property.
     * 
     * @return
     *     possible object is
     *     {@link Float }
     *     
     */
    public Float getCollisionEnergy() {
        return collisionEnergy;
    }

    /**
     * Sets the value of the collisionEnergy property.
     * 
     * @param value
     *     allowed object is
     *     {@link Float }
     *     
     */
    public void setCollisionEnergy(Float value) {
        this.collisionEnergy = value;
    }

    /**
     * Gets the value of the cidGasPressure property.
     * 
     * @return
     *     possible object is
     *     {@link Float }
     *     
     */
    public Float getCidGasPressure() {
        return cidGasPressure;
    }

    /**
     * Sets the value of the cidGasPressure property.
     * 
     * @param value
     *     allowed object is
     *     {@link Float }
     *     
     */
    public void setCidGasPressure(Float value) {
        this.cidGasPressure = value;
    }

    /**
     * Gets the value of the startMz property.
     * 
     * @return
     *     possible object is
     *     {@link Float }
     *     
     */
    public Float getStartMz() {
        return startMz;
    }

    /**
     * Sets the value of the startMz property.
     * 
     * @param value
     *     allowed object is
     *     {@link Float }
     *     
     */
    public void setStartMz(Float value) {
        this.startMz = value;
    }

    /**
     * Gets the value of the endMz property.
     * 
     * @return
     *     possible object is
     *     {@link Float }
     *     
     */
    public Float getEndMz() {
        return endMz;
    }

    /**
     * Sets the value of the endMz property.
     * 
     * @param value
     *     allowed object is
     *     {@link Float }
     *     
     */
    public void setEndMz(Float value) {
        this.endMz = value;
    }

    /**
     * Gets the value of the lowMz property.
     * 
     * @return
     *     possible object is
     *     {@link Float }
     *     
     */
    public Float getLowMz() {
        return lowMz;
    }

    /**
     * Sets the value of the lowMz property.
     * 
     * @param value
     *     allowed object is
     *     {@link Float }
     *     
     */
    public void setLowMz(Float value) {
        this.lowMz = value;
    }

    /**
     * Gets the value of the highMz property.
     * 
     * @return
     *     possible object is
     *     {@link Float }
     *     
     */
    public Float getHighMz() {
        return highMz;
    }

    /**
     * Sets the value of the highMz property.
     * 
     * @param value
     *     allowed object is
     *     {@link Float }
     *     
     */
    public void setHighMz(Float value) {
        this.highMz = value;
    }

    /**
     * Gets the value of the basePeakMz property.
     * 
     * @return
     *     possible object is
     *     {@link Float }
     *     
     */
    public Float getBasePeakMz() {
        return basePeakMz;
    }

    /**
     * Sets the value of the basePeakMz property.
     * 
     * @param value
     *     allowed object is
     *     {@link Float }
     *     
     */
    public void setBasePeakMz(Float value) {
        this.basePeakMz = value;
    }

    /**
     * Gets the value of the basePeakIntensity property.
     * 
     * @return
     *     possible object is
     *     {@link Float }
     *     
     */
    public Float getBasePeakIntensity() {
        return basePeakIntensity;
    }

    /**
     * Sets the value of the basePeakIntensity property.
     * 
     * @param value
     *     allowed object is
     *     {@link Float }
     *     
     */
    public void setBasePeakIntensity(Float value) {
        this.basePeakIntensity = value;
    }

    /**
     * Gets the value of the totIonCurrent property.
     * 
     * @return
     *     possible object is
     *     {@link Float }
     *     
     */
    public Float getTotIonCurrent() {
        return totIonCurrent;
    }

    /**
     * Sets the value of the totIonCurrent property.
     * 
     * @param value
     *     allowed object is
     *     {@link Float }
     *     
     */
    public void setTotIonCurrent(Float value) {
        this.totIonCurrent = value;
    }

    /**
     * Gets the value of the msInstrumentID property.
     * 
     * @return
     *     possible object is
     *     {@link Integer }
     *     
     */
    public Integer getMsInstrumentID() {
        return msInstrumentID;
    }

    /**
     * Sets the value of the msInstrumentID property.
     * 
     * @param value
     *     allowed object is
     *     {@link Integer }
     *     
     */
    public void setMsInstrumentID(Integer value) {
        this.msInstrumentID = value;
    }

    /**
     * Gets the value of the compensationVoltage property.
     * 
     * @return
     *     possible object is
     *     {@link Float }
     *     
     */
    public Float getCompensationVoltage() {
        return compensationVoltage;
    }

    /**
     * Sets the value of the compensationVoltage property.
     * 
     * @param value
     *     allowed object is
     *     {@link Float }
     *     
     */
    public void setCompensationVoltage(Float value) {
        this.compensationVoltage = value;
    }

}
