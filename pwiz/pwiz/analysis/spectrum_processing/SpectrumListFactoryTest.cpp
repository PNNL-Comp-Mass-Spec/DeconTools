//
// $Id: SpectrumListFactoryTest.cpp 3180 2011-12-19 23:17:13Z chambm $
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


#include "SpectrumListFactory.hpp"
#include "pwiz/utility/misc/unit.hpp"
#include "pwiz/utility/misc/Std.hpp"
#include <cstring>


using namespace pwiz::analysis;
using namespace pwiz::util;
using namespace pwiz::cv;
using namespace pwiz::msdata;


ostream* os_ = 0;


void testUsage()
{
    if (os_) *os_ << "SpectrumListFactory::usage():\n" <<  SpectrumListFactory::usage() << endl;
}


void testWrap()
{
    MSData msd;
    examples::initializeTiny(msd);

    SpectrumListPtr& sl = msd.run.spectrumListPtr;

    unit_assert(sl.get());
    unit_assert(sl->size() > 2);

    // CompassXtract and pwiz data processing
    unit_assert_operator_equal(2, msd.allDataProcessingPtrs().size());
    unit_assert_operator_equal(1, msd.allDataProcessingPtrs()[1]->processingMethods.size());

    SpectrumListFactory::wrap(msd, "scanNumber [19,20]");
    unit_assert(sl->size() == 2);

    SpectrumListFactory::wrap(msd, "index [1,1]");
    unit_assert(sl->size() == 1);
    unit_assert(sl->spectrumIdentity(0).id == "scan=20");

    vector<double> profileData(sl->spectrum(0)->getMZArray()->data);
    unit_assert(profileData.size() == 10);
    unit_assert(profileData[0] == 0);
    unit_assert(profileData[9] == 18);

    unit_assert_operator_equal(2, msd.allDataProcessingPtrs().size());
    unit_assert_operator_equal(1, msd.allDataProcessingPtrs()[1]->processingMethods.size());

    SpectrumListFactory::wrap(msd, "peakPicking true [1,6]");

    vector<double> peakData(sl->spectrum(0)->getMZArray()->data);
    unit_assert(peakData.size() == 1);
    unit_assert(peakData[0] == 0);

    unit_assert_operator_equal(2, msd.allDataProcessingPtrs().size());
    unit_assert_operator_equal(2, msd.allDataProcessingPtrs()[1]->processingMethods.size());
}


void testWrapScanTimeRange()
{
    MSData msd;
    examples::initializeTiny(msd);

    SpectrumListPtr& sl = msd.run.spectrumListPtr;
    unit_assert(sl.get());
    unit_assert(sl->size() > 2);

    double timeHighInSeconds = 5.9 * 60; // between first and second scan
    ostringstream oss;
    oss << "scanTime [0," << timeHighInSeconds << "]";
    SpectrumListFactory::wrap(msd, oss.str());
    unit_assert(sl->size() == 2);
    unit_assert(sl->spectrumIdentity(0).id == "scan=19");
    unit_assert(sl->spectrumIdentity(1).id == "sample=1 period=1 cycle=23 experiment=1"); // not in scan time order (42 seconds)
}


void testWrapSortScanTime()
{
    MSData msd;
    examples::initializeTiny(msd);

    SpectrumListPtr& sl = msd.run.spectrumListPtr;
    unit_assert(sl.get());
    unit_assert(sl->size() == 5);

    sl->spectrum(0)->scanList.scans[0].set(MS_scan_start_time, 35, UO_second);
    sl->spectrum(2)->scanList.scans[0].set(MS_scan_start_time, 0.5, UO_minute);

    SpectrumListFactory::wrap(msd, "sortByScanTime");
    unit_assert(sl->size() == 5);
    unit_assert(sl->spectrumIdentity(0).id == "scan=21");
    unit_assert(sl->spectrumIdentity(1).id == "scan=19");
    unit_assert(sl->spectrumIdentity(2).id == "sample=1 period=1 cycle=23 experiment=1");
}


void testWrapMZWindow()
{
    MSData msd;
    examples::initializeTiny(msd);

    SpectrumListPtr& sl = msd.run.spectrumListPtr;
    unit_assert(sl.get() && sl->size()>2);
    SpectrumPtr spectrum = sl->spectrum(0, true);
    vector<MZIntensityPair> data;
    spectrum->getMZIntensityPairs(data);
    unit_assert(data.size() == 15);

    SpectrumListFactory::wrap(msd, "mzWindow [9.5,15]");

    spectrum = sl->spectrum(0, true);
    spectrum->getMZIntensityPairs(data);
    unit_assert(data.size() == 5);

    spectrum = sl->spectrum(1, true);
    spectrum->getMZIntensityPairs(data);
    unit_assert(data.size() == 3);
}


