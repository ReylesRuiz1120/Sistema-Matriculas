using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace T3_09.Models
{
    public class Usuario

    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int IdUsuario { get; set; }

        [Required, StringLength(50)]

        public string NomUsuario { get; set; }

        [Required, StringLength(50)]

        public string ApeUsuario { get; set; }

        [Required, StringLength(50)]

        public string Correo { get; set; }

        [Required, StringLength(20)]

        public string Password { get; set; }

        public int idRol { get; set; }

        [ForeignKey("idRol")]
        public Rol Rol { get; set; }

    }
}
