package uk.ac.ebi.jmzidml.model.utils;

import org.junit.Before;
import org.junit.Test;
import uk.ac.ebi.jmzidml.model.mzidml.AbstractParam;
import uk.ac.ebi.jmzidml.model.mzidml.CvParam;
import uk.ac.ebi.jmzidml.model.mzidml.UserParam;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.NoSuchElementException;

import static org.junit.Assert.assertTrue;

/**
 * Created by IntelliJ IDEA.
 * User: rwang
 * Date: 04/02/11
 * Time: 10:37
 * To change this template use File | Settings | File Templates.
 */
public class FacadeIteratorTest {

    private FacadeList<CvParam> cvList;

    @Before
    public void setUp() throws Exception {
        List<AbstractParam> paramList = new ArrayList<AbstractParam>();

        CvParam cv = new CvParam();
        cv.setAccession("CV1");
        paramList.add(cv);

        CvParam cv1 = new CvParam();
        cv1.setAccession("CV2");
        paramList.add(cv1);

        UserParam user = new UserParam();
        user.setName("User1");
        paramList.add(user);

        UserParam user1 = new UserParam();
        user1.setName("User2");
        paramList.add(user1);

        CvParam cv2 = new CvParam();
        cv2.setAccession("CV3");
        paramList.add(cv2);

        CvParam cv3 = new CvParam();
        cv3.setAccession("CV4");
        paramList.add(cv3);

        UserParam user2 = new UserParam();
        user2.setName("User3");
        paramList.add(user2);

        cvList = new FacadeList<CvParam>(paramList, CvParam.class);
    }

    /**
     * ******************************** hasNext **********************************
     */
    @Test
    public void testIteratorHasNext() throws NoSuchElementException {
        Iterator<CvParam> cvIter = cvList.iterator();
        // test hasnext
        cvIter.hasNext();
        cvIter.hasNext();
        cvIter.hasNext();
        cvIter.hasNext();
    }

    @Test
    public void testIteratorHasNextFailsAtEndOfList() {
        Iterator<CvParam> cvIter = cvList.iterator();
        cvIter.next();
        cvIter.next();
        cvIter.next();

        try {
            cvIter.next();
        } catch (NoSuchElementException e) {
            assertTrue(true);
        } catch (Exception e) {
            assertTrue(false);
        }

    }

    /**
     * ************************************** next ************************************************
     */
    @Test
    public void testIteratorNext() {
        Iterator<CvParam> cvIter = cvList.iterator();

        // remove the first cv param
        CvParam cv = cvIter.next();
        assertTrue(cv.getAccession().equals("CV1"));
    }

    /**
     * *************************************** remove *******************************************
     */
    @Test
    public void testIteratorRemove() {
        Iterator<CvParam> cvIter = cvList.iterator();
        cvIter.next();
        cvIter.next();
        cvIter.remove();
        CvParam cv = cvIter.next();
        assertTrue(cv.getAccession().equals("CV3"));
    }

    /**
     * Get an iterator, call next one or more times then remove an instance from the list using the remove
     * method of list. Call next on the iterator again and check returned instance. This is to test the
     * consistency of the iterator.
     */
    @Test
    public void testIteratorInconsistency() {
        Iterator<CvParam> cvIter = cvList.iterator();

        // remove the first cv param
        cvIter.next();
        cvIter.next();
        cvIter.next();
        CvParam cv = cvIter.next();
        assertTrue(cv.getAccession().equals("CV4"));

        // remove one element
        cvList.remove(cv);

        // get next
        try {
            cv = cvIter.next();
            assertTrue(false);
        } catch (Exception ex) {
            assertTrue(ex instanceof NoSuchElementException);
        }
        //  assertTrue(false);
    }


    @Test
    public void testAddAtIndexWithIteratorBefore() {
        Iterator<CvParam> cvs = cvList.iterator();

        cvs.next();
        cvs.next();
        cvs.next();

        CvParam cv = new CvParam();
        cv.setAccession("New Cv 1");
        cvList.add(1, cv);

        assertTrue(cvs.next().getAccession().equals("CV3"));
    }

    @Test
    public void testAddAtIndexWithIteratorAfter() {
        Iterator<CvParam> cvs = cvList.iterator();

        cvs.next();
        cvs.next();

        CvParam cv = new CvParam();
        cv.setAccession("New Cv 4");
        cvList.add(3, cv);

        assertTrue(cvs.next().getAccession().equals("CV3"));

        assertTrue(cvs.next().getAccession().equals("New Cv 4"));

        assertTrue(cvs.next().getAccession().equals("CV4"));
    }

    /**
     * Confirm for each loop uses FacadeListIterator
     *
     * @throws Exception
     */
    @Test
    public void testForEach() throws Exception {
        for (Object o : this.cvList) {
            if (o instanceof CvParam == false) {
                assertTrue(false);
            }
        }
    }
}
