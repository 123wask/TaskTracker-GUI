using System;
using System.IO;
using TaskTracker.App.Support;  
using TaskTracker.Services;
using TaskTracker.UI;
using TaskTracker.Config;
using TaskTracker.Storage;
using TaskTracker.Core.Services;
using TaskTracker.Help;
using TaskTracker.Models;



string baseDir = AppDomain.CurrentDomain.BaseDirectory;
string dataFolder = Path.Combine(baseDir, "Data");
string logsFolder = Path.Combine(baseDir, "Logs");
string reportsFolder = Path.Combine(baseDir, "Reports");
string backupsFolder = Path.Combine(baseDir, "Backups");
string exportsFolder = Path.Combine(baseDir, "Exports");
string configPath = Path.Combine(baseDir, "Config", "config.json");
string dataFilePath = Path.Combine(dataFolder, "tasks.json");

var cfg = AppConfig.Load();
var storage = new TaskStorageJson();
var service = new TaskService();
service.Load(storage.Load());

var diagnostics = new DiagnosticsService(
    baseDir, configPath, cfg,
    dataFolder, logsFolder, backupsFolder,
    exportsFolder, reportsFolder, dataFilePath
);

while (true)
{
    ConsoleUi.PrintMenu();
    string? input = Console.ReadLine();

    if (input == "0") break;

    // 1. Добавить задачу
    if (input == "1")
    {
        ConsoleUi.ShowInfo("Введите название:");
        string? title = Console.ReadLine();
        ConsoleUi.ShowInfo("Введите описание:");
        string? desc = Console.ReadLine();
        service.Add(title ?? "", desc ?? "");
        storage.Save(service.GetAll());
        ConsoleUi.ShowSuccess("Задача добавлена.");
    }
    // 2. Показать активные задачи
    else if (input == "2")
    {
        ConsoleUi.PrintTasks(service.GetAllActive(), "АКТИВНЫЕ ЗАДАЧИ");
    }
    // 4. Удалить (в корзину)
    else if (input == "4")
    {
        int id = ConsoleUi.AskForId("Id задачи для удаления: ");
        try
        {
            service.Delete(id);
            storage.Save(service.GetAll());
            ConsoleUi.ShowSuccess($"Задача {id} перемещена в корзину.");
        }
        catch (Exception ex)
        {
            ConsoleUi.ShowError(ex.Message);
        }
    }
    // 21. Корзина
    else if (input == "21")
    {
        ConsoleUi.PrintTasks(service.GetTrash(), "КОРЗИНА");
    }
    // 22. Восстановить из корзины
    else if (input == "22")
    {
        int id = ConsoleUi.AskForId("Id задачи для восстановления: ");
        try
        {
            service.Restore(id);
            storage.Save(service.GetAll());
            ConsoleUi.ShowSuccess($"Задача {id} восстановлена.");
        }
        catch (Exception ex)
        {
            ConsoleUi.ShowError(ex.Message);
        }
    }
    // 23. Очистить корзину
    else if (input == "23")
    {
        ConsoleUi.ShowInfo("ВНИМАНИЕ: очистка корзины удаляет задачи НАВСЕГДА.");
        ConsoleUi.ShowInfo("Уверены? (y/n)");
        if (Console.ReadLine()?.ToLower() == "y")
        {
            service.ClearTrash();
            storage.Save(service.GetAll());
            ConsoleUi.ShowSuccess("Корзина очищена.");
        }
        else
        {
            ConsoleUi.ShowInfo("Очистка отменена.");
        }
    }
    // 24. Архивировать
    else if (input == "24")
    {
        int id = ConsoleUi.AskForId("Id задачи для архивации: ");
        try
        {
            service.Archive(id);
            storage.Save(service.GetAll());
            ConsoleUi.ShowSuccess($"Задача {id} отправлена в архив.");
        }
        catch (Exception ex)
        {
            ConsoleUi.ShowError(ex.Message);
        }
    }
    // 25. Показать архив
    else if (input == "25")
    {
        ConsoleUi.PrintTasks(service.GetArchive(), "АРХИВ");
    }
    // 26. Вернуть из архива
    else if (input == "26")
    {
        int id = ConsoleUi.AskForId("Id задачи для возврата из архива: ");
        try
        {
            service.Unarchive(id);
            storage.Save(service.GetAll());
            ConsoleUi.ShowSuccess($"Задача {id} возвращена из архива.");
        }
        catch (Exception ex)
        {
            ConsoleUi.ShowError(ex.Message);
        }
    }
    // 27. Справка
    else if (input == "27")
    {
        Console.WriteLine(HelpText.Get());
    }
    // 28. Диагностика
    else if (input == "28")
    {
        try
        {
            var lines = diagnostics.Run(service);
            foreach (var line in lines) Console.WriteLine(line);
            ConsoleUi.ShowSuccess("Диагностика выполнена.");
        }
        catch (Exception ex)
        {
            ConsoleUi.ShowError($"Ошибка диагностики: {ex.Message}");
        }
    }
    // 29. Отчёт поддержки
    else if (input == "29")
    {
        try
        {
            Directory.CreateDirectory(reportsFolder);
            var lines = diagnostics.Run(service);
            string fileName = $"SupportReport_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
            string filePath = Path.Combine(reportsFolder, fileName);
            File.WriteAllLines(filePath, lines);
            ConsoleUi.ShowSuccess($"Отчёт создан: {filePath}");
        }
        catch (Exception ex)
        {
            ConsoleUi.ShowError($"Ошибка создания отчёта: {ex.Message}");
        }
    }
    // 19. Расширенный фильтр (заглушка)
    else if (input == "19")
    {
        ConsoleUi.ShowInfo("Расширенный фильтр будет добавлен в следующих версиях.");
    }
    // 20. Повтор фильтра (заглушка)
    else if (input == "20")
    {
        ConsoleUi.ShowInfo("Повтор фильтра пока не реализован.");
    }
    // Неизвестная команда
    else if (!string.IsNullOrEmpty(input))
    {
        ConsoleUi.ShowError("Неизвестная команда. Попробуйте снова.");
    }
}