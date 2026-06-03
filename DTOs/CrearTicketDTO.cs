using Microsoft.AspNetCore.Http;

namespace Tickets.DTOs
{
    public class CrearTicketDTO
    {
        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public string Prioridad { get; set; }

        public string Tipo { get; set; }

        public string Grupo { get; set; }

        public List<IFormFile>? Archivos { get; set; }
    }
}