using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySalonProject.Models;
using BeautySalonProject.Areas.Admin.ViewModels;
using BeautySalonProject.Data;

namespace BeautySalonProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DashboardController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var newInquiriesCount = await _db.Inquiries.CountAsync(i => i.Status == 0);

            var todayAppointmentsCount = await _db.Appointments.CountAsync(a => a.StartAt >= today && a.StartAt < tomorrow);

            var latestInquiries = await _db.Inquiries
                .OrderByDescending(i => i.CreatedAt)
                .Take(6)
                .Select(i => new DashboardVm.InquiryRow
                {
                    InquiryId = i.InquiryId,
                    FullName = i.FullName,
                    Phone = i.Phone,
                    Email = i.Email,
                    ServiceText = i.ServiceText,
                    PreferredDateTime = i.PreferredDateTime,
                    Status = i.Status,
                    CreatedAt = i.CreatedAt
                })
                .ToListAsync();

            var todaysSchedule = await _db.Appointments
                .Where(a => a.StartAt >= today && a.StartAt < tomorrow)
                .OrderBy(a => a.StartAt)
                .Include(a => a.Employee)
                .Include(a => a.Variant)
                    .ThenInclude(v => v.Service)
                .Take(10)
                .Select(a => new DashboardVm.AppointmentRow
                {
                    AppointmentId = a.AppointmentId,
                    StartAt = a.StartAt,
                    EndAt = a.EndAt,
                    EmployeeName = a.Employee.FirstName + " " + a.Employee.LastName,
                    ServiceName = a.Variant.Service.Name,
                    VariantName = a.Variant.VariantName,
                    Status = a.Status
                })
                .ToListAsync();

            var topVariants = await _db.Appointments
                .GroupBy(a => a.VariantId)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new { VariantId = g.Key, Count = g.Count() })
                .ToListAsync();

            var topVariantIds = topVariants.Select(x => x.VariantId).ToList();

            var variantNames = await _db.ServiceVariants
                .Where(v => topVariantIds.Contains(v.VariantId))
                .Include(v => v.Service)
                .Select(v => new { v.VariantId, Name = v.Service.Name + " – " + v.VariantName })
                .ToListAsync();

            var topServices = topVariants
                .Join(variantNames,
                    t => t.VariantId,
                    v => v.VariantId,
                    (t, v) => new DashboardVm.TopServiceRow
                    {
                        Title = v.Name,
                        Count = t.Count
                    })
                .OrderByDescending(x => x.Count)
                .ToList();

            var vm = new DashboardVm
            {
                NewInquiriesCount = newInquiriesCount,
                TodayAppointmentsCount = todayAppointmentsCount,
                LatestInquiries = latestInquiries,
                TodaysSchedule = todaysSchedule,
                TopServices = topServices
            };

            return View(vm);
        }
    }
}
