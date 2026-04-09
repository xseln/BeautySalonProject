using BeautySalonProject.Areas.Staff.ViewModels.Profile;
using BeautySalonProject.Data;
using BeautySalonProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BeautySalonProject.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Staff")]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var employee = _context.Employees
                .FirstOrDefault(e => e.UserId == user.Id);

            var vm = new EditProfileVm
            {
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,

                FirstName = employee?.FirstName,
                LastName = employee?.LastName
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(EditProfileVm model)
        {
            var user = await _userManager.GetUserAsync(User);

            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            await _userManager.UpdateAsync(user);

            var employee = _context.Employees
                .FirstOrDefault(e => e.UserId == user.Id);

            if (employee != null)
            {
                employee.FirstName = model.FirstName;
                employee.LastName = model.LastName;

                _context.SaveChanges();
            }

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            }

            TempData["Ok"] = "Профилът е обновен!";
            return RedirectToAction("Index");
        }
    }
}

