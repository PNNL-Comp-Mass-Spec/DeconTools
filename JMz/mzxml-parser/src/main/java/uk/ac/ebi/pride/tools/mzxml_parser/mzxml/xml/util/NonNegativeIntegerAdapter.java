package uk.ac.ebi.pride.tools.mzxml_parser.mzxml.xml.util;

import javax.xml.bind.annotation.adapters.XmlAdapter;

/**
 * Created by IntelliJ IDEA.
 * User: rcote
 * Date: 13/01/11
 * Time: 16:33
 */
public class NonNegativeIntegerAdapter extends XmlAdapter<String, Long> {

    @Override
    public Long unmarshal(String v) throws Exception {
        return Long.valueOf(v);
    }

    @Override
    public String marshal(Long v) throws Exception {
        if (v != null)
            return v.toString();
        else
            return null;
    }
}
