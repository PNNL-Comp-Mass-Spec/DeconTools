//
// $Id: Digestion.cpp 3318 2012-02-27 16:57:51Z chambm $ 
//
//
// Original author: Matt Chambers <matt.chambers .@. vanderbilt.edu>
//
// Copyright 2006 Louis Warschaw Prostate Cancer Center
//   Cedars Sinai Medical Center, Los Angeles, California  90048
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


#define PWIZ_SOURCE

#include "Digestion.hpp"
#include "Peptide.hpp"
#include "AminoAcid.hpp"
#include "pwiz/utility/misc/CharIndexedVector.hpp"
#include "pwiz/utility/misc/Exception.hpp"
#include "pwiz/utility/misc/Std.hpp"
#include "pwiz/utility/misc/Singleton.hpp"


namespace pwiz {
namespace proteome {


using namespace pwiz::cv;
using namespace pwiz::util;
using boost::shared_ptr;

#undef SECURE_SCL


PWIZ_API_DECL
DigestedPeptide::DigestedPeptide(const string& sequence)
:   Peptide(sequence)
{
}

PWIZ_API_DECL
DigestedPeptide::DigestedPeptide(const char* sequence)
:   Peptide(sequence)
{
}

PWIZ_API_DECL
DigestedPeptide::DigestedPeptide(std::string::const_iterator begin,
                                 std::string::const_iterator end,
                                 size_t offset,
                                 size_t missedCleavages,
                                 bool NTerminusIsSpecific,
                                 bool CTerminusIsSpecific, 
                                 std::string NTerminusPrefix,
                                 std::string CTerminusSuffix)
:   Peptide(begin, end),
    offset_(offset),
    missedCleavages_(missedCleavages),
    NTerminusIsSpecific_(NTerminusIsSpecific),
    CTerminusIsSpecific_(CTerminusIsSpecific),
    NTerminusPrefix_(NTerminusPrefix),
    CTerminusSuffix_(CTerminusSuffix)
{
}

PWIZ_API_DECL
DigestedPeptide::DigestedPeptide(const Peptide& peptide,
                                 size_t offset,
                                 size_t missedCleavages,
                                 bool NTerminusIsSpecific,
                                 bool CTerminusIsSpecific, 
                                 std::string NTerminusPrefix,
                                 std::string CTerminusSuffix)
:   Peptide(peptide),
    offset_(offset),
    missedCleavages_(missedCleavages),
    NTerminusIsSpecific_(NTerminusIsSpecific),
    CTerminusIsSpecific_(CTerminusIsSpecific),
    NTerminusPrefix_(NTerminusPrefix),
    CTerminusSuffix_(CTerminusSuffix)
{
}


PWIZ_API_DECL DigestedPeptide::DigestedPeptide(const DigestedPeptide& other)
:   Peptide(other),
    offset_(other.offset_),
    missedCleavages_(other.missedCleavages_),
    NTerminusIsSpecific_(other.NTerminusIsSpecific_),
    CTerminusIsSpecific_(other.CTerminusIsSpecific_),
    NTerminusPrefix_(other.NTerminusPrefix_),
    CTerminusSuffix_(other.CTerminusSuffix_)
{
}

PWIZ_API_DECL DigestedPeptide& DigestedPeptide::operator=(const DigestedPeptide& rhs)
{
    Peptide::operator=(rhs);
    offset_ = rhs.offset_;
    missedCleavages_ = rhs.missedCleavages_;
    NTerminusIsSpecific_ = rhs.NTerminusIsSpecific_;
    CTerminusIsSpecific_ = rhs.CTerminusIsSpecific_;
    NTerminusPrefix_ = rhs.NTerminusPrefix_;
    CTerminusSuffix_ = rhs.CTerminusSuffix_;
    return *this;
}

PWIZ_API_DECL DigestedPeptide::~DigestedPeptide()
{
}

PWIZ_API_DECL size_t DigestedPeptide::offset() const
{
    return offset_;
}

PWIZ_API_DECL size_t DigestedPeptide::missedCleavages() const
{
    return missedCleavages_;
}

PWIZ_API_DECL size_t DigestedPeptide::specificTermini() const
{
    return (size_t) NTerminusIsSpecific_ + (size_t) CTerminusIsSpecific_;
}

PWIZ_API_DECL bool DigestedPeptide::NTerminusIsSpecific() const
{
    return NTerminusIsSpecific_;
}

PWIZ_API_DECL bool DigestedPeptide::CTerminusIsSpecific() const
{
    return CTerminusIsSpecific_;
}

PWIZ_API_DECL std::string DigestedPeptide::NTerminusPrefix() const
{
    return NTerminusPrefix_;
}

PWIZ_API_DECL std::string DigestedPeptide::CTerminusSuffix() const
{
    return CTerminusSuffix_;
}

PWIZ_API_DECL bool DigestedPeptide::operator==(const DigestedPeptide& rhs) const
{
    return this->Peptide::operator==(rhs) &&
           offset() == rhs.offset() &&
           missedCleavages() == rhs.missedCleavages() &&
           NTerminusIsSpecific() == rhs.NTerminusIsSpecific() &&
           CTerminusIsSpecific() == rhs.CTerminusIsSpecific() &&
           NTerminusPrefix() == rhs.NTerminusPrefix() &&
           CTerminusSuffix() == rhs.CTerminusSuffix();
}




PWIZ_API_DECL
Digestion::Config::Config(int maximumMissedCleavages,
                          //double minimumMass,
                          //double maximumMass,
                          int minimumLength,
                          int maximumLength,
                          Specificity minimumSpecificity,
                          bool clipNTerminalMethionine)
:   maximumMissedCleavages(maximumMissedCleavages),
    //minimumMass(minimumMass), maximumMass(maximumMass),
    minimumLength(minimumLength), maximumLength(maximumLength),
    minimumSpecificity(minimumSpecificity),
    clipNTerminalMethionine(clipNTerminalMethionine)
{
}


namespace {

class CleavageAgentInfo : public boost::singleton<CleavageAgentInfo>
{
    public:
    CleavageAgentInfo(boost::restricted)
    {
        const vector<CVID>& cvids = pwiz::cv::cvids();
        for (vector<CVID>::const_iterator itr = cvids.begin();
             itr != cvids.end(); ++itr)
        {
            if (!cvIsA(*itr, MS_cleavage_agent_name))
                continue;

            const CVTermInfo& cvTermInfo = pwiz::cv::cvTermInfo(*itr);
            multimap<string, CVID>::const_iterator regexRelationItr =
                cvTermInfo.otherRelations.find("has_regexp");
            if (regexRelationItr != cvTermInfo.otherRelations.end())
            {
                cleavageAgents_.insert(*itr);
                cleavageAgentNames_.push_back(cvTermInfo.name);
                cleavageAgentNameToCvidMap_[bal::to_lower_copy(cvTermInfo.name)] = *itr;
                const CVTermInfo& cleavageAgentRegexTerm = pwiz::cv::cvTermInfo(regexRelationItr->second);
                cleavageAgentRegexToCvidMap_[cleavageAgentRegexTerm.name] = *itr;
                cleavageAgentToRegexMap_[*itr] = &cleavageAgentRegexTerm;
            }
        }
    }

