using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BeautySalonProject.Models;
using Microsoft.EntityFrameworkCore;

namespace BeautySalonProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class ServicesController : Controller
    {
        private readonly BeautySalonDbContext _db;

        public ServicesController(BeautySalonDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? categoryId, bool? active)
        {
            var q = _db.Services
                .Include(s => s.Category)
                .Include(s => s.Employee)
                .Include(s => s.ServiceVariants)
                .AsQueryable();

            if (categoryId.HasValue)
                q = q.Where(s => s.CategoryId == categoryId.Value);

            if (active.HasValue)
                q = q.Where(s => s.IsActive == active.Value);

            var items = await q
                .OrderBy(s => s.Category.Name)
                .ThenBy(s => s.Name)
                .Select(s => new AdminServiceIndexVm.Row
                {
                    ServiceId = s.ServiceId,
                    CategoryName = s.Category.Name,
                    ServiceName = s.Name,
                    EmployeeName = s.Employee.FirstName + " " + s.Employee.LastName,
                    IsActive = s.IsActive,
                    VariantsCount = s.ServiceVariants.Count(v => v.IsActive),
                    TotalVariantsCount = s.ServiceVariants.Count
                })
                .ToListAsync();

            var categories = await _db.ServiceCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new AdminServiceIndexVm.CategoryFilter
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name
                })
                .ToListAsync();

            var vm = new AdminServiceIndexVm
            {
                CategoryId = categoryId,
                Active = active,
                Categories = categories,
                Items = items
            };

            return View(vm);
        }
    }
}

