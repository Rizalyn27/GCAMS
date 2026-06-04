using Microsoft.EntityFrameworkCore;
using GCAMS.Models.Students;
using GCAMS.Models.Counselor;


namespace GCAMS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        //Student
        public DbSet<Students> Students { get; set; }
        public DbSet<HealthInformation> HealthInformations { get; set; }
        public DbSet<FamilyBackground> FamilyBackgrounds { get; set; }
        public DbSet<EmergencyContact> EmergencyContacts { get; set; }
        public DbSet<EducationalBackground> EducationalBackgrounds { get; set; }


        //Counselor
        public DbSet<Counselor> Counselors { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configure one-to-one relationships

            //Casscade if StudentsID is deleted,
            //the related records in FamilyBackground,
            //EmergencyContact, EducationalBackground,
            //and HealthInformation will also be deleted.
            
            
            //FamilyBackground
            modelBuilder.Entity<Students>()
                .HasOne(s => s.FamilyBackground)
                .WithOne(fb => fb.Student)
                .HasForeignKey<FamilyBackground>(fb => fb.StudentsID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade); 


            //EmergencyContact
            modelBuilder.Entity<Students>()
                .HasOne(s => s.EmergencyContact)
                .WithOne(ec => ec.Student)
                .HasForeignKey<EmergencyContact>(ec => ec.StudentsID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade); 

            //EducationalBackground
            modelBuilder.Entity<Students>()
                .HasOne(s => s.EducationalBackground)
                .WithOne(eb => eb.Student)
                .HasForeignKey<EducationalBackground>(eb => eb.StudentsID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            //HealthInformation
            modelBuilder.Entity<Students>()
                .HasOne(s => s.HealthInformation)
                .WithOne(hi => hi.Student)
                .HasForeignKey<HealthInformation>(hi => hi.StudentsID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
    