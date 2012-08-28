/*
 * Date: 22/7/2008
 * Author: rcote
 * File: uk.ac.ebi.jmzml.model.mzml.utilities.ModelConstants
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

package uk.ac.ebi.jmzidml.model.utils;

import uk.ac.ebi.jmzidml.MzIdentMLElement;
import uk.ac.ebi.jmzidml.model.mzidml.MzIdentML;

import javax.xml.namespace.QName;
import java.util.Collections;
import java.util.HashMap;
import java.util.Map;

/**
 * @author Richard Cote
 * @author Florian Reisinger
 *         Date: 13-Jun-2008
 *         Time: 10:45:32
 *         $Id: $
 */
public class ModelConstants {

    public static final String PACKAGE = MzIdentML.class.getPackage().getName();
    public static final String MZIDML_NS = "http://psidev.info/psi/pi/mzIdentML/1.1";
    public static final String MZIDML_VERSION = "1.1.0";
    public static final String MZIDML_SCHEMA = "http://www.psidev.info/files/mzIdentML1.1.0.xsd";

    private static Map<Class, QName> modelQNames = new HashMap<Class, QName>();

    static {
        for (MzIdentMLElement element : MzIdentMLElement.values()) {
            if (element.getTagName() != null) {
                modelQNames.put(element.getClazz(), new QName(MZIDML_NS, element.getTagName()));
            }
        }
        //now make set unmodifiable
        modelQNames = Collections.unmodifiableMap(modelQNames);

    }

    public static boolean isRegisteredClass(Class cls) {
        return modelQNames.containsKey(cls);
    }

    public static QName getQNameForClass(Class cls) {
        if (isRegisteredClass(cls)) {
            return modelQNames.get(cls);
        } else {
            throw new IllegalStateException("No QName registered for class: " + cls);
        }
    }

    public static String getElementNameForClass(Class cls) {
        if (isRegisteredClass(cls)) {
            return modelQNames.get(cls).getLocalPart();
        } else {
            throw new IllegalStateException("No QName registered for class: " + cls);
        }
    }

    public static Class getClassForElementName(String name) {
        for (Map.Entry<Class, QName> entry : modelQNames.entrySet()) {
            if (entry.getValue().getLocalPart().equals(name)) {
                return entry.getKey();
            }
        }
        return null;
    }

}
