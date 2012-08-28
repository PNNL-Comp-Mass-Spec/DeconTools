//
// $Id: msconvert.cpp 3808 2012-07-24 20:31:10Z donmarsh $
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


#include "pwiz_tools/common/FullReaderList.hpp"
#include "pwiz/data/msdata/MSDataFile.hpp"
#include "pwiz/data/msdata/MSDataMerger.hpp"
#include "pwiz/data/msdata/IO.hpp"
#include "pwiz/data/msdata/SpectrumInfo.hpp"
#include "pwiz/utility/misc/IterationListener.hpp"
#include "pwiz/analysis/spectrum_processing/SpectrumListFactory.hpp"
#include "pwiz/Version.hpp"
#include "pwiz/data/msdata/Version.hpp"
#include "pwiz/analysis/Version.hpp"
#include "boost/program_options.hpp"
#include "pwiz/utility/misc/Filesystem.hpp"
#include "pwiz/utility/misc/Std.hpp"

using namespace pwiz::cv;
using namespace pwiz::data;
using namespace pwiz::msdata;
using namespace pwiz::analysis;
using namespace pwiz::util;

ostream* os_ = &cout;


class usage_exception : public std::runtime_error
{
    public: usage_exception(const string& usage) : runtime_error(usage) {}
};

class user_error : public std::runtime_error
{
    public: user_error(const string& what) : runtime_error(what) {}
};


/// Holds the results of the parseCommandLine function. 
struct Config
{
    vector<string> filenames;
    vector<string> filters;
    string outputPath;
    string extension;
    string outputFile;
    bool verbose;
    MSDataFile::WriteConfig writeConfig;
    string contactFilename;
    bool merge;
    bool simAsSpectra;
    bool srmAsSpectra;

    Config()
    :   outputPath("."), verbose(false), merge(false), simAsSpectra(false), srmAsSpectra(false)
    {}

    string outputFilename(const string& inputFilename, const MSData& inputMSData) const;
};


string Config::outputFilename(const string& filename, const MSData& msd) const
{
    string runId = msd.run.id;

    // if necessary, adjust runId so it makes a suitable filename
    if (!outputFile.empty())
    {
        runId = outputFile;
    }
    if (runId.empty())
        runId = bfs::basename(filename);
    else
    {
        string extension = bal::to_lower_copy(bfs::extension(runId));
        if (extension == ".mzml" ||
            extension == ".mzxml" ||
            extension == ".xml" ||
            extension == ".mgf" ||
            extension == ".ms2" ||
            extension == ".cms2" ||
            extension == ".mz5")
            runId = bfs::basename(runId);
    }

    // this list is for Windows; it's a superset of the POSIX list
    string illegalFilename = "\\/*:?<>|\"";
    BOOST_FOREACH(char& c, runId)
        if (illegalFilename.find(c) != string::npos)
            c = '_';

    bfs::path newFilename = runId + extension;
    bfs::path fullPath = bfs::path(outputPath) / newFilename;
    return fullPath.string(); 
}


ostream& operator<<(ostream& os, const Config& config)
{
    os << "format: " << config.writeConfig << endl;
    os << "outputPath: " << config.outputPath << endl;
    os << "extension: " << config.extension << endl; 
    os << "contactFilename: " << config.contactFilename << endl;
    os << endl;

    os << "filters:\n  ";
    copy(config.filters.begin(), config.filters.end(), ostream_iterator<string>(os,"\n  "));
    os << endl;

    os << "filenames:\n  ";
    copy(config.filenames.begin(), config.filenames.end(), ostream_iterator<string>(os,"\n  "));
    os << endl;

    return os;
}


