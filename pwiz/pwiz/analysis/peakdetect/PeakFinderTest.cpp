//
// $Id: PeakFinderTest.cpp 2051 2010-06-15 18:39:13Z chambm $
//
//
// Original author: Darren Kessner <darren@proteowizard.org>
//
// Copyright 2009 Center for Applied Molecular Medicine
//   University of Southern California, Los Angeles, CA
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


#include "PeakFinder.hpp"
#include "pwiz/utility/misc/unit.hpp"
#include "pwiz/utility/misc/Std.hpp"
#include <cstring>


using namespace pwiz::math;
using namespace pwiz::util;
using namespace pwiz::analysis;


ostream* os_ = 0;


double testNoise_[] =  // generated ~ N(10,1)
{
    0, 10.2134, 1, 9.50442, 2, 11.5754, 3, 8.9408, 4, 11.8393, 5, 11.8858, 6, 10.6047, 7, 9.63402, 8, 9.42174, 9, 9.36562, 
    10, 11.0297, 11, 10.7241, 12, 9.88493, 13, 10.6358, 14, 6.99061, 15, 9.08698, 16, 13.2407, 17, 9.11359, 18, 12.5566, 19, 9.42665, 
    20, 10.8823, 21, 11.3452, 22, 12.7695, 23, 9.48237, 24, 10.4651, 25, 9.87172, 26, 8.21869, 27, 10.1641, 28, 10.2608, 29, 9.20198, 
    30, 10.0615, 31, 10.0762, 32, 9.56936, 33, 10.2306, 34, 11.2333, 35, 9.27828, 36, 10.5381, 37, 8.01883, 38, 11.1455, 39, 9.70199, 
    40, 9.00168, 41, 8.51621, 42, 10.9232, 43, 10.2107, 44, 10.4026, 45, 9.43944, 46, 11.3842, 47, 9.39058, 48, 9.56328, 49, 9.09075, 
    50, 10.0799, 51, 8.35904, 52, 9.95105, 53, 8.86625, 54, 9.18384, 55, 10.6562, 56, 9.65414, 57, 9.48778, 58, 10.4029, 59, 10.746, 
    60, 9.51285, 61, 8.28112, 62, 10.8551, 63, 10.1733, 64, 9.65835, 65, 12.0004, 66, 10.5445, 67, 10.1626, 68, 12.6242, 69, 11.8353, 
    70, 10.8273, 71, 8.33673, 72, 9.82429, 73, 9.51358, 74, 9.30484, 75, 11.55, 76, 11.1051, 77, 9.64263, 78, 11.4417, 79, 10.317, 
    80, 9.51919, 81, 10.1948, 82, 9.49461, 83, 10.4654, 84, 10.0316, 85, 9.67727, 86, 10.0763, 87, 9.73844, 88, 10.396, 89, 10.9456, 
    90, 8.89552, 91, 10.0711, 92, 8.91056, 93, 10.3877, 94, 8.92218, 95, 8.58656, 96, 9.43114, 97, 7.82059, 98, 10.0535, 99, 8.1854
}; // testNoise_


void addSignal(OrderedPair* p)
{
    (p-2)->y += 3;
    (p-1)->y += 5;
    p->y += 6;
    (p+1)->y += 5;
    (p+2)->y += 3;
}


vector<double> createTestData()
{
    vector<double> data;
    copy(testNoise_, testNoise_+sizeof(testNoise_)/sizeof(double), back_inserter(data));

    addSignal((OrderedPair*)&data[50]);
    addSignal((OrderedPair*)&data[100]);
    addSignal((OrderedPair*)&data[150]);

    return data;
}


void test_SNR()
{
    if (os_) *os_ << "test_SNR()\n";

    shared_ptr<NoiseCalculator> noiseCalculator(new NoiseCalculator_2Pass);

    PeakFinder_SNR::Config config;
    config.windowRadius = 2;
    config.log = os_;

    PeakFinder_SNR peakFinder(noiseCalculator, config);

    vector<double> data = createTestData();
    vector<size_t> peakIndices;

    peakFinder.findPeaks(data, peakIndices);

    if (os_)
    {
        *os_ << "peakIndices: " << peakIndices.size() << endl;
        copy(peakIndices.begin(), peakIndices.end(), ostream_iterator<size_t>(*os_, "\n"));
    }

    unit_assert(peakIndices.size() == 3);
    unit_assert(peakIndices[0] == 25);
    unit_assert(peakIndices[1] == 50);
    unit_assert(peakIndices[2] == 75);
}


void test()
{
    test_SNR();
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

