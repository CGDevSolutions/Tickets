namespace Tickets.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        public string Nombre { get; set; }

        public string Correo { get; set; }

        public string Password { get; set; }

        public string Rol { get; set; }

        public string? Departamento { get; set; }

        public bool Activo { get; set; } = true;

        public string NumeroEmpleado { get; set; }
    }
}