using System.Security.Claims;
using Tickets.Data;
using Tickets.Models;

namespace Tickets.Services
{
    public class AuditoriaService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditoriaService(
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task RegistrarAuditoria(
            string tabla,
            int registroId,
            string campo,
            string valorAnterior,
            string valorNuevo
        )
        {
            var user = _httpContextAccessor.HttpContext?.User;

            int? usuarioId = null;

            if (user != null)
            {
                var claim = user.FindFirst("UsuarioId");

                if (claim != null)
                {
                    usuarioId = int.Parse(claim.Value);
                }
            }

            var auditoria = new Auditoria
            {
                Tabla = tabla,
                RegistroId = registroId,
                Campo = campo,
                ValorAnterior = valorAnterior,
                ValorNuevo = valorNuevo,
                UsuarioId = usuarioId
            };

            _context.Auditoria.Add(auditoria);

            await _context.SaveChangesAsync();
        }

       
            public async Task RegistrarCambios<T>(
                string tabla,
                int registroId,
                T anterior,
                T nuevo
            )
                    {
                        var user = _httpContextAccessor.HttpContext?.User;

                        int? usuarioId = null;

                        if (user != null)
                        {
                            var claim = user.FindFirst("UsuarioId");

                            if (claim != null)
                            {
                                usuarioId = int.Parse(claim.Value);
                            }
                        }

                        var propiedades = typeof(T).GetProperties();

                        foreach (var prop in propiedades)
                        {
                            var valorAnterior =
                                prop.GetValue(anterior)?.ToString();

                            var valorNuevo =
                                prop.GetValue(nuevo)?.ToString();

                            if (valorAnterior != valorNuevo)
                            {
                                var auditoria = new Auditoria
                                {
                                    Tabla = tabla,
                                    RegistroId = registroId,
                                    Campo = prop.Name,
                                    ValorAnterior = valorAnterior,
                                    ValorNuevo = valorNuevo,
                                    UsuarioId = usuarioId
                                };

                                _context.Auditoria.Add(auditoria);
                            }
                        }

                        await _context.SaveChangesAsync();
                    }


    }
}