using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GCAMS.Models.Appointment;
using GCAMS.Data;

// must be logged in for anything in this controller

//[Authorize]
public class AppointmentsController : Controller
{
    private readonly AppDbContext _context;

    public AppointmentsController(AppDbContext context)
    {
        _context = context;
    }

    // Only Admin/Counselor see the full list — students never get a "list everyone's appointments" view
    //[Authorize(Roles = "Admin,Counselor")]
    public async Task<IActionResult> Index()
    {
        return View(await _context.Appointments.ToListAsync());
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
                return Forbid(); // not their appointment
        }

        return View(appointment);
    }

    // Anyone logged in can book — Student, Counselor, or Admin
    // Anyone logged in can book — Student, Counselor, or Admin
    public async Task<IActionResult> Create(bool force = false)
    {
        if (User.IsInRole("Student"))
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StuID == User.Identity!.Name);

            if (student == null) return Forbid();

            // Unless they explicitly want to book another one, check for an existing pending appointment first
            if (!force)
            {
                var pending = await _context.Appointments
                    .Where(a => a.StudentsID == student.StudentsID && a.Status == "Pending")
                    .OrderByDescending(a => a.AppointmentDate)
                    .FirstOrDefaultAsync();

                if (pending != null)
                {
                    TempData["Info"] = "You already have a pending appointment. You can view it below, or book another one if needed.";
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
        if (User.IsInRole("Student"))
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StuID == User.Identity!.Name);

            if (student == null) return Forbid();

            // Never trust the posted values for a student — overwrite with their real record
            appointments.StudentsID = student.StudentsID;
            appointments.FullName = student.StuName;
            appointments.Email = student.Email ?? "";
        }

        ModelState.Remove("Status");
        ModelState.Remove("CreatedAt");
        ModelState.Remove("UpdatedAt");

        appointments.Status = "Pending";
        appointments.CreatedAt = DateTime.Now;
        appointments.UpdatedAt = null;

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

        if (ModelState.IsValid)
        {
            _context.Add(appointments);
            await _context.SaveChangesAsync();

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
                return Forbid(); // can't touch a closed appointment

            // Students can only change the appointment details — never identity, never Status
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
            // Status is intentionally NOT editable here anymore — use Reject / UpdateStatus instead
        }

        existing.UpdatedAt = DateTime.Now;

        if (!ModelState.IsValid)
        {
            ViewBag.IsStudentEdit = isStudent;
            return View(posted);
        }

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


    //[Authorize(Roles = "Student")]
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
    //[Authorize(Roles = "Student")]
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

            TempData["Info"] = "Your appointment has been cancelled.";
            return RedirectToAction(nameof(MyAppointments));
        }

        // Admin/Counselor still hitting this endpoint = real delete (e.g. cleaning up bad data)
        _context.Appointments.Remove(appointments);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool AppointmentsExists(int? appointmentid)
    {
        return _context.Appointments.Any(e => e.AppointmentID == appointmentid);
    }

    // Only Admin/Counselor use the manual lookup — students never need this, they're auto-filled
    //[Authorize(Roles = "Admin,Counselor")]
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

    //[Authorize(Roles = "Admin,Counselor")]
    [HttpPost]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
    {
        var appointment = await _context.Appointments.FindAsync(request.AppointmentId);
        if (appointment == null) return NotFound();

        var validStatuses = new[] { "Pending", "Confirmed", "Rejected", "Completed", "Missed", "Cancelled" };
        if (!validStatuses.Contains(request.NewStatus))
            return BadRequest("Invalid status value.");

        appointment.Status = request.NewStatus;
        appointment.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return Ok();
    }


    //[Authorize(Roles = "Admin,Counselor")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectAppointment(int appointmentid)
    {
        var appointment = await _context.Appointments.FindAsync(appointmentid);
        if (appointment == null) return NotFound();

        if (appointment.Status != "Pending" && appointment.Status != "Confirmed")
        {
            TempData["Error"] = "Only pending or confirmed appointments can be rejected.";
            return RedirectToAction(nameof(Index));
        }

        appointment.Status = "Rejected";
        appointment.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> MyAppointments()
    {
        if (!User.IsInRole("Student")) return Forbid();

        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StuID == User.Identity!.Name);

        if (student == null) return NotFound();

        var appointments = await _context.Appointments
            .Where(a => a.StudentsID == student.StudentsID)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        return View(appointments);
    }

    public class UpdateStatusRequest
    {
        public int AppointmentId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
    }
}