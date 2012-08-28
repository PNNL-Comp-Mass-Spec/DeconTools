//
// $Id: Reader_Waters_Test.cpp 3397 2012-03-19 19:20:57Z pcbrefugee $
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


#include "Reader_Waters.hpp"
#include "pwiz/utility/misc/VendorReaderTestHarness.hpp"
#include "pwiz/utility/misc/Filesystem.hpp"
#include "pwiz/utility/misc/String.hpp"
#include "pwiz/utility/misc/Stream.hpp"

struct IsRawData : public pwiz::util::TestPathPredicate
{
    bool operator() (const string& rawpath) const
    {
        return bfs::is_directory(rawpath) &&
               bal::to_lower_copy(BFS_STRING(bfs::path(rawpath).extension())) == ".raw";
    }
};

int main(int argc, char* argv[])
{
    #ifdef PWIZ_READER_WATERS
    const bool testAcceptOnly = false;
    #else
    const bool testAcceptOnly = true;
    #endif

    try
    {
        return pwiz::util::testReader(pwiz::msdata::Reader_Waters(),
                                      vector<string>(argv, argv+argc),
                                      testAcceptOnly,
                                      IsRawData());
    }
    catch (std::runtime_error& e)
    {
        cerr << e.what() << endl;
        return 1;
    }
}
