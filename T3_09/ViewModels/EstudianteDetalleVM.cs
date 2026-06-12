using System;

namespace T3_09.ViewModels
{
    public class EstudianteDetalleVM
    {
        public int IdEstudiante { get; set; }
        public string? NombreCompleto { get; set; }
        public string? Dni { get; set; }
        public string? NombreApoderado { get; set; }
        public string? Direccion { get; set; }
        public DateTime FechaNacimiento { get; set; }

        public string? SituacionMatricula { get; set; }    

        public string? GradoSeccion { get; set; } 
        // Ej: "5to - A"
    }
}
