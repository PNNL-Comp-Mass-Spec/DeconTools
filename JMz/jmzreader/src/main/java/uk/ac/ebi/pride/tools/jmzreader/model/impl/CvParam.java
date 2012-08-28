package uk.ac.ebi.pride.tools.jmzreader.model.impl;

import uk.ac.ebi.pride.tools.jmzreader.model.Param;

/**
 * A CvParam object. Used to report additional
 * information about objects.
 * @author jg
 *
 */
public class CvParam implements Param {
	private String name;
	private String value;
	private String cv;
	private String accession;
	
	public CvParam(String name, String value, String cv, String accession) {
		this.name = name;
		this.value = value;
		this.cv = cv;
		this.accession = accession;
	}

	@Override
	public String getName() {
		return name;
	}

	@Override
	public String getValue() {
		return value;
	}

	@Override
	public void setName(String name) {
		this.name = name;
	}

	@Override
	public void setValue(String value) {
		this.value = value;
	}

	public String getCv() {
		return cv;
	}

	public void setCv(String cv) {
		this.cv = cv;
	}

	public String getAccession() {
		return accession;
	}

	public void setAccession(String accession) {
		this.accession = accession;
	}

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;

        CvParam cvParam = (CvParam) o;

        if (accession != null ? !accession.equals(cvParam.accession) : cvParam.accession != null) return false;
        if (cv != null ? !cv.equals(cvParam.cv) : cvParam.cv != null) return false;
        if (name != null ? !name.equals(cvParam.name) : cvParam.name != null) return false;
        if (value != null ? !value.equals(cvParam.value) : cvParam.value != null) return false;

        return true;
    }

    @Override
    public int hashCode() {
        int result = name != null ? name.hashCode() : 0;
        result = 31 * result + (value != null ? value.hashCode() : 0);
        result = 31 * result + (cv != null ? cv.hashCode() : 0);
        result = 31 * result + (accession != null ? accession.hashCode() : 0);
        return result;
    }
}
