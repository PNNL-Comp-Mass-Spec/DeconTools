//
// $Id: Reader_Bruker.cpp 3808 2012-07-24 20:31:10Z donmarsh $
//
//
// Original author: Matt Chambers <matt.chambers .@. vanderbilt.edu>
//
// Copyright 2009 Vanderbilt University - Nashville, TN 37232
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

// CompassXtractMS DLL usage is msvc only - mingw doesn't provide com support
#if (!defined(_MSC_VER) && defined(PWIZ_READER_BRUKER))
#undef PWIZ_READER_BRUKER
#endif


#include "Reader_Bruker.hpp"
#include "Reader_Bruker_Detail.hpp"
#include "pwiz/utility/misc/String.hpp"
#include "pwiz/utility/misc/DateTime.hpp"
#include "pwiz/data/msdata/Version.hpp"
#include <stdexcept>


// A Bruker Analysis source (representing a "run") is actually a directory
// It contains several files related to a single acquisition, e.g.:
// fid, acqu, acqus, Analysis.FAMethod, AnalysisParameter.xml, sptype

PWIZ_API_DECL
std::string pwiz::msdata::Reader_Bruker::identify(const std::string& filename,
                                                  const std::string& head) const
{
    switch (detail::format(filename))
    {
        case pwiz::msdata::detail::Reader_Bruker_Format_FID: return "Bruker FID";
        case pwiz::msdata::detail::Reader_Bruker_Format_YEP: return "Bruker YEP";
        case pwiz::msdata::detail::Reader_Bruker_Format_BAF: return "Bruker BAF";
        case pwiz::msdata::detail::Reader_Bruker_Format_U2: return "Bruker U2";
        case pwiz::msdata::detail::Reader_Bruker_Format_BAF_and_U2: return "Bruker BAF/U2";

        case pwiz::msdata::detail::Reader_Bruker_Format_Unknown:
        default:
            return "";
    }
}


#ifdef PWIZ_READER_BRUKER
#include "pwiz/utility/misc/SHA1Calculator.hpp"
#include "SpectrumList_Bruker.hpp"
#include "ChromatogramList_Bruker.hpp"
#include "pwiz/utility/misc/Std.hpp"


namespace pwiz {
namespace msdata {


using namespace pwiz::util;
using namespace pwiz::msdata::detail;
using namespace pwiz::vendor_api::Bruker;


//
// Reader_Bruker
//

namespace {

void fillInMetadata(const bfs::path& rootpath, MSData& msd, Reader_Bruker_Format format, CompassDataPtr compassDataPtr)
{
    msd.cvs = defaultCVList();

    msd.id = bfs::basename(rootpath);

    SoftwarePtr software(new Software);
    software->id = "CompassXtract";
    software->set(MS_CompassXtract);
    software->version = "1.0";
    msd.softwarePtrs.push_back(software);

    SoftwarePtr softwarePwiz(new Software);
    softwarePwiz->id = "pwiz_Reader_Bruker";
    softwarePwiz->set(MS_pwiz);
    softwarePwiz->version = pwiz::msdata::Version::str();
    msd.softwarePtrs.push_back(softwarePwiz);

    DataProcessingPtr dpPwiz(new DataProcessing);
    dpPwiz->id = "pwiz_Reader_Bruker_conversion";
    dpPwiz->processingMethods.push_back(ProcessingMethod());
    dpPwiz->processingMethods.back().softwarePtr = softwarePwiz;
    dpPwiz->processingMethods.back().cvParams.push_back(MS_Conversion_to_mzML);
    msd.dataProcessingPtrs.push_back(dpPwiz);

    // give ownership of dpPwiz to the SpectrumList (and ChromatogramList)
    SpectrumList_Bruker* sl = dynamic_cast<SpectrumList_Bruker*>(msd.run.spectrumListPtr.get());
    ChromatogramList_Bruker* cl = dynamic_cast<ChromatogramList_Bruker*>(msd.run.chromatogramListPtr.get());
    if (sl) sl->setDataProcessingPtr(dpPwiz);
    if (cl) cl->setDataProcessingPtr(dpPwiz);

    bool hasMS1 = false;
    bool hasMSn = false;
    for (size_t scan=1, end=compassDataPtr->getMSSpectrumCount();
         scan <= end && (!hasMS1 || !hasMSn);
         ++scan)
    {
        int msLevel = sl->getMSSpectrumPtr(scan)->getMSMSStage();
        if (!hasMS1 && msLevel == 1)
        {
            hasMS1 = true;
            msd.fileDescription.fileContent.set(MS_MS1_spectrum);
        }
        else if (!hasMSn && msLevel > 1)
        {
            hasMSn = true;
            msd.fileDescription.fileContent.set(MS_MSn_spectrum);
        }
    }

    // TODO: read instrument "family" from (first) source
    //initializeInstrumentConfigurationPtrs(msd, rawfile, softwareXcalibur);
    msd.instrumentConfigurationPtrs.push_back(InstrumentConfigurationPtr(new InstrumentConfiguration("IC")));
    msd.instrumentConfigurationPtrs.back()->set(MS_Bruker_Daltonics_instrument_model);
    if (!msd.instrumentConfigurationPtrs.empty())
        msd.run.defaultInstrumentConfigurationPtr = msd.instrumentConfigurationPtrs[0];

    msd.run.id = msd.id;
    msd.run.startTimeStamp = encode_xml_datetime(compassDataPtr->getAnalysisDateTime());
}

} // namespace


PWIZ_API_DECL
void Reader_Bruker::read(const string& filename,
                         const string& head,
                         MSData& result,
                         int runIndex,
                         const Config& config) const
{
    if (runIndex != 0)
        throw ReaderFail("[Reader_Bruker::read] multiple runs not supported");

    Reader_Bruker_Format format = detail::format(filename);
    if (format == Reader_Bruker_Format_Unknown)
        throw ReaderFail("[Reader_Bruker::read] Path given is not a recognized Bruker format");


    // trim filename from end of source path if necessary (it's not valid to pass to CompassXtract)
    bfs::path rootpath = filename;
    if (bfs::is_regular_file(rootpath))
        rootpath = rootpath.branch_path();

    CompassDataPtr compassDataPtr(CompassData::create(rootpath.string(), format));

    SpectrumList_Bruker* sl = new SpectrumList_Bruker(result, rootpath.string(), format, compassDataPtr);
    ChromatogramList_Bruker* cl = new ChromatogramList_Bruker(result, rootpath.string(), format, compassDataPtr);
    result.run.spectrumListPtr = SpectrumListPtr(sl);
    result.run.chromatogramListPtr = ChromatogramListPtr(cl);

    fillInMetadata(rootpath, result, format, compassDataPtr);
}


} // namespace msdata
} // namespace pwiz


#else // PWIZ_READER_BRUKER

//
// non-MSVC implementation
//

namespace pwiz {
namespace msdata {

PWIZ_API_DECL void Reader_Bruker::read(const string& filename, const string& head, MSData& result, int sampleIndex /* = 0 */, const Config& config) const
{
    throw ReaderFail("[Reader_Bruker::read()] Bruker Analysis reader not implemented: "
#ifdef _MSC_VER // should be possible, apparently somebody decided to skip it
        "support was explicitly disabled when program was built"
#elif defined(WIN32) // wrong compiler
        "program was built without COM support and cannot access CompassXtract DLLs - try building with MSVC instead of GCC"
#else // wrong platform
        "requires CompassXtract which only works on Windows"
#endif
        );
}

} // namespace msdata
} // namespace pwiz

#endif // PWIZ_READER_BRUKER