void testWrapMSLevel()
{
    MSData msd;
    examples::initializeTiny(msd);

    SpectrumListPtr& sl = msd.run.spectrumListPtr;
    unit_assert(sl.get());
    unit_assert(sl->size() == 5);

    ostringstream oss;
    oss << "msLevel 2";
    SpectrumListFactory::wrap(msd, oss.str());
    unit_assert(sl->size() == 2);
    unit_assert(sl->spectrumIdentity(0).id == "scan=20");
}


void testWrapDefaultArrayLength()
{
    // test that the minimum length is 1 (due to 0 being the "unset" value)
    {
        MSData msd;
        examples::initializeTiny(msd);

        SpectrumListPtr& sl = msd.run.spectrumListPtr;
        unit_assert(sl.get());
        unit_assert(sl->size() == 5);

        SpectrumListFactory::wrap(msd, "defaultArrayLength 0-");
        unit_assert(sl->size() == 4);
        unit_assert(sl->find("scan=21") == sl->size());
    }

    // test filtering out all spectra
    {
        MSData msd;
        examples::initializeTiny(msd);
        SpectrumListPtr& sl = msd.run.spectrumListPtr;

        SpectrumListFactory::wrap(msd, "defaultArrayLength 100-");
        unit_assert(sl->size() == 0);
    }

    // test filtering out empty spectra
    {
        MSData msd;
        examples::initializeTiny(msd);
        SpectrumListPtr& sl = msd.run.spectrumListPtr;

        SpectrumListFactory::wrap(msd, "defaultArrayLength 1-");
        unit_assert(sl->size() == 4);
        unit_assert(sl->find("scan=21") == sl->size());
    }

    // test filtering out spectra with defaultArrayLength > 14
    {
        MSData msd;
        examples::initializeTiny(msd);
        SpectrumListPtr& sl = msd.run.spectrumListPtr;

        SpectrumListFactory::wrap(msd, "defaultArrayLength 15-");
        unit_assert(sl->size() == 2);
        unit_assert(sl->find("scan=20") == sl->size());
        unit_assert(sl->find("scan=21") == sl->size());
    }

    // test filtering out spectra with 0 < defaultArrayLength < 15
    {
        MSData msd;
        examples::initializeTiny(msd);
        SpectrumListPtr& sl = msd.run.spectrumListPtr;

        SpectrumListFactory::wrap(msd, "defaultArrayLength 1-14");
        unit_assert(sl->size() == 2);
        unit_assert(sl->find("scan=20") == 0);
    }
}

void testWrapActivation()
{
    // test filter by CID activation
    {
        MSData msd;
        examples::initializeTiny(msd);
        SpectrumListFactory::wrap(msd, "msLevel 2-");
        SpectrumListFactory::wrap(msd, "activation CID");
        unit_assert(msd.run.spectrumListPtr->size() == 1);
    }
    // test filter by ETD activation
    {
        MSData msd;
        examples::initializeTiny(msd);
        SpectrumListFactory::wrap(msd, "msLevel 2-");
        SpectrumListFactory::wrap(msd, "activation ETD");
        unit_assert(msd.run.spectrumListPtr->size() == 1);
    }
    // test filter by HCD activation
    {
        MSData msd;
        examples::initializeTiny(msd);
        SpectrumListFactory::wrap(msd, "msLevel 2-");
        SpectrumListFactory::wrap(msd, "activation HCD");
        unit_assert(msd.run.spectrumListPtr->size() == 0);
    }
    // test filter by IRMPD activation
    {
        MSData msd;
        examples::initializeTiny(msd);
        SpectrumListFactory::wrap(msd, "msLevel 2-");
        SpectrumListFactory::wrap(msd, "activation IRMPD");
        unit_assert(msd.run.spectrumListPtr->size() == 0);
    }
    // test invalid argument
    {
        MSData msd;
        examples::initializeTiny(msd);

        unit_assert_throws(SpectrumListFactory::wrap(msd, "activation UNEXPECTED_INPUT"), runtime_error);
    }
}

void testWrapMassAnalyzer()
{
    // test filter by ITMS analyzer type
    {
        MSData msd;
        examples::initializeTiny(msd);

        SpectrumListFactory::wrap(msd, "analyzerType ITMS");
        unit_assert(msd.run.spectrumListPtr->size() == 5);
    }
    // test filter by ITMS analyzer type (new syntax)
    {
        MSData msd;
        examples::initializeTiny(msd);

        SpectrumListFactory::wrap(msd, "analyzerType it");
        unit_assert(msd.run.spectrumListPtr->size() == 5);
    }
    // test filter by FTMS analyzer type
    {
        MSData msd;
        examples::initializeTiny(msd);

        SpectrumListFactory::wrap(msd, "analyzer FTMS");
        unit_assert(msd.run.spectrumListPtr->size() == 0);
    }
    // test filter by Orbi analyzer type
    {
        MSData msd;
        examples::initializeTiny(msd);

        SpectrumListFactory::wrap(msd, "analyzer Orbi");
        unit_assert(msd.run.spectrumListPtr->size() == 0);
    }
    // test filter by TOF analyzer type
    {
        MSData msd;
        examples::initializeTiny(msd);

        SpectrumListFactory::wrap(msd, "analyzer TOF");
        unit_assert(msd.run.spectrumListPtr->size() == 0);
    }
    // test invalid argument
    {
        MSData msd;
        examples::initializeTiny(msd);

        unit_assert_throws(SpectrumListFactory::wrap(msd, "analyzer UNEXPECTED_INPUT"), runtime_error)
    }
}

