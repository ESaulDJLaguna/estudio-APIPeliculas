using System;
using System.ComponentModel.DataAnnotations;

namespace APIPeliculas.Models
{
#pragma warning disable CS1591 // Falta el comentario XML para el tipo o miembro visible públicamente
    public class Categoria
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
#pragma warning restore CS1591 // Falta el comentario XML para el tipo o miembro visible públicamente
}
