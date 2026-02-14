using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySalonProject.Models;
using BeautySalonProject.Areas.Admin.ViewModels.ServiceVariants;
using BeautySalonProject.Data;


namespace BeautySalonProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ServiceVariantsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ServiceVariantsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int serviceId)
        {
            var service = await _db.Services
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

            if (service == null)
                return NotFound();

            var items = await _db.ServiceVariants
                .Where(v => v.ServiceId == serviceId)
                .OrderBy(v => v.VariantName)
                .Select(v => new AdminServiceVariantIndexVm.Row
                {
                    VariantId = v.VariantId,
                    VariantName = v.VariantName,
                    Price = v.Price,
                    DurationMinutes = v.DurationMinutes,
                    IsActive = v.IsActive
                })
                .ToListAsync();

            var vm = new AdminServiceVariantIndexVm
            {
                ServiceId = serviceId,
                ServiceName = service.Name,
                CategoryName = service.Category.Name,
                Items = items
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int serviceId)
        {
            var service = await _db.Services
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

            if (service == null) return NotFound();

            return View(new AdminServiceVariantFormVm
            {
                ServiceId = serviceId,
                ServiceName = service.Name,
                CategoryName = service.Category.Name,
                IsActive = true,
                DurationMinutes = 60
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminServiceVariantFormVm vm)
        {
            var service = await _db.Services
                .Include(s => s.Category)
                .FirstOrDefaultAsync(s => s.ServiceId == vm.ServiceId);

            if (service == null) return NotFound();

            vm.ServiceName = service.Name;
            vm.CategoryName = service.Category.Name;

            if (!ModelState.IsValid) return View(vm);

            var entity = new Models.ServiceVariant
            {
                ServiceId = vm.ServiceId,
                VariantName = vm.VariantName.Trim(),
                Price = vm.Price,
                DurationMinutes = vm.DurationMinutes,
                IsActive = vm.IsActive
            };

            _db.ServiceVariants.Add(entity);
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Вариантът е създаден.";
            return RedirectToAction(nameof(Index), new { serviceId = vm.ServiceId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var v = await _db.ServiceVariants
                .Include(x => x.Service)
                .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(x => x.VariantId == id);

            if (v == null) return NotFound();

            return View(new AdminServiceVariantFormVm
            {
                VariantId = v.VariantId,
                ServiceId = v.ServiceId,
                ServiceName = v.Service.Name,
                CategoryName = v.Service.Category.Name,
                VariantName = v.VariantName,
                Price = v.Price,
                DurationMinutes = v.DurationMinutes,
                IsActive = v.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminServiceVariantFormVm vm)
        {
            if (vm.VariantId == null) return BadRequest();

            var v = await _db.ServiceVariants
                .Include(x => x.Service)
                .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(x => x.VariantId == vm.VariantId.Value);

            if (v == null) return NotFound();

            vm.ServiceName = v.Service.Name;
            vm.CategoryName = v.Service.Category.Name;

            if (!ModelState.IsValid) return View(vm);

            v.VariantName = vm.VariantName.Trim();
            v.Price = vm.Price;
            v.DurationMinutes = vm.DurationMinutes;
            v.IsActive = vm.IsActive;

            await _db.SaveChangesAsync();

            TempData["Ok"] = "Промените са запазени.";
            return RedirectToAction(nameof(Index), new { serviceId = v.ServiceId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var v = await _db.ServiceVariants.FirstOrDefaultAsync(x => x.VariantId == id);
            if (v == null) return NotFound();

            v.IsActive = false;
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Вариантът е деактивиран.";
            return RedirectToAction(nameof(Index), new { serviceId = v.ServiceId });
        }
    }
}

