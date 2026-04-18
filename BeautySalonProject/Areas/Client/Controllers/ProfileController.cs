using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BeautySalonProject.Models;
using BeautySalonProject.Data;

namespace BeautySalonProject.Areas.Client.Controllers
{
    [Area("Client")]
    [Authorize(Roles = "Client")]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            var vm = new Staff.ViewModels.Profile.EditProfileVm
            {
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? ""
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Staff.ViewModels.Profile.EditProfileVm model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(model);

            user.Email = model.Email ?? "";
            user.UserName = model.Email ?? "";
            user.PhoneNumber = model.PhoneNumber ?? "";
            user.FirstName = model.FirstName ?? "";
            user.LastName = model.LastName ?? "";

            await _userManager.UpdateAsync(user);

            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword))
                {
                    ModelState.AddModelError("", "Въведи текуща парола!");
                    return View(model);
                }

                var result = await _userManager.ChangePasswordAsync(
                    user,
                    model.CurrentPassword,
                    model.NewPassword
                );

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Грешна текуща парола!");
                    return View(model);
                }
            }

            TempData["Ok"] = "Профилът е обновен успешно!";
            return RedirectToAction("Index");
        }
    }
}