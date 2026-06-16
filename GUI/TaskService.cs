using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Core.Models;
using TaskTracker.Core.Validation;
using TaskTracker.Core.Reports;
using TaskStatus = TaskTracker.Core.Models.TaskStatus;

namespace TaskTracker.Core.Services;
public class TaskService
{
    private readonly List<TaskItem> _tasks;
    private int _nextId;

    public TaskService(List<TaskItem>? initialTasks = null)
    {
        _tasks = initialTasks ?? new List<TaskItem>();
        _nextId = _tasks.Count == 0 ? 1 : _tasks.Max(t => t.Id) + 1;
    }

    public TaskItem Add(string title, string description = "")
    {
        var task = new TaskItem { Id = _nextId, Title = title, Description = description, Status = TaskStatus.New };
        var error = TaskValidator.Validate(task);
        if (error != null) throw new ArgumentException(error);
        task.Title = task.Title.Trim();
        task.Description = (task.Description ?? "").Trim();
        _tasks.Add(task);
        _nextId++;
        return task;
    }

    public List<TaskItem> GetAll() => _tasks.ToList();

    public List<TaskItem> GetAllActive() => _tasks.Where(t => !t.IsDeleted && !t.IsArchived).ToList();

    public List<TaskItem> GetTrash() => _tasks.Where(t => t.IsDeleted).ToList();

    public void Restore(int id)
    {
        var task = GetExisting(id);
        if (!task.IsDeleted) throw new ArgumentException("Задача не в корзине.");
        task.IsDeleted = false;
    }

    public void ClearTrash()
    {
        _tasks.RemoveAll(t => t.IsDeleted);
    }

    public void Delete(int id)
    {
        var task = GetExisting(id);
        task.IsDeleted = true;
    }

    public void Archive(int id)
    {
        var task = GetExisting(id);
        if (task.IsDeleted) throw new ArgumentException("Нельзя архивировать удалённую задачу.");
        if (task.IsArchived) throw new ArgumentException("Задача уже в архиве.");
        task.IsArchived = true;
    }

    public void Unarchive(int id)
    {
        var task = GetExisting(id);
        if (task.IsDeleted) throw new ArgumentException("Задача в корзине. Восстановите её.");
        if (!task.IsArchived) throw new ArgumentException("Задача не в архиве.");
        task.IsArchived = false;
    }

    public List<TaskItem> GetArchive() => _tasks.Where(t => !t.IsDeleted && t.IsArchived).ToList();

    public TaskItem ChangeStatus(int id, TaskStatus newStatus)
    {
        var task = GetExisting(id);
        task.Status = newStatus;
        return task;
    }

    public TaskItem Update(int id, string newTitle, string newDescription)
    {
        var task = GetExisting(id);
        var originalTitle = task.Title;
        var originalDesc = task.Description;
        task.Title = newTitle;
        task.Description = newDescription;
        var error = TaskValidator.Validate(task);
        if (error != null)
        {
            task.Title = originalTitle;
            task.Description = originalDesc;
            throw new ArgumentException(error);
        }
        task.Title = task.Title.Trim();
        task.Description = (task.Description ?? "").Trim();
        return task;
    }

    public List<TaskItem> SearchAdvanced(string? text, TaskStatus? status)
    {
        var query = (text ?? "").Trim();
        var hasText = query.Length > 0;
        return _tasks.Where(t =>
            (!status.HasValue || t.Status == status.Value) &&
            (!hasText || (t.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) == true ||
                          t.Description?.Contains(query, StringComparison.OrdinalIgnoreCase) == true))
        ).ToList();
    }

    public List<TaskItem> FilterByStatus(TaskStatus? status) =>
        status == null ? GetAll() : _tasks.Where(t => t.Status == status).ToList();

    public List<TaskItem> SortById(bool ascending = true) =>
        ascending ? _tasks.OrderBy(t => t.Id).ToList() : _tasks.OrderByDescending(t => t.Id).ToList();

    public TaskStats GetStats()
    {
        var all = GetAll();
        return new TaskStats
        {
            Total = all.Count,
            NewCount = all.Count(t => t.Status == TaskStatus.New),
            InProgressCount = all.Count(t => t.Status == TaskStatus.InProgress),
            DoneCount = all.Count(t => t.Status == TaskStatus.Done)
        };
    }

    public void ReplaceAll(List<TaskItem> newTasks)
    {
        _tasks.Clear();
        _tasks.AddRange(newTasks);
        _nextId = _tasks.Count == 0 ? 1 : _tasks.Max(t => t.Id) + 1;
    }

    public (int added, int skipped) MergeImport(List<TaskItem> imported)
    {
        imported ??= new List<TaskItem>();
        int added = 0, skipped = 0;
        foreach (var t in imported)
        {
            if (t == null) { skipped++; continue; }
            bool duplicate = _tasks.Any(x =>
                string.Equals(x.Title, t.Title, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Description, t.Description, StringComparison.OrdinalIgnoreCase) &&
                x.Status == t.Status);
            if (duplicate) { skipped++; continue; }
            int newId = t.Id;
            if (_tasks.Any(x => x.Id == newId))
                newId = GetNextFreeId();
            var copy = new TaskItem
            {
                Id = newId,
                Title = t.Title ?? "",
                Description = t.Description ?? "",
                Status = t.Status,
                IsDeleted = t.IsDeleted,
                IsArchived = t.IsArchived
            };
            _tasks.Add(copy);
            added++;
        }
        return (added, skipped);
    }

    private int GetNextFreeId()
    {
        while (_tasks.Any(t => t.Id == _nextId)) _nextId++;
        return _nextId++;
    }

    private TaskItem GetExisting(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task == null) throw new ArgumentException($"Задача с Id={id} не найдена.");
        return task;
    }

    public void PermanentDelete(int id)
    {
        var task = GetExisting(id);
        _tasks.Remove(task);
    }
}