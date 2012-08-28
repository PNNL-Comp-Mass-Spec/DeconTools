//
// $Id: IdentDataTest.cpp 2824 2011-06-29 18:37:47Z chambm $
//
//
// Original author: Robert Burke <robert.burke@proteowizard.org>
//
// Copyright 2009 Spielberg Family Center for Applied Proteomics
//   University of Southern California, Los Angeles, California  90033
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

#include "pwiz/utility/misc/unit.hpp"
#include "pwiz/utility/misc/Std.hpp"
#include "pwiz/data/common/Unimod.hpp"
#include "IdentData.hpp"
#include "Serializer_mzid.hpp"
#include "examples.hpp"
#include "Diff.hpp"
#include "TextWriter.hpp"


using namespace pwiz::identdata;
using namespace pwiz::identdata::examples;
using namespace pwiz::util;
using namespace pwiz::data;
namespace proteome = pwiz::proteome;


ostream* os_;


void testDigestedPeptides()
{
    using namespace pwiz::proteome;

    IdentData mzid;
    initializeBasicSpectrumIdentification(mzid);

    SpectrumIdentificationProtocolPtr sip = mzid.analysisProtocolCollection.spectrumIdentificationProtocol[0];
    SpectrumIdentificationListPtr sil = mzid.dataCollection.analysisData.spectrumIdentificationList[0];

    SpectrumIdentificationResultPtr result2 = sil->spectrumIdentificationResult[1];

    // test with multiple simultaneous enzymes (Lys-C/P and Arg-C)
    {
        // result 2 rank 1: K.QTQTFTTYSDNQPGVLIQVYEGER.A

        SpectrumIdentificationItemPtr result2_rank1 = result2->spectrumIdentificationItem[0];

        // both termini are specific now, one cut from each enzyme
        vector<DigestedPeptide> result2_rank1_digestedPeptides = digestedPeptides(*sip, *result2_rank1);
        unit_assert(result2_rank1_digestedPeptides.size() == 1);
        unit_assert(result2_rank1_digestedPeptides[0] == digestedPeptide(*sip, *result2_rank1->peptideEvidencePtr[0]));
        unit_assert(result2_rank1_digestedPeptides[0].missedCleavages() == 0);
        unit_assert(result2_rank1_digestedPeptides[0].specificTermini() == 2);
        unit_assert(result2_rank1_digestedPeptides[0].NTerminusIsSpecific());
        unit_assert(result2_rank1_digestedPeptides[0].CTerminusIsSpecific());
    }
    
    // test with multiple independent enzymes (Lys-C/P and Arg-C)
    sip->enzymes.independent = true;
    {
        // result 2 rank 1: K.QTQTFTTYSDNQPGVLIQVYEGER.A
        
        SpectrumIdentificationItemPtr result2_rank1 = result2->spectrumIdentificationItem[0];

        // reassign the original prefix residue
        result2_rank1->peptideEvidencePtr[0]->pre = 'K';

        // there are two semi-specific peptides, one cut by Lys-C and the other cut by Arg-C;
        // only the first one will be returned because they have the same "best specificity"

        vector<DigestedPeptide> result2_rank1_digestedPeptides = digestedPeptides(*sip, *result2_rank1);
        unit_assert(result2_rank1_digestedPeptides.size() == 1);
        unit_assert(result2_rank1_digestedPeptides[0] == digestedPeptide(*sip, *result2_rank1->peptideEvidencePtr[0]));
        unit_assert(result2_rank1_digestedPeptides[0].missedCleavages() == 0);
        unit_assert(result2_rank1_digestedPeptides[0].specificTermini() == 1);
        unit_assert(result2_rank1_digestedPeptides[0].NTerminusIsSpecific());
        unit_assert(!result2_rank1_digestedPeptides[0].CTerminusIsSpecific());
    }

    // change from multiple enzymes to trypsin/p and test again
    sip->enzymes.enzymes.clear();
    EnzymePtr trypsin(new Enzyme);
    trypsin->id = "ENZ_1";
    trypsin->cTermGain = "OH";
    trypsin->nTermGain = "H";
    trypsin->missedCleavages = 2;
    trypsin->minDistance = 1;
    trypsin->terminalSpecificity = proteome::Digestion::FullySpecific;
    trypsin->siteRegexp = "(?<=[KR])";
    trypsin->enzymeName.set(MS_Trypsin_P);
    sip->enzymes.enzymes.push_back(trypsin);

    {
        // result 2 rank 1: K.QTQTFTTYSDNQPGVLIQVYEGER.A
        SpectrumIdentificationItemPtr result2_rank1 = result2->spectrumIdentificationItem[0];
        vector<DigestedPeptide> result2_rank1_digestedPeptides = digestedPeptides(*sip, *result2_rank1);
        unit_assert(result2_rank1_digestedPeptides.size() == 1);
        unit_assert(result2_rank1_digestedPeptides[0] == digestedPeptide(*sip, *result2_rank1->peptideEvidencePtr[0]));
        unit_assert(result2_rank1_digestedPeptides[0].offset() == 423);
        unit_assert(result2_rank1_digestedPeptides[0].missedCleavages() == 0);
        unit_assert(result2_rank1_digestedPeptides[0].specificTermini() == 2);
        unit_assert(result2_rank1_digestedPeptides[0].NTerminusIsSpecific());
        unit_assert(result2_rank1_digestedPeptides[0].CTerminusIsSpecific());
        unit_assert(result2_rank1_digestedPeptides[0].NTerminusPrefix() == "K");
        unit_assert(result2_rank1_digestedPeptides[0].CTerminusSuffix() == "A");

        // result 2 rank 2: K.RNSTIPT.K
        SpectrumIdentificationItemPtr result2_rank2 = result2->spectrumIdentificationItem[1];
        vector<DigestedPeptide> result2_rank2_digestedPeptides = digestedPeptides(*sip, *result2_rank2);
        unit_assert(result2_rank2_digestedPeptides.size() == 2);

        // both PeptideEvidences have the same values
        for (int i=0; i < 2; ++i)
        {
            unit_assert(result2_rank2_digestedPeptides[i] == digestedPeptide(*sip, *result2_rank2->peptideEvidencePtr[i]));
            unit_assert(result2_rank2_digestedPeptides[i].offset() == 415);
            unit_assert(result2_rank2_digestedPeptides[i].missedCleavages() == 1);
            unit_assert(result2_rank2_digestedPeptides[i].specificTermini() == 1);
            unit_assert(result2_rank2_digestedPeptides[i].NTerminusIsSpecific());
            unit_assert(!result2_rank2_digestedPeptides[i].CTerminusIsSpecific());
            unit_assert(result2_rank2_digestedPeptides[i].NTerminusPrefix() == "K");
            unit_assert(result2_rank2_digestedPeptides[i].CTerminusSuffix() == "K");
        }
    }

    // change enzyme from trypsin to Lys-C and test again
    sip->enzymes.enzymes[0]->siteRegexp = "(?<=K)";

    {
        // result 2 rank 1: K.QTQTFTTYSDNQPGVLIQVYEGER.A
        SpectrumIdentificationItemPtr result2_rank1 = result2->spectrumIdentificationItem[0];
        vector<DigestedPeptide> result2_rank1_digestedPeptides = digestedPeptides(*sip, *result2_rank1);
        unit_assert(result2_rank1_digestedPeptides.size() == 1);
        unit_assert(result2_rank1_digestedPeptides[0] == digestedPeptide(*sip, *result2_rank1->peptideEvidencePtr[0]));
        unit_assert(result2_rank1_digestedPeptides[0].missedCleavages() == 0);
        unit_assert(result2_rank1_digestedPeptides[0].specificTermini() == 1);
        unit_assert(result2_rank1_digestedPeptides[0].NTerminusIsSpecific());
        unit_assert(!result2_rank1_digestedPeptides[0].CTerminusIsSpecific());

        // result 2 rank 2: K.RNSTIPT.K
        SpectrumIdentificationItemPtr result2_rank2 = result2->spectrumIdentificationItem[1];
        vector<DigestedPeptide> result2_rank2_digestedPeptides = digestedPeptides(*sip, *result2_rank2);
        unit_assert(result2_rank2_digestedPeptides.size() == 2);

        // both PeptideEvidences have the same values
        for (int i=0; i < 2; ++i)
        {
            unit_assert(result2_rank2_digestedPeptides[i] == digestedPeptide(*sip, *result2_rank2->peptideEvidencePtr[i]));
            unit_assert(result2_rank2_digestedPeptides[i].missedCleavages() == 0);
            unit_assert(result2_rank2_digestedPeptides[i].specificTermini() == 1);
            unit_assert(result2_rank2_digestedPeptides[i].NTerminusIsSpecific());
            unit_assert(!result2_rank2_digestedPeptides[i].CTerminusIsSpecific());
        }
    }

    // change enzyme from Lys-C to Lys-N and test again
    sip->enzymes.enzymes[0]->siteRegexp = "(?=K)";

    {
        // result 2 rank 1: K.QTQTFTTYSDNQPGVLIQVYEGER.A
        SpectrumIdentificationItemPtr result2_rank1 = result2->spectrumIdentificationItem[0];
        vector<DigestedPeptide> result2_rank1_digestedPeptides = digestedPeptides(*sip, *result2_rank1);
        unit_assert(result2_rank1_digestedPeptides.size() == 1);
        unit_assert(result2_rank1_digestedPeptides[0] == digestedPeptide(*sip, *result2_rank1->peptideEvidencePtr[0]));
        unit_assert(result2_rank1_digestedPeptides[0].missedCleavages() == 0);
        unit_assert(result2_rank1_digestedPeptides[0].specificTermini() == 0);
        unit_assert(!result2_rank1_digestedPeptides[0].NTerminusIsSpecific());
        unit_assert(!result2_rank1_digestedPeptides[0].CTerminusIsSpecific());

        // result 2 rank 2: K.RNSTIPT.K
        SpectrumIdentificationItemPtr result2_rank2 = result2->spectrumIdentificationItem[1];
        vector<DigestedPeptide> result2_rank2_digestedPeptides = digestedPeptides(*sip, *result2_rank2);
        unit_assert(result2_rank2_digestedPeptides.size() == 2);

        // both PeptideEvidences have the same values
        for (int i=0; i < 2; ++i)
        {
            unit_assert(result2_rank2_digestedPeptides[i] == digestedPeptide(*sip, *result2_rank2->peptideEvidencePtr[i]));
            unit_assert(result2_rank2_digestedPeptides[i].missedCleavages() == 0);
            unit_assert(result2_rank2_digestedPeptides[i].specificTermini() == 1);
            unit_assert(!result2_rank2_digestedPeptides[i].NTerminusIsSpecific());
            unit_assert(result2_rank2_digestedPeptides[i].CTerminusIsSpecific());
        }
    }

    {
        // result 2 rank 1: K.QTQTFTTYSDNQPGVLIQVYEGER.A
        
        SpectrumIdentificationItemPtr result2_rank1 = result2->spectrumIdentificationItem[0];

        // move it to the C terminus
        result2_rank1->peptideEvidencePtr[0]->start = 618;
        result2_rank1->peptideEvidencePtr[0]->post = '-';

        vector<DigestedPeptide> result2_rank1_digestedPeptides = digestedPeptides(*sip, *result2_rank1);
        unit_assert(result2_rank1_digestedPeptides.size() == 1);
        unit_assert(result2_rank1_digestedPeptides[0] == digestedPeptide(*sip, *result2_rank1->peptideEvidencePtr[0]));
        unit_assert(result2_rank1_digestedPeptides[0].offset() == 617);
        unit_assert(result2_rank1_digestedPeptides[0].missedCleavages() == 0);
        unit_assert(result2_rank1_digestedPeptides[0].specificTermini() == 1);
        unit_assert(!result2_rank1_digestedPeptides[0].NTerminusIsSpecific());
        unit_assert(result2_rank1_digestedPeptides[0].CTerminusIsSpecific());
        unit_assert(result2_rank1_digestedPeptides[0].NTerminusPrefix() == "K");
        unit_assert(result2_rank1_digestedPeptides[0].CTerminusSuffix() == "-");

        // move it to the N terminus
        result2_rank1->peptideEvidencePtr[0]->start = 1;
        result2_rank1->peptideEvidencePtr[0]->pre = '-';
        result2_rank1->peptideEvidencePtr[0]->post = 'A';

        result2_rank1_digestedPeptides = digestedPeptides(*sip, *result2_rank1);
        unit_assert(result2_rank1_digestedPeptides.size() == 1);
        unit_assert(result2_rank1_digestedPeptides[0] == digestedPeptide(*sip, *result2_rank1->peptideEvidencePtr[0]));
        unit_assert(result2_rank1_digestedPeptides[0].offset() == 0);
        unit_assert(result2_rank1_digestedPeptides[0].missedCleavages() == 0);
        unit_assert(result2_rank1_digestedPeptides[0].specificTermini() == 1);
        unit_assert(result2_rank1_digestedPeptides[0].NTerminusIsSpecific());
        unit_assert(!result2_rank1_digestedPeptides[0].CTerminusIsSpecific());
        unit_assert(result2_rank1_digestedPeptides[0].NTerminusPrefix() == "-");
        unit_assert(result2_rank1_digestedPeptides[0].CTerminusSuffix() == "A");
    }
}

