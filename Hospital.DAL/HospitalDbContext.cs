using Hospital.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hospital.DAL
{
    public class HospitalDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<DoctorGroup> DoctorGroups => Set<DoctorGroup>();
        public DbSet<Appointment> Appointments => Set<Appointment>();

        public HospitalDbContext(DbContextOptions<HospitalDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Role
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Doctor
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.Group)
                .WithMany(g => g.Doctors)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Composite unique: Doctor, Date, Time
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.DoctorId, a.Date, a.Time })
                .IsUnique();

            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Administrator" },
                new Role { Id = 2, Name = "Manager" },
                new Role { Id = 3, Name = "Patient" }
            );
        }
    }
}