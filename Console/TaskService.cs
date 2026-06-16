using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskStatus = TaskTracker.Models.TaskStatus;
using TaskTracker.Models;

namespace TaskTracker.Core.Services
{
    public class TaskService
    {
        private List<TaskItem> _tasks = new();
        private int _nextId = 1;

        public void Load(List<TaskItem> tasks)
        {
            _tasks = tasks;
            _nextId = _tasks.Count == 0 ? 1 : _tasks.Max(x => x.Id) + 1;
        }

        public List<TaskItem> GetAll() => _tasks;

        public List<TaskItem> GetAllActive()
        {
            return _tasks.Where(t => !t.IsDeleted && !t.IsArchived).ToList();
        }

        public void Add(string title, string desc)
        {
            _tasks.Add(new TaskItem
            {
                Id = _nextId++,
                Title = title,
                Description = desc
            });
        }

        public void Delete(int id)
        {
            var t = GetExisting(id);
            t.IsDeleted = true;
        }

        public List<TaskItem> GetTrash()
        {
            return _tasks.Where(t => t.IsDeleted).ToList();
        }

        public void Restore(int id)
        {
            var t = GetExisting(id);
            t.IsDeleted = false;
        }

        public int ClearTrash()
        {
            int before = _tasks.Count;
            _tasks.RemoveAll(t => t.IsDeleted);
            return before - _tasks.Count;
        }

        // 🔥 Архив
        public void Archive(int id)
        {
            var t = GetExisting(id);
            t.IsArchived = true;
        }

        public void Unarchive(int id)
        {
            var t = GetExisting(id);
            t.IsArchived = false;
        }

        public List<TaskItem> GetArchive()
        {
            return _tasks.Where(t => !t.IsDeleted && t.IsArchived).ToList();
        }

        // 🔥 Фильтр
        public List<TaskItem> SearchAdvanced(string? text, TaskStatus? status)
        {
            var query = (text ?? "").Trim();

            return _tasks.Where(t =>
                !t.IsDeleted &&
                !t.IsArchived &&
                (string.IsNullOrEmpty(query) ||
                 t.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                 t.Description.Contains(query, StringComparison.OrdinalIgnoreCase)) &&
                (!status.HasValue || t.Status == status)
            ).ToList();
        }

        private TaskItem GetExisting(int id)
        {
            return _tasks.First(x => x.Id == id);
        }
    }
}
