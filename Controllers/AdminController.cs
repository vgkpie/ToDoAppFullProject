using Microsoft.AspNetCore.Mvc;
using ToDoApp.Data;
using ToDoApp.Models;
using System.Linq;
using System.Security.Claims;

namespace ToDoApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly ToDoContext _context;

        public AdminController(ToDoContext context)
        {
            _context = context;
        }

        // Helper method to check if current user is admin
        private bool IsAdmin()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = _context.Users.Find(userId);
            return user != null && user.IsAdmin;
        }

        // GET: Admin/Users
        public IActionResult Users()
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }
            var users = _context.Users.ToList();
            return View(users);
        }

        // POST: Admin/DeleteUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int id)
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            _context.Users.Remove(user);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "User deleted successfully.";
            return RedirectToAction("Users");
        }

        // GET: Admin/AddStatus
        public IActionResult AddStatus()
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }
            return RedirectToAction("StatusList");
        }

        // POST: Admin/AddStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddStatus(string statusName)
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }
            if (string.IsNullOrWhiteSpace(statusName))
            {
                ModelState.AddModelError("", "Status name cannot be empty.");
                return View();
            }
            // Assuming there is a Status entity and DbSet<Status> in context
            var status = new Status { Name = statusName };
            _context.Statuses.Add(status);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Status added successfully.";
            return RedirectToAction("AddStatus");
        }

        // GET: Admin/StatusList
        public IActionResult StatusList()
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }
            var statuses = _context.Statuses.ToList();
            return View(statuses);
        }

        // POST: Admin/DeleteStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteStatus(int id)
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }
            var status = _context.Statuses.Find(id);
            if (status == null)
            {
                return NotFound();
            }
            _context.Statuses.Remove(status);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Status deleted successfully.";
            return RedirectToAction("StatusList");
        }
        // GET: Admin/AllTasks
        public IActionResult AllTasks()
        {
            if (!IsAdmin())
            {
                return Unauthorized();
            }

            var usersWithTasks = _context.Users
                .Select(u => new AdminUserTasksViewModel
                {
                    User = u,
                    Tasks = _context.Tasks.Where(t => t.UserId == u.Id).ToList()
                })
                .ToList();

            return View(usersWithTasks);
        }
    }
}
