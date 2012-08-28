//
// $Id: DefaultReaderList.hpp 2820 2011-06-27 22:51:16Z chambm $
//
//
// Original author: Robert Burke <robert.burke@proteowizard.org>
//
// Copyright 2009 Spielberg Family Center for Applied Proteomics
//   University of Southern California, Los Angeles, California  90033
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


#ifndef _MZID_DEFAULTREADERLIST_HPP_
#define _MZID_DEFAULTREADERLIST_HPP_


#include "pwiz/utility/misc/Export.hpp"
#include "Reader.hpp"


namespace pwiz {
namespace identdata {


/// default Reader list
class PWIZ_API_DECL DefaultReaderList : public ReaderList
{
    public:
    DefaultReaderList();
};


} // namespace identdata
} // namespace pwiz


#endif // _MZID_DEFAULTREADERLIST_HPP_

