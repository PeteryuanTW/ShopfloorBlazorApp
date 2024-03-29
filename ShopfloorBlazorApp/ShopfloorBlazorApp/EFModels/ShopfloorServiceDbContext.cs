using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ShopfloorBlazorApp.EFModels;

public partial class ShopfloorServiceDbContext : DbContext
{
    public ShopfloorServiceDbContext()
    {
    }

    public ShopfloorServiceDbContext(DbContextOptions<ShopfloorServiceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<StationConfig> StationConfigs { get; set; }

    public virtual DbSet<WorkOrder> WorkOrders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=ShopfloorServiceDB;Trusted_Connection=True; trustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StationConfig>(entity =>
        {
            entity.HasKey(e => e.Name).HasName("PK_Station");

            entity.ToTable("StationConfig");

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.ProcessName).HasMaxLength(50);
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.HasKey(e => e.WorkerOrderNo);

            entity.ToTable("WorkOrder");

            entity.Property(e => e.WorkerOrderNo)
                .HasMaxLength(50)
                .HasColumnName("WorkerOrderNO");
            entity.Property(e => e.FinishedTime).HasColumnType("datetime");
            entity.Property(e => e.Ngamount).HasColumnName("NGAmount");
            entity.Property(e => e.Okamount).HasColumnName("OKAmount");
            entity.Property(e => e.ProcessName).HasMaxLength(50);
            entity.Property(e => e.StartTime).HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
