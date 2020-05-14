namespace fhir_integration
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Model1 : DbContext
    {
        public Model1(string connectionString)
            : base(connectionString)
        {
        }

        public virtual DbSet<BloodPressureMeasurements> BloodPressureMeasurements { get; set; }
        public virtual DbSet<Cities> Cities { get; set; }
        public virtual DbSet<Doctors> Doctors { get; set; }
        public virtual DbSet<Insurances> Insurances { get; set; }
        public virtual DbSet<PasswordResetRequests> PasswordResetRequests { get; set; }
        public virtual DbSet<Patients> Patients { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<AllUserData> AllUserData { get; set; }
        public virtual DbSet<DoctorView> DoctorView { get; set; }
        public virtual DbSet<PatientView> PatientView { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cities>()
                .Property(e => e.countryCode)
                .IsUnicode(false);

            modelBuilder.Entity<Cities>()
                .Property(e => e.zipCode)
                .IsUnicode(false);

            modelBuilder.Entity<Cities>()
                .HasMany(e => e.Doctors)
                .WithRequired(e => e.Cities)
                .HasForeignKey(e => e.workingPlaceCity)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Cities>()
                .HasMany(e => e.Patients)
                .WithRequired(e => e.Cities)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Insurances>()
                .Property(e => e.insuranceCode)
                .IsUnicode(false);

            modelBuilder.Entity<Insurances>()
                .HasMany(e => e.Patients)
                .WithRequired(e => e.Insurances)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Patients>()
                .HasMany(e => e.BloodPressureMeasurements)
                .WithRequired(e => e.Patients)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Users>()
                .Property(e => e.password)
                .IsUnicode(false);

            modelBuilder.Entity<Users>()
                .Property(e => e.token)
                .IsUnicode(false);

            modelBuilder.Entity<Users>()
                .HasOptional(e => e.Doctors)
                .WithRequired(e => e.Users);

            modelBuilder.Entity<Users>()
                .HasMany(e => e.PasswordResetRequests)
                .WithRequired(e => e.Users)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Users>()
                .HasOptional(e => e.Patients)
                .WithRequired(e => e.Users);

            modelBuilder.Entity<AllUserData>()
                .Property(e => e.password)
                .IsUnicode(false);

            modelBuilder.Entity<AllUserData>()
                .Property(e => e.token)
                .IsUnicode(false);

            modelBuilder.Entity<AllUserData>()
                .Property(e => e.countryCode)
                .IsUnicode(false);

            modelBuilder.Entity<AllUserData>()
                .Property(e => e.zipCode)
                .IsUnicode(false);

            modelBuilder.Entity<AllUserData>()
                .Property(e => e.workingPlaceCountry)
                .IsUnicode(false);

            modelBuilder.Entity<AllUserData>()
                .Property(e => e.workingPlaceZipCode)
                .IsUnicode(false);

            modelBuilder.Entity<AllUserData>()
                .Property(e => e.insuranceCode)
                .IsUnicode(false);

            modelBuilder.Entity<DoctorView>()
                .Property(e => e.countryCode)
                .IsUnicode(false);

            modelBuilder.Entity<DoctorView>()
                .Property(e => e.zipCode)
                .IsUnicode(false);

            modelBuilder.Entity<PatientView>()
                .Property(e => e.countryCode)
                .IsUnicode(false);

            modelBuilder.Entity<PatientView>()
                .Property(e => e.zipCode)
                .IsUnicode(false);

            modelBuilder.Entity<PatientView>()
                .Property(e => e.insuranceCode)
                .IsUnicode(false);
        }
    }
}
