using BeautySalonProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace BeautySalonProject.Data.Seed
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration config)
        {
            var email = config["AdminSeed:Email"];
            var password = config["AdminSeed:Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return;

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(
                        " | ",
                        result.Errors.Select(e => e.Description)));
                }
            }

            if (!await userManager.IsInRoleAsync(user, "Admin"))
                await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}
