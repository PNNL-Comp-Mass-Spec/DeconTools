package uk.ac.ebi.jmzidml.model.utils;

/**
 * User: gokelly
 * Date: 6/9/11
 * Time: 10:50 AM
 */

public class MzIdentMLElementConfig {
    private String tagName;
    private boolean indexed;
    private String xpath;
    private boolean cached;
    private boolean idMapped;
    private Class clazz;
    private Class cvParamClass;
    private Class userParamClass;
    private boolean autoRefResolving;
    private Class refResolverClass;


    public boolean isIndexed() {
        return indexed;
    }

    public void setIndexed(boolean indexed) {
        this.indexed = indexed;
    }

    public String getXpath() {
        return xpath;
    }

    public void setXpath(String xpath) {
        this.xpath = xpath;
    }

    public boolean isCached() {
        return cached;
    }

    public void setCached(boolean cached) {
        this.cached = cached;
    }

    public boolean isIdMapped() {
        return idMapped;
    }

    public void setIdMapped(boolean idMapped) {
        this.idMapped = idMapped;
    }

    public Class getClazz() {
        return clazz;
    }

    public void setClazz(Class clazz) {
        this.clazz = clazz;
    }

    public Class getCvParamClass() {
        return cvParamClass;
    }

    public void setCvParamClass(Class cvParamClass) {
        this.cvParamClass = cvParamClass;
    }

    public Class getUserParamClass() {
        return userParamClass;
    }

    public void setUserParamClass(Class userParamClass) {
        this.userParamClass = userParamClass;
    }

    public boolean isAutoRefResolving() {
        return autoRefResolving;
    }

    public void setAutoRefResolving(boolean autoRefResolving) {
        this.autoRefResolving = autoRefResolving;
    }

    public Class getRefResolverClass() {
        return refResolverClass;
    }

    public void setRefResolverClass(Class refResolverClass) {
        this.refResolverClass = refResolverClass;
    }

    public String getTagName() {
        return tagName;
    }

    public void setTagName(String tagName) {
        this.tagName = tagName;
    }
}
