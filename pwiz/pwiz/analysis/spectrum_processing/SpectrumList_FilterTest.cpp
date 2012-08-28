//
// $Id: SpectrumList_FilterTest.cpp 2628 2011-04-15 16:38:18Z chambm $
//
//
// Original author: Darren Kessner <darren@proteowizard.org>
//
// Copyright 2008 Spielberg Family Center for Applied Proteomics
//   Cedars-Sinai Medical Center, Los Angeles, California  90048
//
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
//


#include "SpectrumList_Filter.hpp"
#include "pwiz/utility/misc/unit.hpp"
#include "pwiz/utility/misc/IntegerSet.hpp"
#include "pwiz/utility/misc/Std.hpp"
#include <cstring>


using namespace pwiz;
using namespace pwiz::msdata;
using namespace pwiz::analysis;
using namespace pwiz::util;
using boost::logic::tribool;


ostream* os_ = 0;


void printSpectrumList(const SpectrumList& sl, ostream& os)
{
    os << "size: " << sl.size() << endl;

    for (size_t i=0, end=sl.size(); i<end; i++)
    {
        SpectrumPtr spectrum = sl.spectrum(i, false);
        os << spectrum->index << " " 
           << spectrum->id << " "
           << "ms" << spectrum->cvParam(MS_ms_level).value << " "
           << "scanEvent:" << spectrum->scanList.scans[0].cvParam(MS_preset_scan_configuration).value << " "
           << "scanTime:" << spectrum->scanList.scans[0].cvParam(MS_scan_start_time).timeInSeconds() << " "
           << endl;
    }
}


SpectrumListPtr createSpectrumList()
{
    SpectrumListSimplePtr sl(new SpectrumListSimple);

    for (size_t i=0; i<10; i++)
    {
        SpectrumPtr spectrum(new Spectrum);
        spectrum->index = i;
        spectrum->id = "scan=" + lexical_cast<string>(100+i);
        vector<MZIntensityPair> pairs(i);
        spectrum->setMZIntensityPairs(pairs, MS_number_of_counts);

        bool isMS1 = i%3==0;
        spectrum->set(MS_ms_level, isMS1 ? 1 : 2);
        spectrum->set(isMS1 ? MS_MS1_spectrum : MS_MSn_spectrum);

        // outfit the spectra with mass analyzer definitions to test the massAnalyzer filter
        spectrum->scanList.scans.push_back(Scan());

        spectrum->scanList.scans[0].instrumentConfigurationPtr = InstrumentConfigurationPtr(new InstrumentConfiguration());
        InstrumentConfigurationPtr p = spectrum->scanList.scans[0].instrumentConfigurationPtr;
        if (i%3 == 0)
        {
            p->componentList.push_back(Component(MS_orbitrap, 0/*order*/));
        }
        else
        {
            if (i%2)
                p->componentList.push_back(Component(MS_orbitrap, 0/*order*/));
            else
                p->componentList.push_back(Component(MS_radial_ejection_linear_ion_trap, 0/*order*/));
        }

        if (i%3 != 0)
            spectrum->precursors.push_back(Precursor(500, 3));

        // add precursors and activation types to the MS2 spectra
        if (i==1 || i ==5) // ETD
        {
            spectrum->precursors[0].activation.set(MS_electron_transfer_dissociation);
        }
        else if (i==2) // CID
        {
            spectrum->precursors[0].activation.set(MS_collision_induced_dissociation);
        }
        else if (i==4) // HCD
        {
            spectrum->precursors[0].activation.set(MS_high_energy_collision_induced_dissociation);
        }
        else if (i==8) // IRMPD
        {
            spectrum->precursors[0].activation.set(MS_IRMPD);
        }
        else if (i==7) // ETD + SA
        {
            spectrum->precursors[0].activation.set(MS_electron_transfer_dissociation);
            spectrum->precursors[0].activation.set(MS_collision_induced_dissociation);
        }

        spectrum->scanList.scans.push_back(Scan());
        spectrum->scanList.scans[0].set(MS_preset_scan_configuration, i%4);
        spectrum->scanList.scans[0].set(MS_scan_start_time, 420+i, UO_second);
        sl->spectra.push_back(spectrum);
    }

    if (os_)
    {
        *os_ << "original spectrum list:\n";
        printSpectrumList(*sl, *os_); 
        *os_ << endl;
    }

    return sl;
}


struct EvenPredicate : public SpectrumList_Filter::Predicate
{
    virtual tribool accept(const SpectrumIdentity& spectrumIdentity) const
    {
        return spectrumIdentity.index%2 == 0;
    }
};


