//
// $Id: spectrum_processing_test.cpp 2973 2011-09-09 23:43:40Z chambm $
//
//
// Original author: Matt Chambers <matt.chambers .@. vanderbilt.edu>
//
// Copyright 2008 Vanderbilt University - Nashville, TN 37232
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

#include "../common/unit.hpp"
#include <stdexcept>


using namespace pwiz::CLI::util;
using namespace pwiz::CLI::cv;
using namespace pwiz::CLI::msdata;
using namespace pwiz::CLI::analysis;
using namespace System;
using namespace System::Collections::Generic;


Nullable<bool> isIndexEven(Spectrum^ s)
{
    if (String::IsNullOrEmpty(s->id))
        return Nullable<bool>();
    return Nullable<bool>((s->index % 2) == 0);
}

void testFilter()
{
    SpectrumListSimple^ sl = gcnew SpectrumListSimple();

    for (int i=0; i < 10; ++i)
    {
        Spectrum^ s = gcnew Spectrum();
        s->id = i.ToString();
        s->index = i;
        sl->spectra->Add(s);
    }

    SpectrumList_FilterAcceptSpectrum^ fas = gcnew SpectrumList_FilterAcceptSpectrum(isIndexEven);
    SpectrumList_Filter^ slf = gcnew SpectrumList_Filter(sl, fas);

    for (int i=0; i < slf->size(); ++i)
        System::Console::WriteLine("{0}: {1} {2}", i, slf->spectrum(i)->index, slf->spectrum(i)->id);

    unit_assert(slf->size() == 5);
    for (int i=0; i < 5; ++i)
    {
        unit_assert(slf->spectrum(i)->index == i); // index is remapped
        unit_assert(Convert::ToInt32(slf->spectrum(i)->id) == i*2); // id is not remapped
    }

    SpectrumList_FilterPredicate^ fp = gcnew SpectrumList_FilterPredicate_IndexSet("4-6"); // {4 5 6}
    slf = gcnew SpectrumList_Filter(sl, fp);
    
    unit_assert(slf->size() == 3);
    for (int i=0; i < 2; ++i)
    {
        unit_assert(slf->spectrum(i)->index == i); // index is remapped
        unit_assert(Convert::ToInt32(slf->spectrum(i)->id) == i+4); // id is not remapped
    }
}


void testPeakFilter()
{
    SpectrumListSimple^ sl = gcnew SpectrumListSimple();

    List<double>^ mzList = gcnew List<double>(gcnew array<double> {1,2,3,4,5,6,7,8,9});
    List<double>^ intensityList = gcnew List<double>(gcnew array<double> {1,5,2,4,2,6,7,4,1});

    Spectrum^ s = gcnew Spectrum();
    s->set(CVID::MS_ms_level, 2);
    s->setMZIntensityArrays(mzList, intensityList);

    sl->spectra->Add(s);

    ThresholdFilter^ tf = gcnew ThresholdFilter(ThresholdFilter::ThresholdingBy_Type::ThresholdingBy_AbsoluteIntensity,
                                                3,
                                                ThresholdFilter::ThresholdingOrientation::Orientation_MostIntense);
    SpectrumList_PeakFilter^ slpf = gcnew SpectrumList_PeakFilter(sl, tf);
    Spectrum^ sf = slpf->spectrum(0, true);
    unit_assert(sf->defaultArrayLength == 5);

    BinaryData^ intensityListFiltered = sf->getIntensityArray()->data;
    unit_assert(intensityListFiltered->Count == 5);
    unit_assert(intensityListFiltered[0] == 5);
    unit_assert(intensityListFiltered[4] == 4);
}


bool indexGreaterThan(Spectrum^ lhs, Spectrum^ rhs)
{
    return lhs->index > rhs->index;
}

void testSorter()
{
    /*SpectrumListSimple^ sl = gcnew SpectrumListSimple();

    for (int i=0; i < 5; ++i)
    {
        Spectrum^ s = gcnew Spectrum();
        s->index = i;
        s->set(CVID::MS_ms_level, 2);
        sl->spectra->Add(s);
    }

    SpectrumList_Sorter_LessThan^ sllt = gcnew SpectrumList_Sorter_LessThan(indexGreaterThan);
    SpectrumList_Sorter^ sls = gcnew SpectrumList_Sorter(sl, sllt);

    for (int i=0; i < 5; ++i)
        unit_assert(sls->spectrum(i)->index == 4-i);*/
}

// TODO: test more filters

int main()
{
    try
    {
        testFilter();
        testPeakFilter();
        //FIXME: testSorter();
        return 0;
    }
    catch (std::exception& e)
    {
        System::Console::Error->WriteLine("Caught std::exception not converted to System::Exception: " + gcnew System::String(e.what()));
        return 1;
    }
    catch (System::Exception^ e)
    {
        System::Console::Error->WriteLine(e->Message);
        return 1;
    }
    catch (...)
    {
        System::Console::Error->WriteLine("Caught unknown exception.");
        return 1;
    }
}
