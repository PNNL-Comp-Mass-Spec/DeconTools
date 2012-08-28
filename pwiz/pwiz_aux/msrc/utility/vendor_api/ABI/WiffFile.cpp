//
// $Id: WiffFile.cpp 3589 2012-05-02 16:24:09Z chambm $
//
//
// Original author: Matt Chambers <matt.chambers .@. vanderbilt.edu>
//
// Copyright 2009 Vanderbilt University - Nashville, TN 37232
//
// Licensed under Creative Commons 3.0 United States License, which requires:
//  - Attribution
//  - Noncommercial
//  - No Derivative Works
//
// http://creativecommons.org/licenses/by-nc-nd/3.0/us/
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//


#define PWIZ_SOURCE

#ifndef PWIZ_READER_ABI
#error compiler is not MSVC or DLL not available
#else // PWIZ_READER_ABI

#pragma unmanaged
#include "WiffFile.hpp"
#include "LicenseKey.h"
#include "pwiz/utility/misc/DateTime.hpp"
#include "pwiz/utility/misc/String.hpp"
#include "pwiz/utility/misc/Container.hpp"

#pragma managed
#include "pwiz/utility/misc/cpp_cli_utilities.hpp"
using namespace pwiz::util;
using namespace System;
using namespace Clearcore2::Data;
using namespace Clearcore2::Data::AnalystDataProvider;
using namespace Clearcore2::Data::DataAccess::SampleData;

#if __CLR_VER > 40000000 // .NET 4
using namespace Clearcore2::RawXYProcessing;
#endif

namespace pwiz {
namespace vendor_api {
namespace ABI {


class WiffFileImpl : public WiffFile
{
    public:
    WiffFileImpl(const std::string& wiffpath);
    ~WiffFileImpl();

    gcroot<AnalystWiffDataProvider^> provider;
    gcroot<Batch^> batch;
    mutable gcroot<Sample^> sample;
    mutable gcroot<MassSpectrometerSample^> msSample;

    virtual int getSampleCount() const;
    virtual int getPeriodCount(int sample) const;
    virtual int getExperimentCount(int sample, int period) const;
    virtual int getCycleCount(int sample, int period, int experiment) const;

    virtual const vector<string>& getSampleNames() const;

    virtual InstrumentModel getInstrumentModel() const;
    virtual InstrumentType getInstrumentType() const;
    virtual IonSourceType getIonSourceType() const;
    virtual blt::local_date_time getSampleAcquisitionTime() const;

    virtual ExperimentPtr getExperiment(int sample, int period, int experiment) const;
    virtual SpectrumPtr getSpectrum(int sample, int period, int experiment, int cycle) const;
    virtual SpectrumPtr getSpectrum(ExperimentPtr experiment, int cycle) const;

    void setSample(int sample) const;
    void setPeriod(int sample, int period) const;
    void setExperiment(int sample, int period, int experiment) const;
    void setCycle(int sample, int period, int experiment, int cycle) const;

    mutable int currentSample, currentPeriod, currentExperiment, currentCycle;

    private:
    // on first access, sample names are made unique (giving duplicates a count suffix) and cached
    mutable vector<string> sampleNames;
};

typedef boost::shared_ptr<WiffFileImpl> WiffFileImplPtr;


struct ExperimentImpl : public Experiment
{
    ExperimentImpl(const WiffFileImpl* wifffile, int sample, int period, int experiment);

    virtual int getSampleNumber() const {return sample;}
    virtual int getPeriodNumber() const {return period;}
    virtual int getExperimentNumber() const {return experiment;}

    virtual size_t getSRMSize() const;
    virtual void getSRM(size_t index, Target& target) const;
    virtual void getSIC(size_t index, std::vector<double>& times, std::vector<double>& intensities) const;
    virtual void getSIC(size_t index, std::vector<double>& times, std::vector<double>& intensities,
                        double& basePeakX, double& basePeakY) const;

