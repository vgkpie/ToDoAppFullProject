using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ToDoApp.Models;
using ToDoApp.Data;
using Microsoft.AspNetCore.Identity;
using BCrypt.Net;
using System.Linq; // Ensure this is in...
using System.Security.Claims; // For accessing user claims
using Microsoft.AspNetCore.Authentication; // For CookieAuthenticationDefaults
using Microsoft.AspNetCore.Authentication.Cookies; // For SignInAsync

namespace ToDoApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ToDoContext _context;

        public HomeController(ILogger<HomeController> logger, ToDoContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard"); // Redirect to a dashboard or home page for authenticated users
            }
            return View(); // Show the index view for unauthenticated users
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Action for user registration
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (ModelState.IsValid)
            {
                // Hash the password and save the user
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                _context.Users.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Login");
            }
            return View(user);
        }

        // Action for user login
        [HttpGet]
        public IActionResult Login()
        {
            return View(); // Return the login view
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            _logger.LogInformation("Attempting to log in user: {Username}", username); // Log the username

            var user = _context.Users.FirstOrDefault(u => u.Username == username); // Use FirstOrDefault
            if (user != null)
            {
                _logger.LogInformation("User found: {Username}", username); // Log if user is found
                if (BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    // Set authentication cookie or session
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.Username)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, "login");
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                    _logger.LogInformation("User {Username} logged in successfully.", username); // Log successful login
                    return RedirectToAction("Dashboard"); // Redirect to a dashboard or home page after login
                }
                else
                {
                    _logger.LogWarning("Password verification failed for user: {Username}", username); // Log password failure
                }
            }
            else
            {
                _logger.LogWarning("User not found: {Username}", username); // Log if user is not found
            }

            ModelState.AddModelError("", "Invalid login attempt.");
            return View();
        }

        // Action for user logout
        public IActionResult Logout()
        {
            // Clear authentication cookie or session
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Home"); // Redirect to the login page after logout
        }

        // Action for dashboard
        public IActionResult Dashboard()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user's ID as a string
            _logger.LogInformation("Logged in User ID: {UserId}", userIdString); // Log the user ID

            if (int.TryParse(userIdString, out int userId)) // Try to parse the string to an integer
            {
                var tasks = _context.Tasks.Where(t => t.UserId == userId).ToList(); // Fetch tasks for the user
                _logger.LogInformation("Tasks retrieved for User ID: {UserId}, Count: {TaskCount}", userId, tasks.Count); // Log task count
                return View(tasks); // Pass the tasks to the view
            }
            return RedirectToAction("Index"); // Redirect if userId is not valid
        }

        // GET: Home/Create
        public IActionResult Create()
        {
            return View(); // Return the view for creating a new task
        }

        // POST: Home/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ToDoApp.Models.Task task)
        {
            if (ModelState.IsValid)
            {
                task.UserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)); // Set the UserId from claims
                _context.Tasks.Add(task);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Task created successfully."; // Set success message
                return RedirectToAction("Dashboard"); // Redirect to the dashboard
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
    