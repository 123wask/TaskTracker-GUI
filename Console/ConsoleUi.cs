using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Models;
using TaskStatus = TaskTracker.Models.TaskStatus;

namespace TaskTracker.UI
{
    public static class ConsoleUi
    {
        private static void WriteColor(string text, ConsoleColor color, bool lineBreak = true)
        {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            if (lineBreak) Console.WriteLine(text);
            else Console.Write(text);
            Console.ForegroundColor = old;
        }

        private static void WriteHeader(string text)
        {
            WriteColor($"\n--- {text} ---", ConsoleColor.Cyan);
        }

        public static void PrintMenu()
        {
            WriteColor("╔════════════════════════════════════════╗", ConsoleColor.DarkYellow);
            WriteColor("║              TASK TRACKER              ║", ConsoleColor.DarkYellow);
            WriteColor("║    Управление задачами, архив, отчёты  ║", ConsoleColor.DarkYellow);
            WriteColor("╚════════════════════════════════════════╝", ConsoleColor.DarkYellow);
            WriteColor("\nВведите номер команды.\n", ConsoleColor.Green);

            WriteHeader("ГЛАВНОЕ МЕНЮ");

            WriteColor("\n[ ОСНОВНЫЕ ]", ConsoleColor.Yellow);
            WriteColor("1  [+] Добавить задачу", ConsoleColor.White);
            WriteColor("2  [>] Показать активные", ConsoleColor.White);
            WriteColor("4  [X] Удалить (в корзину)", ConsoleColor.White);
            WriteColor("19 [~] Расширенный фильтр", ConsoleColor.White);
            WriteColor("20 [R] Повтор фильтра", ConsoleColor.White);

            WriteColor("\n[ КОРЗИНА И АРХИВ ]", ConsoleColor.Yellow);
            WriteColor("21 [#] Корзина", ConsoleColor.White);
            WriteColor("22 [^] Восстановить", ConsoleColor.White);
            WriteColor("23 [!] Очистить корзину", ConsoleColor.Red);
            WriteColor("24 [Z] Архивировать", ConsoleColor.White);
            WriteColor("25 [A] Архив", ConsoleColor.White);
            WriteColor("26 [U] Вернуть из архива", ConsoleColor.White);

            WriteColor("\n[ ДИАГНОСТИКА ]", ConsoleColor.Yellow);
            WriteColor("27 [?] Справка", ConsoleColor.White);
            WriteColor("28 [D] Диагностика", ConsoleColor.White);
            WriteColor("29 [R] Отчёт поддержки", ConsoleColor.White);

            WriteColor("\n[ СИСТЕМА ]", ConsoleColor.Yellow);
            WriteColor("0  [X] Выход", ConsoleColor.Red);

            Console.Write("\n>> Ваш выбор: ");
        }











        public static void PrintTasks(List<TaskItem> tasks, string title = "СПИСОК ЗАДАЧ")
        {
            if (tasks == null || tasks.Count == 0)
            {
                WriteColor($"\n{title} → пусто.", ConsoleColor.DarkGray);
                return;
            }

            WriteHeader(title);
            foreach (var t in tasks)
            {
                // Определяем цвет в зависимости от значения enum
                ConsoleColor statusColor = t.Status switch
                {
                    TaskStatus.New => ConsoleColor.Cyan,
                    TaskStatus.InProgress => ConsoleColor.Yellow,
                    TaskStatus.Done => ConsoleColor.Green,
                    _ => ConsoleColor.Gray
                };

                Console.Write($"  {t.Id,3} | ");
                WriteColor($"{t.Title,-25}", ConsoleColor.White, false);
                Console.Write(" | ");
                // Преобразуем enum в строку для вывода
                WriteColor(t.Status.ToString(), statusColor);
                Console.WriteLine();
            }
        }










        public static int AskForId(string prompt = "Введите Id задачи: ")
        {
            WriteColor(prompt, ConsoleColor.DarkCyan, false);
            string? input = Console.ReadLine();
            if (int.TryParse(input?.Trim(), out int id))
                return id;
            WriteColor("❌ Ошибка: Id должен быть числом. Повторите.", ConsoleColor.Red);
            return AskForId(prompt);
        }

        public static void ShowSuccess(string message) => WriteColor($"✅ {message}", ConsoleColor.Green);
        public static void ShowError(string message) => WriteColor($"❌ {message}", ConsoleColor.Red);
        public static void ShowInfo(string message) => WriteColor($"ℹ️ {message}", ConsoleColor.Cyan);
    }
}

