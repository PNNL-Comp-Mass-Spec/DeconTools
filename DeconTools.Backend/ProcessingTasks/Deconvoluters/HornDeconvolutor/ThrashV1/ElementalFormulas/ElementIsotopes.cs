using System;
using System.Collections.Generic;

namespace DeconTools.Backend.ProcessingTasks.Deconvoluters.HornDeconvolutor.ThrashV1.ElementalFormulas
{
    public class ElementIsotopes : ICloneable
    {
        private readonly Dictionary<string, int> _elementIndexDict = new Dictionary<string, int>();
        public readonly List<ElementData> ElementalIsotopesList = new List<ElementData>();

        public ElementIsotopes()
        {
            SetDefaultIsotopeDistribution();
        }

        public virtual object Clone()
        {
            var elemIsotopes = new ElementIsotopes();
            elemIsotopes.ElementalIsotopesList.AddRange(ElementalIsotopesList);
            return elemIsotopes;
        }

        public int GetNumberOfElements()
        {
            return ElementalIsotopesList.Count;
        }

        public void GetElementalIsotope(int index, ref int atomicity, ref int numIsotopes, ref string elementName,
            ref string elementSymbol, ref float averageMass, ref float massVariance, ref float[] isotopeMass,
            ref float[] isotopeProb)
        {
            GetElementalIsotopeOut(index, out atomicity, out numIsotopes, out elementName, out elementSymbol,
                out averageMass, out massVariance, out isotopeMass, out isotopeProb);
        }

        public void GetElementalIsotopeOut(int index, out int atomicity, out int numIsotopes, out string elementName,
            out string elementSymbol, out float averageMass, out float massVariance, out float[] isotopeMass,
            out float[] isotopeProb)
        {
            var elementData = ElementalIsotopesList[index];
            atomicity = elementData.Atomicity;
            numIsotopes = elementData.NumberOfIsotopes;
            elementName = elementData.Name;
            elementSymbol = elementData.Symbol;
            averageMass = (float) elementData.AverageMass;
            massVariance = (float) elementData.MassVariance;

            isotopeMass = new float[numIsotopes];
            isotopeProb = new float[numIsotopes];
            for (var isotopeNum = 0; isotopeNum < numIsotopes; isotopeNum++)
            {
                var isotopeData = elementData.Isotopes[isotopeNum];
                isotopeMass[isotopeNum] = (float) isotopeData.Mass;
                isotopeProb[isotopeNum] = (float) isotopeData.Probability;
            }
        }

        public int GetElementIndex(string symbol)
        {
            //var numElements = ElementalIsotopesList.Count;
            //for (var elementNum = 0; elementNum < numElements; elementNum++)
            //{
            //    if (
            //        string.Compare(ElementalIsotopesList[elementNum].Symbol, symbol, StringComparison.InvariantCulture) ==
            //        0)
            //    {
            //        // found the element. Return the index.
            //        return elementNum;
            //    }
            //}
            if (_elementIndexDict.TryGetValue(symbol, out var index))
            {
                return index;
            }
            return -1;
        }

