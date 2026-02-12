using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BeautySalonProject.Models;
using Microsoft.EntityFrameworkCore;
using BeautySalonProject.Areas.Admin.ViewModels.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using BeautySalonProject.Data;

namespace BeautySalonProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ServicesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _db.Services
                .Include(s => s.Category)
                .Include(s => s.Employee)
                .OrderBy(s => s.Category.Name)
                .ThenBy(s => s.Name)
                .Select(s => new AdminServiceIndexVm.Row
                {
                    ServiceId = s.ServiceId,
                    Name = s.Name,
                    CategoryName = s.Category.Name,
                    EmployeeName = s.Employee.FirstName + " " + s.Employee.LastName,
                    IsActive = s.IsActive,
                    VariantsCount = s.ServiceVariants.Count(v => v.IsActive),
                    TotalVariantsCount = s.ServiceVariants.Count,
                    MinPrice = s.ServiceVariants.Where(v => v.IsActive).Select(v => (decimal?)v.Price).Min(),
                    MaxPrice = s.ServiceVariants.Where(v => v.IsActive).Select(v => (decimal?)v.Price).Max()
                })
                .ToListAsync();

            return View(new AdminServiceIndexVm { Items = items });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new AdminServiceFormVm();
            await FillLookups(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminServiceFormVm vm)
        {
            await FillLookups(vm);

            if (!ModelState.IsValid)
                return View(vm);

            var service = new Service
            {
                CategoryId = vm.CategoryId,
                EmployeeId = vm.EmployeeId,
                Name = vm.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim(),
                IsActive = vm.IsActive
            };

            _db.Services.Add(service);
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Услугата е създадена.";
            return RedirectToAction(nameof(Edit), new { id = service.ServiceId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var s = await _db.Services.FirstOrDefaultAsync(x => x.ServiceId == id);
            if (s == null) return NotFound();

            var vm = new AdminServiceFormVm
            {
                ServiceId = s.ServiceId,
                CategoryId = s.CategoryId,
                EmployeeId = s.EmployeeId,
                Name = s.Name,
                Description = s.Description,
                IsActive = s.IsActive
            };

            await FillLookups(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminServiceFormVm vm)
        {
            if (vm.ServiceId == null) return BadRequest();

            await FillLookups(vm);

            if (!ModelState.IsValid)
                return View(vm);

            var s = await _db.Services.FirstOrDefaultAsync(x => x.ServiceId == vm.ServiceId.Value);
            if (s == null) return NotFound();

            s.CategoryId = vm.CategoryId;
            s.EmployeeId = vm.EmployeeId;
            s.Name = vm.Name.Trim();
            s.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();
            s.IsActive = vm.IsActive;

            await _db.SaveChangesAsync();

            TempData["Ok"] = "Промените са запазени.";
            return RedirectToAction(nameof(Edit), new { id = s.ServiceId });
        }

        private async Task FillLookups(AdminServiceFormVm vm)
        {
            vm.Categories = await _db.ServiceCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            vm.Employees = await _db.Employees
                .Where(e => e.IsActive)
                .OrderBy(e => e.FirstName).ThenBy(e => e.LastName)
                .Select(e => new SelectListItem
                {
                    Value = e.EmployeeId.ToString(),
                    Text = e.FirstName + " " + e.LastName
                })
                .ToListAsync();
        }
    }

}


