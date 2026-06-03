using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tickets.Data;
using Tickets.Models;

namespace Tickets.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ComentariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ComentariosController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================
        // OBTENER COMENTARIOS DE UN TICKET
        // =========================================

        [HttpGet("{ticketId}")]
        public IActionResult Obtener(int ticketId)
        {
            var comentarios = _context.ComentariosTicket
                .Where(x => x.TicketId == ticketId)
                .OrderBy(x => x.Fecha)
                .Select(x => new
                {
                    x.Id,
                    x.TicketId,
                    x.Comentario,
                    x.Fecha,
                    x.UsuarioId,

                    Usuario = _context.Usuarios
                        .Where(u => u.Id == x.UsuarioId)
                        .Select(u => new
                        {
                            u.Nombre,
                            u.Rol
                        })
                        .FirstOrDefault()
                })
                .ToList();

            return Ok(comentarios);
        }

        // =========================================
        // CREAR COMENTARIO
        // =========================================

        [HttpPost]
        public IActionResult Crear(ComentarioTicket comentario)
        {
            var usuarioId = int.Parse(
                User.FindFirst("UsuarioId")!.Value
            );

            comentario.UsuarioId = usuarioId;

            comentario.Fecha = DateTime.Now;

            _context.ComentariosTicket.Add(comentario);

            _context.SaveChanges();

            // =========================================
            // OBTENER TICKET
            // =========================================

            var ticket = _context.Tickets
                .FirstOrDefault(x => x.Id == comentario.TicketId);

            if (ticket != null)
            {
                var usuarioComentario = _context.Usuarios
                    .FirstOrDefault(x => x.Id == usuarioId);

                // =========================================
                // NOTIFICAR USUARIO DEL TICKET
                // =========================================

                if (ticket.UsuarioId != usuarioId)
                {
                    _context.Notificaciones.Add(new Notificacion
                    {
                        UsuarioId = ticket.UsuarioId,

                        Titulo = "Nuevo comentario",

                        Mensaje =
                            $"{usuarioComentario.Nombre} comentó en el ticket {ticket.Codigo}",

                        LinkUrl = $"/tickets/{ticket.Id}"
                    });
                }

                // =========================================
                // NOTIFICAR TECNICO ASIGNADO
                // =========================================

                if (
                    ticket.TecnicoId != null
                    && ticket.TecnicoId != usuarioId
                )
                {
                    _context.Notificaciones.Add(new Notificacion
                    {
                        UsuarioId = ticket.TecnicoId.Value,

                        Titulo = "Nuevo comentario",

                        Mensaje =
                            $"{usuarioComentario.Nombre} comentó en el ticket {ticket.Codigo}",

                        LinkUrl = $"/tickets/{ticket.Id}"
                    });
                }

                _context.SaveChanges();
            }

            return Ok(comentario);
        }
    }
}