/// Parses command line arguments to setup execution parameters, and
/// contructs help text. Inputs are the arguments to main. A filled
/// Config object is returned.
Config parseCommandLine(int argc, const char* argv[])
{
    namespace po = boost::program_options;

    ostringstream usage;
    usage << "Usage: msconvert [options] [filemasks]\n"
          << "Convert mass spec data file formats.\n"
#ifndef _MSC_VER
          << "\n"
          << "Note: the use of mass spec vendor DLLs is not enabled in this \n"
          << "(non-MSVC) build, this means no Thermo, Bruker, Waters etc input.\n"
#endif
          << "\n"
          << "Return value: # of failed files.\n"
          << "\n";
        
    Config config;
    string filelistFilename;
    string configFilename;

    bool format_text = false;
    bool format_mzML = false;
    bool format_mzXML = false;
    bool format_MGF = false;
    bool format_MS2 = false;
    bool format_CMS2 = false;
    bool format_mz5 = false;
    bool precision_32 = false;
    bool precision_64 = false;
    bool mz_precision_32 = false;
    bool mz_precision_64 = false;
    bool intensity_precision_32 = false;
    bool intensity_precision_64 = false;
    bool noindex = false;
    bool zlib = false;
    bool gzip = false;

    po::options_description od_config("Options");
    od_config.add_options()
        ("filelist,f",
            po::value<string>(&filelistFilename),
            ": specify text file containing filenames")
        ("outdir,o",
            po::value<string>(&config.outputPath)->default_value(config.outputPath),
            ": set output directory ('-' for stdout) [.]")
        ("config,c", 
            po::value<string>(&configFilename),
            ": configuration file (optionName=value)")
        ("outfile",
            po::value<string>(&config.outputFile)->default_value(config.outputFile),
         ": Override the name of output file.")
        ("ext,e",
            po::value<string>(&config.extension)->default_value(config.extension),
            ": set extension for output files [mzML|mzXML|mgf|txt"
#ifndef WITHOUT_MZ5
            "|mz5"
#endif
            "]")
        ("mzML",
            po::value<bool>(&format_mzML)->zero_tokens(),
            ": write mzML format [default]")
        ("mzXML",
            po::value<bool>(&format_mzXML)->zero_tokens(),
            ": write mzXML format")
#ifndef WITHOUT_MZ5
        ("mz5",
            po::value<bool>(&format_mz5)->zero_tokens(),
            ": write mz5 format")
#endif
        ("mgf",
            po::value<bool>(&format_MGF)->zero_tokens(),
            ": write Mascot generic format")
        ("text",
            po::value<bool>(&format_text)->zero_tokens(),
            ": write ProteoWizard internal text format")
        ("ms2",
            po::value<bool>(&format_MS2)->zero_tokens(),
            ": write MS2 format")
        ("cms2",
            po::value<bool>(&format_CMS2)->zero_tokens(),
            ": write CMS2 format")
        ("verbose,v",
            po::value<bool>(&config.verbose)->zero_tokens(),
            ": display detailed progress information")
        ("64",
            po::value<bool>(&precision_64)->zero_tokens(),
            ": set default binary encoding to 64-bit precision [default]")
        ("32",
            po::value<bool>(&precision_32)->zero_tokens(),
            ": set default binary encoding to 32-bit precision")
        ("mz64",
            po::value<bool>(&mz_precision_64)->zero_tokens(),
            ": encode m/z values in 64-bit precision [default]")
        ("mz32",
            po::value<bool>(&mz_precision_32)->zero_tokens(),
            ": encode m/z values in 32-bit precision")
        ("inten64",
            po::value<bool>(&intensity_precision_64)->zero_tokens(),
            ": encode intensity values in 64-bit precision")
        ("inten32",
            po::value<bool>(&intensity_precision_32)->zero_tokens(),
            ": encode intensity values in 32-bit precision [default]")
        ("noindex",
            po::value<bool>(&noindex)->zero_tokens(),
            ": do not write index")
        ("contactInfo,i",
            po::value<string>(&config.contactFilename),
            ": filename for contact info")
        ("zlib,z",
            po::value<bool>(&zlib)->zero_tokens(),
            ": use zlib compression for binary data")
        ("gzip,g",
            po::value<bool>(&gzip)->zero_tokens(),
            ": gzip entire output file (adds .gz to filename)")
        ("filter",
            po::value< vector<string> >(&config.filters),
            ": add a spectrum list filter")
        ("merge",
            po::value<bool>(&config.merge)->zero_tokens(),
            ": create a single output file from multiple input files by merging file-level metadata and concatenating spectrum lists")
        ("simAsSpectra",
            po::value<bool>(&config.simAsSpectra)->zero_tokens(),
            ": write selected ion monitoring as spectra, not chromatograms")
        ("srmAsSpectra",
            po::value<bool>(&config.srmAsSpectra)->zero_tokens(),
            ": write selected reaction monitoring as spectra, not chromatograms")
        ;

    // append options description to usage string

    usage << od_config;

    // extra usage

    usage << SpectrumListFactory::usage() << endl;

    usage << "Examples:\n"
          << endl
          << "# convert data.RAW to data.mzML\n"
          << "msconvert data.RAW\n"
          << endl
          << "# convert data.RAW to data.mzXML\n"
          << "msconvert data.RAW --mzXML\n"
          << endl
          << "# put output file in my_output_dir\n"
          << "msconvert data.RAW -o my_output_dir\n"
          << endl
          << "# extract scan indices 5...10 and 20...25\n"
          << "msconvert data.RAW --filter \"index [5,10] [20,25]\"\n"
          << endl
          << "# extract MS1 scans only\n"
          << "msconvert data.RAW --filter \"msLevel 1\"\n"
          << endl
          << "# extract MS2 and MS3 scans only\n"
          << "msconvert data.RAW --filter \"msLevel 2-3\"\n"
          << endl
          << "# extract MSn scans for n>1\n"
          << "msconvert data.RAW --filter \"msLevel 2-\"\n"
          << endl
          << "# apply ETD precursor mass filter\n"
          << "msconvert data.RAW --filter ETDFilter\n"
          << endl
          << "# remove non-flanking zero value samples\n"
          << "msconvert data.RAW --filter \"zeroSamples removeExtra\"\n"
          << endl
          << "# remove non-flanking zero value samples in MS2 and MS3 only\n"
          << "msconvert data.RAW --filter \"zeroSamples removeExtra 2 3\"\n"
          << endl
          << "# add missing zero value samples (with 5 flanking zeros) in MS2 and MS3 only\n"
          << "msconvert data.RAW --filter \"zeroSamples addMissing=5 2 3\"\n"
          << endl
          << "# keep only HCD spectra from a decision tree data file\n"
          << "msconvert data.RAW --filter \"activation HCD\"\n"
          << endl
          << "# keep the top 42 peaks or samples (depending on whether spectra are centroid or profile):\n"
          << "msconvert data.RAW --filter \"threshold count 42 most-intense\"\n"
          << endl
          << "# multiple filters: select scan numbers and recalculate precursors\n"
          << "msconvert data.RAW --filter \"scanNumber [500,1000]\" --filter \"precursorRecalculation\"\n"
          << endl
          << "# multiple filters: apply peak picking and then keep the bottom 100 peaks:\n"
          << "msconvert data.RAW --filter \"peakPicking true 1-\" --filter \"threshold count 100 least-intense\"\n"
          << endl
          << "# multiple filters: apply peak picking and then keep all peaks that are at least 50% of the intensity of the base peak:\n"
          << "msconvert data.RAW --filter \"peakPicking true 1-\" --filter \"threshold bpi-relative .5 most-intense\"\n"
          << endl
          << "# use a configuration file\n"
          << "msconvert data.RAW -c config.txt\n"
          << endl
          << "# example configuration file\n"
          << "mzXML=true\n"
          << "zlib=true\n"
          << "filter=\"index [3,7]\"\n"
          << "filter=\"precursorRecalculation\"\n"
          << endl
          << endl

          << "Questions, comments, and bug reports:\n"
          << "http://proteowizard.sourceforge.net\n"
          << "support@proteowizard.org\n"
          << "\n"
          << "ProteoWizard release: " << pwiz::Version::str() << " (" << pwiz::Version::LastModified() << ")" << endl
          << "ProteoWizard MSData: " << pwiz::msdata::Version::str() << " (" << pwiz::msdata::Version::LastModified() << ")" << endl
          << "ProteoWizard Analysis: " << pwiz::analysis::Version::str() << " (" << pwiz::analysis::Version::LastModified() << ")" << endl
          << "Build date: " << __DATE__ << " " << __TIME__ << endl;

    if (argc <= 1)
        throw usage_exception(usage.str().c_str());

    // handle positional arguments

    const char* label_args = "args";

    po::options_description od_args;
    od_args.add_options()(label_args, po::value< vector<string> >(), "");

    po::positional_options_description pod_args;
    pod_args.add(label_args, -1);
   
    po::options_description od_parse;
    od_parse.add(od_config).add(od_args);

    // parse command line

    po::variables_map vm;
    po::store(po::command_line_parser(argc, (char**)argv).
              options(od_parse).positional(pod_args).run(), vm);
    po::notify(vm);

    // parse config file if required

    if (!configFilename.empty())
    {
        ifstream is(configFilename.c_str());

        if (is)
        {
            *os_ << "Reading configuration file " << configFilename << "\n\n";
            po::store(parse_config_file(is, od_config), vm);
            po::notify(vm);
        }
        else
        {
            *os_ << "Unable to read configuration file " << configFilename << "\n\n";
        }
    }

    // remember filenames from command line

    if (vm.count(label_args))
    {
        config.filenames = vm[label_args].as< vector<string> >();

        // expand the filenames by globbing to handle wildcards
        vector<bfs::path> globbedFilenames;
        BOOST_FOREACH(const string& filename, config.filenames)
        {
            expand_pathmask(bfs::path(filename), globbedFilenames);
            if (!globbedFilenames.size())
            {
                *os_ <<  "[msconvert] no files found matching \"" << filename << "\"" << endl;
            }
        }

        config.filenames.clear();
        BOOST_FOREACH(const bfs::path& filename, globbedFilenames)
            config.filenames.push_back(filename.string());
    }

    // parse filelist if required

    if (!filelistFilename.empty())
    {
        ifstream is(filelistFilename.c_str());
        while (is)
        {
            string filename;
            getline(is, filename);
            if (is) config.filenames.push_back(filename);
        }
    }

    // check stuff

    if (config.filenames.empty())
        throw user_error("[msconvert] No files specified.");

    int count = format_text + format_mzML + format_mzXML + format_MGF + format_MS2 + format_CMS2 + format_mz5;
    if (count > 1) throw user_error("[msconvert] Multiple format flags specified.");
    if (format_text) config.writeConfig.format = MSDataFile::Format_Text;
    if (format_mzML) config.writeConfig.format = MSDataFile::Format_mzML;
    if (format_mzXML) config.writeConfig.format = MSDataFile::Format_mzXML;
    if (format_MGF) config.writeConfig.format = MSDataFile::Format_MGF;
    if (format_MS2) config.writeConfig.format = MSDataFile::Format_MS2;
    if (format_CMS2) config.writeConfig.format = MSDataFile::Format_CMS2;
    if (format_mz5) config.writeConfig.format = MSDataFile::Format_MZ5;

    config.writeConfig.gzipped = gzip; // if true, file is written as .gz

    if (config.extension.empty())
    {
        switch (config.writeConfig.format)
        {
            case MSDataFile::Format_Text:
                config.extension = ".txt";
                break;
            case MSDataFile::Format_mzML:
                config.extension = ".mzML";
                break;
            case MSDataFile::Format_mzXML:
                config.extension = ".mzXML";
                break;
            case MSDataFile::Format_MGF:
                config.extension = ".mgf";
                break;
            case MSDataFile::Format_MS2:
                config.extension = ".ms2";
                break;
            case MSDataFile::Format_CMS2:
                config.extension = ".cms2";
                break;
            case MSDataFile::Format_MZ5:
#ifdef WITHOUT_MZ5
                throw user_error("[msconvert] Not built with mz5 support."); 
#endif
                config.extension = ".mz5";
                break;
            default:
                throw user_error("[msconvert] Unsupported format."); 
        }
        if (config.writeConfig.gzipped) 
        {
            config.extension += ".gz";
        }
    }

    // precision defaults

    config.writeConfig.binaryDataEncoderConfig.precision = BinaryDataEncoder::Precision_64;
    config.writeConfig.binaryDataEncoderConfig.precisionOverrides[MS_m_z_array] = BinaryDataEncoder::Precision_64;
    config.writeConfig.binaryDataEncoderConfig.precisionOverrides[MS_intensity_array] = BinaryDataEncoder::Precision_32;

    // handle precision flags

    if (precision_32 && precision_64 ||
        mz_precision_32 && mz_precision_64 ||
        intensity_precision_32 && intensity_precision_64)
        throw user_error("[msconvert] Incompatible precision flags.");

    if (precision_32)
    {
        config.writeConfig.binaryDataEncoderConfig.precision
            = config.writeConfig.binaryDataEncoderConfig.precisionOverrides[MS_m_z_array]
            = config.writeConfig.binaryDataEncoderConfig.precisionOverrides[MS_intensity_array] 
            = BinaryDataEncoder::Precision_32;
    }
    else if (precision_64)
    {
        config.writeConfig.binaryDataEncoderConfig.precision
            = config.writeConfig.binaryDataEncoderConfig.precisionOverrides[MS_m_z_array]
            = config.writeConfig.binaryDataEncoderConfig.precisionOverrides[MS_intensity_array] 
            = BinaryDataEncoder::Precision_64;
    }

    if (mz_precision_32)
        config.writeConfig.binaryDataEncoderConfig.precisionOverrides[MS_m_z_array] = BinaryDataEncoder::Precision_32;
    if (mz_precision_64)
        config.writeConfig.binaryDataEncoderConfig.precisionOverrides[MS_m_z_array] = BinaryDataEncoder::Precision_64;
    if (intensity_precision_32)
        config.writeConfig.binaryDataEncoderConfig.precisionOverrides[MS_intensity_array] = BinaryDataEncoder::Precision_32;
    if (intensity_precision_64)
        config.writeConfig.binaryDataEncoderConfig.precisionOverrides[MS_intensity_array] = BinaryDataEncoder::Precision_64;

    // other flags

    if (noindex)
        config.writeConfig.indexed = false;

    if (zlib)
        config.writeConfig.binaryDataEncoderConfig.compression = BinaryDataEncoder::Compression_Zlib;

    return config;
}


