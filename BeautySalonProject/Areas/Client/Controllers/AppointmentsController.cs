using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySalonProject.Models;
using BeautySalonProject.Areas.Client.ViewModels.Appointments;
using BeautySalonProject.Data;

namespace BeautySalonProject.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Client")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AppointmentsController(ApplicationDbContext db)
        {
            _db = db;
        }
       
        [HttpGet]
        public async Task<IActionResult> Book(int variantId, DateTime? date, int? employeeId)
        {
            var variant = await _db.ServiceVariants
                .Include(v => v.Service)
                .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(v => v.VariantId == variantId && v.IsActive);

            if (variant == null) return NotFound();

            var employees = await _db.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                .Select(e => new EmployeeOptionVm
                {
                    EmployeeId = e.EmployeeId,
                    Name = e.FirstName + " " + e.LastName,
                    JobTitle = e.JobTitle,
                    IsActive = e.IsActive
                })
                .ToListAsync();

            var vm = new ClientBookVm
            {
                VariantId = variant.VariantId,
                EmployeeId = employeeId ?? employees.FirstOrDefault()?.EmployeeId ?? 0,
                Date = (date ?? DateTime.Today).Date,
                VariantTitle = $"{variant.Service.Name} – {variant.VariantName}",
                DurationMinutes = variant.DurationMinutes,
                Price = variant.Price,
                Employees = employees
            };

            if (vm.EmployeeId != 0)
            {
                vm.Slots = await BuildSlots(vm.EmployeeId, vm.Date, vm.DurationMinutes);
            }

            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> Slots(int employeeId, DateTime date, int variantId)
        {
            var variant = await _db.ServiceVariants
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VariantId == variantId && v.IsActive);

            if (variant == null) return NotFound();

            var slots = await BuildSlots(employeeId, date.Date, variant.DurationMinutes);
            return Json(slots.Where(s => s.IsAvailable).ToList());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(ClientBookVm vm)
        {
            if (!ModelState.IsValid)
            {
                await FillBookUi(vm);
                return View(vm);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Forbid();

            var variant = await _db.ServiceVariants
                .Include(v => v.Service)
                .FirstOrDefaultAsync(v => v.VariantId == vm.VariantId && v.IsActive);

            if (variant == null) return NotFound();

            if (!TimeOnly.TryParse(vm.StartTime, out var startTime))
            {
                ModelState.AddModelError(nameof(vm.StartTime), "Невалиден час.");
                await FillBookUi(vm);
                return View(vm);
            }

            var startAt = vm.Date.Date.AddHours(startTime.Hour).AddMinutes(startTime.Minute);
            var endAt = startAt.AddMinutes(variant.DurationMinutes);

            var canBook = await CanBook(vm.EmployeeId, startAt, endAt);
            if (!canBook)
            {
                ModelState.AddModelError("", "Часът вече е зает или служителят не работи тогава.");
                await FillBookUi(vm);
                return View(vm);
            }

            var appt = new Appointment
            {
                VariantId = vm.VariantId,
                EmployeeId = vm.EmployeeId,
                ClientUserId = userId,
                StartAt = startAt,
                EndAt = endAt,
                Notes = string.IsNullOrWhiteSpace(vm.Notes) ? null : vm.Notes.Trim(),
                Status = (byte)AppointmentStatus.Booked,
                CreatedAt = DateTime.UtcNow,
                FinalPrice = variant.Price
            };

            _db.Appointments.Add(appt);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(My));
        }
        [HttpGet]
        public async Task<IActionResult> My()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Forbid();

            var now = DateTime.UtcNow;

            var rows = await _db.Appointments
                .Where(a => a.ClientUserId == userId)
                .Include(a => a.Employee)
                .Include(a => a.Variant)
                    .ThenInclude(v => v.Service)
                .OrderByDescending(a => a.StartAt)
                .Select(a => new ClientAppointmentRowVm
                {
                    AppointmentId = a.AppointmentId,
                    StartAt = a.StartAt,
                    EndAt = a.EndAt,
                    ServiceName = a.Variant.Service.Name,
                    VariantName = a.Variant.VariantName,
                    EmployeeName = a.Employee.FirstName + " " + a.Employee.LastName,
                    FinalPrice = a.FinalPrice,
                    Status = a.Status
                })
                .ToListAsync();

            foreach (var r in rows)
            {
                r.StatusText = ((AppointmentStatus)r.Status) switch
                {
                    AppointmentStatus.Booked => "Запазен",
                    AppointmentStatus.Completed => "Приключен",
                    AppointmentStatus.Cancelled => "Отказан",
                    _ => "Неизвестен"
                };

                r.CanCancel = r.Status == (byte)AppointmentStatus.Booked && r.StartAt > now.AddHours(2);
            }

            var vm = new ClientMyAppointmentsVm
            {
                Upcoming = rows.Where(r => r.StartAt >= now.AddMinutes(-1)).OrderBy(r => r.StartAt).ToList(),
                Past = rows.Where(r => r.StartAt < now.AddMinutes(-1)).OrderByDescending(r => r.StartAt).ToList()
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Forbid();

            var appt = await _db.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id && a.ClientUserId == userId);

            if (appt == null) return NotFound();

            if (appt.Status != (byte)AppointmentStatus.Booked || appt.StartAt <= DateTime.Now.AddHours(2))
                return BadRequest("Не можеш да отмениш този час.");

            appt.Status = (byte)AppointmentStatus.Cancelled;
            appt.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            TempData["Ok"] = "Часът е отказан.";
            return RedirectToAction(nameof(My));
        }

        private async Task Refill(ClientAppointmentCreateVm vm)
        {
            var variant = await _db.ServiceVariants
                .Include(v => v.Service)
                .ThenInclude(s => s.Employee)
                .FirstOrDefaultAsync(v => v.VariantId == vm.VariantId);

            if (variant != null)
            {
                vm.VariantTitle = $"{variant.Service.Name} – {variant.VariantName}";
                vm.DurationMinutes = variant.DurationMinutes;
                vm.Price = variant.Price;
            }

            vm.Employees = await _db.Employees
                .Where(e => e.IsActive)
                .Select(e => new EmployeeOptionVm
                {
                    EmployeeId = e.EmployeeId,
                    Name = e.FirstName + " " + e.LastName,
                    JobTitle = e.JobTitle
                })
                .ToListAsync();

            if (variant != null && vm.EmployeeId != 0)
                vm.Slots = await BuildSlots(vm.EmployeeId, vm.Date.Date, variant.DurationMinutes);
        }
        private async Task FillBookUi(ClientBookVm vm)
        {
            var variant = await _db.ServiceVariants
                .Include(v => v.Service)
                .FirstOrDefaultAsync(v => v.VariantId == vm.VariantId);

            vm.VariantTitle = variant == null ? "" : $"{variant.Service.Name} – {variant.VariantName}";
            vm.DurationMinutes = variant?.DurationMinutes ?? vm.DurationMinutes;
            vm.Price = variant?.Price ?? vm.Price;

            vm.Employees = await _db.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                .Select(e => new EmployeeOptionVm
                {
                    EmployeeId = e.EmployeeId,
                    Name = e.FirstName + " " + e.LastName,
                    JobTitle = e.JobTitle,
                    IsActive = e.IsActive
                }).ToListAsync();

            if (vm.EmployeeId != 0)
                vm.Slots = await BuildSlots(vm.EmployeeId, vm.Date.Date, vm.DurationMinutes);
        }
        private async Task<List<TimeSlotVm>> BuildSlots(int employeeId, DateTime date, int durationMinutes)
        {
            var d = DateOnly.FromDateTime(date);

            var workDay = await _db.EmployeeWorkDays
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.EmployeeId == employeeId && w.Date == d);

            TimeOnly start;
            TimeOnly end;

            if (workDay == null || !workDay.IsWorking || workDay.StartTime == null || workDay.EndTime == null)
            {
                // fallback работно време
                start = new TimeOnly(9, 0);
                end = new TimeOnly(18, 0);
            }
            else
            {
                start = workDay.StartTime.Value;
                end = workDay.EndTime.Value;
            }

            var dayStart = date.Date;
            var dayEnd = date.Date.AddDays(1);

            var busy = await _db.Appointments
                .AsNoTracking()
                .Where(a => a.EmployeeId == employeeId
                            && a.StartAt >= dayStart && a.StartAt < dayEnd
                            && a.Status != (byte)AppointmentStatus.Cancelled)
                .Select(a => new { a.StartAt, a.EndAt })
                .ToListAsync();

            var slots = new List<TimeSlotVm>();
            var step = 15;

            var cur = start;
            while (cur.AddMinutes(durationMinutes) <= end)
            {
                var startAt = date.Date.AddHours(cur.Hour).AddMinutes(cur.Minute);
                var endAt = startAt.AddMinutes(durationMinutes);

                bool overlaps = busy.Any(b => startAt < b.EndAt && endAt > b.StartAt);

                slots.Add(new TimeSlotVm
                {
                    Value = cur.ToString("HH:mm"),
                    Label = cur.ToString("HH:mm"),
                    IsAvailable = !overlaps
                });

                cur = cur.AddMinutes(step);
            }

            return slots;

        }

        private async Task<bool> CanBook(int employeeId, DateTime startAt, DateTime endAt)
        {
            var d = DateOnly.FromDateTime(startAt);

            var wd = await _db.EmployeeWorkDays
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.EmployeeId == employeeId && w.Date == d);

            TimeOnly startWork;
            TimeOnly endWork;

            if (wd == null || !wd.IsWorking || wd.StartTime == null || wd.EndTime == null)
            {
                startWork = new TimeOnly(9, 0);
                endWork = new TimeOnly(18, 0);
            }
            else
            {
                startWork = wd.StartTime.Value;
                endWork = wd.EndTime.Value;
            }

            var startT = TimeOnly.FromDateTime(startAt);
            var endT = TimeOnly.FromDateTime(endAt);

            if (startT < startWork || endT > endWork)
                return false;

            var overlaps = await _db.Appointments.AnyAsync(a =>
                a.EmployeeId == employeeId
                && a.Status != (byte)AppointmentStatus.Cancelled
                && startAt < a.EndAt && endAt > a.StartAt);

            return !overlaps;
        }
    }
}
