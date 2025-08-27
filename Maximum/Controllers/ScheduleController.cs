using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Maximum.Data;
using Maximum.Models;

namespace Maximum.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Calendar()
        {
            ViewBag.Employees = await _context.Employees.Where(e => e.IsActive).OrderBy(e => e.FullName).ToListAsync();
            ViewBag.Clients = await _context.Clients.OrderBy(c => c.FullName).ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Events(DateTime start, DateTime end, int? employeeId)
        {
            // FullCalendar передает границы в локальном времени. Переводим в UTC для сравнения с хранением
            var startUtc = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            var endUtc = DateTime.SpecifyKind(end, DateTimeKind.Utc);

            var query = _context.Appointments
                .Include(a => a.Client)
                .Include(a => a.Employee)
                .AsQueryable();

            query = query.Where(a => a.StartAtUtc < endUtc && a.EndAtUtc > startUtc);
            if (employeeId.HasValue)
                query = query.Where(a => a.EmployeeId == employeeId.Value);

            var items = await query
                .Select(a => new
                {
                    id = a.Id,
                    title = a.Title ?? (a.Client.FullName + " — " + a.Employee.FullName),
                    start = a.StartAtUtc,
                    end = a.EndAtUtc,
                    extendedProps = new { client = a.Client.FullName, employee = a.Employee.FullName, employeeId = a.EmployeeId, clientId = a.ClientId }
                })
                .ToListAsync();

            return Json(items);
        }

        public class CreateAppointmentRequest
        {
            public int ClientId { get; set; }
            public int EmployeeId { get; set; }
            public DateTime StartLocal { get; set; }
            public int DurationMinutes { get; set; }
            public string? Title { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] CreateAppointmentRequest req)
        {
            if (req.DurationMinutes <= 0) return BadRequest("Duration must be > 0");

            var startUtc = DateTime.SpecifyKind(req.StartLocal, DateTimeKind.Utc);
            var endUtc = startUtc.AddMinutes(req.DurationMinutes);

            var existsOverlap = await _context.Appointments
                .AnyAsync(a => a.EmployeeId == req.EmployeeId && a.StartAtUtc < endUtc && a.EndAtUtc > startUtc);
            if (existsOverlap)
                return Conflict("Выбранное время уже занято у этого сотрудника.");

            var appt = new Appointment
            {
                ClientId = req.ClientId,
                EmployeeId = req.EmployeeId,
                StartAtUtc = startUtc,
                EndAtUtc = endUtc,
                Title = string.IsNullOrWhiteSpace(req.Title) ? null : req.Title,
            };
            _context.Appointments.Add(appt);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Calendar));
        }
    }
}


