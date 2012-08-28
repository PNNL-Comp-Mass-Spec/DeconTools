package uk.ac.ebi.jmzidml.model.utils;


import junit.framework.Assert;
import org.junit.Before;
import org.junit.Test;
import uk.ac.ebi.jmzidml.model.mzidml.AbstractParam;
import uk.ac.ebi.jmzidml.model.mzidml.CvParam;
import uk.ac.ebi.jmzidml.model.mzidml.UserParam;

import java.util.ArrayList;
import java.util.Collection;
import java.util.List;

import static org.junit.Assert.*;


/**
 * ToDo: test effect of working with CvParam sublist on UserParam sublist
 * ToDo; Add test for equals methods comparing different instances of Params that contain identical value. Params equals methods have to be overridden first.
 */
public class FacadeListTest {
    private List<AbstractParam> paramList;
    private FacadeList<CvParam> cvList;
    private FacadeList<UserParam> userList;

    @Before
    public void setUp() throws Exception {
        paramList = new ArrayList<AbstractParam>();

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
        userList = new FacadeList<UserParam>(paramList, UserParam.class);
    }


    @Test
    public void testAdd() throws Exception {
        CvParam cv = new CvParam();
        cv.setAccession("CV5");
        this.cvList.add(cv);
        try {
            cv = this.cvList.get(4);
            assertTrue(cv.getAccession().equals("CV5"));
        } catch (Exception e) {
            e.printStackTrace();
            assertTrue(false);
        }
    }

    @Test(expected = NullPointerException.class)
    public void testAddNullValue() throws Exception {
        this.cvList.add(null);
    }


    /**
     * Test adding at certain index.
     *
     * @throws Exception
     */
    @Test
    public void testAddAtIndex() {
        CvParam cv = new CvParam();
        cv.setAccession("CV5");
        this.cvList.add(1, cv);
        try {

            cv = this.cvList.get(1);
            assertTrue(cv.getAccession().equals("CV5"));

            cv = this.cvList.get(4);
            assertTrue(cv.getAccession().equals("CV4"));

        } catch (Exception e) {
            e.printStackTrace();
            assertTrue(false);
        }
    }

    @Test
    public void testGet() throws Exception {

    }

    @Test
    public void testGetAtIndex() throws Exception {
        CvParam cv = this.cvList.get(0);
        assertTrue(cv.getAccession().equals("CV1"));
        cv = this.cvList.get(3);
        assertTrue(cv.getAccession().equals("CV4"));

    }

    /**
     * Test the case when an invalid index is used to try retrieve an element from the list.
     *
     * @throws Exception
     */
    @Test
    public void testGetAtIndexOutOfBounds() {
        CvParam cv = this.cvList.get(0);
        assertTrue(cv.getAccession().equals("CV1"));
        try {
            cv = this.cvList.get(4);
        } catch (Exception e) {
            assertTrue(true);
        }
    }


    @Test
    public void testSet() throws Exception {

    }

    @Test
    public void testSize() throws Exception {

    }

    @Test
    public void testIsEmpty() throws Exception {

    }

    @Test
    public void testContains() throws Exception {

    }

    @Test
    public void testIterator() throws Exception {

    }

    @Test
    public void testIndexOf() {
        CvParam cv = cvList.get(2);
        int index = cvList.indexOf(cv);

        assertTrue(index == 2);
    }

    @Test(expected = NullPointerException.class)
    public void testIndexOfPassingNullReference() {
        int index = cvList.indexOf(null);
    }

    @Test(expected = NullPointerException.class)
    public void testIndexOfProcessingNullReference() {
        cvList.set(2, null);

    }


    @Test
    public void testLastIndexOf() throws Exception {
        CvParam cv = cvList.get(1);
        cvList.add(cv);

        assertEquals(cvList.lastIndexOf(cv), 4);
    }


    /**
     * Confirm subList() method returns a sublist with right size
     *
     * @throws Exception
     */
    @Test
    public void testSubList() throws Exception {
        List<CvParam> sublist = cvList.subList(1, 3);
        assertTrue(sublist.size() == 2);
    }

    /**
     * Confirm the value returned by subList() is correct
     *
     * @throws Exception
     */
    @Test
    public void testSubListCheckValue() throws Exception {
        List<CvParam> sublist = cvList.subList(1, 3);
        assertTrue(sublist.get(0).getAccession().equals("CV2"));
        Assert.assertTrue(sublist.get(1).getAccession().equals("CV3"));
    }

