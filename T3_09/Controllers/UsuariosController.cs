using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T3_09.Data;
using T3_09.Models;
using T3_09.ViewModels;

namespace T3_09.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Gestionar()
        {
            var usuarioVM = new UsuarioVM
            {
                Roles = await _context.Rols.ToListAsync(),
                Usuarios = await _context.Usuarios.Include(u => u.Rol).ToListAsync() 
            };

            return View(usuarioVM);
        }

        [HttpPost]
        public async Task<IActionResult> CrearUsuario(UsuarioVM usuarioVM)
        {
           
            ModelState.Remove("Roles");
            ModelState.Remove("Usuarios");        

            // Validar el modelo
            if (!ModelState.IsValid)
            {
             

                usuarioVM.Roles = await _context.Rols.ToListAsync();
                usuarioVM.Usuarios = await _context.Usuarios.Include(u => u.Rol).ToListAsync();
                return View("Gestionar", usuarioVM);
            }
          
            bool correoExiste = await _context.Usuarios.AnyAsync(u => u.Correo == usuarioVM.Correo);
            if (correoExiste)
            {
                ModelState.AddModelError("Correo", "El correo ya está registrado en el sistema.");
                usuarioVM.Roles = await _context.Rols.ToListAsync();
                usuarioVM.Usuarios = await _context.Usuarios.Include(u => u.Rol).ToListAsync();
                return View("Gestionar", usuarioVM);
            }

            bool rolValido = await _context.Rols.AnyAsync(r => r.IdRol == usuarioVM.Id_Rol);
            if (!rolValido)
            {
                ModelState.AddModelError("Id_Rol", "El rol seleccionado no es válido.");
                usuarioVM.Roles = await _context.Rols.ToListAsync();
                usuarioVM.Usuarios = await _context.Usuarios.Include(u => u.Rol).ToListAsync();
                return View("Gestionar", usuarioVM);
            }

            try
            {
                var usuario = new Usuario
                {
                    NomUsuario = usuarioVM.Nombre,
                    ApeUsuario = usuarioVM.Apellido,
                    Correo = usuarioVM.Correo,
                    Password = usuarioVM.Contraseña,
                    idRol = usuarioVM.Id_Rol
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ocurrió un error al guardar: " + ex.Message);
                usuarioVM.Roles = await _context.Rols.ToListAsync();
                usuarioVM.Usuarios = await _context.Usuarios.Include(u => u.Rol).ToListAsync();
                return View("Gestionar", usuarioVM);
            }

            return RedirectToAction(nameof(Gestionar));
        }
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Gestionar));
        }
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            var usuarioVM = new UsuarioVM
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.NomUsuario,
                Apellido = usuario.ApeUsuario,
                Correo = usuario.Correo,
                Id_Rol = usuario.idRol,
                Roles = await _context.Rols.ToListAsync(),

            };

            return View(usuarioVM);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(int id, UsuarioVM usuarioVM)
        {
            ModelState.Remove("Roles");
            ModelState.Remove("Usuarios");

            if (string.IsNullOrEmpty(usuarioVM.Contraseña))
            {
                ModelState.Remove("Contraseña");
                ModelState.Remove("Repite_Contraseña");
            }

            if (!ModelState.IsValid)
            {            
                usuarioVM.Roles = await _context.Rols.ToListAsync();
                return View(usuarioVM);
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();


            usuario.NomUsuario = usuarioVM.Nombre;
            usuario.ApeUsuario = usuarioVM.Apellido;
            usuario.Correo = usuarioVM.Correo;
            usuario.idRol = usuarioVM.Id_Rol;

            if (!string.IsNullOrEmpty(usuarioVM.Contraseña))
            {
                usuario.Password = usuarioVM.Contraseña;
            }

            try
            {
                _context.Update(usuario);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {            
                ModelState.AddModelError("", "Error al actualizar.");
                usuarioVM.Roles = await _context.Rols.ToListAsync();
                return View(usuarioVM);
            }

            return RedirectToAction(nameof(Gestionar));
        }
    }
}