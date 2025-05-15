using Microsoft.AspNetCore.Mvc;
using ToDoApp.Models; // Ensure this is the correct namespace
using ToDoApp.Data;
using System.Security.Claims; // For accessing user claims
using Microsoft.Extensions.Logging; // For logging
using System.Linq; // For LINQ operations
using Microsoft.AspNetCore.Mvc.Rendering;

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
    var statuses = _context.Statuses.ToList();
    var viewModel = new ToDoApp.Models.TaskCreateViewModel
    {
        Task = new ToDoApp.Models.Task(),
        StatusList = statuses
    };
    return View(viewModel);
}

// POST: ToDo/Create
// POST: ToDo/Create
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Create(ToDoApp.Models.TaskCreateViewModel viewModel)
{
    _logger.LogInformation("Create POST called with Task: {@Task}", viewModel.Task);

    try
    {
        // Parse and assign UserId from the logged-in user's claims
        string? userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim))
        {
            _logger.LogWarning("User ID claim is missing.");
            return Unauthorized(); // User is not authenticated
        }

        viewModel.Task.UserId = int.Parse(userIdClaim);
    }
    catch (Exception ex)
    {
        _logger.LogError("Failed to set UserId: {Message}", ex.Message);
        return Unauthorized(); // User ID claim invalid
    }

    if (ModelState.IsValid)
    {
        try
        {
            _context.Tasks.Add(viewModel.Task);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Task created successfully.";
            return RedirectToAction("Dashboard", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error saving task: {Message}", ex.Message);
            TempData["ErrorMessage"] = "An error occurred while creating the task.";
        }
    }
    else
    {
        foreach (var modelStateEntry in ModelState)
        {
            var key = modelStateEntry.Key;
            foreach (var error in modelStateEntry.Value.Errors)
            {
                _logger.LogWarning("ModelState error for {Key}: {ErrorMessage}", key, error.ErrorMessage);
            }
        }
    }

    // Repopulate status list on validation failure or exception
    viewModel.StatusList = _context.Statuses.ToList();
    return View(viewModel);
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
                return RedirectToAction("Dashboard","Home"); // Redirect to the task list
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

            return RedirectToAction("Dashboard", "Home"); // Redirect to the dashboard after deletion
        }
    }
}
