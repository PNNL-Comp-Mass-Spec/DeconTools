package uk.ac.ebi.jmzidml.model;

/**
 * Interface defining the presence of the getId() method.
 * In the mzIdentML model that is guaranteed for all classes extending
 * the Identifiable abstract class. However this is not the case for all.
 * E.g. Cv.class
 * The getId() method is needed to create the ID lookup caches of the MzIdentMLIndexer.
 *
 * @author Florian Reisinger
 *         Date: 16-Nov-2010
 * @since 1.0
 */
public interface IdentifiableMzIdentMLObject {

    public String getId();
}
