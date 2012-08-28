//
// $Id: CompassData.cpp 2844 2011-07-07 22:12:31Z chambm $
//
// 
// Original author: Matt Chambers <matt.chambers .@. vanderbilt.edu>
//
// Copyright 2008 Vanderbilt University - Nashville, TN 37232
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

#pragma unmanaged
#include "pwiz/utility/misc/Std.hpp"
#include "pwiz/utility/misc/DateTime.hpp"
#include "CompassData.hpp"


#pragma managed
#include "pwiz/utility/misc/cpp_cli_utilities.hpp"
using namespace pwiz::util;


using System::String;
using System::Object;
using System::IntPtr;
using System::Runtime::InteropServices::Marshal;

typedef EDAL::MSAnalysis MS_Analysis;
typedef EDAL::MSSpectrumCollection MS_SpectrumCollection;

typedef BDal::CxT::Lc::IAnalysis LC_Analysis;
typedef BDal::CxT::Lc::ISpectrumSourceDeclaration LC_SpectrumSourceDeclaration;
typedef BDal::CxT::Lc::ITraceDeclaration LC_TraceDeclaration;


namespace pwiz {
namespace vendor_api {
namespace Bruker {


struct MSSpectrumParameterListImpl : public MSSpectrumParameterList
{
    MSSpectrumParameterListImpl(EDAL::MSSpectrumParameterCollection^ parameterCollection)
        : parameterCollection_(parameterCollection)
    {}

    virtual const_iterator begin() const {return const_iterator(*this);}
    virtual const_iterator end() const {return const_iterator();}

    gcroot<EDAL::MSSpectrumParameterCollection^> parameterCollection_;
};


struct MSSpectrumParameterIterator::Impl
{
    Impl(EDAL::MSSpectrumParameterCollection^ parameterCollection, int index)
    {
        parameterCollection_ = parameterCollection;
        index_ = index;
        increment();
    }

    Impl(const MSSpectrumParameterIterator::Impl& p)
        : parameterCollection_(p.parameterCollection_),
          parameter_(p.parameter_),
          index_(p.index_),
          cur_(p.cur_)
    {}

    void increment()
    {
        ++index_;
        if (index_ < 1 || index_ > parameterCollection_->Count) return;
        parameter_ = parameterCollection_->default[index_];
        cur_.group = ToStdString(parameter_->GroupName);
        cur_.name = ToStdString(parameter_->ParameterName);
        cur_.value = ToStdString(parameter_->ParameterValue->ToString());
    }

    gcroot<EDAL::MSSpectrumParameterCollection^> parameterCollection_;
    gcroot<EDAL::MSSpectrumParameter^> parameter_;
    int index_; // index is one-based
    MSSpectrumParameter cur_;
};

PWIZ_API_DECL MSSpectrumParameterIterator::MSSpectrumParameterIterator() {}

PWIZ_API_DECL MSSpectrumParameterIterator::MSSpectrumParameterIterator(const MSSpectrumParameterList& pl)
{
    const MSSpectrumParameterListImpl* plImpl = dynamic_cast<const MSSpectrumParameterListImpl*>(&pl);
    if (!plImpl)
        throw std::runtime_error("[MSSpectrumParameterIterator] invalid MSSpectrumParameterList subclass");
    try {impl_.reset(new Impl(plImpl->parameterCollection_, 0));} CATCH_AND_FORWARD
}

PWIZ_API_DECL MSSpectrumParameterIterator::MSSpectrumParameterIterator(const MSSpectrumParameterIterator& other)
    : impl_(other.impl_.get() ? new Impl(*other.impl_) : 0)
{}

PWIZ_API_DECL MSSpectrumParameterIterator::~MSSpectrumParameterIterator() {}

PWIZ_API_DECL void MSSpectrumParameterIterator::increment()
{
    if (!impl_.get()) return;
    impl_->increment();
}

PWIZ_API_DECL bool MSSpectrumParameterIterator::equal(const MSSpectrumParameterIterator& that) const
{
    bool gotThis = this->impl_.get() != NULL;
    bool gotThat = that.impl_.get() != NULL;

    if (gotThis && gotThat)
        return System::Object::ReferenceEquals(this->impl_->parameterCollection_, that.impl_->parameterCollection_) &&
               this->impl_->index_ == that.impl_->index_;
    else if (!gotThis && !gotThat) // end() == end()
        return true;
    else if (gotThis)
        return this->impl_->index_ >= this->impl_->parameterCollection_->Count;
    else // gotThat
        return that.impl_->index_ >= that.impl_->parameterCollection_->Count;
}

PWIZ_API_DECL const MSSpectrumParameter& MSSpectrumParameterIterator::dereference() const
{
    return impl_->cur_;
}


struct MSSpectrumImpl : public MSSpectrum
{
    MSSpectrumImpl(EDAL::IMSSpectrum^ spectrum) : spectrum_(spectrum)
    {
        try
        {
            System::Object^ massArray, ^intensityArray;
            spectrum->GetMassIntensityValues((EDAL::SpectrumTypes) SpectrumType_Line, massArray, intensityArray);
            lineDataSize_ = ((cli::array<double>^) massArray)->Length;

            spectrum->GetMassIntensityValues((EDAL::SpectrumTypes) SpectrumType_Profile, massArray, intensityArray);
            profileDataSize_ = ((cli::array<double>^) massArray)->Length;
        }
        CATCH_AND_FORWARD
    }

