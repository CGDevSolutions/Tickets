using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tickets.Models
{
    public class ArchivoTicket
    {
        [Key]
        public int Id { get; set; }

        public int TicketId { get; set; }

        public string NombreArchivo { get; set; }

        public string RutaArchivo { get; set; }

        public DateTime Fecha { get; set; }

        [ForeignKey("TicketId")]
        public Ticket Ticket { get; set; }
    }
}