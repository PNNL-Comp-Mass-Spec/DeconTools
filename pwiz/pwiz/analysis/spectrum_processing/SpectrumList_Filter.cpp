//
// $Id: SpectrumList_Filter.cpp 3310 2012-02-17 18:29:17Z chambm $
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


#define PWIZ_SOURCE

#include "pwiz/data/common/cv.hpp"
#include "SpectrumList_Filter.hpp"
#include "pwiz/utility/misc/Std.hpp"

namespace pwiz {
namespace analysis {


using namespace pwiz::cv;
using namespace pwiz::util;
using namespace pwiz::msdata;

using boost::logic::tribool;


//
// SpectrumList_Filter::Impl
//


struct SpectrumList_Filter::Impl
{
    const SpectrumListPtr original;
    std::vector<SpectrumIdentity> spectrumIdentities; // local cache, with fixed up index fields
    std::vector<size_t> indexMap; // maps index -> original index
    DetailLevel detailLevel; // the detail level needed for a non-indeterminate result

    Impl(SpectrumListPtr original, const Predicate& predicate);
    void pushSpectrum(const SpectrumIdentity& spectrumIdentity);
};


SpectrumList_Filter::Impl::Impl(SpectrumListPtr _original, const Predicate& predicate)
:   original(_original), detailLevel(DetailLevel_InstantMetadata)
{
    if (!original.get()) throw runtime_error("[SpectrumList_Filter] Null pointer");

    // iterate through the spectra, using predicate to build the sub-list
    for (size_t i=0, end=original->size(); i<end; i++)
    {
        if (predicate.done()) break;

        // first try to determine acceptance based on SpectrumIdentity alone
        const SpectrumIdentity& spectrumIdentity = original->spectrumIdentity(i);
        tribool accepted = predicate.accept(spectrumIdentity);

        if (accepted)
        {
            pushSpectrum(spectrumIdentity);            
        }
        else if (!accepted)
        {
            // do nothing 
        }
        else // indeterminate
        {
            // not enough info -- we need to retrieve the Spectrum
            do
            {
                SpectrumPtr spectrum = original->spectrum(i, detailLevel);
                accepted = predicate.accept(*spectrum);

                if (boost::logic::indeterminate(accepted) && detailLevel != DetailLevel_FullMetadata)
                    detailLevel = DetailLevel(int(detailLevel) + 1);
                else
                {
                    if (accepted)
                       pushSpectrum(spectrumIdentity);
                    break;
                }
            }
            while ((int) detailLevel <= (int) DetailLevel_FullMetadata);
        }
    }
}


void SpectrumList_Filter::Impl::pushSpectrum(const SpectrumIdentity& spectrumIdentity)
{
    indexMap.push_back(spectrumIdentity.index);
    spectrumIdentities.push_back(spectrumIdentity);
    spectrumIdentities.back().index = spectrumIdentities.size()-1;
}


//
// SpectrumList_Filter
//


PWIZ_API_DECL SpectrumList_Filter::SpectrumList_Filter(const SpectrumListPtr original, const Predicate& predicate)
:   SpectrumListWrapper(original), impl_(new Impl(original, predicate))
{}


PWIZ_API_DECL size_t SpectrumList_Filter::size() const
{
    return impl_->indexMap.size();
}


PWIZ_API_DECL const SpectrumIdentity& SpectrumList_Filter::spectrumIdentity(size_t index) const
{
    return impl_->spectrumIdentities.at(index);
}


PWIZ_API_DECL SpectrumPtr SpectrumList_Filter::spectrum(size_t index, bool getBinaryData) const
{
    size_t originalIndex = impl_->indexMap.at(index);
    SpectrumPtr originalSpectrum = impl_->original->spectrum(originalIndex, getBinaryData);  

    SpectrumPtr newSpectrum(new Spectrum(*originalSpectrum));
    newSpectrum->index = index;

    return newSpectrum;
}


//
// SpectrumList_FilterPredicate_IndexSet 
//


PWIZ_API_DECL SpectrumList_FilterPredicate_IndexSet::SpectrumList_FilterPredicate_IndexSet(const IntegerSet& indexSet)
:   indexSet_(indexSet), eos_(false)
{}


PWIZ_API_DECL tribool SpectrumList_FilterPredicate_IndexSet::accept(const SpectrumIdentity& spectrumIdentity) const
{
    if (indexSet_.hasUpperBound((int)spectrumIdentity.index)) eos_ = true;
    bool result = indexSet_.contains((int)spectrumIdentity.index);
    return result;
}


PWIZ_API_DECL bool SpectrumList_FilterPredicate_IndexSet::done() const
{
    return eos_; // end of set
}


//
// SpectrumList_FilterPredicate_ScanNumberSet 
//


PWIZ_API_DECL SpectrumList_FilterPredicate_ScanNumberSet::SpectrumList_FilterPredicate_ScanNumberSet(const IntegerSet& scanNumberSet)
:   scanNumberSet_(scanNumberSet), eos_(false)
{}


PWIZ_API_DECL tribool SpectrumList_FilterPredicate_ScanNumberSet::accept(const SpectrumIdentity& spectrumIdentity) const
{
    int scanNumber = id::valueAs<int>(spectrumIdentity.id, "scan");
    if (scanNumberSet_.hasUpperBound(scanNumber)) eos_ = true;
    bool result = scanNumberSet_.contains(scanNumber);
    return result;
}


PWIZ_API_DECL bool SpectrumList_FilterPredicate_ScanNumberSet::done() const
{
    return eos_; // end of set
}


//
// SpectrumList_FilterPredicate_ScanEventSet 
//


PWIZ_API_DECL SpectrumList_FilterPredicate_ScanEventSet::SpectrumList_FilterPredicate_ScanEventSet(const IntegerSet& scanEventSet)
:   scanEventSet_(scanEventSet)
{}


PWIZ_API_DECL boost::logic::tribool SpectrumList_FilterPredicate_ScanEventSet::accept(const msdata::Spectrum& spectrum) const
{
    Scan dummy;
    const Scan& scan = spectrum.scanList.scans.empty() ? dummy : spectrum.scanList.scans[0];
    CVParam param = scan.cvParam(MS_preset_scan_configuration);
    if (param.cvid == CVID_Unknown) return boost::logic::indeterminate;
    int scanEvent = lexical_cast<int>(param.value);
    bool result = scanEventSet_.contains(scanEvent);
    return result;
}


//
// SpectrumList_FilterPredicate_ScanTimeRange 
//


PWIZ_API_DECL SpectrumList_FilterPredicate_ScanTimeRange::SpectrumList_FilterPredicate_ScanTimeRange(double scanTimeLow, double scanTimeHigh)
:   scanTimeLow_(scanTimeLow), scanTimeHigh_(scanTimeHigh)
{}


PWIZ_API_DECL tribool SpectrumList_FilterPredicate_ScanTimeRange::accept(const SpectrumIdentity& spectrumIdentity) const
{
    // TODO: encode scan time in mzML index (and SpectrumIdentity)
    return boost::logic::indeterminate;
}


PWIZ_API_DECL boost::logic::tribool SpectrumList_FilterPredicate_ScanTimeRange::accept(const msdata::Spectrum& spectrum) const
{
    Scan dummy;
    const Scan& scan = spectrum.scanList.scans.empty() ? dummy : spectrum.scanList.scans[0];
    CVParam param = scan.cvParam(MS_scan_start_time);
    if (param.cvid == CVID_Unknown) return boost::logic::indeterminate;
    double time = param.timeInSeconds();

    return (time>=scanTimeLow_ && time<=scanTimeHigh_);
}


//
// SpectrumList_FilterPredicate_MSLevelSet 
//


PWIZ_API_DECL SpectrumList_FilterPredicate_MSLevelSet::SpectrumList_FilterPredicate_MSLevelSet(const IntegerSet& msLevelSet)
:   msLevelSet_(msLevelSet)
{}


PWIZ_API_DECL boost::logic::tribool SpectrumList_FilterPredicate_MSLevelSet::accept(const msdata::Spectrum& spectrum) const
{
    CVParam param = spectrum.cvParamChild(MS_spectrum_type);
    if (param.cvid == CVID_Unknown) return boost::logic::indeterminate;
    if (!cvIsA(param.cvid, MS_mass_spectrum))
        return true; // MS level filter doesn't affect non-MS spectra
    param = spectrum.cvParam(MS_ms_level);
    if (param.cvid == CVID_Unknown) return boost::logic::indeterminate;
    int msLevel = param.valueAs<int>();
    bool result = msLevelSet_.contains(msLevel);
    return result;
}


//
// SpectrumList_FilterPredicate_PrecursorMzSet 
//


PWIZ_API_DECL SpectrumList_FilterPredicate_PrecursorMzSet::SpectrumList_FilterPredicate_PrecursorMzSet(const std::set<double>& precursorMzSet)
:   precursorMzSet_(precursorMzSet)
{}


PWIZ_API_DECL boost::logic::tribool SpectrumList_FilterPredicate_PrecursorMzSet::accept(const msdata::Spectrum& spectrum) const
{
    double precursorMz = getPrecursorMz(spectrum);
    if (precursorMz == 0)
    {
        CVParam param = spectrum.cvParam(MS_ms_level);
        if (param.cvid == CVID_Unknown) return boost::logic::indeterminate;
        int msLevel = param.valueAs<int>();
        // If not level 1, then it should have a precursor, so request more meta data.
        if (msLevel != 1) return boost::logic::indeterminate;
    }
    bool result = precursorMzSet_.count(precursorMz)>0;
    return result;
}

PWIZ_API_DECL double SpectrumList_FilterPredicate_PrecursorMzSet::getPrecursorMz(const msdata::Spectrum& spectrum) const
{
    for (size_t i=0; i<spectrum.precursors.size(); i++)
    {
		for (size_t j=0; j<spectrum.precursors[i].selectedIons.size(); j++)
		{
			CVParam param = spectrum.precursors[i].selectedIons[j].cvParam(MS_selected_ion_m_z);
			if (param.cvid != CVID_Unknown)
				return lexical_cast<double>(param.value);
        }
	}
	return 0;
}


//
// SpectrumList_FilterPredicate_DefaultArrayLengthSet 
//


PWIZ_API_DECL SpectrumList_FilterPredicate_DefaultArrayLengthSet::SpectrumList_FilterPredicate_DefaultArrayLengthSet(const IntegerSet& defaultArrayLengthSet)
:   defaultArrayLengthSet_(defaultArrayLengthSet)
{}


PWIZ_API_DECL boost::logic::tribool SpectrumList_FilterPredicate_DefaultArrayLengthSet::accept(const msdata::Spectrum& spectrum) const
{
    if (spectrum.defaultArrayLength == 0)
        return boost::logic::indeterminate;
    return defaultArrayLengthSet_.contains(spectrum.defaultArrayLength);
}


//
// SpectrumList_FilterPredicate_ActivationType
//


PWIZ_API_DECL SpectrumList_FilterPredicate_ActivationType::SpectrumList_FilterPredicate_ActivationType(const set<CVID> cvFilterItems_, bool hasNoneOf_)
: hasNoneOf(hasNoneOf_)
{
    BOOST_FOREACH(const CVID cvid, cvFilterItems_)
    {
        CVTermInfo info = cvTermInfo(cvid); 
        if (std::find(info.parentsIsA.begin(), info.parentsIsA.end(), MS_dissociation_method) == info.parentsIsA.end())
        {
            throw runtime_error("first argument not an activation type");
        }

        cvFilterItems.insert(cvid);
    }

}

PWIZ_API_DECL boost::logic::tribool SpectrumList_FilterPredicate_ActivationType::accept(const msdata::Spectrum& spectrum) const
{
    CVParam param = spectrum.cvParamChild(MS_spectrum_type);
    if (param.cvid == CVID_Unknown) return boost::logic::indeterminate;
    if (!cvIsA(param.cvid, MS_mass_spectrum))
        return true; // activation filter doesn't affect non-MS spectra

    param = spectrum.cvParam(MS_ms_level);
    if (param.cvid == CVID_Unknown) return boost::logic::indeterminate;
    int msLevel = param.valueAs<int>();

    if (msLevel == 1)
        return true; // activation filter doesn't affect MS1 spectra

    if (spectrum.precursors.empty() ||
        spectrum.precursors[0].selectedIons.empty() ||
        spectrum.precursors[0].selectedIons[0].empty())
        return boost::logic::indeterminate;

    const Activation& activation = spectrum.precursors[0].activation;

    bool res = true;
    BOOST_FOREACH(const CVID cvid, cvFilterItems)
    {
        if (hasNoneOf)
            res &= !activation.hasCVParam(cvid);
        else
            res &= activation.hasCVParam(cvid);
    }

    return res;
}


//
// SpectrumList_FilterPredicate_AnalyzerType
//


PWIZ_API_DECL SpectrumList_FilterPredicate_AnalyzerType::SpectrumList_FilterPredicate_AnalyzerType(const set<CVID> cvFilterItems_)
{
    BOOST_FOREACH(const CVID cvid, cvFilterItems_)
    {
        CVTermInfo info = cvTermInfo(cvid); 
        if (std::find(info.parentsIsA.begin(), info.parentsIsA.end(), MS_mass_analyzer_type) == info.parentsIsA.end())
        {
            throw runtime_error("first argument not an analyzer type");
        }

        cvFilterItems.insert(cvid);
    }

}

PWIZ_API_DECL boost::logic::tribool SpectrumList_FilterPredicate_AnalyzerType::accept(const msdata::Spectrum& spectrum) const
{
    bool res = false;
    Scan dummy;
    const Scan& scan = spectrum.scanList.scans.empty() ? dummy : spectrum.scanList.scans[0];

    CVID massAnalyzerType = CVID_Unknown;
    if (scan.instrumentConfigurationPtr.get())
        try
        {
            massAnalyzerType = scan.instrumentConfigurationPtr->componentList.analyzer(0)
                                        .cvParamChild(MS_mass_analyzer_type).cvid;
        }
        catch (out_of_range&)
        {
            // ignore out-of-range exception
        }

    if (massAnalyzerType == CVID_Unknown)
        return boost::logic::indeterminate;

    BOOST_FOREACH(const CVID cvid, cvFilterItems)
        if (cvIsA(massAnalyzerType, cvid))
        {
            res = true;
            break;
        }

    return res;
}


//
// SpectrumList_FilterPredicate_Polarity
//


PWIZ_API_DECL SpectrumList_FilterPredicate_Polarity::SpectrumList_FilterPredicate_Polarity(CVID polarity) : polarity(polarity) {}

PWIZ_API_DECL boost::logic::tribool SpectrumList_FilterPredicate_Polarity::accept(const msdata::Spectrum& spectrum) const
{
    CVParam param = spectrum.cvParamChild(MS_polarity);
    if (param.cvid == CVID_Unknown)
        return boost::logic::indeterminate;
    return param.cvid == polarity;
}


} // namespace analysis
} // namespace pwiz

