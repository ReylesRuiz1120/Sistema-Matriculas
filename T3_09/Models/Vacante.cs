using System.ComponentModel.DataAnnotations;

namespace T3_09.Models
{
    public class Vacante
    {
        [Key]
        public int IdVacante { get; set; }
        [Required, StringLength(50)]
        public string Grado { get; set; }
        [Required, StringLength(10)]
        public string Seccion { get; set; }
        public int CuposDisponibles { get; set; }

        public void ReservarCupo() => CuposDisponibles--;
        public void LiberarCupo() => CuposDisponibles++;
    }

}