void testEven(SpectrumListPtr sl)
{
    if (os_) *os_ << "testEven:\n";

    SpectrumList_Filter filter(sl, EvenPredicate());

    if (os_) 
    {
        printSpectrumList(filter, *os_);
        *os_ << endl;
    }

    unit_assert(filter.size() == 5);

    for (size_t i=0, end=filter.size(); i<end; i++)
    {
        const SpectrumIdentity& id = filter.spectrumIdentity(i); 
        unit_assert(id.index == i);
        unit_assert(id.id == "scan=" + lexical_cast<string>(100+i*2));

        SpectrumPtr spectrum = filter.spectrum(i);
        unit_assert(spectrum->index == i);
        unit_assert(spectrum->id == "scan=" + lexical_cast<string>(100+i*2));
    }
}


struct EvenMS2Predicate : public SpectrumList_Filter::Predicate
{
    virtual tribool accept(const SpectrumIdentity& spectrumIdentity) const
    {
        if (spectrumIdentity.index%2 != 0) return false;
        return boost::logic::indeterminate;
    }

    virtual tribool accept(const Spectrum& spectrum) const
    {
        CVParam param = spectrum.cvParamChild(MS_spectrum_type);
        if (param.cvid == CVID_Unknown) return boost::logic::indeterminate;
        if (!cvIsA(param.cvid, MS_mass_spectrum))
            return true; // MS level filter doesn't affect non-MS spectra

        param = spectrum.cvParam(MS_ms_level);
        if (param.cvid == CVID_Unknown) return boost::logic::indeterminate;

        return (param.valueAs<int>() == 2);
    }
};


void testEvenMS2(SpectrumListPtr sl)
{
    if (os_) *os_ << "testEvenMS2:\n";

    SpectrumList_Filter filter(sl, EvenMS2Predicate());
    
    if (os_) 
    {
        printSpectrumList(filter, *os_);
        *os_ << endl;
    }

    unit_assert(filter.size() == 3);
    unit_assert(filter.spectrumIdentity(0).id == "scan=102");
    unit_assert(filter.spectrumIdentity(1).id == "scan=104");
    unit_assert(filter.spectrumIdentity(2).id == "scan=108");
}


struct SelectedIndexPredicate : public SpectrumList_Filter::Predicate
{
    mutable bool pastMaxIndex;

    SelectedIndexPredicate() : pastMaxIndex(false) {}

    virtual tribool accept(const SpectrumIdentity& spectrumIdentity) const
    {
        if (spectrumIdentity.index>5) pastMaxIndex = true;

        return (spectrumIdentity.index==1 ||
                spectrumIdentity.index==3 ||
                spectrumIdentity.index==5);
    }

    virtual bool done() const
    {
        return pastMaxIndex;
    }
};


void testSelectedIndices(SpectrumListPtr sl)
{
    if (os_) *os_ << "testSelectedIndices:\n";

    SpectrumList_Filter filter(sl, SelectedIndexPredicate());
    
    if (os_) 
    {
        printSpectrumList(filter, *os_);
        *os_ << endl;
    }

    unit_assert(filter.size() == 3);
    unit_assert(filter.spectrumIdentity(0).id == "scan=101");
    unit_assert(filter.spectrumIdentity(1).id == "scan=103");
    unit_assert(filter.spectrumIdentity(2).id == "scan=105");
}


void testIndexSet(SpectrumListPtr sl)
{
    if (os_) *os_ << "testIndexSet:\n";

    IntegerSet indexSet;
    indexSet.insert(3,5);
    indexSet.insert(7);
    indexSet.insert(9);

    SpectrumList_Filter filter(sl, SpectrumList_FilterPredicate_IndexSet(indexSet));
    
    if (os_) 
    {
        printSpectrumList(filter, *os_);
        *os_ << endl;
    }

    unit_assert(filter.size() == 5);
    unit_assert(filter.spectrumIdentity(0).id == "scan=103");
    unit_assert(filter.spectrumIdentity(1).id == "scan=104");
    unit_assert(filter.spectrumIdentity(2).id == "scan=105");
    unit_assert(filter.spectrumIdentity(3).id == "scan=107");
    unit_assert(filter.spectrumIdentity(4).id == "scan=109");
}


