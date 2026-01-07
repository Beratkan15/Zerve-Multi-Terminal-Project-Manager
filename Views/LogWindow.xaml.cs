using System;
using System.Collections.ObjectModel;
using System.Windows;
using Wpf.Ui.Controls;
using Zerve.Models;
using Zerve.Services;

namespace Zerve.Views
{
    public partial class LogWindow : FluentWindow
    {
        private readonly Project _project;
        private readonly ProcessManager _processManager;
        private readonly ObservableCollection<string> _logLines = new();

        public LogWindow(Project project, ProcessManager processManager)
        {
            InitializeComponent();
            
            _project = project;
            _processManager = processManager;
            
            TitleBar.Title = $"Logs - {project.Name}";
            
            // Set ItemsSource
            LogItemsControl.ItemsSource = _logLines;
            
            // Load existing log history
            var history = _processManager.GetLogHistory(project.Id);
            if (!string.IsNullOrEmpty(history))
            {
                var lines = history.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    _logLines.Add(line);
                }
                LogScrollViewer.ScrollToEnd();
            }
            
            // Subscribe to log events
            _processManager.LogReceived += ProcessManager_LogReceived;
            
            // Handle window closing
            Closing += LogWindow_Closing;
        }

        private void ProcessManager_LogReceived(object? sender, LogEventArgs e)
        {
            if (e.ProjectId == _project.Id)
            {
                Dispatcher.Invoke(() =>
                {
                    var timestamp = e.Timestamp.ToString("HH:mm:ss");
                    var prefix = e.IsError ? "[ERROR]" : "[INFO] ";
                    var logLine = $"[{timestamp}] {prefix} {e.Message}";
                    
                    _logLines.Add(logLine);
                    
                    // Auto-scroll to bottom
                    LogScrollViewer.ScrollToEnd();
                });
            }
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            _logLines.Clear();
        }

        private void LogWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Unsubscribe from events
            _processManager.LogReceived -= ProcessManager_LogReceived;
        }
    }
}
