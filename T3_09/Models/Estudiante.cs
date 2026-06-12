using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace T3_09.Models
{
        public class Estudiante
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
            public int IdEstudiante { get; set; }

            [Required(ErrorMessage = "El nombre es obligatorio")]
            [StringLength(100)]
            [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras")]
            public string NombreCompleto { get; set; }

            [Required]
            [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe tener 8 dígitos numéricos")]
            public string Dni { get; set; }

            [Required]
            public DateTime FechaNacimiento { get; set; }

            [StringLength(200, ErrorMessage = "La dirección es demasiado larga")]
            public string Direccion { get; set; }

            [Required]
            [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras")]
            public string NombreApoderado { get; set; }
        }
}