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

        public int UsuarioId { get; set; }

        public int? TecnicoId { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaCierre { get; set; }
    }
}