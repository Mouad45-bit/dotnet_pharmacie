using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;

namespace project_pharmacie.Areas.Staff.Pages.Clients
{
	public class EditModel : PageModel
	{
		[BindProperty]
		public Client Client { get; set; }

		public void OnGet(int id)
		{
			// Mock : On simule la récupération du client ID 1 pour l'exemple
			// Plus tard : Client = _service.GetById(id);
			Client = new Client
			{
				Id = id,
				Name = "Jean Dupont",
				Email = "jean@mail.com",
				LoyaltyPoints = 150,
				Status = "Or",
				PersonalizedOffer = "-20% Crèmes"
			};
		}

		public IActionResult OnPost()
		{
			// Logique de mise à jour des points
			if (Client.LoyaltyPoints > 100) Client.Status = "Or";
			else if (Client.LoyaltyPoints > 50) Client.Status = "Argent";
			else Client.Status = "Nouveau";

			TempData["Message"] = "Dossier client mis à jour (Points & Offres sauvegardés).";
			return RedirectToPage("./Index");
		}
	}
}