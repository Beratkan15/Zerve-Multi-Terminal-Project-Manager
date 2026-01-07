using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Zerve.Services
{
    public class ProcessManager
    {
        private readonly Dictionary<Guid, ProcessInfo> _processes = new();
        private readonly Dictionary<Guid, StringBuilder> _logHistory = new();

        public event EventHandler<LogEventArgs>? LogReceived;

        public void StartProcess(Guid projectId, string command, string workingDirectory)
        {
            if (_processes.ContainsKey(projectId))
            {
                StopProcess(projectId);
            }

            // Clear old logs
            if (!_logHistory.ContainsKey(projectId))
            {
                _logHistory[projectId] = new StringBuilder();
            }
            else
            {
                _logHistory[projectId].Clear();
            }

            var processInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            var process = new Process { StartInfo = processInfo };
            
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    var logEvent = new LogEventArgs(projectId, e.Data, false);
                    _logHistory[projectId].AppendLine($"[{logEvent.Timestamp:HH:mm:ss}] [INFO]  {e.Data}");
                    LogReceived?.Invoke(this, logEvent);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    var logEvent = new LogEventArgs(projectId, e.Data, true);
                    _logHistory[projectId].AppendLine($"[{logEvent.Timestamp:HH:mm:ss}] [ERROR] {e.Data}");
                    LogReceived?.Invoke(this, logEvent);
                }
            };

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                _processes[projectId] = new ProcessInfo
                {
                    Process = process,
                    Command = command,
                    WorkingDirectory = workingDirectory
                };

                var startLog = new LogEventArgs(projectId, $"Started: {command}", false);
                _logHistory[projectId].AppendLine($"[{startLog.Timestamp:HH:mm:ss}] [INFO]  Started: {command}");
                LogReceived?.Invoke(this, startLog);
            }
            catch (Exception ex)
            {
                var errorLog = new LogEventArgs(projectId, $"Error starting process: {ex.Message}", true);
                _logHistory[projectId].AppendLine($"[{errorLog.Timestamp:HH:mm:ss}] [ERROR] Error starting process: {ex.Message}");
                LogReceived?.Invoke(this, errorLog);
            }
        }

        public void StopProcess(Guid projectId)
        {
            if (_processes.TryGetValue(projectId, out var processInfo))
            {
                try
                {
                    if (!processInfo.Process.HasExited)
                    {
                        // Kill the entire process tree (including child processes like node.js servers)
                        KillProcessTree(processInfo.Process.Id);
                        
                        var stopLog = new LogEventArgs(projectId, "Process stopped", false);
                        _logHistory[projectId].AppendLine($"[{stopLog.Timestamp:HH:mm:ss}] [INFO]  Process stopped");
                        LogReceived?.Invoke(this, stopLog);
                    }
                }
                catch (Exception ex)
                {
                    var errorLog = new LogEventArgs(projectId, $"Error stopping process: {ex.Message}", true);
                    _logHistory[projectId].AppendLine($"[{errorLog.Timestamp:HH:mm:ss}] [ERROR] Error stopping process: {ex.Message}");
                    LogReceived?.Invoke(this, errorLog);
                }
                finally
                {
                    try
                    {
                        processInfo.Process.Dispose();
                    }
                    catch { }
                    _processes.Remove(projectId);
                }
            }
        }

        private void KillProcessTree(int processId)
        {
            try
            {
                // Use taskkill to kill the process tree (parent and all children)
                var killProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "taskkill",
                        Arguments = $"/PID {processId} /T /F",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                killProcess.Start();
                killProcess.WaitForExit(5000); // Wait max 5 seconds
            }
            catch
            {
                // Fallback to regular kill if taskkill fails
                try
                {
                    var process = Process.GetProcessById(processId);
                    process.Kill(true);
                }
                catch { }
            }
        }

        public void RestartProcess(Guid projectId)
        {
            if (_processes.TryGetValue(projectId, out var processInfo))
            {
                var command = processInfo.Command;
                var workingDir = processInfo.WorkingDirectory;
                
                var restartLog = new LogEventArgs(projectId, "Restarting process...", false);
                _logHistory[projectId].AppendLine($"[{restartLog.Timestamp:HH:mm:ss}] [INFO]  Restarting process...");
                LogReceived?.Invoke(this, restartLog);
                
                StopProcess(projectId);
                
                // Wait a bit for port to be released
                System.Threading.Thread.Sleep(1000);
                
                StartProcess(projectId, command, workingDir);
            }
        }

        public bool IsRunning(Guid projectId)
        {
            if (_processes.TryGetValue(projectId, out var processInfo))
            {
                try
                {
                    return !processInfo.Process.HasExited;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        public string GetLogHistory(Guid projectId)
        {
            if (_logHistory.TryGetValue(projectId, out var logs))
            {
                return logs.ToString();
            }
            return string.Empty;
        }

        public void StopAll()
        {
            foreach (var projectId in new List<Guid>(_processes.Keys))
            {
                StopProcess(projectId);
            }
        }

        private class ProcessInfo
        {
            public Process Process { get; set; } = null!;
            public string Command { get; set; } = string.Empty;
            public string WorkingDirectory { get; set; } = string.Empty;
        }
    }

    public class LogEventArgs : EventArgs
    {
        public Guid ProjectId { get; }
        public string Message { get; }
        public bool IsError { get; }
        public DateTime Timestamp { get; }

        public LogEventArgs(Guid projectId, string message, bool isError)
        {
            ProjectId = projectId;
            Message = message;
            IsError = isError;
            Timestamp = DateTime.Now;
        }
    }
}
