using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace T3_09.Models
{
    public class Rol

    {

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int IdRol { get; set; }

        [Required, StringLength(20)]

        public string NomRol { get; set; }

        [Required, StringLength(200)]

        public string DesRol { get; set; }

        //Relacion de 1 - M (Rol - Usuarios)

        public ICollection<Usuario> Usuarios { get; set; }

    }
}
