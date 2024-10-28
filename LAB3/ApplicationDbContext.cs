using LAB3.Models;
using Microsoft.EntityFrameworkCore;

namespace LAB3
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>()
            .Property(u => u.Status)
            .HasConversion<string>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