void addContactInfo(MSData& msd, const string& contactFilename)
{
    ifstream is(contactFilename.c_str());
    if (!is)
    {
        cerr << "unable to read contact info: " << contactFilename << endl; 
        return;
    }

    Contact contact;
    IO::read(is, contact);
    msd.fileDescription.contacts.push_back(contact);
}


class UserFeedbackIterationListener : public IterationListener
{
    public:

    virtual Status update(const UpdateMessage& updateMessage)
    {
        // add tabs to erase all of the previous line
        *os_ << updateMessage.iterationIndex+1 << "/" << updateMessage.iterationCount << "\t\t\t\r" << flush;

        // spectrum and chromatogram lists both iterate; put them on different lines
        if (updateMessage.iterationIndex+1 == updateMessage.iterationCount)
            *os_ << endl;
        return Status_Ok;
    }
};


void calculateSourceFilePtrSHA1(const SourceFilePtr& sourceFilePtr)
{
    calculateSourceFileSHA1(*sourceFilePtr);
}


/// Combines multiple input files into a single MSData object. Called
/// when the --merge argument is present on the command line.
int mergeFiles(const vector<string>& filenames, const Config& config, const ReaderList& readers)
{
    vector<MSDataPtr> msdList;
    int failedFileCount = 0;

    ReaderList::Config readerConfig;
    readerConfig.simAsSpectra = config.simAsSpectra;
    readerConfig.srmAsSpectra = config.srmAsSpectra;

    // Each file is read in separately in MSData objects in the msdList list.
    BOOST_FOREACH(const string& filename, filenames)
    {
        try
        {
            *os_ << "processing file: " << filename << endl;
            readers.read(filename, msdList, readerConfig);
        }
        catch (exception& e)
        {
            ++failedFileCount;
            cerr << "Error reading file " << filename << ":\n" << e.what() << endl;
        }
    }

    // handle progress updates if requested

    IterationListenerRegistry iterationListenerRegistry;
    // update on the first spectrum, the last spectrum, the 100th spectrum, the 200th spectrum, etc.
    const size_t iterationPeriod = 100;
    iterationListenerRegistry.addListener(IterationListenerPtr(new UserFeedbackIterationListener), iterationPeriod);
    IterationListenerRegistry* pILR = config.verbose ? &iterationListenerRegistry : 0;

    // MSDataMerger handles combining all files in msdList into a single MSDataFile object.
    try
    {
        MSDataMerger msd(msdList);

        *os_ << "calculating source file checksums" << endl;
        calculateSHA1Checksums(msd);

        if (!config.contactFilename.empty())
            addContactInfo(msd, config.contactFilename);

        SpectrumListFactory::wrap(msd, config.filters);

        string outputFilename = config.outputFilename("merged-spectra", msd);
        *os_ << "writing output file: " << outputFilename << endl;

        if (config.outputPath == "-")
            MSDataFile::write(msd, cout, config.writeConfig);
        else
            MSDataFile::write(msd, outputFilename, config.writeConfig, pILR);
    }
    catch (exception& e)
    {
        failedFileCount = (int)filenames.size();
        cerr << "Error merging files: " << e.what() << endl;
    }

    return failedFileCount;
}