    @Test(expected = UnsupportedOperationException.class)
    public void testSubListModification() throws Exception {
        List<CvParam> sublist = cvList.subList(1, 3);
        CvParam cv = new CvParam();
        cv.setAccession("Rubbish");
        sublist.add(cv);
    }

    @Test(expected = IndexOutOfBoundsException.class)
    public void testSubListCheckIndex() throws Exception {
        List<CvParam> sublist = cvList.subList(1, 5);
    }

    @Test(expected = IndexOutOfBoundsException.class)
    public void testSubListFromIndexGreaterThanToIndex() throws Exception {
        List<CvParam> sublist = cvList.subList(3, 2);
    }

    @Test
    public void testSubListToLastELement() throws Exception {
        List<CvParam> sublist = cvList.subList(1, 4);
        Assert.assertTrue(sublist.size() == 3);
    }

    @Test
    public void testSubListChangeToOriginalList() throws Exception {
        List<CvParam> sublist = cvList.subList(1, 4);
        CvParam cv = sublist.get(0);
        cv.setAccession("New CV2");

        CvParam cv1 = cvList.get(1);
        Assert.assertTrue(cv1.getAccession().equals("New CV2"));
    }

    /**
     * Confirm toArray() returns an array with the right size
     *
     * @throws Exception
     */
    @Test
    public void testToArray() throws Exception {
        Object[] arr = cvList.toArray();
        Assert.assertTrue(arr.length == 4);
    }

    /**
     * Confirms changes on element instance within the array will be made to the original list as well.
     *
     * @throws Exception
     */
    @Test
    public void testToArrayChangeInstance() throws Exception {
        Object[] arr = cvList.toArray();
        CvParam cv = (CvParam) arr[0];
        cv.setAccession("CV12");
        Assert.assertTrue(cvList.get(0).getAccession().equals("CV12"));
    }

    /**
     * Changes to reference in returned array are not reflected in list. Setting an array element to a new object does
     * not change the reference in the original list.
     *
     * @throws Exception
     */
    @Test
    public void testToArrayChangeInstanceReference() throws Exception {
        Object[] arr = cvList.toArray();
        CvParam cv = new CvParam();
        cv.setAccession("CV12");
        arr[0] = cv;
        Assert.assertTrue(cvList.get(0).getAccession().equals("CV1"));
    }


    /**
     * ******************************* toArray(T[]) ********************************
     */
    @Test
    public void testToArrayProvidingArrayWithData() throws Exception {
        CvParam[] cvParams = new CvParam[6];
        CvParam cv = new CvParam();
        cv.setAccession("newCV1");
        cvParams[0] = cv;

        cv = new CvParam();
        cv.setAccession("newCV2");
        cvParams[1] = cv;

        cv = new CvParam();
        cv.setAccession("newCV3");
        cvParams[2] = cv;

        cv = new CvParam();
        cv.setAccession("newCV4");
        cvParams[3] = cv;

        cv = new CvParam();
        cv.setAccession("newCV5");
        cvParams[4] = cv;

        cv = new CvParam();
        cv.setAccession("newCV6");
        cvParams[5] = cv;

        CvParam[] returnedArray = this.cvList.toArray(cvParams);
        assertTrue(returnedArray[0].getAccession().equals("CV1"));
        assertTrue(returnedArray[4] == null);
        assertTrue(returnedArray[5].getAccession().equals("newCV6"));

    }

    @Test
    public void testToArrayProvidingArrayChangingValues() throws Exception {
        CvParam[] cvParams = new CvParam[5];
        CvParam[] returnedParams = this.cvList.toArray(cvParams);
        returnedParams[0].setAccession("newCV1");
        assertTrue(this.cvList.get(0).getAccession().equals(("newCV1")));
    }

    @Test
    public void testToArrayProvidingArrayChangingReference() throws Exception {
        CvParam[] cvParams = new CvParam[5];
        CvParam[] returnedParams = this.cvList.toArray(cvParams);
        CvParam newCvParam = new CvParam();
        newCvParam.setAccession("newCV1");
        returnedParams[0] = newCvParam;
        assertTrue(this.cvList.get(0).getAccession().equals(("CV1")));
    }


    @Test
    public void testToArrayProvidingArray() throws Exception {
        CvParam[] cvParams = new CvParam[5];
        CvParam[] returnedParams = this.cvList.toArray(cvParams);
        assertTrue(returnedParams[0].getAccession().equals(("CV1")));
        assertTrue(returnedParams[4] == null);
    }

