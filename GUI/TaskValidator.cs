using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Core.Models;

namespace TaskTracker.Core.Validation;
public static class TaskValidator
{
    public const int TitleMaxLength = 50;
    public const int DescriptionMaxLength = 300;

    public static string? Validate(TaskItem task)
    {
        if (task is null) return "Задача не должна быть null.";
        if (string.IsNullOrWhiteSpace(task.Title))
            return "Название (Title) обязательно.";
        var title = task.Title.Trim();
        if (title.Length > TitleMaxLength)
            return $"Название слишком длинное. Максимум {TitleMaxLength} символов.";
        var desc = (task.Description ?? "").Trim();
        if (desc.Length > DescriptionMaxLength)
            return $"Описание слишком длинное. Максимум {DescriptionMaxLength} символов.";
        return null;
    }
}