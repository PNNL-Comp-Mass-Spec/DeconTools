using System;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.ElementalFormulas
{
    public struct ElementIsotopeData
    {
        public double Mass { get; private set; }
        public double Probability { get; private set; }

        public ElementIsotopeData(double mass, double probability)
            : this()
        {
            Mass = mass;
            Probability = probability;
        }
    }

    public class ElementData
    {
        public const int MaxTagLength = 256;

        public const int NumElements = 103;
        public const double ElectronMass = 0.00054858;
        public int Atomicity;
        public double AverageMass;

        /// <summary>
        ///     Tin has 10 isotopes!!! Don't allocate more, since no other element has more.
        /// </summary>
        public ElementIsotopeData[] Isotopes = new ElementIsotopeData[10];

        public double MassVariance;
        public string Name = "";

        public int NumberOfIsotopes;

        // Elemental symbol
        public string Symbol = "";

        public ElementData()
        {
        }

        public ElementData(ElementData a)
        {
            Symbol = a.Symbol;
            Name = a.Name;
            NumberOfIsotopes = a.NumberOfIsotopes;
            Atomicity = a.Atomicity;
            AverageMass = a.AverageMass;
            MassVariance = a.MassVariance;

            Isotopes = new ElementIsotopeData[NumberOfIsotopes];
            Array.Copy(a.Isotopes, Isotopes, NumberOfIsotopes);
        }
    }
}