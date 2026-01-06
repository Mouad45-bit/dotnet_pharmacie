using Microsoft.AspNetCore.Identity;
using project_pharmacie.Models;

namespace project_pharmacie.Data
{
	public static class DbInitializer
	{
		public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
		{
			// Récupérer les outils de gestion
			var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
			var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

			// --- 1. CRÉATION DES RÔLES ---
			string[] roleNames = { "Administrateur", "Personnel" };
			foreach (var roleName in roleNames)
			{
				if (!await roleManager.RoleExistsAsync(roleName))
				{
					await roleManager.CreateAsync(new IdentityRole(roleName));
				}
			}

			// --- 2. CRÉATION DE L'ADMINISTRATEUR ---
			var adminEmail = "admin@pharmacie.com";
			if (await userManager.FindByEmailAsync(adminEmail) == null)
			{
				var adminUser = new ApplicationUser
				{
					UserName = adminEmail,
					Email = adminEmail,
					NomComplet = "Super Admin",
					EstActif = true,
					EmailConfirmed = true
				};
				// Mot de passe : Admin123!
				var result = await userManager.CreateAsync(adminUser, "Admin123!");
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(adminUser, "Administrateur");
				}
			}

			// --- 3. CRÉATION DU COMPTE PERSONNEL (STAFF) ---
			var staffEmail = "staff@pharmacie.com";
			if (await userManager.FindByEmailAsync(staffEmail) == null)
			{
				var staffUser = new ApplicationUser
				{
					UserName = staffEmail,
					Email = staffEmail,
					NomComplet = "Employé Modèle",
					EstActif = true,
					EmailConfirmed = true
				};
				// Mot de passe : Staff123!
				var result = await userManager.CreateAsync(staffUser, "Staff123!");
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(staffUser, "Personnel");
				}
			}
		}
	}
}