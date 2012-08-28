package uk.ac.ebi.pride.tools.jmzreader.model.impl;

import java.util.ArrayList;
import java.util.List;

import uk.ac.ebi.pride.tools.jmzreader.model.Param;

/**
 * ParamGroup is a container for
 * UserParams and CvParams.
 * @author jg
 *
 */
public class ParamGroup {
	private List<CvParam> cvParams;
	private List<UserParam> userParams;
	
	public ParamGroup() {
		cvParams 	= new ArrayList<CvParam>();
		userParams 	= new ArrayList<UserParam>();
	}
	
	public void addParam(Param param) {
		if (param instanceof UserParam)
			userParams.add((UserParam) param);
		
		if (param instanceof CvParam)
			cvParams.add((CvParam) param);
	}
	
	public void removeParam(Param param) {
		if (param instanceof UserParam)
			userParams.remove((UserParam) param);
		
		if (param instanceof CvParam)
			cvParams.remove((CvParam) param);
	}
	
	public List<CvParam> getCvParams() {
		return new ArrayList<CvParam>(cvParams);
	}
	
	public List<UserParam> getUserParams() {
		return new ArrayList<UserParam>(userParams);
	}
	
	public List<Param> getParams() {
		ArrayList<Param> params = new ArrayList<Param>(cvParams);
		params.addAll(userParams);
		
		return params;
	}

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;

        ParamGroup that = (ParamGroup) o;

        if (cvParams != null ? !cvParams.equals(that.cvParams) : that.cvParams != null) return false;
        if (userParams != null ? !userParams.equals(that.userParams) : that.userParams != null) return false;

        return true;
    }

    @Override
    public int hashCode() {
        int result = cvParams != null ? cvParams.hashCode() : 0;
        result = 31 * result + (userParams != null ? userParams.hashCode() : 0);
        return result;
    }
}