    virtual ~MSSpectrumImpl() {}

    virtual bool hasLineData() const {return lineDataSize_ > 0;}
    virtual bool hasProfileData() const {return profileDataSize_ > 0;}

    virtual size_t getLineDataSize() const {return lineDataSize_;}
    virtual size_t getProfileDataSize() const {return profileDataSize_;}

    virtual void getLineData(automation_vector<double>& mz, automation_vector<double>& intensities) const
    {
        if (!hasLineData())
        {
            mz.clear();
            intensities.clear();
            return;
        }

        try
        {
            // we always get a copy of the arrays because they can be modified by the client
            System::Object^ massArray, ^intensityArray;
            spectrum_->GetMassIntensityValues((EDAL::SpectrumTypes) SpectrumType_Line, massArray, intensityArray);
            ToAutomationVector((cli::array<double>^) massArray, mz);
            ToAutomationVector((cli::array<double>^) intensityArray, intensities);
        }
        CATCH_AND_FORWARD
    }

    virtual void getProfileData(automation_vector<double>& mz, automation_vector<double>& intensities) const
    {
        if (!hasProfileData())
        {
            mz.clear();
            intensities.clear();
            return;
        }

        try
        {
            // we always get a copy of the arrays because they can be modified by the client
            System::Object^ massArray, ^intensityArray;
            spectrum_->GetMassIntensityValues((EDAL::SpectrumTypes) SpectrumType_Profile, massArray, intensityArray);
            ToAutomationVector((cli::array<double>^) massArray, mz);
            ToAutomationVector((cli::array<double>^) intensityArray, intensities);
        }
        CATCH_AND_FORWARD
    }

    virtual int getMSMSStage() const {try {return (int) spectrum_->MSMSStage;} CATCH_AND_FORWARD}

    virtual double getRetentionTime() const {try {return spectrum_->RetentionTime;} CATCH_AND_FORWARD}

    virtual void getIsolationData(std::vector<double>& isolatedMZs,
                                  std::vector<IsolationMode>& isolationModes) const
    {
        try
        {
            System::Object^ mzArrayObject;
            System::Array^ modeArray;
            spectrum_->GetIsolationData(mzArrayObject, modeArray);
            cli::array<double,2>^ mzArray = (cli::array<double,2>^) mzArrayObject;
            isolatedMZs.resize(mzArray->Length);
            for (int i=0; i < mzArray->Length; ++i)
                isolatedMZs[i] = mzArray[i,0];
            ToStdVector((cli::array<EDAL::IsolationModes>^) modeArray, isolationModes);
        }
        CATCH_AND_FORWARD
    }

    virtual void getFragmentationData(std::vector<double>& fragmentedMZs,
                                      std::vector<FragmentationMode>& fragmentationModes) const
    {
        try
        {
            System::Object^ mzArrayObject;
            System::Array^ modeArray;
            spectrum_->GetFragmentationData(mzArrayObject, modeArray);
            cli::array<double,2>^ mzArray = (cli::array<double,2>^) mzArrayObject;
            fragmentedMZs.resize(mzArray->Length);
            for (int i=0; i < mzArray->Length; ++i)
                fragmentedMZs[i] = mzArray[i,0];
            ToStdVector((cli::array<EDAL::FragmentationModes>^) modeArray, fragmentationModes);
        }
        CATCH_AND_FORWARD
    }

    virtual IonPolarity getPolarity() const {try {return (IonPolarity) spectrum_->Polarity;} CATCH_AND_FORWARD}

    virtual MSSpectrumParameterListPtr parameters() const
    {
        try
        {
            MSSpectrumParameterListPtr result(new MSSpectrumParameterListImpl(spectrum_->MSSpectrumParameterCollection));
            return result;
        }
        CATCH_AND_FORWARD
    }

