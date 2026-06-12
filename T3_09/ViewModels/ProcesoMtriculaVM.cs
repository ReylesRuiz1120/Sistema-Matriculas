using System.ComponentModel.DataAnnotations;

namespace T3_09.ViewModels
{
    public class ProcesoMatriculaVM
    {
        // Datos de control (Ocultos)
        public int IdVacanteSeleccionada { get; set; }
        public string? InfoVacante { get; set; }

    
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Mínimo 2 caracteres")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "Solo letras")]
        public string? NombreCompleto { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "Deben ser 8 dígitos")]
        public string? Dni { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        [DataType(DataType.Date)]
 
        [EdadEscolar(11, 18, ErrorMessage = "La edad debe estar entre 11 y 18 años.")]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "La dirección es obligatoria")]
        public string? Direccion { get; set; }

        [Required(ErrorMessage = "El apoderado es obligatorio")]
        public string? NombreApoderado { get; set; }
    }

    //validación personalizada para rango de edades
    public class EdadEscolarAttribute : ValidationAttribute
    {
        private readonly int _min;
        private readonly int _max;
        public EdadEscolarAttribute(int min, int max) { _min = min; _max = max; }

        protected override ValidationResult IsValid(object value, ValidationContext ctx)
        {
            if (value is DateTime fecha)
            {
                var edad = DateTime.Today.Year - fecha.Year;
                if (fecha.Date > DateTime.Today.AddYears(-edad)) edad--;
                if (edad < _min || edad > _max) return new ValidationResult(ErrorMessage);
                return ValidationResult.Success;
            }
            return new ValidationResult("Fecha inválida");
        }
    }
}