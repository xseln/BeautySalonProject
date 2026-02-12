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
    }
}

