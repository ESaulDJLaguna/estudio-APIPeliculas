using APIPeliculas.Models;
using APIPeliculas.Models.Dtos;
using APIPeliculas.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace APIPeliculas.Controllers
{
	[Authorize]
	[Route("api/Usuarios")]
	[ApiController]
	[ApiExplorerSettings(GroupName = "ApiPeliculasUsuarios")]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public class UsuariosController : Controller
	{
		private readonly IUsuarioRepository _userRepo;
		private readonly IMapper _mapper;
		private readonly IConfiguration _config;

		public UsuariosController(IUsuarioRepository userRepo, IMapper mapper, IConfiguration config)
		{
			_userRepo = userRepo;
			_mapper = mapper;
			_config = config;
		}

		/// <summary>
		/// Registro de nuevo usuario
		/// </summary>
		/// <returns></returns>
		[HttpGet]
		public IActionResult GetUsuarios()
		{
			var listaUsuarios = _userRepo.GetUsuarios();
			var listaUsuariosDto = new List<UsuarioDto>();

			foreach (var item in listaUsuarios)
			{
				listaUsuariosDto.Add(_mapper.Map<UsuarioDto>(item));
			}

			return Ok(listaUsuariosDto);
		}

		/// <summary>
		/// Obtener un usuario individual
		/// </summary>
		/// <param name="usuarioId"> Este es el id de la usuario</param>
		/// <returns></returns>
		[HttpGet("{usuarioId:int}", Name = "GetUsuarios")]
		public IActionResult GetUsuarios(int usuarioId)
		{
			var itemUsuario = _userRepo.GetUsuario(usuarioId);

			if (itemUsuario == null)
			{
				return NotFound();
			}

			var itemUsuarioDto = _mapper.Map<UsuarioDto>(itemUsuario);
			return Ok(itemUsuarioDto);
		}

		/// <summary>
		/// Registro de nuevo usuario
		/// </summary>
		/// <returns></returns>
		[AllowAnonymous]
		[HttpPost("Registro")]
		public IActionResult Registro(UsuarioAuthDto usuarioAuthDto)
		{
			// Convertimos a minúsculas el nomre del usuario
			usuarioAuthDto.UsuarioA = usuarioAuthDto.UsuarioA.ToLower();

			if (_userRepo.ExisteUsuario(usuarioAuthDto.UsuarioA))
			{
				return BadRequest("El usuario ya existe");
			}

			var usuarioACrear = new Usuario
			{
				UsuarioA = usuarioAuthDto.UsuarioA
			};

			var usuarioCreado = _userRepo.Registro(usuarioACrear, usuarioAuthDto.Password);
			return Ok(usuarioCreado);
        }

		/// <summary>
		/// Acceso/Autenticación de usuario
		/// </summary>
		/// <returns></returns>
		[AllowAnonymous]
		[HttpPost("Login")]
		public IActionResult Login(UsuarioAuthLoginDto usuarioAuthLoginDto)
		{
			//throw new Exception("Error generado");

			var usuarioDesdeRepo = _userRepo.Login(usuarioAuthLoginDto.UsuarioA,usuarioAuthLoginDto.Password);

			if (usuarioDesdeRepo == null)
			{
				return Unauthorized();
			}
			// Información que se quiere enviar en el token
			var claims = new[]
			{
				new Claim(ClaimTypes.NameIdentifier, usuarioDesdeRepo.Id.ToString()),
				new Claim(ClaimTypes.Name, usuarioDesdeRepo.UsuarioA.ToString())
			};
			//* Generación del token
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
			// Creamos las credenciales del token
			var credenciales = new SigningCredentials(key,SecurityAlgorithms.HmacSha512Signature);
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				// El tiempo de duración del token, 1 día, 1 mes, 1 año, etc. En esteejemplo durará un día
				Expires = DateTime.Now.AddDays(1),
				SigningCredentials = credenciales
			};
			var tokenHandler = new JwtSecurityTokenHandler();
			var token = tokenHandler.CreateToken(tokenDescriptor);

			return Ok(new
			{
				token = tokenHandler.WriteToken(token)
			});
		}
	}
}
