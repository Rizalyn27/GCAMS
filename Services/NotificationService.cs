using GCAMS.Controllers;
using GCAMS.Data;
using GCAMS.Models.Notifs;
using GCAMS.Models.Students;
using Microsoft.EntityFrameworkCore;

namespace GCAMS.Services
{

    public class NotificationService
    {

        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task GenerateDueNotificationsAsync()
        {
            await GenerateAppointmentRemindersAsync();
            await GenerateFollowUpRemindersAsync();
            await AppointmentStatusUpdateAsync();
            await GenerateSameDayPopupAsync();
        }

        private async Task GenerateAppointmentRemindersAsync()
        {
            var windowStart = DateTime.Now;
            var windowEnd = DateTime.Now.AddHours(24);

            var upcoming = await _context.Appointments
                .Where(a => a.AppointmentDate >= windowStart && a.AppointmentDate <= windowEnd)
                .ToListAsync();

            foreach (var appt in upcoming)
            {
                // Notify the student — only if this appointment is actually linked to one
                if (appt.StudentsID.HasValue && appt.Status == "Confirmed")
                {
                    var student = await _context.Students.FindAsync(appt.StudentsID.Value);
                    if (student != null)
                    {
                        bool alreadyNotified = await _context.Notifs.AnyAsync(n =>
                            n.RelatedEntityType == "Appointment" &&
                            n.RelatedEntityId == appt.AppointmentID &&
                            n.RecipientUsername == student.StuID &&
                            n.Type == NotificationType.AppointmentReminder);

                        if (!alreadyNotified)
                        {
                            _context.Notifs.Add(new Notifs
                            {
                                RecipientUsername = student.StuID,
                                Type = NotificationType.AppointmentReminder,
                                Title = "Upcoming Appointment",
                                Message = $"You have an appointment on {appt.AppointmentDate:MMM dd, yyyy - h:mm tt}.",
                                RelatedEntityType = "Appointment",
                                RelatedEntityId = appt.AppointmentID
                            });
                        }
                    }
                }

                // Notify the counselor — only if one has claimed this appointment
                if (appt.CounselorID.HasValue)
                {
                    var counselor = await _context.Counselors.FindAsync(appt.CounselorID.Value);
                    if (counselor != null)
                    {
                        bool alreadyNotified = await _context.Notifs.AnyAsync(n =>
                            n.RelatedEntityType == "Appointment" &&
                            n.RelatedEntityId == appt.AppointmentID &&
                            n.RecipientUsername == counselor.EmailAddress &&
                            n.Type == NotificationType.AppointmentReminder);

                        if (!alreadyNotified)
                        {
                            _context.Notifs.Add(new Notifs
                            {
                                RecipientUsername = counselor.EmailAddress,
                                Type = NotificationType.AppointmentReminder,
                                Title = "Upcoming Appointment",
                                Message = $"You have a session with {appt.FullName} on {appt.AppointmentDate:MMM dd, yyyy - h:mm tt}.",
                                RelatedEntityType = "Appointment",
                                RelatedEntityId = appt.AppointmentID
                            });
                        }
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {

            }
        }
        private async Task GenerateFollowUpRemindersAsync()
        {
            var cutoff = DateTime.Now.AddDays(-14);

            var staleNotes = await _context.CaseNotes
                .Where(n => n.SessionDate <= cutoff)
                .GroupBy(n => n.StudentsID)
                .Select(g => g.OrderByDescending(n => n.SessionDate).First())
                .ToListAsync();

            foreach (var note in staleNotes)
            {
                var student = await _context.Students.FindAsync(note.StudentsID);
                if (student == null) continue;

                if (note.CounselorID == null) continue;
                var counselor = await _context.Counselors.FindAsync(note.CounselorID.Value);
                if (counselor == null) continue;

                bool alreadyNotified = await _context.Notifs.AnyAsync(n =>
                    n.RelatedEntityType == "CaseNotes" &&
                    n.RelatedEntityId == note.CasenoteId &&
                    n.RecipientUsername == counselor.EmailAddress &&
                    n.Type == NotificationType.FollowUp);

                if (!alreadyNotified)
                {
                    _context.Notifs.Add(new Notifs
                    {
                        RecipientUsername = counselor.EmailAddress, // Username == EmailAddress per EnsureCounselorAccountAsync
                        Type = NotificationType.FollowUp,
                        Title = "Follow-Up Due",
                        Message = $"{student.StuName} may need a follow-up session (last seen {note.SessionDate:MMM dd, yyyy}).",
                        RelatedEntityType = "CaseNotes",
                        RelatedEntityId = note.CasenoteId
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {

            }
        }

        // Catches appointments that passed their scheduled time while still Pending/Confirmed
        // (i.e. nobody marked them Completed/Cancelled/Missed) and flags them as Missed.
        private async Task AppointmentStatusUpdateAsync()
        {
            var now = DateTime.Now;
            var pastAppointments = await _context.Appointments
                .Where(a => a.AppointmentDate < now && (a.Status == "Pending" || a.Status == "Confirmed"))
                .ToListAsync();

            foreach (var appt in pastAppointments)
            {
                if (!appt.StudentsID.HasValue) continue;

                var student = await _context.Students.FindAsync(appt.StudentsID.Value);
                if (student == null) continue;

                bool alreadyNotified = await _context.Notifs.AnyAsync(n =>
                    n.RelatedEntityType == "Appointment" &&
                    n.RelatedEntityId == appt.AppointmentID &&
                    n.RecipientUsername == student.StuID &&
                    n.Type == NotificationType.StatusUpdate);

                if (alreadyNotified) continue;

                appt.Status = "Missed";
                appt.UpdatedAt = now;

                _context.Notifs.Add(new Notifs
                {
                    RecipientUsername = student.StuID,
                    Type = NotificationType.StatusUpdate,
                    Title = "Appointment Missed",
                    Message = $"You missed your appointment scheduled on {appt.AppointmentDate:MMM dd, yyyy - h:mm tt}.",
                    RelatedEntityType = "Appointment",
                    RelatedEntityId = appt.AppointmentID
                });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {

            }
        }


        private async Task GenerateSameDayPopupAsync()
        {
            var today = DateTime.Today;

            var todaysAppointments = await _context.Appointments
                .Where(a => a.AppointmentDate.Date == today && a.Status == "Confirmed")
                .ToListAsync();

            foreach (var appt in todaysAppointments)
            {
                if (!appt.StudentsID.HasValue) continue;

                var student = await _context.Students.FindAsync(appt.StudentsID.Value);
                if (student == null) continue;

                bool alreadyNotified = await _context.Notifs.AnyAsync(n =>
                    n.RelatedEntityType == "Appointment" &&
                    n.RelatedEntityId == appt.AppointmentID &&
                    n.RecipientUsername == student.StuID &&
                    n.Type == NotificationType.SameDayAppointment);

                if (!alreadyNotified)
                {
                    _context.Notifs.Add(new Notifs
                    {
                        RecipientUsername = student.StuID,
                        Type = NotificationType.SameDayAppointment,
                        Title = "Appointment Today",
                        Message = $"You have an appointment today at {appt.AppointmentDate:h:mm tt}.",
                        RelatedEntityType = "Appointment",
                        RelatedEntityId = appt.AppointmentID
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {

            }
        }


    }

}