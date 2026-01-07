using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Controls;
using Wpf.Ui.Appearance;
using Zerve.Models;

namespace Zerve.Views
{
    public partial class ProjectsPage : Page
    {
        private readonly MainWindow _mainWindow;
        private string _searchQuery = string.Empty;
        private Folder? _currentFolder = null;
        private Stack<Folder?> _navigationHistory = new Stack<Folder?>();

        public ProjectsPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            
            SearchBox.TextChanged += SearchBox_TextChanged;
            
            // Mouse button navigation
            this.MouseDown += Page_MouseDown;
            
            UpdateUI();
        }

        private void Page_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Back button (Mouse Button 4)
            if (e.ChangedButton == MouseButton.XButton1)
            {
                if (_currentFolder != null)
                {
                    NavigateBack();
                    e.Handled = true;
                }
            }
            // Forward button (Mouse Button 5) - not implemented yet
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            _searchQuery = SearchBox.Text?.ToLower() ?? string.Empty;
            UpdateUI();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (_currentFolder == null)
            {
                // Add Folder
                var dialog = new AddFolderDialog
                {
                    Owner = Window.GetWindow(this)
                };
                
                if (dialog.ShowDialog() == true && dialog.Folder != null)
                {
                    _mainWindow.Folders.Add(dialog.Folder);
                    _mainWindow.DataService.SaveFolders(_mainWindow.Folders);
                    UpdateUI();
                }
            }
            else
            {
                // Add Project to current folder
                var dialog = new AddProjectDialog
                {
                    Owner = Window.GetWindow(this)
                };
                
                if (dialog.ShowDialog() == true && dialog.Project != null)
                {
                    dialog.Project.FolderId = _currentFolder.Id;
                    _mainWindow.Projects.Add(dialog.Project);
                    _mainWindow.DataService.SaveProjects(_mainWindow.Projects);
                    UpdateUI();
                }
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigateBack();
        }

        private void NavigateBack()
        {
            if (_navigationHistory.Count > 0)
            {
                _currentFolder = _navigationHistory.Pop();
                UpdateUI();
            }
        }

        private void NavigateToFolder(Folder folder)
        {
            _navigationHistory.Push(_currentFolder);
            _currentFolder = folder;
            UpdateUI();
        }

        public void UpdateUI()
        {
            ContentStackPanel.Children.Clear();

            if (_currentFolder == null)
            {
                // Folder View
                ShowFolderView();
            }
            else
            {
                // Project View (inside folder)
                ShowProjectView();
            }
        }

        private void ShowFolderView()
        {
            PageTitle.Text = "Folders";
            AddButton.Content = "Add Folder";
            AddButton.Icon = new SymbolIcon { Symbol = SymbolRegular.FolderAdd24 };
            BackButton.Visibility = Visibility.Collapsed;
            SearchBox.PlaceholderText = "Search folders...";

            var filteredFolders = _mainWindow.Folders.Where(f =>
                string.IsNullOrEmpty(_searchQuery) ||
                f.Name.ToLower().Contains(_searchQuery)
            ).ToList();

            if (filteredFolders.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                EmptyStateTitle.Text = string.IsNullOrEmpty(_searchQuery) ? "No folders yet" : $"No folders found matching '{_searchQuery}'";
                EmptyStateSubtitle.Text = string.IsNullOrEmpty(_searchQuery) ? "Click 'Add Folder' to get started" : "Try a different search term";
            }
            else
            {
                EmptyState.Visibility = Visibility.Collapsed;
                
                foreach (var folder in filteredFolders)
                {
                    var card = CreateFolderCard(folder);
                    ContentStackPanel.Children.Add(card);
                }
            }
        }

