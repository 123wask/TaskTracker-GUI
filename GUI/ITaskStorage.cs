using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Core.Models;

namespace TaskTracker.Storage;
public interface ITaskStorage
{
    List<TaskItem> Load();
    void Save(List<TaskItem> tasks);
}