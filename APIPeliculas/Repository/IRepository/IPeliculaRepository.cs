using APIPeliculas.Models;
using System.Collections.Generic;

namespace APIPeliculas.Repository.IRepository
{
    public interface IPeliculaRepository
    {
        ICollection<Pelicula> GetPeliculas();
        // Método para obtener todas las películas de alguna categoría especificada
        ICollection<Pelicula> GetPeliculasEnCategoria(int CatId);
        Pelicula GetPelicula(int PeliculaId);
        bool ExistePelicula(string nombre);
        // Método que nos permite buscar películas
        IEnumerable<Pelicula> BuscarPelicula(string nombre);
        bool ExistePelicula(int id);
        bool CrearPelicula(Pelicula pelicula);
        bool ActualizarPelicula(Pelicula pelicula);
        bool BorrarPelicula(Pelicula pelicula);
        bool Guardar();
    }
}
