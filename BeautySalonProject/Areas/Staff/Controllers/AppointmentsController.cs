using BeautySalonProject.Areas.Staff.ViewModels.Appointments;
using BeautySalonProject.Data;
using BeautySalonProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BeautySalonProject.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Staff")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public AppointmentsController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private async Task<int?> GetMyEmployeeId()
        {
            var userId = _userManager.GetUserId(User);

            return await _db.Employees
                .Where(e => e.IdentityUserId == userId && e.IsActive)
                .Select(e => (int?)e.EmployeeId)
                .FirstOrDefaultAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? date, string? q, byte? status)
        {
            var employeeId = await GetMyEmployeeId();
            if (employeeId == null) return Forbid();

            var day = (date ?? DateTime.Today).Date;
            var dayEnd = day.AddDays(1);

            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim().ToLower();

            var query = _db.Appointments
                .AsNoTracking()
                .Where(a => a.EmployeeId == employeeId.Value)
                .Include(a => a.Variant)
                    .ThenInclude(v => v.Service)
                .AsQueryable();

            if (q == null)
            {
                query = query.Where(a =>
                    a.StartAt >= day && a.StartAt < dayEnd);
            }

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            var appts = await query
                .OrderBy(a => a.StartAt)
                .ToListAsync();

            var clientIds = appts
                .Where(a => !string.IsNullOrWhiteSpace(a.ClientUserId))
                .Select(a => a.ClientUserId!)
                .Distinct()
                .ToList();

            var usersById = clientIds.Count == 0
                ? new Dictionary<string, IdentityUser>()
                : await _db.Users
                    .AsNoTracking()
                    .Where(u => clientIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id);

            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim();

            var rows = appts.Select(a =>
            {
                string clientName;
                string? clientPhone;

                if (!string.IsNullOrWhiteSpace(a.ClientUserId) && usersById.TryGetValue(a.ClientUserId!, out var u))
                {
                    clientName = u.UserName ?? "Клиент (профил)";
                    clientPhone = u.PhoneNumber;
                }
                else
                {
                    clientName = a.GuestFullName ?? "Гост клиент";
                    clientPhone = a.GuestPhone;
                }

                return new StaffAppointmentsIndexVm.Row
                {
                    AppointmentId = a.AppointmentId,
                    StartAt = a.StartAt,
                    EndAt = a.EndAt,
                    ServiceName = a.Variant.Service.Name,
                    VariantName = a.Variant.VariantName,
                    Status = a.Status,
                    ClientName = clientName,
                    ClientPhone = clientPhone
                };
            })
             .Where(r =>
                  q == null ||
                  (r.ClientName != null && r.ClientName.ToLower().Contains(q)) ||
                  (r.ClientPhone != null && r.ClientPhone.Contains(q))
             )
             .ToList();

            var vm = new StaffAppointmentsIndexVm
            {
                Date = day,
                Q = q,
                Status = status,
                Rows = rows
            };

            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var employeeId = await GetMyEmployeeId();
            if (employeeId == null) return Forbid();

            var a = await _db.Appointments
                .Include(x => x.Variant)
                    .ThenInclude(v => v.Service)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.AppointmentId == id);

            if (a == null) return NotFound();

            if (a.EmployeeId != employeeId.Value) return Forbid();

            bool isGuest;
            string clientName;
            string? clientPhone;
            string? clientEmail;

            if (!string.IsNullOrWhiteSpace(a.ClientUserId))
            {
                isGuest = false;
                var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == a.ClientUserId);

                clientName = user?.UserName ?? "Клиент (профил)";
                clientPhone = user?.PhoneNumber;
                clientEmail = user?.Email;
            }
            else
            {
                isGuest = true;
                clientName = a.GuestFullName ?? "Гост клиент";
                clientPhone = a.GuestPhone;
                clientEmail = a.GuestEmail;
            }

            var vm = new StaffAppointmentDetailsVm
            {
                AppointmentId = a.AppointmentId,
                StartAt = a.StartAt,
                EndAt = a.EndAt,

                ServiceName = a.Variant.Service.Name,
                VariantName = a.Variant.VariantName,

                Status = a.Status,
                FinalPrice = a.FinalPrice,
                Notes = a.Notes,

                IsGuest = isGuest,
                ClientName = clientName,
                ClientPhone = clientPhone,
                ClientEmail = clientEmail,

                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var employeeId = await GetMyEmployeeId();
            if (employeeId == null) return Forbid();

            var a = await _db.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == id);
            if (a == null) return NotFound();

            if (a.EmployeeId != employeeId.Value) return Forbid();

            a.Status = (byte)AppointmentStatus.Cancelled;
            a.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            TempData["Ok"] = "Записът е отказан.";
            return RedirectToAction(nameof(Details), new { id });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id, decimal finalPrice)
        {
            var employeeId = await GetMyEmployeeId();
            if (employeeId == null) return Forbid();

            if (finalPrice < 0)
            {
                TempData["Err"] = "Невалидна цена.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var a = await _db.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == id);
            if (a == null) return NotFound();

            if (a.EmployeeId != employeeId.Value) return Forbid();

            a.FinalPrice = finalPrice;
            a.Status = (byte)AppointmentStatus.Completed;
            a.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            TempData["Ok"] = "Посещението е приключено.";
            return RedirectToAction(nameof(Details), new { id });
        }


        private async Task<List<SelectListItem>> GetMyVariantOptions(int employeeId)
        {
            var categoryId = await _db.Employees
                  .Where(e => e.EmployeeId == employeeId)
                  .Select(e => e.PrimaryCategoryId)
                  .FirstOrDefaultAsync();

            if (categoryId == null)
                return new List<SelectListItem>();

            return await _db.ServiceVariants
                   .AsNoTracking()
                   .Where(v => v.IsActive && v.Service.IsActive && v.Service.CategoryId == categoryId.Value)
                   .Include(v => v.Service)
                   .OrderBy(v => v.Service.Name)
                   .ThenBy(v => v.VariantName)
                   .Select(v => new SelectListItem
                   {
                        Value = v.VariantId.ToString(),
                        Text = $"{v.Service.Name} — {v.VariantName} ({v.DurationMinutes} мин / {v.Price:0.00} €.)"
                   })
                   .ToListAsync();


        }

        private async Task<(int duration, decimal price)?> GetVariantMeta(int variantId)
        {
            return await _db.ServiceVariants
                .AsNoTracking()
                .Where(v => v.VariantId == variantId && v.IsActive)
                .Select(v => (ValueTuple<int, decimal>?)new(v.DurationMinutes, v.Price))
                .FirstOrDefaultAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Create(DateTime? startAt)
        {
            var employeeId = await GetMyEmployeeId();
            if (employeeId == null) return Forbid();

            var vm = new StaffAppointmentFormVm
            {
                StartAt = (startAt ?? DateTime.Now).AddSeconds(-DateTime.Now.Second).AddMilliseconds(-DateTime.Now.Millisecond)
            };

            vm.VariantOptions = await GetMyVariantOptions(employeeId.Value);

            if (vm.VariantOptions.Count == 0)
            {
                TempData["Err"] = "Нямаш активни услуги/варианти. Свържи се с администратора.";
                return RedirectToAction("Index");
            }

            vm.VariantId = int.Parse(vm.VariantOptions[0].Value);

            var meta = await GetVariantMeta(vm.VariantId);
            if (meta.HasValue)
            {
                vm.DurationMinutes = meta.Value.duration;
                vm.VariantPrice = meta.Value.price;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffAppointmentFormVm vm)
        {
            var employeeId = await GetMyEmployeeId();
            if (employeeId == null) return Forbid();

            vm.VariantOptions = await GetMyVariantOptions(employeeId.Value);

            if (vm.VariantOptions.Count == 0)
            {
                TempData["Err"] = "Нямаш активни услуги/варианти.";
                return View(vm);
            }

            bool variantAllowed = vm.VariantOptions.Any(x => x.Value == vm.VariantId.ToString());
            if (!variantAllowed)
                ModelState.AddModelError(nameof(vm.VariantId), "Невалиден вариант.");

            if (string.IsNullOrWhiteSpace(vm.GuestFullName))
                ModelState.AddModelError(nameof(vm.GuestFullName), "Въведи име на клиента.");

            if (string.IsNullOrWhiteSpace(vm.GuestPhone))
                ModelState.AddModelError(nameof(vm.GuestPhone), "Въведи телефон.");

            var meta = await GetVariantMeta(vm.VariantId);
            if (!meta.HasValue)
                ModelState.AddModelError(nameof(vm.VariantId), "Невалиден/неактивен вариант.");

            if (!ModelState.IsValid)
                return View(vm);

            var duration = meta!.Value.duration;
            var price = meta.Value.price;

            var start = vm.StartAt;
            var end = start.AddMinutes(duration);

            var a = new Appointment
            {
                EmployeeId = employeeId.Value,
                VariantId = vm.VariantId,

                StartAt = start,
                EndAt = end,

                Notes = string.IsNullOrWhiteSpace(vm.Notes) ? null : vm.Notes.Trim(),

                Status = (byte)AppointmentStatus.Booked,
                CreatedAt = DateTime.Now,

                GuestFullName = vm.GuestFullName.Trim(),
                GuestPhone = vm.GuestPhone.Trim(),
                GuestEmail = string.IsNullOrWhiteSpace(vm.GuestEmail) ? null : vm.GuestEmail.Trim(),

                FinalPrice = price
            };

            _db.Appointments.Add(a);
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Записът е създаден.";
            return RedirectToAction(nameof(Details), new { id = a.AppointmentId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var employeeId = await GetMyEmployeeId();
            if (employeeId == null) return Forbid();

            var a = await _db.Appointments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.AppointmentId == id);

            if (a == null) return NotFound();
            if (a.EmployeeId != employeeId.Value) return Forbid();

            if (a.Status != (byte)AppointmentStatus.Booked)
            {
                TempData["Err"] = "Можеш да редактираш само записан (неприключен) час.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var vm = new StaffAppointmentFormVm
            {
                AppointmentId = a.AppointmentId,
                VariantId = a.VariantId,
                StartAt = a.StartAt,
                Notes = a.Notes,
                GuestFullName = a.GuestFullName,
                GuestPhone = a.GuestPhone,
                GuestEmail = a.GuestEmail
            };

            vm.VariantOptions = await GetMyVariantOptions(employeeId.Value);

            var meta = await GetVariantMeta(vm.VariantId);
            if (meta.HasValue)
            {
                vm.DurationMinutes = meta.Value.duration;
                vm.VariantPrice = meta.Value.price;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StaffAppointmentFormVm vm)
        {
            if (vm.AppointmentId == null) return BadRequest();

            var employeeId = await GetMyEmployeeId();
            if (employeeId == null) return Forbid();

            vm.VariantOptions = await GetMyVariantOptions(employeeId.Value);

            bool variantAllowed = vm.VariantOptions.Any(x => x.Value == vm.VariantId.ToString());
            if (!variantAllowed)
                ModelState.AddModelError(nameof(vm.VariantId), "Невалиден вариант.");

            if (string.IsNullOrWhiteSpace(vm.GuestFullName))
                ModelState.AddModelError(nameof(vm.GuestFullName), "Въведи име на клиента.");

            if (string.IsNullOrWhiteSpace(vm.GuestPhone))
                ModelState.AddModelError(nameof(vm.GuestPhone), "Въведи телефон.");

            var meta = await GetVariantMeta(vm.VariantId);
            if (!meta.HasValue)
                ModelState.AddModelError(nameof(vm.VariantId), "Невалиден/неактивен вариант.");

            if (!ModelState.IsValid)
                return View(vm);

            var a = await _db.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == vm.AppointmentId.Value);
            if (a == null) return NotFound();
            if (a.EmployeeId != employeeId.Value) return Forbid();

            if (a.Status != (byte)AppointmentStatus.Booked)
            {
                TempData["Err"] = "Можеш да редактираш само записан (неприключен) час.";
                return RedirectToAction(nameof(Details), new { id = a.AppointmentId });
            }

            var duration = meta!.Value.duration;
            var price = meta.Value.price;

            a.VariantId = vm.VariantId;
            a.StartAt = vm.StartAt;
            a.EndAt = vm.StartAt.AddMinutes(duration);

            a.Notes = string.IsNullOrWhiteSpace(vm.Notes) ? null : vm.Notes.Trim();

            a.GuestFullName = vm.GuestFullName.Trim();
            a.GuestPhone = vm.GuestPhone.Trim();
            a.GuestEmail = string.IsNullOrWhiteSpace(vm.GuestEmail) ? null : vm.GuestEmail.Trim();

            a.FinalPrice = price;

            a.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            TempData["Ok"] = "Промените са запазени.";
            return RedirectToAction(nameof(Details), new { id = a.AppointmentId });
        }


    }
}
