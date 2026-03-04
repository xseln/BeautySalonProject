using BeautySalonProject.Areas.Staff.ViewModels.Clients;
using BeautySalonProject.Data;
using BeautySalonProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeautySalonProject.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Staff")]
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
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
        public async Task<IActionResult> History(string? q)
        {
            var employeeId = await GetMyEmployeeId();
            if (employeeId == null) return Forbid();

            q = string.IsNullOrWhiteSpace(q) ? null : q.Trim().ToLower();


            var baseQuery = _db.Appointments
                .AsNoTracking()
                .Where(a => a.EmployeeId == employeeId.Value &&
                            a.Status == (byte)AppointmentStatus.Completed)
                .Include(a => a.Variant)
                    .ThenInclude(v => v.Service)
                .OrderByDescending(a => a.StartAt)
                .AsQueryable();

            if (q != null)
            {
                baseQuery = baseQuery.Where(a =>
                    (!string.IsNullOrWhiteSpace(a.GuestFullName) && a.GuestFullName.ToLower().Contains(q)) ||
                    (!string.IsNullOrWhiteSpace(a.GuestPhone) && a.GuestPhone.Contains(q)) ||
                    (!string.IsNullOrWhiteSpace(a.GuestEmail) && a.GuestEmail.ToLower().Contains(q)) ||
                    (!string.IsNullOrWhiteSpace(a.ClientUserId))
                );
            }

            var appts = await baseQuery.Take(q == null ? 20 : 200).ToListAsync();

            var clientIds = appts
                .Where(a => !string.IsNullOrWhiteSpace(a.ClientUserId))
                .Select(a => a.ClientUserId!)
                .Distinct()
                .ToList();

            var usersById = clientIds.Count == 0
                ? new Dictionary<string, ApplicationUser>()
                : await _db.Users
                    .AsNoTracking()
                    .Where(u => clientIds.Contains(u.Id))
                    .ToDictionaryAsync(u => u.Id);

            var rows = appts.Select(a =>
            {
                bool isGuest = string.IsNullOrWhiteSpace(a.ClientUserId);

                string name;
                string? phone;
                string? email;

                if (!isGuest && usersById.TryGetValue(a.ClientUserId!, out var u))
                {
                    name = u.UserName ?? "Клиент (профил)";
                    phone = u.PhoneNumber;
                    email = u.Email;
                }
                else
                {
                    name = a.GuestFullName ?? "Гост клиент";
                    phone = a.GuestPhone;
                    email = a.GuestEmail;
                }

                return new StaffClientHistoryVm.Row
                {
                    AppointmentId = a.AppointmentId,
                    StartAt = a.StartAt,
                    ServiceName = a.Variant.Service.Name,
                    VariantName = a.Variant.VariantName,
                    ClientName = name,
                    ClientPhone = phone,
                    ClientEmail = email,
                    IsGuest = isGuest,
                    FinalPrice = a.FinalPrice
                };
            }).ToList();

            if (q != null)
            {
                rows = rows.Where(r =>
                    (!string.IsNullOrWhiteSpace(r.ClientName) && r.ClientName.ToLower().Contains(q)) ||
                    (!string.IsNullOrWhiteSpace(r.ClientPhone) && r.ClientPhone.Contains(q)) ||
                    (!string.IsNullOrWhiteSpace(r.ClientEmail) && r.ClientEmail.ToLower().Contains(q))
                ).ToList();
            }

            var vm = new StaffClientHistoryVm
            {
                Q = q,
                Rows = rows
            };

            return View(vm);
        }
    }
}