    const set<CVID>& cleavageAgents() const {return cleavageAgents_;}
    const vector<string>& cleavageAgentNames() const {return cleavageAgentNames_;}

    CVID getCleavageAgentByName(const string& agentName) const
    {
        string name = bal::to_lower_copy(agentName);
        map<string, CVID>::const_iterator agentTermItr = cleavageAgentNameToCvidMap_.find(name);

        if (agentTermItr == cleavageAgentNameToCvidMap_.end())
            return CVID_Unknown;

        return agentTermItr->second;
    }

    CVID getCleavageAgentByRegex(const string& agentRegex) const
    {
        map<string, CVID>::const_iterator agentTermItr = cleavageAgentRegexToCvidMap_.find(agentRegex);

        if (agentTermItr == cleavageAgentRegexToCvidMap_.end())
            return CVID_Unknown;

        return agentTermItr->second;
    }

    const std::string& getCleavageAgentRegex(CVID agentCvid) const
    {
        if (!pwiz::cv::cvIsA(agentCvid, MS_cleavage_agent_name))
            throw invalid_argument("[getRegexForCleavageAgent] CVID is not a cleavage agent.");

        map<CVID, const CVTermInfo*>::const_iterator regexTermItr =
            cleavageAgentToRegexMap_.find(agentCvid);

        if (regexTermItr == cleavageAgentToRegexMap_.end())
            throw runtime_error("[getRegexForCleavageAgent] No regex relation for cleavage agent " + cvTermInfo(agentCvid).name);

        return regexTermItr->second->name;
    }

