//
// $Id: examples.hpp 2165 2010-07-31 16:25:10Z chambm $ 
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


#ifndef _TRADATA_EXAMPLES_HPP_CLI_
#define _TRADATA_EXAMPLES_HPP_CLI_


#include "TraData.hpp"


namespace pwiz {
namespace CLI {
namespace tradata {


public ref class examples
{
    public:
    static void initializeTiny(TraData^ trad);
    static void addMIAPEExampleMetadata(TraData^ trad);
};


} // namespace tradata
} // namespace CLI
} // namespace pwiz


#endif // _EXAMPLES_HPP_CLI_
