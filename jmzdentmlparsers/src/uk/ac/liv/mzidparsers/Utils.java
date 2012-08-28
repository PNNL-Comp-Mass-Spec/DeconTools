package uk.ac.liv.mzidparsers;

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
}