/// Handles the reading of a single input file. Called once for each
/// input file when the --merge arguement is absent.
void processFile(const string& filename, const Config& config, const ReaderList& readers)
{
    // read in data file

    *os_ << "processing file: " << filename << endl;

    ReaderList::Config readerConfig;
    readerConfig.simAsSpectra = config.simAsSpectra;
    readerConfig.srmAsSpectra = config.srmAsSpectra;

    vector<MSDataPtr> msdList;
    readers.read(filename, msdList, readerConfig);

    for (size_t i=0; i < msdList.size(); ++i)
    {
        MSData& msd = *msdList[i];
        try
        {
            // calculate SHA1 checksums
            calculateSHA1Checksums(msd);

            // process the data 

            if (!config.contactFilename.empty())
                addContactInfo(msd, config.contactFilename);

            SpectrumListFactory::wrap(msd, config.filters);

            // handle progress updates if requested

            IterationListenerRegistry iterationListenerRegistry;
            // update on the first spectrum, the last spectrum, the 100th spectrum, the 200th spectrum, etc.
            const size_t iterationPeriod = 100;
            iterationListenerRegistry.addListener(IterationListenerPtr(new UserFeedbackIterationListener), iterationPeriod);
            IterationListenerRegistry* pILR = config.verbose ? &iterationListenerRegistry : 0; 

            // write out the new data file
            string outputFilename = config.outputFilename(filename, msd);
            *os_ << "writing output file: " << outputFilename << endl;

            if (config.outputPath == "-")
                MSDataFile::write(msd, cout, config.writeConfig, pILR);
            else
                MSDataFile::write(msd, outputFilename, config.writeConfig, pILR);
        }
        catch (exception& e)
        {
            cerr << "Error writing run " << (i+1) << " in " << bfs::path(filename).leaf() << ":\n" << e.what() << endl;
        }
    }
    *os_ << endl;
}


