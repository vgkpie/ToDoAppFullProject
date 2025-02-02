using Microsoft.EntityFrameworkCore;
using ToDoApp.Models;

namespace ToDoApp.Data
{
    public class ToDoContext : DbContext
    {
        public ToDoContext(DbContextOptions<ToDoContext> options) : base(options)
        {
        }

        public DbSet<ToDoApp.Models.Task> Tasks { get; set; } // Use fully qualified name
        public DbSet<ToDoApp.Models.User> Users { get; set; } // New DbSet for users

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ToDoApp.Models.Task>()
                .HasKey(t => t.Id); // Explicitly define the primary key

            modelBuilder.Entity<ToDoApp.Models.User>()
                .HasKey(u => u.Id); // Explicitly define the primary key for User
        }
    }
}
