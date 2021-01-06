using System;

namespace DeconTools.Backend.Core
{
    /// <summary>
    /// Singleton class that functions to allow only one UIMF file to be opened at a time
    /// and keeps the file open allowing for more efficient use of the UIMFLibrary class.
    /// (instead of opening and closing it every time you want to retrieve information from
    /// the class)
    /// </summary>
    [Obsolete("This class was listed as decommissioned in 2010, though it remained in use by the UIMFRun class through 2020")]
    public class UIMFLibraryAdapter
    {
        private static UIMFLibraryAdapter instance;
        private readonly string fileName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename"></param>
        private UIMFLibraryAdapter(string filename)
        {
            fileName = filename;
            Reader = new UIMFLibrary.DataReader(fileName);
            ConnectionState = System.Data.ConnectionState.Open;
        }

        public UIMFLibrary.DataReader Reader { get; }

        public static UIMFLibraryAdapter getInstance(string filename)
        {
            if (instance == null)
            {
                instance = new UIMFLibraryAdapter(filename);
            }
            else
            {
                if (filename != instance.fileName)     // method's caller is trying to open a file different from that already opened
                {
                    instance.CloseCurrentUIMF();      //so close the one already opened
                    instance = null;
                    getInstance(filename);            //re-instantiate using provided filename
                }
                else     // method's caller is requesting the same filename that is already open
                {
                    if (ConnectionState == System.Data.ConnectionState.Closed)
                    {
                        instance = new UIMFLibraryAdapter(filename);
                    }
                    else
                    {
                        // don't need to do anything but return the instance (below)
                    }
                }
            }

            return instance;
        }

        public static System.Data.ConnectionState ConnectionState { get; set; }

        public void CloseCurrentUIMF()
        {
            if (instance != null)
            {
                Reader.Dispose();

                ConnectionState = System.Data.ConnectionState.Closed;
            }
        }

        internal static string GetLibraryVersion()
        {
            return Utilities.AssemblyInfoRetriever.GetVersion(typeof(UIMFLibrary.DataReader));
        }
    }
}
