using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tickets.Models
{
    public class Notificacion
    {
        [Key]
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public string Titulo { get; set; }

        public string Mensaje { get; set; }

        public bool Leida { get; set; } = false;

        public string? LinkUrl { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }
    }
}