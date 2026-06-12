using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using T3_09.Data;
using T3_09.Models;
using T3_09.ViewModels;

namespace T3_09.Controllers
{
    public class EstudiantesController : Controller
    {
        private readonly AppDbContext _context;

        public EstudiantesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Listar(string buscarDni)
        {
            var query = _context.Estudiantes.AsQueryable();

            if (!string.IsNullOrEmpty(buscarDni))
            {
                query = query.Where(e => e.Dni.Contains(buscarDni));
            }

            var listaEstudiantesBD = await query.ToListAsync();
            var listaParaVista = new List<EstudianteIndexVM>();

            foreach (var est in listaEstudiantesBD)
            {
                // Buscamos si el estudiante con tal dni esta matriculado
                var matricula = await _context.Matriculas
                                              .Include(m => m.Vacante)
                                              .Include(m => m.Estudiante)
                                              .Where(m => m.Estudiante.Dni == est.Dni)
                                              .OrderByDescending(m => m.FechaRegistro)
                                              .FirstOrDefaultAsync();
         
                string textoAula = "Sin Matrícula";
                if (matricula != null && matricula.Estado == "Matriculado" && matricula.Vacante != null)
                {
                    textoAula = $"{matricula.Vacante.Grado} - {matricula.Vacante.Seccion}";
                }

                listaParaVista.Add(new EstudianteIndexVM
                {
                    IdEstudiante = est.IdEstudiante,
                    NombreCompleto = est.NombreCompleto,
                    Dni = est.Dni,
                    NombreApoderado = est.NombreApoderado,
                    FechaNacimiento = est.FechaNacimiento,
                    AulaAsignada = textoAula
                });
            }

            return View(listaParaVista);
        }

        public async Task<IActionResult> Detalles(int? id)
        {
            if (id == null) return NotFound();

            var estudiante = await _context.Estudiantes.FindAsync(id);
            if (estudiante == null) return NotFound();

            var todasLasMatriculas = await _context.Matriculas
                                          .Include(m => m.Vacante)
                                          .Include(m => m.Estudiante)
                                          .Where(m => m.Estudiante.Dni == estudiante.Dni)
                                          .ToListAsync();

            var matriculaAceptada = todasLasMatriculas
                                    .FirstOrDefault(m => m.Estado != null &&
                                                         m.Estado.Equals("Matriculado", StringComparison.OrdinalIgnoreCase));

            var matriculaFinal = matriculaAceptada ?? todasLasMatriculas.OrderByDescending(m => m.FechaRegistro).FirstOrDefault();

            var modelo = new EstudianteDetalleVM
            {
                IdEstudiante = estudiante.IdEstudiante,
                NombreCompleto = estudiante.NombreCompleto,
                Dni = estudiante.Dni,
                NombreApoderado = estudiante.NombreApoderado,
                Direccion = estudiante.Direccion,
                FechaNacimiento = estudiante.FechaNacimiento,

                SituacionMatricula = (matriculaAceptada != null) ? "Matriculado" : "No Matriculado",

                GradoSeccion = (matriculaFinal != null && matriculaFinal.Vacante != null)
                               ? $"{matriculaFinal.Vacante.Grado} - {matriculaFinal.Vacante.Seccion}"
                               : "Sin asignar"
            };

            return View(modelo);
        }

        public IActionResult Crear()
        {
            ViewBag.ListaVacantes = _context.Vacantes
                .Where(v => v.CuposDisponibles > 0)
                .Select(v => new SelectListItem
                {
                    Value = v.IdVacante.ToString(),
                    Text = $"{v.Grado} - {v.Seccion} (Cupos: {v.CuposDisponibles})"
                })
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Estudiante estudiante, int? IdVacanteSeleccionada)
        {

            int anioActual = DateTime.Now.Year;
            int edad = anioActual - estudiante.FechaNacimiento.Year;
            if (edad < 3 || edad > 18)
                ModelState.AddModelError("FechaNacimiento", "Edad fuera de rango escolar.");

            if (await _context.Estudiantes.AnyAsync(e => e.Dni == estudiante.Dni))
                ModelState.AddModelError("Dni", "Este DNI ya existe.");

            if (ModelState.IsValid)
            {
                _context.Add(estudiante);
                await _context.SaveChangesAsync(); 

                if (estudiante.IdEstudiante == 0)
                {
                    
                    ModelState.AddModelError("", " La BD no generó el ID del estudiante.");
                    return View(estudiante);
                }

                _context.Entry(estudiante).State = EntityState.Detached;

                if (IdVacanteSeleccionada.HasValue && IdVacanteSeleccionada.Value > 0)
                {
                    
                    var vacante = await _context.Vacantes.FindAsync(IdVacanteSeleccionada.Value);
                    var usuarioAdmin = await _context.Usuarios.FirstOrDefaultAsync();

                    if (vacante != null && vacante.CuposDisponibles > 0)
                    {
                        var nuevaMatricula = new Matricula
                        {
                            FechaRegistro = DateTime.Now,
                            Estado = "Matriculado",
                            CodigoPago = "ADM-" + DateTime.Now.Ticks.ToString().Substring(12),
                            IdUsuario = usuarioAdmin.IdUsuario,

                            IdEstudiante = estudiante.IdEstudiante,
                            IdVacante = vacante.IdVacante,

                            // FORZAMOS NULOS EN OBJETOS (Para que EF no intente re-insertarlos)
                            Estudiante = null,
                            Vacante = null,

                            Pago = new Pago
                            {
                                Monto = 75.00,
                                FechaPago = DateTime.Now,
                                EntidadBancaria = "Efectivo / Admin",
                                CodigoPago = "ADM-" + DateTime.Now.Ticks.ToString().Substring(12)
                            }
                        };

                        _context.Matriculas.Add(nuevaMatricula);

                       
                        vacante.CuposDisponibles -= 1;
                        _context.Entry(vacante).State = EntityState.Modified;

                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(Listar));
            }

            ViewBag.ListaVacantes = _context.Vacantes
                .Where(v => v.CuposDisponibles > 0)
                .Select(v => new SelectListItem { Value = v.IdVacante.ToString(), Text = $"{v.Grado} - {v.Seccion}" })
                .ToList();

            return View(estudiante);
        }
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();
            var est = await _context.Estudiantes.FindAsync(id);
            return est == null ? NotFound() : View(est);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Estudiante estudiante)
        {
            if (id != estudiante.IdEstudiante) return NotFound();
            if (ModelState.IsValid) { _context.Update(estudiante); await _context.SaveChangesAsync(); return RedirectToAction(nameof(Listar)); }
            return View(estudiante);
        }

        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();
            var est = await _context.Estudiantes.FirstOrDefaultAsync(m => m.IdEstudiante == id);
            return est == null ? NotFound() : View(est);
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarEliminacion(int id)
        {
            var est = await _context.Estudiantes.FindAsync(id);
            if (est != null) { _context.Estudiantes.Remove(est); await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(Listar));
        }
    }
}