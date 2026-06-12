using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T3_09.Data;

namespace T3_09.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalEstudiantes = await _context.Matriculas
                .Where(m => m.Estado != "Pendiente")
                .Select(m => m.IdEstudiante)
                .Distinct()
                .CountAsync();

            var totalPorGrado = await _context.Vacantes
                .Select(v => new
                {
                    Grado = v.Grado,
                    Total = _context.Matriculas
                        .Where(m => m.IdVacante == v.IdVacante && m.Estado != "Pendiente")
                        .Count()
                })
                .ToListAsync();

            ViewBag.TotalEstudiantes = totalEstudiantes;
            ViewBag.TotalPorGrado = totalPorGrado;

            return View();
        }
    }
}