void testScanNumberSet(SpectrumListPtr sl)
{
    if (os_) *os_ << "testScanNumberSet:\n";

    IntegerSet scanNumberSet;
    scanNumberSet.insert(102,104);
    scanNumberSet.insert(107);

    SpectrumList_Filter filter(sl, SpectrumList_FilterPredicate_ScanNumberSet(scanNumberSet));
    
    if (os_) 
    {
        printSpectrumList(filter, *os_);
        *os_ << endl;
    }

    unit_assert(filter.size() == 4);
    unit_assert(filter.spectrumIdentity(0).id == "scan=102");
    unit_assert(filter.spectrumIdentity(1).id == "scan=103");
    unit_assert(filter.spectrumIdentity(2).id == "scan=104");
    unit_assert(filter.spectrumIdentity(3).id == "scan=107");
}


void testScanEventSet(SpectrumListPtr sl)
{
    if (os_) *os_ << "testScanEventSet:\n";

    IntegerSet scanEventSet;
    scanEventSet.insert(0,0);
    scanEventSet.insert(2,3);

    SpectrumList_Filter filter(sl, SpectrumList_FilterPredicate_ScanEventSet(scanEventSet));
    
    if (os_) 
    {
        printSpectrumList(filter, *os_);
        *os_ << endl;
    }

    unit_assert(filter.size() == 7);
    unit_assert(filter.spectrumIdentity(0).id == "scan=100");
    unit_assert(filter.spectrumIdentity(1).id == "scan=102");
    unit_assert(filter.spectrumIdentity(2).id == "scan=103");
    unit_assert(filter.spectrumIdentity(3).id == "scan=104");
    unit_assert(filter.spectrumIdentity(4).id == "scan=106");
    unit_assert(filter.spectrumIdentity(5).id == "scan=107");
    unit_assert(filter.spectrumIdentity(6).id == "scan=108");
}


void testScanTimeRange(SpectrumListPtr sl)
{
    if (os_) *os_ << "testScanTimeRange:\n";

    const double low = 422.5;
    const double high = 427.5;

    SpectrumList_Filter filter(sl, SpectrumList_FilterPredicate_ScanTimeRange(low, high));

    if (os_) 
    {
        printSpectrumList(filter, *os_);
        *os_ << endl;
    }

    unit_assert(filter.size() == 5);
    unit_assert(filter.spectrumIdentity(0).id == "scan=103");
    unit_assert(filter.spectrumIdentity(1).id == "scan=104");
    unit_assert(filter.spectrumIdentity(2).id == "scan=105");
    unit_assert(filter.spectrumIdentity(3).id == "scan=106");
    unit_assert(filter.spectrumIdentity(4).id == "scan=107");
}


void testMSLevelSet(SpectrumListPtr sl)
{
    if (os_) *os_ << "testMSLevelSet:\n";

    IntegerSet msLevelSet;
    msLevelSet.insert(1);

    SpectrumList_Filter filter(sl, SpectrumList_FilterPredicate_MSLevelSet(msLevelSet));
    
    if (os_) 
    {
        printSpectrumList(filter, *os_);
        *os_ << endl;
    }

    unit_assert(filter.size() == 4);
    unit_assert(filter.spectrumIdentity(0).id == "scan=100");
    unit_assert(filter.spectrumIdentity(1).id == "scan=103");
    unit_assert(filter.spectrumIdentity(2).id == "scan=106");
    unit_assert(filter.spectrumIdentity(3).id == "scan=109");

    IntegerSet msLevelSet2;
    msLevelSet2.insert(2);

    SpectrumList_Filter filter2(sl, SpectrumList_FilterPredicate_MSLevelSet(msLevelSet2));
    
    if (os_) 
    {
        printSpectrumList(filter2, *os_);
        *os_ << endl;
    }

    unit_assert(filter2.size() == 6);
    unit_assert(filter2.spectrumIdentity(0).id == "scan=101");
    unit_assert(filter2.spectrumIdentity(1).id == "scan=102");
    unit_assert(filter2.spectrumIdentity(2).id == "scan=104");
    unit_assert(filter2.spectrumIdentity(3).id == "scan=105");
    unit_assert(filter2.spectrumIdentity(4).id == "scan=107");
    unit_assert(filter2.spectrumIdentity(5).id == "scan=108");
}

