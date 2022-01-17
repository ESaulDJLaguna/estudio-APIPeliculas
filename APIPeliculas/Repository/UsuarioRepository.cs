using APIPeliculas.Data;
using APIPeliculas.Models;
using APIPeliculas.Repository.IRepository;
using System.Collections.Generic;
using System.Linq;

namespace APIPeliculas.Repository
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApplicationDbContext _bd;

        public UsuarioRepository(ApplicationDbContext bd)
        {
            _bd = bd;
        }

        public bool ExisteUsuario(string usuario)
        {
            if(_bd.Usuario.Any(x => x.UsuarioA == usuario))
            {
                return true;
            }

            return false;
        }

        public Usuario GetUsuario(int UsuarioId)
        {
            return _bd.Usuario.FirstOrDefault(c => c.Id == UsuarioId);
        }

        public ICollection<Usuario> GetUsuarios()
        {
            return _bd.Usuario.OrderBy(c => c.UsuarioA).ToList();
        }

        public bool Guardar()
        {
            return _bd.SaveChanges() >= 0 ? true : false;
        }

        public Usuario Login(string usuario, string password)
        {
            var user = _bd.Usuario.FirstOrDefault(x => x.UsuarioA == usuario);

            if(user == null)
            {
                return null;
            }

            if(!VerificaPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return null;
            }

            return user;
        }

        public Usuario Registro(Usuario usuario, string password)
        {
            byte[] passwordHash, passwordSalt;

            CrearPasswordHash(password, out passwordHash, out passwordSalt);

            usuario.PasswordHash = passwordHash;
            usuario.PasswordSalt = passwordSalt;

            _bd.Usuario.Add(usuario);
            Guardar();

            return usuario;
        }

        private void CrearPasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            /*
             * HMACSHA512 (Class). Calcula un código de autenticación de mensajes basado en hash (HMAC) mediante la función SHA512
             * HMACSHA512(). Inicializa una nueva instancia con una llave generada de manera aleatoria.
             * HMACSHA512(Byte[]). Inicializa una nueva instancia con una llave específica.
             * ComputedHash (Método sobrecargado)
             * ComputedHash(Byte[]). Calcula el valor hash para la matriz de bytes especificada.
            */
            // Instancia la clase HMACSHA512 para encriptar la contraseña, la llave Salt se genera de forma aleatoria
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                // Key: obtiene o establece la llave a usar en el algoritmo hash
                passwordSalt = hmac.Key;
                // Convierte la contraseña a Byte[] y con el método ComputedHash() encríptala.
                // passwordHash contiene la contraseña encriptada
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerificaPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            /*
             * HMACSHA512 (Class). Calcula un código de autenticación de mensajes basado en hash (HMAC) mediante la función SHA512
             * HMACSHA512(). Inicializa una nueva instancia con una llave generada de manera aleatoria.
             * HMACSHA512(Byte[]). Inicializa una nueva instancia con una llave específica.
             * ComputedHash (Método sobrecargado)
             * ComputedHash(Byte[]). Calcula el valor hash para la matriz de bytes especificada.
            */
            // Asigna en el constructor la llave que se usará para encriptar una contraseña
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                // Convierte la contraseña escrita por el usuario (en el front) a una cadena de bytes[] y lo pasa como parámetro al método
                // ComputeHash() para convertirla en una contraseña encriptada con hash utilizando como llave Salt, la que se pasó como
                // parámetro en el constructor
                var hashComputado = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));

                // Compara la contraseña creada con la contraseña encriptada traida de la base de datos, si son diferentes...
                for (int i = 0; i < hashComputado.Length; i++)
                {
                    // ... esa no es la contraseña del usuario
                    if (hashComputado[i] != passwordHash[i]) return false;
                }
            }
            // Las contraseñas coinciden
            return true;
        }
    }
}
