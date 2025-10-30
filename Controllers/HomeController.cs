using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AVUS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

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
        string dniSesion = HttpContext.Session.GetString("DNI");
        string nombre = null;
        string genero = null;
        if (!string.IsNullOrEmpty(dniSesion))
        {
            (nombre, genero) = BD.ObtenerNombreYGeneroPorDNI(dniSesion);
        }
        ViewBag.NombreUsuario = nombre;
        ViewBag.GeneroUsuario = genero;
        return View();
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public IActionResult CompletarLogin(string DNI, string password)
    {
        // Notar que aquí el parámetro debe coincidir con el name del input en el formulario: "password"
        if (BD.VerificarIniciarSesion(DNI, password))
        {
            HttpContext.Session.SetString("DNI", DNI.Trim().ToLowerInvariant());
            return RedirectToAction("Index");
        }
        TempData["LoginError"] = "DNI o contraseña incorrectos.";
        return RedirectToAction("Login");
    }
    
    public IActionResult CerrarSesion()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
    
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var path = context.HttpContext.Request.Path.Value.ToLower();
        bool requiereLogin = !(path.Contains("login") || path.Contains("register") || path.Contains("tutorial"));
        if (requiereLogin && string.IsNullOrEmpty(HttpContext.Session.GetString("DNI")))
        {
            context.Result = RedirectToAction("Login");
        }
        base.OnActionExecuting(context);
    }
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult CompletarRegistro(string nombre, string apellido, string contraseña, string repetirContraseña, string DNI, string genero, DateTime nacimiento, string nacionalidad)
    {
        try
        {
            if (contraseña != repetirContraseña)
            {
                TempData["RegisterError"] = "Las contraseñas no coinciden.";
                return RedirectToAction("Register");
            }
            BD.AgregarAvu(nombre, apellido, contraseña, DNI, genero, nacimiento, nacionalidad);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["RegisterError"] = ex.Message;
            return RedirectToAction("Register");
        }
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
