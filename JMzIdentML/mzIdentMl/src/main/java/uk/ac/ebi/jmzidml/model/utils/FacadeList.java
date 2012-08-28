package uk.ac.ebi.jmzidml.model.utils;

import java.util.*;

/**
 * Created by IntelliJ IDEA.
 * User: rwang
 * Date: 26/01/11
 * Time: 10:16
 * <p/>
 * Controls access to a standard java list which contains more than one instance type, providing the developer with
 * a virtual list of a specified type. When this list is created, the developer must specify the class of interest.
 * Methods called on this class will be applied to the original list but will only act on instances of the specified
 * type.
 * For example, an originalList could contain a mixture of CvParams and UserParams. To work with a list of CvParams,
 * a developer creates an instance of this class passing in 'CvParam.class' as clazz in the constructor.
 * If the size method is called the list be searched and only instances of CvParam are counted towards the size.
 * Likewise, if get(3) (3 is the index) is called the 3rd instance of CvParam will be returned. Note, this CvParam might
 * not be the third element in the originallist.
 * <p/>
 * <p/>
 * TODO Implement CvParam and UserParam's toString, equals, hashcode. With equals objects are normally considered equals if contents match.
 * TODO Check iterator working with foreach
 * TODO check the checkIndex(), maybe not the best implementation
 * TODO finish all the add methods with checking the null input values
 */
public class FacadeList<T> extends AbstractCollection<T> implements List<T> {
    private List originalList;
    private Class<T> clazz;

    public FacadeList(List originalList, Class<T> clazz) {
        if (originalList == null || clazz == null) {
            throw new IllegalArgumentException("Error: Neither original input list nor sublist class can be NULL");
        }
        this.originalList = originalList;
        this.clazz = clazz;
    }

    private FacadeList() {
        // set to private
    }

    public boolean add(T t) {
        checkArgument(t);
        return originalList.add(t);
    }

    /**
     * Remove a element from the original list
     * The index here is the index of the specified element int the original list
     *
     * @param index list index
     * @return T   element has been removed
     */
    public T remove(int index) {
        T elementAtIndex = this.getAtIndex(index);
        this.originalList.remove(elementAtIndex);
        return elementAtIndex;
    }

    /**
     * Get an element from the original list at index.
     *
     * @param index list index
     * @return T element to get
     */
    public T get(int index) {
        return this.getAtIndex(index);
    }

    /**
     * Set an new element based on the index of the sublist
     *
     * @param index   index of the sublist
     * @param element new element
     * @return T    old element in the position
     */
    public T set(int index, T element) {
        this.checkArgument(element);
        this.checkIndex(index);
        return this.setAtIndex(index, element);
    }

    /**
     * Get the size of the sublist
     *
     * @return int  size of the sublist
     */
    public int size() {
        int cnt = 0;

        for (Object anOriginalList : this.originalList) {

            if (clazz.isInstance(anOriginalList)) {
                cnt++;
            }
        }

        return cnt;
    }

    /**
     * Check whether the sublist is empty
     *
     * @return boolean true means empty
     */
    public boolean isEmpty() {
        boolean em = false;

        for (Object anOriginalList : this.originalList) {
            Object o = anOriginalList;
            if (clazz.isInstance(o)) {
                em = true;
                break;
            }
        }

        return em;
    }

    /**
     * Check whether the sublist contains the element
     *
     * @param o input object
     * @return boolean     true means sublist contains the input object
     */
    public boolean contains(Object o) {
        checkArgument(o);
        return this.originalList.contains(o);
    }

    /**
     * Get an iterator of the sublist
     *
     * @return Iterator<T> an iterator of the sublist
     */
    public Iterator<T> iterator() {
        return new SublistIterator(this.originalList);
    }

    /**
     * Get the index of sublist using a given object
     *
     * @param o input object
     * @return int index of the object
     */
    public int indexOf(Object o) {
        checkArgument(o);
        int cnt = 0;
        for (Object anOriginalList : this.originalList) {
            if (clazz.isInstance(anOriginalList)) {
                if (o.equals(anOriginalList)) {
                    return cnt;
                }
                cnt++;
            }
        }
        return -1;
    }

    /**
     * Get the last index of sublist using a given object
     *
     * @param o input object
     * @return int index of the object
     */
    public int lastIndexOf(Object o) {
        checkArgument(o);

        int pointer = -1;
        int size = this.size();

        if (size > 0) {
            for (int i = (size - 1); i >= 0; i--) {
                if (o.equals(this.get(i))) {
                    pointer = i;
                    break;
                }
            }
        }

        return pointer;
    }

    public ListIterator<T> listIterator() {
        return new SubListListIterator(originalList);
    }

    public ListIterator<T> listIterator(int index) {
        return new SubListListIterator(originalList, index);
    }

