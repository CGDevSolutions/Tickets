namespace Tickets.Models
{
    public class Auditoria
    {
        public int Id { get; set; }

        public string Tabla { get; set; }

        public int RegistroId { get; set; }

        public string Campo { get; set; }

        public string ValorAnterior { get; set; }

        public string ValorNuevo { get; set; }

        public int? UsuarioId { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}