/// Handles the high level logic of msconvert. Constructs the output
/// directory, reads files into memory and writes them out consistent
/// with the options in the supplied Config.
int go(const Config& config)
{
    *os_ << config;

    boost::filesystem::create_directories(config.outputPath);

    FullReaderList readers;

    int failedFileCount = 0;

    if (config.merge)
        failedFileCount = mergeFiles(config.filenames, config, readers);
    else
    {

        for (vector<string>::const_iterator it=config.filenames.begin(); 
             it!=config.filenames.end(); ++it)
        {
            try
            {
                processFile(*it, config, readers);
            }
            catch (exception& e)
            {
                failedFileCount++;
                *os_ << e.what() << endl;
                *os_ << "Error processing file " << *it << "\n\n"; 
            }
        }
    }

    return failedFileCount;
}


int main(int argc, const char* argv[])
{
    try
    {
        Config config = parseCommandLine(argc, argv);

        // if redirecting conversion to cout, use cerr for all console output
        if (config.outputPath == "-")
            os_ = &cerr;

        return go(config);
    }
    catch (usage_exception& e)
    {
        cerr << e.what() << endl;
        return 0;
    }
    catch (user_error& e)
    {
        cerr << e.what() << endl;
        return 1;
    }
    catch (exception& e)
    {
        cerr << e.what() << endl;
    }
    catch (...)
    {
        cerr << "[" << argv[0] << "] Caught unknown exception.\n";
    }

    cerr << "Please report this error to support@proteowizard.org.\n"
         << "Attach the command output and this version information in your report:\n"
         << "\n"
         << "ProteoWizard release: " << pwiz::Version::str() << " (" << pwiz::Version::LastModified() << ")" << endl
         << "ProteoWizard MSData: " << pwiz::msdata::Version::str() << " (" << pwiz::msdata::Version::LastModified() << ")" << endl
         << "ProteoWizard Analysis: " << pwiz::analysis::Version::str() << " (" << pwiz::analysis::Version::LastModified() << ")" << endl
         << "Build date: " << __DATE__ << " " << __TIME__ << endl;

    return 1;
}

