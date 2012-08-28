package uk.ac.ebi.pride.tools.mzxml_parser.mzxml.unmarshaller;

import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.MzXMLObject;
import uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model.MzXmlElement;

public interface MzXMLUnmarshaller {
	public <T extends MzXMLObject> T unmarshal(String xmlSnippet, MzXmlElement element) throws Exception;
}
