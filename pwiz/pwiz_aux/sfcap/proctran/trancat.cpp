//
// $Id: trancat.cpp 1195 2009-08-14 22:12:04Z chambm $
//
// Copyright 2009 Spielberg Family Center for Applied Proteomics 
//   Cedars Sinai Medical Center, Los Angeles, California  90048
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


#include "pwiz_aux/sfcap/transient/TransientData.hpp"
#include <iostream>
#include <string>


using namespace std;
using namespace pwiz::data;


void doSomething(const string& filename)
{
    TransientData td(filename);

    cout << "size: " << td.data().size() << endl;

}



int main(int argc, const char* argv[])
{
    if (argc != 2)
    {
        cout << "Usage: trancat filename\n";
        return 1;
    }

    doSomething(argv[1]);
    return 0;
}

