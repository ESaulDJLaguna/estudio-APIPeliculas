using APIPeliculas.Models;
using APIPeliculas.Models.Dtos;
using APIPeliculas.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace APIPeliculas.Controllers
{
	// TODA LA API REQUIERE DE AUTORIZACIÓN PARA ACCEDER A LOS DATOS
	[Authorize]
	[Route("api/Categorias")]
	[ApiController]
	[ApiExplorerSettings(GroupName = "ApiPeliculasCategorias")]
	// Se agrega un código de estado para toda la clase
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public class CategoriasController : Controller
	{
		private readonly ICategoriaRepository _ctRepo;
		private readonly IMapper _mapper;

		public CategoriasController(ICategoriaRepository ctRepo, IMapper mapper)
		{
			_ctRepo = ctRepo;
			_mapper = mapper;
		}

		/// <summary>
		/// Obtener todas las catagoraías
		/// </summary>
		/// <returns></returns>

		// La petición GET es pública (NO REQUIERE AUTENTICACIÓN)
		[AllowAnonymous]
		[HttpGet]
		[ProducesResponseType(200, Type = typeof(List<CategoriaDto>))]
		[ProducesResponseType(400)]
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

		/// <summary>
		/// Obtener una categoría individual
		/// </summary>
		/// <param name="categoriaId">Este es el id de la categoría</param>
		/// <returns></returns>
		// La petición de un elemento GET es pública (NO REQUIERE AUTENTICACIÓN)
		[AllowAnonymous]
		[HttpGet("{categoriaId:int}", Name = "GetCategorias")]
		[ProducesResponseType(200, Type = typeof(CategoriaDto))]
		[ProducesResponseType(404)]
		[ProducesDefaultResponseType]
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

		/// <summary>
		/// Crear una nueva categoría
		/// </summary>
		/// <param name="categoriaDto"></param>
		/// <returns></returns>
		[HttpPost]
		[ProducesResponseType(201, Type = typeof(CategoriaDto))]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

		/// <summary>
		/// Actualizar una nueva categoría existente
		/// </summary>
		/// <param name="categoriaId"></param>
		/// <param name="categoriaDto"></param>
		/// <returns></returns>
		[HttpPatch("{categoriaId:int}", Name = "ActualizarCategoria")]
		[ProducesResponseType(204)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

		/// <summary>
		/// Borrar una categoría existente
		/// </summary>
		/// <param name="categoriaId"></param>
		/// <returns></returns>
		[HttpDelete("{categoriaId:int}", Name = "BorrarCategoria")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status409Conflict)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
