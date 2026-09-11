using GCAMS.Data;
using GCAMS.Models.Notifs;
using Microsoft.EntityFrameworkCore;

namespace GCAMS.Services
{
    public class AppointmentStatusService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AppointmentStatusService> _logger;

        public AppointmentStatusService(
            IServiceScopeFactory scopeFactory,
            ILogger<AppointmentStatusService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessOverdueAppointmentsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    // Never let this loop take down the whole host. Log it and
                    // try again next cycle.
                    _logger.LogError(ex, "AppointmentStatusService run failed.");
                }

                await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
            }
        }

        private async Task ProcessOverdueAppointmentsAsync(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var overdue = await context.Appointments
                .Where(a => a.AppointmentDate < DateTime.Now &&
                            (a.Status == "Pending" || a.Status == "Confirmed"))
                .ToListAsync(stoppingToken);

            foreach (var appointment in overdue)
            {
                appointment.Status = "Missed";
                appointment.UpdatedAt = DateTime.Now;

                if (appointment.StudentsID.HasValue)
                {
                    var student = await context.Students.FindAsync(
                        new object[] { appointment.StudentsID.Value }, stoppingToken);

                    if (student != null)
                    {
                        // Guard against the unique index: only add if this
                        // notification doesn't already exist.
                        bool alreadyNotified = await context.Notifs.AnyAsync(n =>
                            n.RecipientUsername == student.StuID &&
                            n.Type == NotificationType.StatusUpdate &&
                            n.RelatedEntityType == "Appointment" &&
                            n.RelatedEntityId == appointment.AppointmentID,
                            stoppingToken);

                        if (!alreadyNotified)
                        {
                            context.Notifs.Add(new Notifs
                            {
                                RecipientUsername = student.StuID,
                                Type = NotificationType.StatusUpdate,
                                Title = "Appointment Missed",
                                Message = $"You missed your appointment scheduled on {appointment.AppointmentDate:MMM dd, yyyy - h:mm tt}.",
                                RelatedEntityType = "Appointment",
                                RelatedEntityId = appointment.AppointmentID
                            });
                        }
                    }
                }

                // Save per-appointment instead of one big batch at the end.
                // If one appointment somehow still fails, it doesn't roll back
                // status updates for every other appointment in this run.
                try
                {
                    await context.SaveChangesAsync(stoppingToken);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to update appointment {AppointmentId}, skipping.",
                        appointment.AppointmentID);

                    // Undo the in-memory change tracking for this entity so it
                    // doesn't poison the next SaveChangesAsync call in the loop.
                    context.Entry(appointment).State = EntityState.Unchanged;
                }
            }
        }
    }
}