using System;
using System.Windows.Forms;
using DeconTools.Backend;
using System.IO;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Core;

namespace DeconToolsAutoProcessV1
{
    public partial class MSFileTypeSelectorForm : Form
    {
        Globals.MSFileType selectedFiletype;

        public Globals.MSFileType SelectedFiletype
        {
            get { return selectedFiletype; }
            set { selectedFiletype = value; }
        }

        public MSFileTypeSelectorForm()
            : this("")
        {

        }

        public MSFileTypeSelectorForm(string filename)
        {
            InitializeComponent();
            listBox1.DataSource = Enum.GetValues(typeof(Globals.MSFileType));

            Globals.MSFileType guessedFileType= guessFileTypeFromFileName(filename);
            listBox1.SelectedItem = guessedFileType;
        }

        private Globals.MSFileType guessFileTypeFromFileName(string filename)
        {

            RunFactory runFactory = new RunFactory();

            Run run = runFactory.CreateRun(filename);

            if (run != null)
            {
                Globals.MSFileType filetype = run.MSFileType;
                run.Dispose();
                return filetype;
            }

            try
            {

                if (Directory.Exists(filename))
                {
                    if (filename.EndsWith(".d", StringComparison.OrdinalIgnoreCase))
                    {
                        return Globals.MSFileType.Agilent_D;
                    }

                }


                if (!File.Exists(filename))
                {
                    return Globals.MSFileType.Undefined;
                }

                string extension = Path.GetExtension(filename);
                if (extension == null)
                    return Globals.MSFileType.Undefined;

                switch (extension.ToLower())
                {
                    case ".d":
                        return Globals.MSFileType.Agilent_D;
                    case ".raw":
                        return Globals.MSFileType.Thermo_Raw;
                    case ".imf":
                        return Globals.MSFileType.PNNL_IMS;
                    case ".uimf":
                        return Globals.MSFileType.PNNL_UIMF;
                    case ".mzxml":
                        return Globals.MSFileType.MZXML_Rawdata;

                    // Deprecated in February 2017
                    // case ".yafms":
                    //    return Globals.MSFileType.YAFMS;

                    default:
                        var fileName = Path.GetFileName(filename);
                        if (fileName != null && fileName.ToLower() == "acqus")
                        {
                            return Globals.MSFileType.Bruker;
                        }
                        break;
                }

                return Globals.MSFileType.Undefined;
            }
            catch (Exception)
            {
                return Globals.MSFileType.Undefined;
            }


        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            selectedFiletype = (Globals.MSFileType)listBox1.SelectedItem;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
