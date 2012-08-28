/**
 *
 * User: riteshk
 * Date: Sep 18, 2010
 *
 */

package uk.ac.ebi.jmzidml.test.xml;

import junit.framework.TestCase;
import org.apache.log4j.Logger;
import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.*;
import uk.ac.ebi.jmzidml.xml.io.MzIdentMLUnmarshaller;

import java.net.URL;

public class ProviderTest extends TestCase{

    Logger log = Logger.getLogger(ProviderTest.class);

    
    public void testProviderInformation() throws Exception {
        log.info("testing the <Provider> content.");
        URL xmlFileURL = ProviderTest.class.getClassLoader().getResource("Mascot_MSMS_example.mzid");
        assertNotNull(xmlFileURL);

        MzIdentMLUnmarshaller unmarshaller = new MzIdentMLUnmarshaller(xmlFileURL);
        assertNotNull(unmarshaller);

        // Number of providers
        int totalProvider = unmarshaller.getObjectCountForXpath(MzIdentMLElement.Provider.getXpath());
        assertEquals(1, totalProvider);
        Provider provider = unmarshaller.unmarshal(MzIdentMLElement.Provider);
        assertNotNull(provider);
        assertEquals("PROVIDER", provider.getId());
        assertNotNull(provider.getContactRole().getRole().getCvParam());
        if (MzIdentMLElement.Provider.isAutoRefResolving() && provider.getSoftwareRef() != null) {
            AnalysisSoftware software = provider.getSoftware();
            assertNotNull(software);
            assertNotNull(software.getId());
        } else {
            System.out.println("Provider is not auto-resolving or does not contain a Software reference.");
            assertNull(provider.getSoftware());
        }

        ContactRole cr = provider.getContactRole();
        assertNotNull(cr);
        if (MzIdentMLElement.ContactRole.isAutoRefResolving() && cr.getContactRef() != null) {
            AbstractContact contact = cr.getContact();
            assertNotNull("There should be a contact!", contact);
            assertEquals(cr.getContactRef(), contact.getId());
            //assertTrue(cr.getContact().getEmail().contains("@"));
            assertTrue("The contact should be a Person!", contact instanceof Person);
            Person p = (Person) contact;
            assertNotNull(p);
            assertEquals("The person should have one affiliation!", 1, p.getAffiliation().size());
            Affiliation aff = p.getAffiliation().get(0);
            assertNotNull(aff);
            if (MzIdentMLElement.Affiliation.isAutoRefResolving() && aff.getOrganizationRef() != null) {
                assertNotNull(aff.getOrganization());
                assertEquals(aff.getOrganizationRef(), aff.getOrganization().getId());
            } else {
                System.out.println("Affiliations is not auto-resolving or does not contain a Organization reference.");
                assertNull(aff.getOrganization());
            }
        } else {
            System.out.println("ContactRole is not auto-resolving or does not contain a Contact reference.");
            assertNull(cr.getContact());
        }

        
        assertEquals("researcher", cr.getRole().getCvParam().getName());

    }
    
}
