using APIPeliculas.Models;
using APIPeliculas.Models.Dtos;
using APIPeliculas.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;

namespace APIPeliculas.Controllers
{
	[Route("api/Peliculas")]
	[ApiController]
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

			// Subida de archivos
			var archivo = PeliculaDto.Foto;
			string rutaPrincipal = _hostingEnvironment.WebRootPath;
			var archivos = HttpContext.Request.Form.Files;

			if(archivo.Length > 0)
			{
				// Nueva imagen
				var nombreFoto = Guid.NewGuid().ToString();
				var subidas = Path.Combine(rutaPrincipal, @"fotos");
				var extension = Path.GetExtension(archivos[0].FileName);

				using (var fileStreams = new FileStream(Path.Combine(subidas, nombreFoto + extension), FileMode.Create))
				{
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

		[HttpPatch("{peliculaId:int}", Name = "ActualizarPelicula")]
		public IActionResult ActualizarPelicula(int peliculaId, [FromBody] PeliculaDto peliculaDto)
		{
			if (peliculaDto == null || peliculaId != peliculaDto.Id)
			{
				return BadRequest(ModelState);
			}

			var pelicula = _mapper.Map<Pelicula>(peliculaDto);

			if (!_pelRepo.ActualizarPelicula(pelicula))
			{
				ModelState.AddModelError("", $"Algo salió mal actualizando el registro {pelicula.Nombre}");
				return StatusCode(500, ModelState);
			}
			return NoContent();
		}

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
