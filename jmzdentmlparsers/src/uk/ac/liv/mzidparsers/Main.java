
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
                inputfile = args[1];
                outputfile = args[2];

                if(args[0].equals("Omssa2mzid")){
                    new Omssa2mzid(inputfile, outputfile);
                }
                if(args[0].equals("Tandem2mzid")){
                    new Tandem2mzid(inputfile, outputfile);
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
