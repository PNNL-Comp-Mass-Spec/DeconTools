//
// $Id: SpectrumList_mzML.hpp 2898 2011-08-01 17:55:13Z chambm $
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


#ifndef _SPECTRUMLIST_MZML_HPP_
#define _SPECTRUMLIST_MZML_HPP_


#include "pwiz/utility/misc/Export.hpp"
#include "SpectrumListBase.hpp"
#include "Index_mzML.hpp"
#include <iosfwd>


namespace pwiz {
namespace msdata {


/// implementation of SpectrumList, backed by an mzML file
class PWIZ_API_DECL SpectrumList_mzML : public SpectrumListBase
{
    public:

    static SpectrumListPtr create(boost::shared_ptr<std::istream> is,
                                  const MSData& msd,
                                  const Index_mzML_Ptr& indexPtr);
};


} // namespace msdata
} // namespace pwiz


#endif // _SPECTRUMLIST_MZML_HPP_

