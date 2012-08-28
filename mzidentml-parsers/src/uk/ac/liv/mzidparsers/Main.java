
package uk.ac.liv.mzidparsers;

/**
 *
 * @author F. Ghali
 */
public class Main {
    
     public static void main(String[] args) throws Exception {
        
        String inputfile="";
        String outputfile="";
        if (args.length > 0) {
	        	//Validate:
	        	if(args.length<3){                       
	                System.out.println("Error, Usage java -jar MzidParsers.jar Tandem2mzid|Omssa2mzid|Csv2mzid inputFileLocation outputFileLocation [-decoyRegex decoyRegex] [-databaseFileFormatID databaseFileFormatID] [-massSpecFileFormatID massSpecFileFormatID] [paramsFileLocation]");
	                System.out.println("\nNote: [-databaseFileFormatID databaseFileFormatID] [-massSpecFileFormatID massSpecFileFormatID] are Tandem2mzid specific and suitable values can be found at the mzML Controlled Vocabulary");
	                System.out.println("\nNote: [paramsFileLocation] is Csv2mzid specific");
	                System.out.println("\nExamples of valid commands: \n");
	                //TODO could add more examples here
	                System.out.println(" java -jar MzidParsers.jar Tandem2mzid inputFileLocation outputFileLocation ");
	                System.out.println(" java -jar MzidParsers.jar Tandem2mzid inputFileLocation outputFileLocation -decoyRegex decoyRegex");
	                System.out.println(" java -jar MzidParsers.jar Omssa2mzid inputFileLocation outputFileLocation ");
	                System.out.println(" etc...");
	                System.exit(1);
	            }
        	
                inputfile = args[1];
                outputfile = args[2];

                if(args[0].equals("Omssa2mzid")){
                	String decoyRegex = Utils.getCmdParameter(args, "decoyRegex", false);
                        Boolean outputFragmentation = false;
                        
                        if(Utils.getCmdParameter(args, "outputFragmentation", false) != null){
                            outputFragmentation = true;                            
                        }
                        
                    if(decoyRegex != null){
                        new Omssa2mzid(inputfile, outputfile, outputFragmentation, decoyRegex);
                        System.out.println("Searching for decoys containing " + decoyRegex);
                    }
                    else if(args.length==3 || args.length==4){                        
                        new Omssa2mzid(inputfile, outputfile, outputFragmentation);
                    }
                    else{
                        System.out.println("Error, Usage java -jar MzidParsers.jar Omssa2mzid inputFileLocation outputFileLocation [-outputFragmentation] [-decoyRegex decoyRegex]");
                        
                    }
                }
                else if(args[0].equals("Tandem2mzid")){

                	Boolean idsStartAtZero = false;     //TODO - need to work out how we know whether this TRUE (mzML was searched) or FALSE (anything else was searched)
                    String decoyRegex = Utils.getCmdParameter(args, "decoyRegex", false);
                    if(decoyRegex!= null)
                        System.out.println("Searching for decoys containing " + decoyRegex);                        
                    
                    String databaseFileFormatID = Utils.getCmdParameter(args, "databaseFileFormatID", false);
                    String massSpecFileFormatID = Utils.getCmdParameter(args, "massSpecFileFormatID", false);
                    new Tandem2mzid(inputfile, outputfile, databaseFileFormatID, massSpecFileFormatID, idsStartAtZero, decoyRegex);
                    
                }
                else if(args[0].equals("Csv2mzid")){
                    Boolean idsStartAtZero = false;
                    
                    if(args.length==6){
                        String paramsFile = args[3];
                        String cvAccForPSMOrdering = args[4];
                        String decoyRegex = args[5];
                        
                        new Csv2mzid(inputfile, outputfile, paramsFile,cvAccForPSMOrdering, decoyRegex);
                        System.out.println("Searching for decoys containing " + decoyRegex);
                        
                    }
                    else if(args.length==5){
                        String paramsFile = args[3];
                        String cvAccForPSMOrdering = args[4];
                        new Csv2mzid(inputfile, outputfile, cvAccForPSMOrdering, paramsFile);                        
                    }
                    else{                        
                        System.out.println("Error, Usage  java -jar MzidParsers.jar Csv2mzid inputFileLocation outputFileLocation paramsFileLocation cvAccessionForPSMOrdering [decoyRegex]");
                    }
                }
                else{
                    System.out.println("Error, parser option not recognized");
                    
                }
                
        }
        else{
            System.out.println("Error, no command line arguments entered");
            
        }

    }
    
}
