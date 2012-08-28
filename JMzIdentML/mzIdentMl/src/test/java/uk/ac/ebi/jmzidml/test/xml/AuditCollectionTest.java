package uk.ac.ebi.jmzidml.test.xml;

import junit.framework.TestCase;
import org.apache.log4j.Logger;
import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.*;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLUnmarshaller;

import java.net.URL;
import java.util.Iterator;
import java.util.List;

/**
 * Package  : uk.ac.ebi.jmzidml.test.xml
 * Author   : riteshk
 * Date     : Sep 18, 2010
 */
public class AuditCollectionTest extends TestCase {

    Logger log = Logger.getLogger(AuditCollectionTest.class);

    public void testAuditCollectionInformation() throws Exception {

        URL xmlFileURL = AuditCollectionTest.class.getClassLoader().getResource("Mascot_MSMS_example.mzid");
        assertNotNull(xmlFileURL);

        MzIdentMLUnmarshaller unmarshaller = new MzIdentMLUnmarshaller(xmlFileURL);
        assertNotNull(unmarshaller);

        Iterator<AuditCollection> aci = unmarshaller.unmarshalCollectionFromXpath(MzIdentMLElement.AuditCollection);
        assertNotNull(aci);

        while(aci.hasNext()){
            AuditCollection ac = aci.next();
            assertNotNull(ac);
/*
            for (Contact contact : ac.getContactGroup()) {
                assertNotNull(contact);
                assertNotNull(contact.getId());
                if (contact.getEmail() != null) {
                    assertTrue(contact.getEmail().contains("@"));
                }
            }
*/
        }

        // Resolve the organization_ref
        Iterator<Person> pi = unmarshaller.unmarshalCollectionFromXpath(MzIdentMLElement.Person);
        while(pi.hasNext()){
            assertNotNull(pi);
            Person p = pi.next();
            assertNotNull(p);
            assertEquals(1, p.getAffiliation().size()); // we expect one affiliation per person
            for (Affiliation affiliation : p.getAffiliation()) {
                assertNotNull(affiliation);
                if (MzIdentMLElement.Affiliation.isAutoRefResolving() && affiliation.getOrganizationRef() != null) {
                    Organization org = affiliation.getOrganization();
                    assertNotNull(org);
                    assertNotNull(org.getId());
/*
                    if (org.getEmail() != null) {
                        assertTrue(org.getEmail().contains("@"));
                    }
*/
                } else {
                    System.out.println("Affiliation is not auto-resolving or does not contain a Organization reference.");
                    assertNull(affiliation.getOrganization());
                }
            }

        }

    }

    public void testContactFacadeList(){
        URL xmlFileURL = AuditCollectionTest.class.getClassLoader().getResource("Mascot_MSMS_example.mzid");
        assertNotNull(xmlFileURL);

        MzIdentMLUnmarshaller unmarshaller = new MzIdentMLUnmarshaller(xmlFileURL);
        assertNotNull(unmarshaller);

        Iterator<AuditCollection> aci = unmarshaller.unmarshalCollectionFromXpath(MzIdentMLElement.AuditCollection);
        assertNotNull(aci);

        assertTrue(aci.hasNext())  ;
        AuditCollection ac = aci.next();
        assertNotNull(ac);
        List<AbstractContact> personOrOrganization = ac.getPersonOrOrganization();
        assertTrue(personOrOrganization.size() == 4);
        List<Person> personList = ac.getPerson();
        assertTrue(personList.size() ==2);
        List<Organization> orgList = ac.getOrganization();
        assertTrue(orgList.size() ==2);
    }
}
