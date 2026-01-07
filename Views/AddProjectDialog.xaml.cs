using System;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Wpf.Ui.Controls;
using Zerve.Models;

namespace Zerve.Views
{
    public partial class AddProjectDialog : FluentWindow
    {
        public Project? Project { get; private set; }
        private readonly bool _isEditMode;

        public AddProjectDialog(Project? existingProject = null)
        {
            InitializeComponent();
            
            if (existingProject != null)
            {
                _isEditMode = true;
                Project = existingProject;
                
                // Pre-fill form
                ProjectNameTextBox.Text = existingProject.Name;
                ProjectIdTextBox.Text = existingProject.CustomId;
                FolderPathTextBox.Text = existingProject.FolderPath;
                CommandTextBox.Text = existingProject.Command;
                
                // Update title and button text
                Title = "Edit Project";
            }
        }

        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Project Folder"
            };

            if (dialog.ShowDialog() == true)
            {
                FolderPathTextBox.Text = dialog.FolderName;
            }
        }

        private string GenerateRandomId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var length = random.Next(5, 21); // 5 to 20 characters
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void AddProject_Click(object sender, RoutedEventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(ProjectNameTextBox.Text))
            {
                System.Windows.MessageBox.Show("Please enter a project name.", "Validation Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(FolderPathTextBox.Text))
            {
                System.Windows.MessageBox.Show("Please select a folder path.", "Validation Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CommandTextBox.Text))
            {
                System.Windows.MessageBox.Show("Please enter a command.", "Validation Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            // Generate CustomId if empty
            var customId = string.IsNullOrWhiteSpace(ProjectIdTextBox.Text) 
                ? GenerateRandomId() 
                : ProjectIdTextBox.Text.Trim();

            if (_isEditMode && Project != null)
            {
                // Update existing project (keep same ID and IsRunning state)
                Project.Name = ProjectNameTextBox.Text.Trim();
                Project.CustomId = customId;
                Project.FolderPath = FolderPathTextBox.Text.Trim();
                Project.Command = CommandTextBox.Text.Trim();
            }
            else
            {
                // Create new project
                Project = new Project
                {
                    Name = ProjectNameTextBox.Text.Trim(),
                    CustomId = customId,
                    FolderPath = FolderPathTextBox.Text.Trim(),
                    Command = CommandTextBox.Text.Trim(),
                    IsRunning = false
                };
            }

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