    private:
    set<CVID> cleavageAgents_;
    vector<string> cleavageAgentNames_;
    map<string, CVID> cleavageAgentNameToCvidMap_;
    map<string, CVID> cleavageAgentRegexToCvidMap_;
    map<CVID, const CVTermInfo*> cleavageAgentToRegexMap_;
};

} // namespace


const set<CVID>& Digestion::getCleavageAgents()
{
    return CleavageAgentInfo::instance->cleavageAgents();
}

const vector<string>& Digestion::getCleavageAgentNames()
{
    return CleavageAgentInfo::instance->cleavageAgentNames();
}

CVID Digestion::getCleavageAgentByName(const string& agentName)
{
    return CleavageAgentInfo::instance->getCleavageAgentByName(agentName);
}

CVID Digestion::getCleavageAgentByRegex(const string& agentRegex)
{
    return CleavageAgentInfo::instance->getCleavageAgentByRegex(agentRegex);
}

const string& Digestion::getCleavageAgentRegex(CVID agentCvid)
{
    return CleavageAgentInfo::instance->getCleavageAgentRegex(agentCvid);
}


class Digestion::Impl
{
    public:
    Impl(const Peptide& peptide, const std::vector<CVID>& cleavageAgents, const Config& config)
        :   peptide_(peptide), config_(config)
    {
        if (cleavageAgents.size() == 1)
        {
            cleavageAgentRegex_ = getCleavageAgentRegex(cleavageAgents[0]);
            return;
        }

        string mergedRegex = "((" + getCleavageAgentRegex(cleavageAgents[0]);
        for (size_t i=1; i < cleavageAgents.size(); ++i)
            mergedRegex += ")|(" + getCleavageAgentRegex(cleavageAgents[i]);
        mergedRegex += "))";

        cleavageAgentRegex_ = mergedRegex;
    }

    Impl(const Peptide& peptide, const vector<boost::regex>& cleavageAgentRegexes, const Config& config)
        :   peptide_(peptide), config_(config)
    {
        if (cleavageAgentRegexes.size() == 1)
        {
            cleavageAgentRegex_ = cleavageAgentRegexes[0];
            return;
        }

        string mergedRegex = "((" + cleavageAgentRegexes[0].str();
        for (size_t i=1; i < cleavageAgentRegexes.size(); ++i)
            mergedRegex += ")|(" + cleavageAgentRegexes[i].str();
        mergedRegex += "))";

        cleavageAgentRegex_ = mergedRegex;
    }

    inline void digest() const
    {
        if (sites_.empty())
        {
            try
            {
                if (cleavageAgentRegex_.empty())
                    throw runtime_error("empty cleavage regex");

                const string& sequence = peptide_.sequence();
                std::string::const_iterator start = sequence.begin();
                std::string::const_iterator end = sequence.end();
                boost::smatch what;
                boost::match_flag_type flags = boost::match_default;
                while (regex_search(start, end, what, cleavageAgentRegex_, flags))
                {
                    sites_.push_back(int(what[0].first-sequence.begin()-1));

                    // update search position and flags
                    start = max(what[0].second, start+1);
                    flags |= boost::match_prev_avail;
                    flags |= boost::match_not_bob;
                }

                // if regex didn't match n-terminus, insert it
                if (sites_.empty() || sites_.front() > -1)
                    sites_.insert(sites_.begin(), -1);

                // if regex didn't match c-terminus, insert it
                if (sites_.back() < (int)sequence.length()-1)
                    sites_.push_back(sequence.length()-1);

                if (sites_.size() > 2 && sites_[1] != 0 &&
                    !sequence.empty() && config_.clipNTerminalMethionine && sequence[0] == 'M')
                    sites_.insert(sites_.begin()+1, 0);

                sitesSet_.insert(sites_.begin(), sites_.end());
            }
            catch (exception& e)
            {
                throw runtime_error(string("[Digestion::Impl::digest()] ") + e.what());
            }
        }
    }

