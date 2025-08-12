using Microsoft.AspNetCore.Mvc;

namespace PetPhotographyApp.Controllers
{
    public class LoginController : Controller
    {
        // user data for demonstration purposes
        private readonly Dictionary<string, (string Password, string Role)> _users = new()
        {
            { "admin@example.com", ("adminpass", "Admin") },
            { "user@example.com", ("userpass", "User") }
        };

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (_users.TryGetValue(email, out var userInfo) && userInfo.Password == password)
            {
                HttpContext.Session.SetString("UserRole", userInfo.Role);
                HttpContext.Session.SetString("UserEmail", email);
                return RedirectToAction("Index", "Home"); // transfer to dashboard or home page
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
