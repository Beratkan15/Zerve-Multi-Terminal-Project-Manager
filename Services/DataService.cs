using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Zerve.Models;

namespace Zerve.Services
{
    public class DataService
    {
        private readonly string _dataFilePath;

        public DataService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Zerve"
            );
            Directory.CreateDirectory(appDataPath);
            _dataFilePath = Path.Combine(appDataPath, "projects.json");
        }

        public List<Project> LoadProjects()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    var json = File.ReadAllText(_dataFilePath);
                    var projects = JsonSerializer.Deserialize<List<Project>>(json);
                    return projects ?? new List<Project>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading projects: {ex.Message}");
            }

            return new List<Project>();
        }

        public void SaveProjects(List<Project> projects)
        {
            try
            {
                // Reset running state before saving
                foreach (var project in projects)
                {
                    project.IsRunning = false;
                }

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var json = JsonSerializer.Serialize(projects, options);
                File.WriteAllText(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving projects: {ex.Message}");
            }
        }
    }
}
