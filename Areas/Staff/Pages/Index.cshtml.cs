using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace project_pharmacie.Areas.Staff.Pages
{
	// 👇 LA SÉCURITÉ EST ICI
	// On verrouille tout cet espace pour les rôles : Administrateur OU Personnel
	[Authorize(Roles = "Administrateur,Personnel")]
	public class IndexModel : PageModel
	{
		public void OnGet()
		{
			// On pourra charger des stats ici plus tard
		}
	}
}