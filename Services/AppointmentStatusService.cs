using GCAMS.Data;
using Microsoft.EntityFrameworkCore;

namespace GCAMS.Services
{
    // Runs in the background for the lifetime of the app.
    // Periodically scans for appointments whose date has passed
    // while still "Pending" or "Confirmed", and marks them "Missed".
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
                // BackgroundService doesn't get its own scoped DbContext automatically —
                // we have to create one manually each cycle.
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var overdue = await context.Appointments
                        .Where(a => a.AppointmentDate < DateTime.Now &&
                                    (a.Status == "Pending" || a.Status == "Confirmed"))
                        .ToListAsync(stoppingToken);

                    if (overdue.Count > 0)
                    {
                        foreach (var appointment in overdue)
                        {
                            appointment.Status = "Missed";
                            appointment.UpdatedAt = DateTime.Now;
                        }

                        await context.SaveChangesAsync(stoppingToken);
                    }
                }

                await Task.Delay(TimeSpan.FromHours(3), stoppingToken);
            }
        }
    }
}