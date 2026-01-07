using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using Zerve.Models;
using Zerve.Services;
using Zerve.Views;
using Zerve.Api;

namespace Zerve
{
    public partial class MainWindow : FluentWindow
    {
        public readonly List<Project> Projects = new();
        public readonly DataService DataService = new();
        public readonly ProcessManager ProcessManager = new();
        private ApiServer? _apiServer;
        private ProjectsPage? _projectsPage;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
            
            // Initialize API server
            _apiServer = new ApiServer(
                () => Projects,
                ProcessManager,
                () => DataService.SaveProjects(Projects),
                () => Dispatcher.Invoke(() => _projectsPage?.UpdateUI())
            );
            
            try
            {
                _apiServer.Start();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"API Server could not start (port 27015 may be in use).\nCLI features will not work.\n\nError: {ex.Message}",
                    "Warning",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning
                );
            }
            
            // Load projects
            var projects = DataService.LoadProjects();
            Projects.Clear();
            Projects.AddRange(projects);
            
            // Subscribe to process events
            ProcessManager.LogReceived += ProcessManager_LogReceived;
            
            // Handle window closing
            Closing += MainWindow_Closing;
            
            // Navigate to Projects page by default
            NavigateToProjects();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error initializing application:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                    "Startup Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error
                );
                throw;
            }
        }

        private void ProjectsNav_Click(object sender, RoutedEventArgs e)
        {
            NavigateToProjects();
            ProjectsNavButton.Appearance = ControlAppearance.Secondary;
            CreditsNavButton.Appearance = ControlAppearance.Transparent;
        }

        private void CreditsNav_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new CreditsPage());
            ProjectsNavButton.Appearance = ControlAppearance.Transparent;
            CreditsNavButton.Appearance = ControlAppearance.Secondary;
        }

        private void NavigateToProjects()
        {
            if (_projectsPage == null)
            {
                _projectsPage = new ProjectsPage(this);
            }
            ContentFrame.Navigate(_projectsPage);
            ProjectsNavButton.Appearance = ControlAppearance.Secondary;
            CreditsNavButton.Appearance = ControlAppearance.Transparent;
        }

        private void ProcessManager_LogReceived(object? sender, LogEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var project = Projects.FirstOrDefault(p => p.Id == e.ProjectId);
                if (project != null)
                {
                    project.IsRunning = ProcessManager.IsRunning(e.ProjectId);
                    _projectsPage?.UpdateUI();
                }
            });
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            _apiServer?.Stop();
            ProcessManager.StopAll();
            DataService.SaveProjects(Projects);
        }
    }
}
