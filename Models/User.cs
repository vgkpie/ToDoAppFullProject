using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; } // Auto Incremented

        [Required]
        [StringLength(100)]
        public string Username { get; set; } // Required

        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; } // Required for storing hashed password

        [Required]
        [EmailAddress]
        public string Email { get; set; } // Required for user email

        // Parameterless constructor
        public User() { }

        // Constructor to initialize required properties
        public User(string username, string passwordHash, string email)
        {
            Username = username;
            PasswordHash = passwordHash;
            Email = email;
        }
    }
}
