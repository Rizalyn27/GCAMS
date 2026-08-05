using Microsoft.EntityFrameworkCore;
using GCAMS.Models.Students;
using GCAMS.Models.Counselor;
using GCAMS.Models.Appointment;
using GCAMS.Models.CaseNotes;
using GCAMS.Models.Users;
using GCAMS.Models.AnecRecs;
using GCAMS.Models.Notifs;


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

        //Student Contact Numbers
        public DbSet<StudentContactNumber> StudentContactNumbers { get; set; }
        public DbSet<FamilyContactNumber> FamilyContactNumbers { get; set; }
        public DbSet<EmergencyContactNumber> EmergencyContactNumbers { get; set; }


        //Counselor
        public DbSet<Counselor> Counselors { get; set; }
        public DbSet<CounselorContactNumber> CounselorContactNumbers { get; set; }

        //Appointment
        public DbSet<Appointments> Appointments { get; set; }

        //CaseNotes
        public DbSet<CaseNotes> CaseNotes { get; set; }

        //Account
        public DbSet<Users> Users { get; set; }

        //Anecdotal Records
        public DbSet<AnecRecs> AnecRecs { get; set; }

        //Notifications
        public DbSet<Notifs> Notifs { get; set; }


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

            //CounselorContactNumber
            modelBuilder.Entity<CounselorContactNumber>()
                .HasOne(c => c.Counselor)
                .WithMany(c => c.ContactNumbers)
                .HasForeignKey(c => c.CounselorID)
                .OnDelete(DeleteBehavior.Cascade);

            // Appointments
            modelBuilder.Entity<Appointments>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentsID)
                .OnDelete(DeleteBehavior.Cascade);  

            // StudentContactNumber
            modelBuilder.Entity<StudentContactNumber>()
                .HasOne(scn => scn.Student)
                .WithMany(s => s.ContactNumbers)
                .HasForeignKey(scn => scn.StudentsID)
                .OnDelete(DeleteBehavior.Cascade);

            // AnecRecs
            modelBuilder.Entity<AnecRecs>()
                .HasOne(ar => ar.Student)
                .WithMany()
                .HasForeignKey(ar => ar.StudentsID)
                .OnDelete(DeleteBehavior.Cascade);


            // CaseNotes
            modelBuilder.Entity<CaseNotes>()
                .HasOne(cn => cn.Student)
                .WithMany()
                .HasForeignKey(cn => cn.StudentsID)
                .OnDelete(DeleteBehavior.Cascade);

            // FamilyContactNumber / EmergencyContactNumber
            modelBuilder.Entity<FamilyContactNumber>()
                .HasOne(fcn => fcn.FamilyBackground)
                .WithMany(fb => fb.ContactNumbers)
                .HasForeignKey(fcn => fcn.FamilyBackgroundID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmergencyContactNumber>()
                .HasOne(ecn => ecn.EmergencyContact)
                .WithMany(ec => ec.ContactNumbers)
                .HasForeignKey(ecn => ecn.EmergencyContactID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
    