        private void SetDefaultIsotopeDistribution()
        {
            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "H",
                Name = "Hydrogen",
                AverageMass = 1.007976,
                MassVariance = 0.000152,
                Atomicity = 1,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(1.007825, 0.999850),
                    new ElementIsotopeData(2.014102, 0.000150)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "He",
                Name = "Helium",
                AverageMass = 4.002599,
                MassVariance = 0.000001,
                Atomicity = 2,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(3.016030, 0.000001),
                    new ElementIsotopeData(4.002600, 0.999999)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Li",
                Name = "Lithium",
                AverageMass = 6.940937,
                MassVariance = 0.069497,
                Atomicity = 3,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(6.015121, 0.075000),
                    new ElementIsotopeData(7.016003, 0.925000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Be",
                Name = "Berellium",
                AverageMass = 9.012182,
                MassVariance = 0.000000,
                Atomicity = 4,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(9.012182, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "B",
                Name = "Boron",
                AverageMass = 10.811028,
                MassVariance = 0.158243,
                Atomicity = 5,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(10.012937, 0.199000),
                    new ElementIsotopeData(11.009305, 0.801000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "C",
                Name = "Carbon",
                AverageMass = 12.011107,
                MassVariance = 0.011021,
                Atomicity = 6,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(12.000000, 0.988930),
                    new ElementIsotopeData(13.003355, 0.011070)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "N",
                Name = "Nitrogen",
                AverageMass = 14.006724,
                MassVariance = 0.003628,
                Atomicity = 7,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(14.003072, 0.996337),
                    new ElementIsotopeData(15.000109, 0.003663)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "O",
                Name = "Oxygen",
                AverageMass = 15.999370,
                MassVariance = 0.008536,
                Atomicity = 8,
                NumberOfIsotopes = 3,
                Isotopes = new[]
                {
                    new ElementIsotopeData(15.994914, 0.997590),
                    new ElementIsotopeData(16.999132, 0.000374),
                    new ElementIsotopeData(17.999162, 0.002036)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "F",
                Name = "Fluorine",
                AverageMass = 18.998403,
                MassVariance = 0.000000,
                Atomicity = 9,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(18.998403, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Ne",
                Name = "Neon",
                AverageMass = 20.180041,
                MassVariance = 0.337122,
                Atomicity = 10,
                NumberOfIsotopes = 3,
                Isotopes = new[]
                {
                    new ElementIsotopeData(19.992435, 0.904800),
                    new ElementIsotopeData(20.993843, 0.002700),
                    new ElementIsotopeData(21.991383, 0.092500)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Na",
                Name = "Sodium",
                AverageMass = 22.989767,
                MassVariance = 0.000000,
                Atomicity = 11,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(22.989767, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Mg",
                Name = "Magnesium",
                AverageMass = 24.305052,
                MassVariance = 0.437075,
                Atomicity = 12,
                NumberOfIsotopes = 3,
                Isotopes = new[]
                {
                    new ElementIsotopeData(23.985042, 0.789900),
                    new ElementIsotopeData(24.985837, 0.100000),
                    new ElementIsotopeData(25.982593, 0.110100)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Al",
                Name = "Aluminum",
                AverageMass = 26.981539,
                MassVariance = 0.000000,
                Atomicity = 13,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(26.981539, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Si",
                Name = "Silicon",
                AverageMass = 28.085509,
                MassVariance = 0.158478,
                Atomicity = 14,
                NumberOfIsotopes = 3,
                Isotopes = new[]
                {
                    new ElementIsotopeData(27.976927, 0.922300),
                    new ElementIsotopeData(28.976495, 0.046700),
                    new ElementIsotopeData(29.973770, 0.031000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "P",
                Name = "Phosphrous",
                AverageMass = 30.973762,
                MassVariance = 0.000000,
                Atomicity = 15,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(30.973762, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "S",
                Name = "Sulphur",
                AverageMass = 32.064387,
                MassVariance = 0.169853,
                Atomicity = 16,
                NumberOfIsotopes = 4,
                Isotopes = new[]
                {
                    new ElementIsotopeData(31.972070, 0.950200),
                    new ElementIsotopeData(32.971456, 0.007500),
                    new ElementIsotopeData(33.967866, 0.042100),
                    new ElementIsotopeData(35.967080, 0.000200)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Cl",
                Name = "Chlorine",
                AverageMass = 35.457551,
                MassVariance = 0.737129,
                Atomicity = 17,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(34.968853, 0.755290),
                    new ElementIsotopeData(36.965903, 0.244710)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Ar",
                Name = "Argon",
                AverageMass = 39.947662,
                MassVariance = 0.056083,
                Atomicity = 18,
                NumberOfIsotopes = 3,
                Isotopes = new[]
                {
                    new ElementIsotopeData(35.967545, 0.003370),
                    new ElementIsotopeData(37.962732, 0.000630),
                    new ElementIsotopeData(39.962384, 0.996000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "K",
                Name = "Potassium",
                AverageMass = 39.098301,
                MassVariance = 0.250703,
                Atomicity = 19,
                NumberOfIsotopes = 3,
                Isotopes = new[]
                {
                    new ElementIsotopeData(38.963707, 0.932581),
                    new ElementIsotopeData(39.963999, 0.000117),
                    new ElementIsotopeData(40.961825, 0.067302)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Ca",
                Name = "Calcium",
                AverageMass = 40.078023,
                MassVariance = 0.477961,
                Atomicity = 20,
                NumberOfIsotopes = 6,
                Isotopes = new[]
                {
                    new ElementIsotopeData(39.962591, 0.969410),
                    new ElementIsotopeData(41.958618, 0.006470),
                    new ElementIsotopeData(42.958766, 0.001350),
                    new ElementIsotopeData(43.955480, 0.020860),
                    new ElementIsotopeData(45.953689, 0.000040),
                    new ElementIsotopeData(47.952533, 0.001870)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Sc",
                Name = "Scandium",
                AverageMass = 44.955910,
                MassVariance = 0.000000,
                Atomicity = 21,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(44.955910, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Ti",
                Name = "Titanium",
                AverageMass = 47.878426,
                MassVariance = 0.656425,
                Atomicity = 22,
                NumberOfIsotopes = 5,
                Isotopes = new[]
                {
                    new ElementIsotopeData(45.952629, 0.080000),
                    new ElementIsotopeData(46.951764, 0.073000),
                    new ElementIsotopeData(47.947947, 0.738000),
                    new ElementIsotopeData(48.947871, 0.055000),
                    new ElementIsotopeData(49.944792, 0.054000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "V",
                Name = "Vanadium",
                AverageMass = 50.941470,
                MassVariance = 0.002478,
                Atomicity = 23,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(49.947161, 0.002500),
                    new ElementIsotopeData(50.943962, 0.997500)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Cr",
                Name = "Chromium",
                AverageMass = 51.996125,
                MassVariance = 0.359219,
                Atomicity = 24,
                NumberOfIsotopes = 4,
                Isotopes = new[]
                {
                    new ElementIsotopeData(49.946046, 0.043450),
                    new ElementIsotopeData(51.940509, 0.837900),
                    new ElementIsotopeData(52.940651, 0.095000),
                    new ElementIsotopeData(53.938882, 0.023650)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Mn",
                Name = "Manganese",
                AverageMass = 54.938047,
                MassVariance = 0.000000,
                Atomicity = 25,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(54.938047, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Fe",
                Name = "Iron",
                AverageMass = 55.843820,
                MassVariance = 0.258796,
                Atomicity = 26,
                NumberOfIsotopes = 4,
                Isotopes = new[]
                {
                    new ElementIsotopeData(53.939612, 0.059000),
                    new ElementIsotopeData(55.934939, 0.917200),
                    new ElementIsotopeData(56.935396, 0.021000),
                    new ElementIsotopeData(57.933277, 0.002800)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Co",
                Name = "Cobalt",
                AverageMass = 58.933198,
                MassVariance = 0.000000,
                Atomicity = 27,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(58.933198, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Ni",
                Name = "Nickle",
                AverageMass = 58.687889,
                MassVariance = 1.473521,
                Atomicity = 28,
                NumberOfIsotopes = 5,
                Isotopes = new[]
                {
                    new ElementIsotopeData(57.935346, 0.682700),
                    new ElementIsotopeData(59.930788, 0.261000),
                    new ElementIsotopeData(60.931058, 0.011300),
                    new ElementIsotopeData(61.928346, 0.035900),
                    new ElementIsotopeData(63.927968, 0.009100)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Cu",
                Name = "Copper",
                AverageMass = 63.552559,
                MassVariance = 0.842964,
                Atomicity = 29,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(62.939598, 0.691700),
                    new ElementIsotopeData(64.927793, 0.308300)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Zn",
                Name = "Zinc",
                AverageMass = 65.396363,
                MassVariance = 2.545569,
                Atomicity = 30,
                NumberOfIsotopes = 5,
                Isotopes = new[]
                {
                    new ElementIsotopeData(63.929145, 0.486000),
                    new ElementIsotopeData(65.926034, 0.279000),
                    new ElementIsotopeData(66.927129, 0.041000),
                    new ElementIsotopeData(67.924846, 0.188000),
                    new ElementIsotopeData(69.925325, 0.006000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Ga",
                Name = "Galium",
                AverageMass = 69.723069,
                MassVariance = 0.958287,
                Atomicity = 31,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(68.925580, 0.601080),
                    new ElementIsotopeData(70.924700, 0.398920)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Ge",
                Name = "Germanium",
                AverageMass = 72.632250,
                MassVariance = 3.098354,
                Atomicity = 32,
                NumberOfIsotopes = 5,
                Isotopes = new[]
                {
                    new ElementIsotopeData(69.924250, 0.205000),
                    new ElementIsotopeData(71.922079, 0.274000),
                    new ElementIsotopeData(72.923463, 0.078000),
                    new ElementIsotopeData(73.921177, 0.365000),
                    new ElementIsotopeData(75.921401, 0.078000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "As",
                Name = "Arsenic",
                AverageMass = 74.921594,
                MassVariance = 0.000000,
                Atomicity = 33,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(74.921594, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Se",
                Name = "Selenium",
                AverageMass = 78.977677,
                MassVariance = 2.876151,
                Atomicity = 34,
                NumberOfIsotopes = 6,
                Isotopes = new[]
                {
                    new ElementIsotopeData(73.922475, 0.009000),
                    new ElementIsotopeData(75.919212, 0.091000),
                    new ElementIsotopeData(76.919912, 0.076000),
                    new ElementIsotopeData(77.919000, 0.236000),
                    new ElementIsotopeData(79.916520, 0.499000),
                    new ElementIsotopeData(81.916698, 0.089000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Br",
                Name = "Bromine",
                AverageMass = 79.903527,
                MassVariance = 0.997764,
                Atomicity = 35,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(78.918336, 0.506900),
                    new ElementIsotopeData(80.916289, 0.493100)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Kr",
                Name = "Krypton",
                AverageMass = 83.800003,
                MassVariance = 1.741449,
                Atomicity = 36,
                NumberOfIsotopes = 6,
                Isotopes = new[]
                {
                    new ElementIsotopeData(77.914000, 0.003500),
                    new ElementIsotopeData(79.916380, 0.022500),
                    new ElementIsotopeData(81.913482, 0.116000),
                    new ElementIsotopeData(82.914135, 0.115000),
                    new ElementIsotopeData(83.911507, 0.570000),
                    new ElementIsotopeData(85.910616, 0.173000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Rb",
                Name = "Rubidium",
                AverageMass = 85.467668,
                MassVariance = 0.801303,
                Atomicity = 37,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(84.911794, 0.721700),
                    new ElementIsotopeData(86.909187, 0.278300)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Sr",
                Name = "Strontium",
                AverageMass = 87.616651,
                MassVariance = 0.468254,
                Atomicity = 38,
                NumberOfIsotopes = 4,
                Isotopes = new[]
                {
                    new ElementIsotopeData(83.913430, 0.005600),
                    new ElementIsotopeData(85.909267, 0.098600),
                    new ElementIsotopeData(86.908884, 0.070000),
                    new ElementIsotopeData(87.905619, 0.825800)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Y",
                Name = "Yttrium",
                AverageMass = 88.905849,
                MassVariance = 0.000000,
                Atomicity = 39,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(88.905849, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Zr",
                Name = "Zirconium",
                AverageMass = 91.223646,
                MassVariance = 2.851272,
                Atomicity = 40,
                NumberOfIsotopes = 5,
                Isotopes = new[]
                {
                    new ElementIsotopeData(89.904703, 0.514500),
                    new ElementIsotopeData(90.905644, 0.112200),
                    new ElementIsotopeData(91.905039, 0.171500),
                    new ElementIsotopeData(93.906314, 0.173800),
                    new ElementIsotopeData(95.908275, 0.028000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Nb",
                Name = "Niobium",
                AverageMass = 92.906377,
                MassVariance = 0.000000,
                Atomicity = 41,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(92.906377, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Mo",
                Name = "Molybdenum",
                AverageMass = 95.931290,
                MassVariance = 5.504460,
                Atomicity = 42,
                NumberOfIsotopes = 7,
                Isotopes = new[]
                {
                    new ElementIsotopeData(91.906808, 0.148400),
                    new ElementIsotopeData(93.905085, 0.092500),
                    new ElementIsotopeData(94.905840, 0.159200),
                    new ElementIsotopeData(95.904678, 0.166800),
                    new ElementIsotopeData(96.906020, 0.095500),
                    new ElementIsotopeData(97.905406, 0.241300),
                    new ElementIsotopeData(99.907477, 0.096300)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Tc",
                Name = "Technetium",
                AverageMass = 98.000000,
                MassVariance = 0.000000,
                Atomicity = 43,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(98.000000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Ru",
                Name = "Ruthenium",
                AverageMass = 101.066343,
                MassVariance = 4.148678,
                Atomicity = 44,
                NumberOfIsotopes = 7,
                Isotopes = new[]
                {
                    new ElementIsotopeData(95.907599, 0.055400),
                    new ElementIsotopeData(97.905287, 0.018600),
                    new ElementIsotopeData(98.905939, 0.127000),
                    new ElementIsotopeData(99.904219, 0.126000),
                    new ElementIsotopeData(100.905582, 0.171000),
                    new ElementIsotopeData(101.904348, 0.316000),
                    new ElementIsotopeData(103.905424, 0.186000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Rh",
                Name = "Rhodium",
                AverageMass = 102.905500,
                MassVariance = 0.000000,
                Atomicity = 45,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(102.905500, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Pd",
                Name = "Palladium",
                AverageMass = 106.415327,
                MassVariance = 3.504600,
                Atomicity = 46,
                NumberOfIsotopes = 6,
                Isotopes = new[]
                {
                    new ElementIsotopeData(101.905634, 0.010200),
                    new ElementIsotopeData(103.904029, 0.111400),
                    new ElementIsotopeData(104.905079, 0.223300),
                    new ElementIsotopeData(105.903478, 0.273300),
                    new ElementIsotopeData(107.903895, 0.264600),
                    new ElementIsotopeData(109.905167, 0.117200)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Ag",
                Name = "Silver",
                AverageMass = 107.868151,
                MassVariance = 0.998313,
                Atomicity = 47,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(106.905092, 0.518390),
                    new ElementIsotopeData(108.904757, 0.481610)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Cd",
                Name = "Cadmium",
                AverageMass = 112.411552,
                MassVariance = 3.432071,
                Atomicity = 48,
                NumberOfIsotopes = 8,
                Isotopes = new[]
                {
                    new ElementIsotopeData(105.906461, 0.012500),
                    new ElementIsotopeData(107.904176, 0.008900),
                    new ElementIsotopeData(109.903005, 0.124900),
                    new ElementIsotopeData(110.904182, 0.128000),
                    new ElementIsotopeData(111.902758, 0.241300),
                    new ElementIsotopeData(112.904400, 0.122200),
                    new ElementIsotopeData(113.903357, 0.287300),
                    new ElementIsotopeData(115.904754, 0.074900)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "In",
                Name = "Indium",
                AverageMass = 114.817888,
                MassVariance = 0.164574,
                Atomicity = 49,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(112.904061, 0.043000),
                    new ElementIsotopeData(114.903880, 0.957000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Sn",
                Name = "Tin",
                AverageMass = 118.710213,
                MassVariance = 4.707888,
                Atomicity = 50,
                NumberOfIsotopes = 10,
                Isotopes = new[]
                {
                    new ElementIsotopeData(111.904826, 0.009700),
                    new ElementIsotopeData(113.902784, 0.006500),
                    new ElementIsotopeData(114.903348, 0.003600),
                    new ElementIsotopeData(115.901747, 0.145300),
                    new ElementIsotopeData(116.902956, 0.076800),
                    new ElementIsotopeData(117.901609, 0.242200),
                    new ElementIsotopeData(118.903310, 0.085800),
                    new ElementIsotopeData(119.902200, 0.325900),
                    new ElementIsotopeData(121.903440, 0.046300),
                    new ElementIsotopeData(123.905274, 0.057900)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Sb",
                Name = "Antimony",
                AverageMass = 121.755989,
                MassVariance = 0.978482,
                Atomicity = 51,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(120.903821, 0.574000),
                    new ElementIsotopeData(122.904216, 0.426000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Te",
                Name = "Tellerium",
                AverageMass = 127.590074,
                MassVariance = 4.644177,
                Atomicity = 52,
                NumberOfIsotopes = 8,
                Isotopes = new[]
                {
                    new ElementIsotopeData(119.904048, 0.000950),
                    new ElementIsotopeData(121.903054, 0.025900),
                    new ElementIsotopeData(122.904271, 0.009050),
                    new ElementIsotopeData(123.902823, 0.047900),
                    new ElementIsotopeData(124.904433, 0.071200),
                    new ElementIsotopeData(125.903314, 0.189300),
                    new ElementIsotopeData(127.904463, 0.317000),
                    new ElementIsotopeData(129.906229, 0.338700)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "I",
                Name = "Iodine",
                AverageMass = 126.904473,
                MassVariance = 0.000000,
                Atomicity = 53,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(126.904473, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Xe",
                Name = "Xenon",
                AverageMass = 131.293076,
                MassVariance = 4.622071,
                Atomicity = 54,
                NumberOfIsotopes = 9,
                Isotopes = new[]
                {
                    new ElementIsotopeData(123.905894, 0.001000),
                    new ElementIsotopeData(125.904281, 0.000900),
                    new ElementIsotopeData(127.903531, 0.019100),
                    new ElementIsotopeData(128.904780, 0.264000),
                    new ElementIsotopeData(129.903509, 0.041000),
                    new ElementIsotopeData(130.905072, 0.212000),
                    new ElementIsotopeData(131.904144, 0.269000),
                    new ElementIsotopeData(133.905395, 0.104000),
                    new ElementIsotopeData(135.907214, 0.089000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Cs",
                Name = "Casium",
                AverageMass = 132.905429,
                MassVariance = 0.000000,
                Atomicity = 55,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(132.905429, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Ba",
                Name = "Barium",
                AverageMass = 137.326825,
                MassVariance = 1.176556,
                Atomicity = 56,
                NumberOfIsotopes = 7,
                Isotopes = new[]
                {
                    new ElementIsotopeData(129.906282, 0.001060),
                    new ElementIsotopeData(131.905042, 0.001010),
                    new ElementIsotopeData(133.904486, 0.024200),
                    new ElementIsotopeData(134.905665, 0.065930),
                    new ElementIsotopeData(135.904553, 0.078500),
                    new ElementIsotopeData(136.905812, 0.112300),
                    new ElementIsotopeData(137.905232, 0.717000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "La",
                Name = "Lanthanum",
                AverageMass = 138.905448,
                MassVariance = 0.000898,
                Atomicity = 57,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(137.907110, 0.000900),
                    new ElementIsotopeData(138.906347, 0.999100)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Ce",
                Name = "Cerium",
                AverageMass = 140.115861,
                MassVariance = 0.442985,
                Atomicity = 58,
                NumberOfIsotopes = 4,
                Isotopes = new[]
                {
                    new ElementIsotopeData(135.907140, 0.001900),
                    new ElementIsotopeData(137.905985, 0.002500),
                    new ElementIsotopeData(139.905433, 0.884300),
                    new ElementIsotopeData(141.909241, 0.111300)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Pr",
                Name = "Praseodymium",
                AverageMass = 140.907647,
                MassVariance = 0.000000,
                Atomicity = 59,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(140.907647, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Nd",
                Name = "Neodynium",
                AverageMass = 144.242337,
                MassVariance = 4.834797,
                Atomicity = 60,
                NumberOfIsotopes = 7,
                Isotopes = new[]
                {
                    new ElementIsotopeData(141.907719, 0.271300),
                    new ElementIsotopeData(142.909810, 0.121800),
                    new ElementIsotopeData(143.910083, 0.238000),
                    new ElementIsotopeData(144.912570, 0.083000),
                    new ElementIsotopeData(145.913113, 0.171900),
                    new ElementIsotopeData(147.916889, 0.057600),
                    new ElementIsotopeData(149.920887, 0.056400)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Pm",
                Name = "Promethium",
                AverageMass = 145.000000,
                MassVariance = 0.000000,
                Atomicity = 61,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(145.000000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Sm",
                Name = "Samarium",
                AverageMass = 150.360238,
                MassVariance = 7.576609,
                Atomicity = 62,
                NumberOfIsotopes = 7,
                Isotopes = new[]
                {
                    new ElementIsotopeData(143.911998, 0.031000),
                    new ElementIsotopeData(146.914895, 0.150000),
                    new ElementIsotopeData(147.914820, 0.113000),
                    new ElementIsotopeData(148.917181, 0.138000),
                    new ElementIsotopeData(149.917273, 0.074000),
                    new ElementIsotopeData(151.919729, 0.267000),
                    new ElementIsotopeData(153.922206, 0.227000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Eu",
                Name = "Europium",
                AverageMass = 151.964566,
                MassVariance = 0.999440,
                Atomicity = 63,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(150.919847, 0.478000),
                    new ElementIsotopeData(152.921225, 0.522000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Gd",
                Name = "Gadolinium",
                AverageMass = 157.252118,
                MassVariance = 3.157174,
                Atomicity = 64,
                NumberOfIsotopes = 7,
                Isotopes = new[]
                {
                    new ElementIsotopeData(151.919786, 0.002000),
                    new ElementIsotopeData(153.920861, 0.021800),
                    new ElementIsotopeData(154.922618, 0.148000),
                    new ElementIsotopeData(155.922118, 0.204700),
                    new ElementIsotopeData(156.923956, 0.156500),
                    new ElementIsotopeData(157.924099, 0.248400),
                    new ElementIsotopeData(159.927049, 0.218600)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Tb",
                Name = "Terbium",
                AverageMass = 158.925342,
                MassVariance = 0.000000,
                Atomicity = 65,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(158.925342, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Dy",
                Name = "Dysprosium",
                AverageMass = 162.497531,
                MassVariance = 1.375235,
                Atomicity = 66,
                NumberOfIsotopes = 7,
                Isotopes = new[]
                {
                    new ElementIsotopeData(155.925277, 0.000600),
                    new ElementIsotopeData(157.924403, 0.001000),
                    new ElementIsotopeData(159.925193, 0.023400),
                    new ElementIsotopeData(160.926930, 0.189000),
                    new ElementIsotopeData(161.926795, 0.255000),
                    new ElementIsotopeData(162.928728, 0.249000),
                    new ElementIsotopeData(163.929171, 0.282000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Ho",
                Name = "Holmium",
                AverageMass = 164.930319,
                MassVariance = 0.000000,
                Atomicity = 67,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(164.930319, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Er",
                Name = "Erbium",
                AverageMass = 167.255701,
                MassVariance = 2.024877,
                Atomicity = 68,
                NumberOfIsotopes = 6,
                Isotopes = new[]
                {
                    new ElementIsotopeData(161.928775, 0.001400),
                    new ElementIsotopeData(163.929198, 0.016100),
                    new ElementIsotopeData(165.930290, 0.336000),
                    new ElementIsotopeData(166.932046, 0.229500),
                    new ElementIsotopeData(167.932368, 0.268000),
                    new ElementIsotopeData(169.935461, 0.149000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Tm",
                Name = "Thulium",
                AverageMass = 168.934212,
                MassVariance = 0.000000,
                Atomicity = 69,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(168.934212, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Yb",
                Name = "Ytterbium",
                AverageMass = 173.034187,
                MassVariance = 2.556093,
                Atomicity = 70,
                NumberOfIsotopes = 7,
                Isotopes = new[]
                {
                    new ElementIsotopeData(167.933894, 0.001300),
                    new ElementIsotopeData(169.934759, 0.030500),
                    new ElementIsotopeData(170.936323, 0.143000),
                    new ElementIsotopeData(171.936378, 0.219000),
                    new ElementIsotopeData(172.938208, 0.161200),
                    new ElementIsotopeData(173.938859, 0.318000),
                    new ElementIsotopeData(175.942564, 0.127000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Lu",
                Name = "Lutetium",
                AverageMass = 174.966719,
                MassVariance = 0.025326,
                Atomicity = 71,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(174.940770, 0.974100),
                    new ElementIsotopeData(175.942679, 0.025900)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Hf",
                Name = "Hafnium",
                AverageMass = 178.486400,
                MassVariance = 1.671265,
                Atomicity = 72,
                NumberOfIsotopes = 6,
                Isotopes = new[]
                {
                    new ElementIsotopeData(173.940044, 0.001620),
                    new ElementIsotopeData(175.941406, 0.052060),
                    new ElementIsotopeData(176.943217, 0.186060),
                    new ElementIsotopeData(177.943696, 0.272970),
                    new ElementIsotopeData(178.945812, 0.136290),
                    new ElementIsotopeData(179.946545, 0.351000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Ta",
                Name = "Tantalum",
                AverageMass = 180.947872,
                MassVariance = 0.000120,
                Atomicity = 73,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(179.947462, 0.000120),
                    new ElementIsotopeData(180.947992, 0.999880)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "W",
                Name = "Tungsten",
                AverageMass = 183.849486,
                MassVariance = 2.354748,
                Atomicity = 74,
                NumberOfIsotopes = 5,
                Isotopes = new[]
                {
                    new ElementIsotopeData(179.946701, 0.001200),
                    new ElementIsotopeData(181.948202, 0.263000),
                    new ElementIsotopeData(182.950220, 0.142800),
                    new ElementIsotopeData(183.950928, 0.307000),
                    new ElementIsotopeData(185.954357, 0.286000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Re",
                Name = "Rhenium",
                AverageMass = 186.206699,
                MassVariance = 0.939113,
                Atomicity = 75,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(184.952951, 0.374000),
                    new ElementIsotopeData(186.955744, 0.626000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Os",
                Name = "Osmium",
                AverageMass = 190.239771,
                MassVariance = 2.665149,
                Atomicity = 76,
                NumberOfIsotopes = 7,
                Isotopes = new[]
                {
                    new ElementIsotopeData(183.952488, 0.000200),
                    new ElementIsotopeData(185.953830, 0.015800),
                    new ElementIsotopeData(186.955741, 0.016000),
                    new ElementIsotopeData(187.955860, 0.133000),
                    new ElementIsotopeData(188.958137, 0.161000),
                    new ElementIsotopeData(189.958436, 0.264000),
                    new ElementIsotopeData(191.961467, 0.410000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Ir",
                Name = "Iridium",
                AverageMass = 192.216047,
                MassVariance = 0.937668,
                Atomicity = 77,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(190.960584, 0.373000),
                    new ElementIsotopeData(192.962917, 0.627000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Pt",
                Name = "Platinum",
                AverageMass = 195.080105,
                MassVariance = 1.293292,
                Atomicity = 78,
                NumberOfIsotopes = 6,
                Isotopes = new[]
                {
                    new ElementIsotopeData(189.959917, 0.000100),
                    new ElementIsotopeData(191.961019, 0.007900),
                    new ElementIsotopeData(193.962655, 0.329000),
                    new ElementIsotopeData(194.964766, 0.338000),
                    new ElementIsotopeData(195.964926, 0.253000),
                    new ElementIsotopeData(197.967869, 0.072000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Au",
                Name = "Gold",
                AverageMass = 196.966543,
                MassVariance = 0.000000,
                Atomicity = 79,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(196.966543, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Hg",
                Name = "Mercury",
                AverageMass = 200.596438,
                MassVariance = 2.625230,
                Atomicity = 80,
                NumberOfIsotopes = 7,
                Isotopes = new[]
                {
                    new ElementIsotopeData(195.965807, 0.001500),
                    new ElementIsotopeData(197.966743, 0.100000),
                    new ElementIsotopeData(198.968254, 0.169000),
                    new ElementIsotopeData(199.968300, 0.231000),
                    new ElementIsotopeData(200.970277, 0.132000),
                    new ElementIsotopeData(201.970617, 0.298000),
                    new ElementIsotopeData(203.973467, 0.068500)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Tl",
                Name = "Thallium",
                AverageMass = 204.383307,
                MassVariance = 0.834026,
                Atomicity = 81,
                NumberOfIsotopes = 2,
                Isotopes = new[]
                {
                    new ElementIsotopeData(202.972320, 0.295240),
                    new ElementIsotopeData(204.974401, 0.704760)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Pb",
                Name = "Lead",
                AverageMass = 207.216883,
                MassVariance = 0.834636,
                Atomicity = 82,
                NumberOfIsotopes = 4,
                Isotopes = new[]
                {
                    new ElementIsotopeData(203.973020, 0.014000),
                    new ElementIsotopeData(205.974440, 0.241000),
                    new ElementIsotopeData(206.975872, 0.221000),
                    new ElementIsotopeData(207.976627, 0.524000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Bi",
                Name = "Bismuth",
                AverageMass = 208.980374,
                MassVariance = 0.000000,
                Atomicity = 83,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(208.980374, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Po",
                Name = "Polonium",
                AverageMass = 209.000000,
                MassVariance = 0.000000,
                Atomicity = 84,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(209.000000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "At",
                Name = "Astatine",
                AverageMass = 210.000000,
                MassVariance = 0.000000,
                Atomicity = 85,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(210.000000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Rn",
                Name = "Radon",
                AverageMass = 222.000000,
                MassVariance = 0.000000,
                Atomicity = 86,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(222.000000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Fr",
                Name = "Francium",
                AverageMass = 223.000000,
                MassVariance = 0.000000,
                Atomicity = 87,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(223.000000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Ra",
                Name = "Radium",
                AverageMass = 226.025000,
                MassVariance = 0.000000,
                Atomicity = 88,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(226.025000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Ac",
                Name = "Actinium",
                AverageMass = 227.028000,
                MassVariance = 0.000000,
                Atomicity = 89,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(227.028000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Th",
                Name = "Thorium",
                AverageMass = 232.038054,
                MassVariance = 0.000000,
                Atomicity = 90,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(232.038054, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Pa",
                Name = "Protactinium",
                AverageMass = 231.035900,
                MassVariance = 0.000000,
                Atomicity = 91,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(231.035900, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "U",
                Name = "Uranium",
                AverageMass = 238.028914,
                MassVariance = 0.065503,
                Atomicity = 92,
                NumberOfIsotopes = 3,
                Isotopes = new[]
                {
                    new ElementIsotopeData(234.040946, 0.000055),
                    new ElementIsotopeData(235.043924, 0.007200),
                    new ElementIsotopeData(238.050784, 0.992745)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Np",
                Name = "Neptunium",
                AverageMass = 237.048000,
                MassVariance = 0.000000,
                Atomicity = 93,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(237.048000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Pu",
                Name = "Plutonium",
                AverageMass = 244.000000,
                MassVariance = 0.000000,
                Atomicity = 94,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(244.000000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Am",
                Name = "Americium",
                AverageMass = 243.000000,
                MassVariance = 0.000000,
                Atomicity = 95,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(243.000000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Cm",
                Name = "Curium",
                AverageMass = 247.000000,
                MassVariance = 0.000000,
                Atomicity = 96,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(247.000000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Bk",
                Name = "Berkelium",
                AverageMass = 247.000000,
                MassVariance = 0.000000,
                Atomicity = 97,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(247.000000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Cf",
                Name = "Californium",
                AverageMass = 251.000000,
                MassVariance = 0.000000,
                Atomicity = 98,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(251.000000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Es",
                Name = "Einsteinium",
                AverageMass = 252.000000,
                MassVariance = 0.000000,
                Atomicity = 99,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(252.000000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Fm",
                Name = "Fernium",
                AverageMass = 257.000000,
                MassVariance = 0.000000,
                Atomicity = 100,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(257.000000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Md",
                Name = "Medelevium",
                AverageMass = 258.000000,
                MassVariance = 0.000000,
                Atomicity = 101,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(258.000000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "No",
                Name = "Nobelium",
                AverageMass = 259.000000,
                MassVariance = 0.000000,
                Atomicity = 102,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(259.000000, 1.000000)
                }
            });

            ElementalIsotopesList.Add(new ElementData
            {
                Symbol = "Lr",
                Name = "Lawrencium",
                AverageMass = 260.000000,
                MassVariance = 0.000000,
                Atomicity = 103,
                NumberOfIsotopes = 1,
                Isotopes = new[]
                {
                    new ElementIsotopeData(260.000000, 1.000000)
                }
            });

            for (var i = 0; i < ElementalIsotopesList.Count; i++)
            {
                _elementIndexDict.Add(ElementalIsotopesList[i].Symbol, i);
            }
        }
    }
}