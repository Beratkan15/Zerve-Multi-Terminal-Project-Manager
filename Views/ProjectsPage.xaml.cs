using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using Zerve.Models;

namespace Zerve.Views
{
    public partial class ProjectsPage : Page
    {
        private readonly MainWindow _mainWindow;

        public ProjectsPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            
            AddProjectButton.Click += AddProject_Click;
            
            UpdateUI();
        }

        public void UpdateUI()
        {
            ProjectsStackPanel.Children.Clear();
            
            if (_mainWindow.Projects.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
            }
            else
            {
                EmptyState.Visibility = Visibility.Collapsed;
                
                foreach (var project in _mainWindow.Projects)
                {
                    var card = CreateProjectCard(project);
                    ProjectsStackPanel.Children.Add(card);
                }
            }
        }

        private UIElement CreateProjectCard(Project project)
        {
            var card = new Card
            {
                Margin = new Thickness(0, 0, 0, 16),
                Padding = new Thickness(20)
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Header
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var headerStack = new System.Windows.Controls.StackPanel();
            var nameText = new System.Windows.Controls.TextBlock
            {
                Text = project.Name,
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 4)
            };
            
            var idPathStack = new System.Windows.Controls.StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
            
            // CustomId badge
            if (!string.IsNullOrEmpty(project.CustomId))
            {
                var idBorder = new System.Windows.Controls.Border
                {
                    Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(78, 201, 176)),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(8, 2, 8, 2),
                    Margin = new Thickness(0, 0, 8, 0),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    Tag = project.CustomId
                };
                
                var idText = new System.Windows.Controls.TextBlock
                {
                    Text = project.CustomId,
                    FontSize = 11,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = System.Windows.Media.Brushes.Black
                };
                idBorder.Child = idText;
                
                // Add click event to copy ID
                idBorder.MouseLeftButtonDown += (s, e) =>
                {
                    if (s is System.Windows.Controls.Border border && border.Tag is string customId)
                    {
                        try
                        {
                            Clipboard.SetText(customId);
                            
                            // Visual feedback
                            var originalBrush = border.Background;
                            border.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(22, 198, 12));
                            
                            var timer = new System.Windows.Threading.DispatcherTimer
                            {
                                Interval = TimeSpan.FromMilliseconds(200)
                            };
                            timer.Tick += (sender, args) =>
                            {
                                border.Background = originalBrush;
                                timer.Stop();
                            };
                            timer.Start();
                        }
                        catch { }
                    }
                };
                