    /**
     * sublist returned is an unmodifiable list, structural change to the sublist is not allowed.
     * Please note: this behaves differently from the definition of the interface.
     * Changes to the element in sublist will affect the element in the original list
     * However, the behaviour is undefined if the original list has been changed
     *
     * @param fromIndex low endpoint (inclusive) of the sublist
     * @param toIndex   high endpoint (exclusive) of the sublist
     * @return  List<T> unmodifiable sublist
     */
    public List<T> subList(int fromIndex, int toIndex) {
        checkIndex(fromIndex);
        checkIndex(toIndex - 1);

        if (fromIndex > toIndex) {
            throw new IndexOutOfBoundsException("The start index needs to be greater than the end index: start index: " + fromIndex + " end index: " + toIndex);
        }

        if (toIndex > this.size()) {
            throw new IndexOutOfBoundsException("The end index should not be greater than the size of the list: " + toIndex);
        }


        List<T> result = new ArrayList<T>();
        int cnt = 0;
        int diff = toIndex - fromIndex;
        ListIterator<T> listIter = this.listIterator(fromIndex);
        while(listIter.hasNext()) {
            if (cnt < diff) {
                result.add(listIter.next());
            } else {
                break;
            }
            cnt++;
        }

        return Collections.unmodifiableList(result);
    }


    public Object[] toArray() {
        Object[] arr = new Object[this.size()];
        int index = 0;
        for (T element : this) {
            arr[index] = element;
            index++;
        }
        return arr;
    }


    public <T> T[] toArray(T[] a) {
      int size = this.size();
      if (a.length < size)
            // Make a new array of a's runtime type, but my contents:
            return (T[]) Arrays.copyOf(this.toArray(), size, a.getClass());

        System.arraycopy(this.toArray(), 0, a, 0, size);
        if (a.length > size)
            a[size] = null;
        return a;

    }


    /**
     * Add a new element to the sublist
     * This will add the new element to the immediate index after the element at (index -1) in the sublist
     *
     * @param index   index of the sublist
     * @param element the new element
     */
    public void add(int index, T element) {
        this.checkArgument(element);
        this.checkIndex(index);
        this.addAtIndex(index, element);
    }


    /**
     * Remove an object from the sublist
     *
     * @param o object to be removed
     * @return  boolean true a object has been found and removed
     */
    public boolean remove(Object o) {
        checkArgument(o);
        return this.originalList.remove(o);
    }



    public boolean addAll(int index, Collection<? extends T> c) {
        checkIndex(index);
        this.checkArgumentsInCollection(c);
        // find index in the original list first
        int originalIndex = getOriginalIndex(index);
        // call addAll() of the original list
        return originalList.addAll(originalIndex, c);
    }

    private void checkArgumentsInCollection(Collection<?> c) {
        if (c == null) {
            throw new NullPointerException("Input collection can not be NULL");
        }

        // check for NullPionterException
        for (Object t : c) {
            if (t == null) {
                throw new NullPointerException("Input collection has one or more NULL elements");
            } else if (!clazz.isInstance(t)) {
                throw new ClassCastException("Input collection has mismatching element types, expected: "
                        + clazz.getName() + " found: " + t.getClass().getName());
            }
        }
    }

    @Override
    public boolean removeAll(Collection<?> c) {
        this.checkArgumentsInCollection(c);
        return super.removeAll(c);
    }
/*
    public boolean removeAll(Collection<? extends T> c){
        this.checkArgumentsInCollection(c);
        return false;
    }
  */
    @Override
    public boolean retainAll(Collection<?> c) {
        this.checkArgumentsInCollection(c);
        return super.retainAll(c);
    }


    /**
     * This method is overridden to print out the list in concatenated string format
     *
     * @return String  list string
     */
    @Override
    public String toString() {
        StringBuilder buffer = new StringBuilder();

        buffer.append("[");
        for (T element : this) {
            buffer.append("[");
            buffer.append(element.toString());
            buffer.append("], ");
        }
        if (this.size() > 0) {
            buffer.replace(buffer.length() - 2, buffer.length(), "");
        }

        buffer.append("]");
        return buffer.toString();
    }

    @Override
    public int hashCode() {
        int hashCode = 1;
        Iterator<T> i = this.iterator();
        while (i.hasNext()) {
            T obj = i.next();
            hashCode = 31 * hashCode + (obj == null ? 0 : obj.hashCode());
        }
        return hashCode;
    }

    @Override
    public boolean equals(Object comparedToListObject) {
        if((comparedToListObject instanceof FacadeList) == false){
            return false;
        }
        FacadeList<T> comparedToList = (FacadeList) comparedToListObject;
        if(comparedToList.size() != this.size()){
            return false;
        }
        Iterator<T> thisIt = this.iterator();
        Iterator<T> comparedToListIt = comparedToList.iterator();
        while(thisIt.hasNext()){
            T thisT = thisIt.next();
            T comparedToT = comparedToListIt.next();
            if(!thisT.equals(comparedToT)){
                return false;
            }
        }

        return true;
    }

