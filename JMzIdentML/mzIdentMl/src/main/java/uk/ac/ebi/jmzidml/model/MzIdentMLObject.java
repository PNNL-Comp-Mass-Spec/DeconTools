package uk.ac.ebi.jmzidml.model;

import javax.xml.bind.annotation.XmlTransient;

/**
 * @author Florian Reisinger
 *         Date: 27-May-2009
 * @since $version
 *
 * TODO Look at using long primitive for hid instead of object. Object was used in hibernate prototype - why?
 */
public abstract class MzIdentMLObject {
    @XmlTransient
    private Long hid;

    public Long getHid(){
        return hid;
    }

}
