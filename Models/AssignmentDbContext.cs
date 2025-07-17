using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using HealthcareApi.Models;

namespace HealthcareApi.Models
{
    public partial class AssignmentDbContext : DbContext
    {
        public AssignmentDbContext() { }

        public AssignmentDbContext(DbContextOptions<AssignmentDbContext> options) : base(options) { }

        // PRESERVED: All existing DbSets
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Patient> Patients { get; set; }
        public virtual DbSet<Doctor> Doctors { get; set; }
        public virtual DbSet<Appointment> Appointments { get; set; }
        
        // ADDED: Additional DbSets for complete healthcare system
        public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }
        public virtual DbSet<Prescription> Prescriptions { get; set; }

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
            // ── Appointment ──────────────────────────────────────────────────────
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.ToTable("Appointment");
                entity.HasKey(e => e.AppointmentID).HasName("PK_Appointment");
                entity.Property(e => e.AppointmentDate).HasColumnType("datetime");
                entity.Property(e => e.Reason)
                      .HasMaxLength(255)
                      .IsUnicode(false);
                entity.Property(e => e.Status)
                      .HasMaxLength(20)
                      .IsUnicode(false)
                      .HasDefaultValue("Scheduled");
                entity.Property(e => e.Notes).HasColumnType("text");
        
                // If Doctor or Patient is deleted, set FK to null
                entity.HasOne(e => e.Doctor)
                      .WithMany(d => d.Appointments)
                      .HasForeignKey(e => e.DoctorID)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK_Appointment_Doctor");
        
                entity.HasOne(e => e.Patient)
                      .WithMany(p => p.Appointments)
                      .HasForeignKey(e => e.PatientID)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK_Appointment_Patient");
        
                // Automatic timestamp on modify
                entity.Property(e => e.ModifiedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });
        
            // ── CurrentAppointmentsDTO (no key, view) ─────────────────────────────
            modelBuilder.Entity<CurrentAppointmentsDTO>(eb =>
            {
                eb.HasNoKey().ToView(null);
                eb.Property(v => v.AppointmentID).HasColumnName("AppointmentID");
                eb.Property(v => v.PatientID).HasColumnName("PatientID");
                eb.Property(v => v.DoctorID).HasColumnName("DoctorID");
                eb.Property(v => v.Date).HasColumnName("date");
                eb.Property(v => v.Reason).HasColumnName("Reason");
                eb.Property(v => v.Status).HasColumnName("Status");
                eb.Property(v => v.Notes).HasColumnName("Notes");
                eb.Property(v => v.PatientName).HasColumnName("patientName");
                eb.Property(v => v.DOB).HasColumnName("dob");
                eb.Property(v => v.Gender).HasColumnName("gender");
                eb.Property(v => v.Contact).HasColumnName("contact");
                eb.Property(v => v.MedicalHistory).HasColumnName("medicalHistory");
                eb.Property(v => v.Allergies).HasColumnName("allergies");
                eb.Property(v => v.CurrentMedications).HasColumnName("currentMedications");
            });

            // ── UpcomingAppointmentsDTO (no key, view) ─────────────────────────────
            modelBuilder.Entity<UpcomingAppointmentDTO>(eb =>
            {
                eb.HasNoKey().ToView(null);
                eb.Property(v => v.Date).HasColumnName("Date");
                eb.Property(v => v.Time).HasColumnName("Time");
                eb.Property(v => v.Doctor).HasColumnName("Doctor");
                eb.Property(v => v.Reason).HasColumnName("Reason");
                eb.Property(v => v.Status).HasColumnName("Status");
            });
        
            // ── MedicalRecord ────────────────────────────────────────────────────
            modelBuilder.Entity<MedicalRecord>(entity =>
            {
                entity.ToTable("MedicalRecords");
                entity.HasKey(e => e.RecordID).HasName("PK_MedicalRecords");
                entity.Property(e => e.Diagnosis).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Treatment).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Medications).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.RecordDate).HasColumnType("datetime2");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.ModifiedAt).HasDefaultValueSql("GETUTCDATE()");
        
                entity.HasOne(e => e.Patient)
                      .WithMany(p => p.MedicalRecords)
                      .HasForeignKey(e => e.PatientID)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_MedicalRecords_Patient");
        
                entity.HasOne(e => e.Doctor)
                      .WithMany(d => d.MedicalRecords)
                      .HasForeignKey(e => e.DoctorID)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_MedicalRecords_Doctor");
            });
        
            // ── Prescription ─────────────────────────────────────────────────────
            modelBuilder.Entity<Prescription>(entity =>
            {
                entity.ToTable("Prescriptions");
                entity.HasKey(e => e.PrescriptionID).HasName("PK_Prescriptions");
                entity.Property(e => e.MedicationName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Dosage).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Frequency).HasMaxLength(100).IsRequired();
                entity.Property(e => e.StartDate).HasColumnType("datetime2");
                entity.Property(e => e.EndDate).HasColumnType("datetime2");
                entity.Property(e => e.Status)
                      .HasMaxLength(50)
                      .HasDefaultValue("Active");
                entity.Property(e => e.Instructions).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.ModifiedAt).HasDefaultValueSql("GETUTCDATE()");
        
                entity.HasOne(e => e.Patient)
                      .WithMany(p => p.Prescriptions)
                      .HasForeignKey(e => e.PatientID)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Prescriptions_Patient");
        
                entity.HasOne(e => e.Doctor)
                      .WithMany(d => d.Prescriptions)
                      .HasForeignKey(e => e.DoctorID)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_Prescriptions_Doctor");
            });
        
            // ── LabResult ────────────────────────────────────────────────────────
            modelBuilder.Entity<LabResult>(entity =>
            {
                entity.ToTable("LabResults");
                entity.HasKey(e => e.LabResultID).HasName("PK_LabResults");
                entity.Property(e => e.TestName).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Result).HasMaxLength(255).IsRequired();
                entity.Property(e => e.NormalRange).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
                entity.Property(e => e.TestDate).HasColumnType("datetime2");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.ModifiedAt).HasDefaultValueSql("GETUTCDATE()");
        
                entity.HasOne(e => e.Patient)
                      .WithMany(p => p.LabResults)
                      .HasForeignKey(e => e.PatientID)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_LabResults_Patient");
        
                entity.HasOne(e => e.Doctor)
                      .WithMany(d => d.LabResults)
                      .HasForeignKey(e => e.DoctorID)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("FK_LabResults_Doctor");
            });
        
            // ── User → Patient / Doctor relationships ───────────────────────────
            modelBuilder.Entity<User>()
                .HasMany(u => u.Patients)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        
            modelBuilder.Entity<User>()
                .HasMany(u => u.Doctors)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        
            // ── Automatic timestamp on User, Patient, Doctor, Appointment ───────
            modelBuilder.Entity<User>()
                .Property(e => e.ModifiedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        
            modelBuilder.Entity<Patient>()
                .Property(e => e.ModifiedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        
            modelBuilder.Entity<Doctor>()
                .Property(e => e.ModifiedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        
            modelBuilder.Entity<Appointment>()
                .Property(e => e.ModifiedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        
            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        // PRESERVED: Override SaveChanges to handle timestamp updates
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var utcNow = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = utcNow;
                }

                entry.Property("ModifiedAt").CurrentValue = utcNow;
            }
        }
    }
}
