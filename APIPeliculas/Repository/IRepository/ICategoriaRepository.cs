using APIPeliculas.Models;
using System.Collections.Generic;

namespace APIPeliculas.Repository.IRepository
{
    public interface ICategoriaRepository
    {
        // Traerá todas las categorías
        ICollection<Categoria> GetCategorias();
        // Recibe un id de la categoría y obtiene una categoría específica
        Categoria GetCategoria(int CategoriaId);
        // Validan si existe la categoría, buscan por id o nombre
        bool ExisteCategoria(string nombre);
        bool ExisteCategoria(int id);
        // Creamos, actualizamos y borramos la categoría
        bool CrearCategoria(Categoria categoria);
        bool ActualizarCategoria(Categoria categoria);
        bool BorrarCategoria(Categoria categoria);
        // Valida cuando se gaurda una categoría
        bool Guardar();
    }
}