    inline vector<DigestedPeptide> find_all(const Peptide& peptide)
    {
        typedef boost::iterator_range<string::const_iterator> const_string_iterator_range;

        digest(); // populate sites_ member if necessary

        const string& sequence_ = peptide_.sequence();

        vector<DigestedPeptide> result;

        if ((int) peptide.sequence().length() > config_.maximumLength ||
            (int) peptide.sequence().length() < config_.minimumLength)
            return result;

        vector<const_string_iterator_range> instances;
        bal::find_all(instances, sequence_, peptide.sequence());

        BOOST_FOREACH(const_string_iterator_range& range, instances)
        {
            size_t beginOffset = range.begin() - sequence_.begin();
            size_t endOffset = beginOffset + peptide.sequence().length() - 1;

            bool NTerminusIsSpecific = sitesSet_.count(int(beginOffset) - 1) > 0;
            bool CTerminusIsSpecific = sitesSet_.count(int(endOffset)) > 0;

            if (((size_t) NTerminusIsSpecific + (size_t) CTerminusIsSpecific) < (size_t) config_.minimumSpecificity)
                continue;

            size_t missedCleavages = 0;
            for (size_t i = beginOffset; i < endOffset; ++i)
                if (sitesSet_.count((int) i) > 0)
                    ++missedCleavages;

            if (missedCleavages > (size_t) config_.maximumMissedCleavages)
                continue;

            string NTerminusPrefix, CTerminusSuffix;
            if (beginOffset > 0)
			    NTerminusPrefix = sequence_.substr(beginOffset-1, 1);
		    if (endOffset+1 < sequence_.length())
			    CTerminusSuffix = sequence_.substr(endOffset+1, 1);

            result.push_back(DigestedPeptide(peptide,
                                             beginOffset,
                                             missedCleavages,
                                             NTerminusIsSpecific,
                                             CTerminusIsSpecific,
                                             NTerminusPrefix,
                                             CTerminusSuffix));
        }

        return result;
    }

    inline DigestedPeptide find_first(const Peptide& peptide, int offsetHint)
    {
        digest(); // populate sites_ member if necessary
        const string& sequence_ = peptide_.sequence();

        if ((int) peptide.sequence().length() > config_.maximumLength ||
            (int) peptide.sequence().length() < config_.minimumLength)
            throw runtime_error("[Digestion::find_first()] Peptide \"" + peptide.sequence() + "\" not found in \"" + sequence_ + "\"");

        if(offsetHint + peptide.sequence().length() > sequence_.length())
            offsetHint = 0;

        size_t beginOffset = sequence_.find(peptide.sequence(), offsetHint);
        if (beginOffset == string::npos)
            beginOffset = sequence_.find(peptide.sequence(), 0);

        if (beginOffset == string::npos)
            throw runtime_error("[Digestion::find_first()] Peptide \"" + peptide.sequence() + "\" not found in \"" + sequence_ + "\"");

        size_t endOffset = beginOffset + peptide.sequence().length() - 1;

        size_t missedCleavages = 0;
        for (size_t i = beginOffset; i < endOffset; ++i)
            if (sitesSet_.count((int) i) > 0)
                ++missedCleavages;

        if (missedCleavages > (size_t) config_.maximumMissedCleavages)
            throw runtime_error("[Digestion::find_first()] Peptide \"" + peptide.sequence() + "\" not found in \"" + sequence_ + "\"");

        bool NTerminusIsSpecific, CTerminusIsSpecific;
        do
        {
            endOffset = beginOffset + peptide.sequence().length() - 1;

            NTerminusIsSpecific = sitesSet_.count(int(beginOffset)-1) > 0;
            CTerminusIsSpecific = sitesSet_.count(int(endOffset)) > 0;

            if (((size_t) NTerminusIsSpecific + (size_t) CTerminusIsSpecific) >= (size_t) config_.minimumSpecificity)
                break;
            beginOffset = sequence_.find(peptide.sequence(), beginOffset + 1);
        }
        while (beginOffset != string::npos);

        if (beginOffset == string::npos ||
            ((size_t) NTerminusIsSpecific + (size_t) CTerminusIsSpecific) < (size_t) config_.minimumSpecificity)
            throw runtime_error("[Digestion::find_first()] Peptide \"" + peptide.sequence() + "\" not found in \"" + sequence_ + "\"");

        string NTerminusPrefix, CTerminusSuffix;
        if (beginOffset > 0)
		    NTerminusPrefix = sequence_.substr(beginOffset-1, 1);
	    if (endOffset+1 < sequence_.length())
		    CTerminusSuffix = sequence_.substr(endOffset+1, 1);

        return DigestedPeptide(peptide,
                               beginOffset,
                               missedCleavages,
                               NTerminusIsSpecific,
                               CTerminusIsSpecific,
                               NTerminusPrefix,
                               CTerminusSuffix);
    }

