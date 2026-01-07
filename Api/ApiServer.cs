using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using Zerve.Models;
using Zerve.Services;

namespace Zerve.Api
{
    public class ApiServer
    {
        private HttpListener? _listener;
        private Thread? _listenerThread;
        private readonly Func<List<Project>> _getProjects;
        private readonly ProcessManager _processManager;
        private readonly Action _saveProjects;
        private readonly Action _updateUI;

        public ApiServer(Func<List<Project>> getProjects, ProcessManager processManager, Action saveProjects, Action updateUI)
        {
            _getProjects = getProjects;
            _processManager = processManager;
            _saveProjects = saveProjects;
            _updateUI = updateUI;
        }

        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:27015/");
            _listener.Start();

            _listenerThread = new Thread(HandleRequests);
            _listenerThread.Start();
        }

        public void Stop()
        {
            _listener?.Stop();
            _listenerThread?.Join();
        }

        private void HandleRequests()
        {
            while (_listener != null && _listener.IsListening)
            {
                try
                {
                    var context = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(_ => ProcessRequest(context));
                }
                catch { }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.ContentType = "application/json";

            try
            {
                var path = request.Url?.AbsolutePath ?? "";
                var method = request.HttpMethod;

                object? result = null;

                if (path == "/api/projects" && method == "GET")
                {
                    result = _getProjects().Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.CustomId,
                        p.FolderPath,
                        p.Command,
                        p.IsRunning
                    });
                }
                else if (path.StartsWith("/api/project/") && method == "GET")
                {
                    var customId = path.Replace("/api/project/", "");
                    var project = _getProjects().FirstOrDefault(p => p.CustomId == customId);
                    if (project != null)
                    {
                        result = new
                        {
                            project.Id,
                            project.Name,
                            project.CustomId,
                            project.FolderPath,
                            project.Command,
                            project.IsRunning
                        };
                    }
                    else
                    {
                        response.StatusCode = 404;
                        result = new { error = "Project not found" };
                    }
                }
                else if (path.StartsWith("/api/run/") && method == "POST")
                {
                    var customId = path.Replace("/api/run/", "");
                    var project = _getProjects().FirstOrDefault(p => p.CustomId == customId);
                    if (project != null && !project.IsRunning)
                    {
                        _processManager.StartProcess(project.Id, project.Command, project.FolderPath);
                        project.IsRunning = true;
                        _saveProjects();
                        _updateUI();
                        result = new { success = true, message = $"Started {project.Name}" };
                    }
                    else if (project == null)
                    {
                        response.StatusCode = 404;
                        result = new { error = "Project not found" };
                    }
                    else
                    {
                        result = new { success = false, message = "Project already running" };
                    }
                }
                else if (path.StartsWith("/api/restart/") && method == "POST")
                {
                    var customId = path.Replace("/api/restart/", "");
                    var project = _getProjects().FirstOrDefault(p => p.CustomId == customId);
                    if (project != null && project.IsRunning)
                    {
                        _processManager.RestartProcess(project.Id);
                        _updateUI();
                        result = new { success = true, message = $"Restarted {project.Name}" };
                    }
                    else if (project == null)
                    {
                        response.StatusCode = 404;
                        result = new { error = "Project not found" };
                    }
                    else
                    {
                        result = new { success = false, message = "Project not running" };
                    }
                }
                else if (path.StartsWith("/api/stop/") && method == "POST")
                {
                    var customId = path.Replace("/api/stop/", "");
                    var project = _getProjects().FirstOrDefault(p => p.CustomId == customId);
                    if (project != null && project.IsRunning)
                    {
                        _processManager.StopProcess(project.Id);
                        project.IsRunning = false;
                        _saveProjects();
                        _updateUI();
                        result = new { success = true, message = $"Stopped {project.Name}" };
                    }
                    else if (project == null)
                    {
                        response.StatusCode = 404;
                        result = new { error = "Project not found" };
                    }
                    else
                    {
                        result = new { success = false, message = "Project not running" };
                    }
                }
                else if (path.StartsWith("/api/logs/") && method == "GET")
                {
                    var customId = path.Replace("/api/logs/", "");
                    var project = _getProjects().FirstOrDefault(p => p.CustomId == customId);
                    if (project != null)
                    {
                        var logs = _processManager.GetLogHistory(project.Id);
                        result = new { projectName = project.Name, logs };
                    }
                    else
                    {
                        response.StatusCode = 404;
                        result = new { error = "Project not found" };
                    }
                }
                else
                {
                    response.StatusCode = 404;
                    result = new { error = "Endpoint not found" };
                }

                var json = JsonSerializer.Serialize(result);
                var buffer = Encoding.UTF8.GetBytes(json);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                var error = JsonSerializer.Serialize(new { error = ex.Message });
                var buffer = Encoding.UTF8.GetBytes(error);
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                response.OutputStream.Close();
            }
        }
    }
}