    /**
     * Check the legality of the argument
     * if illegal, then throw an IllegalArgument exception
     *
     * @param o Object to check
     */
    private void checkArgument(Object o) {
        if (o == null) {
            throw new NullPointerException("Argument cannot be a null value");
        }
        if (!clazz.isInstance(o)) {
            throw new ClassCastException("Argument must be an instance of " + clazz.getName() + ". Received instance of " + o.getClass().getName());
        }
    }

    /**
     * check whether the input index is greater or equal than zero, and less than the size of the original list
     *
     * @param index input index
     */
    private T getAtIndex(int index) {
        this.checkIndex(index);
        int cnt = 0;
        Iterator it = originalList.iterator();
        while (it.hasNext()) {
            Object o = it.next();
            if (clazz.isInstance(o)) {
                if (index == cnt) {
                    return (T) o;
                }
                cnt++;
            }
        }
        throw new IndexOutOfBoundsException("Input index for sublist should be greater than or equal than zero, and less than the size of the list: " + index);
    }

    /**
     * TODO: Move this into set method if not reused in future.
     *
     * @param index
     * @param newElement
     * @return
     */
    private T setAtIndex(int index, T newElement) {
        int cnt = 0;
        int originalListIndex = this.getOriginalIndex(index);

        // check index and throw an exception
        //this.checkIndex(index, "Input sublist index doesn't exist " + index);

        return (T) this.originalList.set(originalListIndex, newElement);
    }

    /**
     * TODO: Move this into add at index method if not reused in future.
     *
     * @param index
     * @param newElement
     * @return
     */
    private void addAtIndex(int index, T newElement) {

        int originalListIndex = this.getOriginalIndex(index);

        //this.checkIndex(originalListIndex, "Input sublist index doesn't exist" + index);

        this.originalList.add(originalListIndex, newElement);
    }

    /**
     * Get the index from the original list
     *
     * @param index index for the sublist
     * @return int index for the original list
     */
    private int getOriginalIndex(int index) {
        int cnt = 0;
        int originalListIndex = 0;

        for (Object element : originalList) {
            if (clazz.isInstance(element)) {
                if (index == cnt) {
                    break;
                }
                cnt++;
            }
            originalListIndex++;
        }

        return originalListIndex;
    }


    private void checkIndex(int index) {
        this.checkIndex(index, null);
    }

    private void checkIndex(int index, String errMsg) {
        if (index < 0  || index >= this.size()) {
            throw new IndexOutOfBoundsException(errMsg == null ? ("Input sublist index does not exist: " + index) : errMsg);
        }
    }

    private class SublistIterator implements Iterator<T> {
        private List superList;
        /**
         * Next position in the super list
         */
        //private int nextPosition;

        /**
         * Current position keeps track of the superlist index of the last instance returned by next()
         * or -1 if next() has not been called
         * The value of this index will be affected by next() and previous()
         */
        private int currPosition = -1;
        private boolean nextHasBeenCalled = false;

        public SublistIterator(List superList) {
            this.superList = superList;
        }


        public boolean hasNext() {
           // check whether this is a next element in the super collection
            if ((currPosition + 1) <= (superList.size() - 1)) {
                // starting from the current position, loop through the super collection
                for (int i = (currPosition + 1); i < superList.size(); i++) {
                    if (clazz.isInstance(superList.get(i))) {
                        return true;
                    }
                }
            }
            return false;
        }

        public T next() {
            // check whether this is a next element in the super collection
            if ((currPosition + 1) <= (superList.size() - 1)) {
                nextHasBeenCalled = true;
                // starting from the current position, loop through the super collection

                for (int i = ++currPosition; i < superList.size(); i++) {
                    if (clazz.isInstance(superList.get(i))) {
                        return (T) superList.get(i);
                    }
                    currPosition++;
                }
            }

            throw new NoSuchElementException("Sublist does not contain any more elements.");
        }

        public void remove() {
            if (this.nextHasBeenCalled == false) {
                throw new IllegalStateException("Next method for sublist iterator must be called at least once before remove can be called.");
            }

            this.superList.remove(this.currPosition);
            this.currPosition--;
            this.nextHasBeenCalled = false;

        }
    }

    private class SubListListIterator implements ListIterator<T> {
        private List superList;
        /**
         * Current position keeps track of the superlist index of the last instance returned by next()
         * or -1 if next() has not been called
         * The value of this index will be affected by next() and previous()
         */
        private int currPosition = -1;
        /**
         * Start super position is the superlist index of the first element in the sublist
         * Once set, this should never change
         */
        private int startSuperPosition = -1;
        /**
         * start index is the starting index of the sublist
         * Once set, this should never change
         */
        private int startIndex = -1;
        private boolean nextOrPreviousHasBeenCalled = false;
        private boolean addOrRemoveCalled;

