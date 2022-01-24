using APIPeliculas.Models;
using APIPeliculas.Models.Dtos;
using APIPeliculas.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace APIPeliculas.Controllers
{
	[Authorize]
	[Route("api/Peliculas")]
	[ApiController]
	[ApiExplorerSettings(GroupName = "ApiPeliculas")]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public class PeliculasController : Controller
	{
		private readonly IPeliculaRepository _pelRepo;
		private readonly IWebHostEnvironment _hostingEnvironment;
		private readonly IMapper _mapper;

		public PeliculasController(IPeliculaRepository pelRepo, IMapper mapper, IWebHostEnvironment hostEnvironment)
		{
			_pelRepo = pelRepo;
			_mapper = mapper;
			_hostingEnvironment = hostEnvironment;
		}

		/// <summary>
		/// Obtener todas las peliculas
		/// </summary>
		/// <returns></returns>
		[AllowAnonymous]
		[HttpGet]
		public IActionResult GetPeliculas()
		{
			var listaPeliculas = _pelRepo.GetPeliculas();
			var listaPeliculasDto = new List<PeliculaDto>();

			foreach (var item in listaPeliculas)
			{
				listaPeliculasDto.Add(_mapper.Map<PeliculaDto>(item));
			}

			return Ok(listaPeliculasDto);
		}

		/// <summary>
		/// Obtener una pelicula individual
		/// </summary>
		/// <param name="peliculaId"> Este es el id de la pelicula</param>
		/// <returns></returns>
		[AllowAnonymous]
		[HttpGet("{peliculaId:int}", Name = "GetPeliculas")]
		public IActionResult GetPeliculas(int peliculaId)
		{
			var itemPelicula = _pelRepo.GetPelicula(peliculaId);

			if (itemPelicula == null)
			{
				return NotFound();
			}

			var itemPeliculaDto = _mapper.Map<PeliculaDto>(itemPelicula);
			return Ok(itemPeliculaDto);
		}

		// ERROR, AUNQUE SE PODRÍA HACER DE ESTA FORMA, YA EXISTE ARRIBA UN MÉTODO QUE PASA COMO PARÁMETRO UN ENTERO, PARA DIFERENCIARLO DE ESTE, SE LE DA UN NUEVO NOMBRE O UNA NUEVA RUTA
		//[HttpGet("{categoriaId:int}", Name = "GetPeliculasEnCategoria")]
		/// <summary>
		/// Obtener peliculas por categoria
		/// </summary>
		/// <param name="categoriaId"> Este es el id de la categoría</param>
		/// <returns></returns>
		[AllowAnonymous]
		[HttpGet("GetPeliculasEnCategoria/{categoriaId:int}")]
		public IActionResult GetPeliculasEnCategoria(int categoriaId)
        {
			var listaPeliculas = _pelRepo.GetPeliculasEnCategoria(categoriaId);

			if(listaPeliculas == null)
            {
				return NotFound();
            }

			var itemPelicula = new List<PeliculaDto>();

            foreach (var item in listaPeliculas)
            {
				itemPelicula.Add(_mapper.Map<PeliculaDto>(item));
            }

			return Ok(itemPelicula);
        }
		// Para pasar un dato a buscar, se hace desde la url de la API, utilizando un signo de interrogación y el nombre del parámetro (si incluye espacios, se ponen tal cual), así que utilizando Postman, se haría de la siguiente forma:
		// https://localhost:44317/api/Peliculas/Buscar?nombreABuscar=nombre con espacios
		/// <summary>
		/// Buscar peliculas por nombre o descripción
		/// </summary>
		/// <param name="nombreABuscar"> Este es el nombre a buscar</param>
		/// <returns></returns>
		[AllowAnonymous]
		[HttpGet("Buscar")]
		public IActionResult Buscar(string nombreABuscar)
        {
            try
            {
				var resultado = _pelRepo.BuscarPelicula(nombreABuscar);

				if(resultado.Any())
                {
					return Ok(resultado);
                }

				return NotFound();
            }
            catch (Exception)
            {
				return StatusCode(StatusCodes.Status500InternalServerError, "Error recuperando datos de la aplicación");
            }
        }

		/// <summary>
		/// Crear una nueva pelicula
		/// </summary>       
		/// <returns></returns>
		[HttpPost]
		public IActionResult CrearPelicula([FromForm] PeliculaCreateDto PeliculaDto)
		{
			if (PeliculaDto == null)
			{
				return BadRequest(ModelState);
			}
			if (_pelRepo.ExistePelicula(PeliculaDto.Nombre))
			{
				ModelState.AddModelError("", "La película ya existe");
				return StatusCode(404, ModelState);
			}

			//* Subida de archivos
			// Obtiene la imagen que se subió
			var archivo = PeliculaDto.Foto;
			// Accede hasta la carpeta (NO A fotos): C:\...\wwwroot
			string rutaPrincipal = _hostingEnvironment.WebRootPath;
			// Obtiene la información de la imagen, pero directamente desde el formulario dode se subió
			var archivos = HttpContext.Request.Form.Files;

			// Sí eligieron una imagen
			if(archivo.Length > 0)
			{
				// *Nueva imagen
				// Crea un nombre nuevo e irrepetible para la imagen
				var nombreFoto = Guid.NewGuid().ToString();
				// Dónde se guardará la imagen: C:\...\wwwroot\fotos
				var subidas = Path.Combine(rutaPrincipal, @"fotos");
				// FileName es el nombre del archivo que se subió (por ejemplo: foto.jpg)
				// Obtiene la extensión de la imagen (incluyendo el punto)
				var extension = Path.GetExtension(archivos[0].FileName);

				// Crea un archivo "vacío" y su propiedad Name será toda la ruta donde se guardará, concatenado con el nombre y la extensión
				// Por ejemplo: C:\...wwwroot\fotos\06c5cff6-ab8a-491c-887c-d960daf38489.jpg
				using (var fileStreams = new FileStream(Path.Combine(subidas, nombreFoto + extension), FileMode.Create))
				{
					// Copia toda la información en el archivo que se creó ("construye" la imagen)
					archivos[0].CopyTo(fileStreams);
				}
				PeliculaDto.RutaImagen = @"\fotos\" + nombreFoto + extension;
			}

			var pelicula = _mapper.Map<Pelicula>(PeliculaDto);

			if (!_pelRepo.CrearPelicula(pelicula))
			{
				ModelState.AddModelError("", $"Algo salió mal guardando el registro {pelicula.Nombre}");
				return StatusCode(500, ModelState);
			}
			//return Ok();
			return CreatedAtRoute("GetPeliculas", new { peliculaId = pelicula.Id }, pelicula);
		}

		/// <summary>
		/// Actualizar una película existente
		/// </summary>        
		/// <returns></returns>
		[HttpPatch("{peliculaId:int}", Name = "ActualizarPelicula")]
		public IActionResult ActualizarPelicula(int peliculaId, [FromBody] PeliculaUpdateDto peliculaUpdateDto)
		{
			if (peliculaUpdateDto == null || peliculaId != peliculaUpdateDto.Id)
			{
				return BadRequest(ModelState);
			}

			var pelicula = _mapper.Map<Pelicula>(peliculaUpdateDto);

			if (!_pelRepo.ActualizarPelicula(pelicula))
			{
				ModelState.AddModelError("", $"Algo salió mal actualizando el registro {pelicula.Nombre}");
				return StatusCode(500, ModelState);
			}
			return NoContent();
		}

		/// <summary>
		/// Borrar una pelicula existente
		/// </summary>
		/// <param name="peliculaId"> Este es el id de la película</param>
		/// <returns></returns>
		[HttpDelete("{peliculaId:int}", Name = "BorrarPelicula")]
		public IActionResult BorrarPelicula(int peliculaId)
		{
			if (!_pelRepo.ExistePelicula(peliculaId))
			{
				return NotFound();
			}

			var pelicula = _pelRepo.GetPelicula(peliculaId);

			if (!_pelRepo.BorrarPelicula(pelicula))
			{
				ModelState.AddModelError("", $"Algo salió mal borrando el registro {pelicula.Nombre}");
				return StatusCode(500, ModelState);
			}

			return NoContent();
		}
	}
}
