using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProjectOverview.Models;

public partial class ProjectOverviewContext : DbContext
{
    public ProjectOverviewContext()
    {
    }

    public ProjectOverviewContext(DbContextOptions<ProjectOverviewContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Country> Countries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=STAMOS611\\MSSQLSERVER01;Database=PROJECT_OVERVIEW;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("COUNTRIES");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Borders).HasMaxLength(50);
            entity.Property(e => e.Capital).HasMaxLength(50);
            entity.Property(e => e.CommonName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
