using BeautySalonProject.Areas.Staff.ViewModels.Dashboard;
using BeautySalonProject.Data;
using BeautySalonProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautySalonProject.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles ="Staff")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
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
        public async Task<IActionResult> Index()
        {
            var employeeId = await GetMyEmployeeId();
            if (employeeId == null) return Forbid();

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var now = DateTime.Now;

            var todayCount = await _db.Appointments
                .AsNoTracking()
                .Where(a => a.EmployeeId == employeeId.Value &&
                            a.StartAt >= today && a.StartAt < tomorrow)
                .CountAsync();

            var completedTodayCount = await _db.Appointments
               .AsNoTracking()
               .Where(a => a.EmployeeId == employeeId.Value &&
                           a.StartAt >= today && a.StartAt < tomorrow &&
                           a.Status == (byte)AppointmentStatus.Completed)
               .CountAsync();

            var upcomingCount = await _db.Appointments
                .AsNoTracking()
                .Where(a => a.EmployeeId == employeeId.Value &&
                            a.StartAt >= now &&
                            a.Status == (byte)AppointmentStatus.Booked)
                .CountAsync();

            var appts = await _db.Appointments
                .AsNoTracking()
                .Where(a => a.EmployeeId == employeeId.Value
                            && a.StartAt >= today && a.StartAt < tomorrow)
                .Include(a => a.Variant)
                    .ThenInclude(v => v.Service)
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

            var todayRows = appts.Select(a =>
            {
                string clientName;
                string? clientPhone;

                if (!string.IsNullOrWhiteSpace(a.ClientUserId) &&
                    usersById.TryGetValue(a.ClientUserId!, out var u))
                {
                    clientName = u.UserName ?? "Клиент (профил)";
                    clientPhone = u.PhoneNumber;
                }
                else
                {
                    clientName = a.GuestFullName ?? "Гост клиент";
                    clientPhone = a.GuestPhone;
                }

                return new StaffDashboardVm.TodayRow
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
            }).ToList();

            var vm = new StaffDashboardVm
            {
                TodayCount = todayCount,
                CompletedTodayCount = completedTodayCount,
                UpcomingCount = upcomingCount,
                TodaySchedule = todayRows
            };

            return View(vm);
        }
    }
}
