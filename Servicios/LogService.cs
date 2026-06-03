using System.Security.Claims;
using Tickets.Data;
using Tickets.Models;

namespace Tickets.Services
{
    public class LogService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

    public LogService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor
    )
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task RegistrarLog(string accion)
        {
            var user = _httpContextAccessor.HttpContext?.User;

            int? usuarioId = null;

            if (user != null)
            {
                var idClaim = user.FindFirst("UsuarioId");

                if (idClaim != null)
                {
                    usuarioId = int.Parse(idClaim.Value);
                }
            }

            var ip =
                _httpContextAccessor.HttpContext?
                .Connection?
                .RemoteIpAddress?
                .ToString();

            var log = new LogSistema
            {
                UsuarioId = usuarioId,
                Accion = accion,
                Ip = ip
            };

            _context.LogsSistema.Add(log);

            await _context.SaveChangesAsync();
        }
    }


}
