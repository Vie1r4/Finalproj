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
        public DbSet<Produto> Produtos => Set<Produto>();
        public DbSet<EntradaPaiol> EntradasPaiol => Set<EntradaPaiol>();
        public DbSet<PaiolAcesso> PaiolAcessos => Set<PaiolAcesso>();
        public DbSet<SaidaPaiol> SaidasPaiol => Set<SaidaPaiol>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Paiol>()
                .Property(p => p.LimiteMLE)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Produto>()
                .Property(p => p.NEMPorUnidade)
                .HasPrecision(18, 4);

            modelBuilder.Entity<EntradaPaiol>()
                .Property(e => e.Quantidade)
                .HasPrecision(18, 4);

            modelBuilder.Entity<EntradaPaiol>()
                .HasOne(e => e.Paiol)
                .WithMany()
                .HasForeignKey(e => e.PaiolId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EntradaPaiol>()
                .HasOne(e => e.Produto)
                .WithMany()
                .HasForeignKey(e => e.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PaiolAcesso>()
                .HasOne(a => a.Paiol)
                .WithMany()
                .HasForeignKey(a => a.PaiolId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SaidaPaiol>()
                .Property(s => s.Quantidade)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SaidaPaiol>()
                .HasOne(s => s.Paiol)
                .WithMany()
                .HasForeignKey(s => s.PaiolId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaidaPaiol>()
                .HasOne(s => s.Produto)
                .WithMany()
                .HasForeignKey(s => s.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