void testSnapModifications()
{
    IdentData mzid, mzid2;
    initializeBasicSpectrumIdentification(mzid);
    initializeBasicSpectrumIdentification(mzid2);

    BOOST_FOREACH(SpectrumIdentificationProtocolPtr& sip, mzid2.analysisProtocolCollection.spectrumIdentificationProtocol)
    BOOST_FOREACH(SearchModificationPtr& mod, sip->modificationParams)
        mod->cvParams.clear();

    BOOST_FOREACH(PeptidePtr& pep, mzid2.sequenceCollection.peptides)
    BOOST_FOREACH(ModificationPtr& mod, pep->modification)
        mod->cvParams.clear();

    Diff<IdentData, DiffConfig> diff(mzid, mzid2);
    unit_assert(diff);

    BOOST_FOREACH(SpectrumIdentificationPtr& si, mzid2.analysisCollection.spectrumIdentification)
        snapModificationsToUnimod(*si);

    diff(mzid, mzid2);
    if (diff && os_) *os_ << "diff:\n" << diff_string<TextWriter>(diff) << endl;
    unit_assert(!diff);
}

void testConversion()
{
    using proteome::ModificationMap;

    IdentData mzid;
    initializeBasicSpectrumIdentification(mzid);

    // PEP_2: TAIGIDLGT[80]TYSC[57]VGVFQHGK
    proteome::Peptide pep2 = peptide(*mzid.sequenceCollection.peptides[1]);
    unit_assert_operator_equal("TAIGIDLGTTYSCVGVFQHGK", pep2.sequence());
    unit_assert_operator_equal(2, pep2.modifications().size());
    unit_assert_operator_equal(1, pep2.modifications().count(8));
    unit_assert_operator_equal(unimod::modification(UNIMOD_Phospho).deltaMonoisotopicMass(),
                               pep2.modifications().find(8)->second.monoisotopicDeltaMass());
    unit_assert_operator_equal(1, pep2.modifications().count(12));
    unit_assert_operator_equal(unimod::modification(UNIMOD_Carbamidomethyl).deltaMonoisotopicMass(),
                               pep2.modifications().find(12)->second.monoisotopicDeltaMass());

    // PEP_5: RNS[80]TIPT[-1]
    proteome::Peptide pep5 = peptide(*mzid.sequenceCollection.peptides[4]);
    unit_assert_operator_equal("RNSTIPT", pep5.sequence());
    unit_assert_operator_equal(2, pep5.modifications().size());
    unit_assert_operator_equal(1, pep5.modifications().count(2));
    unit_assert_operator_equal(unimod::modification(UNIMOD_Phospho).deltaMonoisotopicMass(),
                               pep5.modifications().find(2)->second.monoisotopicDeltaMass());
    unit_assert_operator_equal(1, pep5.modifications().count(ModificationMap::CTerminus()));
    unit_assert_operator_equal(unimod::modification(UNIMOD_Amidated).deltaMonoisotopicMass(),
                               pep5.modifications().find(ModificationMap::CTerminus())->second.monoisotopicDeltaMass());
}

