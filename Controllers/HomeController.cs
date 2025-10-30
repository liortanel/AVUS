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

    [HttpPost]
    public IActionResult CompletarLogin(string DNI, string contraseña)
    {
        bool loginCorrecto = BD.VerificarIniciarSesion(DNI, contraseña);
        if (!loginCorrecto)
        {
            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View("Login");
        }
        return RedirectToAction("Index");
    }
    
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult CompletarRegistro(string nombre, string apellido, string contraseña, string repetirContraseña, string DNI, string genero, DateTime nacimiento, string nacionalidad)
    {
        BD.AgregarAvu(nombre, apellido, contraseña, DNI, genero, nacimiento, nacionalidad);
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
