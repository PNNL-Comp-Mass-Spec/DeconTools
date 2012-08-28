//
// $Id: ClickwrapPrompterTest.cpp 2281 2010-09-23 18:06:45Z chambm $
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


#include "unit.hpp"
#include "ClickwrapPrompter.hpp"
#include "Std.hpp"


using namespace pwiz::util;


ostream* os_ = 0;


void test()
{
    unit_assert(ClickwrapPrompter::prompt("Test", "This is a test of the emergency prompting system.", "test"));
}


int main(int argc, char* argv[])
{
    try
    {
        if (argc>1 && !strcmp(argv[1],"-v")) os_ = &cout;
        if (os_) *os_ << "ClickwrapPrompterTest\n";
        test();
        return 0;
    }
    catch (exception& e)
    {
        cerr << e.what() << endl;
    }
    catch (...)
    {
        cerr << "Unknown exception" << endl;
    }
    return 1;
}
