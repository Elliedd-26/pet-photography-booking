using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.Models;

namespace PetPhotographyApp.Controllers;

public class HomeController : Controller
{
    /// <summary>
    /// Displays the application's landing (home) page.
    /// </summary>
    /// <returns>The home view.</returns>
    /// <example>
    /// GET: /
    /// </example>
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Displays an error page when an unhandled exception occurs.
    /// Response is not cached.
    /// </summary>
    /// <returns>The error view.</returns>
    /// <example>
    /// GET: /Home/Error
    /// </example>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}