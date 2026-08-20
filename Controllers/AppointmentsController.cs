using GCAMS.Data;
using GCAMS.Models.Appointment;
using GCAMS.Models.Notifs;
using GCAMS.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GCAMS.Models.ActivityLogs;

// must be logged in for anything in this controller
[Authorize]
public class AppointmentsController : Controller
{
    private readonly AppDbContext _context;

    private static readonly int[] AllowedHours = { 8, 9, 10, 11, 13, 14, 15, 16 };

    private static bool IsWithinWorkingHours(DateTime dt) => AllowedHours.Contains(dt.Hour);

    public AppointmentsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Appointments — now a calendar, matching Announcements
    [Authorize(Roles = "Admin,Counselor")]
    public async Task<IActionResult> Index(int? year, int? month, int? day)
    {
        var today = DateTime.Today;
        int y = year ?? today.Year;
        int m = month ?? today.Month;

        var firstOfMonth = new DateTime(y, m, 1);
        var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

        var monthAppointments = await _context.Appointments
            .Include(a => a.Counselor)
            .Where(a => a.AppointmentDate.Date >= firstOfMonth && a.AppointmentDate.Date <= lastOfMonth)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

        var countsByDay = monthAppointments
            .GroupBy(a => a.AppointmentDate.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var gridStart = firstOfMonth.AddDays(-(int)firstOfMonth.DayOfWeek);
        var gridEnd = lastOfMonth.AddDays(6 - (int)lastOfMonth.DayOfWeek);

        var days = new List<CalendarDay>();
        for (var d = gridStart; d <= gridEnd; d = d.AddDays(1))
        {
            days.Add(new CalendarDay
            {
                Date = d,
                IsCurrentMonth = d.Month == m,
                IsToday = d == today,
                Count = countsByDay.TryGetValue(d, out var c) ? c : 0
            });
        }

        List<Appointments> selectedDayAppointments = new();
        DateTime? selectedDate = null;

        if (day.HasValue)
        {
            selectedDate = new DateTime(y, m, day.Value);
            selectedDayAppointments = monthAppointments
                .Where(a => a.AppointmentDate.Date == selectedDate.Value)
                .OrderBy(a => a.AppointmentDate)
                .ToList();
        }

        var vm = new AppointmentCalendarViewModel
        {
            Year = y,
            Month = m,
            MonthName = firstOfMonth.ToString("MMMM yyyy"),
            Days = days,
            SelectedDate = selectedDate,
            SelectedDayAppointments = selectedDayAppointments
        };

        return View(vm);
    }

    // Anyone logged in can view Details — but a Student can only view their OWN appointment
    public async Task<IActionResult> Details(int? appointmentid)
    {
        if (appointmentid == null) return NotFound();

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(m => m.AppointmentID == appointmentid);
        if (appointment == null) return NotFound();

        if (User.IsInRole("Student"))
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StuID == User.Identity!.Name);

            if (student == null || appointment.StudentsID != student.StudentsID)
                return Forbid();
        }

