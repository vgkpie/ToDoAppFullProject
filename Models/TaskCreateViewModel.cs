using System.Collections.Generic;
using ToDoApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ToDoApp.Models
{
    public class TaskCreateViewModel
    {
        public Task Task { get; set; }

        [ValidateNever]
        public List<Status> StatusList { get; set; }
    }
}
