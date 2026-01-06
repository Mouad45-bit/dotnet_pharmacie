using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;
using System.ComponentModel.DataAnnotations;

namespace project_pharmacie.Areas.Admin.Pages.Personnel
{
	// Sécurité : Seul l'admin peut accéder ici
	[Authorize(Roles = "Administrateur")]
	public class CreateModel : PageModel
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public CreateModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_roleManager = roleManager;
		}

		[BindProperty]
		public InputModel Input { get; set; }

		public class InputModel
		{
			[Required(ErrorMessage = "Le nom est obligatoire")]
			public string FullName { get; set; }

			[Required(ErrorMessage = "L'email est obligatoire")]
			[EmailAddress]
			public string Email { get; set; }

			// LE CHAMP MOT DE PASSE EST ICI 👇
			[Required(ErrorMessage = "Le mot de passe est obligatoire")]
			[DataType(DataType.Password)]
			public string Password { get; set; }

			[Required]
			public string Role { get; set; } = "Personnel";
		}

		public void OnGet()
		{
		}

		public async Task<IActionResult> OnPostAsync()
		{
			if (ModelState.IsValid)
			{
				var user = new ApplicationUser
				{
					UserName = Input.Email,
					Email = Input.Email,
					NomComplet = Input.FullName, // On remplit ton champ personnalisé
					EstActif = true,
					EmailConfirmed = true
				};

				// Création de l'utilisateur avec le mot de passe saisi
				var result = await _userManager.CreateAsync(user, Input.Password);

				if (result.Succeeded)
				{
					// On lui donne son rôle (Admin ou Personnel)
					await _userManager.AddToRoleAsync(user, Input.Role);

					// Succès : retour à la liste
					return RedirectToPage("Index");
				}

				// Si erreur (ex: mot de passe trop simple ou email pris)
				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}

			// Si on est là, c'est qu'il y a un problème, on réaffiche le formulaire
			return Page();
		}
	}
}