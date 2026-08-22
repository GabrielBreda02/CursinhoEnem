using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
    public DbSet<TemaRedacao> TemasRedacao { get; set; }
    public DbSet<TentativaProva> Tentativas { get; set; }
    public DbSet<RespostaAluno> RespostasAluno { get; set; }
    public DbSet<Turma> Turmas { get; set; }

    // O SQLite não guarda o Kind do DateTime — toda leitura volta com Kind=Unspecified,
    // o que faz o Newtonsoft omitir o "Z" na serialização e o navegador interpretar o
    // horário como local em vez de UTC (o app inteiro só grava DateTime.UtcNow, então é
    // seguro forçar Utc de volta em toda leitura).
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<UtcNullableDateTimeConverter>();
    }

    private class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter() : base(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }

    private class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
    {
        public UtcNullableDateTimeConverter() : base(
            v => v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
        {
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configuração da entidade Questao
        modelBuilder.Entity<Questao>(entity =>
        {
            entity.HasKey(e => e.IdQuestao);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AssuntosJson).HasDefaultValue("[]");
            entity.Property(e => e.Area).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ImagemUrl).HasMaxLength(500);
            entity.Property(e => e.Fonte).HasMaxLength(150);
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
            entity.Property(e => e.TempoLimiteMinutos).IsRequired();

            entity.HasMany(e => e.Questoes)
                  .WithMany(q => q.Provas)
                  .UsingEntity<Dictionary<string, object>>(
                      "ProvaQuestao",
                      j => j.HasOne<Questao>().WithMany().HasForeignKey("QuestaoId"),
                      j => j.HasOne<Prova>().WithMany().HasForeignKey("ProvaId"));

            entity.HasOne(e => e.TemaRedacao)
                  .WithMany()
                  .HasForeignKey(e => e.TemaRedacaoId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Turma)
                  .WithMany(t => t.Provas)
                  .HasForeignKey(e => e.TurmaId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuração da entidade Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.SenhaHash).IsRequired();
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasOne(e => e.Turma)
                  .WithMany(t => t.Alunos)
                  .HasForeignKey(e => e.TurmaId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuração da entidade Turma
        modelBuilder.Entity<Turma>(entity =>
        {
            entity.HasKey(e => e.IdTurma);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
        });

        // Configuração da entidade TemaRedacao
        modelBuilder.Entity<TemaRedacao>(entity =>
        {
            entity.HasKey(e => e.IdTemaRedacao);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(300);
            entity.Property(e => e.TextoMotivador).IsRequired();
            entity.Property(e => e.Fonte).HasMaxLength(150);
        });

        // Configuração da entidade TentativaProva
        modelBuilder.Entity<TentativaProva>(entity =>
        {
            entity.HasKey(e => e.IdTentativa);

            entity.HasOne(e => e.Prova)
                  .WithMany()
                  .HasForeignKey(e => e.ProvaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Aluno)
                  .WithMany()
                  .HasForeignKey(e => e.AlunoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(e => e.NotaRedacao);
        });

        // Configuração da entidade RespostaAluno
        modelBuilder.Entity<RespostaAluno>(entity =>
        {
            entity.HasKey(e => e.IdResposta);

            entity.HasOne(e => e.Tentativa)
                  .WithMany(t => t.Respostas)
                  .HasForeignKey(e => e.TentativaId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Questao)
                  .WithMany()
                  .HasForeignKey(e => e.QuestaoId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AlternativaSelecionada)
                  .WithMany()
                  .HasForeignKey(e => e.AlternativaSelecionadaId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.TentativaId, e.QuestaoId }).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}