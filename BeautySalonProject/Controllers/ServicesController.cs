using BeautySalonProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySalonProject.ViewModels;
using static BeautySalonProject.ViewModels.CombinedPriceListVm;
using BeautySalonProject.Data;



namespace BeautySalonProject.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ServicesController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index() => View();

        public async Task<IActionResult> Category(int id)
        {
            var category = await _db.ServiceCategories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.IsActive);

            if (category == null) return NotFound();

            var variants = await _db.ServiceVariants
                .Include(v => v.Service)
                .Where(v => v.IsActive
                            && v.Service.IsActive
                            && v.Service.CategoryId == id)
                .OrderBy(v => v.Service.Name)
                .ThenBy(v => v.VariantName)
               .Select(v => new
               {
                   v.VariantId,
                   ServiceName = v.Service.Name,
                   VariantName = v.VariantName,
                   v.Price,
                   v.DurationMinutes
               })
                .ToListAsync();

            var grouped = variants
                .GroupBy(x => x.ServiceName)
                .Select(g => new PriceGroupVm
                {
                    ServiceName = g.Key,
                    Items = g.Select(x => new PriceItemVm
                    {
                        VariantId = x.VariantId,
                        VariantName = x.VariantName,
                        Price = x.Price,
                        DurationMinutes = x.DurationMinutes
                    }).ToList()

                })
                .ToList();

            var vm = new ServicesCategoryVm
            {
                CategoryId = category.CategoryId,
                CategoryName = category.Name,
                Groups = grouped
            };

            return View(vm);
        }
        public async Task<IActionResult> HairAndMakeup()
        {
            int hairId = 1;   
            int makeupId = 7;  

            var vm = await BuildCombinedVm("Фризьор & Грим", new[] { hairId, makeupId });
            return View("CombinedPriceList", vm);
        }

        public async Task<IActionResult> ManiPedi()
        {
            int maniId = 2;    
            int pediId = 3;   

            var vm = await BuildCombinedVm("Маникюр & Педикюр", new[] { maniId, pediId });
            return View("CombinedPriceList", vm);
        }

        private async Task<CombinedPriceListVm> BuildCombinedVm(string title, int[] categoryIds)
        {
            var services = await _db.Services
                .Where(s => s.IsActive && categoryIds.Contains(s.CategoryId))
                .OrderBy(s => s.Name)
                .Select(s => new
                {
                    s.ServiceId,
                    s.Name,
                    s.CategoryId,
                    CategoryName = s.Category.Name,
                    Variants = s.ServiceVariants
                        .Where(v => v.IsActive)
                        .OrderBy(v => v.Price)
                        .Select(v => new
                        {
                            v.VariantId,
                            v.VariantName,
                            v.Price,
                            v.DurationMinutes
                        })
                        .ToList()
                })
                .ToListAsync();

            var grouped = services
                .GroupBy(x => new { x.CategoryId, x.CategoryName })
                .Select(g => new CategoryBlockVm
                {
                    CategoryId = g.Key.CategoryId,
                    CategoryName = g.Key.CategoryName,
                    Services = g.Select(s => new ServiceBlockVm
                    {
                        ServiceId = s.ServiceId,
                        ServiceName = s.Name,
                        Variants = s.Variants.Select(v => new VariantRowVm
                        {
                            VariantId = v.VariantId,
                            VariantName = v.VariantName,
                            Price = v.Price,
                            DurationMinutes = v.DurationMinutes
                        }).ToList()
                    }).ToList()
                })
                .OrderBy(c => c.CategoryName)
                .ToList();

            return new CombinedPriceListVm
            {
                Title = title,
                Categories = grouped
            };
        }
    }
}
