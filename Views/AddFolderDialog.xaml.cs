using System.Windows;
using Wpf.Ui.Controls;
using Zerve.Models;

namespace Zerve.Views
{
    public partial class AddFolderDialog : FluentWindow
    {
        public Folder? Folder { get; private set; }

        public AddFolderDialog()
        {
            InitializeComponent();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var folderName = FolderNameTextBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(folderName))
            {
                System.Windows.MessageBox.Show("Please enter a folder name.", "Validation Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            Folder = new Folder
            {
                Name = folderName
            };

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
