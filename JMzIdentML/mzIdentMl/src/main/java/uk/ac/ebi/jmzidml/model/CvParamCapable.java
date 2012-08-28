package uk.ac.ebi.jmzidml.model;

import uk.ac.ebi.jmzidml.model.mzidml.CvParam;

/**
 * This interface defines the presence of a Cvparam getCvParam() method. It is used
 * in the Marshaller/Unmarshaller to update the CvParam containing classes
 * with the respective subclasses of CvParam.
 * Note: this interface is use together with the CvParamListCapable interface,
 * to distinguish if a object has a CvParam or a List<CvParam>.
 *
 * @see uk.ac.ebi.jmzidml.model.CvParamListCapable
 * @author Florian Reisinger
 *         Date: 09-Nov-2010
 * @since 1.0
 */
public interface CvParamCapable {

    /**
     * @return A single CvParam.
     */
    public CvParam getCvParam();

    public void setCvParam(CvParam param);
}