void testWrapPolarity()
{
    // test filter by positive polarity
    {
        MSData msd;
        examples::initializeTiny(msd);

        SpectrumListPtr& sl = msd.run.spectrumListPtr;
        unit_assert(sl.get());
        unit_assert(sl->size() == 5);

        SpectrumListFactory::wrap(msd, "polarity positive");
        unit_assert(sl->size() == 3);
    }
    // test filter by + polarity
    {
        MSData msd;
        examples::initializeTiny(msd);

        SpectrumListPtr& sl = msd.run.spectrumListPtr;
        unit_assert(sl.get());
        unit_assert(sl->size() == 5);

        SpectrumListFactory::wrap(msd, "polarity +");
        unit_assert(sl->size() == 3);
    }
    // test filter by negative polarity
    {
        MSData msd;
        examples::initializeTiny(msd);

        SpectrumListPtr& sl = msd.run.spectrumListPtr;
        unit_assert(sl.get());
        unit_assert(sl->size() == 5);

        SpectrumListFactory::wrap(msd, "polarity -");
        unit_assert(sl->size() == 2);
    }
    // test invalid argument
    {
        MSData msd;
        examples::initializeTiny(msd);

        SpectrumListPtr& sl = msd.run.spectrumListPtr;
        unit_assert(sl.get());
        unit_assert(sl->size() == 5);
        unit_assert_throws(SpectrumListFactory::wrap(msd, "polarity UNEXPECTED_INPUT"), runtime_error)
    }
}

void testWrapTitleMaker()
{
    MSData msd;
    examples::initializeTiny(msd);

    {
        SpectrumListFactory::wrap(msd, "titleMaker <Id>");
        SpectrumListPtr& sl = msd.run.spectrumListPtr;
        unit_assert_operator_equal("scan=19", sl->spectrum(0)->cvParam(MS_spectrum_title).value);
        unit_assert_operator_equal("scan=20", sl->spectrum(1)->cvParam(MS_spectrum_title).value);
        unit_assert_operator_equal("scan=21", sl->spectrum(2)->cvParam(MS_spectrum_title).value);
        unit_assert_operator_equal("scan=22", sl->spectrum(3)->cvParam(MS_spectrum_title).value);
        unit_assert_operator_equal("sample=1 period=1 cycle=23 experiment=1", sl->spectrum(4)->cvParam(MS_spectrum_title).value);
    }

    {
        // the outer titleMaker overrides the inner one
        SpectrumListFactory::wrap(msd, "titleMaker <Index>; <SpectrumType>, <MsLevel>");
        SpectrumListPtr& sl = msd.run.spectrumListPtr;
        unit_assert_operator_equal("0; MS1 spectrum, 1", sl->spectrum(0)->cvParam(MS_spectrum_title).value);
        unit_assert_operator_equal("1; MSn spectrum, 2", sl->spectrum(1)->cvParam(MS_spectrum_title).value);
        unit_assert_operator_equal("2; MS1 spectrum, 1", sl->spectrum(2)->cvParam(MS_spectrum_title).value);
        unit_assert_operator_equal("3; MSn spectrum, 2", sl->spectrum(3)->cvParam(MS_spectrum_title).value);
        unit_assert_operator_equal("4; MS1 spectrum, 1", sl->spectrum(4)->cvParam(MS_spectrum_title).value);
    }

    {
        SpectrumListFactory::wrap(msd, "titleMaker <ScanNumber> <MsLevel> <ActivationType> <ChargeState> <PrecursorSpectrumId>");
        SpectrumListPtr& sl = msd.run.spectrumListPtr;
        unit_assert_operator_equal("19 1   ", sl->spectrum(0)->cvParam(MS_spectrum_title).value);
        unit_assert_operator_equal("20 2 CID 2 scan=19", sl->spectrum(1)->cvParam(MS_spectrum_title).value);
        unit_assert_operator_equal("21 1   ", sl->spectrum(2)->cvParam(MS_spectrum_title).value);
        unit_assert_operator_equal("22 2 ETD/CID 2 scan=19", sl->spectrum(3)->cvParam(MS_spectrum_title).value);
        unit_assert_operator_equal("5 1   ", sl->spectrum(4)->cvParam(MS_spectrum_title).value);
    }
}

void test()
{
    testUsage(); 
    testWrap();
    testWrapScanTimeRange();
    testWrapSortScanTime();
    testWrapMZWindow();
    testWrapMSLevel();
    testWrapDefaultArrayLength();
    testWrapActivation();
    testWrapMassAnalyzer();
    testWrapPolarity();
    testWrapTitleMaker();
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
    catch (...)
    {
        cerr << "Caught unknown exception.\n";
        return 1;
    }
}