    @Test
    public void testToArrayProvidingSmallerArray() throws Exception {
        CvParam[] cvParams = new CvParam[1];
        CvParam[] returnedParams = this.cvList.toArray(cvParams);
        assertTrue(returnedParams[0].getAccession().equals(("CV1")));
        assertTrue(returnedParams.length == 4);
    }

    @Test(expected = NullPointerException.class)
    public void testToArrayProvidingNullArray() throws Exception {
        CvParam[] nullArray = null;
        CvParam[] returnedParams = this.cvList.toArray(nullArray);
    }

    /************************** Test remove() ***********************************/
    /**
     * Remove an existing element from the sublist
     *
     * @throws Exception
     */
    @Test
    public void testRemove() throws Exception {
        CvParam cv = cvList.get(0);
        cvList.remove(cv);
        assertTrue(!cvList.contains(cv));
    }

    /**
     * Remove null should throw an NullPointerException
     * @throws Exception
     */
    @Test(expected = NullPointerException.class)
    public void testRemoveNullObject() throws Exception {
        cvList.remove(null);
    }

    /**
     * Remove a user param should throw a ClassCastException
     * @throws Exception
     */
    @Test (expected = ClassCastException.class)
    public void testRemoveUserParam() throws Exception {
        UserParam userParam = (UserParam)paramList.get(2);
        cvList.remove(userParam);
    }

    /**
     * Remove a cv param not in the list
     *
     * @throws Exception
     */
    @Test
    public void testRemoveNewCvParam() throws Exception {
        CvParam cv = new CvParam();
        cv.setAccession("NewCV");
        assertTrue(!cvList.remove(cv));
    }

    /********************************** containsAll ***********************************/
    @Test
    public void testContainsAll() throws Exception {
        List<CvParam> testList = new ArrayList<CvParam>();
        testList.add(this.cvList.get(0));
        testList.add(this.cvList.get(1));
        assertTrue(this.cvList.containsAll(testList));
    }

    @Test
    public void testContainsAllWithAdditionalCvParam() throws Exception {
        List<CvParam> testList = new ArrayList<CvParam>();
        testList.add(this.cvList.get(0));
        testList.add(this.cvList.get(1));
        CvParam cv = new CvParam();
        cv.setAccession("additionalCv");
        testList.add(cv);
        assertFalse(this.cvList.containsAll(testList));
    }

    @Test(expected = NullPointerException.class)
    public void testContainsAllWithNull() throws Exception {
        List<CvParam> testList = new ArrayList<CvParam>();
        testList.add(this.cvList.get(0));
        testList.add(this.cvList.get(1));
        testList.add(null);
        this.cvList.containsAll(testList);
    }


    @Test(expected = NullPointerException.class)
    public void testContainsAllWithNullCollection() throws Exception {
        this.cvList.containsAll(null);
    }


    @Test(expected = ClassCastException.class)
    public void testContainsAllWithWrongClasses() throws Exception {
        List testList = new ArrayList();
        testList.add("wrong class");
        testList.add(this.cvList.get(1));
        this.cvList.containsAll(testList);
    }

    /********************************* clear **********************************/
    @Test
    public void testClear() throws Exception {
        this.cvList.clear();
        assertTrue(this.cvList.size() == 0);
        assertTrue(this.paramList.size() == 3);
        assertTrue(this.paramList.get(0).getName().equals("User1"));
    }

    /******************************** addAll **********************************/
    @Test
    public void testAddAll() throws Exception {
        List<CvParam> testList = new ArrayList<CvParam>();
        int prevCvListSize = this.cvList.size();
        int prevParamListSize = this.paramList.size();
        CvParam cv = new CvParam();
        cv.setAccession("newCV1");
        testList.add(cv);
        cv = new CvParam();
        cv.setAccession("newCV2");
        testList.add(cv);
        cv = new CvParam();
        cv.setAccession("newCV3");
        testList.add(cv);
        this.cvList.addAll(testList);
        // Confirm new size of sublist is correct
        assertTrue((prevCvListSize + testList.size()) == this.cvList.size());
        // Confirm last element in sublist is last new element added.
        assertTrue(this.cvList.get(this.cvList.size()-1).getAccession().equals("newCV3"));
        // Confirm param list size has changed also.
        assertTrue(this.paramList.size() == (prevParamListSize+testList.size()));
    }

    @Test(expected = NullPointerException.class)
    public void testAddAllWithNullElement() throws Exception {
        List<CvParam> testList = new ArrayList<CvParam>();
        CvParam cv = new CvParam();
        cv.setAccession("newCV1");
        testList.add(cv);
        testList.add(null);
        this.cvList.addAll(testList);
    }


