
package uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlAttribute;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlType;


/**
 * Description of the components of the mass spectrometer used
 * 
 * <p>Java class for instrumentDescriptionType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="instrumentDescriptionType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="instrumentName" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="source" type="{}paramType"/>
 *         &lt;element name="analyzerList">
 *           &lt;complexType>
 *             &lt;complexContent>
 *               &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *                 &lt;sequence>
 *                   &lt;element name="analyzer" type="{}paramType" maxOccurs="unbounded"/>
 *                 &lt;/sequence>
 *                 &lt;attribute name="count" use="required" type="{http://www.w3.org/2001/XMLSchema}int" />
 *               &lt;/restriction>
 *             &lt;/complexContent>
 *           &lt;/complexType>
 *         &lt;/element>
 *         &lt;element name="detector" type="{}paramType"/>
 *         &lt;element name="additional" type="{}paramType" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "instrumentDescriptionType", propOrder = {
    "instrumentName",
    "source",
    "analyzerList",
    "detector",
    "additional"
})
public class InstrumentDescription
    implements Serializable, MzDataObject
{

    private final static long serialVersionUID = 105L;
    @XmlElement(required = true)
    protected String instrumentName;
    @XmlElement(required = true)
    protected Param source;
    @XmlElement(required = true)
    protected InstrumentDescription.AnalyzerList analyzerList;
    @XmlElement(required = true)
    protected Param detector;
    protected Param additional;

    /**
     * Gets the value of the instrumentName property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getInstrumentName() {
        return instrumentName;
    }

    /**
     * Sets the value of the instrumentName property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setInstrumentName(String value) {
        this.instrumentName = value;
    }

    /**
     * Gets the value of the source property.
     * 
     * @return
     *     possible object is
     *     {@link Param }
     *     
     */
    public Param getSource() {
        return source;
    }

    /**
     * Sets the value of the source property.
     * 
     * @param value
     *     allowed object is
     *     {@link Param }
     *     
     */
    public void setSource(Param value) {
        this.source = value;
    }

    /**
     * Gets the value of the analyzerList property.
     * 
     * @return
     *     possible object is
     *     {@link InstrumentDescription.AnalyzerList }
     *     
     */
    public InstrumentDescription.AnalyzerList getAnalyzerList() {
        return analyzerList;
    }

    /**
     * Sets the value of the analyzerList property.
     * 
     * @param value
     *     allowed object is
     *     {@link InstrumentDescription.AnalyzerList }
     *     
     */
    public void setAnalyzerList(InstrumentDescription.AnalyzerList value) {
        this.analyzerList = value;
    }

    /**
     * Gets the value of the detector property.
     * 
     * @return
     *     possible object is
     *     {@link Param }
     *     
     */
    public Param getDetector() {
        return detector;
    }

    /**
     * Sets the value of the detector property.
     * 
     * @param value
     *     allowed object is
     *     {@link Param }
     *     
     */
    public void setDetector(Param value) {
        this.detector = value;
    }

    /**
     * Gets the value of the additional property.
     * 
     * @return
     *     possible object is
     *     {@link Param }
     *     
     */
    public Param getAdditional() {
        return additional;
    }

    /**
     * Sets the value of the additional property.
     * 
     * @param value
     *     allowed object is
     *     {@link Param }
     *     
     */
    public void setAdditional(Param value) {
        this.additional = value;
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
     *         &lt;element name="analyzer" type="{}paramType" maxOccurs="unbounded"/>
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
        "analyzer"
    })
    public static class AnalyzerList
        implements Serializable, MzDataObject
    {

        private final static long serialVersionUID = 105L;
        @XmlElement(required = true)
        protected List<Param> analyzer;
        @XmlAttribute(required = true)
        protected int count;

        /**
         * Gets the value of the analyzer property.
         * 
         * <p>
         * This accessor method returns a reference to the live list,
         * not a snapshot. Therefore any modification you make to the
         * returned list will be present inside the JAXB object.
         * This is why there is not a <CODE>set</CODE> method for the analyzer property.
         * 
         * <p>
         * For example, to add a new item, do as follows:
         * <pre>
         *    getAnalyzer().add(newItem);
         * </pre>
         * 
         * 
         * <p>
         * Objects of the following type(s) are allowed in the list
         * {@link Param }
         * 
         * 
         */
        public List<Param> getAnalyzer() {
            if (analyzer == null) {
                analyzer = new ArrayList<Param>();
            }
            return this.analyzer;
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

    }

}
