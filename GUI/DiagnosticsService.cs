using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Core.Services;

namespace TaskTracker.Core.Support;
public class DiagnosticsService
{
    private readonly string _baseDir;
    private readonly string _configPath;
    private readonly string _dataFolder;
    private readonly string _logsFolder;
    private readonly string _backupsFolder;
    private readonly string _exportsFolder;
    private readonly string _reportsFolder;
    private readonly string _dataFilePath;
    private readonly string _role;
    private readonly string _storageMode;

    public DiagnosticsService(string baseDir, string configPath, string role, string storageMode,
        string dataFolder, string logsFolder, string backupsFolder, string exportsFolder, string reportsFolder, string dataFilePath)
    {
        _baseDir = baseDir;
        _configPath = configPath;
        _role = role;
        _storageMode = storageMode;
        _dataFolder = dataFolder;
        _logsFolder = logsFolder;
        _backupsFolder = backupsFolder;
        _exportsFolder = exportsFolder;
        _reportsFolder = reportsFolder;
        _dataFilePath = dataFilePath;
    }

    public List<string> Run(TaskService service)
    {
        var lines = new List<string>();
        lines.Add("=== Diagnostics ===");
        lines.Add($"DateTime: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        lines.Add($"BaseDir: {_baseDir}");
        var version = typeof(DiagnosticsService).Assembly.GetName().Version?.ToString() ?? "unknown";
        lines.Add($"AppVersion: {version}");
        lines.Add($"Role: {_role}");
        lines.Add($"StorageMode: {_storageMode}");
        lines.Add($"ConfigPath exists: {File.Exists(_configPath)}");
        lines.Add($"TasksFile exists: {File.Exists(_dataFilePath)}");
        lines.Add("");
        lines.Add("Folders check:");
        lines.Add(CheckFolder("Data", _dataFolder));
        lines.Add(CheckFolder("Logs", _logsFolder));
        lines.Add(CheckFolder("Backups", _backupsFolder));
        lines.Add(CheckFolder("Exports", _exportsFolder));
        lines.Add(CheckFolder("Reports", _reportsFolder));
        lines.Add("");
        lines.Add("Tasks summary:");
        lines.Add($"Total: {service.GetAll().Count}");
        lines.Add($"Active: {service.GetAllActive().Count}");
        lines.Add($"Trash: {service.GetTrash().Count}");
        lines.Add($"Archive: {service.GetArchive().Count}");
        var todayLog = Path.Combine(_logsFolder, $"app_{DateTime.Now:yyyy-MM-dd}.log");
        var todayErr = Path.Combine(_logsFolder, $"errors_{DateTime.Now:yyyy-MM-dd}.log");
        lines.Add("");
        lines.Add("Logs:");
        lines.Add($"Today log exists: {File.Exists(todayLog)}");
        lines.Add($"Today errors exists: {File.Exists(todayErr)}");
        lines.Add("===================");
        return lines;
    }

    private static string CheckFolder(string name, string path)
    {
        try
        {
            Directory.CreateDirectory(path);
            var testFile = Path.Combine(path, ".write_test.tmp");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return $"{name}: OK ({path})";
        }
        catch (Exception ex)
        {
            return $"{name}: ERROR ({path}) -> {ex.Message}";
        }
    }
}