    private:
    Peptide peptide_;
    Config config_;
    boost::regex cleavageAgentRegex_;
    friend class Digestion::const_iterator::Impl;

    // precalculated offsets to digestion sites in order of occurence;
    // the sites are between offset and offset+1;
    // -1 is the N terminus digestion site
    // peptide_.sequence().length()-1 is the C terminus digestion site
    mutable vector<int> sites_;
    mutable set<int> sitesSet_;
};


PWIZ_API_DECL
Digestion::Digestion(const Peptide& peptide,
                     CVID cleavageAgent,
                     const Config& config)
:   impl_(new Impl(peptide, vector<CVID>(1, cleavageAgent), config))
{
}

PWIZ_API_DECL
Digestion::Digestion(const Peptide& peptide,
                     const vector<CVID>& cleavageAgents,
                     const Config& config)
:   impl_(new Impl(peptide, cleavageAgents, config))
{
}

PWIZ_API_DECL
Digestion::Digestion(const Peptide& peptide,
                     const boost::regex& cleavageAgentRegex,
                     const Config& config)
:   impl_(new Impl(peptide, vector<boost::regex>(1, cleavageAgentRegex), config))
{
}

PWIZ_API_DECL
Digestion::Digestion(const Peptide& peptide,
                     const vector<boost::regex>& cleavageAgentRegexes,
                     const Config& config)
:   impl_(new Impl(peptide, cleavageAgentRegexes, config))
{
}

PWIZ_API_DECL Digestion::~Digestion()
{
}

PWIZ_API_DECL vector<DigestedPeptide> Digestion::find_all(const Peptide& peptide) const
{
    return impl_->find_all(peptide);
}

PWIZ_API_DECL DigestedPeptide Digestion::find_first(const Peptide& peptide, size_t offsetHint) const
{
    return impl_->find_first(peptide, offsetHint);
}

PWIZ_API_DECL Digestion::const_iterator Digestion::begin() const
{
    return const_iterator(*this);
}

PWIZ_API_DECL Digestion::const_iterator Digestion::end() const
{
    return const_iterator();
}


class Digestion::const_iterator::Impl
{
    public:
    Impl(const Digestion& digestion)
        :   digestionImpl_(*digestion.impl_),
            config_(digestionImpl_.config_),
            sequence_(digestionImpl_.peptide_.sequence()),
            sites_(digestionImpl_.sites_)
    {
        digestionImpl_.digest();
        try
        {
            switch (config_.minimumSpecificity)
            {
                default:
                case FullySpecific:
                    initFullySpecific();
                    break;

                case SemiSpecific: // TODO: optimize semi-specific
                case NonSpecific:
                    initNonSpecific();
                    break;
            }
        }
        catch (exception& e)
        {
            throw runtime_error(string("[Digestion::const_iterator::Impl::Impl()] ") + e.what());
        }
    }

