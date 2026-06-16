using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Core.Models;

namespace TaskTracker.App.UI;
public static class ConsoleUi
{
    public static void PrintTasks(List<TaskItem> tasks, string title = "Список задач")
    {
        if (tasks.Count == 0) { Console.WriteLine($"{title} → пусто."); return; }
        Console.WriteLine($"\n--- {title} ---");
        foreach (var t in tasks)
            Console.WriteLine($"{t.Id,3} | {t.Title,-25} | {t.Status}");
    }
}