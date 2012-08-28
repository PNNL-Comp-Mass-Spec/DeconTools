
package uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlRootElement;
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
 *         &lt;element name="cvLookup" type="{}cvLookupType" maxOccurs="unbounded" minOccurs="0"/>
 *         &lt;element name="description">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 &lt;sequence>
 *                   &lt;element name="admin" type="{}adminType"/>
 *                   &lt;element name="instrument" type="{}instrumentDescriptionType"/>
 *                   &lt;element name="dataProcessing" type="{}dataProcessingType"/>
 *                 &lt;/sequence>
 *               &lt;/restriction>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element name="spectrumList">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 &lt;sequence>
 *                   &lt;element name="spectrum" maxOccurs="unbounded">
 *                     &lt;complexType>
 *                       &lt;complexContent>
 *                         &lt;extension base="{}spectrumType">
 *                         &lt;/extension>
 *                       &lt;/complexContent>
 *                     &lt;/complexType>
 *                   &lt;/element>
 *                 &lt;/sequence>
 *                 &lt;attribute name="count" use="required" type="{http://www.w3.org/2001/XMLSchema}int" />
 *               &lt;/restriction>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *       &lt;/sequence>
 *       &lt;attribute name="version" use="required" type="{http://www.w3.org/2001/XMLSchema}string" fixed="1.05" />
 *       &lt;attribute name="accessionNumber" use="required" type="{http://www.w3.org/2001/XMLSchema}string" />
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "", propOrder = {
    "cvLookup",
    "description",
    "spectrumList"
})
@XmlRootElement(name = "mzData")
public class MzData
    implements Serializable, MzDataObject
{

    private final static long serialVersionUID = 105L;
    protected List<CvLookup> cvLookup;
    @XmlElement(required = true)
    protected MzData.Description description;
    @XmlElement(required = true)
    protected MzData.SpectrumList spectrumList;
    @XmlAttribute(required = true)
    protected String version;
    @XmlAttribute(required = true)
    protected String accessionNumber;

    /**
     * Gets the value of the cvLookup property.
     * 
     * <p>
     * This accessor method returns a reference to the live list,
     * not a snapshot. Therefore any modification you make to the
     * returned list will be present inside the JAXB object.
     * This is why there is not a <CODE>set</CODE> method for the cvLookup property.
     * 
     * <p>
     * For example, to add a new item, do as follows:
     * <pre>
     *    getCvLookup().add(newItem);
     * </pre>
     * 
     * 
     * <p>
     * Objects of the following type(s) are allowed in the list
     * {@link CvLookup }
     * 
     * 
     */
    public List<CvLookup> getCvLookup() {
        if (cvLookup == null) {
            cvLookup = new ArrayList<CvLookup>();
        }
        return this.cvLookup;
    }

    /**
     * Gets the value of the description property.
     * 
     * @return
     *     possible object is
     *     {@link MzData.Description }
     *     
     */
    public MzData.Description getDescription() {
        return description;
    }

    /**
     * Sets the value of the description property.
     * 
     * @param value
     *     allowed object is
     *     {@link MzData.Description }
     *     
     */
    public void setDescription(MzData.Description value) {
        this.description = value;
    }

    /**
     * Gets the value of the spectrumList property.
     * 
     * @return
     *     possible object is
     *     {@link MzData.SpectrumList }
     *     
     */
    public MzData.SpectrumList getSpectrumList() {
        return spectrumList;
    }

    /**
     * Sets the value of the spectrumList property.
     * 
     * @param value
     *     allowed object is
     *     {@link MzData.SpectrumList }
     *     
     */
    public void setSpectrumList(MzData.SpectrumList value) {
        this.spectrumList = value;
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
        if (version == null) {
            return "1.05";
        } else {
            return version;
        }
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
     * Gets the value of the accessionNumber property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getAccessionNumber() {
        return accessionNumber;
    }

    /**
     * Sets the value of the accessionNumber property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setAccessionNumber(String value) {
        this.accessionNumber = value;
    }


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
     *         &lt;element name="admin" type="{}adminType"/>
     *         &lt;element name="instrument" type="{}instrumentDescriptionType"/>
     *         &lt;element name="dataProcessing" type="{}dataProcessingType"/>
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
        "admin",
        "instrument",
        "dataProcessing"
    })
    public static class Description
        implements Serializable, MzDataObject
    {

        private final static long serialVersionUID = 105L;
        @XmlElement(required = true)
        protected Admin admin;
        @XmlElement(required = true)
        protected InstrumentDescription instrument;
        @XmlElement(required = true)
        protected DataProcessing dataProcessing;

        /**
         * Gets the value of the admin property.
         * 
         * @return
         *     possible object is
         *     {@link Admin }
         *     
         */
        public Admin getAdmin() {
            return admin;
        }

        /**
         * Sets the value of the admin property.
         * 
         * @param value
         *     allowed object is
         *     {@link Admin }
         *     
         */
        public void setAdmin(Admin value) {
            this.admin = value;
        }

        /**
         * Gets the value of the instrument property.
         * 
         * @return
         *     possible object is
         *     {@link InstrumentDescription }
         *     
         */
        public InstrumentDescription getInstrument() {
            return instrument;
        }

        /**
         * Sets the value of the instrument property.
         * 
         * @param value
         *     allowed object is
         *     {@link InstrumentDescription }
         *     
         */
        public void setInstrument(InstrumentDescription value) {
            this.instrument = value;
        }

        /**
         * Gets the value of the dataProcessing property.
         * 
         * @return
         *     possible object is
         *     {@link DataProcessing }
         *     
         */
        public DataProcessing getDataProcessing() {
            return dataProcessing;
        }

        /**
         * Sets the value of the dataProcessing property.
         * 
         * @param value
         *     allowed object is
         *     {@link DataProcessing }
         *     
         */
        public void setDataProcessing(DataProcessing value) {
            this.dataProcessing = value;
        }

    }


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
     *         &lt;element name="spectrum" maxOccurs="unbounded">
     *           &lt;complexType>
     *             &lt;complexContent>
     *               &lt;extension base="{}spectrumType">
     *               &lt;/extension>
     *             &lt;/complexContent>
     *           &lt;/complexType>
     *         &lt;/element>
     *       &lt;/sequence>
     *       &lt;attribute name="count" use="required" type="{http://www.w3.org/2001/XMLSchema}int" />
     *     &lt;/restriction>
     *   &lt;/complexContent>
     * &lt;/complexType>
     * </pre>
     * 
     * 
     */
    @XmlAccessorType(XmlAccessType.FIELD)
    @XmlType(name = "", propOrder = {
        "spectrum"
    })
    public static class SpectrumList
        implements Serializable, MzDataObject
    {

        private final static long serialVersionUID = 105L;
        @XmlElement(required = true)
        protected List<MzData.SpectrumList.Spectrum> spectrum;
        @XmlAttribute(required = true)
        protected int count;

        /**
         * Gets the value of the spectrum property.
         * 
         * <p>
         * This accessor method returns a reference to the live list,
         * not a snapshot. Therefore any modification you make to the
         * returned list will be present inside the JAXB object.
         * This is why there is not a <CODE>set</CODE> method for the spectrum property.
         * 
         * <p>
         * For example, to add a new item, do as follows:
         * <pre>
         *    getSpectrum().add(newItem);
         * </pre>
         * 
         * 
         * <p>
         * Objects of the following type(s) are allowed in the list
         * {@link MzData.SpectrumList.Spectrum }
         * 
         * 
         */
        public List<MzData.SpectrumList.Spectrum> getSpectrum() {
            if (spectrum == null) {
                spectrum = new ArrayList<MzData.SpectrumList.Spectrum>();
            }
            return this.spectrum;
        }

        /**
         * Gets the value of the count property.
         * 
         */
        public int getCount() {
            return count;
        }

        /**
         * Sets the value of the count property.
         * 
         */
        public void setCount(int value) {
            this.count = value;
        }


        /**
         * <p>Java class for anonymous complex type.
         * 
         * <p>The following schema fragment specifies the expected content contained within this class.
         * 
         * <pre>
         * &lt;complexType>
         *   &lt;complexContent>
         *     &lt;extension base="{}spectrumType">
         *     &lt;/extension>
         *   &lt;/complexContent>
         * &lt;/complexType>
         * </pre>
         * 
         * 
         */
        @XmlAccessorType(XmlAccessType.FIELD)
        @XmlType(name = "")
        public static class Spectrum
            extends uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.Spectrum
            implements Serializable
        {

            private final static long serialVersionUID = 105L;

        }

    }

}
