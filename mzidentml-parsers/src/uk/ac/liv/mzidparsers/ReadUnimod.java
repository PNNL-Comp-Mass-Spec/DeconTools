package uk.ac.liv.mzidparsers;

/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */

import uk.ac.liv.unimod.ModT;
import uk.ac.liv.unimod.SpecificityT;
import uk.ac.liv.unimod.CompositionT;
import uk.ac.liv.unimod.UnimodT;
import uk.ac.liv.unimod.ObjectFactory;
import uk.ac.liv.unimod.ModificationsT;
import java.util.*;
import javax.xml.bind.*;
import uk.ac.liv.unimod.*;
import java.io.*;


/**
 *
 * @author jonesar
 */
public class ReadUnimod {


    private ObjectFactory of;
    private UnimodT unimodType;

    private static String inputUnimod =  "/resources/unimod.xml";
    private List<ModT> modList;



    public ReadUnimod(){
        try{
        	//Use the getResourceAsStream trick to read the unimod.xml file as  
        	//a classpath resource. This enables us to also distribute the unimod.xml file
        	//inside the .jar file which simplifies usage of the solution as no extra 
        	//classpath or path configurations are needed to let the code below find
        	//the unimod.xml file: 
            InputStream stream = this.getClass().getResourceAsStream(inputUnimod);
            UnimodT unimod = unmarshal(UnimodT.class,stream);

            ModificationsT mods = unimod.getModifications();

            modList = mods.getMod();

            /*
            for (ModT mod : modList) {

                
                    Long id = mod.getRecordId();
                    String modName  = mod.getTitle();

                    CompositionT comp = mod.getDelta();
                    Double mass = comp.getMonoMass();


                System.out.println(id + " " + modName + " " + mass);

            }
            */

        }
        catch(Exception e){
            e.printStackTrace();

        }
        

    }

    
    public ModT getModByMass(Double testMass, Double massError, Boolean isMono, char res){

        
        
        ModT foundMod = null;
        Boolean isFound = false;
        
        String residue = ""+res;
        
        if(residue.equals("[")){
            residue = "N-term";
        }
        else if(residue.equals("]")){
            residue = "C-term";
        }

        //System.out.println("Searching for " + testMass + " @ " + res);
        
        try{

            for (ModT mod : modList) {

                Double diffFound = 1000000.0;   //Choose smallest mass difference
                Long id = mod.getRecordId();
                String modName  = mod.getTitle();

                CompositionT comp = mod.getDelta();
                Double mass;
                
                if(isMono){
                    mass = comp.getMonoMass();
                }
                else{
                    mass = comp.getAvgeMass();
                }

                boolean siteMatch = false;
                for(SpecificityT spec : mod.getSpecificity()){

                    String site = spec.getSite();
                    
                    if(site.equals(""+residue)){
                        siteMatch=true;
                    }
                }

                if(mass < testMass + massError && mass > testMass - massError && siteMatch){
                    //System.out.println("found: " + id + " " + modName + " " + mass);
                    if(Math.abs(mass - testMass) < diffFound){
                        //System.out.println("Error: Multiple mods found with same mass, choosi: " + testMass);
                        foundMod = mod; 
                        diffFound = Math.abs(mass - testMass);
                    }
                    isFound = true;
                }
            }

        }
        catch(Exception e){
            
            e.printStackTrace();
        }

        return foundMod;

    }

    public ModT getModByMass(Double testMass, Double massError, Boolean isMono, Vector<String> residues){

        ModT foundMod = null;
        Boolean isFound = false;
        
        //System.out.println("testMass: " + testMass + "error mass window: " + massError);

        try{

            for (ModT mod : modList) {
                
                Double diffFound = 1000000.0;   //Choose smallest mass difference


                Long id = mod.getRecordId();
                String modName  = mod.getTitle();

                CompositionT comp = mod.getDelta();
                Double mass;

                if(isMono){
                    mass = comp.getMonoMass();
                }
                else{
                    mass = comp.getAvgeMass();
                }

                boolean siteMatch = false;
                
                
                
                for(SpecificityT spec : mod.getSpecificity()){
                    
                    for(String residue : residues){
                        
                        if(residue.equals("[")){
                            residue = "N-term";
                        }
                        else if(residue.equals("]")){
                            residue = "C-term";
                        }
                        
                        String site = spec.getSite();
                        //System.out.print("site: "+ site);
                        if(site.equals(""+residue)){
                            siteMatch=true;
                        }
                    }
                }

                if(mass < testMass + massError && mass > testMass - massError && siteMatch){                    
                    if(Math.abs(mass - testMass) < diffFound){
                        //System.out.println("Error: Multiple mods found with same mass, choosi: " + testMass);
                        foundMod = mod; 
                        diffFound = Math.abs(mass - testMass);
                    }
                    isFound = true;
                }               
            }
            
            if(!isFound){
                System.out.println("No mod found in Uniod with mass:" + testMass + " error: " + massError);
            }

        }
        catch(Exception e){

            e.printStackTrace();
        }

        return foundMod;

    }


    public <T> T unmarshal( Class<T> docClass, InputStream inputStream )
        throws JAXBException {
        String packageName = docClass.getPackage().getName();
        JAXBContext jc = JAXBContext.newInstance( packageName );
        Unmarshaller u = jc.createUnmarshaller();
        JAXBElement<T> doc = (JAXBElement<T>)u.unmarshal( inputStream );
        return doc.getValue();
    }



}
