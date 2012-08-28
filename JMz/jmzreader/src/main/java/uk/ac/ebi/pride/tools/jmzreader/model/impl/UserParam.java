package uk.ac.ebi.pride.tools.jmzreader.model.impl;

import uk.ac.ebi.pride.tools.jmzreader.model.Param;

public class UserParam implements Param {
	private String name;
	private String value;
	
	public UserParam(String name, String value) {
		this.name = name;
		this.value = value;
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

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (o == null || getClass() != o.getClass()) return false;

        UserParam userParam = (UserParam) o;

        if (name != null ? !name.equals(userParam.name) : userParam.name != null) return false;
        if (value != null ? !value.equals(userParam.value) : userParam.value != null) return false;

        return true;
    }

    @Override
    public int hashCode() {
        int result = name != null ? name.hashCode() : 0;
        result = 31 * result + (value != null ? value.hashCode() : 0);
        return result;
    }
}
