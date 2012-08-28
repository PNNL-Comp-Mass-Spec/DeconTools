//
// $Id: MassHunterData.cpp 2856 2011-07-13 18:24:36Z chambm $
//
//
// Original author: Brendan MacLean <brendanx .@. u.washington.edu>
//
// Copyright 2009 University of Washington - Seattle, WA
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

#pragma unmanaged
#include "MassHunterData.hpp"
#include "pwiz/utility/misc/Std.hpp"
#include "pwiz/utility/misc/DateTime.hpp"
#include "pwiz/utility/misc/Filesystem.hpp"
#include "boost/thread/mutex.hpp"


#pragma managed
#include "pwiz/utility/misc/cpp_cli_utilities.hpp"
using namespace pwiz::util;


using System::String;
using System::Math;
namespace MHDAC = Agilent::MassSpectrometry::DataAnalysis;


namespace pwiz {
namespace vendor_api {
namespace Agilent {


namespace {

MHDAC::IMsdrPeakFilter^ msdrPeakFilter(PeakFilterPtr peakFilter)
{
    MHDAC::IMsdrPeakFilter^ result = gcnew MHDAC::MsdrPeakFilter();
    if (peakFilter.get())
    {
        result->MaxNumPeaks = peakFilter->maxNumPeaks;
        result->AbsoluteThreshold = peakFilter->absoluteThreshold;
        result->RelativeThreshold = peakFilter->relativeThreshold;
    }
    return result;
}

MHDAC::IBDASpecFilter^ bdaSpecFilterForScanId(int scanId, bool preferProfileData = false )
{
    MHDAC::IBDASpecFilter^ result = gcnew MHDAC::BDASpecFilter();
    result->ScanIds = gcnew cli::array<int> { scanId };
    result->SpectrumType = MHDAC::SpecType::MassSpectrum;

    // default is DesiredMSStorageType::PeakElseProfile
    if (preferProfileData)
        result->DesiredMSStorageType = MHDAC::DesiredMSStorageType::ProfileElsePeak;
    else
        result->DesiredMSStorageType = MHDAC::DesiredMSStorageType::PeakElseProfile;

    return result;
}

boost::mutex massHunterInitMutex;

} // namespace


class MassHunterDataImpl : public MassHunterData
{
    public:
    MassHunterDataImpl(const std::string& path);
    ~MassHunterDataImpl();

    virtual std::string getVersion() const;
    virtual DeviceType getDeviceType() const;
    virtual std::string getDeviceName(DeviceType deviceType) const;
    virtual blt::local_date_time getAcquisitionTime() const;
    virtual IonizationMode getIonModes() const;
    virtual MSScanType getScanTypes() const;
    virtual MSStorageMode getSpectraFormat() const;
    virtual int getTotalScansPresent() const;

    virtual const set<Transition>& getTransitions() const;
    virtual ChromatogramPtr getChromatogram(const Transition& transition) const;

    virtual const automation_vector<double>& getTicTimes() const;
    virtual const automation_vector<double>& getBpcTimes() const;
    virtual const automation_vector<float>& getTicIntensities() const;
    virtual const automation_vector<float>& getBpcIntensities() const;

    virtual ScanRecordPtr getScanRecord(int row) const;
    virtual SpectrumPtr getProfileSpectrumByRow(int row) const;
    virtual SpectrumPtr getPeakSpectrumByRow(int row, PeakFilterPtr peakFilter = PeakFilterPtr()) const;

    virtual SpectrumPtr getProfileSpectrumById(int scanId) const;
    virtual SpectrumPtr getPeakSpectrumById(int scanId, PeakFilterPtr peakFilter = PeakFilterPtr()) const;

    private:
    gcroot<MHDAC::IMsdrDataReader^> reader_;
    gcroot<MHDAC::IBDAMSScanFileInformation^> scanFileInfo_;
    automation_vector<double> ticTimes_, bpcTimes_;
    automation_vector<float> ticIntensities_, bpcIntensities_;
    set<Transition> transitions_;
    map<Transition, int> transitionToChromatogramIndexMap_;

