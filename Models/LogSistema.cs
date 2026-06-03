using System.ComponentModel.DataAnnotations;

namespace Tickets.Models
{
    public class LogSistema
    {
        [Key]
        public int Id { get; set; }

    public int? UsuarioId { get; set; }

        public string Accion { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;

        public string Ip { get; set; }
    }


}
