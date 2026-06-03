using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tickets.Data;

namespace Tickets.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificacionesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificacionesController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================
        // OBTENER MIS NOTIFICACIONES
        // =========================================

        [HttpGet]
        public IActionResult Obtener()
        {
            var usuarioId = int.Parse(
                User.FindFirst("UsuarioId")!.Value
            );

            var notificaciones = _context.Notificaciones
                .Where(x => x.UsuarioId == usuarioId)
                .OrderByDescending(x => x.Fecha)
                .Take(30)
                .Select(x => new
                {
                    x.Id,
                    x.Titulo,
                    x.Mensaje,
                    x.Leida,
                    x.LinkUrl,
                    x.Fecha
                })
                .ToList();

            return Ok(notificaciones);
        }

        // =========================================
        // MARCAR COMO LEIDA
        // =========================================

        [HttpPut("leer/{id}")]
        public IActionResult MarcarLeida(int id)
        {
            var usuarioId = int.Parse(
                User.FindFirst("UsuarioId")!.Value
            );

            var notificacion = _context.Notificaciones
                .FirstOrDefault(x =>
                    x.Id == id
                    && x.UsuarioId == usuarioId
                );

            if (notificacion == null)
            {
                return NotFound();
            }

            notificacion.Leida = true;

            _context.SaveChanges();

            return Ok();
        }
    }
}