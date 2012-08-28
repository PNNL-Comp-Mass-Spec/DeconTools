/*
* Date: 26/5/2010
* Adopted by - Ritesh
*
*/

package uk.ac.ebi.jmzidml.xml.xxindex;

import org.apache.log4j.Logger;

import java.io.*;
import java.net.URL;

public class FileUtils {

    private static Logger logger = Logger.getLogger(FileUtils.class);

    public static File getFileFromURL(URL url) {

        File tempFile;
        BufferedInputStream in = null;
        BufferedOutputStream out = null;
        try {

            String tempDir = System.getProperty("java.io.tmpdir", ".");

            // Create temp file.
            tempFile = File.createTempFile("xxindex", ".tmp", new File(tempDir));

            // Delete temp file when program exits.
            tempFile.deleteOnExit();

            // copy content of URL to local file
            InputStream is = url.openStream();
            in = new BufferedInputStream(is);

            FileOutputStream fos = new FileOutputStream(tempFile);
            out = new BufferedOutputStream(fos);

            byte[] b = new byte[1];
            while (in.read(b) >= 0) {
                out.write(b);
            }

            logger.debug(url + " written to local file " + tempFile.getAbsolutePath());


        } catch (IOException e) {
            throw new IllegalStateException("Could not create local file for URL: " + url, e);
        } finally {
            try {
                if (in != null) {
                    in.close();
                }
                if (out != null) {
                    out.close();
                }
            } catch (IOException e) {
                /* ignore */
            }

        }

        return tempFile;
    }

}
