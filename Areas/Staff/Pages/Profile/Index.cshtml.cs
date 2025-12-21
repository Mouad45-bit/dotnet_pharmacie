using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace project_pharmacie.Areas.Staff.Pages.Profile
{
	public class IndexModel : PageModel
	{
		public string Username { get; set; }
		public string Role { get; set; }
		public string Email { get; set; }
		public string EmployeeId { get; set; }

		[BindProperty]
		public string CurrentPassword { get; set; }
		[BindProperty]
		public string NewPassword { get; set; }

		[TempData]
		public string Message { get; set; }

		public void OnGet()
		{
			// Simulation de l'utilisateur connecté (Toi)
			Username = "Souhail Hajji";
			Role = "Responsable Secteur C";
			Email = "souhail@pharmacie.com";
			EmployeeId = "EMP-2025-C";
		}

		public IActionResult OnPost()
		{
			// Simulation du changement de mot de passe
			if (!string.IsNullOrEmpty(NewPassword))
			{
				Message = "Mot de passe modifié avec succès !";
			}

			// On recharge les infos car la page se rafraîchit
			Username = "Souhail Hajji";
			Role = "Responsable Secteur C";
			Email = "souhail@pharmacie.com";
			EmployeeId = "EMP-2025-C";

			return Page();
		}
	}
}