    @Test(expected = NullPointerException.class)
    public void testAddAllWithNullCollection() throws Exception {
        List<CvParam> testList = new ArrayList<CvParam>();
        this.cvList.addAll(null);
    }

    @Test (expected = ClassCastException.class)
    public void testAddAllClassCastException() throws Exception {
        List testList = new ArrayList<CvParam>();
        CvParam cv = new CvParam();
        cv.setAccession("newCV1");
        testList.add(cv);
        testList.add("test");
        this.cvList.addAll(testList);
    }

    /********************************* Test addAll() with index ************************************/
    /**
     * Add a collection to the start of the sublist
     * @throws Exception
     */
    @Test
    public void tesAddAllToStart() throws Exception {
        // store the size of the original list
        int originalSize = paramList.size();
        int originalSubListSize = cvList.size();

        Collection<CvParam> cvs = new ArrayList<CvParam>();

        CvParam cv1 = new CvParam();
        cv1.setAccession("NewCV1");
        cvs.add(cv1);

        CvParam cv2 = new CvParam();
        cv2.setAccession("NewCV2");
        cvs.add(cv2);

        this.cvList.addAll(0, cvs);

        // check the size of the original list
        assertTrue(paramList.size() == (originalSize + cvs.size()));
        // check the size of the sub list
        assertTrue(cvList.size() == (originalSubListSize + cvs.size()));
        // check the element is correct
        CvParam cv = cvList.get(0);
        assertTrue(cv.getAccession().equals("NewCV1"));
        // check the original starting element has been moved to the correct position
        cv = cvList.get(2);
        assertTrue(cv.getAccession().equals("CV1"));
    }

    /**
     * Add a collection the middle of the sublist
     * @throws Exception
     */
    @Test
    public void testAddAllToMiddle() throws Exception {
        // store the size of the original list
        int originalSize = paramList.size();
        int originalSubListSize = cvList.size();

        Collection<CvParam> cvs = new ArrayList<CvParam>();

        CvParam cv1 = new CvParam();
        cv1.setAccession("NewCV1");
        cvs.add(cv1);

        CvParam cv2 = new CvParam();
        cv2.setAccession("NewCV2");
        cvs.add(cv2);

        this.cvList.addAll(2, cvs);

        // check the size of the original list
        assertTrue(paramList.size() == (originalSize + cvs.size()));
        // check the size of the sub list
        assertTrue(cvList.size() == (originalSubListSize + cvs.size()));
        // check the element is correct
        CvParam cv = cvList.get(2);
        assertTrue(cv.getAccession().equals("NewCV1"));
        // check the original starting element has been moved to the correct position
        cv = cvList.get(4);
        assertTrue(cv.getAccession().equals("CV3"));
    }

    /**
     * Add a collection to the end of the list
     * @throws Exception
     */
    @Test
    public void testAddAllToEnd() throws Exception {
        // store the size of the original list
        int originalSize = paramList.size();
        int originalSubListSize = cvList.size();

        Collection<CvParam> cvs = new ArrayList<CvParam>();

        CvParam cv1 = new CvParam();
        cv1.setAccession("NewCV1");
        cvs.add(cv1);

        CvParam cv2 = new CvParam();
        cv2.setAccession("NewCV2");
        cvs.add(cv2);

        this.cvList.addAll(3, cvs);

        // check the size of the original list
        assertTrue(paramList.size() == (originalSize + cvs.size()));
        // check the size of the sub list
        assertTrue(cvList.size() == (originalSubListSize + cvs.size()));
        // check the element is correct
        CvParam cv = cvList.get(3);
        assertTrue(cv.getAccession().equals("NewCV1"));
        // check the original starting element has been moved to the correct position
        cv = cvList.get(5);
        assertTrue(cv.getAccession().equals("CV4"));
    }

    /**
     * Add a null collection should throw an NullPointerException
     * @throws Exception
     */
    @Test (expected = NullPointerException.class)
    public void testAddAllNull() throws Exception {
        this.cvList.addAll(0, null);
    }

    /**
     * Add a collection contains null element should throw an NullPointerException
     * @throws Exception
     */
    @Test (expected = NullPointerException.class)
    public void testAddAllNullElement() throws Exception {
        Collection<CvParam> cvs = new ArrayList<CvParam>();

        CvParam cv1 = new CvParam();
        cv1.setAccession("NewCV1");
        cvs.add(cv1);
        cvs.add(null);

        this.cvList.addAll(0, cvs);
    }

