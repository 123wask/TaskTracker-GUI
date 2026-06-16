using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TaskTracker.Core.Models;

namespace TaskTracker.Storage.Services;
public class JsonTaskStorage : ITaskStorage
{
    private readonly string _filePath;
    public JsonTaskStorage(string filePath) => _filePath = filePath;
    public List<TaskItem> Load()
    {
        if (!File.Exists(_filePath)) return new List<TaskItem>();
        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<TaskItem>>(json) ?? new List<TaskItem>();
        }
        catch { return new List<TaskItem>(); }
    }
    public void Save(List<TaskItem> tasks)
    {
        var dir = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}