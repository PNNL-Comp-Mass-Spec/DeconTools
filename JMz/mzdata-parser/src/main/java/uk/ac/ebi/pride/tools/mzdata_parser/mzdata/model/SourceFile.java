
package uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model;

import java.io.Serializable;
import javax.xml.bind.annotation.XmlAccessType;
import javax.xml.bind.annotation.XmlAccessorType;
import javax.xml.bind.annotation.XmlElement;
import javax.xml.bind.annotation.XmlSchemaType;
import javax.xml.bind.annotation.XmlType;


/**
 * Description of the source file, including location and type.
 * 
 * <p>Java class for sourceFileType complex type.
 * 
 * <p>The following schema fragment specifies the expected content contained within this class.
 * 
 * <pre>
 * &lt;complexType name="sourceFileType">
 *   &lt;complexContent>
 *     &lt;restriction base="{http://www.w3.org/2001/XMLSchema}anyType">
 *       &lt;sequence>
 *         &lt;element name="nameOfFile" type="{http://www.w3.org/2001/XMLSchema}string"/>
 *         &lt;element name="pathToFile" type="{http://www.w3.org/2001/XMLSchema}anyURI"/>
 *         &lt;element name="fileType" type="{http://www.w3.org/2001/XMLSchema}string" minOccurs="0"/>
 *       &lt;/sequence>
 *     &lt;/restriction>
 *   &lt;/complexContent>
 * &lt;/complexType>
 * </pre>
 * 
 * 
 */
@XmlAccessorType(XmlAccessType.FIELD)
@XmlType(name = "sourceFileType", propOrder = {
    "nameOfFile",
    "pathToFile",
    "fileType"
})
public class SourceFile
    implements Serializable, MzDataObject
{

    private final static long serialVersionUID = 105L;
    @XmlElement(required = true)
    protected String nameOfFile;
    @XmlElement(required = true)
    @XmlSchemaType(name = "anyURI")
    protected String pathToFile;
    protected String fileType;

    /**
     * Gets the value of the nameOfFile property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getNameOfFile() {
        return nameOfFile;
    }

    /**
     * Sets the value of the nameOfFile property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setNameOfFile(String value) {
        this.nameOfFile = value;
    }

    /**
     * Gets the value of the pathToFile property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getPathToFile() {
        return pathToFile;
    }

    /**
     * Sets the value of the pathToFile property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setPathToFile(String value) {
        this.pathToFile = value;
    }

    /**
     * Gets the value of the fileType property.
     * 
     * @return
     *     possible object is
     *     {@link String }
     *     
     */
    public String getFileType() {
        return fileType;
    }

    /**
     * Sets the value of the fileType property.
     * 
     * @param value
     *     allowed object is
     *     {@link String }
     *     
     */
    public void setFileType(String value) {
        this.fileType = value;
    }

}
