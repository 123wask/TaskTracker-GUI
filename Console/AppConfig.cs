using System;
using System.IO;
using System.Text.Json;


namespace TaskTracker.Config
{
    public class AppConfig
    {
        public string LastFilterText { get; set; } = "";
        public string LastFilterStatus { get; set; } = "Any";
        public string Role { get; set; } = "Admin";
        public string StorageMode { get; set; } = "JSON"; 

        private string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "config.json");

        public static AppConfig Load()
        {
            var cfg = new AppConfig();

            Directory.CreateDirectory(Path.GetDirectoryName(cfg.path)!);

            if (!File.Exists(cfg.path))
            {
                cfg.Save();
                return cfg;
            }

            return JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(cfg.path))!;
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            File.WriteAllText(path,
                JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}