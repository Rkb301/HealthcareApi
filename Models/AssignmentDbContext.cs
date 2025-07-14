using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using HealthcareApi.Models;

namespace HealthcareApi.Models;

public partial class AssignmentDbContext : DbContext
{
    public AssignmentDbContext() { }

    public AssignmentDbContext(DbContextOptions<AssignmentDbContext> options)
        : base(options) { }

    public virtual DbSet<Appointment> Appointments { get; set; }
    public virtual DbSet<Doctor> Doctors { get; set; }
    public virtual DbSet<Patient> Patients { get; set; }
    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=ICPL15191\\MSSQLSERVER2025;Database=assignment;" +
                "User Id=sa;Password=Power@123$;TrustServerCertificate=True;");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.AppointmentID).HasName("PK__Appointm__8ECDFCA218E8DF08");
            entity.ToTable("Appointment");
            entity.Property(e => e.AppointmentDate).HasColumnType("datetime");
            entity.Property(e => e.Notes).HasColumnType("text");
            entity.Property(e => e.Reason).HasMaxLength(255).IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Scheduled");
            
            entity.HasOne(d => d.Doctor)
                .WithMany(p => p.Appointments)
                .HasForeignKey(d => d.DoctorID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Docto__440B1D61");
            
            entity.HasOne(d => d.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(d => d.PatientID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Appointme__Patie__4316F928");
        });

        modelBuilder.Entity<CurrentAppointmentsDTO>().HasNoKey().ToView(null);
        modelBuilder.Entity<CurrentAppointmentsDTO>().Property(e => e.AppointmentID).HasColumnName("AppointmentID");
        modelBuilder.Entity<CurrentAppointmentsDTO>().Property(e => e.PatientID).HasColumnName("PatientID");
        modelBuilder.Entity<CurrentAppointmentsDTO>().Property(e => e.DoctorID).HasColumnName("DoctorID");
        modelBuilder.Entity<CurrentAppointmentsDTO>().Property(e => e.Date).HasColumnName("date");
        modelBuilder.Entity<CurrentAppointmentsDTO>().Property(e => e.Reason).HasColumnName("Reason");
        modelBuilder.Entity<CurrentAppointmentsDTO>().Property(e => e.Status).HasColumnName("Status");
        modelBuilder.Entity<CurrentAppointmentsDTO>().Property(e => e.Notes).HasColumnName("Notes");
        modelBuilder.Entity<CurrentAppointmentsDTO>().Property(e => e.PatientName).HasColumnName("patientName");
        modelBuilder.Entity<CurrentAppointmentsDTO>().Property(e => e.DOB).HasColumnName("dob");
        modelBuilder.Entity<CurrentAppointmentsDTO>().Property(e => e.Gender).HasColumnName("gender");
        modelBuilder.Entity<CurrentAppointmentsDTO>().Property(e => e.Contact).HasColumnName("contact");
        modelBuilder.Entity<CurrentAppointmentsDTO>().Property(e => e.MedicalHistory).HasColumnName("medicalHistory");
        modelBuilder.Entity<CurrentAppointmentsDTO>().Property(e => e.Allergies).HasColumnName("allergies");
        modelBuilder.Entity<CurrentAppointmentsDTO>().Property(e => e.CurrentMedications).HasColumnName("currentMedications");

        // Configure other entities similarly
        // Add CreatedAt/ModifiedAt defaults if needed

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
