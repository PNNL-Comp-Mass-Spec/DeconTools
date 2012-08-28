//
// $Id: Reader_ABI_T2D_Test.cpp 1751 2010-01-27 17:53:22Z chambm $
//
//
// Original author: Matt Chambers <matt.chambers .@. vanderbilt.edu>
//
// Copyright 2010 Vanderbilt University - Nashville, TN 37232
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

#include "Reader_ABI_T2D.hpp"
#include "pwiz/utility/misc/VendorReaderTestHarness.hpp"
#include "pwiz/utility/misc/Filesystem.hpp"
#include "pwiz/utility/misc/String.hpp"
#include "pwiz/utility/misc/Stream.hpp"

struct Is_T2D_Directory : public pwiz::util::TestPathPredicate
{
    bool operator() (const string& datapath) const
    {
        vector<bfs::path> t2d_filepaths;
        pwiz::util::expand_pathmask(bfs::path(datapath) / "*.t2d", t2d_filepaths);
        return !t2d_filepaths.empty();
    }
};

int main(int argc, char* argv[])
{
    #ifdef PWIZ_READER_ABI_T2D
    const bool testAcceptOnly = false;
    #else
    const bool testAcceptOnly = true;
    #endif

    try
    {
        return pwiz::util::testReader(pwiz::msdata::Reader_ABI_T2D(),
                                      vector<string>(argv, argv+argc),
                                      testAcceptOnly,
                                      Is_T2D_Directory());
    }
    catch (std::runtime_error& e)
    {
        cerr << e.what() << endl;
        return 1;
    }
}
