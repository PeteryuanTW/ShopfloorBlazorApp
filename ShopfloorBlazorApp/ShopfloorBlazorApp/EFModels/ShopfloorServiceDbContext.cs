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
    public virtual DbSet<DeveloperCommand> DeveloperCommands { get; set; }
    public virtual DbSet<MapConfig> MapConfigs { get; set; }
    public virtual DbSet<MapStationConfig> MapStationConfigs { get; set; }
    public virtual DbSet<ProcessConfig> ProcessConfigs { get; set; }
    public virtual DbSet<SignalRserverConfig> SignalRserverConfigs { get; set; }
    public virtual DbSet<StationConfig> StationConfigs { get; set; }
    public virtual DbSet<StationCustomAttribute> StationCustomAttributes { get; set; }
    public virtual DbSet<StationWorkOrderPartDetail> StationWorkOrderPartDetails { get; set; }
    public virtual DbSet<SystemConfig> SystemConfigs { get; set; }
    public virtual DbSet<WorkOrder> WorkOrders { get; set; }
    public virtual DbSet<WorkorderPart> WorkorderParts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=ShopfloorServiceDB;Trusted_Connection=True; trustServerCertificate=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeveloperCommand>(entity =>
        {
            entity.HasKey(e => e.CommandCode);
            entity.ToTable("DeveloperCommand");

            entity.Property(e => e.CommandCode).HasColumnName("CommandCode");
            entity.Property(e => e.CommandName).HasMaxLength(50);
            entity.Property(e => e.ParameterAmount).HasColumnName("ParameterAmount");
            entity.Property(e => e.Hint).HasColumnName("Hint");
            entity.Property(e => e.ParameterType).HasMaxLength(50);
        });
        modelBuilder.Entity<MapConfig>(entity =>
        {
            entity.HasKey(e => e.MapName);
            entity.ToTable("MapConfigs");

            entity.Property(e => e.MapName).HasMaxLength(50);
            entity.Property(e => e.MapImageName).HasMaxLength(50);
        });
        modelBuilder.Entity<MapStationConfig>(entity =>
        {
            entity.HasKey(e => new { e.MapName, e.StationName});
            entity.ToTable("MapStationConfigs");

            entity.Property(e => e.MapName).HasMaxLength(50);
            entity.Property(e => e.StationName).HasMaxLength(50);
            entity.Property(e => e.Position_x);
            entity.Property(e => e.Position_y);
            entity.Property(e => e.Width);
            entity.Property(e => e.Height);
        });
        modelBuilder.Entity<ProcessConfig>(entity =>
        {
            entity.HasKey(e => e.ProcessName);

            entity.ToTable("ProcessConfig");

            entity.Property(e => e.ProcessName).HasMaxLength(50);
        });
        modelBuilder.Entity<SignalRserverConfig>(entity =>
        {
            entity.HasKey(e => new { e.Protocol, e.Ip, e.Port, e.Route });

            entity.ToTable("SignalRServerConfig");

            entity.Property(e => e.Ip).HasMaxLength(50);
            entity.Property(e => e.Route).HasMaxLength(50);
        });

        modelBuilder.Entity<StationConfig>(entity =>
        {
            entity.HasKey(e => e.Name).HasName("PK_Station");

            entity.ToTable("StationConfig");

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.ProcessName).HasMaxLength(50);
        });

        modelBuilder.Entity<StationCustomAttribute>(entity =>
        {
            entity.HasKey(e => new { e.StationName, e.CustomAttribute });

            entity.ToTable("StationCustomAttribute");

            entity.Property(e => e.StationName).HasMaxLength(50);
            entity.Property(e => e.CustomAttribute).HasMaxLength(50);
            entity.Property(e => e.CustomName).HasMaxLength(50);
        });

        modelBuilder.Entity<StationWorkOrderPartDetail>(entity =>
        {
            entity.HasKey(e => e.ID);

            entity.ToTable("StationWorkOrderPartDetail");

            entity.Property(e => e.ID).HasColumnName("ID");
            entity.Property(e => e.StationName)
                .HasMaxLength(50)
                .HasColumnName("StationName");
            entity.Property(e => e.WorkOrderNo)
                .HasMaxLength(50)
                .HasColumnName("WorkOrderNO");
            entity.Property(e => e.PartName)
                .HasMaxLength(50)
                .HasColumnName("PartName");
            entity.Property(e => e.SerialNO)
                .HasMaxLength(50)
                .HasColumnName("SerialNO");
            entity.Property(e => e.WIP).HasColumnName("WIP");
            entity.Property(e => e.TargetAmount).HasColumnName("TargetAmount");
            entity.Property(e => e.OKAmount).HasColumnName("OKAmount");
            entity.Property(e => e.NGAmount).HasColumnName("NGAmount");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.FinishedTime).HasColumnType("datetime");
            entity.Property(e => e.Status).HasColumnName("Status");

            entity.Property(e => e.Bool_1).HasColumnName("Bool_1");
            entity.Property(e => e.Bool_2).HasColumnName("Bool_2");
            entity.Property(e => e.Bool_3).HasColumnName("Bool_3");
            entity.Property(e => e.Bool_4).HasColumnName("Bool_4");
            entity.Property(e => e.Bool_5).HasColumnName("Bool_5");
            
            entity.Property(e => e.Int_1).HasColumnName("Int_1");
            entity.Property(e => e.Int_2).HasColumnName("Int_2");
            entity.Property(e => e.Int_3).HasColumnName("Int_3");
            entity.Property(e => e.Int_4).HasColumnName("Int_4");
            entity.Property(e => e.Int_5).HasColumnName("Int_5");

            entity.Property(e => e.Double_1).HasColumnName("Double_1");
            entity.Property(e => e.Double_2).HasColumnName("Double_2");
            entity.Property(e => e.Double_3).HasColumnName("Double_3");
            entity.Property(e => e.Double_4).HasColumnName("Double_4");
            entity.Property(e => e.Double_5).HasColumnName("Double_5");
            
            entity.Property(e => e.String_1).HasColumnName("String_1");
            entity.Property(e => e.String_2).HasColumnName("String_2");
            entity.Property(e => e.String_3).HasColumnName("String_3");
            entity.Property(e => e.String_4).HasColumnName("String_4");
            entity.Property(e => e.String_5).HasColumnName("String_5");
        });

        modelBuilder.Entity<SystemConfig>(entity =>
        {
            entity.HasKey(e => e.ConfigName);

            entity.ToTable("SystemConfig");

            entity.Property(e => e.ConfigName).HasMaxLength(50).HasColumnName("ConfigName");
            entity.Property(e => e.ValueType);
            entity.Property(e => e.Value).HasMaxLength(50).HasColumnName("Value");
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.HasKey(e => e.WorkOrderNo);

            entity.ToTable("WorkOrder");

            entity.Property(e => e.WorkOrderNo)
                .HasMaxLength(50)
                .HasColumnName("WorkOrderNO");
            entity.Property(e => e.PartName)
                .HasMaxLength(50)
                .HasColumnName("PartName");
            entity.Property(e => e.HasSerialNo);
            entity.Property(e => e.FinishedTime).HasColumnType("datetime");
            entity.Property(e => e.Ngamount).HasColumnName("NGAmount");
            entity.Property(e => e.Okamount).HasColumnName("OKAmount");
            entity.Property(e => e.ProcessName).HasMaxLength(50);
            entity.Property(e => e.StartTime).HasColumnType("datetime");
        });

        modelBuilder.Entity<WorkorderPart>(entity =>
        {
            entity.HasKey(e => new { e.WorkorderNo, e.PartNo });

            entity.ToTable("WorkorderPart");

            entity.Property(e => e.WorkorderNo).HasMaxLength(50);
            entity.Property(e => e.PartNo).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}