using System;
using System.Windows;
using IQ.ViewModel;

namespace IQ.View
{
    /// <summary>
    /// Interaction logic for AutoprocessorView.xaml
    /// </summary>
    public partial class AutoprocessorView : Window
    {
        public AutoprocessorView()
        {
            InitializeComponent();

            ViewModel = new AutoprocessorViewModel();

            DataContext = ViewModel;

            LoadSettings();
        }

        public AutoprocessorViewModel ViewModel { get; set; }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Execute();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CancelProcessing();
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }

        private void LoadSettings()
        {
            ViewModel.DatasetPath = Properties.Settings.Default.LastDatasetPath ?? "";
            ViewModel.TargetsFilePath = Properties.Settings.Default.LastTargetFilePath ?? "";
            ViewModel.WorkflowParametersFilePath = Properties.Settings.Default.LastWorkflowFilePath ?? "";
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.LastDatasetPath = ViewModel.DatasetPath ?? "";
            Properties.Settings.Default.LastTargetFilePath = ViewModel.TargetsFilePath ?? "";
            Properties.Settings.Default.LastWorkflowFilePath = ViewModel.WorkflowParametersFilePath ?? "";

            Properties.Settings.Default.Save();
        }

        private void btnSelectDataset_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "All files (*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                string filename = dlg.FileName;
                ViewModel.DatasetPath = filename;
            }
        }

        private void btnSelectWorkflowFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "XML workflow files (.xml)|*.xml|All files (*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                string filename = dlg.FileName;
                ViewModel.WorkflowParametersFilePath = filename;
            }
        }

        private void btnSelectTargetFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "target / result files (.txt)|*.txt|All files (*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                string filename = dlg.FileName;
                ViewModel.TargetsFilePath = filename;
            }
        }

        private void btnClearDataset_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DatasetPath = String.Empty;
        }

        private void btnClearWorkflowFilePath_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.WorkflowParametersFilePath = String.Empty;
        }

        private void btnClearTargetFilePath_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.TargetsFilePath = String.Empty;
        }
    }
}
