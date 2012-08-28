//
// $Id: SpectrumList_Smoother.hpp 1191 2009-08-14 19:33:05Z chambm $
//
//
// Original author: Matt Chambers <matt.chambers <a.t> vanderbilt.edu>
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


#ifndef _SPECTRUMLIST_SMOOTHER_HPP_ 
#define _SPECTRUMLIST_SMOOTHER_HPP_ 


#include "pwiz/utility/misc/Export.hpp"
#include "pwiz/utility/misc/IntegerSet.hpp"
#include "pwiz/data/msdata/SpectrumListWrapper.hpp"
#include "pwiz/analysis/common/SavitzkyGolaySmoother.hpp"
#include "pwiz/analysis/common/WhittakerSmoother.hpp"


namespace pwiz {
namespace analysis {


/// SpectrumList implementation to return smoothed spectral data
class PWIZ_API_DECL SpectrumList_Smoother : public msdata::SpectrumListWrapper
{
    public:

    SpectrumList_Smoother(const msdata::SpectrumListPtr& inner,
                          SmootherPtr algorithm,
                          const util::IntegerSet& msLevelsToSmooth);


    static bool accept(const msdata::SpectrumListPtr& inner);

    virtual msdata::SpectrumPtr spectrum(size_t index, bool getBinaryData = false) const;

    private:
    SmootherPtr algorithm_;
    const util::IntegerSet msLevelsToSmooth_;
};


} // namespace analysis 
} // namespace pwiz


#endif // _SPECTRUMLIST_SMOOTHER_HPP_ 

