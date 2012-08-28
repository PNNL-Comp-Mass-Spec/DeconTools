package uk.ac.ebi.jmzidml.model.utils;

import uk.ac.ebi.jmzidml.model.mzidml.AbstractParam;
import uk.ac.ebi.jmzidml.model.mzidml.CvParam;
import uk.ac.ebi.jmzidml.model.mzidml.UserParam;

import java.lang.reflect.Field;
import java.util.ArrayList;
import java.util.List;

/**
 * @author Florian Reisinger
 *         Date: 18-Oct-2010
 * @since 0.2
 */
public class ParamUpdater {

    private static void updateAbstractParamProperties(AbstractParam input, AbstractParam newParam) throws NoSuchFieldException, IllegalAccessException{
        if(input == null || newParam == null) throw new IllegalArgumentException("Neither input or newParam can be null");
        Field unitRefField = input.getClass().getSuperclass().getDeclaredField("unitCvRef");
        unitRefField.setAccessible(true);
        unitRefField.set(newParam, input.getUnitCvRef());
    }

    public static <T extends CvParam> T updateCvParamSubclass(CvParam input, Class<T> cvParamSubClass) throws IllegalAccessException, InstantiationException {
        if (input != null) {
            // create a tmp holder of the new ParamSubclass
            T newParam = cvParamSubClass.newInstance();
            // copy fields
            newParam.setAccession( input.getAccession() );
            /**
             * Check for null value as otherwise cvRef will be overwritten and set to null.
             */
            if(input.getCv() != null){
                newParam.setCv( input.getCv() );
            } else {
                /**
                 * No set*ref methods is provided so if auto resolving is off there is no way to record the cvParam.
                 * Use reflection to set the value.
                 */
                try{
                    Class cls = input.getClass();
                    Field cvRefField = cls.getDeclaredField("cvRef");
                    cvRefField.setAccessible(true);
                    cvRefField.set(newParam, input.getCvRef());


                } catch(Exception e){
                    throw new InstantiationError("Unable to create new instance of CvParam subclass due to problem updating cvRef.");
                }
               try{
                    ParamUpdater.updateAbstractParamProperties(input, newParam);
                } catch(Exception e){
                    throw new InstantiationError("Unable to create new instance of CvParam subclass due to problem updating superclass properties.");

                }
            }

            if (input.getUnitCv() != null) {
                newParam.setUnitCv(input.getUnitCv());
            } else if (input.getUnitCvRef() != null) {
                // no unitCv object reference! might only be a reference string (not auto-resolving)
                /**
                 * No set*ref methods is provided so if auto resolving is off there is no way to record the cvParam.
                 * Use reflection to set the value.
                 */
                try{
                    Field unitCvRefField = AbstractParam.class.getDeclaredField("unitCvRef");
                    unitCvRefField.setAccessible(true);
                    unitCvRefField.set(newParam, input.getUnitCvRef());
                } catch(Exception e){
                    throw new InstantiationError("Unable to create new instance of CvParam subclass due to problem updating unitCvRef.");
                }
               try{
                    ParamUpdater.updateAbstractParamProperties(input, newParam);
                }catch(Exception e){
                    throw new InstantiationError("Unable to create new instance of CvParam subclass due to problem updating superclass properties.");

                }
            } else {
                // no unitCV reference given: not updating!
            }

            newParam.setName( input.getName() );
            newParam.setValue( input.getValue() );
            newParam.setUnitAccession( input.getUnitAccession() );
            newParam.setUnitName( input.getUnitName() );

            // replace old with new
            return newParam;
        }
        return null;
    }

    public static <T extends CvParam> void updateCvParamSubclassesList(List<CvParam> inputs, Class<T> cvParamSubClass)
            throws InstantiationException, IllegalAccessException {
        if (inputs != null && inputs.size() > 0) {
            // create tmp holder
            List<T> newList = new ArrayList<T>();

            // copy values to new SubClasses
            for (CvParam cvParam : inputs) {
                newList.add(updateCvParamSubclass(cvParam, cvParamSubClass));
            }

            // switch list content
            //inputs.clear();
            inputs.removeAll(inputs);
            inputs.addAll(newList);
        }
    }


    public static <T extends UserParam> T updateUserParamSubclass(UserParam input, Class<T> userParamSubClass)
            throws InstantiationException, IllegalAccessException {
        if (input != null) {
            // create a tmp holder of the new ParamSubclass
            T newParam = userParamSubClass.newInstance();
            try{
                ParamUpdater.updateAbstractParamProperties(input, newParam);
            }catch(Exception e){
                throw new InstantiationException("Unable to create new instance of UserParam subclass due to problem updating superclass properties.");
            }
            // copy fields
            newParam.setName(input.getName());
            newParam.setValue( input.getValue() );
            newParam.setUnitAccession( input.getUnitAccession() );
            newParam.setUnitName( input.getUnitName() );
            // replace old with new
            return newParam;
        }
        return null;
    }

    public static <T extends UserParam> void updateUserParamSubclassesList(List<UserParam> inputs, Class<T> userParamSubClass) throws InstantiationException, IllegalAccessException {
        if (inputs != null && inputs.size() > 0) {
            // create tmp holder
            List<T> newList = new ArrayList<T>();

            // copy values to new SubClasses
            for (UserParam userParam : inputs) {
                newList.add(updateUserParamSubclass(userParam, userParamSubClass));
            }

            // switch list content
            //inputs.clear();
            inputs.removeAll(inputs);
            inputs.addAll(newList);
        }
    }



}