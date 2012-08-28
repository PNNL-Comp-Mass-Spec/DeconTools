package uk.ac.liv.mzidparsers;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.util.HashMap;

/**
 * General utilities class 
 * 
 * @author lukas007
 *
 */
public class Utils 
{
	/**
	 * Round a double value and keeping (at max) the given number of decimal places.
	 * 
	 * @param value
	 * @param numberOfDecimalPlaces
	 * @return
	 */
	public static double round(double value, int numberOfDecimalPlaces)
	{
		double multipicationFactor = Math.pow(10, numberOfDecimalPlaces);
	    return Math.round(value * multipicationFactor) / multipicationFactor;
	}
	
	
	
    /**
     * Initialized the CV map based on the /resources/CV_psi-ms.obo.txt  CV file.
     * 
     * @return
     * @throws IOException
     */
	public static HashMap<String, String> getInitializedCVMap() throws IOException 
	{
		//Read resource file and build up map:
		BufferedReader in = null;
		HashMap<String, String> resultMap = new HashMap<String, String>();
		try
		{
        	//Use the getResourceAsStream trick to read the file packaged in
			//the .jar .  This simplifies usage of the solution as no extra 
        	//classpath or path configurations are needed: 
            InputStream resourceAsStream = Utils.class.getResourceAsStream("/resources/CV_psi-ms.obo.txt");
    		InputStreamReader reader = new InputStreamReader(resourceAsStream);
    		in = new BufferedReader(reader);
    		String inputLine;
    		String key = "";
    		String value = "";
    		
    		while ((inputLine = in.readLine()) != null)
    		{
    			if (inputLine.startsWith("id:"))
    				key = inputLine.split("id:")[1].trim();
    			if (inputLine.startsWith("name:"))
    			{
    				//validate:
    				if (key.equals(""))
    					throw new RuntimeException("Unexpected name: preceding id: entry in CV file");
    				value = inputLine.split("name:")[1].trim();
    				resultMap.put(key,value);
    				//reset:
    				key = "";
    				value = "";
    			}	
    		}
    		return resultMap;
        		
		}
		finally
		{
			if (in != null)
				in.close();
		}
            
	}
	
	
	/**
	 * Returns the value of a command-line parameter
	 * 
	 * @param args : command-line arguments (assuming couples in the form "-argname", "argvalue" )
	 * @param name : the parameter 'name' 
	 * @return returns null if the parameter is not found (and is not required). If the parameter is not
	 * found but is required, it throws an error.
	 */
	public static String getCmdParameter(String[] args, String name, boolean required) 
	{
		for (int i = 0; i < args.length; i++)
		{
			String argName = args[i];
			if (argName.equals("-" + name))
			{
				String argValue = "";
				if (i + 1 < args.length)
					argValue = args[i+1];
				if (required && (argValue.trim().length() == 0 || argValue.startsWith("-")))
				{
					System.err.println("Parameter value expected for " + argName);
					throw new RuntimeException("Expected parameter value not found: " + argName);
				}
				else if (argValue.trim().length() == 0 || argValue.startsWith("-"))
					return "";
				else
					return argValue;
			}	
		}
		//Nothing found, if required, throw error, else return "";
		if (required)
		{
			System.err.println("Parameter -" + name + " expected ");
			throw new RuntimeException("Expected parameter not found: " + name);
		}
		
		return null;
	}
	
}
