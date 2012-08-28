//
// $Id: SpectrumListWrapper.hpp 1189 2009-08-14 17:36:06Z chambm $
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


#ifndef _SPECTRUMLISTWRAPPER_HPP_ 
#define _SPECTRUMLISTWRAPPER_HPP_ 


#include "pwiz/data/msdata/MSData.hpp"
#include <stdexcept>


namespace pwiz {
namespace msdata {


/// Inheritable pass-through implementation for wrapping a SpectrumList 
class PWIZ_API_DECL SpectrumListWrapper : public SpectrumList
{
    public:

    SpectrumListWrapper(const SpectrumListPtr& inner)
    :   inner_(inner),
        dp_(inner->dataProcessingPtr().get() ? new DataProcessing(*inner->dataProcessingPtr())
                                             : new DataProcessing("pwiz_Spectrum_Processing"))
    {
        if (!inner.get()) throw std::runtime_error("[SpectrumListWrapper] Null SpectrumListPtr.");
    }

    virtual size_t size() const {return inner_->size();}
    virtual bool empty() const {return inner_->empty();}
    virtual const SpectrumIdentity& spectrumIdentity(size_t index) const {return inner_->spectrumIdentity(index);} 
    virtual SpectrumPtr spectrum(size_t index, bool getBinaryData = false) const {return inner_->spectrum(index, getBinaryData);}
    virtual const boost::shared_ptr<const DataProcessing> dataProcessingPtr() const {return dp_;}
    protected:

    SpectrumListPtr inner_;
    DataProcessingPtr dp_;
};


} // namespace msdata 
} // namespace pwiz


#endif // _SPECTRUMLISTWRAPPER_HPP_ 

