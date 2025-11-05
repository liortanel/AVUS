using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
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
        HttpContext.Session.SetString("UserDNI", DNI);
        return RedirectToAction("Index");
    }
    
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public IActionResult CompletarRegistro(string nombre, string apellido, string contraseña, string repetirContraseña, string DNI, string genero, DateTime nacimiento, string nacionalidad)
    {
        if (string.IsNullOrWhiteSpace(DNI) || string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido) || string.IsNullOrWhiteSpace(contraseña) || string.IsNullOrWhiteSpace(repetirContraseña) || string.IsNullOrWhiteSpace(genero) || string.IsNullOrWhiteSpace(nacionalidad))
        {
            ViewBag.Error = "Completa todos los campos obligatorios.";
            return View("Register");
        }
        if (contraseña != repetirContraseña)
        {
            ViewBag.Error = "Las contraseñas no coinciden.";
            return View("Register");
        }
        // Validación simple de DNI numérico
        if (!System.Text.RegularExpressions.Regex.IsMatch(DNI, @"^\d{7,10}$"))
        {
            ViewBag.Error = "El DNI debe tener 7 a 10 dígitos numéricos.";
            return View("Register");
        }

        BD.AgregarAvu(nombre, apellido, contraseña, DNI, genero, nacimiento, nacionalidad);
        // Opcional: iniciar sesión automáticamente luego de registrarse
        HttpContext.Session.SetString("UserDNI", DNI);
        return RedirectToAction("Index");
    }

    public IActionResult Tutoriales()
    {
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
        return View();
    }

    public IActionResult Tutorial(string app)
    {
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
        ViewData["App"] = app;
        return View();
    }

    public IActionResult CerrarSesion()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    public IActionResult Calendario(int? year, int? month)
    {
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
        var dni = HttpContext.Session.GetString("UserDNI");
        var avuId = BD.ObtenerAvuIdPorDni(dni!);
        if (avuId == null)
        {
            return RedirectToAction("Login");
        }
        var now = DateTime.Now;
        int y = year ?? now.Year;
        int m = month ?? now.Month;
        if (m < 1) { m = 12; y--; }
        if (m > 12) { m = 1; y++; }
        var eventos = BD.ObtenerEventosDelMes(avuId.Value, y, m);
        ViewBag.Year = y;
        ViewBag.Month = m;
        ViewBag.Eventos = eventos;
        return View("Calendario");
    }

    public IActionResult EditarCalendario(DateTime fecha)
    {
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
        var dni = HttpContext.Session.GetString("UserDNI");
        var avuId = BD.ObtenerAvuIdPorDni(dni!);
        if (avuId == null)
        {
            return RedirectToAction("Login");
        }
        var eventos = BD.ObtenerEventosDelDia(avuId.Value, fecha.Date);
        ViewBag.Fecha = fecha.Date;
        ViewBag.Eventos = eventos;
        return View("EditarCalendario");
    }

    [HttpPost]
    public IActionResult CrearEvento(string nombre, DateTime fecha, string? hora)
    {
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
        var dni = HttpContext.Session.GetString("UserDNI");
        var avuId = BD.ObtenerAvuIdPorDni(dni!);
        if (avuId == null)
        {
            return RedirectToAction("Login");
        }
        TimeSpan? horaTs = null;
        if (!string.IsNullOrWhiteSpace(hora) && TimeSpan.TryParse(hora, out var parsed))
        {
            horaTs = parsed;
        }
        BD.CrearEvento(avuId.Value, nombre?.Trim() ?? "Evento", fecha.Date, horaTs);
        return RedirectToAction("EditarCalendario", new { fecha = fecha.ToString("yyyy-MM-dd") });
    }

    [HttpPost]
    public IActionResult ActualizarEvento(int evento_id, string nombre, string? hora, DateTime fecha)
    {
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
        TimeSpan? horaTs = null;
        if (!string.IsNullOrWhiteSpace(hora) && TimeSpan.TryParse(hora, out var parsed))
        {
            horaTs = parsed;
        }
        BD.ActualizarEvento(evento_id, nombre?.Trim() ?? "Evento", horaTs);
        return RedirectToAction("EditarCalendario", new { fecha = fecha.ToString("yyyy-MM-dd") });
    }

    [HttpPost]
    public IActionResult EliminarEvento(int evento_id, DateTime fecha)
    {
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
        BD.EliminarEvento(evento_id);
        return RedirectToAction("EditarCalendario", new { fecha = fecha.ToString("yyyy-MM-dd") });
    }
    private bool EstaLogueado()
    {
        string? dni = HttpContext.Session.GetString("UserDNI");
        return !string.IsNullOrEmpty(dni);
    }
    public IActionResult Perfil()
    {
        var dni = HttpContext.Session.GetString("UserDNI");
        var perfil = BD.DatosPerfil(dni);
        return View("Perfil", perfil);
    }

    public IActionResult Salud()
    {
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
        return View("Salud");
    }

    public IActionResult Pastillero()
    {
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
        var dni = HttpContext.Session.GetString("UserDNI");
        var avuId = BD.ObtenerAvuIdPorDni(dni!);
        if (avuId == null)
        {
            return RedirectToAction("Login");
        }
        var pastillas = BD.ObtenerPastillas(avuId.Value);
        var ahora = DateTime.Now.TimeOfDay;
        var ordenadas = pastillas
            .OrderBy(p => p.hora.HasValue ? ((p.hora.Value >= ahora) ? 0 : 1) : 2)
            .ThenBy(p => p.hora ?? TimeSpan.MaxValue)
            .ToList();
        ViewBag.Pastillas = ordenadas;
        ViewBag.HoraActual = ahora;
        return View("Pastillero");
    }

    public IActionResult TurnosMedicos()
    {
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
        var dni = HttpContext.Session.GetString("UserDNI");
        var avuId = BD.ObtenerAvuIdPorDni(dni!);
        if (avuId == null)
        {
            return RedirectToAction("Login");
        }
        var turnos = BD.ObtenerTurnosProximos(avuId.Value);
        var ahora = DateTime.Now;
        // Construir DateTime completo para ordenar
        var ordenados = turnos
            .Select(t => new {
                Turno = t,
                FechaHora = (t.dia.HasValue ? t.dia.Value.Date : DateTime.MaxValue.Date)
                            + (t.hora ?? TimeSpan.Zero)
            })
            .OrderBy(x => x.FechaHora < ahora ? 1 : 0)
            .ThenBy(x => x.FechaHora)
            .Select(x => x.Turno)
            .ToList();
        ViewBag.Turnos = ordenados;
        ViewBag.Ahora = ahora;
        return View("TurnosMedicos");
    }

    [HttpPost]
    public IActionResult CrearPastilla(string nombre, string? dosis, string? hora)
    {
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
        var dni = HttpContext.Session.GetString("UserDNI");
        var avuId = BD.ObtenerAvuIdPorDni(dni!);
        if (avuId == null)
        {
            return RedirectToAction("Login");
        }
        if (string.IsNullOrWhiteSpace(nombre))
        {
            TempData["PastilleroError"] = "El nombre es obligatorio.";
            return RedirectToAction("Pastillero");
        }
        TimeSpan? horaTs = null;
        if (!string.IsNullOrWhiteSpace(hora) && TimeSpan.TryParse(hora, out var parsed))
        {
            horaTs = parsed;
        }
        BD.CrearPastilla(avuId.Value, nombre.Trim(), string.IsNullOrWhiteSpace(dosis) ? null : dosis.Trim(), horaTs);
        TempData["PastilleroOk"] = "Medicamento agregado.";
        return RedirectToAction("Pastillero");
    }

    [HttpPost]
    public IActionResult CrearTurno(string? especialidad, string? medico, string? dia, string? hora)
    {
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
        var dni = HttpContext.Session.GetString("UserDNI");
        var avuId = BD.ObtenerAvuIdPorDni(dni!);
        if (avuId == null)
        {
            return RedirectToAction("Login");
        }
        DateTime? diaDt = null;
        if (!string.IsNullOrWhiteSpace(dia) && DateTime.TryParse(dia, out var parsedDia))
        {
            diaDt = parsedDia.Date;
        }
        TimeSpan? horaTs = null;
        if (!string.IsNullOrWhiteSpace(hora) && TimeSpan.TryParse(hora, out var parsedHora))
        {
            horaTs = parsedHora;
        }
        BD.CrearTurno(avuId.Value, string.IsNullOrWhiteSpace(especialidad) ? null : especialidad.Trim(), string.IsNullOrWhiteSpace(medico) ? null : medico.Trim(), diaDt, horaTs);
        TempData["TurnoOk"] = "Turno agregado.";
        return RedirectToAction("TurnosMedicos");
    }

    [HttpPost]
    public IActionResult ActualizarEmail(string email)
    {
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
        var dni = HttpContext.Session.GetString("UserDNI");
        if (string.IsNullOrWhiteSpace(email) || !System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            TempData["PerfilError"] = "Email inválido.";
            return RedirectToAction("Perfil");
        }
        try
        {
            BD.ActualizarEmail(dni!, email.Trim());
            TempData["PerfilOk"] = "Email actualizado.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando email");
            TempData["PerfilError"] = "No se pudo actualizar el email.";
        }
        return RedirectToAction("Perfil");
    }

    [HttpPost]
    public IActionResult ActualizarTelefono(string telefono)
    {
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
        var dni = HttpContext.Session.GetString("UserDNI");
        if (string.IsNullOrWhiteSpace(telefono) || !System.Text.RegularExpressions.Regex.IsMatch(telefono, @"^[+]?\d[\d\s-]{6,}$"))
        {
            TempData["PerfilError"] = "Teléfono inválido.";
            return RedirectToAction("Perfil");
        }
        try
        {
            BD.ActualizarTelefono(dni!, telefono.Trim());
            TempData["PerfilOk"] = "Teléfono actualizado.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando teléfono");
            TempData["PerfilError"] = "No se pudo actualizar el teléfono.";
        }
        return RedirectToAction("Perfil");
    }

    [HttpPost]
    public IActionResult EliminarEmail()
    {
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
        var dni = HttpContext.Session.GetString("UserDNI");
        try
        {
            BD.EliminarEmail(dni!);
            TempData["PerfilOk"] = "Email eliminado.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando email");
            TempData["PerfilError"] = "No se pudo eliminar el email.";
        }
        return RedirectToAction("Perfil");
    }

    [HttpPost]
    public IActionResult EliminarTelefono()
    {
        if (!EstaLogueado())
        {
            return RedirectToAction("Login");
        }
        var dni = HttpContext.Session.GetString("UserDNI");
        try
        {
            BD.EliminarTelefono(dni!);
            TempData["PerfilOk"] = "Teléfono eliminado.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando teléfono");
            TempData["PerfilError"] = "No se pudo eliminar el teléfono.";
        }
        return RedirectToAction("Perfil");
    }
}
