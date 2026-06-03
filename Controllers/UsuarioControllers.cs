
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tickets.Data;
using Tickets.Models;

namespace Tickets.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================
        // LISTAR TODOS LOS USUARIOS
        // =========================================

        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> ObtenerUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Select(x => new
                {
                    x.Id,
                    x.Nombre,
                    x.Correo,
                    x.Rol,
                    x.Activo,
                    x.NumeroEmpleado,
                    x.Departamento
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // =========================================
        // TECNICOS
        // =========================================

        [Authorize(Roles = "Administrador")]
        [HttpGet("tecnicos")]
        public IActionResult Tecnicos()
        {
            var tecnicos = _context.Usuarios
                .Where(x => x.Rol == "Tecnico")
                .ToList();

            return Ok(tecnicos);
        }

        // =========================================
        // USUARIOS PARA ASIGNACION
        // =========================================

        [Authorize(Roles = "Administrador")]
        [HttpGet("usuarios-asignacion")]
        public IActionResult UsuariosAsignacion()
        {
            var usuarios = _context.Usuarios
                .Where(x =>
                    x.Rol == "Tecnico"
                    || x.Rol == "Administrador"
                )
                .Select(x => new
                {
                    x.Id,
                    x.Nombre,
                    x.Rol
                })
                .ToList();

            return Ok(usuarios);
        }

        // =========================================
        // CREAR USUARIO
        // =========================================

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        public async Task<IActionResult> CrearUsuario(
            [FromBody] Usuario usuario
        )
        {
            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            return Ok(usuario);
        }

        // =========================================
        // EDITAR USUARIO
        // =========================================

        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditarUsuario(
            int id,
            [FromBody] Usuario datos
        )
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(x => x.Id == id);

            if (usuario == null)
                return NotFound();

            usuario.Nombre = datos.Nombre;
            usuario.Correo = datos.Correo;
            usuario.Rol = datos.Rol;
            usuario.Activo = datos.Activo;
            usuario.NumeroEmpleado = datos.NumeroEmpleado;
            usuario.Departamento = datos.Departamento;

            if (!string.IsNullOrEmpty(datos.Password))
            {
                usuario.Password = datos.Password;
            }

            await _context.SaveChangesAsync();

            return Ok(usuario);
        }

        // =========================================
        // ELIMINAR USUARIO
        // =========================================

        [Authorize(Roles = "Administrador")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(x => x.Id == id);

            if (usuario == null)
                return NotFound();

            _context.Usuarios.Remove(usuario);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}

