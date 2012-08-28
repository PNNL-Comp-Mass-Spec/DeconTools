package uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model;

import java.util.HashSet;
import java.util.Set;

import uk.ac.ebi.pride.tools.mzdata_parser.mzdata.model.MzData.Description;

public enum MzDataElement {

	CV_LOOKUP(		"/mzData/cvLookup", CvLookup.class),
	DESCRIPTION(	"/mzData/description", Description.class),
	SPECTRUM(		"/mzData/spectrumList/spectrum", Spectrum.class);	

    private final String xpath;

    @SuppressWarnings("rawtypes")
	private final Class type;

    private static final Set<String> xpaths;

    static {
        xpaths = new HashSet<String>();
        for (MzDataElement xpath : values()) {
            xpaths.add(xpath.getXpath());
        }
    }

    private MzDataElement(String xpath, @SuppressWarnings("rawtypes") Class clazz) {
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
