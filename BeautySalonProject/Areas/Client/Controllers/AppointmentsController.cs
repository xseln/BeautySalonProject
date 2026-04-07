using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeautySalonProject.Models;
using BeautySalonProject.Areas.Client.ViewModels.Appointments;
using BeautySalonProject.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;

namespace BeautySalonProject.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Client")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Book(int? serviceId)
        {
            var vm = new ClientBookVm();

            vm.Categories = await _db.ServiceCategories
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                .ToListAsync();

            vm.Employees = await _db.Employees
                .Where(e => e.IsActive) 
                .Select(e => new EmployeeOptionVm
                {
                    EmployeeId = e.EmployeeId,
                    Name = e.FirstName + " " + e.LastName
                })
                .ToListAsync();

            if (serviceId.HasValue)
            {
                var service = await _db.Services
                    .Include(s => s.Category)
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

                if (service != null)
                {
                    vm.CategoryId = service.CategoryId;
                    var variant = await _db.ServiceVariants
                        .FirstOrDefaultAsync(v => v.ServiceId == service.ServiceId && v.IsActive);

                    if (variant != null)
                    {
                        vm.VariantId = variant.VariantId;
                        vm.DurationMinutes = variant.DurationMinutes;
                        vm.Price = variant.Price;
                        vm.VariantTitle = variant.VariantName;
                    }
                }
            }

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GetServicesByCategory(int categoryId)
        {
            var services = await _db.Services
                .Where(s => s.CategoryId == categoryId && s.IsActive)
                .Select(s => new { s.ServiceId, s.Name })
                .ToListAsync();

            return Json(services);
        }

        [HttpGet]
        public async Task<IActionResult> GetVariants(int serviceId)
        {
            var variants = await _db.ServiceVariants
                .Where(v => v.ServiceId == serviceId && v.IsActive)
                .Select(v => new {
                    v.VariantId,
                    v.VariantName,
                    v.Price,
                    v.DurationMinutes
                })
                .ToListAsync();

            return Json(variants);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int employeeId, int variantId, string date)
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
                return Json(new List<string>());

            var variant = await _db.ServiceVariants.FindAsync(variantId);
            if (variant == null) return Json(new List<string>());

            var workDay = await _db.EmployeeWorkDays
                .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.Date == parsedDate);

            if (workDay == null || !workDay.IsWorking || !workDay.StartTime.HasValue || !workDay.EndTime.HasValue)
                return Json(new List<string>());

            var startAtDate = parsedDate.ToDateTime(TimeOnly.MinValue);
            var endAtDate = startAtDate.AddDays(1);

            var appointments = await _db.Appointments
                .Where(a => a.EmployeeId == employeeId &&
                            a.StartAt >= startAtDate && a.StartAt < endAtDate &&
                            a.Status != (byte)AppointmentStatus.Cancelled)
                .ToListAsync();

            var slots = new List<string>();
            var current = workDay.StartTime.Value;
            var end = workDay.EndTime.Value;

            while (current.AddMinutes(variant.DurationMinutes) <= end)
            {
                var slotStart = parsedDate.ToDateTime(current);
                var slotEnd = slotStart.AddMinutes(variant.DurationMinutes);

                bool isTaken = appointments.Any(a => slotStart < a.EndAt && slotEnd > a.StartAt);

                if (!isTaken)
                {
                    slots.Add(current.ToString("HH:mm"));
                }

                current = current.AddMinutes(30);
            }

            return Json(slots);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(ClientBookVm model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _db.ServiceCategories
                    .Where(c => c.IsActive)
                    .Select(c => new SelectListItem { Value = c.CategoryId.ToString(), Text = c.Name })
                    .ToListAsync();
                return View(model);
            }

            var variant = await _db.ServiceVariants.FindAsync(model.VariantId);
            if (variant == null) return RedirectToAction("Book");

            var user = await _userManager.GetUserAsync(User);
            if (!TimeOnly.TryParse(model.StartTime, out var startTime))
            {
                ModelState.AddModelError("StartTime", "Невалиден час.");
                return View(model);
            }

            var startDateTime = model.Date.Date
            .AddHours(startTime.Hour)
            .AddMinutes(startTime.Minute);
            var endDateTime = startDateTime.AddMinutes(variant.DurationMinutes);

            var appointment = new Appointment
            {
                EmployeeId = model.EmployeeId,
                VariantId = model.VariantId,
                StartAt = startDateTime,
                EndAt = endDateTime,
                ClientUserId = user?.Id,
                CreatedAt = DateTime.Now,
                Status = (byte)AppointmentStatus.Booked,
                FinalPrice = variant.Price
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            return RedirectToAction("My");
        }

		[HttpGet]
		public async Task<IActionResult> My(int tab = 0)
		{
			var userId = _userManager.GetUserId(User);
			if (string.IsNullOrEmpty(userId)) return Forbid();

			var allAppointments = await _db.Appointments
				.Where(a => a.ClientUserId == userId)
				.Include(a => a.Employee)
				.Include(a => a.Variant).ThenInclude(v => v.Service)
				.OrderByDescending(a => a.StartAt)
				.ToListAsync();

			var now = DateTime.Now;

			var vm = new ClientMyAppointmentsVm
			{
				Tab = tab,
				Upcoming = allAppointments
					.Where(a => a.StartAt >= now)
					.Select(a => MapToRow(a))
					.ToList(),

				Past = allAppointments
					.Where(a => a.StartAt < now)
					.Select(a => MapToRow(a))
					.ToList()
			};

			return View(vm);
		}

		private ClientAppointmentRowVm MapToRow(Appointment a)
		{
			// Прозорец от 2 часа след създаването
			var canEditOrCancel = (DateTime.Now - a.CreatedAt).TotalHours <= 2;

			return new ClientAppointmentRowVm
			{
				AppointmentId = a.AppointmentId,
				StartAt = a.StartAt,
				EndAt = a.EndAt,
				ServiceName = a.Variant.Service.Name,
				VariantName = a.Variant.VariantName,
				EmployeeName = a.Employee.FirstName + " " + a.Employee.LastName,
				FinalPrice = a.FinalPrice,
				Status = a.Status,
				StatusText = a.Status == 1 ? "Резервиран" : (a.Status == 3 ? "Отменен" : "Завършен"),

				// Това свойство ще управлява видимостта на бутоните
				CanCancel = canEditOrCancel && a.Status != 3
			};
		}

		[HttpPost]
		public async Task<IActionResult> Cancel(int id)
		{
			var appointment = await _db.Appointments.FindAsync(id);
			if (appointment == null) return NotFound();

			// ПРОВЕРКА ЗА СИГУРНОСТ:
			var hoursSinceCreation = (DateTime.Now - appointment.CreatedAt).TotalHours;

			if (hoursSinceCreation > 2)
			{
				TempData["Err"] = "Срокът за промяна на тази резервация (2 часа) е изтекъл. Моля, свържете се с нас по телефона.";
				return RedirectToAction(nameof(My));
			}

			appointment.Status = 3; // Отменен
			await _db.SaveChangesAsync();

			TempData["Ok"] = "Резервацията е отменена успешно.";
			return RedirectToAction(nameof(My));
		}

        // 1. Зареждане на формата за редакция
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var appointment = await _db.Appointments
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return NotFound();

            // Проверка на 2-часовия прозорец
            var hoursSinceCreation = (DateTime.Now - appointment.CreatedAt).TotalHours;
            if (hoursSinceCreation > 2)
            {
                TempData["Err"] = "Срокът за редакция на този час е изтекъл.";
                return RedirectToAction(nameof(My));
            }

            // Подготовка на ViewModel-а
            var vm = new EditAppointmentVm
            {
                AppointmentId = appointment.AppointmentId,
                EmployeeId = appointment.EmployeeId,
                Date = DateOnly.FromDateTime(appointment.StartAt),
                // Вземаме всички активни служители за избор
                Employees = await _db.Employees
                    .Where(e => e.IsActive)
                    .Select(e => new EditAppointmentVm.EmployeeOption
                    {
                        Id = e.EmployeeId,
                        Name = e.FirstName + " " + e.LastName
                    }).ToListAsync()
            };

            return View(vm);
        }

        // 2. Записване на промените
        [HttpPost]
        public async Task<IActionResult> Edit(int id, int employeeId, DateOnly date, TimeOnly slot)
        {
            var appointment = await _db.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            // Повторна проверка за сигурност на сървъра
            if ((DateTime.Now - appointment.CreatedAt).TotalHours > 2)
            {
                TempData["Err"] = "Грешка: Времето за редакция е изтекло.";
                return RedirectToAction(nameof(My));
            }

            // Изчисляваме новите начален и краен час
            // Тук приемаме, че времетраенето остава същото (от оригиналния запис)
            var duration = appointment.EndAt - appointment.StartAt;

            DateTime newStart = date.ToDateTime(slot);
            DateTime newEnd = newStart.Add(duration);

            // TODO: Тук е добре да добавиш проверка дали новият слот не е зает междувременно!

            appointment.EmployeeId = employeeId;
            appointment.StartAt = newStart;
            appointment.EndAt = newEnd;

            await _db.SaveChangesAsync();

            TempData["Ok"] = "Часът беше променен успешно.";
            return RedirectToAction(nameof(My));
        }
    }
}