    private:
    gcroot<EDAL::IMSSpectrum^> spectrum_;
    size_t lineDataSize_, profileDataSize_;
};


struct LCSpectrumSourceImpl : public LCSpectrumSource
{
    LCSpectrumSourceImpl(BDal::CxT::Lc::ISpectrumSourceDeclaration^ ssd) : spectrumSourceDeclaration_(ssd) {}
    virtual ~LCSpectrumSourceImpl() {}

    virtual int getCollectionId() const {try {return spectrumSourceDeclaration_->SpectrumCollectionId;} CATCH_AND_FORWARD}
    virtual std::string getInstrument() const {try {return ToStdString(spectrumSourceDeclaration_->Instrument);} CATCH_AND_FORWARD}
    virtual std::string getInstrumentId() const {try {return ToStdString(spectrumSourceDeclaration_->InstrumentId);} CATCH_AND_FORWARD}
    virtual double getTimeOffset() const {try {return spectrumSourceDeclaration_->TimeOffset;} CATCH_AND_FORWARD}
    virtual void getXAxis(vector<double>& xAxis) const {try {ToStdVector((cli::array<double>^) spectrumSourceDeclaration_->XAxis, xAxis);} CATCH_AND_FORWARD}
    virtual LCUnit getXAxisUnit() const {try {return (LCUnit) spectrumSourceDeclaration_->XAxisUnit;} CATCH_AND_FORWARD}

    gcroot<BDal::CxT::Lc::ISpectrumSourceDeclaration^> spectrumSourceDeclaration_;
};

struct LCSpectrumImpl : public LCSpectrum
{
    LCSpectrumImpl(BDal::CxT::Lc::ISpectrum^ spectrum) : spectrum_(spectrum) {}
    virtual ~LCSpectrumImpl() {}

    virtual void getData(vector<double>& intensities) const {try {ToStdVector((cli::array<double>^) spectrum_->Intensity, intensities);} CATCH_AND_FORWARD}
    virtual double getTime() const {try {return spectrum_->Time;} CATCH_AND_FORWARD}

    gcroot<BDal::CxT::Lc::ISpectrum^> spectrum_;
};


struct CompassDataImpl : public CompassData
{
    CompassDataImpl(const string& rawpath, msdata::detail::Reader_Bruker_Format format_)
    {
        using namespace msdata::detail;
        try
        {
            if (format_ == Reader_Bruker_Format_Unknown)
                format_ = format(rawpath);
            if (format_ == Reader_Bruker_Format_Unknown)
                throw runtime_error("[CompassData::ctor] unknown file format");

            if (format_ != Reader_Bruker_Format_U2)
            {
                msAnalysis_ = gcnew EDAL::MSAnalysisClass();
                msAnalysis_->Open(ToSystemString(rawpath));
                msSpectrumCollection_ = msAnalysis_->MSSpectrumCollection;
                hasMSData_ = msSpectrumCollection_->Count > 0;
            }
            else
                hasMSData_ = false;
            
            if (format_ == Reader_Bruker_Format_U2 ||
                format_ == Reader_Bruker_Format_BAF_and_U2)
            {
                BDal::CxT::Lc::AnalysisFactory^ factory = gcnew BDal::CxT::Lc::AnalysisFactory();
                lcAnalysis_ = factory->Open(ToSystemString(rawpath));
                lcSources_ = lcAnalysis_->GetSpectrumSourceDeclarations();
                hasLCData_ = lcSources_->Length > 0;
            }
            else
                hasLCData_ = false;
        }
        CATCH_AND_FORWARD
    }

    virtual ~CompassDataImpl()
    {
        if ((MS_Analysis^) msAnalysis_ != nullptr) delete msAnalysis_;
        if ((LC_Analysis^) lcAnalysis_ != nullptr) lcAnalysis_->Close();
    }

    virtual bool hasMSData() const {return hasMSData_;}
    virtual bool hasLCData() const {return hasLCData_;}

    virtual size_t getMSSpectrumCount() const
    {
        if (!hasMSData_) return 0;
        try {return msSpectrumCollection_->Count;} CATCH_AND_FORWARD
    }

    virtual MSSpectrumPtr getMSSpectrum(int scan) const
    {
        if (!hasMSData_) throw runtime_error("[CompassData::getMSSpectrum] No MS data.");
        if (scan < 1 || scan > (int) getMSSpectrumCount())
            throw out_of_range("[CompassData::getMSSpectrum] Scan number " + lexical_cast<string>(scan) + " is out of range.");

        try {return MSSpectrumPtr(new MSSpectrumImpl(msSpectrumCollection_->default[scan]));} CATCH_AND_FORWARD
    }

