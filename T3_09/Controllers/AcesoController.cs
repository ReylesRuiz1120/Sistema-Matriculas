using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using T3_09.Data;
using T3_09.Models;
using T3_09.ViewModels;

namespace T3_09.Controllers
{
    public class AccesoController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public AccesoController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public IActionResult Registro()
        {
            ViewBag.Roles = _appDbContext.Rols.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registro(UsuarioVM usuarioVM)
        {
            ViewBag.Roles = _appDbContext.Rols.ToList();

            if (!ModelState.IsValid) return View(usuarioVM);

            bool correoExiste = await _appDbContext.Usuarios.AnyAsync(u => u.Correo == usuarioVM.Correo);
            if (correoExiste)
            {
                ViewData["Mensaje"] = "Ese correo ya está registrado en el sistema";
                return View(usuarioVM);
            }

            if (usuarioVM.Contrasena != usuarioVM.RepiteContrasena)
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden";
                return View(usuarioVM);
            }

            Usuario usuario = new Usuario()
            {
                NomUsuario = usuarioVM.Nombre,
                ApeUsuario = usuarioVM.Apellido,
                Correo = usuarioVM.Correo,
                Password = usuarioVM.Contrasena,
                idRol = usuarioVM.Id_Rol
            };

            await _appDbContext.Usuarios.AddAsync(usuario);
            await _appDbContext.SaveChangesAsync();

            if (usuario.IdUsuario != 0)
            {
                return RedirectToAction("Login", "Acceso");
            }

            ViewData["Mensaje"] = "El usuario no se pudo crear por un error interno";
            return View(usuarioVM);
        }

        [HttpGet]
        public IActionResult Login()
        {
          
            if (TempData["MensajeExito"] != null)
            {
                ViewData["MensajeExito"] = TempData["MensajeExito"];
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid) return View(loginVM);

            Usuario? usuario_encontrado = await _appDbContext.Usuarios
                                            .Include(u => u.Rol)
                                            .FirstOrDefaultAsync(u => u.Correo == loginVM.Correo &&
                                                                    u.Password == loginVM.Contrasena);

            if (usuario_encontrado == null)
            {
                ViewData["Mensaje"] = "No se encontraron usuarios con esas credenciales";
                return View(loginVM);
            }

            switch (usuario_encontrado.Rol.NomRol)
            {
              
                case "Administrador":
                    return RedirectToAction("Listar", "Estudiantes");

                case "Usuario":
                    return RedirectToAction("Seleccion", "Matricula");

                case "Visitante":
                    return RedirectToAction("Index", "Visit");

                default:
                    return RedirectToAction("Index", "Home");
            }
        }
    }
}