        private void ShowProjectView()
        {
            PageTitle.Text = _currentFolder!.Name;
            AddButton.Content = "Add Project";
            AddButton.Icon = new SymbolIcon { Symbol = SymbolRegular.Add24 };
            BackButton.Visibility = Visibility.Visible;
            SearchBox.PlaceholderText = "Search projects...";

            var filteredProjects = _mainWindow.Projects.Where(p =>
                p.FolderId == _currentFolder.Id &&
                (string.IsNullOrEmpty(_searchQuery) ||
                 p.Name.ToLower().Contains(_searchQuery) ||
                 (!string.IsNullOrEmpty(p.CustomId) && p.CustomId.ToLower().Contains(_searchQuery)))
            ).ToList();

            if (filteredProjects.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                EmptyStateTitle.Text = string.IsNullOrEmpty(_searchQuery) ? "No projects yet" : $"No projects found matching '{_searchQuery}'";
                EmptyStateSubtitle.Text = string.IsNullOrEmpty(_searchQuery) ? "Click 'Add Project' to get started" : "Try a different search term";
            }
            else
            {
                EmptyState.Visibility = Visibility.Collapsed;
                
                foreach (var project in filteredProjects)
                {
                    var card = CreateProjectCard(project);
                    ContentStackPanel.Children.Add(card);
                }
            }
        }