void testMS2Activation(SpectrumListPtr sl)
{
    if (os_) *os_ << "testMS2Activation:\n";

    SpectrumListPtr ms2filter(new SpectrumList_Filter(sl, SpectrumList_FilterPredicate_MSLevelSet(IntegerSet(2))));

    set<CVID> cvIDs;
    // CID
    cvIDs.insert(MS_electron_transfer_dissociation);
    cvIDs.insert(MS_high_energy_collision_induced_dissociation);
    cvIDs.insert(MS_IRMPD);
    SpectrumList_Filter filter(ms2filter,
                        SpectrumList_FilterPredicate_ActivationType(cvIDs, true));

    if (os_) 
    {
        printSpectrumList(filter, *os_);
        *os_ << endl;
    }

    unit_assert(filter.size() == 1);
    unit_assert(filter.spectrumIdentity(0).id == "scan=102");

    // ETD + SA
    cvIDs.clear();
    cvIDs.insert(MS_electron_transfer_dissociation);
    cvIDs.insert(MS_collision_induced_dissociation);
    SpectrumList_Filter filter1(ms2filter, 
                        SpectrumList_FilterPredicate_ActivationType(cvIDs, false));
    if (os_) 
    {
        printSpectrumList(filter1, *os_);
        *os_ << endl;
    }

    unit_assert(filter1.size() == 1);
    unit_assert(filter1.spectrumIdentity(0).id == "scan=107");

    // ETD
    cvIDs.clear();
    cvIDs.insert(MS_electron_transfer_dissociation);
    SpectrumList_Filter filter2(ms2filter, 
                        SpectrumList_FilterPredicate_ActivationType(cvIDs, false));
    if (os_) 
    {
        printSpectrumList(filter2, *os_);
        *os_ << endl;
    }

    unit_assert(filter2.size() == 3);
    unit_assert(filter2.spectrumIdentity(0).id == "scan=101");
    unit_assert(filter2.spectrumIdentity(1).id == "scan=105");
    unit_assert(filter2.spectrumIdentity(2).id == "scan=107");

    // HCD
    cvIDs.clear();
    cvIDs.insert(MS_high_energy_collision_induced_dissociation);
    SpectrumList_Filter filter3(ms2filter, 
                        SpectrumList_FilterPredicate_ActivationType(cvIDs, false));
    if (os_) 
    {
        printSpectrumList(filter3, *os_);
        *os_ << endl;
    }

    unit_assert(filter3.size() == 1);
    unit_assert(filter3.spectrumIdentity(0).id == "scan=104");

    // IRMPD
    cvIDs.clear();
    cvIDs.insert(MS_IRMPD);
    SpectrumList_Filter filter4(ms2filter, 
                        SpectrumList_FilterPredicate_ActivationType(cvIDs, false));
    if (os_) 
    {
        printSpectrumList(filter4, *os_);
        *os_ << endl;
    }

    unit_assert(filter4.size() == 1);
    unit_assert(filter4.spectrumIdentity(0).id == "scan=108");

}
 
void testMassAnalyzerFilter(SpectrumListPtr sl)
{
    if (os_) *os_ << "testMassAnalyzerFilter:\n";

    set<CVID> cvIDs;
    // msconvert mass analyzer filter FTMS option
    cvIDs.insert(MS_orbitrap);
    cvIDs.insert(MS_fourier_transform_ion_cyclotron_resonance_mass_spectrometer);
    SpectrumList_Filter filter(sl, 
                        SpectrumList_FilterPredicate_AnalyzerType(cvIDs));

    if (os_) 
    {
        printSpectrumList(filter, *os_);
        *os_ << endl;
    }

    unit_assert(filter.size() == 7);
    unit_assert(filter.spectrumIdentity(0).id == "scan=100");

    cvIDs.clear();
	// msconvert mass analyzer filter ITMS option
    cvIDs.insert(MS_ion_trap);

    SpectrumList_Filter filter1(sl, 
                        SpectrumList_FilterPredicate_AnalyzerType(cvIDs));

    if (os_) 
    {
        printSpectrumList(filter1, *os_);
        *os_ << endl;
    }

    unit_assert(filter1.size() == 3);
    unit_assert(filter1.spectrumIdentity(0).id == "scan=102");
}

void test()
{
    SpectrumListPtr sl = createSpectrumList();
    testEven(sl);
    testEvenMS2(sl);
    testSelectedIndices(sl);
    testIndexSet(sl);
    testScanNumberSet(sl);
    testScanEventSet(sl);
    testScanTimeRange(sl);
    testMSLevelSet(sl);
    testMS2Activation(sl);
    testMassAnalyzerFilter(sl);
}


int main(int argc, char* argv[])
{
    try
    {
        if (argc>1 && !strcmp(argv[1],"-v")) os_ = &cout;
        test();
        return 0;
    }
    catch (exception& e)
    {
        cerr << e.what() << endl;
        return 1;
    }
}


