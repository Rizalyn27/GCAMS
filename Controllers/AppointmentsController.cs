
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GCAMS.Models.Appointment;
using GCAMS.Data;

public class AppointmentsController : Controller
{
    private readonly AppDbContext _context;

    public AppointmentsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: APPOINTMENTSS
    public async Task<IActionResult> Index()    
    {
        return View(await _context.Appointments.ToListAsync());
    }

    // GET: APPOINTMENTSS/Details/5
    public async Task<IActionResult> Details(int? appointmentid)
    {
        if (appointmentid == null)
        {
            return NotFound();
        }

        var appointments = await _context.Appointments
            .FirstOrDefaultAsync(m => m.AppointmentID == appointmentid);
        if (appointments == null)
        {
            return NotFound();
        }

        return View(appointments);
    }

    // GET: APPOINTMENTSS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: APPOINTMENTSS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FullName,Email,ContactNumber,AppointmentDate,AppointmentType,Notes,StudentsID")] Appointments appointments)
    {
        ModelState.Remove("Status");
        ModelState.Remove("CreatedAt");
        ModelState.Remove("UpdatedAt");

        appointments.Status = "Pending";
        appointments.CreatedAt = DateTime.Now;
        appointments.UpdatedAt = null;

        // Prevent double-booking: same student, same day, not already cancelled/rejected
        var hasConflict = await _context.Appointments.AnyAsync(a =>
            a.StudentsID == appointments.StudentsID &&
            a.AppointmentDate.Date == appointments.AppointmentDate.Date &&
            a.Status != "Cancelled" &&
            a.Status != "Rejected");

        if (hasConflict)
        {
            ModelState.AddModelError("", "This student already has an appointment scheduled on this date.");
            return View(appointments);
        }

        if (ModelState.IsValid)
        {
            _context.Add(appointments);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(appointments);
    }

    // GET: APPOINTMENTSS/Edit/5
    public async Task<IActionResult> Edit(int? appointmentid)
    {
        if (appointmentid == null)
        {
            return NotFound();
        }

        var appointments = await _context.Appointments.FindAsync(appointmentid);
        if (appointments == null)
        {
            return NotFound();
        }
        return View(appointments);
    }

    // POST: APPOINTMENTSS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? appointmentid, [Bind("AppointmentID,FullName,Email,AppointmentDate,AppointmentType,Notes,Status,CreatedAt,StudentsID")]  Appointments appointments)
    {
        if (appointmentid != appointments.AppointmentID)
        {
            return NotFound();
        }

        //Set the UpdatedAt property to the current date and time when editing an appointment
        appointments.UpdatedAt = DateTime.Now;

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(appointments);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentsExists(appointments.AppointmentID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(appointments);
    }

    // GET: APPOINTMENTSS/Delete/5
    public async Task<IActionResult> Delete(int? appointmentid)
    {
        if (appointmentid == null)
        {
            return NotFound();
        }

        var appointments = await _context.Appointments
            .FirstOrDefaultAsync(m => m.AppointmentID == appointmentid);
        if (appointments == null)
        {
            return NotFound();
        }

        return View(appointments);
    }

    // POST: APPOINTMENTSS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? appointmentid)
    {
        var appointments = await _context.Appointments.FindAsync(appointmentid);
        if (appointments != null)
        {
            _context.Appointments.Remove(appointments);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool AppointmentsExists(int? appointmentid)
    {
        return _context.Appointments.Any(e => e.AppointmentID == appointmentid);
    }

    //Get students by ID

    [HttpGet]
    public async Task<IActionResult> GetStudentByStuID(string? stuId)
    {
        if (string.IsNullOrWhiteSpace(stuId))
            return BadRequest("Student ID is required.");

        // Search by the school-issued StuID string, not the database PK
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StuID == stuId.Trim());

        if (student == null)
            return NotFound($"Student with ID '{stuId}' not found.");

        return Json(new
        {
            studentsID = student.StudentsID, // DB primary key (int) — stored in hidden field to link the appointment
            fullName = student.StuName,
            email = student.Email ?? "",
            gradeLevel = student.GradeLevel,
            section = student.Section
        });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
    {
        var appointment = await _context.Appointments.FindAsync(request.AppointmentId);

        if (appointment == null)
            return NotFound();

        // Only accept known status values — reject anything else to prevent tampering
        var validStatuses = new[] { "Pending", "Confirmed", "Cancelled", "Completed", "Missed" };
        if (!validStatuses.Contains(request.NewStatus))
            return BadRequest("Invalid status value.");

        appointment.Status = request.NewStatus;
        appointment.UpdatedAt = DateTime.Now; // record when the status was last changed

        await _context.SaveChangesAsync();
        return Ok();
    }

    // Simple DTO to receive the JSON body from the fetch() call
    public class UpdateStatusRequest
    {
        public int AppointmentId { get; set; }
        public string NewStatus { get; set; } = string.Empty;
    }

}
