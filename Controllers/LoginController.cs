using Microsoft.AspNetCore.Mvc;

namespace PetPhotographyApp.Controllers
{
    /// <summary>
    /// Handles user login and logout functionality for the application.
    /// </summary>
    public class LoginController : Controller
    {   
        // user data for demonstration purposes
        private readonly Dictionary<string, (string Password, string Role)> _users = new()
        {
            { "admin@example.com", ("adminpass", "Admin") },
            { "user@example.com", ("userpass", "User") }
        };

        /// <summary>
        /// Displays the login form.
        /// </summary>
        /// <returns>Login view</returns>
        /// <example>GET: /Login</example>
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// Authenticates a user and starts a session if credentials are valid.
        /// </summary>
        /// <param name="email">The user's email address</param>
        /// <param name="password">The user's password</param>
        /// <returns>Redirects to Home on success, or redisplays the login form with an error</returns>
        /// <example>POST: /Login</example>
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

        /// <summary>
        /// Logs out the current user and clears session data.
        /// </summary>
        /// <returns>Redirects to Login view</returns>
        /// <example>GET: /Logout</example>
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
