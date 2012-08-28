package uk.ac.ebi.jmzidml.model;

import uk.ac.ebi.jmzidml.model.mzidml.CvParam;

import java.util.List;

/**
 * This interface defines the presence of a List<Cvparam> getCvParam() method.
 * It is used in the Marshaller/Unmarshaller to update the CvParam containing
 * classes with the respective subclasses of CvParam.
 * Note: this interface is use together with the CvParamCapable interface,
 * to distinguish if a object has a CvParam or a List<CvParam>.
 *
 * @see uk.ac.ebi.jmzidml.model.CvParamCapable
 * @author Florian Reisinger
 *         Date: 18-Nov-2010
 * @since 1.0
 */
public interface CvParamListCapable {

    /**
     *
     * @return A List of CvParam objects.
     */
    public List<CvParam> getCvParam();
}