        private UIElement CreateFolderCard(Folder folder)
        {
            var card = new Card
            {
                Margin = new Thickness(0, 0, 0, 16),
                Padding = new Thickness(20)
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Icon
            var icon = new SymbolIcon
            {
                Symbol = SymbolRegular.Folder24,
                FontSize = 32,
                Margin = new Thickness(0, 0, 16, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(icon, 0);

            // Folder Info
            var infoPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            
            var nameText = new System.Windows.Controls.TextBlock
            {
                Text = folder.Name,
                FontSize = 18,
                FontWeight = FontWeights.SemiBold
            };
            infoPanel.Children.Add(nameText);

            var projectCount = _mainWindow.Projects.Count(p => p.FolderId == folder.Id);
            var countText = new System.Windows.Controls.TextBlock
            {
                Text = $"{projectCount} project{(projectCount != 1 ? "s" : "")}",
                FontSize = 13,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(0, 4, 0, 0)
            };
            infoPanel.Children.Add(countText);

            Grid.SetColumn(infoPanel, 1);

            // Open Button
            var openBtn = new Wpf.Ui.Controls.Button
            {
                Content = "Open",
                Icon = new SymbolIcon { Symbol = SymbolRegular.FolderOpen24 },
                Appearance = ControlAppearance.Primary,
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 8, 0),
                Tag = folder
            };
            openBtn.Click += OpenFolder_Click;
            Grid.SetColumn(openBtn, 2);

            // Delete Button
            var deleteBtn = new Wpf.Ui.Controls.Button
            {
                Icon = new SymbolIcon { Symbol = SymbolRegular.Delete24 },
                Appearance = ControlAppearance.Danger,
                Padding = new Thickness(12, 8, 12, 8),
                Tag = folder.Id
            };
            deleteBtn.Click += DeleteFolder_Click;
            Grid.SetColumn(deleteBtn, 3);

            grid.Children.Add(icon);
            grid.Children.Add(infoPanel);
            grid.Children.Add(openBtn);
            grid.Children.Add(deleteBtn);

            card.Content = grid;

            return card;
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button button && button.Tag is Folder folder)
            {
                NavigateToFolder(folder);
            }
        }

        private void DeleteFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button button && button.Tag is Guid folderId)
            {
                var folder = _mainWindow.Folders.FirstOrDefault(f => f.Id == folderId);
                if (folder != null)
                {
                    var projectCount = _mainWindow.Projects.Count(p => p.FolderId == folderId);
                    var message = projectCount > 0
                        ? $"This folder contains {projectCount} project{(projectCount != 1 ? "s" : "")}. Are you sure you want to delete it?"
                        : "Are you sure you want to delete this folder?";

                    var result = System.Windows.MessageBox.Show(message, "Confirm Delete",
                        System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);

                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        // Delete all projects in folder
                        var projectsToDelete = _mainWindow.Projects.Where(p => p.FolderId == folderId).ToList();
                        foreach (var project in projectsToDelete)
                        {
                            if (project.IsRunning)
                            {
                                _mainWindow.ProcessManager.StopProcess(project.Id);
                            }
                            _mainWindow.Projects.Remove(project);
                        }

                        _mainWindow.Folders.Remove(folder);
                        _mainWindow.DataService.SaveFolders(_mainWindow.Folders);
                        _mainWindow.DataService.SaveProjects(_mainWindow.Projects);
                        UpdateUI();
                    }
                }
            }
        }

        // Keep existing CreateProjectCard method from old code
        private UIElement CreateProjectCard(Project project)
        {
            var card = new Card
            {
                Margin = new Thickness(0, 0, 0, 16),
                Padding = new Thickness(20),
                Tag = project.Id.ToString()
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Header
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titlePanel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
            
            var nameText = new System.Windows.Controls.TextBlock
            {
                Text = project.Name,
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center
            };
            titlePanel.Children.Add(nameText);

            if (!string.IsNullOrEmpty(project.CustomId))
            {
                var idBadge = new Wpf.Ui.Controls.Button
                {
                    Content = project.CustomId,
                    Appearance = ControlAppearance.Secondary,
                    Padding = new Thickness(8, 4, 8, 4),
                    Margin = new Thickness(12, 0, 0, 0),
                    FontSize = 12,
                    Tag = project.CustomId
                };
                idBadge.Click += CopyId_Click;
                titlePanel.Children.Add(idBadge);
            }

            Grid.SetColumn(titlePanel, 0);

            var statusBadge = new Wpf.Ui.Controls.Badge
            {
                Content = project.IsRunning ? "Running" : "Stopped",
                Appearance = project.IsRunning ? ControlAppearance.Success : ControlAppearance.Secondary,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(statusBadge, 1);

            headerGrid.Children.Add(titlePanel);
            headerGrid.Children.Add(statusBadge);
            Grid.SetRow(headerGrid, 0);

            // Path
            var pathText = new System.Windows.Controls.TextBlock
            {
                Text = project.FolderPath,
                FontSize = 13,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(0, 8, 0, 0),
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            Grid.SetRow(pathText, 1);

            // Buttons
            var buttonsPanel = new System.Windows.Controls.StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, Margin = new Thickness(0, 16, 0, 0) };
            
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
                IsEnabled = project.IsRunning,
                Tag = project.Id
            };
            stopBtn.Click += StopProject_Click;

            var logsBtn = new Wpf.Ui.Controls.Button
            {
                Content = "Logs",
                Icon = new SymbolIcon { Symbol = SymbolRegular.DocumentText24 },
                Appearance = ControlAppearance.Secondary,
                Padding = new Thickness(12, 8, 12, 8),
                Margin = new Thickness(0, 0, 8, 0),
                Tag = project.Id
            };
            logsBtn.Click += ShowLogs_Click;

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

            var deleteBtn = new Wpf.Ui.Controls.Button
            {
                Icon = new SymbolIcon { Symbol = SymbolRegular.Delete24 },
                Appearance = ControlAppearance.Danger,
                Padding = new Thickness(12, 8, 12, 8),
                Tag = project.Id
            };
            deleteBtn.Click += DeleteProject_Click;

            buttonsPanel.Children.Add(openBtn);
            buttonsPanel.Children.Add(runRestartBtn);
            buttonsPanel.Children.Add(stopBtn);
            buttonsPanel.Children.Add(logsBtn);
            buttonsPanel.Children.Add(editBtn);
            buttonsPanel.Children.Add(deleteBtn);

            Grid.SetRow(buttonsPanel, 2);

            grid.Children.Add(headerGrid);
            grid.Children.Add(pathText);
            grid.Children.Add(buttonsPanel);

            card.Content = grid;
            return card;
        }

        // Keep all existing project button handlers
        private void CopyId_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button button && button.Tag is string customId)
            {
                try
                {
                    Clipboard.SetText(customId);
                }
                catch { }
            }
        }

        private void OpenProjectFolder_Click(object sender, RoutedEventArgs e)
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
                
                if (dialog.ShowDialog() == true)
                {
                    _mainWindow.DataService.SaveProjects(_mainWindow.Projects);
                    UpdateUI();
                }
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

        private void ShowLogs_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Button button && button.Tag is Guid projectId)
            {
                var project = _mainWindow.Projects.FirstOrDefault(p => p.Id == projectId);
                if (project != null)
                {
                    var logWindow = new LogWindow(project, _mainWindow.ProcessManager);
                    logWindow.Show();
                }
            }
        }
    }
}
