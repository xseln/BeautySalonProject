using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySalonProject.Models;
using BeautySalonProject.Areas.Admin.ViewModels;
using BeautySalonProject.Models.Enums;
using BeautySalonProject.Areas.Admin.ViewModels.Inquiries;

namespace BeautySalonProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class InquiriesController : Controller
    {
        private readonly BeautySalonDbContext _db;

        public InquiriesController(BeautySalonDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(byte? status)
        {
            var q = _db.Inquiries.AsQueryable();

            if (status.HasValue)
                q = q.Where(i => i.Status == status.Value);

            var rows = await q
                .OrderBy(i => i.Status)
                .ThenByDescending(i => i.CreatedAt)
                .Select(i => new InquiryIndexVm.Row
                {
                    InquiryId = i.InquiryId,
                    VariantId = i.VariantId,
                    FullName = i.FullName,
                    Phone = i.Phone,
                    ServiceText = i.ServiceText,
                    PreferredDateTime = i.PreferredDateTime,
                    Status = (InquiryStatus)i.Status,
                    CreatedAt = i.CreatedAt
                })
                .ToListAsync();

            var vm = new InquiryIndexVm
            {
                StatusFilter = status.HasValue ? (InquiryStatus)status.Value : null,
                Items = rows
            };

            return View(vm);
        }
        public async Task<IActionResult> Details(int id)
        {
            var i = await _db.Inquiries.FirstOrDefaultAsync(x => x.InquiryId == id);
            if (i == null) return NotFound();

            var vm = new InquiryDetailsVm
            {
                InquiryId = i.InquiryId,
                VariantId = i.VariantId,
                FullName = i.FullName,
                Phone = i.Phone,
                Email = i.Email,
                ServiceText = i.ServiceText,
                PreferredDateTime = i.PreferredDateTime,
                Message = i.Message,
                Status = i.Status,
                CreatedAt = i.CreatedAt
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, byte status)
        {
            var inquiry = await _db.Inquiries.FirstOrDefaultAsync(x => x.InquiryId == id);
            if (inquiry == null) return NotFound();

            inquiry.Status = status;
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Статусът е обновен.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var inquiry = await _db.Inquiries.FirstOrDefaultAsync(x => x.InquiryId == id);
            if (inquiry == null) return NotFound();

            inquiry.Status = (byte)InquiryStatus.Rejected;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var inquiry = await _db.Inquiries.FirstOrDefaultAsync(x => x.InquiryId == id);
            if (inquiry == null) return NotFound();

            inquiry.Status = (byte)InquiryStatus.Approved;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConvertToAppointment(int inquiryId)
        {
            var inquiry = await _db.Inquiries.FirstOrDefaultAsync(i => i.InquiryId == inquiryId);
            if (inquiry == null) return NotFound();

            if (inquiry.PreferredDateTime == null)
            {
                TempData["Err"] = "Запитването няма предпочитан час.";
                return RedirectToAction(nameof(Details), new { id = inquiryId });
            }

            if (inquiry.VariantId == null)
            {
                TempData["Err"] = "Запитването няма избрана услуга (VariantId).";
                return RedirectToAction(nameof(Details), new { id = inquiryId });
            }

            var employeeId = await _db.Employees
                .Where(e => e.IsActive)
                .Select(e => e.EmployeeId)
                .FirstOrDefaultAsync();

            if (employeeId == 0)
            {
                TempData["Err"] = "Няма активен служител, към който да се запише часът.";
                return RedirectToAction(nameof(Details), new { id = inquiryId });
            }

            var duration = await _db.ServiceVariants
                .Where(v => v.VariantId == inquiry.VariantId.Value)
                .Select(v => (int?)v.DurationMinutes)
                .FirstOrDefaultAsync() ?? 60;

            var start = inquiry.PreferredDateTime.Value;
            var end = start.AddMinutes(duration);

            var appointment = new Appointment
            {
                VariantId = inquiry.VariantId.Value,
                EmployeeId = employeeId,
                ClientUserId = null, 
                StartAt = start,
                EndAt = end,
                Notes = inquiry.Message,
                Status = 1, 
                CreatedAt = DateTime.Now
            };

            _db.Appointments.Add(appointment);
            inquiry.Status = (byte)InquiryStatus.Converted;

            await _db.SaveChangesAsync();

            TempData["Ok"] = "Запитването е превърнато в записан час.";
            return RedirectToAction(nameof(Index));
        }



    }
}
