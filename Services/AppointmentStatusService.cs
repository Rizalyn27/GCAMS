using GCAMS.Data;
using GCAMS.Models.Notifs;
using Microsoft.EntityFrameworkCore;

namespace GCAMS.Services
{
    public class AppointmentStatusService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public AppointmentStatusService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
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
                            var student = await context.Students.FindAsync(new object[] { appointment.StudentsID.Value }, stoppingToken);
                            if (student != null)
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

                    if (overdue.Count > 0)
                        await context.SaveChangesAsync(stoppingToken);
                }

                await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
            }
        }
    }
}