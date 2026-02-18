using BeautySalonProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BeautySalonProject.Data
{
    public class ApplicationDbContext
         : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Inquiry> Inquiries { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<ServiceVariant> ServiceVariants { get; set; }
		public DbSet<EmployeeWorkDay> EmployeeWorkDays { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasIndex(e => new { e.EmployeeId, e.StartAt }, "IX_Appointments_Employee_StartAt");

                entity.Property(e => e.ClientUserId).HasMaxLength(450);
                entity.Property(e => e.CreatedAt)
                    .HasPrecision(0)
                    .HasDefaultValueSql("(sysdatetime())");
                entity.Property(e => e.EndAt).HasPrecision(0);
                entity.Property(e => e.FinalPrice).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.GuestEmail).HasMaxLength(120);
                entity.Property(e => e.GuestFullName).HasMaxLength(120);
                entity.Property(e => e.GuestPhone).HasMaxLength(30);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.StartAt).HasPrecision(0);
                entity.Property(e => e.UpdatedAt).HasPrecision(0);

                entity.HasOne(d => d.Employee).WithMany(p => p.Appointments)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Appointments_Employees");

                entity.HasOne(d => d.Inquiry).WithMany(p => p.Appointments)
                    .HasForeignKey(d => d.InquiryId)
                    .HasConstraintName("FK_Appointment_Inquiry");

                entity.HasOne(d => d.Variant).WithMany(p => p.Appointments)
                    .HasForeignKey(d => d.VariantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Appointments_Variants");
            });


            modelBuilder.Entity<Employee>(entity =>
            {
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.FirstName).HasMaxLength(60);
                entity.Property(e => e.IdentityUserId).HasMaxLength(450);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.LastName).HasMaxLength(60);
                entity.Property(e => e.Phone).HasMaxLength(30);
                entity.HasOne(e => e.PrimaryCategory)
                      .WithMany()
                      .HasForeignKey(e => e.PrimaryCategoryId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Inquiry>(entity =>
            {
                entity.Property(e => e.AdminNote).HasMaxLength(500);
                entity.Property(e => e.CreatedAt)
                    .HasPrecision(0)
                    .HasDefaultValueSql("(sysdatetime())");
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.FullName).HasMaxLength(120);
                entity.Property(e => e.Message).HasMaxLength(1000);
                entity.Property(e => e.Phone).HasMaxLength(30);
                entity.Property(e => e.PreferredDateTime).HasPrecision(0);
                entity.Property(e => e.ProcessedAt).HasColumnType("datetime");
                entity.Property(e => e.ServiceText).HasMaxLength(200);

                entity.HasOne(d => d.ServiceVariant).WithMany(p => p.Inquiries)
                    .HasForeignKey(d => d.ServiceVariantId)
                    .HasConstraintName("FK_Inquiry_ServiceVariant");
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Name).HasMaxLength(120);

                entity.HasOne(d => d.Category).WithMany(p => p.Services)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Services_Categories");

                entity.HasOne(d => d.Employee).WithMany(p => p.Services)
                    .HasForeignKey(d => d.EmployeeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Services_Employees");
            });

            modelBuilder.Entity<ServiceCategory>(entity =>
            {
                entity.HasKey(e => e.CategoryId);

                entity.HasIndex(e => e.Name, "UQ_ServiceCategories_Name").IsUnique();

                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<ServiceVariant>(entity =>
            {
                entity.HasKey(e => e.VariantId);

                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.VariantName).HasMaxLength(120);

                entity.HasOne(d => d.Service).WithMany(p => p.ServiceVariants)
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ServiceVariants_Services");
            });

			modelBuilder.Entity<EmployeeWorkDay>(entity =>
			{
				entity.HasKey(x => x.EmployeeWorkDayId);

				entity.HasIndex(x => new { x.EmployeeId, x.Date })
					  .IsUnique();

				entity.Property(x => x.IsWorking).HasDefaultValue(true);

				entity.HasOne(x => x.Employee)
					  .WithMany(e => e.WorkDays)
					  .HasForeignKey(x => x.EmployeeId)
					  .OnDelete(DeleteBehavior.Cascade);
			});
		}

    }
}
