using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DeconTools.Backend;
using System.IO;

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
            this.listBox1.DataSource = System.Enum.GetValues(typeof(Globals.MSFileType));

            Globals.MSFileType guessedFileType= guessFileTypeFromFileName(filename);
            this.listBox1.SelectedItem = guessedFileType;
        }

        private Globals.MSFileType guessFileTypeFromFileName(string filename)
        {
            try
            {
                if (File.Exists(filename))
                {
                    string extension = Path.GetExtension(filename);
                    if (extension.ToLower() == ".d")
                    {
                        return Globals.MSFileType.Agilent_D;
                    }
                    if (extension.ToLower() == ".raw")
                    {
                        return Globals.MSFileType.Finnigan;
                    }
                    else if (extension.ToLower() == ".imf")
                    {   
                        return Globals.MSFileType.PNNL_IMS;
                    }
                    else if (extension.ToLower() == ".uimf")
                    {
                        return Globals.MSFileType.PNNL_UIMF;
                    }
                    else if (extension.ToLower() == ".mzxml")
                    {
                        return Globals.MSFileType.MZXML_Rawdata;
                    }
                    else if (extension.ToLower() == ".yafms")
                    {
                        return Globals.MSFileType.YAFMS;
                    }
                    else if (Path.GetFileName(filename).ToLower() == "acqus")
                    {
                        return Globals.MSFileType.Bruker;
                    }
                   
                    else
                    {
                        return Globals.MSFileType.Undefined;
                    }

                }
                else
                {
                    return Globals.MSFileType.Undefined;
                }

            }
            catch (Exception)
            {
                return Globals.MSFileType.Undefined;
            }
           

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.selectedFiletype = (Globals.MSFileType)listBox1.SelectedItem;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