    inline void initFullySpecific()
    {
        begin_ = end_ = sites_.end();

        // iteration requires at least 2 digestion sites
        if (sites_.size() < 2)
            return;

        // try each possible pair of digestion sites;
        // initialize begin_ and end_ to the first pair that meet
        // config's filtering criteria
        for (size_t i=0; i < sites_.size(); ++i)
        {
            int testBegin = sites_[i];

            for (size_t j=1; j < sites_.size(); ++j)
            {
                int testEnd = sites_[j];

                int curMissedCleavages = int(end_ - begin_)-1;
                if (config_.clipNTerminalMethionine && begin_ != sites_.end() && *begin_ < 0 && sequence_[0] == 'M')
                    --curMissedCleavages;
                if (curMissedCleavages > config_.maximumMissedCleavages)
                    break;

                int curLength = testEnd - testBegin;
                if (curLength > config_.maximumLength)
                    break;
                if (curLength < config_.minimumLength)
                    continue;

                begin_ = sites_.begin()+i;
                end_ = sites_.begin()+j;
                break;
            }

            if (begin_ != sites_.end())
                break;
        }
    }

    inline void initNonSpecific()
    {
        begin_ = end_ = sites_.begin();
        beginNonSpecific_ = endNonSpecific_ = sequence_.length();
        int maxLength = (int) sequence_.length();

        // try each possible pair of digestion sites;
        // initialize beginNonSpecific_ and beginNonSpecific_ to the first pair that meet
        // config's filtering criteria
        bool passesFilter = false;
        for (int testBegin = -1; testBegin < maxLength && !passesFilter; ++testBegin)
        {
            for (int testEnd = testBegin+config_.minimumLength; testEnd < maxLength; ++testEnd)
            {
                // end offset is too far, start again with a new begin offset
                int curLength = testEnd - testBegin;
                if (curLength > config_.maximumLength)
                    break;

                beginNonSpecific_ = testBegin;
                endNonSpecific_ = testEnd;
                while (begin_ != sites_.end() && *begin_ <= beginNonSpecific_)
                    ++begin_;
                if (begin_ != sites_.end())
                {
                    end_ = begin_--;
                    while (end_ != sites_.end() && *end_ < endNonSpecific_)
                        ++end_;
                }

                // end offset is too far, start again with a new begin offset
                int curMissedCleavages = int(end_ - begin_)-1;
                if (config_.clipNTerminalMethionine && begin_ != sites_.end() && *begin_ < 0 && sequence_[0] == 'M')
                    --curMissedCleavages;
                if (curMissedCleavages > config_.maximumMissedCleavages)
                    break;

                if (beginNonSpecific_ != maxLength)
                {
                    passesFilter = true;
                    break;
                }
            }
        }
    }

    const DigestedPeptide& peptide() const
    {
        try
        {
            string prefix = "";
            string suffix = "";

            int missedCleavages = int(end_ - begin_)-1;
            if (config_.clipNTerminalMethionine && begin_ != sites_.end() && *begin_ < 0 && sequence_[0] == 'M')
                --missedCleavages;

            if (!peptide_.get())
            {
                switch (config_.minimumSpecificity)
                {
                    default:
                    case FullySpecific:
                        if(*begin_ >= 0 && *begin_ < (int) sequence_.length())
                            prefix = sequence_.substr(*begin_, 1); //this could be changed to be something other than 1 by a config option later
                        if(*end_ != (int) sequence_.length())
                            suffix = sequence_.substr(*end_+1, 1);

                        peptide_.reset(
                            new DigestedPeptide(sequence_.begin()+(*begin_+1),
                                                sequence_.begin()+(*end_+1),
                                                *begin_+1,
                                                missedCleavages,
                                                true,
                                                true,
                                                prefix, 
                                                suffix
						));
			                    
                    break;

                    case SemiSpecific:
                    case NonSpecific:
                        if(beginNonSpecific_ >= 0 && beginNonSpecific_ < (int) sequence_.length())
                            prefix = sequence_.substr(beginNonSpecific_, 1); //this could be changed to be something other than 1 by a config option later
                        if(endNonSpecific_ != (int) sequence_.length())
                            suffix = sequence_.substr(endNonSpecific_+1, 1);

                        peptide_.reset(
                            new DigestedPeptide(sequence_.begin()+(beginNonSpecific_+1),
                                                sequence_.begin()+(endNonSpecific_+1),
                                                beginNonSpecific_+1,
                                                missedCleavages,
                                                begin_ != sites_.end() && *begin_ == beginNonSpecific_,
                                                end_ != sites_.end() && *end_ == endNonSpecific_,
                                                prefix,
                                                suffix
						));
                        break;
                }
            }
            return *peptide_;
        }
        catch (exception& e)
        {
            throw runtime_error(string("[Digestion::const_iterator::Impl::peptide()] ") + e.what());
        }
    }