void testCleavageAgent()
{
    {
        Enzyme ez;
        ez.enzymeName.set(MS_Trypsin_P);
        unit_assert_operator_equal(MS_Trypsin_P, cleavageAgent(ez));
    }

    {
        Enzyme ez;
        ez.enzymeName.userParams.push_back(UserParam("trypsin/p"));
        unit_assert_operator_equal(MS_Trypsin_P, cleavageAgent(ez));
    }

    {
        Enzyme ez;
        ez.name = "trypsin/p";
        unit_assert_operator_equal(MS_Trypsin_P, cleavageAgent(ez));
    }

    {
        Enzyme ez;
        ez.siteRegexp = "(?<=[KR])(?!P)";
        unit_assert_operator_equal(MS_Trypsin, cleavageAgent(ez));
    }
}


int main(int argc, char** argv)
{
    if (argc>1 && !strcmp(argv[1],"-v")) os_ = &cout;
    if (os_) *os_ << "MzIdentMLTest\n";

    try
    {
        testDigestedPeptides();
        testSnapModifications();
        testConversion();
        testCleavageAgent();
        return 0;
    }
    catch (exception& e)
    {
        cerr << e.what() << endl;
    }
    catch (...)
    {
        cerr << "Caught unknown exception.\n";
    }

    return 1;
}
