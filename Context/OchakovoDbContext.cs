using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Ochakovo.Entities;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Ochakovo.Context;

public partial class OchakovoDbContext : DbContext
{
    public OchakovoDbContext()
    {
    }

    public OchakovoDbContext(DbContextOptions<OchakovoDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<candidate> candidates { get; set; }

    public virtual DbSet<candidatevacancy> candidatevacancies { get; set; }

    public virtual DbSet<hr> hrs { get; set; }

    public virtual DbSet<vacancy> vacancies { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseMySql("Server=localhost;Database=ochakovo;User=root;Password=J#tu81/vfR;", 
                ServerVersion.AutoDetect("Server=localhost;Database=ochakovo;User=root;Password=J#tu81/vfR;"));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<candidate>(entity =>
        {
            entity.HasKey(e => e.CandidateId).HasName("PRIMARY");

            entity.Property(e => e.EducationType).HasColumnType("enum('вуз','колледж','другое')");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.Gender)
                .HasDefaultValueSql("'не указан'")
                .HasColumnType("enum('м','ж','не указан')");
            entity.Property(e => e.InstitutionName).HasMaxLength(100);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Status)
                .HasDefaultValueSql("'новый'")
                .HasColumnType("enum('новый','в процессе','откликнулся','отказ')");
            entity.Property(e => e.WorkReadiness).HasColumnType("enum('готов','не готов','после учёбы','в течение месяца')");
        });

        modelBuilder.Entity<candidatevacancy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasIndex(e => e.CandidateId, "CandidateId");

            entity.HasIndex(e => e.VacancyId, "VacancyId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Candidate).WithMany(p => p.candidatevacancies)
                .HasForeignKey(d => d.CandidateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("candidatevacancies_ibfk_1");

            entity.HasOne(d => d.Vacancy).WithMany(p => p.candidatevacancies)
                .HasForeignKey(d => d.VacancyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("candidatevacancies_ibfk_2");
        });

        modelBuilder.Entity<hr>(entity =>
        {
            entity.HasKey(e => e.HrId).HasName("PRIMARY");

            entity.ToTable("hr");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.Phone, "Phone").IsUnique();

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.Password).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<vacancy>(entity =>
        {
            entity.HasKey(e => e.VacancyId).HasName("PRIMARY");

            entity.Property(e => e.Department)
                .HasMaxLength(100)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.Requirement).HasColumnType("text");
            entity.Property(e => e.Salary).HasPrecision(10, 2);
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
