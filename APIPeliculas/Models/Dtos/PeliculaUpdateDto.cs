using System.ComponentModel.DataAnnotations;
using static APIPeliculas.Models.Pelicula;

namespace APIPeliculas.Models.Dtos
{
	public class PeliculaUpdateDto
    {
		public int Id { get; set; }
		[Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La duración es obligatoria")]
        public string Duracion { get; set; }

        [Required(ErrorMessage = "La clasificación es obligatoria")]
        public TipoClasificacion Clasificacion { get; set; }

        public int categoriaId { get; set; }
    }
}
