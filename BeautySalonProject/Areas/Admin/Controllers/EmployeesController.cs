using BeautySalonProject.Areas.Admin.ViewModels.Employees;
using BeautySalonProject.Data;
using BeautySalonProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BeautySalonProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public EmployeesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [HttpGet]
        public async Task<IActionResult> Index(bool? active)
        {
            var employees = await _db.Employees
                .Include(e => e.Services)
                .ThenInclude(s => s.Category)
                .OrderByDescending(e => e.IsActive)
                .ThenBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .Select(e => new AdminEmployeeIndexVm.Row
                {
                    EmployeeId = e.EmployeeId,
                    FullName = e.FirstName + " " + e.LastName,
                    JobTitle = e.JobTitle,
                    Phone = e.Phone,
                    Email = e.Email,
                    IsActive = e.IsActive,

                    CategoryName = e.PrimaryCategoryId == null
                            ? null
                            : _db.ServiceCategories
                            .Where(c => c.CategoryId == e.PrimaryCategoryId)
                            .Select(c => c.Name)
                            .FirstOrDefault(),

                   Services = e.PrimaryCategoryId == null
                          ? new List<string>()
                          : _db.Services
                          .Where(s => s.IsActive && s.CategoryId == e.PrimaryCategoryId)
                          .OrderBy(s => s.Name)
                          .Select(s => s.Name)
                          .ToList()
                })
                .ToListAsync();

            return View(new AdminEmployeeIndexVm
            {
                Active = active,
                Items = employees
            });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new AdminEmployeeFormVm();
            await FillEmployeeFormOptions(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminEmployeeFormVm vm)
        {
            await FillEmployeeFormOptions(vm);

            if (!ModelState.IsValid)
                return View(vm);

            var employee = new Employee
            {
                FirstName = vm.FirstName.Trim(),
                LastName = vm.LastName.Trim(),
                JobTitle = string.IsNullOrWhiteSpace(vm.JobTitle) ? null : vm.JobTitle.Trim(),
                Phone = string.IsNullOrWhiteSpace(vm.Phone) ? null : vm.Phone.Trim(),
                Email = string.IsNullOrWhiteSpace(vm.Email) ? null : vm.Email.Trim(),
                IsActive = vm.IsActive,
                PrimaryCategoryId = vm.PrimaryCategoryId
            };

            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Служителят е създаден.";
            return RedirectToAction(nameof(Edit), new { id = employee.EmployeeId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var e = await _db.Employees.FirstOrDefaultAsync(x => x.EmployeeId == id);
            if (e == null) return NotFound();

            var vm = new AdminEmployeeFormVm
            {
                EmployeeId = e.EmployeeId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                JobTitle = e.JobTitle,
                Phone = e.Phone,
                Email = e.Email,
                IsActive = e.IsActive,
                PrimaryCategoryId = e.PrimaryCategoryId
            };

            await FillEmployeeFormOptions(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminEmployeeFormVm vm)
        {
            if (vm.EmployeeId == null) return BadRequest();

            await FillEmployeeFormOptions(vm);

            if (!ModelState.IsValid)
                return View(vm);

            var e = await _db.Employees.FirstOrDefaultAsync(x => x.EmployeeId == vm.EmployeeId.Value);
            if (e == null) return NotFound();

            e.FirstName = vm.FirstName.Trim();
            e.LastName = vm.LastName.Trim();
            e.JobTitle = string.IsNullOrWhiteSpace(vm.JobTitle) ? null : vm.JobTitle.Trim();
            e.Phone = string.IsNullOrWhiteSpace(vm.Phone) ? null : vm.Phone.Trim();
            e.Email = string.IsNullOrWhiteSpace(vm.Email) ? null : vm.Email.Trim();
            e.IsActive = vm.IsActive;
            e.PrimaryCategoryId = vm.PrimaryCategoryId;

            await _db.SaveChangesAsync();

            TempData["Ok"] = "Промените са запазени.";
            return RedirectToAction(nameof(Edit), new { id = e.EmployeeId });
        }
        private static List<string> JobTitles() => new()
        {
            "Педикюрист",
            "Маникюрист",
            "Лазерен специалист",
            "Фризьор",
            "Брау артист",
            "Гримьор",
            "Козметик"
        };

        private async Task<List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>> GetCategoryOptions()
        {
            return await _db.ServiceCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name
                })
                .ToListAsync();
        }

        private async Task FillEmployeeFormOptions(AdminEmployeeFormVm vm)
        {
            vm.JobTitleOptions = JobTitles();
            vm.CategoryOptions = await _db.ServiceCategories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToListAsync();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var e = await _db.Employees
                .Include(x => x.Services)
                .FirstOrDefaultAsync(x => x.EmployeeId == id);

            if (e == null) return NotFound();

            if (e.IsActive)
            {
                var hasActiveServices = e.Services.Any(s => s.IsActive);
                if (hasActiveServices)
                {
                    TempData["Err"] = "Не можеш да деактивираш служителя, защото има активни услуги към него. Първо прехвърли/деактивирай услугите.";
                    return RedirectToAction(nameof(Index));
                }
            }

            e.IsActive = !e.IsActive;
            await _db.SaveChangesAsync();

            TempData["Ok"] = e.IsActive ? "Служителят е активиран." : "Служителят е деактивиран.";
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var e = await _db.Employees
                .Include(x => x.Services)
                    .ThenInclude(s => s.Category)
                .FirstOrDefaultAsync(x => x.EmployeeId == id);

            if (e == null) return NotFound();

            IdentityUser? acc = null;

            if (!string.IsNullOrWhiteSpace(e.IdentityUserId))
            {
                acc = await _userManager.FindByIdAsync(e.IdentityUserId);
            }

            var now = DateTime.Now;

            var upcoming = await _db.Appointments
                .Include(a => a.Variant)
                    .ThenInclude(v => v.Service)
                .Where(a => a.EmployeeId == id && a.StartAt >= now)
                .OrderBy(a => a.StartAt)
                .Take(5)
                .Select(a => new AdminEmployeeDetailsVm.UpcomingRow
                {
                    AppointmentId = a.AppointmentId,
                    StartAt = a.StartAt,
                    EndAt = a.EndAt,
                    ServiceName = a.Variant.Service.Name,
                    VariantName = a.Variant.VariantName,
                    ClientName = !string.IsNullOrWhiteSpace(a.ClientUserId) ? "Клиент (профил)" : (a.GuestFullName ?? "Гост клиент"),
                    ClientPhone = !string.IsNullOrWhiteSpace(a.ClientUserId) ? null : a.GuestPhone
                })
                .ToListAsync();

            var vm = new AdminEmployeeDetailsVm
            {
                EmployeeId = e.EmployeeId,
                FullName = e.FirstName + " " + e.LastName,
                JobTitle = e.JobTitle,
                Phone = e.Phone,
                Email = e.Email,
                IsActive = e.IsActive,
                IdentityUserId = e.IdentityUserId,
                AccountEmail = acc?.Email,
                AccountUserName = acc?.UserName,

                Services = e.Services
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Category.Name)
                    .ThenBy(s => s.Name)
                    .Select(s => s.Category.Name + ": " + s.Name)
                    .ToList(),

                UpcomingAppointments = upcoming
            };

            return View(vm);
        }
        [HttpGet]
        public async Task<IActionResult> Weekly(int id, DateTime? week)
        {
            var employee = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == id);
            if (employee == null) return NotFound();

            var baseDay = (week ?? DateTime.Today).Date;
            int diff = (7 + ((int)baseDay.DayOfWeek - (int)DayOfWeek.Monday)) % 7;
            var monday = baseDay.AddDays(-diff);

            var weekStart = DateOnly.FromDateTime(monday);
            var weekEndExclusive = DateOnly.FromDateTime(monday.AddDays(7));

            var overrides = await _db.EmployeeWorkDays
                .Where(x => x.EmployeeId == id && x.Date >= weekStart && x.Date < weekEndExclusive)
                .ToListAsync();

            string[] dayNames = { "Неделя", "Понеделник", "Вторник", "Сряда", "Четвъртък", "Петък", "Събота" };
            TimeOnly weekdayStart = new TimeOnly(10, 0);
            TimeOnly weekdayEnd = new TimeOnly(18, 0);
            TimeOnly satStart = new TimeOnly(11, 0);
            TimeOnly satEnd = new TimeOnly(17, 0);

            var vm = new AdminEmployeeWeeklyScheduleVm
            {
                EmployeeId = employee.EmployeeId,
                EmployeeName = employee.FirstName + " " + employee.LastName,
                WeekStart = weekStart,
                Days = Enumerable.Range(0, 7).Select(i =>
                {
                    var dt = monday.AddDays(i);
                    var date = DateOnly.FromDateTime(dt);
                    int dow = (int)dt.DayOfWeek;

                    var ov = overrides.FirstOrDefault(x => x.Date == date);

                    bool isSunday = dow == 0;
                    bool isSaturday = dow == 6;

                    bool defaultWorking = !isSunday;
                    TimeOnly defaultStart = isSaturday ? satStart : weekdayStart;
                    TimeOnly defaultEnd = isSaturday ? satEnd : weekdayEnd;

                    bool isWorking = ov?.IsWorking ?? defaultWorking;

                    TimeOnly? start = ov != null ? ov.StartTime : defaultStart;
                    TimeOnly? end = ov != null ? ov.EndTime : defaultEnd;

                    if (!isWorking)
                    {
                        start = null;
                        end = null;
                    }

                    return new AdminEmployeeWeeklyScheduleVm.DayRow
                    {
                        Date = date,
                        DayOfWeek = dow,
                        DayName = dayNames[dow],
                        IsWorking = isWorking,
                        StartTime = start,
                        EndTime = end
                    };
                }).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Weekly(AdminEmployeeWeeklyScheduleVm vm)
        {
            var employee = await _db.Employees.FirstOrDefaultAsync(e => e.EmployeeId == vm.EmployeeId);
            if (employee == null) return NotFound();

            vm.EmployeeName = employee.FirstName + " " + employee.LastName;

            TimeOnly weekdayStart = new TimeOnly(10, 0);
            TimeOnly weekdayEnd = new TimeOnly(18, 0);
            TimeOnly satStart = new TimeOnly(11, 0);
            TimeOnly satEnd = new TimeOnly(17, 0);

            foreach (var d in vm.Days)
            {
                if (!d.IsWorking)
                {
                    d.StartTime = null;
                    d.EndTime = null;
                    continue;
                }

                if (!d.StartTime.HasValue || !d.EndTime.HasValue || d.EndTime <= d.StartTime)
                    ModelState.AddModelError("", $"Невалиден диапазон за {d.DayName} ({d.Date:dd.MM}).");
            }

            if (!ModelState.IsValid)
                return View(vm);

            var start = vm.WeekStart;
            var endExclusive = vm.WeekStart.AddDays(7);

            var existing = await _db.EmployeeWorkDays
                .Where(x => x.EmployeeId == vm.EmployeeId && x.Date >= start && x.Date < endExclusive)
                .ToListAsync();

            foreach (var d in vm.Days)
            {
                var row = existing.FirstOrDefault(x => x.Date == d.Date);

                var dt = d.Date.ToDateTime(new TimeOnly(0, 0));
                int dow = (int)dt.DayOfWeek;
                bool isSunday = dow == 0;
                bool isSaturday = dow == 6;

                bool defaultWorking = !isSunday;
                TimeOnly defaultStart = isSaturday ? satStart : weekdayStart;
                TimeOnly defaultEnd = isSaturday ? satEnd : weekdayEnd;

                bool isDefault =
                    d.IsWorking == defaultWorking &&
                    (
                        !d.IsWorking ||
                        (d.StartTime == defaultStart && d.EndTime == defaultEnd)
                    );

                if (isDefault)
                {
                    if (row != null)
                        _db.EmployeeWorkDays.Remove(row);

                    continue;
                }

                if (row == null)
                {
                    row = new EmployeeWorkDay
                    {
                        EmployeeId = vm.EmployeeId,
                        Date = d.Date
                    };
                    _db.EmployeeWorkDays.Add(row);
                }

                row.IsWorking = d.IsWorking;
                row.StartTime = d.StartTime;
                row.EndTime = d.EndTime;
            }

            await _db.SaveChangesAsync();

            TempData["Ok"] = "Седмичният график е запазен.";
            return RedirectToAction(nameof(Weekly), new
            {
                id = vm.EmployeeId,
                week = vm.WeekStart.ToDateTime(new TimeOnly(0, 0))
            });
        }
        [HttpGet]
        public async Task<IActionResult> CreateAccount(int id)
        {
            var e = await _db.Employees.FirstOrDefaultAsync(x => x.EmployeeId == id);
            if (e == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(e.IdentityUserId))
            {
                TempData["Err"] = "Този служител вече има акаунт.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var vm = new AdminCreateEmployeeAccountVm
            {
                EmployeeId = e.EmployeeId,
                FullName = e.FirstName + " " + e.LastName,
                Email = e.Email ?? "",
                UserName = !string.IsNullOrWhiteSpace(e.Email) ? e.Email! : (e.Phone ?? "")
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccount(AdminCreateEmployeeAccountVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var e = await _db.Employees.FirstOrDefaultAsync(x => x.EmployeeId == vm.EmployeeId);
            if (e == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(e.IdentityUserId))
            {
                TempData["Err"] = "Този служител вече има акаунт.";
                return RedirectToAction(nameof(Details), new { id = e.EmployeeId });
            }

            if (!await _roleManager.RoleExistsAsync("Staff"))
                await _roleManager.CreateAsync(new IdentityRole("Staff"));

            if (await _userManager.FindByNameAsync(vm.UserName.Trim()) != null)
            {
                ModelState.AddModelError(nameof(vm.UserName), "Това потребителско име вече се използва.");
                return View(vm);
            }

            if (await _userManager.FindByEmailAsync(vm.Email.Trim()) != null)
            {
                ModelState.AddModelError(nameof(vm.Email), "Този имейл вече се използва.");
                return View(vm);
            }

            var user = new ApplicationUser
            {
                UserName = vm.UserName.Trim(),
                Email = vm.Email.Trim(),
                EmailConfirmed = true,
                PhoneNumber = e.Phone
            };

            var tempPass = string.IsNullOrWhiteSpace(vm.TempPassword)
                ? ("Temp@" + Guid.NewGuid().ToString("N").Substring(0, 8))
                : vm.TempPassword;

            var createRes = await _userManager.CreateAsync(user, tempPass);
            if (!createRes.Succeeded)
            {
                foreach (var err in createRes.Errors)
                    ModelState.AddModelError("", err.Description);
                return View(vm);
            }

            await _userManager.AddToRoleAsync(user,"Staff");

            e.IdentityUserId = user.Id;
            await _db.SaveChangesAsync();

            TempData["Ok"] = $"Акаунтът е създаден. Временна парола: {tempPass}";
            return RedirectToAction(nameof(Details), new { id = e.EmployeeId });
        }

    }
}
