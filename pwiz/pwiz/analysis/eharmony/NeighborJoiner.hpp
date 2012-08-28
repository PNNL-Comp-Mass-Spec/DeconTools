//
// $Id: NeighborJoiner.hpp 1539 2009-11-19 20:12:28Z khoff $
//
//
// Original author: Kate Hoff <katherine.hoff@proteowizard.org>
//
// Copyright 2009 Center for Applied Molecular Medicine
//   University of Southern California, Los Angeles, CA
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

///
/// NeighborJoiner.hpp
///

#ifndef _NEIGHBORJOINER_HPP
#define _NEIGHBORJOINER_HPP

#include "Matrix.hpp"
#include "AMTContainer.hpp"
#include "DistanceAttributes.hpp"

namespace pwiz{
namespace eharmony{

typedef AMTContainer Entry;

struct NeighborJoiner : public Matrix
{     
    NeighborJoiner(const vector<boost::shared_ptr<Entry> >& entries, const WarpFunctionEnum& wfe = Default);

    void addDistanceAttribute(boost::shared_ptr<DistanceAttribute> attr) { _attributes.push_back(attr); }
    void calculateDistanceMatrix();
    void joinNearest();
    void joinAll() { while (_rowEntries.size() > 1) joinNearest(); }

    vector<Entry > _rowEntries;
    vector<Entry > _columnEntries; 

    vector<boost::shared_ptr<DistanceAttribute> > _attributes;
    vector<pair<int, int> > _tree; // stores the row/column indices of the merge at each step
    
    WarpFunctionEnum _wfe;

};

}
}


#endif // _NEIGHBORJOINER_HPP_