    // cached because MassHunter can only load all chromatograms
    // at the same time. Not caching caused serious performance problems.
    gcroot<array<MHDAC::IBDAChromData^>^> chromMrm_;
    // unable to achieve identical results with cached SIM chromatograms
    // gcroot<array<MHDAC::IBDAChromData^>^> chromSim_;

};

typedef boost::shared_ptr<MassHunterDataImpl> MassHunterDataImplPtr;


struct ScanRecordImpl : public ScanRecord
{
    ScanRecordImpl(MHDAC::IMSScanRecord^ scanRecord) : scanRecord_(scanRecord) {}

    virtual int getScanId() const {return scanRecord_->ScanID;}
    virtual double getRetentionTime() const {return scanRecord_->RetentionTime;}
    virtual int getMSLevel() const {return scanRecord_->MSLevel == MHDAC::MSLevel::MSMS ? 2 : 1;}
    virtual MSScanType getMSScanType() const {return (MSScanType) scanRecord_->MSScanType;}
    virtual double getTic() const {return scanRecord_->Tic;}
    virtual double getBasePeakMZ() const {return scanRecord_->BasePeakMZ;}
    virtual double getBasePeakIntensity() const {return scanRecord_->BasePeakIntensity;}
    virtual IonizationMode getIonizationMode() const {return (IonizationMode) scanRecord_->IonizationMode;}
    virtual IonPolarity getIonPolarity() const {return (IonPolarity) scanRecord_->IonPolarity;}
    virtual double getMZOfInterest() const {return scanRecord_->MZOfInterest;}
    virtual int getTimeSegment() const {return scanRecord_->TimeSegment;}
    virtual double getFragmentorVoltage() const {return scanRecord_->FragmentorVoltage;}
    virtual double getCollisionEnergy() const {return scanRecord_->CollisionEnergy;}
    virtual bool getIsFragmentorVoltageDynamic() const {return scanRecord_->IsFragmentorVoltageDynamic;}
    virtual bool getIsCollisionEnergyDynamic() const {return scanRecord_->IsCollisionEnergyDynamic;}

    private:
    gcroot<MHDAC::IMSScanRecord^> scanRecord_;
};


struct SpectrumImpl : public Spectrum
{
    SpectrumImpl(MHDAC::IBDASpecData^ specData) : specData_(specData) {}

    virtual int getScanId() const {return specData_->ScanId;}
    virtual int getMSLevel() const {return specData_->MSLevelInfo == MHDAC::MSLevel::MSMS ? 2 : 1;}
    virtual MSScanType getMSScanType() const {return (MSScanType) specData_->MSScanType;}
    virtual MSStorageMode getMSStorageMode() const {return (MSStorageMode) specData_->MSStorageMode;}
    virtual IonPolarity getIonPolarity() const {return (IonPolarity) specData_->IonPolarity;}
    virtual DeviceType getDeviceType() const {return (DeviceType) specData_->DeviceType;}
    virtual double getCollisionEnergy() const {return specData_->CollisionEnergy;}
    virtual int getTotalDataPoints() const {return specData_->TotalDataPoints;}
    virtual int getParentScanId() const {return (int) specData_->ParentScanId;}

    virtual MassRange getMeasuredMassRange() const;
    virtual void getPrecursorIons(vector<double>& precursorIons) const;
    virtual bool getPrecursorCharge(int& charge) const;
    virtual bool getPrecursorIntensity(double& precursorIntensity) const;
    virtual void getXArray(automation_vector<double>& x) const;
    virtual void getYArray(automation_vector<float>& y) const;

    private:
    gcroot<MHDAC::IBDASpecData^> specData_;
};


struct ChromatogramImpl : public Chromatogram
{
    ChromatogramImpl(MHDAC::IBDAChromData^ chromData) : chromData_(chromData) {}

    virtual double getCollisionEnergy() const;
    virtual int getTotalDataPoints() const;
    virtual void getXArray(automation_vector<double>& x) const;
    virtual void getYArray(automation_vector<float>& y) const;