    inline void nextFullySpecific()
    {
        bool newBegin = (end_ == sites_.end());
        if (!newBegin)
        {
            // advance end_ to the next digestion site
            bool foundNextValidSitePair = false;
            for (++end_; end_ != sites_.end(); ++end_)
            {
                int curMissedCleavages = int(end_ - begin_)-1;
                if (config_.clipNTerminalMethionine && begin_ != sites_.end() && *begin_ < 0 && sequence_[0] == 'M')
                    --curMissedCleavages;
                if (curMissedCleavages > config_.maximumMissedCleavages)
                    break;

                int curLength = *end_ - *begin_;
                if (curLength > config_.maximumLength)
                    break;
                if (curLength < config_.minimumLength)
                    continue;

                foundNextValidSitePair = true;
                break;
            }

            // there may not be another site that makes a valid site pair
            newBegin = !foundNextValidSitePair;
        }

        if (newBegin)
        {
            // advance to the next valid digestion site pair
            bool foundNextValidSitePair = false;
            for (++begin_; begin_ != sites_.end(); ++begin_)
            {
                for (end_ = begin_+1; end_ != sites_.end(); ++end_)
                {
                    int curMissedCleavages = int(end_ - begin_)-1;
                    if (curMissedCleavages > config_.maximumMissedCleavages)
                        break;

                    int curLength = *end_ - *begin_;
                    if (curLength > config_.maximumLength)
                        break;
                    if (curLength < config_.minimumLength)
                        continue;

                    foundNextValidSitePair = true;
                    break;
                }

                if (foundNextValidSitePair)
                    break;
            }
        }
    }

    inline void nextNonSpecific()
    {
        int maxLength = (int) sequence_.length();
        bool newBegin = (endNonSpecific_ == maxLength);
        if (!newBegin)
        {
            // advance endNonSpecific_ to the next digestion site
            bool foundNextValidSitePair = false;
            end_ = begin_;
            for (++endNonSpecific_; endNonSpecific_ < maxLength; ++endNonSpecific_)
            {
                while (end_ != sites_.end() && *end_ < endNonSpecific_)
                    ++end_;

                int curMissedCleavages = int(end_ - begin_)-1;
                if (config_.clipNTerminalMethionine && begin_ != sites_.end() && *begin_ < 0 && sequence_[0] == 'M')
                    --curMissedCleavages;
                if (curMissedCleavages > config_.maximumMissedCleavages)
                    break;

                int curLength = endNonSpecific_ - beginNonSpecific_;
                if (curLength > config_.maximumLength)
                    break;
                if (curLength < config_.minimumLength)
                    continue;

                foundNextValidSitePair = true;
                break;
            }

            // there may not be another site that makes a valid site pair
            newBegin = !foundNextValidSitePair;
        }

        if (newBegin)
        {
            // advance to the next valid digestion site pair
            bool foundNextValidSitePair = false;
            for (++beginNonSpecific_; beginNonSpecific_ < maxLength; ++beginNonSpecific_)
            {
                while (begin_ != sites_.end() && *begin_ <= beginNonSpecific_)
                    ++begin_;
                end_ = begin_--;

                for (endNonSpecific_ = beginNonSpecific_+config_.minimumLength; endNonSpecific_ < maxLength; ++endNonSpecific_)
                {
                    while (end_ != sites_.end() && *end_ < endNonSpecific_)
                        ++end_;

                    int curMissedCleavages = int(end_ - begin_)-1;
                    if (config_.clipNTerminalMethionine && begin_ != sites_.end() && *begin_ < 0 && sequence_[0] == 'M')
                        --curMissedCleavages;
                    if (curMissedCleavages > config_.maximumMissedCleavages)
                        break;

                    int curLength = endNonSpecific_ - beginNonSpecific_;
                    if (curLength > config_.maximumLength)
                        break;
                    if (curLength < config_.minimumLength)
                        continue;

                    foundNextValidSitePair = true;
                    break;
                }

                if (foundNextValidSitePair)
                    break;
            }
        }
    }