                idPathStack.Children.Add(idBorder);
            }
            
            var pathText = new System.Windows.Controls.TextBlock
            {
                Text = project.FolderPath,
                FontSize = 13,
                VerticalAlignment = VerticalAlignment.Center
            };
            idPathStack.Children.Add(pathText);
            
            headerStack.Children.Add(nameText);
            headerStack.Children.Add(idPathStack);

            var deleteBtn = new Wpf.Ui.Controls.Button
            {
                Icon = new SymbolIcon { Symbol = SymbolRegular.Delete24 },
                Appearance = ControlAppearance.Danger,
                Padding = new Thickness(8, 8, 8, 8),
                Tag = project.Id
            };
            deleteBtn.Click += DeleteProject_Click;

            Grid.SetColumn(headerStack, 0);
            Grid.SetColumn(deleteBtn, 1);
            headerGrid.Children.Add(headerStack);
            headerGrid.Children.Add(deleteBtn);

            // Command display
            var commandTextBox = new System.Windows.Controls.TextBox
            {
                Text = project.Command,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 13,
                IsReadOnly = true,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 12, 0, 12),
                Padding = new Thickness(12, 12, 12, 12),
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(1, 1, 1, 1),
                BorderBrush = System.Windows.Media.Brushes.Gray
            };

            // Buttons
            var buttonsPanel = new System.Windows.Controls.StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
            
            var openBtn = new Wpf.Ui.Controls.Button
            {
                Content = "Open",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Folder24 },
                Appearance = ControlAppearance.Secondary,
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 8, 0),
                Tag = project.FolderPath
            };
            openBtn.Click += OpenFolder_Click;

            var runRestartBtn = new Wpf.Ui.Controls.Button
            {
                Content = project.IsRunning ? "Restart" : "Run",
                Icon = new SymbolIcon { Symbol = project.IsRunning ? SymbolRegular.ArrowSync24 : SymbolRegular.Play24 },
                Appearance = project.IsRunning ? ControlAppearance.Secondary : ControlAppearance.Success,
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 8, 0),
                Tag = project.Id
            };
            runRestartBtn.Click += RunRestartProject_Click;

            var stopBtn = new Wpf.Ui.Controls.Button
            {
                Content = "Stop",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Stop24 },
                Appearance = ControlAppearance.Danger,
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 8, 0),
                Tag = project.Id,
                IsEnabled = project.IsRunning
            };
            stopBtn.Click += StopProject_Click;

            var editBtn = new Wpf.Ui.Controls.Button
            {
                Content = "Edit",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Edit24 },
                Appearance = ControlAppearance.Secondary,
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 8, 0),
                Tag = project
            };
            editBtn.Click += EditProject_Click;

            var logsBtn = new Wpf.Ui.Controls.Button
            {
                Content = "Logs",
                Icon = new SymbolIcon { Symbol = SymbolRegular.DocumentText24 },
                Appearance = ControlAppearance.Secondary,
                Padding = new Thickness(12, 8, 12, 8),
                Tag = project
            };
            logsBtn.Click += ShowLogs_Click;

            buttonsPanel.Children.Add(openBtn);
            buttonsPanel.Children.Add(runRestartBtn);
            buttonsPanel.Children.Add(stopBtn);
            buttonsPanel.Children.Add(editBtn);
            buttonsPanel.Children.Add(logsBtn);

            Grid.SetRow(headerGrid, 0);
            Grid.SetRow(commandTextBox, 1);
            Grid.SetRow(buttonsPanel, 2);

            grid.Children.Add(headerGrid);
            grid.Children.Add(commandTextBox);
            grid.Children.Add(buttonsPanel);

            card.Content = grid;
            return card;
        }

        private void AddProject_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddProjectDialog
            {
                Owner = Window.GetWindow(this)
            };
            
            if (dialog.ShowDialog() == true && dialog.Project != null)
            {
                _mainWindow.Projects.Add(dialog.Project);
                _mainWindow.DataService.SaveProjects(_mainWindow.Projects);
                UpdateUI();
            }
        }

        private void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button button && button.Tag is Guid projectId)
            {
                var project = _mainWindow.Projects.FirstOrDefault(p => p.Id == projectId);
                if (project != null)
                {
                    if (project.IsRunning)
                    {
                        _mainWindow.ProcessManager.StopProcess(projectId);
                    }
                    
                    _mainWindow.Projects.Remove(project);
                    _mainWindow.DataService.SaveProjects(_mainWindow.Projects);
                    UpdateUI();
                }
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button button && button.Tag is string folderPath)
            {
                try
                {
                    Process.Start("explorer.exe", folderPath);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error opening folder: {ex.Message}", "Error", 
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void RunRestartProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button button && button.Tag is Guid projectId)
            {
                var project = _mainWindow.Projects.FirstOrDefault(p => p.Id == projectId);
                if (project != null)
                {
                    if (project.IsRunning)
                    {
                        _mainWindow.ProcessManager.RestartProcess(projectId);
                    }
                    else
                    {
                        _mainWindow.ProcessManager.StartProcess(projectId, project.Command, project.FolderPath);
                        project.IsRunning = true;
                    }
                    _mainWindow.DataService.SaveProjects(_mainWindow.Projects);
                    UpdateUI();
                }
            }
        }

        private void StopProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button button && button.Tag is Guid projectId)
            {
                var project = _mainWindow.Projects.FirstOrDefault(p => p.Id == projectId);
                if (project != null && project.IsRunning)
                {
                    _mainWindow.ProcessManager.StopProcess(projectId);
                    project.IsRunning = false;
                    _mainWindow.DataService.SaveProjects(_mainWindow.Projects);
                    UpdateUI();
                }
            }
        }

        private void EditProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button button && button.Tag is Project project)
            {
                var dialog = new AddProjectDialog(project)
                {
                    Owner = Window.GetWindow(this)
                };
                
                if (dialog.ShowDialog() == true && dialog.Project != null)
                {
                    project.Name = dialog.Project.Name;
                    project.CustomId = dialog.Project.CustomId;
                    project.FolderPath = dialog.Project.FolderPath;
                    project.Command = dialog.Project.Command;
                    
                    _mainWindow.DataService.SaveProjects(_mainWindow.Projects);
                    UpdateUI();
                }
            }
        }

        private void ShowLogs_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button button && button.Tag is Project project)
            {
                var logWindow = new LogWindow(project, _mainWindow.ProcessManager)
                {
                    Owner = Window.GetWindow(this)
                };
                logWindow.Show();
            }
        }
    }
}
