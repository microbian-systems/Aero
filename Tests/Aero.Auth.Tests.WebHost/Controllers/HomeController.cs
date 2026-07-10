using System.Diagnostics;
using Aero.Auth.Tests.WebHost.Models;
using Microsoft.AspNetCore.Mvc;

namespace Aero.Auth.Tests.WebHost.Controllers;

/// <summary>
/// Represents a class for HomeController.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

        /// <summary>
    /// Initializes a new instance of the <see cref="HomeController"/> class.
    /// </summary>
public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

        /// <summary>
    /// Index method.
    /// </summary>
public IActionResult Index()
    {
        return View();
    }

        /// <summary>
    /// Privacy method.
    /// </summary>
public IActionResult Privacy()
    {
        return View();
    }

        /// <summary>
    /// Error method.
    /// </summary>
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}