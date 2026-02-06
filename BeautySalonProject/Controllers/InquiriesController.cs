using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BeautySalonProject.Models;
using BeautySalonProject.ViewModels;
using BeautySalonProject.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BeautySalonProject.Controllers
{
    [AllowAnonymous]
    public class InquiriesController : Controller
    {
        private readonly BeautySalonDbContext _db;
        private readonly IEmailSender _email;
        private readonly EmailSettings _settings;

        public InquiriesController(BeautySalonDbContext db, IEmailSender email, IOptions<EmailSettings> settings)
        {
            _db = db;
            _email = email;
            _settings = settings.Value;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Create", "Appointments", new { area = "Client" });

            var vm = new InquiryCreateVm
            {
                Categories = await LoadCategoriesAsync(),
                ServiceVariants = new List<SelectListItem>() 
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InquiryCreateVm vm)
        {
            if (User.Identity?.IsAuthenticated ?? false)
                return RedirectToAction("Create", "Appointments", new { area = "Client" });

            vm.Categories = await LoadCategoriesAsync();
            vm.ServiceVariants = new List<SelectListItem>();

            if (!ModelState.IsValid)
                return View(vm);

            var sv = await _db.ServiceVariants
                .Include(x => x.Service)
                    .ThenInclude(s => s.Employee)
                .FirstOrDefaultAsync(x => x.VariantId == vm.ServiceVariantId);

            if (sv == null || sv.Service == null)
            {
                ModelState.AddModelError(nameof(vm.ServiceVariantId), "Невалидна услуга.");
                return View(vm);
            }

            var employee = sv.Service.Employee;
            var serviceName = $"{sv.Service.Name} – {sv.VariantName}";

            var inquiry = new Inquiry
            {
                VariantId = vm.ServiceVariantId,
                FullName = $"{vm.FirstName} {vm.LastName}",
                Phone = vm.Phone,
                Email = vm.Email,
                ServiceText = serviceName,
                PreferredDateTime = vm.PreferredDateTime,
                Message = vm.Note,
                CreatedAt = DateTime.Now,
                Status = 0
            };

            _db.Inquiries.Add(inquiry);
            await _db.SaveChangesAsync();

            var subject = $"Ново запитване: {serviceName}";
            var employeeName = employee == null ? "Служител" : $"{employee.FirstName} {employee.LastName}";
            var employeeEmail = employee?.Email;

            var body = $@"
                <h2>Ново запитване (гост)</h2>
                <p><b>Име:</b> {vm.FirstName} {vm.LastName}</p>
                <p><b>Телефон:</b> {vm.Phone}</p>
                <p><b>Email:</b> {vm.Email}</p>
                <p><b>Услуга:</b> {serviceName}</p>
                <p><b>Предпочитана дата/час:</b> {vm.PreferredDateTime:dd.MM.yyyy HH:mm}</p>
                <p><b>Бележка:</b> {System.Net.WebUtility.HtmlEncode(vm.Note ?? "-")}</p>
                <hr/>
                <p><i>SH Beauty Salon</i></p>";

            await _email.SendAsync(_settings.AdminEmail, subject, body);

            if (!string.IsNullOrWhiteSpace(employeeEmail))
                await _email.SendAsync(employeeEmail, subject, body);

            TempData["Ok"] = $"Запитването е изпратено успешно! Ще се свържем с вас скоро.";
            return RedirectToAction(nameof(Create));
        }

        private async Task<List<SelectListItem>> LoadCategoriesAsync()
        {
            var cats = await _db.ServiceCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new { c.CategoryId, c.Name })
                .ToListAsync();

            return cats.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            }).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> GetVariants(int categoryId)
        {
            var variants = await _db.ServiceVariants
                .Include(v => v.Service)
                .Where(v => v.IsActive
                         && v.Service.CategoryId == categoryId
                         && v.Service.IsActive)
                .OrderBy(v => v.Service.Name)
                .ThenBy(v => v.VariantName)
                .Select(v => new
                {
                    id = v.VariantId,
                    text = $"{v.Service.Name} – {v.VariantName} ({v.Price:0.##} €.)"
                })
                .ToListAsync();

            return Json(variants);
        }
    }
}
