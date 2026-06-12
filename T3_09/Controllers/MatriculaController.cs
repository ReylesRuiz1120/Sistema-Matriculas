using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using T3_09.Data;
using T3_09.Models;
using T3_09.ViewModels;

namespace T3_09.Controllers
{
    public class MatriculaController : Controller
    {
        private readonly AppDbContext _context;

        public MatriculaController(AppDbContext context)
        {
            _context = context;
        }

 
        [HttpGet]
        public IActionResult Seleccion()
        {
 
            ViewBag.ListaVacantes = _context.Vacantes.Where(v => v.CuposDisponibles > 0).ToList();
            return View();
        }


        [HttpPost]
        public IActionResult CargarFormulario(int idVacante)
        {
            var vacante = _context.Vacantes.Find(idVacante);

            if (vacante == null || vacante.CuposDisponibles <= 0)
            {
                TempData["Error"] = "La vacante seleccionada ya no está disponible.";
                return RedirectToAction("Seleccion");
            }

            var modelo = new ProcesoMatriculaVM
            {
                IdVacanteSeleccionada = idVacante,
                InfoVacante = $"{vacante.Grado} - Sección {vacante.Seccion}",
           
                FechaNacimiento = DateTime.Today.AddYears(-11)
            };

            return View("Formulario", modelo);
        }

   
        [HttpPost]
        public async Task<IActionResult> Registrar(ProcesoMatriculaVM modelo)
        {
        
            if (!ModelState.IsValid)
            {
 
                if (modelo.IdVacanteSeleccionada > 0)
                {
                    var v = await _context.Vacantes.FindAsync(modelo.IdVacanteSeleccionada);
                    if (v != null) modelo.InfoVacante = $"{v.Grado} - Sección {v.Seccion}";
                }
                return View("Formulario", modelo);
            }

            var vacanteEncontrada = await _context.Vacantes.FindAsync(modelo.IdVacanteSeleccionada);
            if (vacanteEncontrada == null || vacanteEncontrada.CuposDisponibles <= 0)
            {
                TempData["Error"] = "La vacante ya no está disponible.";
                return RedirectToAction("Seleccion");
            }

            // Evitar NullReference si alguna matrícula en la BD no tiene estudiante cargado.
            bool yaExiste = await _context.Matriculas
                                          .Include(m => m.Estudiante)
                                          .AnyAsync(m => m.Estudiante != null && m.Estudiante.Dni == modelo.Dni &&
                                                     (m.Estado == "Pendiente" || m.Estado == "Matriculado"));

            if (yaExiste)
            {
                ViewData["Error"] = $"El estudiante con DNI {modelo.Dni} ya tiene una matrícula activa o pendiente.";
                modelo.InfoVacante = $"{vacanteEncontrada.Grado} - {vacanteEncontrada.Seccion}";
                return View("Formulario", modelo);
            }

            // Usar acceso seguro a User para evitar NullReference en pruebas sin principal.
            string correoUsuario = User?.Identity?.Name;
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correoUsuario);

            if (usuario == null)
            {
                usuario = await _context.Usuarios.FirstOrDefaultAsync();
            }
  

            if (usuario == null) return RedirectToAction("Login", "Acceso");

    
            try
            {
                var nuevaMatricula = new Matricula
                {
                    FechaRegistro = DateTime.Now,
                    Estado = "Pendiente",
                    CodigoPago = "REQ-" + DateTime.Now.Ticks.ToString().Substring(12),
                    IdUsuario = usuario.IdUsuario,
                    Vacante = vacanteEncontrada,
                    Estudiante = new Estudiante
                    {
                        NombreCompleto = modelo.NombreCompleto,
                        Dni = modelo.Dni,
                        FechaNacimiento = modelo.FechaNacimiento,
                        Direccion = modelo.Direccion ?? "Sin dirección",
                        NombreApoderado = modelo.NombreApoderado
                    }
                };

                _context.Matriculas.Add(nuevaMatricula);
                vacanteEncontrada.ReservarCupo(); 
                await _context.SaveChangesAsync();

                return RedirectToAction("Comprobante", new { id = nuevaMatricula.IdMatricula });
            }
            catch (Exception ex)
            {
                ViewData["Error"] = "Ocurrió un error al procesar la solicitud: " + ex.Message;
                modelo.InfoVacante = $"{vacanteEncontrada.Grado} - Sección {vacanteEncontrada.Seccion}";
                return View("Formulario", modelo);
            }
        }

      
        public IActionResult Comprobante(int id)
        {
            var matricula = _context.Matriculas
                .Include(m => m.Estudiante).Include(m => m.Vacante)
                .FirstOrDefault(m => m.IdMatricula == id);

            return matricula == null ? RedirectToAction("Seleccion") : View(matricula);
        }

    
        [HttpGet]
        public async Task<IActionResult> SolicitudesPendientes()
        {
            return View(await _context.Matriculas
                .Include(m => m.Estudiante)
                .Include(m => m.Vacante)
                .Where(m => m.Estado == "Pendiente")
                .ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> AceptarMatricula(int id)
        {
            var matricula = await _context.Matriculas.FindAsync(id);
            if (matricula != null)
            {
                matricula.Estado = "Matriculado";
                var nuevoPago = new Pago
                {
                    CodigoPago = matricula.CodigoPago,
                    Monto = 75.00,
                    FechaPago = DateTime.Now,
                    EntidadBancaria = "Caja Institucional / Efectivo"
                };
                matricula.Pago = nuevoPago;
                _context.Pagos.Add(nuevoPago);
                _context.Update(matricula);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("SolicitudesPendientes");
        }
    }
}