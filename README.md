# Zerve - Multi-Terminal Project Manager

![Version](https://img.shields.io/badge/version-1.1-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)

**Zerve** is a modern, powerful project manager for Windows that allows you to manage and run multiple terminal-based projects simultaneously with an elegant WPF interface.

## 🔨 Build from Source

If you prefer to build the application yourself instead of downloading the pre-built executable:

1. Clone the repository:
   ```bash
   git clone https://github.com/Beratkan15/Zerve-Multi-Terminal-Project-Manager.git
   cd Zerve-Multi-Terminal-Project-Manager/app
   ```

2. Build the single-file executable:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```

3. Find your executable at:
   ```
   bin/Release/net8.0-windows/win-x64/publish/Zerve.exe
   ```

## ✨ Features

### 🎯 Project Management
- **Add, Edit, Delete Projects** - Manage your projects with ease
- **Custom Project IDs** - Assign unique identifiers (auto-generated or manual)
- **One-Click Operations** - Run, Restart, Stop projects instantly
- **Folder Integration** - Quick access to project directories

### 📊 Process Monitoring
- **Real-time Logs** - View live terminal output with color-coded messages
- **Persistent Log History** - Access logs even after restarting processes
- **Terminal-Style Display** - Windows Terminal-inspired log viewer
- **Process Tree Management** - Properly terminates all child processes

### 🎨 Modern UI
- **Dark Theme** - Easy on the eyes with Mica backdrop
- **Fluent Design** - Modern WPF UI with smooth interface
- **Collapsible Sidebar** - Toggle between full (200px) and compact (70px) mode
- **Icon in TitleBar** - Application icon displayed next to title
- **Responsive Layout** - Clean and intuitive interface

### 🔍 Smart Search
- **Real-time Filtering** - Search projects by name or custom ID
- **Instant Results** - Filter as you type
- **Clear Feedback** - Shows "No projects found" when search yields no results

### 🔧 Advanced Features
- **Port Conflict Prevention** - Automatically kills process trees to free ports
- **Custom ID Badges** - Click to copy project IDs to clipboard
- **HTTP API Server** - Built-in API for CLI integration (port 27015)
- **Single-File Executable** - Portable, self-contained application

## 📥 Installation

1. Download `Zerve.exe` from the releases
2. Run the executable - no installation required!
3. The app is fully portable and self-contained

## 🚀 Quick Start

1. **Launch Zerve** - Double-click `Zerve.exe`
2. **Add a Project** - Click "Add Project" button
   - Enter project name
   - Set a custom ID (optional - auto-generated if empty)
   - Select project folder
   - Enter the command to run (e.g., `npm run dev`)
3. **Run Your Project** - Click the "Run" button
4. **View Logs** - Click "Logs" to see terminal output
5. **Search Projects** - Use the search box to filter by name or ID
6. **Toggle Sidebar** - Click the navigation icon to collapse/expand sidebar
7. **Manage** - Use Stop, Restart, Edit, or Delete as needed

## 📋 Project ID System

Each project has a unique **Custom ID** that appears as a cyan badge:
- Auto-generated: 5-20 random alphanumeric characters
- Manual: Set your own custom identifier
- Click the ID badge to copy it to clipboard
- Used for CLI commands and API access

## 🎨 Screenshots

The application features:
- **Project Manager** - Main view with all your projects
- **Credits Page** - Information about the creator
- **Log Viewer** - Terminal-style output with color coding

## ⚙️ Technical Details

- **Framework**: .NET 8.0 (WPF)
- **UI Library**: WPF UI 3.0.5
- **Platform**: Windows x64
- **API Port**: 27015 (HTTP)
- **Data Storage**: JSON files in AppData

## 📝 License

This project is licensed under the MIT License with Attribution Requirement.

**Key Points:**
- ✅ Free to use, modify, and distribute
- ✅ Must credit the original creator (Beratkan15)
- ✅ Must include link to original repository when sharing
- ⚠️ Only the creator can publish official updates

See [LICENSE.md](LICENSE.md) for full details.

## 👨‍💻 Creator

**Beratkan15**

- GitHub: [@Beratkan15](https://github.com/Beratkan15)
- Repository: [Zerve-Multi-Terminal-Project-Manager](https://github.com/Beratkan15/Zerve-Multi-Terminal-Project-Manager)

## 🙏 Acknowledgments

Built with:
- [WPF UI](https://github.com/lepoco/wpfui) - Modern WPF controls
- .NET 8.0 - Cross-platform framework
- Love and dedication ❤️

---

**Made with ❤️ by Beratkan15**

*If you find this project useful, please give it a ⭐ on GitHub!*
