using BeautySalonProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeautySalonProject.Data.Seed
{
    public static class SeedRunner
    {
        public static async Task RunAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var config = services.GetRequiredService<IConfiguration>();
            var db = services.GetRequiredService<ApplicationDbContext>();

            await RoleSeeder.SeedRolesAsync(roleManager);
            await AdminSeeder.SeedAdminAsync(userManager, config);
            await SalonDataSeeder.SeedAsync(db);
        }
    }
}
