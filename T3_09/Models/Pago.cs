using System.ComponentModel.DataAnnotations;

namespace T3_09.Models
{
    public class Pago
    {
        [Key]
        public int IdPago { get; set; }
        public string CodigoPago { get; set; }
        public DateTime FechaPago { get; set; }
        public string EntidadBancaria { get; set; }
        public double Monto { get; set; }
    }
}