    private:
    gcroot<MHDAC::IBDAChromData^> chromData_;
};


#pragma unmanaged
PWIZ_API_DECL
bool Transition::operator< (const Transition& rhs) const
{
    if (type == rhs.type)
        if (Q1 == rhs.Q1)
            if (Q3 == rhs.Q3)
                if (acquiredTimeRange.start == rhs.acquiredTimeRange.start)
                    return acquiredTimeRange.end < rhs.acquiredTimeRange.end;
                else
                    return acquiredTimeRange.start < rhs.acquiredTimeRange.start;
            else
                return Q3 < rhs.Q3;
        else
            return Q1 < rhs.Q1;
    else
        return type < rhs.type;
}


PWIZ_API_DECL
MassHunterDataPtr MassHunterData::create(const string& path)
{
    MassHunterDataImplPtr dataReader(new MassHunterDataImpl(path));
    return boost::static_pointer_cast<MassHunterData>(dataReader);
}

#pragma managed
MassHunterDataImpl::MassHunterDataImpl(const std::string& path)
{
    try
    {
        {
            boost::mutex::scoped_lock lock(massHunterInitMutex);
            reader_ = gcnew MHDAC::MassSpecDataReader();
            if (!reader_->OpenDataFile(ToSystemString(path)))
            {}    // TODO: log warning about incomplete acquisition, possibly indicating corrupt data
        }

        scanFileInfo_ = reader_->MSScanFileInformation;

        // cycle summing can make the full file chromatograms have the wrong number of points
        MHDAC::IBDAChromFilter^ filter = gcnew MHDAC::BDAChromFilter();
        filter->DoCycleSum = false;

        // set filter for TIC
        filter->ChromatogramType = MHDAC::ChromType::TotalIon;
        MHDAC::IBDAChromData^ tic = reader_->GetChromatogram(filter)[0];
        ToAutomationVector(tic->XArray, ticTimes_);
        ToAutomationVector(tic->YArray, ticIntensities_);

        // set filter for BPC
        filter->ChromatogramType = MHDAC::ChromType::BasePeak;
        MHDAC::IBDAChromData^ bpc = reader_->GetChromatogram(filter)[0];
        ToAutomationVector(bpc->XArray, bpcTimes_);
        ToAutomationVector(bpc->YArray, bpcIntensities_);

		// chromatograms are always read completely into memory, and failing
		// to store them on this object after reading cost a 50x performance
		// hit on large MRM files.
        filter = gcnew MHDAC::BDAChromFilter();
        filter->DoCycleSum = false;
        filter->ExtractOneChromatogramPerScanSegment = true;
        filter->ChromatogramType = MHDAC::ChromType::MultipleReactionMode;
        array<MHDAC::IBDAChromData^>^ chromatograms = reader_->GetChromatogram(filter);
        for each (MHDAC::IBDAChromData^ chromatogram in chromatograms)
        {
            if (chromatogram->MZOfInterest->Length == 0 ||
                chromatogram->MeasuredMassRange->Length == 0)
                // TODO: log this anomaly
                continue;

            Transition t;
            t.type = Transition::MRM;
            t.Q1 = chromatogram->MZOfInterest[0]->Start;
            t.Q3 = chromatogram->MeasuredMassRange[0]->Start;

            if (chromatogram->AcquiredTimeRange->Length > 0)
            {
                t.acquiredTimeRange.start = chromatogram->AcquiredTimeRange[0]->Start;
                t.acquiredTimeRange.end = chromatogram->AcquiredTimeRange[0]->End;
            }
            else
                t.acquiredTimeRange.start = t.acquiredTimeRange.end;

            transitionToChromatogramIndexMap_[t] = transitions_.size();
            transitions_.insert(t);
        }
        chromMrm_ = chromatograms;

        int mrmCount = transitions_.size();

        filter->ChromatogramType = MHDAC::ChromType::SelectedIonMonitoring;
        chromatograms = reader_->GetChromatogram(filter);
        for each (MHDAC::IBDAChromData^ chromatogram in chromatograms)
        {
            if (chromatogram->MeasuredMassRange->Length == 0)
                // TODO: log this anomaly
                continue;

            Transition t;
            t.type = Transition::SIM;
            t.Q1 = chromatogram->MeasuredMassRange[0]->Start;
            t.Q3 = 0;

            if (chromatogram->AcquiredTimeRange->Length > 0)
            {
                t.acquiredTimeRange.start = chromatogram->AcquiredTimeRange[0]->Start;
                t.acquiredTimeRange.end = chromatogram->AcquiredTimeRange[0]->End;
            }
            else
                t.acquiredTimeRange.start = t.acquiredTimeRange.end = 0;

            transitionToChromatogramIndexMap_[t] = transitions_.size() - mrmCount;
            transitions_.insert(t);
        }
        // unfortunately, storing the chromatograms read here for SelectedIonMonitoring
        // even with the new filter did not produce results identical to the original
        // code, causing tests to fail.
        // someone with more knowledge of the tests and SIM would have to fix this.
        // chromSim_ = chromatograms;
    }
    CATCH_AND_FORWARD
}

MassHunterDataImpl::~MassHunterDataImpl()
{
    try {reader_->CloseDataFile();} CATCH_AND_FORWARD
}

std::string MassHunterDataImpl::getVersion() const
{
    try {return ToStdString(reader_->Version);} CATCH_AND_FORWARD
}

DeviceType MassHunterDataImpl::getDeviceType() const
{
    try {return (DeviceType) scanFileInfo_->DeviceType;} CATCH_AND_FORWARD
}

std::string MassHunterDataImpl::getDeviceName(DeviceType deviceType) const
{
    try {return ToStdString(reader_->FileInformation->GetDeviceName((MHDAC::DeviceType) deviceType));} CATCH_AND_FORWARD
}

blt::local_date_time MassHunterDataImpl::getAcquisitionTime() const
{
    try
    {
        bpt::ptime pt(bdt::time_from_OADATE<bpt::ptime>(reader_->FileInformation->AcquisitionTime.ToUniversalTime().ToOADate()));
        return blt::local_date_time(pt, blt::time_zone_ptr()); // keep time as UTC
    }
    CATCH_AND_FORWARD
}

IonizationMode MassHunterDataImpl::getIonModes() const
{
    try {return (IonizationMode) scanFileInfo_->IonModes;} CATCH_AND_FORWARD
}

MSScanType MassHunterDataImpl::getScanTypes() const
{
    try {return (MSScanType) scanFileInfo_->ScanTypes;} CATCH_AND_FORWARD
}

MSStorageMode MassHunterDataImpl::getSpectraFormat() const
{
    try {return (MSStorageMode) scanFileInfo_->SpectraFormat;} CATCH_AND_FORWARD
}

int MassHunterDataImpl::getTotalScansPresent() const
{
    try {return (int) scanFileInfo_->TotalScansPresent;} CATCH_AND_FORWARD
}

const set<Transition>& MassHunterDataImpl::getTransitions() const
{
    return transitions_;
}

const automation_vector<double>& MassHunterDataImpl::getTicTimes() const
{
    return ticTimes_;
}

const automation_vector<double>& MassHunterDataImpl::getBpcTimes() const
{
    return bpcTimes_;
}

const automation_vector<float>& MassHunterDataImpl::getTicIntensities() const
{
    return ticIntensities_;
}

const automation_vector<float>& MassHunterDataImpl::getBpcIntensities() const
{
    return bpcIntensities_;
}

ChromatogramPtr MassHunterDataImpl::getChromatogram(const Transition& transition) const
{
    try
    {
        if (!transitionToChromatogramIndexMap_.count(transition))
            throw gcnew System::Exception("[MassHunterData::getChromatogram()] No chromatogram corresponds to the transition.");

        int index = transitionToChromatogramIndexMap_.find(transition)->second;
        // until someone can figure out why storing SIM chromatograms in the constructor
        // causes the unit test to fail, only MRM uses faster way of retrieving chromatograms
        // while SIM continues to use the original, slower method.
		array<MHDAC::IBDAChromData^>^ chromatograms = chromMrm_; // transition.type == Transition::MRM ? chromMrm_ : chromSim_;
		if (transition.type != Transition::MRM)
		{
            MHDAC::IBDAChromFilter^ filter = gcnew MHDAC::BDAChromFilter();
			filter->ChromatogramType = MHDAC::ChromType::SelectedIonMonitoring;
			filter->ExtractOneChromatogramPerScanSegment = true;
			filter->DoCycleSum = false;
			chromatograms = reader_->GetChromatogram(filter);
		}

        return ChromatogramPtr(new ChromatogramImpl(chromatograms[index]));
    }
    CATCH_AND_FORWARD
}

ScanRecordPtr MassHunterDataImpl::getScanRecord(int rowNumber) const
{
    try {return ScanRecordPtr(new ScanRecordImpl(reader_->GetScanRecord(rowNumber)));} CATCH_AND_FORWARD
}

SpectrumPtr MassHunterDataImpl::getProfileSpectrumByRow(int rowNumber) const
{
    try {return SpectrumPtr(new SpectrumImpl(reader_->GetSpectrum(rowNumber, nullptr, nullptr, MHDAC::DesiredMSStorageType::ProfileElsePeak)));} CATCH_AND_FORWARD
}

SpectrumPtr MassHunterDataImpl::getPeakSpectrumByRow(int rowNumber, PeakFilterPtr peakFilter /*= PeakFilterPtr()*/) const
{
    try
    {
        // MHDAC doesn't support post-acquisition centroiding of non-TOF spectra
        MHDAC::IMsdrPeakFilter^ msdrPeakFilter_ = nullptr;
        if (scanFileInfo_->DeviceType != MHDAC::DeviceType::Quadrupole &&
            scanFileInfo_->DeviceType != MHDAC::DeviceType::TandemQuadrupole)
            msdrPeakFilter_ = msdrPeakFilter(peakFilter);
        return SpectrumPtr(new SpectrumImpl(reader_->GetSpectrum(rowNumber, msdrPeakFilter_, msdrPeakFilter_, MHDAC::DesiredMSStorageType::PeakElseProfile)));
    }
    CATCH_AND_FORWARD
}

SpectrumPtr MassHunterDataImpl::getProfileSpectrumById(int scanId) const
{
    try {return SpectrumPtr(new SpectrumImpl(reader_->GetSpectrum(bdaSpecFilterForScanId(scanId, true), nullptr)[0]));} CATCH_AND_FORWARD
}

SpectrumPtr MassHunterDataImpl::getPeakSpectrumById(int scanId, PeakFilterPtr peakFilter /*= PeakFilterPtr()*/) const
{
    try
    {
        // MHDAC doesn't support post-acquisition centroiding of non-TOF spectra
        MHDAC::IMsdrPeakFilter^ msdrPeakFilter_ = nullptr;
        if (scanFileInfo_->DeviceType != MHDAC::DeviceType::Quadrupole &&
            scanFileInfo_->DeviceType != MHDAC::DeviceType::TandemQuadrupole)
            msdrPeakFilter_ = msdrPeakFilter(peakFilter);
        return SpectrumPtr(new SpectrumImpl(reader_->GetSpectrum(bdaSpecFilterForScanId(scanId), msdrPeakFilter_)[0]));
    }
    CATCH_AND_FORWARD
}


MassRange SpectrumImpl::getMeasuredMassRange() const
{
    try
    {
        MHDAC::IRange^ massRange = specData_->MeasuredMassRange;
        MassRange mr;
        mr.start = massRange->Start;
        mr.end = massRange->End;
        return mr;
    }
    CATCH_AND_FORWARD
}

void SpectrumImpl::getPrecursorIons(vector<double>& precursorIons) const
{
    int count;
    try {return ToStdVector(specData_->GetPrecursorIon(count), precursorIons);} CATCH_AND_FORWARD
}

bool SpectrumImpl::getPrecursorCharge(int& charge) const
{
    try {return specData_->GetPrecursorCharge(charge);} CATCH_AND_FORWARD
}

bool SpectrumImpl::getPrecursorIntensity(double& precursorIntensity) const
{
    try {return specData_->GetPrecursorIntensity(precursorIntensity);} CATCH_AND_FORWARD
}

void SpectrumImpl::getXArray(automation_vector<double>& x) const
{
    try {return ToAutomationVector(specData_->XArray, x);} CATCH_AND_FORWARD
}

void SpectrumImpl::getYArray(automation_vector<float>& y) const
{
    try {return ToAutomationVector(specData_->YArray, y);} CATCH_AND_FORWARD
}


double ChromatogramImpl::getCollisionEnergy() const
{
    try {return chromData_->CollisionEnergy;} CATCH_AND_FORWARD
}

int ChromatogramImpl::getTotalDataPoints() const
{
    try {return chromData_->TotalDataPoints;} CATCH_AND_FORWARD
}

void ChromatogramImpl::getXArray(automation_vector<double>& x) const
{
    try {return ToAutomationVector(chromData_->XArray, x);} CATCH_AND_FORWARD
}

void ChromatogramImpl::getYArray(automation_vector<float>& y) const
{
    try {return ToAutomationVector(chromData_->YArray, y);} CATCH_AND_FORWARD
}


} // Agilent
} // vendor_api
} // pwiz