    /**
     * Confirm IndexOutOfBoundException with illegal index
     * @throws Exception
     */
    @Test (expected = IndexOutOfBoundsException.class)
    public void testAddAllIllegalIndex() throws Exception {
        Collection<CvParam> cvs = new ArrayList<CvParam>();

        CvParam cv1 = new CvParam();
        cv1.setAccession("NewCV1");

        this.cvList.addAll(4, cvs);
    }

    /**
     * Confirm ClassCastException if input collection contains illegal object types
     *
     * @throws Exception
     */
    @Test (expected = ClassCastException.class)
    public void testAddAllMixedElementType() throws Exception {
        Collection cvs = new ArrayList();

        CvParam cv1 = new CvParam();
        cv1.setAccession("NewCV1");
        cvs.add(cv1);

        UserParam up1 = new UserParam();
        up1.setName("NewUserParam1");
        cvs.add(up1);

        this.cvList.addAll(1, cvs);
    }

    @Test
    public void testRemoveAll() throws Exception {
        // store the size of the original list
        int originalSize = paramList.size();
        int originalSubListSize = cvList.size();

        Collection<CvParam> cvs = new ArrayList<CvParam>();
        cvs.add(this.cvList.get(0));
        cvs.add(this.cvList.get(1));
        boolean success = this.cvList.removeAll(cvs);
        assertTrue(success);
        // check the size of the original list
        assertTrue(paramList.size() == (originalSize - cvs.size()));
        // check the size of the sub list
        assertTrue(cvList.size() == (originalSubListSize - cvs.size()));
        // check the element is correct
        CvParam cv = cvList.get(0);
        assertTrue(cv.getAccession().equals("CV3"));
    }

    /**
     * Try removeAll supplying a collection that only contains objects NOT present in target collection.
     * @throws Exception
     */
    @Test
    public void testRemoveAllWithNoMatches() throws Exception{
        // store the size of the original list
        int originalSize = paramList.size();
        int originalSubListSize = cvList.size();

        Collection<CvParam> cvs = new ArrayList<CvParam>();
        CvParam cv = new CvParam();
        cv.setAccession("NewCV1");
        cvs.add(cv);
        cv = new CvParam();
        cv.setAccession("NewCV2");
        cvs.add(cv);
        boolean success = this.cvList.removeAll(cvs);
        assertTrue(!success);
        // check the size of the original list
        assertTrue(paramList.size() == originalSize);
        // check the size of the sub list
        assertTrue(cvList.size() == originalSubListSize);
        // check the element is correct
        cv = cvList.get(0);
        assertTrue(cv.getAccession().equals("CV1"));

    }

    @Test(expected = NullPointerException.class)
    public void testRemoveAllNullCollection() throws Exception{
        this.cvList.removeAll(null);
    }

    @Test(expected = NullPointerException.class)
    public void testRemoveAllNullElement() throws Exception{
        Collection<CvParam> cvs = new ArrayList<CvParam>();
        cvs.add(null);
        this.cvList.removeAll(cvs);
    }

    @Test(expected = ClassCastException.class)
    public void testRemoveAllClassCastException() throws Exception{
        Collection cvs = new ArrayList();
        int originalSize = this.paramList.size();
        int userSize = this.userList.size();
        cvs.add(this.userList.get(0));
        assertTrue(originalSize == this.paramList.size());
        assertTrue(userSize == this.userList.size());
        this.cvList.removeAll(cvs);
    }


    @Test
    public void testRetainAll() throws Exception {
        // store the size of the original list
        int originalSize = paramList.size();
        int originalSubListSize = cvList.size();

        Collection<CvParam> cvs = new ArrayList<CvParam>();
        cvs.add(this.cvList.get(0));
        cvs.add(this.cvList.get(1));
        boolean success = this.cvList.retainAll(cvs);
        assertTrue(success);
        // check the size of the original list
        assertTrue(paramList.size() == (originalSize - originalSubListSize + cvs.size()));
        // check the size of the sub list
        assertTrue(cvList.size() == cvs.size());
        // check the element is correct
        CvParam cv = cvList.get(0);
        assertTrue(cv.getAccession().equals("CV1"));
    }

