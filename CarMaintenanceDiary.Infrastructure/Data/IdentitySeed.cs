using Microsoft.AspNetCore.Identity;

namespace CarMaintenanceDiary.Infrastructure.Data
{
    public static class IdentitySeed
    {
        public static async Task EnsureSeedAsync(UserManager<CarMaintenanceDiary.Infrastructure.Identity.ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var roles = new[] { "Admin", "User" };
            foreach (var r in roles)
            {
                if (!await roleManager.RoleExistsAsync(r))
                {
                    await roleManager.CreateAsync(new IdentityRole(r));
                }
            }

            // create default admin if not exists
            var adminEmail = "admin@local";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new CarMaintenanceDiary.Infrastructure.Identity.ApplicationUser { Email = adminEmail, UserName = adminEmail };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
