using System;

namespace T3_09.ViewModels
{
    public class EstudianteIndexVM
    {
        public int IdEstudiante { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Dni { get; set; }
        public string? NombreApoderado { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string? AulaAsignada { get; set; }
    }
}
