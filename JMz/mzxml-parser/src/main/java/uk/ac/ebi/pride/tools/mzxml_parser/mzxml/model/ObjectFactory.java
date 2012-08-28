
package uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model;

import javax.xml.bind.JAXBElement;
import javax.xml.bind.annotation.XmlElementDecl;
import javax.xml.bind.annotation.XmlRegistry;
import javax.xml.namespace.QName;


/**
 * This object contains factory methods for each 
 * Java content interface and Java element interface 
 * generated in the uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model package. 
 * <p>An ObjectFactory allows you to programatically 
 * construct new instances of the Java representation 
 * for XML content. The Java representation of XML 
 * content can consist of schema derived interfaces 
 * and classes representing the binding of schema 
 * type definitions, element declarations and model 
 * groups.  Factory methods for each of these are 
 * provided in this class.
 * 
 */
@XmlRegistry
public class ObjectFactory {

    private final static QName _SeparationTechnique_QNAME = new QName("http://sashimi.sourceforge.net/schema_revision/mzXML_3.2", "separationTechnique");

    /**
     * Create a new ObjectFactory that can be used to create new instances of schema derived classes for package: uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model
     * 
     */
    public ObjectFactory() {
    }

    /**
     * Create an instance of {@link MsManufacturer }
     * 
     */
    public MsManufacturer createMsManufacturer() {
        return new MsManufacturer();
    }

    /**
     * Create an instance of {@link SeparationTechnique }
     * 
     */
    public SeparationTechnique createSeparationTechnique() {
        return new SeparationTechnique();
    }

    /**
     * Create an instance of {@link Peaks }
     * 
     */
    public Peaks createPeaks() {
        return new Peaks();
    }

    /**
     * Create an instance of {@link PrecursorMz }
     * 
     */
    public PrecursorMz createPrecursorMz() {
        return new PrecursorMz();
    }

    /**
     * Create an instance of {@link Scan }
     * 
     */
    public Scan createScan() {
        return new Scan();
    }

    /**
     * Create an instance of {@link NameValue }
     * 
     */
    public NameValue createNameValue() {
        return new NameValue();
    }

    /**
     * Create an instance of {@link Software }
     * 
     */
    public Software createSoftware() {
        return new Software();
    }

    /**
     * Create an instance of {@link DataProcessing }
     * 
     */
    public DataProcessing createDataProcessing() {
        return new DataProcessing();
    }

    /**
     * Create an instance of {@link ParentFile }
     * 
     */
    public ParentFile createParentFile() {
        return new ParentFile();
    }

    /**
     * Create an instance of {@link Operator }
     * 
     */
    public Operator createOperator() {
        return new Operator();
    }

    /**
     * Create an instance of {@link Spot }
     * 
     */
    public Spot createSpot() {
        return new Spot();
    }

    /**
     * Create an instance of {@link Pattern }
     * 
     */
    public Pattern createPattern() {
        return new Pattern();
    }

    /**
     * Create an instance of {@link ScanOrigin }
     * 
     */
    public ScanOrigin createScanOrigin() {
        return new ScanOrigin();
    }

    /**
     * Create an instance of {@link Plate }
     * 
     */
    public Plate createPlate() {
        return new Plate();
    }

    /**
     * Create an instance of {@link Maldi }
     * 
     */
    public Maldi createMaldi() {
        return new Maldi();
    }

    /**
     * Create an instance of {@link Separation }
     * 
     */
    public Separation createSeparation() {
        return new Separation();
    }

    /**
     * Create an instance of {@link Spotting }
     * 
     */
    public Spotting createSpotting() {
        return new Spotting();
    }

    /**
     * Create an instance of {@link MsRun }
     * 
     */
    public MsRun createMsRun() {
        return new MsRun();
    }

    /**
     * Create an instance of {@link Robot }
     * 
     */
    public Robot createRobot() {
        return new Robot();
    }

    /**
     * Create an instance of {@link OntologyEntry }
     * 
     */
    public OntologyEntry createOntologyEntry() {
        return new OntologyEntry();
    }

    /**
     * Create an instance of {@link MsMassAnalyzer }
     * 
     */
    public MsMassAnalyzer createMsMassAnalyzer() {
        return new MsMassAnalyzer();
    }

    /**
     * Create an instance of {@link Orientation }
     * 
     */
    public Orientation createOrientation() {
        return new Orientation();
    }

    /**
     * Create an instance of {@link MsInstrument }
     * 
     */
    public MsInstrument createMsInstrument() {
        return new MsInstrument();
    }

    /**
     * Create an instance of {@link JAXBElement }{@code <}{@link SeparationTechnique }{@code >}}
     * 
     */
    @XmlElementDecl(namespace = "http://sashimi.sourceforge.net/schema_revision/mzXML_3.2", name = "separationTechnique")
    public JAXBElement<SeparationTechnique> createSeparationTechnique(SeparationTechnique value) {
        return new JAXBElement<SeparationTechnique>(_SeparationTechnique_QNAME, SeparationTechnique.class, null, value);
    }

}
