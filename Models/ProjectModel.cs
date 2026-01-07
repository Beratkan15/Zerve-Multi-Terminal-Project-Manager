using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Zerve.Models
{
    public class Project : INotifyPropertyChanged
    {
        private bool _isRunning;

        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CustomId { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public Guid? FolderId { get; set; } // Folder this project belongs to (null = root)
        
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        public Project()
        {
            Id = Guid.NewGuid();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
