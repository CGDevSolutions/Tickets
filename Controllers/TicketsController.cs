using Microsoft.AspNetCore.Mvc;
using Tickets.Data;
using Tickets.Models;
using Microsoft.AspNetCore.Authorization;

namespace Tickets.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TicketsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Obtener()
        {
            var tickets = _context.Tickets.ToList();

            return Ok(tickets);
        }

        [HttpPost]
        public IActionResult Crear(Ticket ticket)
        {
            ticket.Codigo = "TK-" + DateTime.Now.Ticks;

            ticket.Estado = "Nuevo";

            ticket.FechaCreacion = DateTime.Now;

            _context.Tickets.Add(ticket);

            _context.SaveChanges();

            return Ok(ticket);
        }
    }
}