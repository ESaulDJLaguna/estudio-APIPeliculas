using APIPeliculas.Data;
using APIPeliculas.Models;
using APIPeliculas.Repository.IRepository;
using System.Collections.Generic;
using System.Linq;

namespace APIPeliculas.Repository
{
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly ApplicationDbContext _bd;

        public CategoriaRepository(ApplicationDbContext bd)
        {
            _bd = bd;
        }
        public bool ActualizarCategoria(Categoria categoria)
        {
            _bd.Categoria.Update(categoria);
            return Guardar();
        }

        public bool BorrarCategoria(Categoria categoria)
        {
            _bd.Categoria.Remove(categoria);
            return Guardar();
        }

        public bool CrearCategoria(Categoria categoria)
        {
            _bd.Categoria.Add(categoria);
            return Guardar();
        }

        public bool ExisteCategoria(string nombre)
        {
            // Accedemos por el contexto (_db) a Categoria y buscamos con el método Any (de Linq), si existe.
            // c contendrá las variables (campos) del modelo (tabla) Categoria, convertiremos a minúsculas el nombre y eliminaremos espacios
            // a los lados y lo compararemos con el nombre de la catagoría buscada
            bool valor = _bd.Categoria.Any(c => c.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
            return valor;
        }

        public bool ExisteCategoria(int id)
        {
            return _bd.Categoria.Any(c => c.Id == id);
        }

        public Categoria GetCategoria(int CategoriaId)
        {
            return _bd.Categoria.FirstOrDefault(c=> c.Id == CategoriaId);
        }

        public ICollection<Categoria> GetCategorias()
        {
            // Ordenado de forma ascendente
            return _bd.Categoria.OrderBy(c => c.Nombre).ToList();
            // Ordenado de forma descendente
            // return _bd.Categoria.OrderByDescending(c => c.Nombre).ToList();
        }

        public bool Guardar()
        {
            return _bd.SaveChanges() >= 0 ? true : false;
        }
    }
}
