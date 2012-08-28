/*
 * Date: 22/7/2008
 * Author: rcote
 * File: uk.ac.ebi.jmzml.xml.jaxb.adapters.CVAdapter
 *
 * jmzml is Copyright 2008 The European Bioinformatics Institute
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 *
 *
 */

package uk.ac.ebi.jmzidml.xml.jaxb.adapters;

import uk.ac.ebi.jmzidml.model.mzidml.Cv;
import uk.ac.ebi.jmzidml.xml.Constants;
import uk.ac.ebi.jmzidml.xml.jaxb.unmarshaller.cache.AdapterObjectCache;
import uk.ac.ebi.jmzidml.xml.xxindex.MzIdentMLIndexer;

public class CvAdapter extends AbstractResolvingAdapter<String, Cv> {

    public CvAdapter(MzIdentMLIndexer index, AdapterObjectCache cache) {
        super(index, cache);
    }

    public Cv unmarshal(String refId) {
        Cv retval;
        if (cache.getCachedObject(refId, Cv.class) != null) {
            retval = (Cv) cache.getCachedObject(refId, Cv.class);
            logger.debug("used cached value for ID: " + refId);
        } else {
            retval = super.unmarshal(refId, Constants.ReferencedType.CV);
            cache.putInCache(refId, retval);
            logger.debug("cached object at ID: " + refId);
        }
        return retval;
    }

    public String marshal(Cv cv) {
        if (cv != null) {
            return cv.getId();
        } else {
            return null;
        }
    }

}