        private SubListListIterator(List superList) {
            this(superList, 0);
        }

        public SubListListIterator(List superList, int startIndex) {
            if (superList == null) {
                throw new NullPointerException("Input super list cannot be null");
            }

            if (startIndex < 0 || (superList.size() > 0 && startIndex >= superList.size()) || (superList.size() == 0 && startIndex > 0)) {
                throw new IndexOutOfBoundsException("Start index of the iterator cannot be less than zero or greater-equal than the size of the list");
            }

            this.superList = superList;
            this.startIndex = startIndex;

            initNextPosition();
        }

        /**
         * set the currPosition according the start index
         */
        private void initNextPosition() {
            if (this.startIndex > 0) {
                int matchCnt = 0;    // Keep track of how many instances of interest we have encountered
                int cnt = 0;
                for (Object o : superList) {
                    if (clazz.isInstance(o)) {
                        if (matchCnt == this.startIndex) {
                            startSuperPosition = cnt;
//                            currPosition++;
                            break;
                        }
                        matchCnt++;
                    }
                    cnt++;
                }

                if (startSuperPosition == -1) {
                    throw new IndexOutOfBoundsException("Index out of the bound of the sublist: " + startIndex);
                }
                currPosition = startSuperPosition - 1;
            } else {
                startSuperPosition = 0;
            }

        }

        public boolean hasNext() {
            // check whether this is a next element in the super collection
            if ((currPosition + 1) <= (superList.size() - 1)) {
                // starting from the current position, loop through the super collection
                for (int i = (currPosition + 1); i < superList.size(); i++) {
                    if (clazz.isInstance(superList.get(i))) {
                        return true;
                    }
                }
            }

            return false;
        }

        public T next() {
            this.addOrRemoveCalled = false;
            // check whether this is a next element in the super collection
            if ((currPosition + 1) <= (superList.size() - 1)) {
                nextOrPreviousHasBeenCalled = true;
                // starting from the current position, loop through the super collection

                for (int i = ++currPosition; i < superList.size(); i++) {
                    if (clazz.isInstance(superList.get(i))) {
                        return (T) superList.get(i);
                    }
                    currPosition++;
                }
            }

            throw new NoSuchElementException("Sublist does not contain any more elements.");
        }

        public boolean hasPrevious() {
            if (currPosition >= 0) {
                nextOrPreviousHasBeenCalled = true;
                for (int i = currPosition; i >= startSuperPosition; i--) {
                    if (clazz.isInstance(superList.get(i))) {
                        return true;
                    }
                }
            }

            return false;
        }

        public T previous() {
            if (currPosition >= 0) {
                for (int i = currPosition; i >= this.startSuperPosition; i--) {
                    currPosition--;

                    if (clazz.isInstance(superList.get(i))) {
                        return (T) superList.get(i);
                    }
                }
            }
            this.addOrRemoveCalled = false;
            throw new NoSuchElementException("Failed to find a previous element");
        }

        public int nextIndex() {
            int cnt = 0;
            int nextIndex = -1;
            // starting from the current position, loop through the super collection
            for (int i = startSuperPosition; i < superList.size(); i++) {
                if (clazz.isInstance(superList.get(i))) {
                    if (nextIndex == -1 && i >= (this.currPosition + 1)) {
                        nextIndex = cnt;
                    }
                    cnt++;
                }
            }
            return nextIndex == -1 ? cnt : nextIndex;
        }

        public int previousIndex() {
            int previousIndex = -1;

            // starting from the current position, loop backward through the super collection
            for (int i = currPosition; i >= startSuperPosition; i--) {
                if (clazz.isInstance(superList.get(i))) {
                    previousIndex++;
                }
            }

            return previousIndex;
        }

        public void remove() {
            if (this.nextOrPreviousHasBeenCalled == false) {
                throw new IllegalStateException("Next method for sublist iterator must be called at least once before remove can be called.");
            }
            this.nextOrPreviousHasBeenCalled = false;
            this.addOrRemoveCalled = true;
            if (currPosition >= startSuperPosition) {
                this.superList.remove(this.currPosition);
                this.currPosition--;
            } else {
                this.superList.remove(startSuperPosition);
            }
        }

        public void set(T t) {
            if (this.nextOrPreviousHasBeenCalled && !this.addOrRemoveCalled) {
                if (currPosition >= startSuperPosition) {
                    this.superList.set(this.currPosition, t);
                } else {
                    this.superList.set(startSuperPosition, t);
                }
            } else {
                throw new IllegalStateException();
            }
        }

        public void add(T t) {
            this.addOrRemoveCalled = true;
            this.superList.add(currPosition + 1, t);
        }
    }
}
