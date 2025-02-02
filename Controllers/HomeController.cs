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
        
    }
}