    virtual void getAcquisitionMassRange(double& startMz, double& stopMz) const;
    virtual ScanType getScanType() const;
    virtual ExperimentType getExperimentType() const;
    virtual Polarity getPolarity() const;

    virtual void getTIC(std::vector<double>& times, std::vector<double>& intensities) const;
    virtual void getBPC(std::vector<double>& times, std::vector<double>& intensities) const;

    const WiffFileImpl* wifffile_;
    gcroot<MSExperiment^> msExperiment;
    int sample, period, experiment;

    size_t transitionCount;
    typedef map<pair<double, double>, pair<int, int> > TransitionParametersMap;
    TransitionParametersMap transitionParametersMap;

    const vector<double>& cycleTimes() const {initialize(); return cycleTimes_;}
    const vector<double>& cycleIntensities() const {initialize(); return cycleIntensities_;}
    const vector<double>& basePeakMZs() const {initialize(); return basePeakMZs_;}
    const vector<double>& basePeakIntensities() const {initialize(); return basePeakIntensities_;}

    private:
    void initialize() const;
    mutable bool initialized_;
    mutable vector<double> cycleTimes_;
    mutable vector<double> cycleIntensities_;
    mutable vector<double> basePeakMZs_;
    mutable vector<double> basePeakIntensities_;
};

typedef boost::shared_ptr<ExperimentImpl> ExperimentImplPtr;


struct SpectrumImpl : public Spectrum
{
    SpectrumImpl(ExperimentImplPtr experiment, int cycle);

    virtual int getSampleNumber() const {return experiment->sample;}
    virtual int getPeriodNumber() const {return experiment->period;}
    virtual int getExperimentNumber() const {return experiment->experiment;}
    virtual int getCycleNumber() const {return cycle;}

    virtual int getMSLevel() const;

    virtual bool getHasIsolationInfo() const;
    virtual void getIsolationInfo(double& centerMz, double& lowerLimit, double& upperLimit) const;

    virtual bool getHasPrecursorInfo() const;
    virtual void getPrecursorInfo(double& selectedMz, double& intensity, int& charge) const;

    virtual double getStartTime() const;

    virtual bool getDataIsContinuous() const {return pointsAreContinuous;}
    size_t getDataSize(bool doCentroid) const;
    virtual void getData(bool doCentroid, std::vector<double>& mz, std::vector<double>& intensities) const;

    virtual double getSumY() const {return sumY;}
    virtual double getBasePeakX() const {return bpX;}
    virtual double getBasePeakY() const {return bpY;}
    virtual double getMinX() const {return minX;}
    virtual double getMaxX() const {return maxX;}

    ExperimentImplPtr experiment;
    gcroot<MassSpectrumInfo^> spectrumInfo;
    mutable gcroot<MassSpectrum^> spectrum;

#if __CLR_VER > 40000000 // .NET 4
    mutable gcroot<cli::array<PeakClass^>^> peakList;
#endif

    int cycle;

    // data points
    double sumY, bpX, bpY, minX, maxX;
    vector<double> x, y;
    bool pointsAreContinuous;

