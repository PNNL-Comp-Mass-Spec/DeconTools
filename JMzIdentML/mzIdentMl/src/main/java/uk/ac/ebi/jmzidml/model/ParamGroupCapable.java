package uk.ac.ebi.jmzidml.model;

import uk.ac.ebi.jmzidml.model.mzidml.CvParam;
import uk.ac.ebi.jmzidml.model.mzidml.Param;
import uk.ac.ebi.jmzidml.model.mzidml.UserParam;

import java.util.List;

/**
 * Defines the methods needed by the Marshal/Unmarshal listeners to split
 * a List<Param> into respective List<CvParam> and List<UserParam> and
 * methods used to replace the CvParam/UserParam classes with their
 * respective subclasses.
 *
 * @see uk.ac.ebi.jmzidml.model.utils.ParamUpdater
 * @see uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.listeners.RawXMLListener
 * @see uk.ac.ebi.jmzidml.xml.jaxb.marshaller.listeners.ObjectClassListener
 * @author Florian Reisinger
 *         Date: 09-Nov-2010
 * @since 1.0
 */
public interface ParamGroupCapable extends CvParamListCapable {

    public List<CvParam> getCvParam();

    public List<UserParam> getUserParam();
}
