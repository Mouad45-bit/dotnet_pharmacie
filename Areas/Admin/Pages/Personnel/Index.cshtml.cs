using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using project_pharmacie.Models;

namespace project_pharmacie.Areas.Admin.Pages.Personnel
{
	[Authorize(Roles = "Administrateur")]
	public class IndexModel : PageModel
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public IndexModel(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}

		// --- 1. CE QUE TON DESIGN ATTEND (Le contrat) ---

		// Ton HTML veut une liste qui s'appelle "Items", pas "Users"
		public List<UserViewModel> Items { get; set; } = new List<UserViewModel>();

		// Ton HTML veut "Q" pour la barre de recherche
		[BindProperty(SupportsGet = true)]
		public string Q { get; set; }

		// Ton HTML veut "Total" pour le compteur
		public int Total { get; set; }


		// --- 2. LA LOGIQUE (On remplit les données) ---
		public async Task OnGetAsync()
		{
			// Préparer la requête
			var query = _userManager.Users.AsQueryable();

			// Gestion de la Recherche (Si Q n'est pas vide)
			if (!string.IsNullOrWhiteSpace(Q))
			{
				query = query.Where(u => u.Email.Contains(Q) || u.NomComplet.Contains(Q));
			}

			// Calcul du Total
			Total = await query.CountAsync();

			// Récupérer les données brutes de la BD
			var usersFromDb = await query.ToListAsync();

			// --- 3. LA TRADUCTION (BD -> Design) ---
			Items = new List<UserViewModel>();

			foreach (var user in usersFromDb)
			{
				var roles = await _userManager.GetRolesAsync(user);

				Items.Add(new UserViewModel
				{
					Id = user.Id,
					// Ici on connecte ton 'NomComplet' (BD) au 'FullName' (Design)
					FullName = user.NomComplet ?? "Sans nom",
					Email = user.Email,
					// On prend le premier rôle trouvé
					Role = roles.FirstOrDefault() ?? "Aucun",
					// On connecte 'EstActif' (BD) à 'IsActive' (Design)
					IsActive = user.EstActif
				});
			}
		}

		// Gestion du bouton Supprimer
		public async Task<IActionResult> OnPostDeleteAsync(string id)
		{
			var user = await _userManager.FindByIdAsync(id);
			if (user != null)
			{
				await _userManager.DeleteAsync(user);
				// Petit message flash si tu as le système, sinon ça ne plante pas
				TempData["FlashMessage"] = "Utilisateur supprimé.";
			}
			return RedirectToPage(); // Recharge la page
		}
	}

	// --- 4. LA CLASSE MODÈLE SPÉCIALE POUR TON DESIGN ---
	// C'est ça qui permet d'utiliser p.FullName, p.IsActive, etc.
	public class UserViewModel
	{
		public string Id { get; set; }
		public string FullName { get; set; }
		public string Email { get; set; }
		public string Role { get; set; }
		public bool IsActive { get; set; }
	}
}