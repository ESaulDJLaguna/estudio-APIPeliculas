using System.ComponentModel.DataAnnotations;

namespace APIPeliculas.Models.Dtos
{
    public class UsuarioAuthDto
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "El usuario es obligatorio")]
        public string UsuarioA { get; set; }
        [Required(ErrorMessage = "El pasword es obligatorio")]
        [StringLength(10, MinimumLength =4, ErrorMessage = "La contraseña debe estar entre 4 y 10 caracteres")]
        public string Password { get; set; }
    }
}
