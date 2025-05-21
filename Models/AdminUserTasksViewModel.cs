using System.Collections.Generic;

namespace ToDoApp.Models
{
    public class AdminUserTasksViewModel
    {
        public User User { get; set; }
        public List<Task> Tasks { get; set; }
    }
}