    inline Impl& operator++()
    {
        try
        {
            peptide_.reset();

            switch (config_.minimumSpecificity)
            {
                default:
                case FullySpecific:
                    nextFullySpecific();
                    break;

                case SemiSpecific:
                    while (beginNonSpecific_ < (int) sequence_.length())
                    {
                        nextNonSpecific();
                        if ((begin_ != sites_.end() && *begin_ == beginNonSpecific_) ||
                            (end_ != sites_.end() && *end_ == endNonSpecific_))
                            break;
                    }
                    break;

                case NonSpecific:
                    nextNonSpecific();
                    break;
            }
        }
        catch (exception& e)
        {
            throw runtime_error(string("[Digestion::Impl::operator++()] ") + e.what());
        }

        return *this;
    }

    inline Impl operator++(int)
    {
        Impl tmp(*this);
        ++*this;
        return tmp;
    }

    inline bool atEnd()
    {
        switch (config_.minimumSpecificity)
        {
            case SemiSpecific: // TODO: optimize semi-specific
            case NonSpecific:
                return beginNonSpecific_ == (int) sequence_.length();

            case FullySpecific:
            default:
                return begin_ == sites_.end();
        }
    }

    private:
    const Digestion::Impl& digestionImpl_;
    const Config& config_;
    const string& sequence_;
    const vector<int>& sites_;

    // used for all digests
    // fully specific: iterator to the current peptide's N terminal offset-1
    // semi- and non-specific: iterator to the previous valid digestion site before or at beginNonSpecific_
    vector<int>::const_iterator begin_;

    // fully specific: iterator to the current peptide's C terminal offset
    // semi- and non-specific: iterator to the next valid digestion site at or after endNonSpecific_
    vector<int>::const_iterator end_;

    // used for semi- and non-specific digest
    int beginNonSpecific_;
    int endNonSpecific_;

    mutable shared_ptr<DigestedPeptide> peptide_;
    friend class Digestion::const_iterator;
};

PWIZ_API_DECL Digestion::const_iterator::const_iterator()
{
}

PWIZ_API_DECL Digestion::const_iterator::const_iterator(const Digestion& digestion)
:   impl_(new Impl(digestion))
{
}

PWIZ_API_DECL Digestion::const_iterator::const_iterator(const const_iterator& rhs)
:   impl_(rhs.impl_.get() ? new Impl(*rhs.impl_) : 0)
{
}

PWIZ_API_DECL Digestion::const_iterator::~const_iterator()
{
}

PWIZ_API_DECL const DigestedPeptide& Digestion::const_iterator::operator*() const
{
    return impl_->peptide();
}

PWIZ_API_DECL const DigestedPeptide* Digestion::const_iterator::operator->() const
{
    return &(impl_->peptide());
}

PWIZ_API_DECL Digestion::const_iterator& Digestion::const_iterator::operator++()
{
    ++(*impl_);
    return *this;
}

PWIZ_API_DECL Digestion::const_iterator Digestion::const_iterator::operator++(int)
{
    const_iterator tmp(*this);
    ++(*impl_);
    return tmp;
}

PWIZ_API_DECL bool Digestion::const_iterator::operator!=(const Digestion::const_iterator& that) const
{
    return !(*this == that);
}

PWIZ_API_DECL bool Digestion::const_iterator::operator==(const Digestion::const_iterator& that) const
{
    bool gotThis = this->impl_.get() != NULL;
    bool gotThat = that.impl_.get() != NULL;

    if (gotThis && gotThat)
        return this->impl_->begin_ == that.impl_->begin_ &&
               this->impl_->end_ == that.impl_->end_;
    else if (!gotThis && !gotThat) // end() == end()
        return true;
    else if (gotThis)
        return this->impl_->atEnd();
    else // gotThat
        return that.impl_->atEnd();
}

} // namespace proteome
} // namespace pwiz
