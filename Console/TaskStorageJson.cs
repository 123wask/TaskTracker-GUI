using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using TaskTracker.Models;

namespace TaskTracker.Storage
{
    public class TaskStorageJson
    {
        private string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "tasks.json");

        public List<TaskItem> Load()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            if (!File.Exists(path))
                File.WriteAllText(path, "[]");

            return JsonSerializer.Deserialize<List<TaskItem>>(File.ReadAllText(path))!;
        }

        public void Save(List<TaskItem> tasks)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            File.WriteAllText(path,
                JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}