    virtual size_t getLCSourceCount() const
    {
        if (!hasLCData_) return 0;
        try {return (size_t) lcSources_->Length;} CATCH_AND_FORWARD
    }

    virtual size_t getLCSpectrumCount(int source) const
    {
        if (!hasLCData_) return 0;
        if (source < 0 || source >= lcSources_->Length)
            throw out_of_range("[CompassData::getLCSpectrumCount] Source index out of range.");
        try {return lcAnalysis_->GetSpectrumCollection(lcSources_[source]->SpectrumCollectionId)->NumberOfSpectra;} CATCH_AND_FORWARD
    }

    virtual LCSpectrumSourcePtr getLCSource(int source) const
    {
        if (!hasLCData_) throw runtime_error("[CompassData::getLCSource] No LC data.");
        if (source < 0 || source >= lcSources_->Length)
            throw out_of_range("[CompassData::getLCSpectrum] Source index out of range.");
        try {return LCSpectrumSourcePtr(new LCSpectrumSourceImpl(lcSources_[source]));} CATCH_AND_FORWARD
    }

    virtual LCSpectrumPtr getLCSpectrum(int source, int scan) const
    {
        if (!hasLCData_) throw runtime_error("[CompassData::getLCSpectrum] No LC data.");
        if (source < 0 || source >= lcSources_->Length)
            throw out_of_range("[CompassData::getLCSpectrum] Source index out of range.");
        try
        {
            BDal::CxT::Lc::ISpectrumCollection^ sc = lcAnalysis_->GetSpectrumCollection(lcSources_[source]->SpectrumCollectionId);
            if (scan < 0 || scan >= sc->NumberOfSpectra)
                throw out_of_range("[CompassData::getLCSpectrum] Scan number " + lexical_cast<string>(scan) + " is out of range.");
            return LCSpectrumPtr(new LCSpectrumImpl(sc->default[scan]));
        }
        CATCH_AND_FORWARD
    }

    virtual std::string getOperatorName() const
    {
        if (!hasMSData_) return "";
        try {return ToStdString(msAnalysis_->OperatorName);} CATCH_AND_FORWARD
    }

    virtual std::string getAnalysisName() const
    {
        if (!hasMSData_) return "";
        try {return ToStdString(msAnalysis_->AnalysisName);} CATCH_AND_FORWARD
    }

    virtual boost::local_time::local_date_time getAnalysisDateTime() const
    {
        using bpt::ptime;
        using blt::local_date_time;
        if (!hasMSData_) return local_date_time(bdt::not_a_date_time);

        try
        {
            ptime pt(bdt::time_from_OADATE<ptime>(msAnalysis_->AnalysisDateTime.ToOADate()));
            return local_date_time(pt, blt::time_zone_ptr());
        }
        CATCH_AND_FORWARD
    }

    virtual std::string getSampleName() const
    {
        if (!hasMSData_) return "";
        try {return ToStdString(msAnalysis_->SampleName);} CATCH_AND_FORWARD
    }

    virtual std::string getMethodName() const
    {
        if (!hasMSData_) return "";
        try {return ToStdString(msAnalysis_->MethodName);} CATCH_AND_FORWARD
    }

    virtual InstrumentFamily getInstrumentFamily() const
    {
        if (!hasMSData_) return InstrumentFamily_Unknown;
        try {return (InstrumentFamily) msAnalysis_->InstrumentFamily;} CATCH_AND_FORWARD
    }

    virtual std::string getInstrumentDescription() const
    {
        if (!hasMSData_) return "";
        try {return ToStdString(msAnalysis_->InstrumentDescription);} CATCH_AND_FORWARD
    }

    private:
    bool hasMSData_;
    gcroot<MS_Analysis^> msAnalysis_;
    gcroot<MS_SpectrumCollection^> msSpectrumCollection_;

    bool hasLCData_;
    gcroot<LC_Analysis^> lcAnalysis_;
    gcroot<cli::array<LC_SpectrumSourceDeclaration^>^> lcSources_;
};


PWIZ_API_DECL CompassDataPtr CompassData::create(const string& rawpath,
                                                 msdata::detail::Reader_Bruker_Format format)
{
    return CompassDataPtr(new CompassDataImpl(rawpath, format));
}

} // namespace Bruker
} // namespace vendor_api
} // namespace pwiz
