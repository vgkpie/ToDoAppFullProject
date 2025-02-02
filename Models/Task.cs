using System.ComponentModel.DataAnnotations; 
using System.ComponentModel.DataAnnotations.Schema; // Add this line for foreign key

namespace ToDoApp.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; } // Auto Incremented
        [Required]
        public int UserId { get; set; } // New property to link tasks to users
        [Required]
        public string Title { get; set; } // Required
        [Required]
        public string Description { get; set; } // Required
        [Required]
        public DateTime DueDate { get; set; } // Date and Time
        [Required]
        public string Priority { get; set; } // High, Medium, Low
        [Required]
        public string Category { get; set; } // Work, Personal, Shopping, etc.
        [Required]
        public string Status { get; set; }  // To Do, In Progress, Done
        [Required]
        public bool IsCompleted { get; set; } // True or False
    }
}
