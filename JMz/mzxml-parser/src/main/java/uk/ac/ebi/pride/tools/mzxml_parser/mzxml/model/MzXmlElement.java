package uk.ac.ebi.pride.tools.mzxml_parser.mzxml.model;

import java.util.HashSet;
import java.util.Set;

public enum MzXmlElement {

    PARENT_FILE(	"/mzXML/msRun/parentFile", 	ParentFile.class),
    MS_INSTRUMENT(	"/mzXML/msRun/msInstrument", 	MsInstrument.class),
    DATA_PROCESSING("/mzXML/msRun/dataProcessing",DataProcessing.class),
    SEPARATION(		"/mzXML/msRun/separation",	Separation.class),
    SPOTTING(		"/mzXML/msRun/spotting",		Spotting.class),
    SCAN_LEVEL1(	"/mzXML/msRun/scan",			Scan.class),
    SCAN_LEVEL2(	"/mzXML/msRun/scan/scan", 		Scan.class);	

    private final String xpath;

    @SuppressWarnings("rawtypes")
	private final Class type;

    private static final Set<String> xpaths;

    static {
        xpaths = new HashSet<String>();
        for (MzXmlElement xpath : values()) {
            xpaths.add(xpath.getXpath());
        }
    }

    private MzXmlElement(String xpath, @SuppressWarnings("rawtypes") Class clazz) {
        this.xpath = xpath;
        this.type = clazz;
    }

    public String getXpath() {
        return xpath;
    }

    @SuppressWarnings("rawtypes")
	public Class getClassType() {
        return type;
    }

    public static Set<String> getXpaths() {
        return xpaths;
    }
}
