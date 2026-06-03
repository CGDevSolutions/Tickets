using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1;
using Tickets.Models;

namespace Tickets.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<ComentarioTicket> ComentariosTicket { get; set; }

        public DbSet<Notificacion> Notificaciones { get; set; }
        
        public DbSet<ArchivoTicket> ArchivosTicket { get; set; }

        public DbSet<LogSistema> LogsSistema { get; set; }

        public DbSet<Auditoria> Auditoria { get; set; }
    }
}