using APIPeliculas.Models;
using APIPeliculas.Models.Dtos;
using APIPeliculas.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace APIPeliculas.Controllers
{
	[Route("api/Categorias")]
	[ApiController]
	public class CategoriasController : Controller
	{
		private readonly ICategoriaRepository _ctRepo;
		private readonly IMapper _mapper;

		public CategoriasController(ICategoriaRepository ctRepo, IMapper mapper)
		{
			_ctRepo = ctRepo;
			_mapper = mapper;
		}

		[HttpGet]
		public IActionResult GetCategorias()
		{
			var listaCategorias = _ctRepo.GetCategorias();
			var listaCategoriasDto = new List<CategoriaDto>();

			foreach (var item in listaCategorias)
			{
				listaCategoriasDto.Add(_mapper.Map<CategoriaDto>(item));
			}

			return Ok(listaCategoriasDto);
		}

		[HttpGet("{categoriaId:int}", Name = "GetCategorias")]
		public IActionResult GetCategorias(int categoriaId)
		{
			var itemCategoria = _ctRepo.GetCategoria(categoriaId);

			if (itemCategoria == null)
			{
				return NotFound();
			}

			var itemCategoriaDto = _mapper.Map<CategoriaDto>(itemCategoria);
			return Ok(itemCategoriaDto);
		}

		[HttpPost]
		public IActionResult CrearCategoria([FromBody] CategoriaDto categoriaDto)
		{
			if (categoriaDto == null)
			{
				return BadRequest(ModelState);
			}
			if (_ctRepo.ExisteCategoria(categoriaDto.Nombre))
			{
				ModelState.AddModelError("", "La categoría ya existe");
				return StatusCode(404, ModelState);
			}

			var categoria = _mapper.Map<Categoria>(categoriaDto);

			if (!_ctRepo.CrearCategoria(categoria))
			{
				ModelState.AddModelError("", $"Algo salió mal guardando el registro {categoria.Nombre}");
				return StatusCode(500, ModelState);
			}
			//return Ok();

			// Muestra el último registro creado. GetCategorias es el nombre que se le dio al método del mismo nombre, pero está refiriéndose al nombre en el HttpGet(...,Name=""). categoriaId hace referencia a los parámetros HttpGet del método GetCategorias y categoria.Id busca en la variable creada antes del último if de este método, además le pasamos la variable categoria
			return CreatedAtRoute("GetCategorias", new { categoriaId = categoria.Id }, categoria);
		}

		[HttpPatch("{categoriaId:int}", Name = "ActualizarCategoria")]
		public IActionResult ActualizarCategoria(int categoriaId, [FromBody] CategoriaDto categoriaDto)
		{
			if (categoriaDto == null || categoriaId != categoriaDto.Id)
			{
				return BadRequest(ModelState);
			}

			var categoria = _mapper.Map<Categoria>(categoriaDto);

			if (!_ctRepo.ActualizarCategoria(categoria))
			{
				ModelState.AddModelError("", $"Algo salió mal actualizando el registro {categoria.Nombre}");
				return StatusCode(500, ModelState);
			}
			return NoContent();
		}

		[HttpDelete("{categoriaId:int}", Name = "BorrarCategoria")]
		public IActionResult BorrarCategoria(int categoriaId)
		{
			if (!_ctRepo.ExisteCategoria(categoriaId))
			{
				return NotFound();
			}

			var categoria = _ctRepo.GetCategoria(categoriaId);

			if (!_ctRepo.BorrarCategoria(categoria))
			{
				ModelState.AddModelError("", $"Algo salió mal borrando el registro {categoria.Nombre}");
				return StatusCode(500, ModelState);
			}

			return NoContent();
		}
	}
}
