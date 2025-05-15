using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Models
{
    public class Status
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
