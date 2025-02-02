using Microsoft.AspNetCore.Mvc;
using ToDoApp.Models; // Ensure this is the correct namespace
using ToDoApp.Data;
using System.Security.Claims; // For accessing user claims
using Microsoft.Extensions.Logging; // For logging
using System.Linq; // For LINQ operations

namespace ToDoApp.Controllers
{
    public class ToDoController : Controller
    {
        private readonly ToDoContext _context;
        private readonly ILogger<ToDoController> _logger; // Logger for error handling

        public ToDoController(ToDoContext context, ILogger<ToDoController> logger)
        {
            _context = context;
            _logger = logger; // Initialize logger
        }

        // GET: ToDo/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ToDo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ToDoApp.Models.Task task) // Fully qualified Task reference
        {
            if (ModelState.IsValid)
            {
                task.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)); // Set the UserId from claims
                _context.Tasks.Add(task);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Task created successfully."; // Set success message
                return RedirectToAction("Index"); // Redirect to the task list
            }
            return View(task); // Return the view with the task model if validation fails
        }

        // GET: ToDo/Edit/5
        public IActionResult Edit(int id)
        {
            var task = _context.Tasks.Find(id);
            if (task == null)
            {
                return NotFound(); // Return 404 if task not found
            }
            return View(task); // Return the view with the task model
        }

        // POST: ToDo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ToDoApp.Models.Task task)
        {
            if (ModelState.IsValid)
            {
                _context.Tasks.Update(task);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Task updated successfully."; // Set success message
                return RedirectToAction("Index"); // Redirect to the task list
            }
            return View(task); // Return the view with the task model if validation fails
        }

        // GET: ToDo/Delete/5
        public IActionResult Delete(int id)
        {
            var task = _context.Tasks.Find(id);
            if (task == null)
            {
                return NotFound(); // Return 404 if task not found
            }
            return View(task); // Return the view with the task model for confirmation
        }

        // POST: ToDo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _logger.LogInformation("DeleteConfirmed called for Task ID: {TaskId}", id); // Log the incoming request

            var task = _context.Tasks.Find(id);
            if (task == null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found for deletion.", id);
                return NotFound(); // Return 404 if task not found
            }

            if (task.UserId != int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))
            {
                _logger.LogWarning("User is not authorized to delete task with ID {TaskId}.", id);
                return Unauthorized(); // Prevent unauthorized deletions
            }

            try
            {
                _context.Tasks.Remove(task);
                _context.SaveChanges();
                _logger.LogInformation("Deleted task with ID: {TaskId}", id); // Log the deletion
                TempData["SuccessMessage"] = "Task deleted successfully."; // Set success message
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting task with ID {TaskId}: {Message}", id, ex.Message);
                TempData["ErrorMessage"] = "An error occurred while deleting the task."; // Set error message
            }

            return RedirectToAction("Index", "ToDo"); // Redirect to the dashboard after deletion
        }
    }
}
