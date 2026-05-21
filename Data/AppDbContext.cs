

    using global::Tickets.Models;
    using Microsoft.EntityFrameworkCore;
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
        }
    }

