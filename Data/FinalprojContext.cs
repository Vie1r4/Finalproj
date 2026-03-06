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
        public DbSet<PaiolDocumentoExtra> PaiolDocumentoExtras => Set<PaiolDocumentoExtra>();
        public DbSet<Perfil> Perfis => Set<Perfil>();
        public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
        public DbSet<FuncionarioDocumentoExtra> FuncionarioDocumentoExtras => Set<FuncionarioDocumentoExtra>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<ClienteDocumentoExtra> ClienteDocumentoExtras => Set<ClienteDocumentoExtra>();
        public DbSet<Produto> Produtos => Set<Produto>();
        public DbSet<EntradaPaiol> EntradasPaiol => Set<EntradaPaiol>();
        public DbSet<PaiolAcesso> PaiolAcessos => Set<PaiolAcesso>();
        public DbSet<SaidaPaiol> SaidasPaiol => Set<SaidaPaiol>();
        public DbSet<Encomenda> Encomendas => Set<Encomenda>();
        public DbSet<EncomendaItem> EncomendaItems => Set<EncomendaItem>();
        public DbSet<Reserva> Reservas => Set<Reserva>();
        public DbSet<LogSistema> LogSistema => Set<LogSistema>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Paiol>()
                .Property(p => p.LimiteMLE)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PaiolDocumentoExtra>()
                .HasOne(d => d.Paiol)
                .WithMany(p => p.DocumentosExtras)
                .HasForeignKey(d => d.PaiolId)
                .OnDelete(DeleteBehavior.Cascade);

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

            modelBuilder.Entity<Encomenda>()
                .HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EncomendaItem>()
                .HasOne(i => i.Encomenda)
                .WithMany(e => e.Itens)
                .HasForeignKey(i => i.EncomendaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EncomendaItem>()
                .HasOne(i => i.Produto)
                .WithMany()
                .HasForeignKey(i => i.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Encomenda)
                .WithMany()
                .HasForeignKey(r => r.EncomendaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Produto)
                .WithMany()
                .HasForeignKey(r => r.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reserva>()
                .HasIndex(r => new { r.EncomendaId, r.ProdutoId })
                .IsUnique();

            modelBuilder.Entity<EncomendaItem>()
                .Property(i => i.QuantidadePedida)
                .HasPrecision(18, 4);

            modelBuilder.Entity<Reserva>()
                .Property(r => r.Quantidade)
                .HasPrecision(18, 4);

            modelBuilder.Entity<SaidaPaiol>()
                .HasOne(s => s.EntradaPaiol)
                .WithMany()
                .HasForeignKey(s => s.EntradaPaiolId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<FuncionarioDocumentoExtra>()
                .HasOne(d => d.Funcionario)
                .WithMany(f => f.DocumentosExtras)
                .HasForeignKey(d => d.FuncionarioId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClienteDocumentoExtra>()
                .HasOne(d => d.Cliente)
                .WithMany(c => c.DocumentosExtras)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
