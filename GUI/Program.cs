using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Core.Services;
using TaskTracker.Core.Help;
using TaskTracker.Core.Support;
using TaskTracker.Storage.Services;
using TaskTracker.Storage;
using TaskTracker.App.UI;

namespace TaskTracker.App;
internal class Program
{
    static void Main()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var dataFolder = Path.Combine(baseDir, "Data");
        var logsFolder = Path.Combine(baseDir, "Logs");
        var reportsFolder = Path.Combine(baseDir, "Reports");
        var backupsFolder = Path.Combine(baseDir, "Backups");
        var exportsFolder = Path.Combine(baseDir, "Exports");
        var configPath = Path.Combine(baseDir, "Config", "config.json");
        var dataFilePath = Path.Combine(dataFolder, "tasks.json");

        ITaskStorage storage = new JsonTaskStorage(dataFilePath);
        var loadedTasks = storage.Load();
        var service = new TaskService(loadedTasks);
        var logger = new AppLogger(logsFolder);
        logger.Info("Консольное приложение запущено");

        var diagnostics = new DiagnosticsService(baseDir, configPath, "Admin", "JSON",
            dataFolder, logsFolder, backupsFolder, exportsFolder, reportsFolder, dataFilePath);

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== TaskTracker v1.0 (консоль) ===\n");
            Console.WriteLine("1. Добавить задачу");
            Console.WriteLine("2. Показать активные");
            Console.WriteLine("3. Изменить статус");
            Console.WriteLine("4. Удалить (в корзину)");
            Console.WriteLine("21. Корзина");
            Console.WriteLine("22. Восстановить");
            Console.WriteLine("24. Архивировать");
            Console.WriteLine("25. Архив");
            Console.WriteLine("26. Вернуть из архива");
            Console.WriteLine("27. Справка");
            Console.WriteLine("28. Диагностика");
            Console.WriteLine("29. Отчёт поддержки");
            Console.WriteLine("0. Выход");
            Console.Write("\nВаш выбор: ");
            var input = Console.ReadLine();

            if (input == "0") break;

            try
            {
                switch (input)
                {
                    case "1":
                        Console.Write("Название: ");
                        var title = Console.ReadLine();
                        service.Add(title ?? "");
                        storage.Save(service.GetAll());
                        logger.Info($"ADD: {title}");
                        break;
                    case "2":
                        ConsoleUi.PrintTasks(service.GetAllActive());
                        break;
                    case "3":
                        Console.Write("Id: ");
                        if (int.TryParse(Console.ReadLine(), out int sid))
                        {
                            Console.Write("Новый статус (0-New,1-InProgress,2-Done): ");
                            if (int.TryParse(Console.ReadLine(), out int st) && st >= 0 && st <= 2)
                            {
                                service.ChangeStatus(sid, (TaskStatus)st);
                                storage.Save(service.GetAll());
                                logger.Info($"STATUS id={sid} new={(TaskStatus)st}");
                            }
                        }
                        break;
                    case "4":
                        Console.Write("Id: ");
                        if (int.TryParse(Console.ReadLine(), out int id))
                        {
                            service.Delete(id);
                            storage.Save(service.GetAll());
                            logger.Info($"DELETE id={id}");
                        }
                        break;
                    case "21":
                        ConsoleUi.PrintTasks(service.GetTrash(), "Корзина");
                        break;
                    case "22":
                        Console.Write("Id: ");
                        if (int.TryParse(Console.ReadLine(), out int rid))
                        {
                            service.Restore(rid);
                            storage.Save(service.GetAll());
                            logger.Info($"RESTORE id={rid}");
                        }
                        break;
                    case "24":
                        Console.Write("Id для архива: ");
                        if (int.TryParse(Console.ReadLine(), out int aid))
                        {
                            service.Archive(aid);
                            storage.Save(service.GetAll());
                            logger.Info($"ARCHIVE id={aid}");
                        }
                        break;
                    case "25":
                        ConsoleUi.PrintTasks(service.GetArchive(), "Архив");
                        break;
                    case "26":
                        Console.Write("Id для возврата: ");
                        if (int.TryParse(Console.ReadLine(), out int uid))
                        {
                            service.Unarchive(uid);
                            storage.Save(service.GetAll());
                            logger.Info($"UNARCHIVE id={uid}");
                        }
                        break;
                    case "27":
                        Console.WriteLine(HelpText.Get());
                        break;
                    case "28":
                        var diagLines = diagnostics.Run(service);
                        foreach (var line in diagLines) Console.WriteLine(line);
                        break;
                    case "29":
                        Directory.CreateDirectory(reportsFolder);
                        var lines = diagnostics.Run(service);
                        var file = Path.Combine(reportsFolder, $"SupportReport_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
                        File.WriteAllLines(file, lines);
                        Console.WriteLine($"Отчёт создан: {file}");
                        logger.Info($"REPORT created {file}");
                        break;
                }
            }
            catch (Exception ex) { Console.WriteLine($"Ошибка: {ex.Message}"); }
            Console.WriteLine("\nНажмите любую клавишу...");
            Console.ReadKey();
        }
    }
}