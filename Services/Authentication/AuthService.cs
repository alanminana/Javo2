using Javo2.IServices.Authentication;
using Javo2.Models.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Javo2.Services.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IRolService _rolService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUsuarioService usuarioService,
            IRolService rolService,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _usuarioService = usuarioService;
            _rolService = rolService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            return await _usuarioService.AutenticarAsync(username, password);
        }

        public async Task<string> GenerateJwtTokenAsync(string username)
        {
            try
            {
                var usuario = await _usuarioService.GetUsuarioByNombreUsuarioAsync(username);
                if (usuario == null)
                {
                    return null;
                }

                // Obtener permisos del usuario
                var permisos = await _usuarioService.GetPermisosUsuarioAsync(usuario.UsuarioID);

                // Crear claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                    new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioID.ToString()),
                    new Claim(ClaimTypes.GivenName, usuario.Nombre),
                    new Claim(ClaimTypes.Surname, usuario.Apellido),
                    new Claim(ClaimTypes.Email, usuario.Email)
                };

                // Agregar roles como claims
                foreach (var usuarioRol in usuario.Roles)
                {
                    var rol = await _rolService.GetRolByIDAsync(usuarioRol.RolID);
                    if (rol != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, rol.Nombre));
                    }
                }

                // Agregar permisos como claims
                foreach (var permiso in permisos)
                {
                    if (permiso.Activo)
                    {
                        claims.Add(new Claim("Permission", permiso.Codigo));
                    }
                }

                // Crear token
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "JavoDevelopmentSecurityKey2024"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expires = DateTime.Now.AddHours(3);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"] ?? "JavoIssuer",
                    audience: _configuration["Jwt:Audience"] ?? "JavoAudience",
                    claims: claims,
                    expires: expires,
                    signingCredentials: creds
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar token JWT para el usuario {Username}", username);
                return null;
            }
        }

        public async Task<bool> RevokeTokenAsync(string username)
        {
            // En un entorno real, aquí se implementaría la revocación del token
            // Por ejemplo, agregando el token a una lista negra en una base de datos
            _logger.LogInformation("Token revocado para el usuario: {Username}", username);
            return await Task.FromResult(true);
        }
    }
}