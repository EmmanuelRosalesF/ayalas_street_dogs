using ayalas_street_dogs.Models;
using ayalas_street_dogs.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ayalas_street_dogs.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("admin")) { return RedirectToAction("Index", "Home"); }
                else if (User.IsInRole("empleado")) { return RedirectToAction("Index", "POS"); }
            }

            return View();
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u =>
                        u.Usuario1 == model.UsuarioOEmail ||
                        u.EmailUsuario == model.UsuarioOEmail);

                if (usuario == null)
                {
                    ModelState.AddModelError("", "Usuario o correo no encontrado");
                    return View(model);
                }

                if (usuario.Contrasena != model.Contrasena)
                {
                    ModelState.AddModelError("", "Contraseña incorrecta");
                    return View(model);
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Usuario1),
                    new Claim(ClaimTypes.Email, usuario.EmailUsuario ?? ""),
                    new Claim(ClaimTypes.Role, usuario.Rol),
                    new Claim("NombreCompleto", $"{usuario.Nombres} {usuario.Apellidos}"),
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RecordarMe,
                    ExpiresUtc = model.RecordarMe
                        ? DateTimeOffset.UtcNow.AddDays(30)
                        : DateTimeOffset.UtcNow.AddHours(2)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                TempData["Success"] = $"¡Bienvenido {usuario.Nombres}!";

                if (usuario.Rol == "admin")
                {
                    return RedirectToAction("Index", "Home");
                }
                else if (usuario.Rol == "empleado")
                {
                    return RedirectToAction("Index", "POS");
                }
                else
                {
                    return RedirectToAction("Index", "POS");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al iniciar sesión: {ex.Message}");
                return View(model);
            }
        }

        // GET: Auth/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "Sesión cerrada correctamente";
            return RedirectToAction("Login");
        }
    }
}
