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

            var variant = await _db.ServiceVariants
                .Include(v => v.Service)
                .FirstOrDefaultAsync(v => v.VariantId == inquiry.VariantId.Value);

            if (variant == null) return NotFound("Няма такъв ServiceVariant.");
            if (variant.Service == null) return NotFound("Variant няма Service.");

            var employeeId = variant.Service.EmployeeId;
            var duration = variant.DurationMinutes > 0 ? variant.DurationMinutes : 60;

            var appointment = new Appointment
            {
                VariantId = variant.VariantId,
                EmployeeId = employeeId,

                ClientUserId = null,
                GuestFullName = inquiry.FullName,
                GuestPhone = inquiry.Phone,
                GuestEmail = inquiry.Email,

                StartAt = inquiry.PreferredDateTime.Value,
                EndAt = inquiry.PreferredDateTime.Value.AddMinutes(duration),

                Notes = inquiry.Message,
                Status = 0,
                CreatedAt = DateTime.Now,

                FinalPrice = variant.Price,
                InquiryId = inquiry.InquiryId
            };

            _db.Appointments.Add(appointment);

            inquiry.Status = (byte)InquiryStatus.Converted;

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }



    }
}