    /**
     * Use a mixture of matching and non-matching objects.
     * @throws Exception
     */
    @Test
    public void testRetainAllWithSomeNonMatchingObjects() throws Exception {
        // store the size of the original list
        int originalSize = paramList.size();
        int originalSubListSize = cvList.size();

        Collection<CvParam> cvs = new ArrayList<CvParam>();
        cvs.add(this.cvList.get(0));
        cvs.add(this.cvList.get(1));
        CvParam cv = new CvParam();
        cv.setAccession("newCV1");
        cvs.add(cv);
        boolean success = this.cvList.retainAll(cvs);
        assertTrue(success);
        // check the size of the original list
        assertTrue(paramList.size() == (originalSize - originalSubListSize + (cvs.size()-1)));
        // check the size of the sub list
        assertTrue(cvList.size() == cvs.size()-1);
        // check the element is correct
        cv = cvList.get(0);
        assertTrue(cv.getAccession().equals("CV1"));
    }

    @Test(expected = NullPointerException.class)
    public void testRetainAllNullCollection() throws Exception{
        this.cvList.retainAll(null);
    }

    @Test(expected = NullPointerException.class)
    public void testRetainAllNullElement() throws Exception{
        Collection<CvParam> cvs = new ArrayList<CvParam>();
        cvs.add(null);
        this.cvList.retainAll(cvs);
    }

    /**
     * Pass in a userParam. Specifically use an instance contained in the same original list rather than a new instance.
     *
     *
     * @throws Exception
     */
    @Test(expected = ClassCastException.class)
    public void testRetainAllClassCastException() throws Exception{
        Collection cvs = new ArrayList();
        int originalSize = this.paramList.size();
        int userSize = this.userList.size();
        cvs.add(this.userList.get(0));
        assertTrue(originalSize == this.paramList.size());
        assertTrue(userSize == this.userList.size());
        this.cvList.retainAll(cvs);
    }

    /**
     * Should clear the list as nothing will match
     * @throws Exception
     */
    @Test
    public void testRetainAllWithEmptyList() throws Exception{
        Collection cvs = new ArrayList();
        boolean changed = this.cvList.retainAll(cvs);
        assertTrue(changed);
    }

    

    /*********************************** equals ************************************/
    @Test
    public void testEquals() throws Exception{
        assertTrue(this.cvList.equals(this.cvList));
    }

    @Test
    public void testNotEquals() throws Exception{
        assertTrue(!this.cvList.equals(this.paramList));
    }

    @Test
    public void testEqualsSameCvParamDiffList() throws Exception{
        List<AbstractParam> newParamList = new ArrayList<AbstractParam>(paramList.size());
        newParamList.addAll(paramList);
        FacadeList<CvParam> newCvList = new FacadeList<CvParam>(newParamList, CvParam.class);
        assertTrue(this.cvList.equals(newCvList));
    }

    @Test
    public void testEqualsSameCvParamDiffUserParamOrder() throws Exception{
        List<AbstractParam> newParamList = new ArrayList<AbstractParam>(paramList.size());
        newParamList.addAll(paramList);
        UserParam userParam = this.userList.get(0);
        this.userList.set(1, userParam);
        FacadeList<CvParam> newCvList = new FacadeList<CvParam>(newParamList, CvParam.class);
        assertTrue(this.cvList.equals(newCvList));
    }

    /********************************* hashcode *************************************/

    @Test
    public void testHashCode() throws Exception{
        assertTrue(this.cvList.hashCode() == this.cvList.hashCode());
    }

    @Test
    public void testHashCodeWithNewElement() throws Exception{
        int originalHashCode = this.cvList.hashCode();
        CvParam cv = new CvParam();
        cv.setAccession("newCV1");
        this.cvList.add(cv);
        assertFalse(originalHashCode == this.cvList.hashCode());
    }

    @Test
    public void testHashCodeWithPositionModifiedUserParamsElements() throws Exception{
        List<AbstractParam> newParamList = new ArrayList<AbstractParam>(paramList.size());
        newParamList.addAll(paramList);
        UserParam userParam = this.userList.get(0);
        this.userList.set(1, userParam);
        FacadeList<CvParam> newCvList = new FacadeList<CvParam>(newParamList, CvParam.class);
        assertTrue(this.cvList.hashCode() == newCvList.hashCode());
    }

    @Test
    public void testHashCodeWithSwappedCvParamPositions() throws Exception{
        int originalHashCode = this.cvList.hashCode();
        CvParam cv = this.cvList.get(0);
        CvParam cv2 = this.cvList.get(1);
        this.cvList.set(0, cv2);
        this.cvList.set(1, cv);
        assertFalse(this.cvList.hashCode() == originalHashCode);
    }
}
