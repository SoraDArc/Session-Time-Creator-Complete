using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace STCAPI.Models;

public partial class StcdbContext : DbContext
{
    string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db", "stcdb.db");
    public StcdbContext()
    {
    }

    public StcdbContext(DbContextOptions<StcdbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditoryReserve> AuditoryReserves { get; set; }

    public virtual DbSet<BusyAuditory> BusyAuditories { get; set; }

    public virtual DbSet<Cabinet> Cabinets { get; set; }

    public virtual DbSet<GroupOfStudent> GroupOfStudents { get; set; }

    public virtual DbSet<Institute> Institutes { get; set; }

    public virtual DbSet<Lecturer> Lecturers { get; set; }

    public virtual DbSet<ScheduleTemplate> ScheduleTemplates { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<User> Users { get; set; }

    // optionsBuilder.UseSqlite($"Data Source=D:\\\\\\\\STC\\\\\\\\STCAPI\\\\\\\\db\\stcdb.db");
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={dbPath}");
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditoryReserve>(entity =>
        {
            entity.ToTable("auditory_reserve");

            entity.HasIndex(e => e.Id, "IX_auditory_reserve_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.ScheduleTemplatesId).HasColumnName("schedule_templates_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.UsersId).HasColumnName("users_id");

            entity.HasOne(d => d.ScheduleTemplates).WithMany(p => p.AuditoryReserves).HasForeignKey(d => d.ScheduleTemplatesId);

            entity.HasOne(d => d.Users).WithMany(p => p.AuditoryReserves)
                .HasForeignKey(d => d.UsersId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<BusyAuditory>(entity =>
        {
            entity.ToTable("busy_auditory");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Auditory).HasColumnName("auditory");
            entity.Property(e => e.AuditoryReserveId).HasColumnName("auditory_reserve_id");
            entity.Property(e => e.BusyDate).HasColumnName("busy_date");
            entity.Property(e => e.Subject).HasColumnName("subject");

            entity.HasOne(d => d.AuditoryReserve).WithMany(p => p.BusyAuditories)
                .HasForeignKey(d => d.AuditoryReserveId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Cabinet>(entity =>
        {
            entity.ToTable("cabinets");

            entity.HasIndex(e => e.Id, "IX_cabinets_id").IsUnique();

            entity.HasIndex(e => e.InstitutesId, "IX_cabinets_institutes_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.InstitutesId).HasColumnName("institutes_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.Institutes).WithMany(p => p.Cabinets)
                .HasForeignKey(d => d.InstitutesId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<GroupOfStudent>(entity =>
        {
            entity.ToTable("group_of_students");

            entity.HasIndex(e => e.Id, "IX_group_of_students_id").IsUnique();

            entity.HasIndex(e => e.InstitutesId, "IX_group_of_students_institutes_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.InstitutesId).HasColumnName("institutes_id");
            entity.Property(e => e.Title).HasColumnName("title");

            entity.HasOne(d => d.Institutes).WithMany(p => p.GroupOfStudents)
                .HasForeignKey(d => d.InstitutesId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Institute>(entity =>
        {
            entity.ToTable("institutes");

            entity.HasIndex(e => e.Id, "IX_institutes_id").IsUnique();

            entity.HasIndex(e => e.Id, "IX_institutes_id").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.ShortName).HasColumnName("short_name");
        });

        modelBuilder.Entity<Lecturer>(entity =>
        {
            entity.ToTable("lecturers");

            entity.HasIndex(e => e.Id, "IX_lecturers_id").IsUnique();

            entity.HasIndex(e => e.InstitutesId, "IX_lecturers_institutes_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.InstitutesId).HasColumnName("institutes_id");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Patronomyc).HasColumnName("patronomyc");
            entity.Property(e => e.Shortname).HasColumnName("shortname");
            entity.Property(e => e.Surname).HasColumnName("surname");

            entity.HasOne(d => d.Institutes).WithMany(p => p.Lecturers)
                .HasForeignKey(d => d.InstitutesId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ScheduleTemplate>(entity =>
        {
            entity.ToTable("schedule_templates");

            entity.HasIndex(e => e.Id, "IX_schedule_templates_id").IsUnique();

            entity.HasIndex(e => e.InstitutesId, "IX_schedule_templates_institutes_id");

            entity.HasIndex(e => e.OwnerUserId, "IX_schedule_templates_owner_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.InstitutesId).HasColumnName("institutes_id");
            entity.Property(e => e.JsonTemplate).HasColumnName("json_template");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.OwnerUserId).HasColumnName("owner_user_id");

            entity.HasOne(d => d.Institutes).WithMany(p => p.ScheduleTemplates)
                .HasForeignKey(d => d.InstitutesId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.OwnerUser).WithMany(p => p.ScheduleTemplates)
                .HasForeignKey(d => d.OwnerUserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("subjects");

            entity.HasIndex(e => e.Id, "IX_subjects_id").IsUnique();

            entity.HasIndex(e => e.InstitutesId, "IX_subjects_institutes_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.InstitutesId).HasColumnName("institutes_id");
            entity.Property(e => e.Name).HasColumnName("name");

            entity.HasOne(d => d.Institutes).WithMany(p => p.Subjects)
                .HasForeignKey(d => d.InstitutesId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasIndex(e => e.Id, "IX_users_id").IsUnique();

            entity.HasIndex(e => e.InstitutesId, "IX_users_institutes_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.InstitutesId).HasColumnName("institutes_id");
            entity.Property(e => e.Login).HasColumnName("login");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.Patronymic).HasColumnName("patronymic");
            entity.Property(e => e.Surname).HasColumnName("surname");

            entity.HasOne(d => d.Institutes).WithMany(p => p.Users)
                .HasForeignKey(d => d.InstitutesId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
