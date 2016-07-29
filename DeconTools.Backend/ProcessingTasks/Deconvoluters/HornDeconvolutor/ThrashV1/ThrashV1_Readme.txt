This group of code, including additions to the file HornDeconvolutor.cs, was imported/converted from DeconEngineV2,
which was originally written in VB6, then converted to C++ with /clr:oldsyntax, then ported to C# and copied to this project.

The original code is available at https://github.com/PNNL-Comp-Mass-Spec/DeconEngineV2
The code ported to C# is at https://github.com/PNNL-Comp-Mass-Spec/DeconEngineV2/tree/master/C%23_Version/DeconEngine

File mapping of code ported from C#_Version\DeconEngine to DeconTools.Backened:

/-----------------------  Project DeconTools.Backend -----------------------\    /- C#_Version\DeconEngine -\
Folder      Category                   File                                      File
------      --------                   ----                                      ----
ThrashV1    HornTransformResults.cs    HornTransformResults.cs                   clsHornTransformResults.cs
ThrashV1    ChargeDetermination        AutoCorrelationChargeDetermination.cs     AutoCorrelationChargeDetermination.cs
ThrashV1    ElementalFormulas          AtomicCount.cs                            MolecularFormula.cs
ThrashV1    ElementalFormulas          Averagine.cs                              Averagine.cs
ThrashV1    ElementalFormulas          ElementData.cs                            ElementData.cs
ThrashV1    ElementalFormulas          ElementIsotopes.cs                        MolecularFormula.cs
ThrashV1    ElementalFormulas          MolecularFormula.cs                       MolecularFormula.cs
ThrashV1    FitScoring                 AreaFitScorer.cs                          AreaFitScorer.cs
ThrashV1    FitScoring                 ChiSqFitScorer.cs                         ChiSqFitScorer.cs
ThrashV1    FitScoring                 IsotopicProfileFitScorer.cs               IsotopicProfileFitScorer.cs
ThrashV1    FitScoring                 PeakFitScorer.cs                          PeakFitScorer.cs
ThrashV1    Mercury                    MercuryCache.cs                           MercuryCache.cs
ThrashV1    Mercury                    MercuryIsotopeDistribution.cs             MercuryIsotopeDistribution.cs
ThrashV1    PeakProcessing             PeakData.cs                               PeakData.cs
ThrashV1    PeakProcessing             PeakFitter.cs                             PeakFitter.cs
ThrashV1    PeakProcessing             PeakIndex.cs                              PeakIndex.cs
ThrashV1    PeakProcessing             PeakProcessor.cs                          PeakProcessor.cs
ThrashV1    PeakProcessing             PeakStatistician.cs                       PeakStatistician.cs
ThrashV1    PeakProcessing             ThrashV1Peak.cs                           clsPeak.cs
