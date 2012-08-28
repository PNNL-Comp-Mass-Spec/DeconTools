package uk.ac.ebi.jmzidml.model.utils;

import javax.xml.bind.annotation.XmlRootElement;
import java.util.List;

/**
 * User: gokelly
 * Date: 6/9/11
 * Time: 11:21 AM
 */
@XmlRootElement
public class MzIdentMLElementProperties {
    private List<MzIdentMLElementConfig> configurations;


    public List<MzIdentMLElementConfig> getConfigurations() {
        return configurations;
    }

    public void setConfigurations(List<MzIdentMLElementConfig> configurations) {
        this.configurations = configurations;
    }


}
