package uk.ac.ebi.jmzidml.model.utils;

import junit.framework.Assert;
import org.junit.Before;
import org.junit.Test;
import uk.ac.ebi.jmzidml.model.mzidml.Cv;
import uk.ac.ebi.jmzidml.model.mzidml.CvParam;
import uk.ac.ebi.jmzidml.model.mzidml.UserParam;
import uk.ac.ebi.jmzidml.model.mzidml.params.AmbiguousResidueCvParam;
import uk.ac.ebi.jmzidml.model.mzidml.params.AmbiguousResidueUserParam;

import java.util.ArrayList;
import java.util.List;



/**
 * @author Florian Reisinger
 *         Date: 18-Oct-2010
 * @since 1.0
 */
public class ParamUpdaterTest {

    CvParam param1;
    CvParam param2;
    UserParam userParam1;
    UserParam userParam2;

    @Before
    public void setup() {
        param1 = new CvParam();
        param1.setAccession("PSI:012345");
        Cv cv = new Cv();
        cv.setFullName("PSI ontology");
        cv.setId("PSI");
        param1.setCv(cv);
        param1.setName("test1 param");
        param1.setValue("test1");


        param2 = new CvParam();
        param2.setAccession("PSI:098765");
        param2.setCv(cv);
        param2.setName("test2 param");
        param2.setValue("test2");

        userParam1 = new UserParam();
        userParam1.setName("user test param 1");
        userParam1.setValue("value 1");

        userParam2 = new UserParam();
        userParam2.setName("user test param 2");
        userParam2.setValue("value 2");
    }

    @Test
    public void testSubListClassing() throws IllegalAccessException, InstantiationException {

        List<CvParam> list = new ArrayList<CvParam>();
        list.add(param1);
        list.add(param2);
        // not sub-classed yet
        for (CvParam cvParam : list) {
            Assert.assertFalse("Params can not be of sub-class type yet!",  (cvParam instanceof AmbiguousResidueCvParam) );
        }
        // sub-class the params
        ParamUpdater.updateCvParamSubclassesList(list, AmbiguousResidueCvParam.class);
        // check the sub-classing
        for (CvParam cvParam : list) {
            Assert.assertTrue("Params have to be of sub-class type!", (cvParam instanceof AmbiguousResidueCvParam) );
        }
    }


    @Test
    public void testSubClassing() throws InstantiationException, IllegalAccessException {

        Assert.assertNotNull(param1.getAccession());
        Assert.assertFalse("Param can not be of sub-class type yet!", (param1 instanceof AmbiguousResidueCvParam) );
        CvParam newCvParam = ParamUpdater.updateCvParamSubclass(param1, AmbiguousResidueCvParam.class);
        Assert.assertTrue("Param has to be of sub-class type!", (newCvParam instanceof AmbiguousResidueCvParam));

        Assert.assertNotNull(userParam1.getName());
        Assert.assertFalse("Param can not be of sub-class type yet!", (userParam1 instanceof AmbiguousResidueUserParam) );
        userParam1 = ParamUpdater.updateUserParamSubclass(userParam1, AmbiguousResidueUserParam.class);
        Assert.assertTrue("Param has to be of sub-class type!", (userParam1 instanceof AmbiguousResidueUserParam));
    }

}
