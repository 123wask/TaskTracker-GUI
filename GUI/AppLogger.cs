using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskTracker.Storage.Services;
public class AppLogger
{
    private readonly string _logDir;
    public AppLogger(string logDir)
    {
        _logDir = logDir;
        Directory.CreateDirectory(_logDir);
    }
    private string GetLogFilePath() => Path.Combine(_logDir, $"app_{DateTime.Now:yyyy-MM-dd}.log");
    private string GetErrorFilePath() => Path.Combine(_logDir, $"errors_{DateTime.Now:yyyy-MM-dd}.log");
    public void Info(string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} INFO {message}";
        File.AppendAllText(GetLogFilePath(), line + Environment.NewLine);
    }
    public void Error(string message)
    {
        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR {message}";
        File.AppendAllText(GetLogFilePath(), line + Environment.NewLine);
    }
    public void Exception(string context, Exception ex)
    {
        Error($"{context}: {ex.Message}");
        var details = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} EXCEPTION {context}{Environment.NewLine}{ex}{Environment.NewLine}------------------------------------------{Environment.NewLine}";
        File.AppendAllText(GetErrorFilePath(), details);
    }
}