        return View(appointment);
    }

    // Anyone logged in can book — Student, Counselor, or Admin
    public async Task<IActionResult> Create(bool force = false)
    {
        if (User.IsInRole("Student"))
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StuID == User.Identity!.Name);

            if (student == null) return Forbid();

            if (!force)
            {
                var pending = await _context.Appointments
                    .Where(a => a.StudentsID == student.StudentsID && a.Status == "Pending")
                    .OrderByDescending(a => a.AppointmentDate)
                    .FirstOrDefaultAsync();

                if (pending != null)
                {
                    TempData["Info"] = "You already have a pending appointment. You can view it in MyAppointments, or book another one if needed.";
                    return RedirectToAction(nameof(Details), new { appointmentid = pending.AppointmentID });
                }
            }

            ViewBag.IsStudentBooking = true;
            ViewBag.GradeLevel = student.GradeLevel;
            ViewBag.Section = student.Section;

            return View(new Appointments
            {
                StudentsID = student.StudentsID,
                FullName = student.StuName,
                Email = student.Email ?? ""
            });
        }

        ViewBag.IsStudentBooking = false;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FullName,Email,ContactNumber,AppointmentDate,AppointmentType,Notes,StudentsID")] Appointments appointments)
    {
        GCAMS.Models.Students.Students? student = null;

        if (User.IsInRole("Student"))
        {
            student = await _context.Students
                .FirstOrDefaultAsync(s => s.StuID == User.Identity!.Name);

            if (student == null) return Forbid();

            appointments.StudentsID = student.StudentsID;
            appointments.FullName = student.StuName;
            appointments.Email = student.Email ?? "";
        }

        ViewBag.IsStudentBooking = student != null;
        ViewBag.GradeLevel = student?.GradeLevel;
        ViewBag.Section = student?.Section;

        ModelState.Remove("Status");
        ModelState.Remove("CreatedAt");
        ModelState.Remove("UpdatedAt");

        appointments.AppointmentDate = appointments.AppointmentDate.Date
            .AddHours(appointments.AppointmentDate.Hour);

        if (!IsWithinWorkingHours(appointments.AppointmentDate))
        {
            ModelState.AddModelError("AppointmentDate", "Appointments must be booked between 8–11 AM or 1–4 PM.");
            return View(appointments);
        }

        appointments.Status = "Pending";
        appointments.CreatedAt = DateTime.Now;
        appointments.UpdatedAt = null;

        if (!User.IsInRole("Student"))
            appointments.CounselorID = await GetCurrentCounselorIdAsync();

        var hasConflict = await _context.Appointments.AnyAsync(a =>
            a.StudentsID == appointments.StudentsID &&
            a.AppointmentDate.Date == appointments.AppointmentDate.Date &&
            a.Status != "Cancelled" &&
            a.Status != "Missed");

        if (hasConflict)
        {
            ModelState.AddModelError("", "This student already has an appointment scheduled on this date.");
            return View(appointments);
        }

        // NEW — the exact hour slot on this day is already taken by someone else
        var slotTaken = await _context.Appointments.AnyAsync(a =>
            a.AppointmentDate == appointments.AppointmentDate &&
            a.Status != "Cancelled" &&
            a.Status != "Missed");

        if (slotTaken)
        {
            ModelState.AddModelError("AppointmentDate", "This time slot is already booked. Please choose a different hour.");
            return View(appointments);
        }

        if (ModelState.IsValid)
        {
            _context.Add(appointments);
            await _context.SaveChangesAsync();

            // Activity Log
            _context.ActivityLogs.Add(new ActivityLog
            {
                Who = User.Identity?.Name ?? "Unknown",
                Date = DateTime.Now,
                ActivityAction = ActivityAction.BookAppointment.ToString(),
                Details = $"Appointment booked for {appointments.FullName} ({appointments.AppointmentType}) on {appointments.AppointmentDate:MMM dd, yyyy - h:mm tt}."
            });
            await _context.SaveChangesAsync();

            // Dedup by EmailAddress + save one-at-a-time, same fix as Announcements —
            // one collision no longer takes the whole batch (and every other counselor's
            // notification) down with it.
            var counselors = await _context.Counselors
                .GroupBy(c => c.EmailAddress)
                .Select(g => g.First())
                .ToListAsync();

            foreach (var c in counselors)
            {
                _context.Notifs.Add(new Notifs
                {
                    RecipientUsername = c.EmailAddress,
                    Type = NotificationType.NewAppointment,
                    Title = "New Appointment Request",
                    Message = $"A new appointment has been requested by {appointments.FullName} for {appointments.AppointmentDate:MMM dd, yyyy - h:mm tt}.",
                    RelatedEntityType = "Appointment",
                    RelatedEntityId = appointments.AppointmentID
                });

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    _context.ChangeTracker.Clear();
                }
            }

            if (User.IsInRole("Student"))
                return RedirectToAction(nameof(Details), new { appointmentid = appointments.AppointmentID });

            return RedirectToAction(nameof(Index));
        }

        return View(appointments);
    }

    public async Task<IActionResult> Edit(int? appointmentid)
    {
        if (appointmentid == null) return NotFound();

        var appointment = await _context.Appointments.FindAsync(appointmentid);
        if (appointment == null) return NotFound();

        if (User.IsInRole("Student"))
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StuID == User.Identity!.Name);

            if (student == null || appointment.StudentsID != student.StudentsID)
                return Forbid();

            if (appointment.Status != "Pending" && appointment.Status != "Confirmed")
            {
                TempData["Error"] = "This appointment can no longer be edited.";
                return RedirectToAction(nameof(MyAppointments));
            }
        }

        ViewBag.IsStudentEdit = User.IsInRole("Student");
        return View(appointment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int appointmentid,
        [Bind("AppointmentID,FullName,Email,AppointmentDate,AppointmentType,Notes,StudentsID")] Appointments posted)
    {
        if (appointmentid != posted.AppointmentID) return NotFound();

        var existing = await _context.Appointments.FindAsync(appointmentid);
        if (existing == null) return NotFound();

        bool isStudent = User.IsInRole("Student");

        if (isStudent)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StuID == User.Identity!.Name);

            if (student == null || existing.StudentsID != student.StudentsID)
                return Forbid();

            if (existing.Status != "Pending" && existing.Status != "Confirmed")
                return Forbid();

            existing.AppointmentDate = posted.AppointmentDate;
            existing.AppointmentType = posted.AppointmentType;
            existing.Notes = posted.Notes;
        }
        else
        {
            existing.FullName = posted.FullName;
            existing.Email = posted.Email;
            existing.AppointmentDate = posted.AppointmentDate;
            existing.AppointmentType = posted.AppointmentType;
            existing.Notes = posted.Notes;

            existing.CounselorID ??= await GetCurrentCounselorIdAsync();
        }

        existing.AppointmentDate = existing.AppointmentDate.Date
            .AddHours(existing.AppointmentDate.Hour);

        if (!IsWithinWorkingHours(existing.AppointmentDate))
        {
            ModelState.AddModelError("AppointmentDate", "Appointments must be booked between 8–11 AM or 1–4 PM.");
            ViewBag.IsStudentEdit = isStudent;
            return View(posted);
        }

        existing.UpdatedAt = DateTime.Now;

        if (!ModelState.IsValid)
        {
            ViewBag.IsStudentEdit = isStudent;
            return View(posted);
        }


        // Activity Log
        _context.ActivityLogs.Add(new ActivityLog
        {
            Who = User.Identity?.Name ?? "Unknown",
            Date = DateTime.Now,
            ActivityAction = ActivityAction.RescheduleAppointment.ToString(),
            Details = $"Appointment #{existing.AppointmentID} for {existing.FullName} was rescheduled to {existing.AppointmentDate:MMM dd, yyyy - h:mm tt}."
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AppointmentsExists(appointmentid)) return NotFound();
            throw;
        }

        return RedirectToAction(isStudent ? nameof(MyAppointments) : nameof(Index));
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Delete(int? appointmentid)
    {
        if (appointmentid == null) return NotFound();

        var appointments = await _context.Appointments
            .FirstOrDefaultAsync(m => m.AppointmentID == appointmentid);
        if (appointments == null) return NotFound();

        if (User.IsInRole("Student"))
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StuID == User.Identity!.Name);

            if (student == null || appointments.StudentsID != student.StudentsID)
                return Forbid();

            if (appointments.Status != "Pending" && appointments.Status != "Confirmed")
            {
                TempData["Error"] = "This appointment can no longer be cancelled.";
                return RedirectToAction(nameof(MyAppointments));
            }
        }

        return View(appointments);
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Student")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? appointmentid)
    {
        var appointments = await _context.Appointments.FindAsync(appointmentid);
        if (appointments == null) return NotFound();

        if (User.IsInRole("Student"))
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StuID == User.Identity!.Name);

            if (student == null || appointments.StudentsID != student.StudentsID)
                return Forbid();

            if (appointments.Status != "Pending" && appointments.Status != "Confirmed")
            {
                TempData["Error"] = "This appointment can no longer be cancelled.";
                return RedirectToAction(nameof(MyAppointments));
            }

            appointments.Status = "Cancelled";
            appointments.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Activity Log
            _context.ActivityLogs.Add(new ActivityLog
            {
                Who = User.Identity?.Name ?? "Unknown",
                Date = DateTime.Now,
                ActivityAction = ActivityAction.CancelAppointment.ToString(),
                Details = $"Appointment #{appointments.AppointmentID} for {appointments.FullName} was cancelled."
            });
            await _context.SaveChangesAsync();

            if (appointments.CounselorID.HasValue)
            {
                var counselor = await _context.Counselors.FindAsync(appointments.CounselorID.Value);
                if (counselor != null)
                {
                    _context.Notifs.Add(new Notifs
                    {
                        RecipientUsername = counselor.EmailAddress,
                        Type = NotificationType.StatusUpdate,
                        Title = "Appointment Cancelled",
                        Message = $"{appointments.FullName} cancelled their appointment scheduled on {appointments.AppointmentDate:MMM dd, yyyy - h:mm tt}.",
                        RelatedEntityType = "Appointment",
                        RelatedEntityId = appointments.AppointmentID
                    });

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateException)
                    {
                        _context.ChangeTracker.Clear();
                    }
                }
            }
            else
            {
                var counselors = await _context.Counselors
                    .GroupBy(c => c.EmailAddress)
                    .Select(g => g.First())
                    .ToListAsync();

                foreach (var c in counselors)
                {
                    _context.Notifs.Add(new Notifs
                    {
                        RecipientUsername = c.EmailAddress,
                        Type = NotificationType.StatusUpdate,
                        Title = "Appointment Cancelled",
                        Message = $"{appointments.FullName} cancelled their appointment scheduled on {appointments.AppointmentDate:MMM dd, yyyy - h:mm tt}.",
                        RelatedEntityType = "Appointment",
                        RelatedEntityId = appointments.AppointmentID
                    });

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateException)
                    {
                        _context.ChangeTracker.Clear();
                    }
                }
            }

            TempData["Info"] = "Your appointment has been cancelled.";
            return RedirectToAction(nameof(MyAppointments));
        }

        _context.Appointments.Remove(appointments);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool AppointmentsExists(int? appointmentid)
    {
        return _context.Appointments.Any(e => e.AppointmentID == appointmentid);
    }

    [Authorize(Roles = "Admin,Counselor")]
    [HttpGet]
    public async Task<IActionResult> GetStudentByStuID(string? stuId)
    {
        if (string.IsNullOrWhiteSpace(stuId))
            return BadRequest("Student ID is required.");

        var student = await _context.Students.FirstOrDefaultAsync(s => s.StuID == stuId.Trim());
        if (student == null)
            return NotFound($"Student with ID '{stuId}' not found.");

        return Json(new
        {
            studentsID = student.StudentsID,
            fullName = student.StuName,
            email = student.Email ?? "",
            gradeLevel = student.GradeLevel,
            section = student.Section
        });
    }

    [Authorize(Roles = "Admin,Counselor")]
    [HttpPost]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
    {
        var appointment = await _context.Appointments.FindAsync(request.AppointmentId);
        if (appointment == null) return NotFound();

        var validStatuses = new[] { "Pending", "Confirmed", "Completed", "Missed", "Cancelled" };
        if (!validStatuses.Contains(request.NewStatus))
            return BadRequest("Invalid status value.");

        appointment.CounselorID ??= await GetCurrentCounselorIdAsync();
        appointment.Status = request.NewStatus;
        appointment.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        if (appointment.StudentsID.HasValue)
        {
            var student = await _context.Students.FindAsync(appointment.StudentsID.Value);
            if (student != null)
            {
                var (title, message) = request.NewStatus switch
                {
                    "Confirmed" => ("Appointment Confirmed", $"Your appointment on {appointment.AppointmentDate:MMM dd, yyyy - h:mm tt} has been confirmed."),
                    "Completed" => ("Appointment Completed", $"Your appointment on {appointment.AppointmentDate:MMM dd, yyyy - h:mm tt} is marked completed."),
                    "Missed" => ("Appointment Missed", $"You missed your appointment scheduled on {appointment.AppointmentDate:MMM dd, yyyy - h:mm tt}."),
                    _ => (null as string, null as string)
                };

                if (title != null)
                {
                    _context.Notifs.Add(new Notifs
                    {
                        RecipientUsername = student.StuID,
                        Type = NotificationType.StatusUpdate,
                        Title = title,
                        Message = message!,
                        RelatedEntityType = "Appointment",
                        RelatedEntityId = appointment.AppointmentID
                    });

                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateException)
                    {
                        _context.ChangeTracker.Clear();
                    }
                }
            }
        }

        return Ok();
    }

    public async Task<IActionResult> MyAppointments()
    {
        if (!User.IsInRole("Student")) return Forbid();

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StuID == User.Identity!.Name);

        if (student == null) return NotFound();

        var appointments = await _context.Appointments
            .Where(a => a.StudentsID == student.StudentsID)
            .OrderBy(a => (a.Status == "Cancelled" || a.Status == "Rejected") ? 1 : 0)
            .ThenByDescending(a => a.AppointmentDate)
            .ToListAsync();

        return View(appointments);
    }

    private async Task<int?> GetCurrentCounselorIdAsync()
    {
        var email = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email)) return null;

        return await _context.Counselors
            .Where(c => c.EmailAddress == email)
            .Select(c => (int?)c.CounselorID)
            .FirstOrDefaultAsync();
    }

    public class UpdateStatusRequest
    {
        public int AppointmentId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
    }
}