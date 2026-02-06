using BeautySalonProject.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BeautySalonProject.Controllers
{
    public class GalleryController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public GalleryController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpGet("/Gallery/{slug}")]
        public IActionResult Category(string slug)
        {
            slug = (slug ?? "").Trim().ToLowerInvariant();

            var map = new Dictionary<string, GalleryVm>(StringComparer.OrdinalIgnoreCase)
            {
                ["manicure"] = new GalleryVm
                {
                    Title = "Маникюр",
                    Subtitle = "Красота до върха на пръстите",
                    Description = "Професионален маникюр, гел лак, поддръжка и декорации – прецизност и стил във всеки детайл.",
                    Folder = "manicure"
                },
                ["pedicure"] = new GalleryVm
                {
                    Title = "Педикюр",
                    Subtitle = "Нежна грижа и комфорт",
                    Description = "Комфортен педикюр с внимание към детайла и усещане за свежест.",
                    Folder = "pedicure"
                },
                ["cosmetics"] = new GalleryVm
                {
                    Title = "Козметика",
                    Subtitle = "Здрава, сияйна кожа",
                    Description = "Почистване, маски и терапии – грижа според нуждите на кожата.",
                    Folder = "cosmetics"
                },
                ["brows-lashes"] = new GalleryVm
                {
                    Title = "Вежди и мигли",
                    Subtitle = "Изразителен поглед",
                    Description = "Оформяне, ламиниране и процедури, които подчертават естествената красота.",
                    Folder = "brows-lashes"
                },
                ["hair"] = new GalleryVm
                {
                    Title = "Фризьор",
                    Subtitle = "Стил и уверeност",
                    Description = "Подстригване, боядисване, прически и терапии за красива и здрава коса.",
                    Folder = "hair"
                },
                ["makeup"] = new GalleryVm
                {
                    Title = "Грим",
                    Subtitle = "За всеки повод",
                    Description = "Дневен и официален грим – подчертаваме най-доброто в теб.",
                    Folder = "makeup"
                },
                ["laser"] = new GalleryVm
                {
                    Title = "Лазерна епилация",
                    Subtitle = "Гладки резултати",
                    Description = "Ефективна грижа за различни зони с дълготраен резултат.",
                    Folder = "laser"
                },
            };

            if (!map.TryGetValue(slug, out var vm))
                return NotFound();

            var folderPath = Path.Combine(_env.WebRootPath, "gallery", vm.Folder);

            if (Directory.Exists(folderPath))
            {
                vm.Images = Directory.EnumerateFiles(folderPath)
                    .Where(f => f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                             || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                             || f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                             || f.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
                   .Select(f => Path.GetFileName(f)!)
                    .OrderBy(x => x)
                    .ToList();
            }

            return View("Category", vm);
        }
    }
}
