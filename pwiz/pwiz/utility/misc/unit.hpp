//
// $Id: unit.hpp 2586 2011-03-22 20:09:17Z chambm $
//
//
// Original author: Darren Kessner <darren@proteowizard.org>
//
// Copyright 2006 Louis Warschaw Prostate Cancer Center
//   Cedars Sinai Medical Center, Los Angeles, California  90048
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


#ifndef _UNIT_HPP_
#define _UNIT_HPP_


#include "Exception.hpp"
#include <string>
#include <sstream>
#include <cmath>


namespace pwiz {
namespace util {


//
// These are assertion macros for unit testing.  They throw a runtime_error 
// exception on failure, instead of calling abort(), allowing the application
// to recover and return an appropriate error value to the shell.
//
// unit_assert(x):                             asserts x is true
// unit_assert_equal(x, y, epsilon):           asserts x==y, within epsilon
// unit_assert_matrices_equal(A, B, epsilon):  asserts A==B, within epsilon
//


inline std::string unit_assert_message(const char* filename, int line, const char* expression)
{
    std::ostringstream oss;
    oss << "[" << filename << ":" << line << "] Assertion failed: " << expression;
    return oss.str();
}

inline std::string unit_assert_equal_message(const char* filename, int line, const std::string& x, const std::string& y, const char* expression)
{
    std::ostringstream oss;
    oss << "[" << filename << ":" << line << "] Assertion failed: expected \"" << x << "\" but got \"" << y << "\" (" << expression << ")";
    return oss.str();
}

inline std::string unit_assert_numeric_equal_message(const char* filename, int line, double x, double y, double epsilon)
{
    std::ostringstream oss;
    oss.precision(10);
    oss << "[" << filename << ":" << line << "] Assertion failed: |" << x << " - " << y << "| < " << epsilon;
    return oss.str();
}

inline std::string unit_assert_exception_message(const char* filename, int line, const char* expression, const std::string& exception)
{
    std::ostringstream oss;
    oss << "[" << filename << ":" << line << "] Assertion failed to throw \"" << exception << "\": " << expression;
    return oss.str();
}


#define unit_assert(x) \
    (!(x) ? throw std::runtime_error(unit_assert_message(__FILE__, __LINE__, #x)) : 0) 


#define unit_assert_operator_equal(expected, actual) \
    (!(expected == actual) ? throw std::runtime_error(unit_assert_equal_message(__FILE__, __LINE__, lexical_cast<string>(expected), lexical_cast<string>(actual), #actual)) : 0)


#define unit_assert_equal(x, y, epsilon) \
    (!(fabs((x)-(y)) <= (epsilon)) ? throw std::runtime_error(unit_assert_numeric_equal_message(__FILE__, __LINE__, (x), (y), (epsilon))) : 0)


#define unit_assert_throws(x, exception) \
    { \
        bool threw = false; \
        try { (x); } \
        catch (exception&) \
        { \
            threw = true; \
        } \
        if (!threw) \
            throw std::runtime_error(unit_assert_exception_message(__FILE__, __LINE__, #x, #exception)); \
    }


#define unit_assert_throws_what(x, exception, whatStr) \
    { \
        bool threw = false; \
        try { (x); } \
        catch (exception& e) \
        { \
            if (e.what() == std::string(whatStr)) \
                threw = true; \
            else \
                throw std::runtime_error(unit_assert_exception_message(__FILE__, __LINE__, #x, std::string(#exception)+" "+(whatStr)+"\nBut a different exception was thrown: ")+(e.what())); \
        } \
        if (!threw) \
            throw std::runtime_error(unit_assert_exception_message(__FILE__, __LINE__, #x, std::string(#exception)+" "+(whatStr))); \
    }


#define unit_assert_matrices_equal(A, B, epsilon) \
    unit_assert(boost::numeric::ublas::norm_frobenius((A)-(B)) < (epsilon))


#define unit_assert_vectors_equal(A, B, epsilon) \
    unit_assert(boost::numeric::ublas::norm_2((A)-(B)) < (epsilon))


} // namespace util
} // namespace pwiz


#endif // _UNIT_HPP_

