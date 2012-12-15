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
    }
}
