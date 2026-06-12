using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace T3_09.Models
{
    public class Matricula
    {
        [Key]
        public int IdMatricula { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Estado { get; set; }
        public string CodigoPago { get; set; }

        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; }

        
        public int IdEstudiante { get; set; }
        [ForeignKey("IdEstudiante")]
        public Estudiante Estudiante { get; set; }

        
        public int IdVacante { get; set; }
        [ForeignKey("IdVacante")]
        public Vacante Vacante { get; set; }

        public int? IdPago { get; set; }
        [ForeignKey("IdPago")]
        public Pago? Pago { get; set; }
    }
}
