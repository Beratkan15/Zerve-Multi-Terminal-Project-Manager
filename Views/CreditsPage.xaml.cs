using System.Windows.Controls;

namespace Zerve.Views
{
    public partial class CreditsPage : Page
    {
        public CreditsPage()
        {
            InitializeComponent();
        }

        private void GitHubLink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/Beratkan15/Zerve-Multi-Terminal-Project-Manager",
                    UseShellExecute = true
                });
            }
            catch { }
        }
    }
}
