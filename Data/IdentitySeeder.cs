using Microsoft.AspNetCore.Identity;
using project_pharmacie.Models;

namespace project_pharmacie.Data;

public static class IdentitySeeder
{
    public const string AdminRole = "ADMIN";
    public const string PersonnelRole = "PERSONNEL";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Roles
        foreach (var role in new[] { AdminRole, PersonnelRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Admin default
        var adminUsername = "admin";
        var admin = await userManager.FindByNameAsync(adminUsername);

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminUsername,
                Email = "admin@local",
                EmailConfirmed = true,
                Nom = "Admin Principal"
            };

            var create = await userManager.CreateAsync(admin, "Admin@12345");
            if (create.Succeeded)
                await userManager.AddToRoleAsync(admin, AdminRole);
        }
        else
        {
            if (!await userManager.IsInRoleAsync(admin, AdminRole))
                await userManager.AddToRoleAsync(admin, AdminRole);
        }
    }
}