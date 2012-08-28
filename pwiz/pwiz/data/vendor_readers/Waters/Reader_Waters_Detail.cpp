//
// $Id: Reader_Waters_Detail.cpp 2485 2011-01-31 16:17:29Z chambm $
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

#include "Reader_Waters_Detail.hpp"
#include "pwiz/utility/misc/Container.hpp"
#include "pwiz/utility/misc/String.hpp"

namespace pwiz {
namespace msdata {
namespace detail {


PWIZ_API_DECL
vector<InstrumentConfiguration> createInstrumentConfigurations(RawDataPtr rawdata)
{
    return vector<InstrumentConfiguration>();
}


PWIZ_API_DECL CVID translateAsInstrumentModel(RawDataPtr rawdata)
{
    return CVID_Unknown;
}


PWIZ_API_DECL
void translateFunctionType(PwizFunctionType functionType,
                           int& msLevel,
                           CVID& spectrumType)
{
    switch (functionType)
    {
        /*case FunctionType_MSMSMS:
            msLevel = 3;
            spectrumType = MS_MSn_spectrum;
            break;*/

        case FunctionType_Daughters:
        //case FunctionType_MSMS:
        case FunctionType_MS2:
        case FunctionType_TOF_Daughter:
        case FunctionType_Auto_Daughters:
            msLevel = 2;
            spectrumType = MS_MSn_spectrum;
            break;

        case FunctionType_SIR:
            msLevel = 1;
            spectrumType = MS_SIM_spectrum;

        case FunctionType_MRM:
        case FunctionType_AutoSpec_MRM:
        case FunctionType_AutoSpec_Q_MRM_Quad:
        case FunctionType_AutoSpec_MIKES_Scan:
            msLevel = 2;
            spectrumType = MS_SRM_spectrum;
            break;

            
        case FunctionType_Neutral_Loss:
            msLevel = 2;
            spectrumType = MS_constant_neutral_loss_spectrum;
            break;

        case FunctionType_Neutral_Gain:
            msLevel = 2;
            spectrumType = MS_constant_neutral_gain_spectrum;
            break;

        case FunctionType_Parents:
        case FunctionType_Scan:
        case FunctionType_Q1F:
        case FunctionType_TOF:
        case FunctionType_TOF_MS:
        case FunctionType_TOF_Survey:
        case FunctionType_TOF_Parent:
        case FunctionType_MALDI_TOF:
            msLevel = 1;
            spectrumType = MS_MS1_spectrum;
            break;

        /* TODO: figure out what these function types translate to
            FunctionType_Delay
            FunctionType_Concatenated
            FunctionType_Off
            FunctionType_Diode_Array
            FunctionType_TOF_PSD
            FunctionType_Voltage_Scan
            FunctionType_Magnetic_Scan
            FunctionType_Voltage_SIR
            FunctionType_Magnetic_SIR
            FunctionType_AutoSpec_B_E_Scan
            FunctionType_AutoSpec_B2_E_Scan
            FunctionType_AutoSpec_CNL_Scan
            FunctionType_AutoSpec_MIKES_Scan
            FunctionType_AutoSpec_NRMS_Scan
        */

        default:
            throw std::runtime_error("[translateFunctionType] Unable to translate function type.");
    }
}


PWIZ_API_DECL CVID translateAsIonizationType(PwizIonizationType ionizationType)
{
    /*switch (ionizationType)
    {
        case IonizationType_EI = 0,       // Electron Ionization
        case IonizationType_CI,           // Chemical Ionization
        case IonizationType_FB,           // Fast Atom Bombardment
        case IonizationType_TS,           // Thermospray
        case IonizationType_ES,           // Electrospray Ionization
        case IonizationType_AI,           // Atmospheric Ionization
        case IonizationType_LD,           // Laser Desorption Ionization
        case IonizationType_FI,           // ?
        case IonizationType_Generic,
        case IonizationType_Count*/
    return CVID_Unknown;
}


PWIZ_API_DECL CVID translate(PwizPolarityType polarityType)
{
    switch (polarityType)
    {
        case PolarityType_Positive: return MS_positive_scan;
        case PolarityType_Negative: return MS_negative_scan;
        default: return CVID_Unknown;
    }
}


} // detail
} // msdata
} // pwiz
