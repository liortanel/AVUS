using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AVUS.Models;

namespace AVUS.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Register(string dni, string nombre, string apellido, string genero, string nacionalidad, string password, string repetirPassword)
    {
        // Aquí agregarás la lógica de registro
        return RedirectToAction("Index");
    }

    public IActionResult Tutoriales()
    {
        return View();
    }

    public IActionResult Tutorial(string app)
    {
        ViewData["App"] = app;
        return View();
    }
}
