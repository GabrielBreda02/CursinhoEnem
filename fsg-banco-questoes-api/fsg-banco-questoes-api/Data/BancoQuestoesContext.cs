using Microsoft.EntityFrameworkCore;
using BancoQuestoes.Api.Models;

namespace BancoQuestoes.Api.Data;

public class BancoQuestoesContext : DbContext
{
    public BancoQuestoesContext(DbContextOptions<BancoQuestoesContext> options) : base(options)
    {
    }

    public DbSet<Questao> Questoes { get; set; }
    public DbSet<Alternativa> Alternativas { get; set; }
    public DbSet<Prova> Provas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuração da entidade Questao
        modelBuilder.Entity<Questao>(entity =>
        {
            entity.HasKey(e => e.IdQuestao);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Disciplina).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AssuntosJson).HasDefaultValue("[]");
            entity.Ignore(e => e.Assuntos);
            
            entity.HasMany(e => e.Alternativas)
                  .WithOne(a => a.Questao)
                  .HasForeignKey(a => a.QuestaoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuração da entidade Alternativa
        modelBuilder.Entity<Alternativa>(entity =>
        {
            entity.HasKey(e => e.IdAlternativa);
            entity.Property(e => e.Descricao).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Correta).IsRequired();
        });

        // Configuração da entidade Prova
        modelBuilder.Entity<Prova>(entity =>
        {
            entity.HasKey(e => e.IdProva);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Disciplina).IsRequired().HasMaxLength(100);
            
            entity.HasMany(e => e.Questoes)
                  .WithMany(q => q.Provas)
                  .UsingEntity<Dictionary<string, object>>(
                      "ProvaQuestao",
                      j => j.HasOne<Questao>().WithMany().HasForeignKey("QuestaoId"),
                      j => j.HasOne<Prova>().WithMany().HasForeignKey("ProvaId"));
        });

        // Configuração da entidade Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SenhaHash).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}