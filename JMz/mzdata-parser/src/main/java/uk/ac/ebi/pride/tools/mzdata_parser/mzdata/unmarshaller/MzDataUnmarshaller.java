package uk.ac.ebi.pride.tools.mzdata_parser.mzdata.unmarshaller;

import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.MzDataElement;
import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.MzDataObject;

public interface MzDataUnmarshaller {
	public <T extends MzDataObject> T unmarshal(String xmlSnippet, MzDataElement element) throws Exception;
}