    // precursor info
    double selectedMz, intensity;
    int charge;
};

typedef boost::shared_ptr<SpectrumImpl> SpectrumImplPtr;


WiffFileImpl::WiffFileImpl(const string& wiffpath)
: currentSample(-1), currentPeriod(-1), currentExperiment(-1), currentCycle(-1)
{
    try
    {
#if __CLR_VER > 40000000 // .NET 4
        Clearcore2::Licensing::LicenseKeys::Keys = gcnew array<String^> {ABI_BETA_LICENSE_KEY};
#else
        Licenser::LicenseKey = ABI_BETA_LICENSE_KEY;
#endif

        provider = gcnew AnalystWiffDataProvider();
        batch = AnalystDataProviderFactory::CreateBatch(ToSystemString(wiffpath), provider);
        // This caused WIFF files where the first sample had been interrupted to
        // throw before they could be successfully constructed, which made investigators
        // unhappy when they were seeking access to later, successfully acquired samples.
        // setSample(1);
    }
    CATCH_AND_FORWARD
}

WiffFileImpl::~WiffFileImpl()
{
    provider->Close();
}


int WiffFileImpl::getSampleCount() const
{
    try {return getSampleNames().size();} CATCH_AND_FORWARD
}

int WiffFileImpl::getPeriodCount(int sample) const
{
    try
    {
        setSample(sample);
        return 1;
    }
    CATCH_AND_FORWARD
}

int WiffFileImpl::getExperimentCount(int sample, int period) const
{
    try
    {
        setPeriod(sample, period);
        return msSample->ExperimentCount;
    }
    CATCH_AND_FORWARD
}

int WiffFileImpl::getCycleCount(int sample, int period, int experiment) const
{
    try
    {
        setExperiment(sample, period, experiment);
        return msSample->GetMSExperiment(experiment-1)->Details->NumberOfScans;
    }
    CATCH_AND_FORWARD
}

const vector<string>& WiffFileImpl::getSampleNames() const
{
    try
    {
        if (sampleNames.size() == 0)
        {
            // make duplicate sample names unique by appending the duplicate count
            // e.g. foo, bar, foo (2), foobar, bar (2), foo (3)
            map<string, int> duplicateCountMap;
            array<System::String^>^ sampleNamesManaged = batch->GetSampleNames();
            sampleNames.resize(sampleNamesManaged->Length, "");
            for (int i=0; i < sampleNamesManaged->Length; ++i)
                sampleNames[i] = ToStdString(sampleNamesManaged[i]);

            // inexplicably, some files have more samples than sample names;
            // pad the name vector with duplicates of the last sample name;
            // if there are no names, use empty string
            //while (sampleNames.size() < (size_t) batch->SampleCount)
            //    sampleNames.push_back(sampleNames.back());

            for (size_t i=0; i < sampleNames.size(); ++i)
            {
                int duplicateCount = duplicateCountMap[sampleNames[i]]++; // increment after getting current count
                if (duplicateCount)
                    sampleNames[i] += " (" + lexical_cast<string>(duplicateCount+1) + ")";
            }

        }
        return sampleNames;
    }
    CATCH_AND_FORWARD
}

InstrumentModel WiffFileImpl::getInstrumentModel() const
{
    try {return (InstrumentModel) 0;} CATCH_AND_FORWARD
}

InstrumentType WiffFileImpl::getInstrumentType() const
{
    try {return (InstrumentType) 0;} CATCH_AND_FORWARD
}

IonSourceType WiffFileImpl::getIonSourceType() const
{
    try {return (IonSourceType) 0;} CATCH_AND_FORWARD
}

blt::local_date_time WiffFileImpl::getSampleAcquisitionTime() const
{
    try
    {
        bpt::ptime pt(bdt::time_from_OADATE<bpt::ptime>(sample->Details->AcquisitionDateTime.ToOADate()));
        return blt::local_date_time(pt, blt::time_zone_ptr()); // keep time as UTC
    }
    CATCH_AND_FORWARD
}


ExperimentPtr WiffFileImpl::getExperiment(int sample, int period, int experiment) const
{
    setExperiment(sample, period, experiment);
    ExperimentImplPtr msExperiment(new ExperimentImpl(this, sample, period, experiment));
    return msExperiment;
}


ExperimentImpl::ExperimentImpl(const WiffFileImpl* wifffile, int sample, int period, int experiment)
: wifffile_(wifffile), sample(sample), period(period), experiment(experiment), transitionCount(0), initialized_(false)
{
    try
    {
        wifffile_->setExperiment(sample, period, experiment);
        msExperiment = wifffile_->msSample->GetMSExperiment(experiment-1);

        if ((ExperimentType) msExperiment->Details->ExperimentType == MRM)
            transitionCount = msExperiment->Details->MassRangeInfo->Length;

        /*for (int i=0; i < msExperiment->MRMTransitions->Count; ++i)
        {
            MRMTransition^ transition = msExperiment->MRMTransitions[i];
            pair<int, int>& e = transitionParametersMap[make_pair(transition->Q1Mass->MassAsDouble, transition->Q3Mass->MassAsDouble)];
            e.first = i;
            e.second = -1;
        }

        MRMTransitionsForAcquisitionCollection^ transitions = wifffile_->reader->Provider->GetMRMTransitionsForAcquisition();
        for (int i=0; i < transitions->Count; ++i)
        {
            MRMTransition^ transition = transitions[i]->Transition;
            pair<int, int>& e = transitionParametersMap[make_pair(transition->Q1Mass->MassAsDouble, transition->Q3Mass->MassAsDouble)];
            if (e.second != -1) // this Q1/Q3 wasn't added by the MRMTransitions loop
                e.first = -1;
            e.second = i;
        }*/
    }
    CATCH_AND_FORWARD
}

void ExperimentImpl::initialize() const
{
    if (initialized_)
        return;

    TotalIonChromatogram^ tic = msExperiment->GetTotalIonChromatogram();
    ToStdVector(tic->GetActualXValues(), cycleTimes_);
    ToStdVector(tic->GetActualYValues(), cycleIntensities_);

    BasePeakChromatogramSettings^ bpcs = gcnew BasePeakChromatogramSettings(0, 0, gcnew array<double>(0), gcnew array<double>(0));
    BasePeakChromatogram^ bpc = msExperiment->GetBasePeakChromatogram(bpcs);
    BasePeakChromatogramInfo^ bpci = bpc->Info;
    ToStdVector(bpc->GetActualYValues(), basePeakIntensities_);

    basePeakMZs_.resize(cycleTimes_.size());
    for (size_t i=0; i < cycleTimes_.size(); ++i)
        basePeakMZs_[i] = bpci->GetBasePeakMass(i);

    initialized_ = true;
}

size_t ExperimentImpl::getSRMSize() const
{
    try {return transitionCount;} CATCH_AND_FORWARD
}

void ExperimentImpl::getSRM(size_t index, Target& target) const
{
    try
    {
        if (index >= transitionCount)
            throw std::out_of_range("[Experiment::getSRM()] index out of range");

        MRMMassRange^ transition = (MRMMassRange^) msExperiment->Details->MassRangeInfo[index];
        //const pair<int, int>& e = transitionParametersMap.find(make_pair(transition->Q1Mass->MassAsDouble, transition->Q3Mass->MassAsDouble))->second;

        target.type = TargetType_SRM;
        target.Q1 = transition->Q1Mass;
        target.Q3 = transition->Q3Mass;
        target.dwellTime = transition->DwellTime;
        // TODO: store RTWindow?

        /*if (e.second > -1)
        {
            MRMTransitionsForAcquisitionCollection^ transitions = wifffile_->reader->Provider->GetMRMTransitionsForAcquisition();
            CompoundDependentParametersDictionary^ parameters = transitions[e.second]->Parameters;
            target.collisionEnergy = (double) parameters["CE"];
            target.declusteringPotential = (double) parameters["DP"];
        }
        else*/
        {
            // TODO: use NaN to indicate these values should be considered missing?
            target.collisionEnergy = 0;
            target.declusteringPotential = 0;
        }
    }
    CATCH_AND_FORWARD
}

void ExperimentImpl::getSIC(size_t index, std::vector<double>& times, std::vector<double>& intensities) const
{
    double x, y;
    getSIC(index, times, intensities, x, y);
}

void ExperimentImpl::getSIC(size_t index, std::vector<double>& times, std::vector<double>& intensities,
                            double& basePeakX, double& basePeakY) const
{
    try
    {
        if (index >= transitionCount)
            throw std::out_of_range("[Experiment::getSIC()] index out of range");

        ExtractedIonChromatogramSettings^ option = gcnew ExtractedIonChromatogramSettings(index);
        ExtractedIonChromatogram^ xic = msExperiment->GetExtractedIonChromatogram(option);

        ToStdVector(xic->GetActualXValues(), times);
        ToStdVector(xic->GetActualYValues(), intensities);

        basePeakY = xic->MaximumYValue;
        basePeakX = 0;
        for (size_t i=0; i < intensities.size(); ++i)
            if (intensities[i] == basePeakY)
            {
                basePeakX = times[i];
                break;
            }
    }
    CATCH_AND_FORWARD
}

void ExperimentImpl::getAcquisitionMassRange(double& startMz, double& stopMz) const
{
    try
    {
        if ((ExperimentType) msExperiment->Details->ExperimentType != MRM)
        {
            startMz = msExperiment->Details->StartMass;
            stopMz = msExperiment->Details->EndMass;
        }
        else
            startMz = stopMz = 0;
    }
    CATCH_AND_FORWARD
}

ScanType ExperimentImpl::getScanType() const
{
    try {return (ScanType) msExperiment->Details->SpectrumType;} CATCH_AND_FORWARD
}

ExperimentType ExperimentImpl::getExperimentType() const
{
    try {return (ExperimentType) msExperiment->Details->ExperimentType;} CATCH_AND_FORWARD
}

Polarity ExperimentImpl::getPolarity() const
{
    try {return (Polarity) msExperiment->Details->Polarity;} CATCH_AND_FORWARD
}

void ExperimentImpl::getTIC(std::vector<double>& times, std::vector<double>& intensities) const
{
    try
    {
        times = cycleTimes();
        intensities = cycleIntensities();
    }
    CATCH_AND_FORWARD
}

void ExperimentImpl::getBPC(std::vector<double>& times, std::vector<double>& intensities) const
{
    try
    {
        times = cycleTimes();
        intensities = basePeakIntensities();
    }
    CATCH_AND_FORWARD
}

SpectrumImpl::SpectrumImpl(ExperimentImplPtr experiment, int cycle)
: experiment(experiment), cycle(cycle), selectedMz(0)
{
    try
    {
        spectrumInfo = experiment->msExperiment->GetMassSpectrumInfo(cycle-1);

        pointsAreContinuous = !spectrumInfo->CentroidMode;

        sumY = experiment->cycleIntensities()[cycle-1];
        //minX = experiment->; // TODO Mass range?
        //maxX = spectrum->MaximumXValue;
        bpY = experiment->basePeakIntensities()[cycle-1];
        bpX = bpY > 0 ? experiment->basePeakMZs()[cycle-1] : 0;

        if (spectrumInfo->IsProductSpectrum)
        {
            selectedMz = spectrumInfo->ParentMZ;
            intensity = 0;
            charge = spectrumInfo->ParentChargeState;
        }
    }
    CATCH_AND_FORWARD
}

int SpectrumImpl::getMSLevel() const
{
    try {return spectrumInfo->MSLevel == 0 ? 1 : spectrumInfo->MSLevel;} CATCH_AND_FORWARD
}

bool SpectrumImpl::getHasIsolationInfo() const
{
    return false;
}

void SpectrumImpl::getIsolationInfo(double& centerMz, double& lowerLimit, double& upperLimit) const
{
}

bool SpectrumImpl::getHasPrecursorInfo() const
{
    return selectedMz != 0;
}

void SpectrumImpl::getPrecursorInfo(double& selectedMz, double& intensity, int& charge) const
{
    selectedMz = this->selectedMz;
    intensity = this->intensity;
    charge = this->charge;
}

double SpectrumImpl::getStartTime() const
{
    return spectrumInfo->StartRT;
}

size_t SpectrumImpl::getDataSize(bool doCentroid) const
{
    try
    {
#if __CLR_VER > 40000000 // .NET 4
        if (doCentroid)
        {
            if ((cli::array<PeakClass^>^) peakList == nullptr) peakList = experiment->msExperiment->GetPeakArray(cycle-1);
            return (size_t) peakList->Length;
        }
        else
#endif
        {
            if ((MassSpectrum^) spectrum == nullptr)
            {
                spectrum = experiment->msExperiment->GetMassSpectrum(cycle-1);
                // TODO: enable this when it's more efficient:
                // experiment->msExperiment->AddZeros((MassSpectrum^) spectrum);
            }
            return (size_t) spectrum->NumDataPoints;
        }
    }
    CATCH_AND_FORWARD
}

void SpectrumImpl::getData(bool doCentroid, std::vector<double>& mz, std::vector<double>& intensities) const
{
    try
    {
#if __CLR_VER > 40000000 // .NET 4
        if (doCentroid && pointsAreContinuous)
        {
            if ((cli::array<PeakClass^>^) peakList == nullptr) peakList = experiment->msExperiment->GetPeakArray(cycle-1);
            size_t numPoints = peakList->Length;
            mz.resize(numPoints);
            intensities.resize(numPoints);
            for (size_t i=0; i < numPoints; ++i)
            {
                PeakClass^ peak = peakList[(int)i];
                mz[i] = peak->apexX;
                intensities[i] = peak->apexY;
            }
        }
        else
#endif
        {
            if ((MassSpectrum^) spectrum == nullptr)
            {
                spectrum = experiment->msExperiment->GetMassSpectrum(cycle-1);
                // TODO: enable this when it's more efficient:
                //experiment->msExperiment->AddZeros((MassSpectrum^) spectrum);
            }
            ToStdVector(spectrum->GetActualXValues(), mz);
            ToStdVector(spectrum->GetActualYValues(), intensities);
        }
    }
    CATCH_AND_FORWARD
}


SpectrumPtr WiffFileImpl::getSpectrum(int sample, int period, int experiment, int cycle) const
{
    try
    {
        ExperimentPtr msExperiment = getExperiment(sample, period, experiment);
        return getSpectrum(msExperiment, cycle);
    }
    CATCH_AND_FORWARD
}

SpectrumPtr WiffFileImpl::getSpectrum(ExperimentPtr experiment, int cycle) const
{
    SpectrumImplPtr spectrum(new SpectrumImpl(boost::static_pointer_cast<ExperimentImpl>(experiment), cycle));
    return spectrum;
}


void WiffFileImpl::setSample(int sample) const
{
    try
    {
        if (sample != currentSample)
        {
            this->sample = batch->GetSample(sample-1);
            msSample = this->sample->MassSpectrometerSample;

            currentSample = sample;
            currentPeriod = currentExperiment = currentCycle = -1;
        }
    }
    CATCH_AND_FORWARD
}

void WiffFileImpl::setPeriod(int sample, int period) const
{
    try
    {
        setSample(sample);

        if (period != currentPeriod)
        {
            //reader->PeriodNum = period;
            currentPeriod = period;
            currentExperiment = currentCycle = -1;
        }
    }
    CATCH_AND_FORWARD
}

void WiffFileImpl::setExperiment(int sample, int period, int experiment) const
{
    try
    {
        setPeriod(sample, period);

        if (experiment != currentExperiment)
        {
            //reader->ExperimentNum = experiment;
            currentExperiment = experiment;
            currentCycle = -1;
        }
    }
    CATCH_AND_FORWARD
}

void WiffFileImpl::setCycle(int sample, int period, int experiment, int cycle) const
{
    try
    {
        setExperiment(sample, period, experiment);

        if (cycle != currentCycle)
        {
            //reader->SetCycles(cycle);
            currentCycle = cycle;
        }
    }
    CATCH_AND_FORWARD
}


PWIZ_API_DECL
WiffFilePtr WiffFile::create(const string& wiffpath)
{
    WiffFileImplPtr wifffile(new WiffFileImpl(wiffpath));
    return boost::static_pointer_cast<WiffFile>(wifffile);
}


} // ABI
} // vendor_api
} // pwiz

#endif