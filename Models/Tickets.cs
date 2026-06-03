using System.ComponentModel.DataAnnotations.Schema;

namespace Tickets.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        public string Codigo { get; set; }

        public string Titulo { get; set; }

        public string Descripcion { get; set; }

        public string Estado { get; set; }

        public string Prioridad { get; set; }

        public string? Tipo { get; set; }

        public string? Grupo { get; set; }

        // =====================================
        // USUARIO CREADOR
        // =====================================

        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        // =====================================
        // TECNICO/ADMIN ASIGNADO
        // =====================================

        public int? TecnicoId { get; set; }

        [ForeignKey("TecnicoId")]
        public Usuario? Tecnico { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaCierre { get; set; }

        public ICollection<ArchivoTicket>? Archivos { get; set; }
    }
}