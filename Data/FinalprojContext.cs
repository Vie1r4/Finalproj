using Finalproj.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Finalproj.Data
{
    public class FinalprojContext : IdentityDbContext<Microsoft.AspNetCore.Identity.IdentityUser>
    {
        public FinalprojContext(DbContextOptions<FinalprojContext> options)
            : base(options)
        {
        }

        public DbSet<Paiol> Paiol => Set<Paiol>();
        public DbSet<Perfil> Perfis => Set<Perfil>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Paiol>()
                .Property(p => p.LimiteMLE)
                .HasPrecision(18, 2);
        }
    }
}
