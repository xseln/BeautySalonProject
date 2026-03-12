using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySalonProject.Models;
using BeautySalonProject.Areas.Admin.ViewModels.Inquiries;
using BeautySalonProject.Areas.Admin.ViewModels.Appointments;
using Microsoft.AspNetCore.Identity;
using BeautySalonProject.Data;


namespace BeautySalonProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(DateTime? date, byte? status, int? employeeId)
        {
            var day = date?.Date;

            var q = _db.Appointments
               .Include(a => a.Employee)
               .Include(a => a.Variant)
               .ThenInclude(v => v.Service)
               .Include(a => a.ClientUser)
               .AsQueryable();

            if (employeeId.HasValue)
                q = q.Where(a => a.EmployeeId == employeeId.Value);

            if (day.HasValue)
            {
                q = q.Where(a => a.StartAt >= day.Value && a.StartAt < day.Value.AddDays(1));
            }
            else
            {
                if (employeeId.HasValue)
                {
                    var now = DateTime.Now;
                    q = q.Where(a => a.StartAt >= now);
                }
                else
                {
                    var today = DateTime.Today;
                    q = q.Where(a => a.StartAt >= today && a.StartAt < today.AddDays(1));
                    day = today;
                }
            }

            if (status.HasValue)
                q = q.Where(a => a.Status == status.Value);

            var rows = await q
                .OrderBy(a => a.StartAt)
                .Select(a => new AdminAppointmentRowVm
                {
                    AppointmentId = a.AppointmentId,
                    StartAt = a.StartAt,
                    EndAt = a.EndAt,
                    EmployeeName = a.Employee.FirstName + " " + a.Employee.LastName,
                    ServiceName = a.Variant.Service.Name,
                    VariantName = a.Variant.VariantName,

                    ClientName = a.ClientUser != null
                    ? a.ClientUser.FirstName + " " + a.ClientUser.LastName
                    : a.GuestFullName ?? "Гост клиент",

                    Status = a.Status
                })
                .ToListAsync();

            var vm = new AdminAppointmentsIndexVm
            {
                Date = day ?? DateTime.Today, 
                StatusFilter = status,
                Rows = rows
            };

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var a = await _db.Appointments
                .Include(x => x.Employee)
                .Include(x => x.Variant)
                    .ThenInclude(v => v.Service)
                .FirstOrDefaultAsync(x => x.AppointmentId == id);

            if (a == null) return NotFound();

            bool isGuest;
            string clientName;
            string? clientPhone;
            string? clientEmail;

            if (!string.IsNullOrWhiteSpace(a.ClientUserId))
            {
                isGuest = false;
                var user = await _userManager.FindByIdAsync(a.ClientUserId);

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

            var employees = await _db.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                .Select(e => new AdminAppointmentDetailsVm.EmployeeOption
                {
                    Id = e.EmployeeId,
                    Name = e.FirstName + " " + e.LastName
                })
                .ToListAsync();

            var vm = new AdminAppointmentDetailsVm
            {
                AppointmentId = a.AppointmentId,
                StartAt = a.StartAt,
                EndAt = a.EndAt,

                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee.FirstName + " " + a.Employee.LastName,

                ServiceName = a.Variant.Service.Name,
                VariantName = a.Variant.VariantName,

                IsGuest = isGuest,
                ClientName = clientName,
                ClientPhone = clientPhone,
                ClientEmail = clientEmail,

                Notes = a.Notes,
                Status = a.Status,

                Employees = employees,

                FinalPrice = a.FinalPrice,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                InquiryId = a.InquiryId
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, byte status, DateTime? date)
        {
            var a = await _db.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == id);
            if (a == null) return NotFound();

            a.Status = status;
            a.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Статусът е обновен.";
            return RedirectToAction(nameof(Index), new { date = (date ?? a.StartAt).Date });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reassign(int id, int employeeId)
        {
            var a = await _db.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == id);
            if (a == null) return NotFound();

            var exists = await _db.Employees.AnyAsync(e => e.EmployeeId == employeeId && e.IsActive);
            if (!exists)
            {
                TempData["Err"] = "Невалиден служител.";
                return RedirectToAction(nameof(Details), new { id });
            }

            a.EmployeeId = employeeId;
            a.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            TempData["Ok"] = "Записването е преразпределено.";
            return RedirectToAction(nameof(Details), new { id });
        }

    }
}
