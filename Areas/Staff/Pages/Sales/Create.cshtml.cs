using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using project_pharmacie.Models;
using System;

namespace project_pharmacie.Areas.Staff.Pages.Sales
{
	public class CreateModel : PageModel
	{
		[BindProperty]
		public Models.Vente NewSale { get; set; } = new Models.Vente();

		public void OnGet()
		{
		}

		public IActionResult OnPost()
		{
			if (!ModelState.IsValid)
			{
				return Page();
			}

			// Simule l'enregistrement (à connecter à une vraie BDD plus tard)
			NewSale.DateVente = DateTime.Now;

			// Redirection vers l'historique
			return RedirectToPage("./History");
		}
	}
}