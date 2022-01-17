using System.ComponentModel.DataAnnotations;

namespace APIPeliculas.Models.Dtos
{
    public class UsuarioAuthLoginDto
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string UsuarioA { get; set; }
        [Required(ErrorMessage = "El pasword es obligatorio")]
        public string Password { get; set; }
    }
}
