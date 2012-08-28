//
// $Id: DiffTest.cpp 1785 2010-02-02 22:57:03Z chambm $
//
//
// Original author: Darren Kessner <darren@proteowizard.org>
//
// Copyright 2007 Spielberg Family Center for Applied Proteomics
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

//#include "Diff.hpp"
//#include "examples.hpp"
#include "pwiz/utility/misc/unit.hpp"


using namespace pwiz::util;
using namespace pwiz::CLI::cv;
using namespace pwiz::CLI::proteome;
using System::Console;
typedef System::String string;


public ref class Log
{
    public: static System::IO::TextWriter^ writer = nullptr;
};


void testProteomeData()
{
    if (Log::writer != nullptr) Log::writer->Write("testProteomeData()\n");

    ProteomeData a, b;
   
    a.id = "goober";  
    b.id = "goober";  

    Diff diff(a, b);
    unit_assert(!diff);

    b.id = "raisinet";        

    ProteinListSimple^ pl = gcnew ProteinListSimple();
    pl->proteins->Add(gcnew Protein("p1", 0, "", ""));
    b.proteinList = pl;

    diff.apply(a, b);
    if (Log::writer != nullptr) Log::writer->WriteLine((string^)diff);
    unit_assert((bool)diff);

    unit_assert(diff.a_b->proteinList != nullptr);
    unit_assert(diff.a_b->proteinList->size() == 1);
}


void test()
{
    testProteomeData();
}


int main(int argc, char* argv[])
{
    try
    {
        if (argc>1 && !strcmp(argv[1],"-v")) Log::writer = Console::Out;
        test();
        return 0;
    }
    catch (std::exception& e)
    {
        Console::Error->WriteLine("std::exception: " + gcnew string(e.what()));
    }
    catch (System::Exception^ e)
    {
        Console::Error->WriteLine("System.Exception: " + e->Message);
    }
    catch (...)
    {
        Console::Error->WriteLine("Caught unknown exception.\n");
    }
    
    return 1;
}

