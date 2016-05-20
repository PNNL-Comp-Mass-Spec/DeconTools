namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.ElementalFormulas
{
    public class AtomicCount
    {
        public AtomicCount(int index, double numCopies)
        {
            Index = index;
            NumCopies = numCopies;
        }

        public AtomicCount(AtomicCount ac)
        {
            Index = ac.Index;
            NumCopies = ac.NumCopies;
        }

        /// <summary>
        ///     index of this element in list of elements used.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        ///     Number of copies of the above element in compound. we have set this to be a double to allow for normalized values.
        /// </summary>
        public double NumCopies { get; set; }
    }
}